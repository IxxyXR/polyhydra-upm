using Johnson;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class JohnsonTest : ExampleBase
{
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [Range(3,40)] public int sides = 4;
    
    public override void Generate()
    {
        poly = JohnsonPoly.Build(JohnsonPolyType, sides);
        base.Generate();
    }

}
