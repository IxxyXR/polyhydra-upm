using System;
using System.Collections.Generic;
using Conway;
using Johnson;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FaceLoopsTest3 : MonoBehaviour
{
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [Range(3, 32)] public int sides=5;
    [Range(0, 32)] public int splits;
    [Range(0, 16)] public int edgeIndex;
    [Range(0.001f, .999f)] public float splitRatio = 0.5f;
    public PolyHydraEnums.ColorMethods ColorMethod;
    
    public bool ApplyOp;
    public Ops op1;
    public FaceSelections op1Facesel;
    public float op1Amount1 = 0;
    public float op1Amount2 = 0;
    public Ops op2;
    public FaceSelections op2Facesel;
    public float op2Amount1 = 0;
    public float op2Amount2 = 0;
    public bool Canonicalize;
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;

    private ConwayPoly poly;
    private List<Tuple<int, int>> loop;

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
        poly = JohnsonPoly.Build(JohnsonPolyType, sides);

        
        Face face;
        var sidesFilter = poly.FaceselToFaceFilterFunc(FaceSelections.FourSided);
        var roleFilter = poly.FaceselToFaceFilterFunc(FaceSelections.New);
        Func<FilterParams, bool> filterFunc = x => sidesFilter(x);
        for (int i = 0; i < splits; i++)
        {
            face = poly.GetFace(new OpParams(filterFunc), 0);
            if (face != null)
            {
                var edges = face.GetHalfedges();
                poly = poly.SplitLoop(poly.GetFaceLoop(edges[edgeIndex % edges.Count]), splitRatio);
            }
            // Change the filter after the first loop iteration as we can
            // ensure we get the right face based on it's role
            filterFunc = x => sidesFilter(x) && roleFilter(x);
        }
        
        if (ApplyOp)
        {
            var o1 = new OpParams(op1Amount1, op1Amount2, op1Facesel);
            poly = poly.ApplyOp(op1, o1);
            
            var o2 = new OpParams(op2Amount1, op2Amount2, op2Facesel);
            poly = poly.ApplyOp(op2, o2);
        }
        
        if (Canonicalize)
        {
            poly = poly.Canonicalize(0.1, 0.1);
        }

        poly.Transform(Position, Rotation, Scale);
        
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
