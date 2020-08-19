using System.Diagnostics;
using Conway;
using Johnson;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class JohnsonTest : MonoBehaviour
{
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [Range(1,40)] public int sides = 4;

    public bool ApplyOp;
    public Ops op;
    public FaceSelections facesel;
    public float opAmount = 0;
    public float op2Amount = 0;

    public bool ColorBySides;

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
        switch (JohnsonPolyType)
        {
                case PolyHydraEnums.JohnsonPolyTypes.Prism:
                    poly = JohnsonPoly.Prism(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.Antiprism:
                    poly = JohnsonPoly.Antiprism(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.Pyramid:
                    poly = JohnsonPoly.Pyramid(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.ElongatedPyramid:
                    poly = JohnsonPoly.ElongatedPyramid(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.GyroelongatedPyramid:
                    poly = JohnsonPoly.GyroelongatedPyramid(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.Dipyramid:
                    poly = JohnsonPoly.Dipyramid(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.ElongatedDipyramid:
                    poly = JohnsonPoly.ElongatedDipyramid(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.GyroelongatedDipyramid:
                    poly = JohnsonPoly.GyroelongatedDipyramid(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.Cupola:
                    poly = JohnsonPoly.Cupola(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.ElongatedCupola:
                    poly = JohnsonPoly.ElongatedCupola(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.GyroelongatedCupola:
                    poly = JohnsonPoly.GyroelongatedCupola(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.OrthoBicupola:
                    poly = JohnsonPoly.OrthoBicupola(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.GyroBicupola:
                    poly = JohnsonPoly.GyroBicupola(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.ElongatedOrthoBicupola:
                    poly = JohnsonPoly.ElongatedBicupola(sides, false);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.ElongatedGyroBicupola:
                    poly = JohnsonPoly.ElongatedBicupola(sides, true);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.GyroelongatedBicupola:
                    poly = JohnsonPoly.GyroelongatedBicupola(sides, false);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.Rotunda:
                    poly = JohnsonPoly.Rotunda();
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.ElongatedRotunda:
                    poly = JohnsonPoly.ElongatedRotunda();
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.GyroelongatedRotunda:
                    poly = JohnsonPoly.GyroelongatedRotunda();
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.GyroelongatedBirotunda:
                    poly = JohnsonPoly.GyroelongatedBirotunda();
                    break;

        }
        var colorMethod = ColorBySides ? PolyHydraEnums.ColorMethods.BySides : PolyHydraEnums.ColorMethods.ByRole;
        if (ApplyOp)
        {
            poly.TagFaces("top", facesel);
            var o = new OpParams {valueA = opAmount, valueB = op2Amount, tags = "top"};
            poly = poly.ApplyOp(op, o);
        }
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, colorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
