using System;
using System.Collections.Generic;
using Conway;
using Grids;
using Johnson;
using NaughtyAttributes;
using UnityEngine;
using Wythoff;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CollapseMultipleEdges : ExampleBase
{
    
    public enum ShapeTypes
    {
        Johnson = 0,
        Grid = 1,
        Wythoff = 2,
        Other = 3
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
    public GridEnums.GridTypes GridType;
    [ShowIf("ShapeIsGrid")]
    public GridEnums.GridShapes GridShape;
    [ShowIf("ShapeIsOther")]
    public PolyHydraEnums.OtherPolyTypes Other;

    [BoxGroup("Pre Op")] public Ops preop;
    [BoxGroup("Pre Op")] public FaceSelections preopFacesel;
    [BoxGroup("Pre Op")] public float preopAmount1 = 0;
    [BoxGroup("Pre Op")] public float preopAmount2 = 0;
    [BoxGroup("Pre Op 2")] public Ops preop2;
    [BoxGroup("Pre Op 2")] public FaceSelections preop2Facesel;
    [BoxGroup("Pre Op 2")] public float preop2Amount1 = 0;
    [BoxGroup("Pre Op 2")] public float preop2Amount2 = 0;
    
    [Serializable]
    public class CollapseSetting
    {
        public int numSidesA = 1;
        public int numSidesB = 1;
        public bool either = true;
    }
    [BoxGroup("Collapse")] public List<CollapseSetting> collapseSides;
    
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

        poly = poly.ApplyOp(preop2, new OpParams {
            valueA = preop2Amount1,
            valueB = preop2Amount2,
            facesel = preop2Facesel
        });

        foreach (var collapse in collapseSides)
        {
            if (collapse.numSidesA > 2 && collapse.numSidesB > 2)
            {
                poly.CollapseEdges(collapse.numSidesA, collapse.numSidesB, collapse.either);
            }
        }

        base.Generate();
    }

}
