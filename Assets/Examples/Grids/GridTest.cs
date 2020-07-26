using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridTest : MonoBehaviour
{
    public PolyHydraEnums.GridTypes GridType;
    public PolyHydraEnums.GridShapes GridShape;
    [Range(1,40)] public int width = 4;
    [Range(1,40)] public int depth = 3;

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
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, PolyHydraEnums.ColorMethods.BySides);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
