using System;
using System.Collections.Generic;
using System.Linq;
using Conway;
using Johnson;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FaceLoopsTest : MonoBehaviour
{

    public enum LoopActions
    {
        None,
        Remove,
        Keep,
        SplitFaces,
        Split,
        SetRole
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
        var face = poly.Faces[ConwayPoly.ActualMod(StartingFace, poly.Faces.Count)];
        var edges = face.GetHalfedges();
        var startingEdge = edges[ConwayPoly.ActualMod(StartingEdge, edges.Count)];
        loop = poly.GetFaceLoop(startingEdge);
        var faceIndices = loop.Select(x=>x.Item1).ToList();
        if (LoopAction == LoopActions.Remove)
        {
            poly = poly.FaceRemove(false, faceIndices);
        }
        else if (LoopAction == LoopActions.Keep)
        {
            poly = poly.FaceRemove(true, faceIndices);
        }
        else if (LoopAction == LoopActions.SplitFaces)
        {
            poly = poly.SplitLoop(loop);
        }
        else if (LoopAction == LoopActions.Split)
        {
            ConwayPoly otherPoly;
            
            (otherPoly, poly) = poly.Split(new OpParams(x=>faceIndices.Contains(x.index)));
            poly.FaceRoles = Enumerable.Repeat(ConwayPoly.Roles.Existing, poly.Faces.Count).ToList();
            poly.VertexRoles = Enumerable.Repeat(ConwayPoly.Roles.Existing, poly.Vertices.Count).ToList();
            otherPoly.FaceRoles = Enumerable.Repeat(ConwayPoly.Roles.New, otherPoly.Faces.Count).ToList();
            otherPoly.VertexRoles = Enumerable.Repeat(ConwayPoly.Roles.New, otherPoly.Vertices.Count).ToList();
            
            poly.Append(otherPoly);
        }
        else if (LoopAction == LoopActions.SetRole)
        {
            var faceRoles = new List<ConwayPoly.Roles>();
            for (var faceIndex = 0; faceIndex < poly.Faces.Count; faceIndex++)
            {
                faceRoles.Add(faceIndices.Contains(faceIndex) ? ConwayPoly.Roles.Existing : ConwayPoly.Roles.New);
            }
            poly.FaceRoles = faceRoles;
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

        poly.Transform(Position, Rotation, Scale);

        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void OnDrawGizmos()
    {
        #if UNITY_EDITOR
        // Gizmos.color = Color.green;
        // for (var i = 0; i < loop.Count; i++)
        // {
        //     var f = loop[i];
        //     var face = poly.Faces[f.Item1];
        //     var edges = face.GetHalfedges(); 
        //     var edge = edges[ConwayPoly.ActualMod(f.Item2, edges.Count)]; 
        //     var pos = edge.Midpoint;
        //     pos = transform.TransformPoint(pos);
        //     Gizmos.DrawWireSphere(pos, .03f);
        //     Handles.Label(pos + new Vector3(0, .15f, 0), i.ToString());
        // }
        GizmoHelper.DrawGizmos(poly, transform, vertexGizmos: false, edgeGizmos: false);
        #endif
    }
}
