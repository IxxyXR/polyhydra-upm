using System.Collections.Generic;
using UnityEngine;


public class WallpaperSymmetry
{
    private SymmetryGroup.R group;
    private int repeatX;
    private int repeatY;
    private Vector2 center;
    private float unitScale;
    private Vector2 unitOffset;
    private Vector2 spacing;
    
    public readonly SymmetryGroup groupProperties;
    public readonly List<Matrix4x4> matrices;
    
    public WallpaperSymmetry(SymmetryGroup.R _group, int _repeatX, int _repeatY, Vector2 _center, float _unitScale, Vector2 _unitOffset, Vector2 _spacing)
    {
        group = _group;
        repeatX = _repeatX;
        repeatY = _repeatY;
        center = _center;
        unitScale = _unitScale;
        unitOffset = _unitOffset;
        spacing = _spacing;

        groupProperties = new SymmetryGroup(group, center); // TODO width and height don't do anything
        matrices = new List<Matrix4x4>();

        var initialMatrix = Matrix4x4.identity;
        var translation = Matrix4x4.Translate(unitOffset);
        var scale = Matrix4x4.Scale(Vector3.one * unitScale);
        createTranslations(initialMatrix * translation * scale);

        var ms = groupProperties.getCosetReps();
        for (var i = 0; i < ms.Length; i++)
        {
            Matrix4x4 m = ms[i];
            createTranslations(m * translation * scale);
        }
    }
    
    private void createSingleDirectionTranslations(Matrix4x4 m, float dx, float dy)
    {
        for (int n=0; n<repeatX; n++)
        {
            var translation = Matrix4x4.Translate(new Vector2(dx, dy));
            m = translation * m;
            matrices.Add(m);
        }
    }

    private void createTranslations(Matrix4x4 m)
    {
        float d1x = groupProperties.getTranslationX()[0] * spacing.x;
        float d1y = groupProperties.getTranslationY()[0] * spacing.y;
        float d2x = groupProperties.getTranslationX()[1] * spacing.x;
        float d2y = groupProperties.getTranslationY()[1] * spacing.y;
        
        for (int n=0; n < repeatY; n++)
        {
            createSingleDirectionTranslations(m, d1x, d1y);
            m = Matrix4x4.Translate(new Vector3(d2x, d2y, 0)) * m;
        }
    }
}