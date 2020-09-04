using System;
using Conway;
using UnityEngine;
using Random = UnityEngine.Random;


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
        
        Func<FilterParams, bool> randomBoolPerFace = x =>
        {
            // Sets random seed based on face index
            // So that the results are the same if we call more than once
            Random.InitState(x.index);
            return Random.value<Density;
        };

        var grid = Grids.Grids.MakeGrid(GridType, GridShape, width, depth);
        grid = grid.VertexRotate(new OpParams(Jitter, randomValues: true));
        var houses = grid.FaceKeep(new OpParams(.1f, selection: randomBoolPerFace));
        houses = houses.Loft(new OpParams(0, x=>Random.Range(.5f, 1.5f)));
        var (walls, domes) = houses.Split(new OpParams(FaceSelections.Existing));
        walls = walls.Loft(new OpParams(0.75f, FaceSelections.AllNew));
        walls = walls.FaceSlide(new OpParams(0.15f, FaceSelections.Existing));
        walls = walls.FaceRemove(new OpParams(FaceSelections.Existing));
        walls = walls.Shell(0.025f);
        domes = domes.Dome(FaceSelections.All, DomeHeight, DomeSegments, DomeCurve1, DomeCurve2);

        var ground = grid.Dual();
        ground = ground.Bevel(new OpParams(0.25f));
        ground = ground.Medial(new OpParams(3f));
        walls.Append(ground);
        walls.Append(domes);

        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(walls, false, Colors, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
