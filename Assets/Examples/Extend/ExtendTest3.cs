using Conway;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ExtendTest3 : MonoBehaviour
{
    public PolyHydraEnums.GridTypes GridType;
    public PolyHydraEnums.GridShapes GridShape;
    [Range(1,40)] public int width = 4;
    [Range(1,40)] public int depth = 3;
    public float amount = .1f;
    public float repeats = 4f;
    public float frequency = 1f;
    public float amplitude = 1f;
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
        for (int i = 0; i < repeats; i++)
        {
            float angle = Mathf.Sin(((float)i/repeats) * frequency * Mathf.PI) * amplitude;
            poly = poly.ExtendBoundaries(new OpParams{valueA = amount, valueB = angle});
        }
        
        if (ApplyOp)
        {
            var o = new OpParams {valueA = opAmount, valueB = op2Amount, facesel = facesel};
            poly = poly.ApplyOp(op, o);
        }
        
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, colorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}