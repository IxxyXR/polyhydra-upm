using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;


public class WallPaperTest : ExampleBase
{

    [Header("Poly")]
    
    public PolyHydraEnums.ShapeTypes ShapeType;
    
    // Only used to hide inspector fields with "ShowIf"
    private bool ShapeIsWythoff() => ShapeType==PolyHydraEnums.ShapeTypes.Uniform;
    private bool ShapeIsJohnson() => ShapeType==PolyHydraEnums.ShapeTypes.Johnson;
    private bool ShapeIsGrid() => ShapeType==PolyHydraEnums.ShapeTypes.Grid;
    private bool ShapeIsOther() => ShapeType==PolyHydraEnums.ShapeTypes.Grid;
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
    public PolyHydraEnums.GridTypes GridType;
    [ShowIf("ShapeIsGrid")]
    public PolyHydraEnums.GridShapes GridShape;
    [ShowIf("ShapeIsOther")]
    public PolyHydraEnums.OtherPolyTypes OtherPolyType;
    
    [ShowIf("UsesPrismP")] [Range(1,40)] public int PrismP = 4;
    [ShowIf("UsesPrismQ")] [Range(1,40)] public int PrismQ = 4;
    
    [Header("Symmetry")]
    public SymmetryGroup.R group;
    public int RepeatX = 1;
    public int RepeatY = 1;
    public Vector2 Center = Vector2.zero;
    public float UnitScale = 1f;
    public Vector2 UnitOffset = Vector2.zero;
    public Vector2 Spacing = Vector2.one;

    [BoxGroup("Gizmos")] public bool symmetryGizmos;
    [BoxGroup("Gizmos")] public bool domainGizmos;
    
    private WallpaperSymmetry sym;
    private List<Vector2> gizmoPath;

    public override void Generate()
    {
        var polybuilder = new PolyBuilder(ShapeType, PolyType, JohnsonPolyType, GridType, GridShape, OtherPolyType);
        polybuilder.Build(PrismP, PrismQ);
        poly = polybuilder.poly;
        base.Generate();
    }

    public override void AfterAllOps()
    {
        base.AfterAllOps();
        sym = new WallpaperSymmetry(group, RepeatX, RepeatY, Center, UnitScale, UnitOffset, Spacing);
        poly = poly.WallpaperClone(sym);
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
        
        if (domainGizmos)
        {
            foreach (var m in sym.matrices)
            {
                var points = sym.groupProperties.fundamentalRegion.points;
                var path = points.Select(v => (Vector2)m.MultiplyPoint3x4(v)).ToList();
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
