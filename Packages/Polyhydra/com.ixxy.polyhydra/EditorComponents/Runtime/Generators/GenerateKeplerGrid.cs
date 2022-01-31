using Conway;
using Grids;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateKeplerGrid : BaseGenerator
{
    public GridEnums.KeplerTypes KeplerGridType;
    public GridEnums.GridShapes GridShape;
    [Range(1, 40)] public int width = 4;
    [Range(1, 40)] public int depth = 3;

    public override ConwayPoly Generate()
    {
        poly = Grids.Grids.MakeKepler(KeplerGridType, GridShape, width, depth);
        base.Generate();
        return poly;
    }
}