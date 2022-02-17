using System.Collections;
using System.Collections.Generic;
using Conway;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ExampleTemplate : MonoBehaviour
{
    public bool Animate;
    public PolyHydraEnums.ColorMethods ColorMethod;
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;

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
        poly.Transform(Position, Rotation, Scale);
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    void OnDrawGizmos () {
        // GizmoHelper.DrawGizmos(poly, transform, true, false, false);
    }
}