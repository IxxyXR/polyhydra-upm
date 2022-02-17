using Conway;
using Grids;
using Johnson;
using NaughtyAttributes;
using UnityEngine;
using Wythoff;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SegmentTest : MonoBehaviour
{
    public enum ShapeTypes
    {
        Johnson,
        Grid,
        Wythoff
    }

    // Only used to hide inspector fields with "ShowIf"
    private bool ShapeIsWythoff() => ShapeType == ShapeTypes.Wythoff;
    private bool ShapeIsJohnson() => ShapeType == ShapeTypes.Johnson;
    private bool ShapeIsGrid() => ShapeType == ShapeTypes.Grid;

    public ShapeTypes ShapeType;
    
    [ShowIf("ShapeIsWythoff")]
    public PolyTypes PolyType;
    [ShowIf("ShapeIsJohnson")]
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [ShowIf("ShapeIsGrid")]
    public GridEnums.GridTypes GridType;
    [ShowIf("ShapeIsGrid")]
    public GridEnums.GridShapes GridShape;
    
    [Range(1,40)] public int PrismP = 4;
    [ShowIf("ShapeIsGrid")] [Range(1,40)] public int PrismQ = 4;

    public bool ApplyOp;
    
    [BoxGroup("Op 1")] public bool PreCanonicalize;
    [BoxGroup("Op 1"), Label("")] public Ops op1;
    [BoxGroup("Op 1"), Label("Faces")] public FaceSelections op1Facesel;
    [BoxGroup("Op 1"), Label("Amount")] public float op1Amount1;
    [BoxGroup("Op 1"), Label("Amount2")] public float op1Amount2;

    [Space]
    
    [BoxGroup("Segments")] public float Amount;
    [BoxGroup("Segments")] public float NormalBlend;
    [BoxGroup("Segments")] public FaceSelections Facesel;

    [Space]

    [BoxGroup("Op 2")] public bool Canonicalize;
    [BoxGroup("Op 2"), Label("")] public Ops op2;
    [BoxGroup("Op 2"), Label("Faces")] public FaceSelections op2Facesel;
    [BoxGroup("Op 2"), Label("Amount")] public float op2Amount1;
    [BoxGroup("Op 2"), Label("Amount2")] public float op2Amount2;
    
    [Space]
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;
    public PolyHydraEnums.ColorMethods ColorMethod;

    [Space]
    public float AnimateAmount;
    public float AnimateAmountRate = 1f;

    private ConwayPoly preOpPoly;
    
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
        switch (ShapeType)
        {
            case ShapeTypes.Wythoff:
                var wythoff = new WythoffPoly(PolyType, PrismP, PrismQ);
                wythoff.BuildFaces();
                preOpPoly = new ConwayPoly(wythoff);
                break;
            case ShapeTypes.Johnson:
                preOpPoly = JohnsonPoly.Build(JohnsonPolyType, PrismP);
                break;
            case ShapeTypes.Grid:
                preOpPoly = Grids.Grids.MakeGrid(GridType, GridShape, PrismP, PrismQ);
                break;
        }
        
        // var animValue1 = Mathf.PerlinNoise(Time.time / AnimateAmountRate, 0) * Mathf.PerlinNoise(Time.time / AnimateAmountRate * 3.1f, Time.time);
        var animValue1 = 1;
        if (ApplyOp)
        {
            var o1 = new OpParams {valueA = op1Amount1 * animValue1, valueB = op1Amount2, facesel = op1Facesel};
            preOpPoly = preOpPoly.ApplyOp(op1, o1);
        }

        if (PreCanonicalize)
        {
            preOpPoly = preOpPoly.Canonicalize(0.01, 0.01);
        }
        
        var postOpPoly = preOpPoly.Duplicate(Position, Rotation, Scale);

        var animValue2 = Mathf.Sin(Time.time / AnimateAmountRate);
        // var animValue2 = Mathf.PerlinNoise(Time.time / AnimateAmountRate, 0) * Mathf.PerlinNoise(Time.time / AnimateAmountRate * .3f, 10) -.5f;
        var _amount = AnimateAmount > 0 ? animValue2 * AnimateAmount : Amount;
        var o = new OpParams(_amount, NormalBlend, Facesel);
        postOpPoly = postOpPoly.Segment(o);

        if (Canonicalize)
        {
            postOpPoly = postOpPoly.Canonicalize(0.01, 0.01);
        }

        if (ApplyOp)
        {
            var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2, facesel = op2Facesel};
            postOpPoly = postOpPoly.ApplyOp(op2, o2);
        }

        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(postOpPoly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    void OnDrawGizmos () {
        // GizmoHelper.DrawGizmos(poly, transform, true, false, false, false);
    }


}
