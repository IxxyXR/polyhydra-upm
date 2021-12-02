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
public class WindowTest : ExampleBase
{
    
    public enum ShapeTypes
    {
        Johnson,
        Grid,
        Wythoff,
        Other
    }

    [Serializable]
    public class FaceConnectSetting
    {
        public bool enabled = true;
        public int FaceOneIndex = 0;
        public int FaceTwoIndex = 3;
        public float InsetAmount = 0.2f;
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
    
    [BoxGroup("Add Depth to Flat Shapes")] public float ExtrusionDepth = .4f;
    [BoxGroup("Connect")] public List<FaceConnectSetting> FaceConnectSettings;

    public override void Generate()
    {
        OpParams extrudeOp;
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
                extrudeOp = new OpParams(ExtrusionDepth);
                poly = poly.Shell(extrudeOp);
                break;
            case ShapeTypes.Other:
                poly = JohnsonPoly.BuildOther(Other, PrismP, PrismQ);
                extrudeOp = new OpParams(ExtrusionDepth);
                switch (Other)
                {
                    case PolyHydraEnums.OtherPolyTypes.Polygon:
                    case PolyHydraEnums.OtherPolyTypes.C_Shape:
                    case PolyHydraEnums.OtherPolyTypes.H_Shape:
                        poly = poly.Shell(extrudeOp);
                        break;
                    default:
                        break;
                }
                break;
        }
        
        poly = poly.ApplyOp(preop, new OpParams {
            valueA = preopAmount1,
            valueB = preopAmount2,
            facesel = preopFacesel
        });

        foreach (var connection in FaceConnectSettings)
        {
            if (!connection.enabled) continue;
            poly = poly.ConnectFaces(
                connection.FaceOneIndex,
                connection.FaceTwoIndex,
                connection.InsetAmount);
        }

        base.Generate();
    }

}
