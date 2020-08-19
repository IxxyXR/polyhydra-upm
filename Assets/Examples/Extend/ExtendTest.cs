using Conway;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ExtendTest : MonoBehaviour
{
    public PolyHydraEnums.GridTypes GridType;
    public PolyHydraEnums.GridShapes GridShape;
    [Range(1,40)] public int width = 4;
    [Range(1,40)] public int depth = 3;
    public float amount = 1;
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
        var colorMethod = ColorBySides ? PolyHydraEnums.ColorMethods.BySides : PolyHydraEnums.ColorMethods.ByRole;
        var poly = Grids.Grids.MakeGrid(GridType, GridShape, width, depth);
        poly = poly.FaceRemove(new OpParams {facesel = FaceSelections.Inner});
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, colorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
