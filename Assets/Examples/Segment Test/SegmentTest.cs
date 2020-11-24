using Conway;
using UnityEngine;
using Wythoff;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SegmentTest : MonoBehaviour
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
    public bool PreCanonicalize;
    public bool Canonicalize;
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;
    public PolyHydraEnums.ColorMethods ColorMethod;

    [Space]
    public float Amount;
    public float NormalBlend;
    public FaceSelections Facesel;
    public float AnimateAmount;
    public float AnimateAmountRate = 1f;

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
        
        // var animValue1 = Mathf.PerlinNoise(Time.time / AnimateAmountRate, 0) * Mathf.PerlinNoise(Time.time / AnimateAmountRate * 3.1f, Time.time);
        var animValue1 = 1;
        if (ApplyOp)
        {
            var o1 = new OpParams {valueA = op1Amount1 * animValue1, valueB = op1Amount2, facesel = op1Facesel};
            poly = poly.ApplyOp(op1, o1);
        }

        if (PreCanonicalize)
        {
            poly = poly.Canonicalize(0.01, 0.01);
        }
        
        poly = poly.Transform(Position, Rotation, Scale);

        var animValue2 = Mathf.Sin(Time.time / AnimateAmountRate);
        // var animValue2 = Mathf.PerlinNoise(Time.time / AnimateAmountRate, 0) * Mathf.PerlinNoise(Time.time / AnimateAmountRate * .3f, 10) -.5f;
        var _amount = AnimateAmount > 0 ? animValue2 * AnimateAmount : Amount;
        var o = new OpParams(_amount, NormalBlend, Facesel);
        poly = poly.Segment(o);

        if (Canonicalize)
        {
            poly = poly.Canonicalize(0.01, 0.01);
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
        // GizmoHelper.DrawGizmos(poly, transform, true, false, false, false);
    }


}
