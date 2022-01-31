using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Conway;
using Grids;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class MultigridsTest : MonoBehaviour
{

    public enum ColorFunctions
    {
        Unchanged,
        Mod,
        ActualMod,
        Normalized,
        Abs
    }

    [Range(1, 30)] public int Divisions = 5;
    [Range(3, 30)] public int Dimensions = 5;
    public float Offset = .2f;
    public bool randomize;

    public float MinDistance = 0f;
    public float MaxDistance = 1f;

    public float colorRatio = 1.0f;
    public float colorIndex;
    public float colorIntersect;
    public Gradient ColorGradient;
    public bool SharedVertices;

    public bool ApplyOp;
    public Ops op1;
    public FaceSelections op1Facesel;
    public float op1Amount1 = 0;
    public float op1Amount2 = 0;
    public Ops op2;
    public FaceSelections op2Facesel;
    public float op2Amount1 = 0;
    public float op2Amount2 = 0;
    public Ops op3;
    public FaceSelections op3Facesel;
    public float op3Amount1 = 0;
    public float op3Amount2 = 0;

    public PolyHydraEnums.ColorMethods ColorMethod;
    public ColorFunctions ColorFunction;
    public ConwayPoly.TagType TagType;

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
        (poly, shapes, colors) = multigrid.Build(SharedVertices, randomize);
        if (shapes.Count == 0) return;
        if (ColorMethod == PolyHydraEnums.ColorMethods.ByTags)
        {
            float colorMin = colors.Min();
            float colorMax = colors.Max();

            for (var faceIndex = 0; faceIndex < poly.Faces.Count; faceIndex++)
            {
                var face = poly.Faces[faceIndex];
                var colorIndex = colors[faceIndex];
                float colorValue;
                switch (ColorFunction)
                {
                    case ColorFunctions.Mod:
                        colorValue = colorIndex % 1;
                        break;
                    case ColorFunctions.ActualMod:
                        colorValue = PolyUtils.ActualMod(colorIndex, 1);
                        break;
                    case ColorFunctions.Normalized:
                        colorValue = Mathf.InverseLerp(colorMin, colorMax, colorIndex);
                        break;
                    case ColorFunctions.Abs:
                        colorValue = Mathf.Abs(colorIndex);
                        break;
                    default:
                        colorValue = colorIndex;
                        break;
                }
                string colorString = ColorUtility.ToHtmlStringRGB(ColorGradient.Evaluate(colorValue));
                var tag = new Tuple<string, ConwayPoly.TagType>(
                    $"#{colorString}", 
                    TagType);
                poly.FaceTags[faceIndex].Add(tag);
            }
        }

        if (ApplyOp)
        {
            var o1 = new OpParams {valueA = op1Amount1, valueB = op1Amount2, facesel = op1Facesel};
            poly = poly.ApplyOp(op1, o1);
            var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2, facesel = op2Facesel};
            poly = poly.ApplyOp(op2, o2);
            var o3 = new OpParams {valueA = op3Amount1, valueB = op3Amount2, facesel = op3Facesel};
            poly = poly.ApplyOp(op3, o3);
        }

        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void OnDrawGizmos ()
    {
#if UNITY_EDITOR
        GizmoHelper.DrawGizmos(poly, transform, vertexGizmos, faceGizmos, edgeGizmos, faceCenterGizmos, false, 0.3f);
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
#endif
    }

}
