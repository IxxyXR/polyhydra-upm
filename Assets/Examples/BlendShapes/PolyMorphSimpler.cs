﻿using System;
using System.Collections.Generic;
using System.Linq;
using Conway;
using Johnson;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;


public class PolyMorphSimpler : MonoBehaviour
{
    [Serializable]
    public struct PolyOpItem
    {
        public Ops op;
        public FaceSelections opFacesel;
        public float opAmount1;
        public float opAmount2;
    }
    
    [Serializable]
    public struct PolyMorphItem
    {
        public Ops op;
        public FaceSelections opFacesel;
        public float opAmount1Start;
        public float opAmount1End;
        public float opAmount2Start;
        public float opAmount2End;
        public float frequency;
    }

    private MeshFilter[] polyList;
    private bool initialized = false;

    
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [Range(3,40)] public int sides = 4;
    public PolyHydraEnums.ColorMethods ColorMethod;
    public PolyOpItem PreMorph;
    [FormerlySerializedAs("PolyMorphItems")] public List<PolyMorphItem> Morphs;
    public PolyOpItem PostMorph;
    
    private SkinnedMeshRenderer polymorphSkinnedMeshRenderer;
    private Material polyMaterial;
    private ConwayPoly poly;
    private Mesh baseMesh;

    [Range(0f, 1f)]
    public float[] blends;

    void Start()
    {
        if (!initialized) Rebuild();
    }

    public Mesh MakeMorphTarget(int morphIndex, float amount)
    {
        Debug.Log($"Making morphIndex {morphIndex}");
        var morphedPoly = poly.Duplicate();
        for (var i = 0; i < Morphs.Count; i++)
        {
            float a = i == morphIndex ? amount : 0;  // Zero amount for ops other than the requested one
            var morphOp = Morphs[i];
            var morphParams = new OpParams(
                Mathf.Lerp(morphOp.opAmount1Start, morphOp.opAmount1End, a),
                Mathf.Lerp(morphOp.opAmount2Start, morphOp.opAmount2End, a),
                morphOp.opFacesel
            );    
            morphedPoly = morphedPoly.ApplyOp(morphOp.op, morphParams);
            morphedPoly = morphedPoly.ApplyOp(PostMorph.op, new OpParams(PostMorph.opAmount1, PostMorph.opAmount2, PostMorph.opFacesel));
        }
        var result = PolyMeshBuilder.BuildMeshFromConwayPoly(morphedPoly, false, null, ColorMethod);
        // result.RecalculateNormals();
        // result.RecalculateTangents();
        return result;
    }

    private Mesh AddBlendShapeFrameFromMesh(string name, Mesh mesh1, Mesh mesh2)
    {
        mesh1.AddBlendShapeFrame(
            name,
            100f,
            CalculateVertexDeltas(mesh1, mesh2),
            // modifiedMesh.vertices.Select((val, index) => val - baseMesh.vertices[index]).ToArray(),
            mesh1.normals.Select((val, index) => val - mesh2.normals[index]).ToArray(),
            mesh1.tangents.Select((val, index) => (Vector3) (val - mesh2.tangents[index])).ToArray()
        );
        // mesh1.RecalculateNormals();
        // mesh1.RecalculateTangents();
        return mesh1;
    }
    
    [Button]
    void Rebuild()
    {
        // Build the initial poly
        polymorphSkinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
        poly = JohnsonPoly.Build(JohnsonPolyType, sides);
        poly.Recenter();
        poly = poly.ApplyOp(PreMorph.op, new OpParams(PreMorph.opAmount1, PreMorph.opAmount2, PreMorph.opFacesel));
        
        // Build the mesh without any morph ops
        var baseMesh = MakeMorphTarget(0, 0);
        baseMesh.ClearBlendShapes();
        baseMesh = AddBlendShapeFrameFromMesh("Base", baseMesh, baseMesh);
        baseMesh.RecalculateNormals();
        baseMesh.RecalculateTangents();
        
        // Build the morphed meshes
        for (var i = 0; i < Morphs.Count; i++)
        {
            var morphedMesh = MakeMorphTarget(i, 1);
            baseMesh = AddBlendShapeFrameFromMesh(i.ToString(), baseMesh, morphedMesh);
            // baseMesh.RecalculateNormals();
            // baseMesh.RecalculateTangents();
        }
        polymorphSkinnedMeshRenderer.sharedMesh = baseMesh;
        initialized = true;
    }

    private Vector3[] CalculateVertexDeltas(Mesh originalMesh, Mesh modifiedMesh)
    {
        var vertexDeltas = new Vector3[originalMesh.vertexCount];
        for (var index = 0; index < originalMesh.vertices.Length; index++)
        {
            vertexDeltas[index] = modifiedMesh.vertices[index] - originalMesh.vertices[index];
        }
        return vertexDeltas;
    }

    void Update()
    {
        if (!initialized) Rebuild();
        for (var i = 0; i < Morphs.Count; i++)
        {
            var item = Morphs[i];
            var x = Time.time * item.frequency;
            //var val = Mathf.SmoothStep(-0.5f, 0.5f, Mathf.Sin(x)) * 2 - 1;
            //var val = Mathf.Pow(Mathf.Abs(Mathf.Cos(x)), 1f / 2f) * Mathf.Sign(Mathf.Cos(x));
            //var val = 1 - Mathf.Pow(25, -1 * Mathf.Sin(x)) / 25f;
            var val = (Mathf.Sin(x) + 1f) / 2f;
            // var val = Mathf.PerlinNoise(x, i * .5f);
            // var val = blends[i];
            var blendFrame = i + 1;  // The first frame is the same as the base mesh
            
            polymorphSkinnedMeshRenderer.SetBlendShapeWeight(blendFrame, Mathf.Lerp(0, 100, val));
        }
        polymorphSkinnedMeshRenderer.sharedMesh.RecalculateNormals();
        polymorphSkinnedMeshRenderer.sharedMesh.RecalculateTangents();        }
}
