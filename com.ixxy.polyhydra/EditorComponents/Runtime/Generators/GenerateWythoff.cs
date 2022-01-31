using Conway;
using Grids;
using UnityEngine;
using Wythoff;

[ExecuteInEditMode]
public class GenerateWythoff : BaseGenerator
{
    public PolyTypes PolyType;
    [Range(1,40)] public int PrismP = 4;
    [Range(1,40)] public int PrismQ = 4;

    public override ConwayPoly Generate()
    {
        var wythoff = new WythoffPoly(PolyType, PrismP, PrismQ);
        wythoff.BuildFaces();
        poly = new ConwayPoly(wythoff);
        base.Generate();
        return poly;
    }
}