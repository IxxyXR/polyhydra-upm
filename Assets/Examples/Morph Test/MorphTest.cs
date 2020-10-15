using System;
using System.Collections.Generic;
using Conway;
using Johnson;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using Wythoff;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class MorphTest : MonoBehaviour
{

    public enum ShapeTypes
    {
        Johnson,
        Grid,
        Wythoff
    }

    // Only used to hide inspector fields with "ShowIf"
    private bool ShapeIsWythoff() => ShapeType == ShapeTypes.Wythoff;
    private bool ShapeIsJohnson() => ShapeType == ShapeTypes.Johnson;
    private bool ShapeIsGrid() => ShapeType == ShapeTypes.Grid;

    public ShapeTypes ShapeType;
    
    [ShowIf("ShapeIsWythoff")]
    public PolyTypes PolyType1;
    [ShowIf("ShapeIsWythoff")]
    public PolyTypes PolyType2;
    
    [ShowIf("ShapeIsJohnson")]
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType1;
    [ShowIf("ShapeIsJohnson")]
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType2;
    
    [ShowIf("ShapeIsGrid")]
    public PolyHydraEnums.GridTypes GridType;
    [ShowIf("ShapeIsGrid")]
    public PolyHydraEnums.GridShapes GridShape1;
    [ShowIf("ShapeIsGrid")]
    public PolyHydraEnums.GridShapes GridShape2;

    [Range(1,40)] public int P1 = 6;
    [Range(1,40)] public int P2 = 6;
    [ShowIf("ShapeIsGrid")] [Range(1,40)] public int Q1 = 6;
    [ShowIf("ShapeIsGrid")] [Range(1,40)] public int Q2 = 6;

    [BoxGroup("Op 1"), Label("")] public Ops op1;
    [BoxGroup("Op 1"), Label("Faces")] public FaceSelections op1Facesel;
    [BoxGroup("Op 1"), Range(0, 3f), Label("Amount")] public float op1Amount1;
    [BoxGroup("Op 1"), Range(0, .5f), Label("Amount2")] public float op1Amount2;
    
    [BoxGroup("Op 2"), Label("")] public Ops op2;
    [BoxGroup("Op 2"), Label("Faces")] public FaceSelections op2Facesel;
    [BoxGroup("Op 2"), Range(0, .5f), Label("Amount")] public float op2Amount1;
    [BoxGroup("Op 2"), Range(0, .5f), Label("Amount2")] public float op2Amount2;
    
    [Space]
    public PolyHydraEnums.ColorMethods ColorMethod;

    [Space]
    public float morphAmount = 0.5f;
    public bool reverseVertexOrder;
    
    private ConwayPoly poly1;
    private ConwayPoly poly2;


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
        switch (ShapeType)
        {
            case ShapeTypes.Wythoff:
                var wythoff1 = new WythoffPoly(PolyType1, P1, Q1);
                wythoff1.BuildFaces();
                poly1 = new ConwayPoly(wythoff1);
                var wythoff2 = new WythoffPoly(PolyType2, P2, Q2);
                wythoff2.BuildFaces();
                poly2 = new ConwayPoly(wythoff2);
                break;
            case ShapeTypes.Johnson:
                poly1 = JohnsonPoly.Build(JohnsonPolyType1, P1);
                poly2 = JohnsonPoly.Build(JohnsonPolyType2, P2);
                break;
            case ShapeTypes.Grid:
                poly1 = Grids.Grids.MakeGrid(GridType, GridShape1, P1, Q1, false);
                poly2 = Grids.Grids.MakeGrid(GridType, GridShape2, P2, Q2, false);
                break;
        }

        var o1 = new OpParams {valueA = op1Amount1, valueB = op1Amount2, facesel = op1Facesel};
        poly1 = poly1.ApplyOp(op1, o1);

        var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2, facesel = op2Facesel};
        poly2 = poly2.ApplyOp(op2, o2);

        poly1.Morph(poly2, morphAmount, reverseVertexOrder);

        // Final Mesh
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly1, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}

