using System;
using System.Collections.Generic;
using System.Linq;
using Conway;
using Grids;
using Johnson;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using Wythoff;
using Face = Conway.Face;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SplitEdgesTest : ExampleBase
{
    [Serializable] public class SplitEdgeSetting
    {
        [Range(0,24)] public int face = 0;
        [Range(1,24)] public int edge = 2;
    }

    public List<SplitEdgeSetting> splitEdgeSettings;

    public enum ShapeTypes
    {
        Johnson,
        Grid,
        Kepler,
        Wythoff,
        Other
    }

    // Only used to hide inspector fields with "ShowIf"
    private bool ShapeIsWythoff() => ShapeType == ShapeTypes.Wythoff;
    private bool ShapeIsJohnson() => ShapeType == ShapeTypes.Johnson;
    private bool ShapeIsGrid() => ShapeType == ShapeTypes.Grid;
    private bool ShapeIsKepler() => ShapeType == ShapeTypes.Kepler;
    private bool ShapeIsAnyGrid() => ShapeType == ShapeTypes.Grid || ShapeType == ShapeTypes.Kepler;
    private bool ShapeIsOther() => ShapeType == ShapeTypes.Other;
    private bool UsesPandQ() =>
        ShapeType == ShapeTypes.Grid || ShapeType == ShapeTypes.Other || ShapeType == ShapeTypes.Kepler;
    
    public ShapeTypes ShapeType;

    [Range(1,40)] public int PrismP = 4;
    [ShowIf("UsesPandQ")] [Range(1,40)]
    public int PrismQ = 4;

    
    [ShowIf("ShapeIsWythoff")]
    public PolyTypes PolyType;
    [ShowIf("ShapeIsJohnson")]
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [ShowIf("ShapeIsGrid")]
    public GridEnums.GridTypes GridType;
    [ShowIf("ShapeIsKepler")]
    public GridEnums.KeplerTypes KeplerType;
    [ShowIf("ShapeIsAnyGrid")]
    public GridEnums.GridShapes GridShape;
    [ShowIf("ShapeIsOther")]
    public PolyHydraEnums.OtherPolyTypes Other;

    [BoxGroup("Pre Op 1")] public Ops preop;
    [BoxGroup("Pre Op 1")] public FaceSelections preopFacesel;
    [BoxGroup("Pre Op 1")] public float preopAmount1 = 0;
    [BoxGroup("Pre Op 1")] public float preopAmount2 = 0;
    
    [BoxGroup("Pre Op 2")] public Ops preop2;
    [BoxGroup("Pre Op 2")] public FaceSelections preop2Facesel;
    [BoxGroup("Pre Op 2")] public float preop2Amount1 = 0;
    [BoxGroup("Pre Op 2")] public float preop2Amount2 = 0;
    
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
            case ShapeTypes.Kepler:
                poly = Grids.Grids.MakeKepler(KeplerType, GridShape, PrismP, PrismQ);
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

        poly = poly.ApplyOp(preop2, new OpParams {
            valueA = preop2Amount1,
            valueB = preop2Amount2,
            facesel = preop2Facesel
        });


        int faceCount = poly.Faces.Count;
        var edges = splitEdgeSettings.Select(x =>
        {
            var face = poly.Faces[x.face % faceCount];
            return face.GetHalfedges()[x.edge % face.Sides];
        });
        poly.SplitEdges(edges);

        base.Generate();
    }
}