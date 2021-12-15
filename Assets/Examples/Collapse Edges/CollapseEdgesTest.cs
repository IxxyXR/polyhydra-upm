using System;
using System.Collections.Generic;
using System.Linq;
using Conway;
using Johnson;
using NaughtyAttributes;
using UnityEngine;
using Wythoff;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CollapseEdgesTest : ExampleBase
{
    
    public enum ShapeTypes
    {
        Johnson,
        Grid,
        Wythoff,
        Other
    }

    [Serializable]
    public class FaceCollapseSetting
    {
        public bool enabled = true;
        public int FaceIndex = 0;
        public int EdgeIndex = 3;
    }
    
    // Only used to hide inspector fields with "ShowIf"
    private bool ShapeIsWythoff() => ShapeType == ShapeTypes.Wythoff;
    private bool ShapeIsJohnson() => ShapeType == ShapeTypes.Johnson;
    private bool ShapeIsGrid() => ShapeType == ShapeTypes.Grid;
    private bool ShapeIsOther() => ShapeType == ShapeTypes.Other;
    private bool UsesPandQ() => ShapeType == ShapeTypes.Grid || ShapeType == ShapeTypes.Other;
    
    public ShapeTypes ShapeType;

    [Range(1,40)] public int PrismP = 4;
    [ShowIf("UsesPandQ")] [Range(1,40)]
    public int PrismQ = 4;

    
    [ShowIf("ShapeIsWythoff")]
    public PolyTypes PolyType;
    [ShowIf("ShapeIsJohnson")]
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [ShowIf("ShapeIsGrid")]
    public PolyHydraEnums.GridTypes GridType;
    [ShowIf("ShapeIsGrid")]
    public PolyHydraEnums.GridShapes GridShape;
    [ShowIf("ShapeIsOther")]
    public PolyHydraEnums.OtherPolyTypes Other;

    [BoxGroup("Pre Op")] public Ops preop;
    [BoxGroup("Pre Op")] public FaceSelections preopFacesel;
    [BoxGroup("Pre Op")] public float preopAmount1 = 0;
    [BoxGroup("Pre Op")] public float preopAmount2 = 0;
    
    [BoxGroup("Collapse")] public List<FaceCollapseSetting> FaceCollapseSettings;
    [BoxGroup("Collapse")] public bool before;
    public override void Generate()
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
                poly = JohnsonPoly.BuildOther(Other, PrismP, PrismQ);
                break;
        }
        
        poly = poly.ApplyOp(preop, new OpParams {
            valueA = preopAmount1,
            valueB = preopAmount2,
            facesel = preopFacesel
        });

        foreach (var connection in FaceCollapseSettings)
        {
            if (!connection.enabled) continue;
            poly.CollapseEdge(connection.FaceIndex, connection.EdgeIndex);
        }
        
        if (Canonicalize && before)
        {
            poly = poly.Canonicalize(0.01, 0.01);
        }

        base.Generate();
    }

    public override void AfterAllOps()
    {

        if (Canonicalize && !before)
        {
            poly = poly.Canonicalize(0.01, 0.01);
        }
        if (Position != Vector3.zero || Rotation != Vector3.zero || Scale != Vector3.one)
        {
            poly = poly.Transform(Position, Rotation, Scale);
        }
    }

}
