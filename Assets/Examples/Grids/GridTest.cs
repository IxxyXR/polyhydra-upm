using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridTest : ExampleBase
{
    public PolyHydraEnums.GridTypes GridType;
    public PolyHydraEnums.GridShapes GridShape;
    [Range(1, 40)] public int width = 4;
    [Range(1, 40)] public int depth = 3;
    
    public override void Generate()
    {
        poly = Grids.Grids.MakeGrid(GridType, GridShape, width, depth);
        base.Generate();
    }
}
