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

    public bool ApplyOp;
    public Ops op1;
    public FaceSelections op1Facesel;
    public float op1Amount1 = 0;
    public float op1Amount2 = 0;
    public Ops op2;
    public FaceSelections op2Facesel;
    public float op2Amount1 = 0;
    public float op2Amount2 = 0;
    public bool Canonicalize;
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;

    public PolyHydraEnums.ColorMethods ColorMethod;

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
                case PolyHydraEnums.OtherPolyTypes.GriddedCube:
                    poly = JohnsonPoly.GriddedCube(sides);
                    break;
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
        if (ApplyOp)
        {
            var o1 = new OpParams {valueA = op1Amount1, valueB = op1Amount2, facesel = op1Facesel};
            poly = poly.ApplyOp(op1, o1);
            var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2, facesel = op2Facesel};
            poly = poly.ApplyOp(op2, o2);
        }
        if (Canonicalize)
        {
            poly = poly.Canonicalize(0.1, 0.1);
        }
        poly = poly.Transform(Position, Rotation, Scale);

        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
