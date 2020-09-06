using System.Diagnostics;
using Conway;
using Johnson;
using UnityEditor;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WatermanTest : MonoBehaviour
{
    public int root = 2;
    [Range(0, 6)]
    public int c = 0;

    public bool DoublePrecision;

    public bool ApplyOps;
    public Ops op1;
    public FaceSelections op1Facesel;
    public float op1Amount1 = 0;
    public float op1Amount2 = 0;
    public Ops op2;
    public FaceSelections op2Facesel;
    public float op2Amount1 = 0;
    public float op2Amount2 = 0;

    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;

    public bool Canonicalize;
    public PolyHydraEnums.ColorMethods ColorMethod;

    private ConwayPoly poly;
    
    public Color[] Colors =
    {
        new Color(1.0f, 0.5f, 0.5f),
        new Color(0.8f, 0.85f, 0.9f),
        new Color(0.5f, 0.6f, 0.6f),
        new Color(1.0f, 0.94f, 0.9f),
        new Color(0.66f, 0.2f, 0.2f),
        new Color(0.6f, 0.0f, 0.0f),
        new Color(1.0f, 1.0f, 1.0f),
        new Color(0.6f, 0.6f, 0.6f),
        new Color(0.5f, 1.0f, 0.5f),
        new Color(0.5f, 0.5f, 1.0f),
        new Color(0.5f, 1.0f, 1.0f),
        new Color(1.0f, 0.5f, 1.0f),
    };
    
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
         poly = WatermanPoly.Build(1f, root, c);

        if (ApplyOps)
        {
            var o1 = new OpParams {valueA = op1Amount1, valueB = op1Amount2, facesel = op1Facesel};
            poly = poly.ApplyOp(op1, o1);
            var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2, facesel = op2Facesel};
            poly = poly.ApplyOp(op2, o2);
        }

        if (Canonicalize)
        {
            poly = poly.Canonicalize(0.1, 0.1);
        }

        poly = poly.Transform(Position, Rotation, Scale);
        
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, Colors, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void OnDrawGizmos () {
        // GizmoHelper.DrawGizmos(poly, false, false, true);
	}


}



