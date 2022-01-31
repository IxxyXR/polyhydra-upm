using Conway;
using Grids;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LoftProfileTest : MonoBehaviour
{
    public GridEnums.GridTypes GridType;
    public GridEnums.GridShapes GridShape;
    [Range(1,40)] public int width = 4;
    [Range(1,40)] public int depth = 3;
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;


    public bool ApplyOp;
    public Ops op1;
    public FaceSelections op1Facesel;
    public float op1Amount1 = 0;
    public float op1Amount2 = 0;
    public bool op1Animate;
    public FaceSelections domeFaceSel;
    public Easing.EasingType easingType;
    public float DomeHeight = 1f;
    public int DomeSegments = 8;
    public Ops op2;
    public FaceSelections op2Facesel;
    public float op2Amount1 = 0;
    public float op2Amount2 = 0;
    public bool op2Animate;
    public PolyHydraEnums.ColorMethods ColorMethod;

    void Start()
    {
        Generate();
    }

    void Update()
    {
        if (op1Animate || op2Animate)
        {
            Generate();
        }
    }

    private void OnValidate()
    {
        Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        var poly = Grids.Grids.MakeGrid(GridType, GridShape, width, depth);
        if (ApplyOp)
        {
            var o1 = new OpParams {valueA = op1Amount1 * Mathf.Abs(op1Animate ? Mathf.Sin(Time.time) : 1), valueB = op1Amount2, facesel = op1Facesel};
            poly = poly.ApplyOp(op1, o1);
        }
        poly = poly.LoftAlongProfile(domeFaceSel, DomeHeight, DomeSegments, easingType);
        if (ApplyOp)
        {
            var o2 = new OpParams {valueA = op2Amount1 * Mathf.Abs(op2Animate ? Mathf.Cos(Time.time * .6f) : 1), valueB = op2Amount2, facesel = op2Facesel};
            poly = poly.ApplyOp(op2, o2);
        }
        poly = poly.Transform(Position, Rotation, Scale);

        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
