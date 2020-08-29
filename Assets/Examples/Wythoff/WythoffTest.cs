using Conway;
using UnityEngine;
using Wythoff;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WythoffTest : MonoBehaviour
{
    public PolyTypes PolyType;
    [Range(1,40)] public int PrismP = 4;
    [Range(1,40)] public int PrismQ = 4;

    public bool ApplyOp;
    public Ops op;
    public FaceSelections facesel;
    public float opAmount = 0;
    public float op2Amount = 0;
    public bool Canonicalize;
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
        var wythoff = new WythoffPoly(PolyType, PrismP, PrismQ);
        wythoff.BuildFaces();
        var poly = new ConwayPoly(wythoff);
        var colorMethod = ColorBySides ? PolyHydraEnums.ColorMethods.BySides : PolyHydraEnums.ColorMethods.ByRole;
        if (ApplyOp)
        {
            var o = new OpParams {valueA = opAmount, valueB = op2Amount, facesel = facesel};
            poly = poly.ApplyOp(op, o);
        }

        if (Canonicalize)
        {
            poly = poly.Canonicalize(0.01, 0.01);
        }
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, colorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
