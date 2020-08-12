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
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, colorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
