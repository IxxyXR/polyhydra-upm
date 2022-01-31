using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
public class StackWallpaperSymmetry : BaseStackModifier
{
    [Header("Symmetry")]
    public SymmetryGroup.R group;
    public Vector2 TileSize = Vector2.one;
    public float UnitScale = 1f;
    public Vector2 UnitOffset = Vector2.zero;
    public Vector2 Spacing = Vector2.one;
    [Header("Iteration")]
    public int RepeatX = 1;
    public int RepeatY = 1;

    public override IEnumerable<Matrix4x4> GetTransformList()
    {
        return new WallpaperSymmetry(group, RepeatX, RepeatY, TileSize, UnitScale, UnitOffset, Spacing).matrices;
    }
}
