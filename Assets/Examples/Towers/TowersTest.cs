using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Conway;
using Johnson;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TowersTest : MonoBehaviour
{
    public float height = 5;
    public int divisions = 1;
    public bool Animate;
    public PolyHydraEnums.ColorMethods ColorMethod;
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;

    private ConwayPoly poly;
    
    void Start()
    {
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
        poly = JohnsonPoly.Polygon(4);
        poly = poly.Loft(new OpParams(0, height));
        var loop = poly.GetFaceLoop(poly.Halfedges.First());
        poly = poly.MultiSplitLoop(loop, divisions);


        
        // poly = poly.Transform(Position, Rotation, Scale);
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    void OnDrawGizmos () {
        GizmoHelper.DrawGizmos(poly, transform, true, false, false);
    }

}
