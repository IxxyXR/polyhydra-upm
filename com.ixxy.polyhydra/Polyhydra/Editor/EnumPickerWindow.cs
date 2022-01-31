using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using Conway;
using UnityEditor;

public class EnumPickerWindow
    : EditorWindow
{
    private static GUIStyle regularStyle;
    private static GUIStyle selectedStyle;

    private string enumName;
    private List<string> valuesRaw;
    private List<string> valuesFiltered;
    private string filter;

    private EditorWindow parent;
    private Vector2 scroll;

    private System.Action<string> onSelectCallback;

    public void ShowCustom(string name, List<string> values, Rect rect, System.Action<string> onSelect)
    {
        regularStyle = new GUIStyle(EditorStyles.label);
        regularStyle.active = regularStyle.normal;

        selectedStyle = new GUIStyle(EditorStyles.label);
        selectedStyle.normal = selectedStyle.focused;
        selectedStyle.active = selectedStyle.focused;

        enumName = name;
        valuesRaw = new List<string>(values);
        valuesFiltered = new List<string>(values);
        filter = "";
        onSelectCallback = onSelect;

        parent = focusedWindow;

        var screenRect = rect;
        var screenSize = new Vector2(300, 400);

        screenRect.position = GUIUtility.GUIToScreenPoint(screenRect.position);

        ShowAsDropDown(screenRect, screenSize);
        Focus();

        GUI.FocusControl("filter");
    }

    private void OnGUI()
    {
        if (Event.current.keyCode == KeyCode.Escape)
        {
            this.Close();
        }
        GUILayout.Label(string.Format("Enum Type: {0}", enumName));

        GUI.SetNextControlName("filter");
        var filterUpdate = GUILayout.TextField(filter);
        if (filterUpdate != filter)
            FilterValues(filterUpdate);

        // always focused
        GUI.FocusControl("filter");

        scroll = GUILayout.BeginScrollView(scroll);

        for (int i = 0; i < valuesFiltered.Count; ++i)
        {
            var value = valuesFiltered[i];
            var style = i == 0 ? selectedStyle : regularStyle;
            Func<Rect> getRect = () => GUILayoutUtility.GetRect(new GUIContent(value), style);
            switch((Ops)i)
            {
                case Ops.Identity:
                    GUI.Label(getRect(), "Conway");
                    break;
                case Ops.Lace:
                    GUI.Label(getRect(), "Extended Conway");
                    break;
                case Ops.Extrude:
                    GUI.Label(getRect(), "Extrusion");
                    break;
                case Ops.FaceOffset:
                    GUI.Label(getRect(), "Transforms");
                    break;
                case Ops.AddDual:
                    GUI.Label(getRect(), "Copies");
                    break;
                case Ops.FaceRemove:
                    GUI.Label(getRect(), "Add/Remove");
                    break;
                case Ops.FillHoles:
                    GUI.Label(getRect(), "Various");
                    break;
                default:
                    break;
            }
            Rect rect = GUILayoutUtility.GetRect(new GUIContent(value), style);
            var clicked = GUI.Button(rect, value);
            if (clicked)
            {
                GUILayout.EndScrollView();

                onSelectCallback(value);
                Close();
                parent.Repaint();
                parent.Focus();

                return;
            }
        }

        GUILayout.EndScrollView();

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            if (valuesFiltered.Count > 0)
                onSelectCallback(valuesFiltered[0]);

            Close();
            parent.Repaint();
            parent.Focus();
        }

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
        {
            Close();
            parent.Repaint();
            parent.Focus();
        }

        if (workaroundLostFocusFlag)
            Close();
    }

    private bool workaroundLostFocusFlag;

    public void OnLostFocus()
    {
        // removing this because it crashes unity sometimes
        //Close();

        workaroundLostFocusFlag = true;
        this.Repaint();
    }

    private void FilterValues(string filterUpdate)
    {
        filter = filterUpdate;

        var filterLower = filter.ToLower();

        valuesFiltered.Clear();

        for (int i = 0; i < valuesRaw.Count; ++i)
        {
            var value = valuesRaw[i];
            var lower = value.ToLower();
            if (lower.StartsWith(filterLower))
                valuesFiltered.Add(value);
        }
    }
}

[CustomPropertyDrawer(typeof(Ops))]
public class WaveBreakEnumDrawers : EditorEnumDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
    }
}

public class EditorEnumDrawer
    : PropertyDrawer
{
    private EnumPickerWindow window;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var valuesRaw = Enum.GetValues(fieldInfo.FieldType);
        if (valuesRaw.Length <= 0)
            return;

        var valuesStr = new List<string>();
        for (int i = 0; i < valuesRaw.Length; ++i)
        {
            var raw = valuesRaw.GetValue(i);
            var str = raw.ToString();

            valuesStr.Add(str);
        }

        var enumName = fieldInfo.FieldType.FullName;
        var currentIndex = Mathf.Clamp(property.enumValueIndex, 0, valuesStr.Count);
        var currentName = valuesStr[property.enumValueIndex];

        EditorGUI.PrefixLabel(position, label);

        GUI.SetNextControlName(property.propertyPath);

        var fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);

        if (GUI.Button(fieldRect, currentName, EditorStyles.popup))
        {
            window = EditorWindow.GetWindow<EnumPickerWindow>();

            System.Action<string> callback = str =>
            {
                var index = valuesStr.IndexOf(str);

                property.serializedObject.Update();
                property.enumValueIndex = index;
                property.serializedObject.ApplyModifiedProperties();
            };

            window.ShowCustom(enumName, valuesStr, fieldRect, callback);
            window.Focus();
        }
    }
}

#endif
