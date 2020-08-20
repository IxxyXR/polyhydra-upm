using Conway;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridTest : MonoBehaviour
{
    public PolyHydraEnums.GridTypes GridType;
    public PolyHydraEnums.GridShapes GridShape;
    [Range(1,40)] public int width = 4;
    [Range(1,40)] public int depth = 3;
    public bool ColorBySides;

    public bool ApplyOp;
    public Ops op;
    public FaceSelections facesel;
    public float opAmount = 0;
    public float op2Amount = 0;

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
        var poly = Grids.Grids.MakeGrid(GridType, GridShape, width, depth);
        var colorMethod = ColorBySides ? PolyHydraEnums.ColorMethods.BySides : PolyHydraEnums.ColorMethods.ByRole;
        if (ApplyOp)
        {
            var o = new OpParams {valueA = opAmount, valueB = op2Amount, facesel = facesel};
            poly = poly.ApplyOp(op, o);
        }
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, colorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
