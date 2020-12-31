using Conway;
using UnityEngine;
using Wythoff;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SliceTest : MonoBehaviour
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
    public float animateSlice = 0;
    public bool includeTop = true;
    public bool includeBottom = true;
    public bool Weld;
    public bool Cap;
    public Vector3 SlicePosition;
    public Vector3 SliceRotation;
    public Vector3 topTransform;

    private ConwayPoly poly;
    private ConwayPoly cap;
    
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
        
        poly = poly.Transform(Position, Rotation, Scale);

        var pos = SlicePosition;
        if (animateSlice > 0)
        {
            pos.y = .1f + transform.position.y + ((Time.time % 2f) * animateSlice);
        }
        
        var result = poly.SliceByPlane(new Plane(Quaternion.Euler(SliceRotation) * Vector3.up, pos), Cap, includeTop, includeBottom);
        poly = result.bottom;
        var top = result.top.Transform(topTransform);
        poly.Append(top);

        // var sliceWythoff = new WythoffPoly(PolyTypes.Cube, 3, 3);
        // sliceWythoff.BuildFaces();
        // var slicePoly = new ConwayPoly(wythoff);
        // slicePoly = slicePoly.Transform(SlicePosition, SliceRotation, Vector3.one * SliceScale);
        // var result = poly.SliceByPoly(slicePoly, Cap);
        // poly = result.outside;
        // var top = result.outside.Transform(topTransform);

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
        GizmoHelper.DrawGizmos(cap, transform, true, false, false, false);
    }


}
