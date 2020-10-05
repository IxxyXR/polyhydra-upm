using System.Collections.Generic;
using UnityEngine;

// Attempts to record all public fields and properties in a <string, string> Dictionary
// Calls .ToString on values
// Useful for undo and checking for changes
// Usage: prevValues = ComponentRecorder.RecordComponent(this);
//        if (prevValues["Foo"] != Foo.ToString() {}

public static class ComponentRecorder
{
    public static Dictionary<string,string> RecordComponent<T>(T original) where T : Component
    {
        System.Type type = original.GetType();
        var prevValues = new Dictionary<string, string>();

        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            var val = field.GetValue(original);
            prevValues[field.Name] = val != null ? val.ToString() : "";
        }

        var props = type.GetProperties();
        foreach (var prop in props)
        {
            if (!prop.CanWrite) continue;
            var val = prop.GetValue(original);
            prevValues[prop.Name] = val != null ? val.ToString() : "";
        }

        return prevValues;
    }
}
