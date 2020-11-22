using Conway;
using UnityEngine;
using Wythoff;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MatrixComparison : MonoBehaviour
{
    public PolyTypes PolyType;
    [Range(1,40)] public int PrismP = 4;
    [Range(1,40)] public int PrismQ = 4;

    public Ops op;
    public FaceSelections op1Facesel;
    public float op1Amount1 = 0;
    public float op1Amount2 = 0;
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
        var wythoff = new WythoffPoly(PolyType, PrismP, PrismQ);
        wythoff.BuildFaces();
        var poly = new ConwayPoly(wythoff);
        ConwayPoly d, x, xd, dx, dxd;
        var o = new OpParams {valueA = op1Amount1, valueB = op1Amount2, facesel = op1Facesel};
        d = poly.Dual();
        
        x = poly.ApplyOp(op, o);
        
        xd = d.ApplyOp(op, o);
        
        dx = poly.ApplyOp(op, o);
        dx = dx.Dual();
        
        dxd = d.ApplyOp(op, o);
        dxd = dxd.Dual();

        if (Canonicalize)
        {
            x = x.Canonicalize(0.01, 0.01);
            dx = dx.Canonicalize(0.01, 0.01);
            dxd = dxd.Canonicalize(0.01, 0.01);
            xd = xd.Canonicalize(0.01, 0.01);
        }
        
        Debug.Log($"x: {x.vef.v},{x.vef.e},{x.vef.f}");
        Debug.Log($"xd: {xd.vef.v},{xd.vef.e},{xd.vef.f}");
        Debug.Log($"dx: {dx.vef.v},{dx.vef.e},{dx.vef.f}");
        Debug.Log($"dxd: {dxd.vef.v},{dxd.vef.e},{dxd.vef.f}");

        var allPoly = new ConwayPoly();
            
        x = x.Transform(-Vector3.left);
        xd = xd.Transform(Vector3.left * 2);
        dx = dx.Transform(Vector3.left * 4);
        dxd = dxd.Transform(Vector3.left * 6);
        
        allPoly.Append(x);
        allPoly.Append(xd);
        allPoly.Append(dx);
        allPoly.Append(dxd);
        allPoly.Recenter();

        allPoly = allPoly.Transform(Position, Rotation, Scale);
        
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(allPoly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
