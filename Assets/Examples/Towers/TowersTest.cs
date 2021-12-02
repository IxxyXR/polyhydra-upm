using System;
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
    public float ratio = 0.5f;
    public int divisions = 1;
    public int edgeIndex = 0;
    public bool Animate;
    public PolyHydraEnums.ColorMethods ColorMethod;
    // public Vector3 Position = Vector3.zero;
    // public Vector3 Rotation = Vector3.zero;
    // public Vector3 Scale = Vector3.one;

    private ConwayPoly initialPoly;
    
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
        // WIP Can't remember what the aim was here!
        
        initialPoly = JohnsonPoly.BuildOther(PolyHydraEnums.OtherPolyTypes.GriddedCube, divisions, divisions);
        initialPoly = initialPoly.Stretch(height);
        
        var poly = initialPoly.SliceFaceLoop(edgeIndex);

        // poly = poly.Loft(new OpParams(0, 2f, FaceSelections.Existing));
        // poly.Append(initialPoly);
        // List<Tuple<int, int>> loop = poly.GetFaceLoop(edgeIndex);
        // poly = poly.MultiSplitLoop(loop, ratio, divisions);
        
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    void OnDrawGizmos () {
        GizmoHelper.DrawGizmos(initialPoly, transform, true, false, false);
    }

}
