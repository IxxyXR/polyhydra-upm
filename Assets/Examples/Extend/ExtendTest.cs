using Conway;
using Grids;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ExtendTest : MonoBehaviour
{
    public GridEnums.GridTypes GridType;
    public GridEnums.GridShapes GridShape;
    [Range(1,40)] public int width = 4;
    [Range(1,40)] public int depth = 3;
    public float amount = 1;
    public float angle = 0;
    public FaceSelections removeSelection = FaceSelections.Inner;
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
        var colorMethod = ColorBySides ? PolyHydraEnums.ColorMethods.BySides : PolyHydraEnums.ColorMethods.ByRole;
        var poly = Grids.Grids.MakeGrid(GridType, GridShape, width, depth);
        poly = poly.FaceRemove(new OpParams {facesel = removeSelection});
        poly = poly.ExtendBoundaries(new OpParams{valueA = amount, valueB = angle});
        
        if (ApplyOp)
        {
            var o = new OpParams {valueA = opAmount, valueB = op2Amount, facesel = facesel};
            poly = poly.ApplyOp(op, o);
        }
        
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, colorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
