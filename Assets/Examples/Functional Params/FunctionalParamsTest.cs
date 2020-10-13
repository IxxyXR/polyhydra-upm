using System;
using Conway;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FunctionalParamsTest : MonoBehaviour
{
    public enum Equations
    {
        LinearX,
        LinearY,
        LinearZ,
        Radial,
        Perlin,
    }

    public PolyHydraEnums.GridTypes GridType;
    public PolyHydraEnums.GridShapes GridShape;
    [Range(1,40)] public int width = 4;
    [Range(1,40)] public int depth = 3;
    public Equations Equation;
    public Ops op;
    public float frequency1;
    public float amplitude1;
    public float phase1;
    public float offset1;
    public float animationSpeed1;
    public float frequency2;
    public float amplitude2;
    public float phase2;
    public float offset2;
    public float animationSpeed2;
    public bool animatePhase;
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;


    public PolyHydraEnums.ColorMethods ColorMethod;

    void Start()
    {
        Generate();
    }

    private void OnValidate()
    {
        Generate();
    }

    void Update()
    {
        if (animatePhase)
        {
            Generate();
        }
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        var poly = Grids.Grids.MakeGrid(GridType, GridShape, width, depth);
        OpParams amount1Func = null;
        switch (Equation)
        {
            case Equations.LinearX:
                amount1Func = new OpParams
                {
                    funcA=x=>offset1 + Mathf.Sin((x.poly.Faces[x.index].Centroid.x * frequency1 + phase1 - (Time.time * animationSpeed1))) * amplitude1,
                    funcB=x=>offset2 + Mathf.Sin((x.poly.Faces[x.index].Centroid.x * frequency2 + phase2 - (Time.time * animationSpeed2))) * amplitude2
                };
                break;
            case Equations.LinearY:
                amount1Func = new OpParams
                {
                    funcA=x=>offset1 + Mathf.Sin((x.poly.Faces[x.index].Centroid.y * frequency1 + phase1 - (Time.time * animationSpeed1))) * amplitude1,
                    funcB=x=>offset2 + Mathf.Sin((x.poly.Faces[x.index].Centroid.y * frequency2 + phase2 - (Time.time * animationSpeed2))) * amplitude2
                };
                break;
            case Equations.LinearZ:
                amount1Func = new OpParams
                {
                    funcA=x=>offset1 + Mathf.Sin((x.poly.Faces[x.index].Centroid.z * frequency1 + phase1 - (Time.time * animationSpeed1))) * amplitude1,
                    funcB=x=>offset2 + Mathf.Sin((x.poly.Faces[x.index].Centroid.z * frequency2 + phase2 - (Time.time * animationSpeed2))) * amplitude2
                };
                break;
            case Equations.Radial:
                amount1Func = new OpParams
                {
                    funcA=x=>offset1 + Mathf.Sin((x.poly.Faces[x.index].Centroid.magnitude * frequency1 + phase1 - (Time.time * animationSpeed1))) * amplitude1,
                    funcB=x=>offset2 + Mathf.Sin((x.poly.Faces[x.index].Centroid.magnitude * frequency2 + phase2 - (Time.time * animationSpeed2))) * amplitude2
                };
                break;
            case Equations.Perlin:
                amount1Func = new OpParams
                {
                    funcA=x=>offset1 + Mathf.PerlinNoise(x.poly.Faces[x.index].Centroid.x * frequency1 + phase1 - (Time.time * animationSpeed1), x.poly.Faces[x.index].Centroid.z * frequency1 + phase1 - (Time.time * animationSpeed1)) * amplitude1,
                    funcB=x=>offset2 + Mathf.PerlinNoise(x.poly.Faces[x.index].Centroid.x * frequency2 + phase2 - (Time.time * animationSpeed2), x.poly.Faces[x.index].Centroid.z * frequency2 + phase2 - Time.time) * amplitude2,
                };
                break;
        }
        
        poly = poly.ApplyOp(op, amount1Func);
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
