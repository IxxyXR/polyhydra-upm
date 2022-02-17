using Conway;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
public partial class TransformModifier : BaseModifier
{
    [Foldout("Transform")] public Vector3 Position = Vector3.zero;
    [Foldout("Transform")] public Vector3 Rotation = Vector3.zero;
    [Foldout("Transform")] public Vector3 Scale = Vector3.one;

    public override ConwayPoly Modify(ConwayPoly poly)
    {
        poly.Transform(Position, Rotation, Scale);
        return poly;
    }
}