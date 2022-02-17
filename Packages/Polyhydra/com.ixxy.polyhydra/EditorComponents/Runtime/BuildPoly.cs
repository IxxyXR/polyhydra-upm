using System;
using System.Collections.Generic;
using System.Linq;
using Conway;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
class BuildPoly : MonoBehaviour
{
    [BoxGroup("Coloring")] public PolyHydraEnums.ColorMethods ColorMethod = PolyHydraEnums.ColorMethods.ByRole;
    [BoxGroup("Coloring")] public bool UseCustomColors;
    [BoxGroup("Coloring")] public Gradient CustomColors;
    [BoxGroup("Coloring")] public float CustomColorRange = 1f;
    [BoxGroup("Coloring")] public float CustomColorOffset;

    [BoxGroup("Animation")] public bool EnableAnimation;
    [BoxGroup("Animation")] public float AnimationAmount = 1.0f;
    [BoxGroup("Animation")] public float AnimationSpeed = 1.0f;

    [BoxGroup("Transform")] public Vector3 Position = Vector3.zero;
    [BoxGroup("Transform")] public Vector3 Rotation = Vector3.zero;
    [BoxGroup("Transform")] public Vector3 Scale = Vector3.one;
   
    [BoxGroup("Tweaks")] public bool Canonicalize;
    [BoxGroup("Tweaks")] public bool Rescale;
    [BoxGroup("Tweaks")] public PolyHydraEnums.UVMethods UVMethod;

    [BoxGroup("Gizmos")] public bool vertexGizmos;
    [BoxGroup("Gizmos")] public bool faceGizmos;
    [BoxGroup("Gizmos")] public bool edgeGizmos;
    [BoxGroup("Gizmos")] public bool faceCenterGizmos;
    [BoxGroup("Gizmos")] public bool FaceOrientationGizmos;

    public bool BuildAll;
    
    [NonSerialized] public bool NeedsGenerate;
    [NonSerialized] public Stack<ConwayPoly> polyStack;
    [NonSerialized] public ConwayPoly finalPoly;
    
#if UNITY_EDITOR
    private int _ComponentCount;
#endif

    private void OnValidate()
    {
        NeedsGenerate = true;
    }

    void Update()
    {
        
#if UNITY_EDITOR
        // Catches removing components which otherwise wouldn't trigger a refresh.
        int currentComponentCount = GetComponents<BaseComponent>().Length;
        if (currentComponentCount != _ComponentCount) NeedsGenerate = true;
        _ComponentCount = currentComponentCount;
#endif
          
        if (NeedsGenerate)
        {
            NeedsGenerate = false;
            Generate();
        }
    }

    protected virtual void Generate()
    {
        polyStack = new Stack<ConwayPoly>();
        float animMultiplier = Mathf.Abs(Mathf.Sin(Time.time * AnimationSpeed) * AnimationAmount);
        foreach (var component in GetComponents<BaseComponent>())
        {
            if (!component.enabled) continue;
            if (component is BaseGenerator generator)
            {
                polyStack.Push(generator.Generate());
            }
            else if (component is BaseStackModifier stackmod)
            {
                polyStack = stackmod.Modify(polyStack);
            }
            else if (component is BaseModifier modifier)
            {
                NeedsGenerate = modifier.Animate && EnableAnimation;
                modifier.animMultiplier = animMultiplier;
                if (polyStack.Count > 0) polyStack.Push(modifier.Modify(polyStack.Pop()));
            }
        }

        if (polyStack.Count == 0)
        {
            GetComponent<MeshFilter>().mesh = null;
            return;
        }
        
        if (BuildAll)
        {
            finalPoly = new ConwayPoly();
            do
            {finalPoly.Append(polyStack.Pop());} while (polyStack.Count > 0);
        }
        else
        {
            finalPoly = polyStack.Pop();
        }

        AfterAll();
        GenerateMesh();
    }

    private void GenerateMesh()
    {
        Color[] colors = null;

        if (UseCustomColors)
        {
            colors = Enumerable.Range(0, 8)
                .Select(x => CustomColors.Evaluate(((x / 8f) * CustomColorRange + CustomColorOffset) % 1))
                .ToArray();
        }
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(finalPoly, false, colors, ColorMethod, UVMethod);
        if (Rescale)
        {
            var size = mesh.bounds.size;
            var maxDimension = Mathf.Max(size.x, size.y, size.z);
            var scale = (1f / maxDimension) * 2f;
            if (scale > 0 && !float.IsPositiveInfinity(scale))
            {
                transform.localScale = new Vector3(scale, scale, scale);
            }
        }
        else
        {
            transform.localScale = Vector3.one;
        }
        GetComponent<MeshFilter>().mesh = mesh;

    }

    protected virtual void AfterAll()
    {
        if (Position != Vector3.zero || Rotation != Vector3.zero || Scale != Vector3.one)
        {
            finalPoly.Transform(Position, Rotation, Scale);
        }
    }
    private void OnDrawGizmos()
    {
        DoDrawGizmos();
    }

    protected virtual void DoDrawGizmos()
    {
        if (finalPoly == null) return;
#if UNITY_EDITOR
        GizmoHelper.DrawGizmos(finalPoly, transform, vertexGizmos, faceGizmos, edgeGizmos, faceCenterGizmos,
            FaceOrientationGizmos, 0.3f);
#endif
    }
}