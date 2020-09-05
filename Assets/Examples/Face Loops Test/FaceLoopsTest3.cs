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

        
        // if (ApplyOp)
        // {
        // }

        Face face;
        for (int i = 0; i < splits; i++)
        {
            face = poly.GetFace(new OpParams(FaceSelections.FourSided), 0);
            if (face != null)
            {
                poly = poly.SplitLoop(poly.GetFaceLoop(face.GetHalfedges()[0]));
            }
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

        poly = poly.Transform(Position, Rotation, Scale);
        
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod, true);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
