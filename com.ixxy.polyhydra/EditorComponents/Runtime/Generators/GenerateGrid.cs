using Conway;
using Grids;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateGrid : BaseGenerator
{
    public GridEnums.GridTypes GridType;
    public GridEnums.GridShapes GridShape;
    [Range(1, 40)] public int width = 4;
    [Range(1, 40)] public int depth = 3;

    public override ConwayPoly Generate()
    {
        poly = Grids.Grids.MakeGrid(GridType, GridShape, width, depth);
        base.Generate();
        return poly;
    }
}