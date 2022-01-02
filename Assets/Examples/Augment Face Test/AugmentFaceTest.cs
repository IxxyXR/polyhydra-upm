using System;
using System.Collections.Generic;
using Conway;
using Johnson;
using NaughtyAttributes;
using UnityEngine;
using Wythoff;
using Face = Conway.Face;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AugmentFaceTest : ExampleBase
{
    
    [Serializable] public class AugmentSetting
    {
        [Range(0,24)] public int startingEdge = 0;
        [Range(1,24)] public int faceSkip = 2;
        [Range(1,24)] public int edgeSkip = 2;
        [Range(3,24)] public int augmentSides = 4;
        public bool weld = false;
    }

    public List<AugmentSetting> AugmentSettings;
    
    // [Range(0,24)] public int startingEdge = 0;
    // [Range(1,24)] public int faceSkip = 2;
    // [Range(1,24)] public int edgeSkip = 2;
    // [Range(3,24)] public int augmentSides = 4;

    
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
        
        
        foreach (var setting in AugmentSettings)
        {
            var facesToAugment = new List<Face>();
            
            for (var i = 0; i < poly.Faces.Count; i += Mathf.Max(setting.faceSkip, 1))
            {
                facesToAugment.Add(poly.Faces[i]);
            }
            
            foreach (var face in facesToAugment)
            {
                int numSides = face.Sides;
                for (int j = setting.startingEdge % numSides; j < numSides; j += Mathf.Max(setting.edgeSkip, 1))
                {
                    poly.AugmentFace(face, j, setting.augmentSides);
                }
            }
            
            if (setting.weld)
            {
                poly = poly.Weld(0.01f);
            }
        }

        base.Generate();
    }
}