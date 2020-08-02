using System.Diagnostics;
using Conway;
using Johnson;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class OtherPolysTest : MonoBehaviour
{
    public PolyHydraEnums.OtherPolyTypes otherPolyType;
    [Range(1,40)] public int sides = 4;
    [Range(1,40)] public int segments = 3;

    void Start()
    {
        Generate();
    }

    private void OnValidate()
    {
        Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        ConwayPoly poly = null;

        // TODO move this into a method on JohnsonPoly
        switch (otherPolyType)
        {
                case PolyHydraEnums.OtherPolyTypes.Polygon:
                    poly = JohnsonPoly.Polygon(sides);
                    break;
                case PolyHydraEnums.OtherPolyTypes.UvSphere:
                    poly = JohnsonPoly.UvSphere(sides, segments);
                    break;
                case PolyHydraEnums.OtherPolyTypes.UvHemisphere:
                    poly = JohnsonPoly.UvHemisphere(sides, segments);
                    break;
                // TODO
                // case PolyHydraEnums.OtherPolyTypes.GriddedCube:
                //     poly = JohnsonPoly.GriddedCube();
                //     break;
                case PolyHydraEnums.OtherPolyTypes.C_Shape:
                    poly = JohnsonPoly.C_Shape();
                    break;
                case PolyHydraEnums.OtherPolyTypes.L_Shape:
                    poly = JohnsonPoly.L_Shape();
                    break;
                case PolyHydraEnums.OtherPolyTypes.L_Alt_Shape:
                    poly = JohnsonPoly.L_Alt_Shape();
                    break;
                case PolyHydraEnums.OtherPolyTypes.H_Shape:
                    poly = JohnsonPoly.H_Shape();
                    break;
        }
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, PolyHydraEnums.ColorMethods.BySides);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
