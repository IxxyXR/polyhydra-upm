using System;
using System.Collections.Generic;
using System.Linq;
using Conway;
using NaughtyAttributes;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ExampleBase : MonoBehaviour
{
    [BoxGroup("Op settings")] public bool ApplyOp;
    
    [BoxGroup("Op 1")] public Ops op1;
    [BoxGroup("Op 1")] public FaceSelections op1Facesel;
    [BoxGroup("Op 1")] public float op1Amount1 = 0;
    [BoxGroup("Op 1")] public float op1Amount2 = 0;
    [BoxGroup("Op 1")] [Range(1, 5)] public int op1iterations = 1;
    [BoxGroup("Op 1")] public bool op1Animate;
    
    [BoxGroup("Op 2")] public Ops op2;
    [BoxGroup("Op 2")] public FaceSelections op2Facesel;
    [BoxGroup("Op 2")] public float op2Amount1 = 0;
    [BoxGroup("Op 2")] public float op2Amount2 = 0;
    [BoxGroup("Op 2")] public bool op2Animate;
    [BoxGroup("Op 2")] [Range(1, 5)] public int op2iterations = 1;
    
    [BoxGroup("Op 2")] public Ops op3;
    [BoxGroup("Op 2")] public FaceSelections op3Facesel;
    [BoxGroup("Op 2")] public float op3Amount1 = 0;
    [BoxGroup("Op 2")] public float op3Amount2 = 0;
    [BoxGroup("Op 2")] public bool op3Animate;
    [BoxGroup("Op 2")] [Range(1, 5)] public int op3iterations = 1;
    
    [BoxGroup("Transform")] public Vector3 Position = Vector3.zero;
    [BoxGroup("Transform")] public Vector3 Rotation = Vector3.zero;
    [BoxGroup("Transform")] public Vector3 Scale = Vector3.one;
   
    [BoxGroup("Tweaks")] public bool Canonicalize;
    [BoxGroup("Tweaks")] public bool Rescale;
    [BoxGroup("Tweaks")] public PolyHydraEnums.UVMethods UVMethod;
    
    [BoxGroup("Coloring")] public PolyHydraEnums.ColorMethods ColorMethod;
    [BoxGroup("Coloring")] public bool UseCustomColors;
    [BoxGroup("Coloring")] public Gradient Colors;
    [BoxGroup("Coloring")] public float ColorRange;
    [BoxGroup("Coloring")] public float ColorOffset;

    [BoxGroup("Gizmos")] public bool vertexGizmos;
    [BoxGroup("Gizmos")] public bool faceGizmos;
    [BoxGroup("Gizmos")] public bool edgeGizmos;
    [BoxGroup("Gizmos")] public bool faceCenterGizmos;
    [BoxGroup("Gizmos")] public bool FaceOrientationGizmos;

    protected ConwayPoly poly;

    protected void Start()
    {
        Generate();
    }

    void Update()
    {
        if (op1Animate || op2Animate)
        {
            Generate();
        }
    }

    private void OnValidate()
    {
        Generate();
    }

    [ContextMenu("Generate")]
    public virtual void Generate()
    {
        if (ApplyOp)
        {
            for (var i = 0; i < op1iterations; i++)
            {
                var o1 = new OpParams {valueA = op1Amount1 * Mathf.Abs(op1Animate ? Mathf.Sin(Time.time) : 1), valueB = op1Amount2, facesel = op1Facesel};
                poly = poly.ApplyOp(op1, o1);
            }
            for (var i = 0; i < op2iterations; i++)
            {
                var o2 = new OpParams {valueA = op2Amount1 * Mathf.Abs(op2Animate ? Mathf.Cos(Time.time * .6f) : 1), valueB = op2Amount2, facesel = op2Facesel};
                poly = poly.ApplyOp(op2, o2);
            }
            for (var i = 0; i < op3iterations; i++)
            {
                var o3 = new OpParams {valueA = op3Amount1 * Mathf.Abs(op3Animate ? Mathf.Cos(Time.time * .6f) : 1), valueB = op3Amount2, facesel = op3Facesel};
                poly = poly.ApplyOp(op3, o3);
            }
        }

        AfterAllOps();
        
        Color[] colors = null;
        if (UseCustomColors)
        {
            colors = Enumerable.Range(0,8).Select(x => Colors.Evaluate(((x / 8f) * ColorRange + ColorOffset) % 1)).ToArray();
        }
        
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, colors, ColorMethod, UVMethod);
        if (Rescale)
        {
            var size = mesh.bounds.size;
            var maxDimension = Mathf.Max(size.x, size.y, size.z);
            var scale = (1f / maxDimension) * 2f;
            if (scale > 0 && scale != Mathf.Infinity)
            {
                transform.localScale = new Vector3(scale, scale, scale);
            }
            else
            {
                Debug.LogError("Failed to rescale");
            }
        }

        GetComponent<MeshFilter>().mesh = mesh;
    }

    public virtual void AfterAllOps()
    {
        if (Canonicalize)
        {
            poly = poly.Canonicalize(0.01, 0.01);
        }

        if (Position != Vector3.zero || Rotation != Vector3.zero || Scale != Vector3.one)
        {
            poly = poly.Transform(Position, Rotation, Scale);
        }
    }

    public virtual void DoDrawGizmos()
    {
        if (poly == null) return;
        #if UNITY_EDITOR
            GizmoHelper.DrawGizmos(poly, transform, vertexGizmos, faceGizmos, edgeGizmos, faceCenterGizmos, FaceOrientationGizmos, 0.3f);
        #endif
    }

    private void OnDrawGizmos()
    {
        DoDrawGizmos();
    }
}
    
