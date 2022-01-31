using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
public class StackPointSymmetry : BaseStackModifier
{
    [Header("Symmetry")]
    public PointSymmetry.Family family;
    public int n = 3;
    public float radius = 1f;

    public override IEnumerable<Matrix4x4> GetTransformList()
    {
        return new PointSymmetry(family, n, radius).matrices;
    }
}
