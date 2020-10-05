using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Conway;
using Johnson;
using UnityEditor;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GableTest : MonoBehaviour
{
    public bool Animate;
    public PolyHydraEnums.ColorMethods ColorMethod;
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;
    
    
    [Range(0f, 10f)] public int Iterations = 3;
    [Range(.01f, 5f)] public float BranchLength;
    [Range(.001f, 1f)] public float Bud1;
    [Range(.001f, 0.5f)] public float Bud2;
    [Range(.001f, .999f)] public float BranchLengthScale;

    [Range(-2f, 2f)] public float amount1;
    [Range(-2f, 2f)] public float direction1;
    [Range(-2f, 2f)] public float amount2;
    [Range(-2f, 2f)] public float direction2;

    public int sides = 4;

    private ConwayPoly poly;
    
    void Start()
    {
        poly = new ConwayPoly();
        Generate();
    }

    void Update()
    {
        if (Animate)
        {
            Generate();
        }
    }

    private void OnValidate()
    {
        Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        poly = JohnsonPoly.Polygon(sides);
        poly = poly.Loft(new OpParams(0.5f));
        var gableSel = FaceSelections.Existing;
        for (int i=0; i<Iterations; i++)
        {
            poly = poly.Gable(new OpParams(Bud1, Bud2, FaceSelections.Existing));
            
            poly.ClearTags();
            var newFaces = poly.GetFaceSelection(FaceSelections.New);
            poly.TagFaces("fork1", filter: x => x.index == newFaces.FirstOrDefault(), introvert: true);
            poly.TagFaces("fork2", filter: x => x.index == newFaces.LastOrDefault(), introvert: true);
            
            poly = poly.Loft(new OpParams(BranchLengthScale, BranchLength, FaceSelections.New));
            // poly = poly.Loft(new OpParams((5f-i)/10f, (BranchLength-i)/BranchLengthScale, FaceSelections.New));
            poly = poly.FaceSlide(new OpParams(amount1, direction1 * i, selectByTags: "fork1"));
            poly = poly.FaceSlide(new OpParams(amount2, direction2 * i, selectByTags: "fork2"));
        }
        
        // poly = poly.Transform(Position, Rotation, Scale);
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    void OnDrawGizmos () {
        
        // int gizmoColor = 0;
        // var faces = poly.Faces;
        // for (int f = 0; f < faces.Count; f++)
        // {
        //     Gizmos.color = Color.white;
        //     var face = faces[f];
        //     var faceVerts = face.GetVertices();
        //     for (int i = 0; i < faceVerts.Count; i++)
        //     {
        //         var edgeStart = faceVerts[i];
        //         var edgeEnd = faceVerts[(i + 1) % faceVerts.Count];
        //         Gizmos.DrawLine(
        //             transform.TransformPoint(edgeStart.Position),
        //             transform.TransformPoint(edgeEnd.Position)
        //         );
        //     }
        //
        //     string label;
        //
        //     label = $"{face.Sides}/{poly.FaceRoles[f]}";
        //     Handles.Label(Vector3.Scale(face.Centroid, transform.lossyScale) + new Vector3(0, .03f, 0), label);
        // }
        
        // GizmoHelper.DrawGizmos(poly, transform, true, false, false);
    }
}