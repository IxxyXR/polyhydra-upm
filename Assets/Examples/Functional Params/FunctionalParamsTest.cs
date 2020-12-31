using Conway;
using Johnson;
using NaughtyAttributes;
using UnityEngine;
using Wythoff;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FunctionalParamsTest : MonoBehaviour
{
    public enum ShapeTypes
    {
        Johnson,
        Grid,
        Wythoff,
        Other
    }

    // Only used to hide inspector fields with "ShowIf"
    private bool ShapeIsWythoff() => ShapeType == ShapeTypes.Wythoff;
    private bool ShapeIsJohnson() => ShapeType == ShapeTypes.Johnson;
    private bool ShapeIsGrid() => ShapeType == ShapeTypes.Grid;
    private bool ShapeIsOther() => ShapeType == ShapeTypes.Other;
    private bool ShapeIsGridOrOther() => ShapeType == ShapeTypes.Grid || ShapeType == ShapeTypes.Other;

    public enum Equations
    {
        LinearX,
        LinearY,
        LinearZ,
        Radial,
        Perlin,
    }

    public ShapeTypes ShapeType;
    
    [ShowIf("ShapeIsWythoff")]
    public PolyTypes PolyType;
    [ShowIf("ShapeIsJohnson")]
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [ShowIf("ShapeIsGrid")]
    public PolyHydraEnums.GridTypes GridType;
    [ShowIf("ShapeIsGrid")]
    public PolyHydraEnums.GridShapes GridShape;
    [ShowIf("ShapeIsOther")]
    public PolyHydraEnums.OtherPolyTypes OtherPolyType;
    
    [Range(1,40)] public int PrismP = 4;
    [ShowIf("ShapeIsGridOrOther")] [Range(1,40)] public int PrismQ = 4;

    public bool applyPreOp = false;
    public Ops preOp;
    public FaceSelections preOpFacesel;
    public float preOpAmount1;
    public float preOpAmount2;
    public int preOpIterations = 1;

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

    private ConwayPoly poly;

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
        switch (ShapeType)
        {
            case ShapeTypes.Wythoff:
                var wythoff = new WythoffPoly(PolyType, PrismP, PrismQ);
                wythoff.BuildFaces();
                poly = new ConwayPoly(wythoff);
                break;
            case ShapeTypes.Johnson:
                poly = JohnsonPoly.Build(JohnsonPolyType, PrismP);
                break;
            case ShapeTypes.Grid:
                poly = Grids.Grids.MakeGrid(GridType, GridShape, PrismP, PrismQ);
                break;
            case ShapeTypes.Other:
                poly = JohnsonPoly.BuildOther(OtherPolyType, PrismP, PrismQ);
                break;
        }

        if (applyPreOp)
        {
            var preOpParams = new OpParams {valueA = preOpAmount1, valueB = preOpAmount2, facesel = preOpFacesel};
            for (int i = 0; i < preOpIterations; i++)
            {
                poly = poly.ApplyOp(preOp, preOpParams);
            }
        }

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
