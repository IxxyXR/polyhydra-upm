using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Conway;
using Johnson;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using Wythoff;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class PolyStats : MonoBehaviour
{

    public enum ShapeTypes
    {
        Johnson,
        Grid,
        Wythoff
    }
    //
    // // Only used to hide inspector fields with "ShowIf"
    // private bool ShapeIsWythoff() => ShapeType == ShapeTypes.Wythoff;
    // private bool ShapeIsJohnson() => ShapeType == ShapeTypes.Johnson;
    // private bool ShapeIsGrid() => ShapeType == ShapeTypes.Grid;

    // public ShapeTypes ShapeType;
    //
    // [ShowIf("ShapeIsWythoff")]
    // public PolyTypes PolyType;
    //
    // [ShowIf("ShapeIsJohnson")]
    // public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    //
    // [ShowIf("ShapeIsGrid")]
    // public GridEnums.GridTypes GridType;
    // [ShowIf("ShapeIsGrid")]
    // public GridEnums.GridShapes GridShape;
    //
    // [Range(1,40)] public int P = 6;
    // [ShowIf("ShapeIsGrid")] [Range(1,40)] public int Q = 6;

    // [BoxGroup("Op 1"), Label("")] public Ops op1;
    // [BoxGroup("Op 1"), Label("Faces")] public FaceSelections op1Facesel;
    // [BoxGroup("Op 1"), Range(0, 3f), Label("Amount")] public float op1Amount1;
    // [BoxGroup("Op 1"), Range(0, .5f), Label("Amount2")] public float op1Amount2;
    //
    // [BoxGroup("Op 2"), Label("")] public Ops op2;
    // [BoxGroup("Op 2"), Label("Faces")] public FaceSelections op2Facesel;
    // [BoxGroup("Op 2"), Range(0, .5f), Label("Amount")] public float op2Amount1;
    // [BoxGroup("Op 2"), Range(0, .5f), Label("Amount2")] public float op2Amount2;
    
    [Space]
    public PolyHydraEnums.ColorMethods ColorMethod;
    
    void Start()
    {
        GenerateAll();
    }

    public void GenerateAll()
    {
        var polyTypeList = Uniform.Platonic
            .Concat(Uniform.Archimedean)
            .Concat(Uniform.KeplerPoinsot);

        foreach (var targetItem in polyTypeList.Distinct())
        {
            var targetPolyType = targetItem;
            var targetPoly = GenerateWythoff(targetPolyType);
            foreach (var sourceItem in polyTypeList)
            {
                var sourcePolyType = sourceItem;
                var sourcePoly = GenerateWythoff(sourcePolyType);
                FindOpMatches(sourcePoly, sourcePolyType.Name, targetPoly, targetPolyType.Name);
            }
        }
    }

    public ConwayPoly GenerateWythoff(Uniform polyType)
    {
        var wythoff = new WythoffPoly(polyType.Wythoff);
        wythoff.BuildFaces();
        var poly = new ConwayPoly(wythoff);
        // var o = new OpParams {valueA = .2f, valueB = .2f, facesel = FaceSelections.All};
        // poly = poly.ApplyOp(op, o);
        // var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod);
        // GetComponent<MeshFilter>().mesh = mesh;
        return poly;
    }

    [ContextMenu("FindOpMatches")]
    public void FindOpMatches(ConwayPoly sourcePoly, string sourcePolyName, ConwayPoly targetPoly, string targetPolyName)
    {
        // int v = poly.Vertices.Count;
        // int e = poly.EdgeCount;
        // int f = poly.Faces.Count;
        //
        // // Compare predicted and actual v,e,f
        // Debug.Log($"{v} {e} {f} = {PolyHydraEnums.CalcVef(poly, op2)}");

        // Loops through all Conway ops and records which ones
        // Result in matching Vertex, Edge and Face counts
        
        var matches = new Dictionary<int[], List<Ops>>();
        
        // For all ops use:
        // var opCount = ((Ops[]) Enum.GetValues(typeof(Ops))).Length;
        // Just the conway ops
        var opCount = 34;
        
        var targetFaceTypes = targetPoly.GetFaceCountsByType();
        
        for (var i = 1; i < 34; i++)
        {
            var o = new OpParams {valueA = 0.2f, valueB = 0.2f, facesel = FaceSelections.All};
            var newPoly = sourcePoly.ApplyOp((Ops)i, o);

            var sourceFaceTypes = newPoly.GetFaceCountsByType();
            if (!sourceFaceTypes.SequenceEqual(targetFaceTypes)) continue; 
            if (!matches.ContainsKey(sourceFaceTypes))
            {
                matches[sourceFaceTypes] = new List<Ops>();
            }
            matches[sourceFaceTypes].Add((Ops)i);
        }

        if (matches.Count < 1) return;
        Debug.Log($"Matching {sourcePolyName} and {targetPolyName}: {matches.Count} matches");
        foreach (var match in matches)
        {
            // if (match.Value.Count < 2) continue;
            var lst = string.Join(",", match.Value);
            Debug.Log($"{lst}");
        }
    }
}

