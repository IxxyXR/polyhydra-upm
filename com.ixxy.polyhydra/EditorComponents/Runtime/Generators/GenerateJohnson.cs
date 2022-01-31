using Johnson;
using Conway;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateJohnson : BaseGenerator
{
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [Range(3,40)] public int sides = 4;

    public override ConwayPoly Generate()
    {
        poly = JohnsonPoly.Build(JohnsonPolyType, sides);
        base.Generate();
        return poly;
    }
}