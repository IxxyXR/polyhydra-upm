using System;
using System.Collections.Generic;
using System.Linq;
using Conway;
using Johnson;
using UnityEditor;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FaceLoopsTest : MonoBehaviour
{

    public enum LoopActions
    {
        None,
        Remove,
        Split,
    }
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [Range(1,40)] public int sides = 4;

    public bool ApplyOp;
    public Ops op1;
    public FaceSelections op1Facesel;
    public float op1Amount1 = 0;
    public float op1Amount2 = 0;
    public int StartingFace;
    public int StartingEdge;
    public LoopActions LoopAction;
    public Ops op2;
    public FaceSelections op2Facesel;
    public float op2Amount1 = 0;
    public float op2Amount2 = 0;
    public bool Canonicalize;    
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;

    public PolyHydraEnums.ColorMethods ColorMethod;

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

        if (ApplyOp)
        {
            var o1 = new OpParams {valueA = op1Amount1, valueB = op1Amount2, facesel = op1Facesel};
            poly = poly.ApplyOp(op1, o1);
        }

        // Find and remove the edge loop
        var face = poly.Faces[StartingFace % poly.Faces.Count];
        var edges = face.GetHalfedges();
        var startingEdge = edges[StartingEdge % edges.Count];
        loop = poly.GetFaceLoop(startingEdge);
        if (LoopAction == LoopActions.Remove)
        {
            poly = poly.FaceRemove(false, loop.Select(x=>x.Item1).ToList());
        }
        else if (LoopAction == LoopActions.Split)
        {
            poly = poly.SplitLoop(loop);
        }

        if (ApplyOp)
        {
            var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2, facesel = op2Facesel};
            poly = poly.ApplyOp(op2, o2);
        }
        
        if (Canonicalize)
        {
             poly = poly.Canonicalize(0.1, 0.1);
        }

//        poly = poly.Transform(Position, Rotation, Scale);

        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void OnDrawGizmos()
    {
        #if UNITY_EDITOR
            Gizmos.color = Color.green;
            for (var i = 0; i < loop.Count; i++)
            {
                var f = loop[i];
                var pos = poly.Faces[f.Item1].GetHalfedges()[f.Item2].Midpoint;
                Gizmos.DrawWireSphere(transform.TransformPoint(pos), .03f);
                Handles.Label(pos + new Vector3(0, .15f, 0), i.ToString());
            }

            GizmoHelper.DrawGizmos(poly, transform, edgeGizmos: false, vertexGizmos: false);
        #endif
    }
}
