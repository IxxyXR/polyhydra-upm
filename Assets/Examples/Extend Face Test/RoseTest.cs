using System.Collections.Generic;
using System.Linq;
using Conway;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoseTest : ExampleBase
{
    public int a = 1;
    public int b = 1;
    public int c = 1;
    public int d = 72;
    
    [Range(3, 32)] public int seed = 8;
    [Range(3, 32)] public int tooth = 3;
    [Range(0, 32)] public int cycles = 1;
    public float amount = .5f;
    public override void Generate()
    {
        poly = ConwayPoly._MakePolygon(seed);
        for (int edgeIndex = 0; edgeIndex < seed; edgeIndex++)
        {
            poly.ExtendFace(0, edgeIndex, tooth);
        }
        
        int j = 2;
        for (int i = 1; i <= seed; i++)
        {
            if (i==seed) j = 1;
            poly.AddKite(i, tooth-1, j , 1);
            j++;
        }
        
        poly = poly.Weld(0.1f);
        
        var angle1 = poly.Faces[0].Halfedge.Angle * 2; // 72
        var angle2 = 180 - angle1; // 108
        
        int faceCount = seed + 1;
        for (int i = 1; i < faceCount; i += 1)
        {
            var face = poly.Faces[i];
            poly.AddRhombus(face, 3, angle2);
            if (tooth%2==0) poly.AddRhombus(face, 2, 180 - poly.Faces.Last().Halfedge.Angle);
        }
        
        poly = poly.Weld(0.1f);

        // for (int i = seed * 1 + 1; i < seed * 3 + 1; i += a)
        // {
        //     var face = poly.Faces[i];
        //     poly.AddRhombus(face, b, c);
        // }
        
        poly = poly.Weld(0.1f);
        
        for (int i = 0; i < cycles; i++)
        {
            poly = poly.ExtendBoundaries(new OpParams(amount));
            poly = poly.Weld(0.1f);
        }
        
        base.Generate();
    }
}