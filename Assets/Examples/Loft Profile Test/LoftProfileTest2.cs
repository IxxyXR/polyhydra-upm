using System;
using Conway;
using Grids;
using UnityEngine;
using Random = UnityEngine.Random;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LoftProfileTest2 : MonoBehaviour
{
    public GridEnums.GridTypes GridType;
    public GridEnums.GridShapes GridShape;
    [Range(1,40)] public int width = 4;
    [Range(1,40)] public int depth = 3;

    [Range(0,1)]public float Jitter;
    [Range(0,1)]public float Density;

    public float DomeHeight = 1f;
    public int DomeSegments = 8;
    public Easing.EasingType easingType;
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
        
        Func<FilterParams, bool> pickRandomly = x =>
        {
            // Sets random seed based on face index
            // So that the results are the same if we call more than once
            Random.InitState(x.index);
            return Random.value<Density;
        };

        // Generate the ground grid and extrude random buildings
        var grid = Grids.Grids.MakeGrid(GridType, GridShape, width, depth);
        grid = grid.VertexRotate(new OpParams(Jitter, randomValues: true));
        var floors = grid.FaceKeep(new OpParams(.1f, pickRandomly));
        var houses = floors.Loft(new OpParams(0, x=>Random.Range(.5f, 1.5f)));
        var (walls, roofs) = houses.Split(new OpParams(FaceSelections.Existing));
        
        // Make window holes
        walls = walls.Loft(new OpParams(0.75f, FaceSelections.AllNew));
        walls = walls.FaceSlide(new OpParams(0.15f, FaceSelections.Existing));
        walls = walls.FaceRemove(new OpParams(FaceSelections.Existing));
        
        // Thicken the walls
        walls = walls.Shell(0.025f);
        
        // Add domes to the roofs
        var domes = roofs.LoftAlongProfile(FaceSelections.All, DomeHeight, DomeSegments, easingType);

        // Make nice patterns on the ground
        var ground = grid.Dual();
        ground = ground.Bevel(new OpParams(0.25f));
        ground = ground.Medial(new OpParams(3f));
        
        // Add some edging around buildings
        var edging = floors.Transform(new Vector3(0, .03f, 0));
        edging = edging.FaceScale(new OpParams(0.25f));
        edging.SetFaceRoles(ConwayPoly.Roles.Existing);
        
        // Assemble everything
        var town = new ConwayPoly();
        town.Append(edging);
        town.Append(ground);
        town.Append(walls);
        town.Append(domes);

        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(town, false, Colors, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
