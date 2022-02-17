using Conway;
using UnityEngine;
using UnityEngine.Serialization;
using Wythoff;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SliceByPolyTest : MonoBehaviour
{
    public PolyTypes PolyType;
    [Range(1,40)] public int PrismP = 4;
    [Range(1,40)] public int PrismQ = 4;

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

    [Space]
    public PolyTypes SlicePolyType;
    public float animateSlice = 0;
    public bool Weld;
    public bool Cap;
    public bool ShowSlicePoly;
    public Vector3 SlicePosition;
    public Vector3 SliceRotation;
    public float SliceScale = 1f;
    public Vector3 insideTransform;

    [Space]
    public bool VertexGizmos;
    public bool FaceGizmos;
    public bool EdgeGizmos;
    public bool FaceCenterGizmos;

    public int FaceCount;

    private ConwayPoly poly;
    
    void Start()
    {
        Generate();
    }

    private void OnValidate()
    {
        Generate();
    }
    
    private void Update()
    {
        Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        var wythoff = new WythoffPoly(PolyType, PrismP, PrismQ);
        wythoff.BuildFaces();
        poly = new ConwayPoly(wythoff);
        if (ApplyOp)
        {
            var o1 = new OpParams {valueA = op1Amount1, valueB = op1Amount2, facesel = op1Facesel};
            poly = poly.ApplyOp(op1, o1);
        }

        if (Canonicalize)
        {
            poly = poly.Canonicalize(0.01, 0.01);
        }
        
        poly.Transform(Position, Rotation, Scale);

        var rot = SliceRotation;
        if (animateSlice > 0)
        {
            rot.y = Time.time * animateSlice;
        }
        SliceRotation = rot;
        
        var sliceWythoff = new WythoffPoly(SlicePolyType, 3, 3);
        sliceWythoff.BuildFaces();
        var slicePoly = new ConwayPoly(sliceWythoff);
        slicePoly.Transform(SlicePosition, SliceRotation, Vector3.one * SliceScale);
        var result = poly.SliceByPoly(slicePoly, Cap, FaceCount);
        poly = result.outside;
        var inside = result.inside.Duplicate(insideTransform);
        poly.Append(inside);
        
        if (ShowSlicePoly) poly.Append(slicePoly);

        if (Weld)
        {
            poly = poly.Weld(0.0001f);
        }
        
        if (ApplyOp)
        {
            var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2, facesel = op2Facesel};
            poly = poly.ApplyOp(op2, o2);
        }

        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    void OnDrawGizmos () {
#if UNITY_EDITOR
        GizmoHelper.DrawGizmos(poly, transform, VertexGizmos, FaceGizmos, EdgeGizmos, FaceCenterGizmos);
#endif
    }


}
