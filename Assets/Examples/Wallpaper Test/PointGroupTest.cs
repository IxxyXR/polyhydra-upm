using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Grids;
using NaughtyAttributes;
using UnityEngine.Rendering;


public class PointGroupTest : ExampleBase
{

    [Header("Poly")]
    
    public PolyHydraEnums.ShapeTypes ShapeType;
    
    // Only used to hide inspector fields with "ShowIf"
    private bool ShapeIsWythoff() => ShapeType==PolyHydraEnums.ShapeTypes.Uniform;
    private bool ShapeIsJohnson() => ShapeType==PolyHydraEnums.ShapeTypes.Johnson;
    private bool ShapeIsGrid() => ShapeType==PolyHydraEnums.ShapeTypes.Grid;
    private bool ShapeIsOther() => ShapeType==PolyHydraEnums.ShapeTypes.Other;
    private bool UsesPrismP()
    {
        var polybuilder = new PolyBuilder(ShapeType, PolyType, JohnsonPolyType, GridType, GridShape, OtherPolyType);
        return polybuilder.NumberofParams() > 0;
    }
    private bool UsesPrismQ()
    {
        var polybuilder = new PolyBuilder(ShapeType, PolyType, JohnsonPolyType, GridType, GridShape, OtherPolyType);
        return polybuilder.NumberofParams() > 1;
    }

    [ShowIf("ShapeIsWythoff")]
    public PolyTypes PolyType;
    [ShowIf("ShapeIsJohnson")]
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [ShowIf("ShapeIsGrid")]
    public GridEnums.GridTypes GridType;
    [ShowIf("ShapeIsGrid")]
    public GridEnums.GridShapes GridShape;
    [ShowIf("ShapeIsOther")]
    public PolyHydraEnums.OtherPolyTypes OtherPolyType;
    
    [ShowIf("UsesPrismP")] [Range(1,40)] public int PrismP = 4;
    [ShowIf("UsesPrismQ")] [Range(1,40)] public int PrismQ = 4;
    
    [Header("Symmetry")]
    public PointSymmetry.Family family;
    public int n = 3;
    public float radius = 1f;
    public bool SlowMerge = false;
    
    // [Header("Transform Before")]
    // public Vector3 PositionBefore = Vector3.zero;
    // public Vector3 RotationBefore = Vector3.zero;
    // public Vector3 ScaleBefore = Vector3.one;

    [Header("Transform Each")]
    public Vector3 PositionEach = Vector3.zero;
    public Vector3 RotationEach = Vector3.zero;
    public Vector3 ScaleEach = Vector3.one;
    public bool ApplyAfter = true;

    [Header("Rendering")]
    public bool EnableShadows;
        
    [BoxGroup("Gizmos")] public bool symmetryGizmos;
    
    private PointSymmetry sym;
    private List<Vector2> gizmoPath;

    public override void Generate()
    {
        var polybuilder = new PolyBuilder(ShapeType, PolyType, JohnsonPolyType, GridType, GridShape, OtherPolyType);
        polybuilder.Build(PrismP, PrismQ);
        poly = polybuilder.poly;
        sym = new PointSymmetry(family, n, radius);

        base.Generate();
    }
    public override void DoUpdate()
    {
        if (!SlowMerge)
        {
            GetComponent<MeshRenderer>().enabled = false;
            Mesh mesh;
            if (Application.isPlaying)
            {
                mesh = GetComponent<MeshFilter>().mesh;
            }
            else
            {
                mesh = GetComponent<MeshFilter>().sharedMesh;
            }
            
            if (mesh == null) return;
            
            var matrices = new List<Matrix4x4>();
            var transformBefore = Matrix4x4.TRS(Position, Quaternion.Euler(Rotation), Scale);
            var cumulativeTransform = Matrix4x4.TRS(PositionEach, Quaternion.Euler(RotationEach), ScaleEach);
            var currentCumulativeTransform = cumulativeTransform;

            var baseMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            foreach (var m in sym.matrices)
            {
                matrices.Add(
                    (baseMatrix * (ApplyAfter ? currentCumulativeTransform * m : m * currentCumulativeTransform)) * transformBefore
                );
                currentCumulativeTransform *= cumulativeTransform;
            }
            DrawInstances(mesh, GetComponent<MeshRenderer>().material, matrices);
        }

        base.DoUpdate();
    }
    
    private List<List<T>> Split<T> (List<T> source, int size)
    {
        return source
            .Select ((x, i) => new { Index = i, Value = x })
            .GroupBy (x => x.Index / size)
            .Select (x => x.Select (v => v.Value).ToList ())
            .ToList ();
    }
    
    public void DrawInstances(Mesh mesh, Material material, List<Matrix4x4> matrices)
    {
        
        ShadowCastingMode castShadows = EnableShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
        bool receiveShadows = EnableShadows;

        List<List<Matrix4x4>> batches;
        batches = Split (matrices, 1023);

        for (int batchIndex = 0; batchIndex < batches.Count; batchIndex++)
        {
            for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
            {
                Graphics.DrawMeshInstanced (mesh, subMeshIndex, material, batches[batchIndex], null, castShadows, receiveShadows);
            }
        }
    }

    public override void AfterAllOps()
    {
        base.AfterAllOps();
        if (SlowMerge)
        {
            GetComponent<MeshRenderer>().enabled = true;
            poly = poly.Cloner(sym.matrices, Matrix4x4.TRS(PositionEach, Quaternion.Euler(RotationEach), ScaleEach), ApplyAfter);
        }
    }

    public override void DoDrawGizmos()
    {
        if (sym==null) return;
            
        if (symmetryGizmos)
        {
            if (gizmoPath == null || gizmoPath.Count == 0)
            {
                gizmoPath = new List<Vector2>
                {
                    new Vector2(-0.25f, -0.5f),
                    new Vector2(0.25f, -0.5f),
                    new Vector2(0.25f, -0.2f),
                    new Vector2(-0.05f, -0.2f),
                    new Vector2(-0.05f, 0.5f),
                    // new Vector2(-0.25f, 0.5f),
                };
            }
            
            foreach (var m in sym.matrices)
            {
                var path = gizmoPath.Select(v => (Vector2)m.MultiplyPoint3x4(v)).ToList();
                DrawPathGizmo(path);
            }
        }
        
        base.DoDrawGizmos();
    }

    private void DrawPathGizmo(List<Vector2> path)
    {
        var initialPoint = new Vector3(path[0].x, path[0].y, 0);
        var prevPoint = initialPoint;
        for (int i = 1; i < path.Count; i++)
        {
            if (i==1) Gizmos.color = Color.red;
            else Gizmos.color = Color.yellow;
            
            var currentPoint = new Vector3(path[i].x, path[i].y, 0);
            Gizmos.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(prevPoint, initialPoint);
    }
}
