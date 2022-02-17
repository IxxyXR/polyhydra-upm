using System.Linq;
using NaughtyAttributes;
using UnityEngine;

public class BaseComponent : MonoBehaviour
{
    private void OnValidate()
    {
        GetComponent<BuildPoly>().NeedsGenerate = true;
    }
    
    private void OnEnable()
    {
        GetComponent<BuildPoly>().NeedsGenerate = true;
    }
    
    [Button]
    private void MoveUp()
    {
#if UNITY_EDITOR
        UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
#endif
    }
    
    [Button]
    private void MoveDown()
    {
#if UNITY_EDITOR
        UnityEditorInternal.ComponentUtility.MoveComponentDown(this);
#endif
    }
    
    [Button][ContextMenu("Duplicate")]
    private void Duplicate()
    {
#if UNITY_EDITOR
        UnityEditorInternal.ComponentUtility.CopyComponent(this);
        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(gameObject);
        MoveLastComponentToMe();
#endif
    }
    
    // Utility. Moves the last component (usually the one just added by another method)
    // into the position just after this component.
    protected void MoveLastComponentToMe()
    {
#if UNITY_EDITOR
        var components = GetComponents<MonoBehaviour>().ToList();
        var newComponent = components.Last();
        int posFromEnd = components.Count - components.IndexOf(this) - 2;
        for (int i = 0; i < posFromEnd; i++)
        {
            UnityEditorInternal.ComponentUtility.MoveComponentUp(newComponent);
        }
#endif
    }
}