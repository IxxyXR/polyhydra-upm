using System.Collections;
using System.Collections.Generic;
using Conway;
using Johnson;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PolarPointsTest : MonoBehaviour
{
    [Range(3, 16)]
    public int sides;
    [Range(-2, 2)]
    public float distance;
    [Range(2, 180)]
    public float theta = 10;

    public float cubeScale = .1f;
    
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
        var poly = JohnsonPoly.Antiprism(4);
        var cube = JohnsonPoly.Prism(4);
        var decorations = new ConwayPoly();
        cube.Transform(Vector3.zero, Vector3.zero, Vector3.one * cubeScale);
        foreach (var face in poly.Faces)
        {
            for (float angle = 0; angle < 360; angle += theta)
            {
                var pos = face.GetPolarPoint(angle, distance);
                var look = Quaternion.LookRotation(face.Normal, Vector3.up).eulerAngles + new Vector3(90, 0, 0);
                decorations.Append(cube, pos, look, Vector3.one);
            }
        }
        //poly.Append(decorations);
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(decorations, false);
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
