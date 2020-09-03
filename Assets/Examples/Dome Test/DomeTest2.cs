using Conway;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DomeTest2 : MonoBehaviour
{
    public PolyHydraEnums.GridTypes GridType;
    public PolyHydraEnums.GridShapes GridShape;
    [Range(1,40)] public int width = 4;
    [Range(1,40)] public int depth = 3;

    [Range(0,1)]public float Jitter;
    [Range(0,1)]public float Density;

    public float DomeHeight = 1f;
    public int DomeSegments = 8;
    [Range(0.001f, 1f)] public float DomeCurve1 = .01f;
    [Range(0.001f, 2f)] public float DomeCurve2 = .01f;
    public PolyHydraEnums.ColorMethods ColorMethod;
    
    
    public Color[] Colors =
    {
        new Color(1.0f, 0.5f, 0.5f),
        new Color(0.8f, 0.85f, 0.9f),
        new Color(0.5f, 0.6f, 0.6f),
        new Color(1.0f, 0.94f, 0.9f),
        new Color(0.66f, 0.2f, 0.2f),
        new Color(0.6f, 0.0f, 0.0f),
        new Color(1.0f, 1.0f, 1.0f),
        new Color(0.6f, 0.6f, 0.6f),
        new Color(0.5f, 1.0f, 0.5f),
        new Color(0.5f, 0.5f, 1.0f),
        new Color(0.5f, 1.0f, 1.0f),
        new Color(1.0f, 0.5f, 1.0f),
    };

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
        poly = poly.VertexRotate(new OpParams{valueA = Jitter, randomize = true});
        var (ground, houses) = poly.Split(new OpParams{filterFunc = x=>Random.value<Density, valueA = .1f});
        houses = houses.Loft(new OpParams{funcB=x=>Random.Range(.5f, 1.5f)});
        var (walls, domes) = houses.Split(new OpParams{facesel=FaceSelections.Existing});
        walls = walls.Loft(new OpParams {facesel = FaceSelections.AllNew, valueA = 0.75f});
        walls = walls.FaceSlide(new OpParams {valueA = 0.15f, facesel = FaceSelections.Existing});
        walls = walls.FaceRemove(new OpParams {facesel = FaceSelections.Existing});
        walls = walls.Shell(0.025f);
        domes = domes.Dome(FaceSelections.All, DomeHeight, DomeSegments, DomeCurve1, DomeCurve2);
        
        walls.Append(ground);
        walls.Append(domes);
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(walls, false, Colors, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
