using System.Collections.Generic;
using Conway;
using Grids;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateMultigrid : BaseGenerator
{

    [Range(1, 30)] public int Divisions = 5;
    [Range(3, 30)] public int Dimensions = 5;
    public float Offset = .2f;
    public bool randomize;
    public float MinDistance = 0f;
    public float MaxDistance = 1f;
    public bool SharedVertices;
    public ConwayPoly.TagType TagType;
    public float colorRatio = 1.0f;
    public float colorIndex;
    public float colorIntersect;

    private List<List<Vector2>> shapes;

    public override ConwayPoly Generate()
    {
        var multigrid = new MultiGrid(Divisions, Dimensions, Offset, MinDistance, MaxDistance, colorRatio, colorIndex, colorIntersect);
        (poly, shapes, _) = multigrid.Build(SharedVertices, randomize);
        if (shapes.Count == 0) return poly;
        base.Generate();
        return poly;
    }
}