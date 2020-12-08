using System;
using System.Collections;
using System.Collections.Generic;
using Conway;
using Johnson;
using UnityEditor;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FaceSplitTest : MonoBehaviour
{
    public bool Animate;
    public PolyHydraEnums.ColorMethods ColorMethod;
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;

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
        
        for (int i=0; i<3; i++)
        {
            poly = poly.SplitFaces(new OpParams(FaceSelections.Existing));
        
            poly.ClearTags();
            poly.TagFaces("split1", FaceSelections.New, introvert: true);
            poly.TagFaces("split2", FaceSelections.New, introvert: true);
        
            poly = poly.Loft(new OpParams(0.01f, 1f, FaceSelections.AllNew));
            poly = poly.FaceSlide(new OpParams(amount1, direction1 * i, selectByTags: "split1"));
            poly = poly.FaceSlide(new OpParams(amount2, direction2 * i, selectByTags: "split2"));
        }
        
        poly = poly.Transform(Position, Rotation, Scale);
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
        //     label = $"{f}:{poly.FaceRoles[f]}";
        //     Handles.Label(Vector3.Scale(face.Centroid, transform.lossyScale) + new Vector3(0, .03f, 0), label);
        // }
        
        // GizmoHelper.DrawGizmos(poly, transform, true, false, false);
    }
}