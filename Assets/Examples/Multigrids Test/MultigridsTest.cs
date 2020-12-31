using System;
using System.Collections;
using System.Collections.Generic;
using Conway;
using Grids;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class MultigridsTest : MonoBehaviour
{

    [Range(1, 30)] public int Divisions = 5;
    [Range(3, 30)] public int Dimensions = 5;
    public float Offset = .2f;

    public float MinDistance = 0f;
    public float MaxDistance = 1f;

    public float colorRatio = 1.0f;
    public float colorIndex;
    public float colorIntersect;
    public Gradient ColorGradient;
    public bool SharedVertices;
    public bool Weld;

    public bool ApplyOp;
    public Ops op1;
    public FaceSelections op1Facesel;
    public float op1Amount1 = 0;
    public float op1Amount2 = 0;
    public Ops op2;
    public FaceSelections op2Facesel;
    public float op2Amount1 = 0;
    public float op2Amount2 = 0;

    public PolyHydraEnums.ColorMethods ColorMethod;

    public bool vertexGizmos;
    public bool faceGizmos;
    public bool edgeGizmos;
    public bool faceCenterGizmos;
    public bool shapeGizmos;

    private ConwayPoly poly;
    private List<List<Vector2>> shapes;
    private List<float> colors;

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
        var multigrid = new MultiGrid(Divisions, Dimensions, Offset, MinDistance, MaxDistance, colorRatio, colorIndex, colorIntersect);
        (poly, shapes, colors) = multigrid.Build(SharedVertices);
        if (ColorMethod == PolyHydraEnums.ColorMethods.ByTags)
        {
            for (var faceIndex = 0; faceIndex < poly.Faces.Count; faceIndex++)
            {
                var face = poly.Faces[faceIndex];
                var colorIndex = colors[faceIndex];
                string colorString = ColorUtility.ToHtmlStringRGB(ColorGradient.Evaluate(colorIndex));
                var tag = new Tuple<string, ConwayPoly.TagType>(
                    $"#{colorString}", 
                    ConwayPoly.TagType.Extrovert);
                poly.FaceTags[faceIndex].Add(tag);
            }
        }

        // OpParams o = new OpParams(x=>
        // {
        //     float distance = x.poly.Faces[x.index].Centroid.magnitude;
        //     return distance > MaxDistance || distance < MinDistance;
        // });
        // poly = poly.FaceRemove(o);

        if (Weld)
        {
            poly = poly.Weld(.001f);
        }
        
        if (ApplyOp)
        {
            var o1 = new OpParams {valueA = op1Amount1, valueB = op1Amount2, facesel = op1Facesel};
            poly = poly.ApplyOp(op1, o1);
            var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2, facesel = op2Facesel};
            poly = poly.ApplyOp(op2, o2);
        }

        
        // Final Mesh
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    void OnDrawGizmos () {
        GizmoHelper.DrawGizmos(poly, transform, vertexGizmos, faceGizmos, edgeGizmos, faceCenterGizmos, 0.3f);
        if (shapeGizmos)
        {
            for (int i = 0; i < shapes.Count; i++)
            {
                // if (i != ShapeGizmoIndex) continue;
                var shape = shapes[i];
                for (int j = 0; j < shape.Count; j++)
                {
                    // if (j != EdgeGizmoIndex) continue;
                    Gizmos.color = Color.yellow;
                    Vector3 vv1 = transform.TransformPoint(shape[j]);
                    var vv2 = transform.TransformPoint(shape[(j + 1) % shape.Count]);
                    var v1 = new Vector3(vv1.x, 0, vv1.y);
                    var v2 = new Vector3(vv2.x, 0, vv2.y);
                    var center = v1 + .2f * ((v2 - v1)/2f);
                    Gizmos.DrawLine(v1, v2);
                    Gizmos.DrawWireSphere(center, .01f);
                }
            }
        }
    }

}
