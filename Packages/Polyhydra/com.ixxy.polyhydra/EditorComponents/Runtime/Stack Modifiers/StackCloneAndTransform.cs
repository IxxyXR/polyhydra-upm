using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
public class StackCloneAndTransform : BaseStackModifier
{
    [Foldout("Transform")] public Vector3 Position = Vector3.zero;
    [Foldout("Transform")] public Vector3 Rotation = Vector3.zero;
    [Foldout("Transform")] public Vector3 Scale = Vector3.one;

    public override IEnumerable<Matrix4x4> GetTransformList()
    {
        var transforms = new List<Matrix4x4>();
        var matrix = Matrix4x4.TRS(Position, Quaternion.Euler(Rotation), Scale);
        var currentTransform = Matrix4x4.identity;
        for (int i = 0; i < Copies; i++)
        {
            transforms.Add(currentTransform);
            currentTransform *= matrix;
        }

        return transforms;
    }
}
