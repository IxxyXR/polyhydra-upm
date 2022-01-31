using Conway;
using Grids;
using Johnson;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateOther : BaseGenerator
{
    public PolyHydraEnums.OtherPolyTypes otherPolyType;
    [Range(1,40)] public int sides = 4;
    [Range(1,40)] public int segments = 3;

    public override ConwayPoly Generate()
    {
        var poly = JohnsonPoly.BuildOther(otherPolyType, sides, segments);
        base.Generate();
        return poly;
    }
}