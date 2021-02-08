using System;
using System.Collections.Generic;
using System.Linq;
using Conway;
using Johnson;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Wythoff;
using Face = UnityEngine.ProBuilder.Face;
using PBFace = UnityEngine.ProBuilder.Face;
using Random = UnityEngine.Random;

// Fairly crappy attempt to use Probuilder to create a mesh from a poly.
// Advantage being - it can triangulate concave faces more correctly.
// Disadvantage is that the Probuilder API is a cruel joke invented by a sociopathic programmer (joke!)

[ExecuteInEditMode]
public class ProbuilderTest : MonoBehaviour
{
    public enum ShapeTypes
    {
        Johnson,
        Grid,
        Wythoff
    }

    // Only used to hide inspector fields with "ShowIf"
    private bool ShapeIsWythoff() => ShapeType == ShapeTypes.Wythoff;
    private bool ShapeIsJohnson() => ShapeType == ShapeTypes.Johnson;
    private bool ShapeIsGrid() => ShapeType == ShapeTypes.Grid;

    private ProBuilderMesh pbmesh;

    public ShapeTypes ShapeType;
    [ShowIf("ShapeIsWythoff")]
    public PolyTypes PolyType;
    [ShowIf("ShapeIsJohnson")]
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [ShowIf("ShapeIsGrid")]
    public PolyHydraEnums.GridTypes GridType;
    [ShowIf("ShapeIsGrid")]
    public PolyHydraEnums.GridShapes GridShape;
    
    [Range(1,40)] public int PrismP = 4;
    [ShowIf("ShapeIsGrid")] [Range(1,40)] public int PrismQ = 4;

    public bool ApplyOp;
    
    [BoxGroup("Op 1")] public bool PreCanonicalize;
    [BoxGroup("Op 1"), Label("")] public Ops op1;
    [BoxGroup("Op 1"), Label("Faces")] public FaceSelections op1Facesel;
    [BoxGroup("Op 1"), Range(-3f, 3f), Label("Amount")] public float op1Amount1;
    [BoxGroup("Op 1"), Range(-3f, 3f), Label("Amount2")] public float op1Amount2;
    
    [Space]

    [BoxGroup("Op 2")] public bool Canonicalize;
    [BoxGroup("Op 2"), Label("")] public Ops op2;
    [BoxGroup("Op 2"), Label("Faces")] public FaceSelections op2Facesel;
    [BoxGroup("Op 2"), Range(-3f, 3f), Label("Amount")] public float op2Amount1;
    [BoxGroup("Op 2"), Range(-3f, 3f), Label("Amount2")] public float op2Amount2;
    
    [Space]

    [BoxGroup("Op 3"), Label("")] public Ops op3;
    [BoxGroup("Op 3"), Label("Faces")] public FaceSelections op3Facesel;
    [BoxGroup("Op 3"), Range(-3f, 3f), Label("Amount")] public float op3Amount1;
    [BoxGroup("Op 3"), Range(-3f, 3f), Label("Amount2")] public float op3Amount2;
    
    [Space]
    public Vector3 Position = Vector3.zero;
    public Vector3 Rotation = Vector3.zero;
    public Vector3 Scale = Vector3.one;
    public PolyHydraEnums.ColorMethods ColorMethod;
    public Gradient Colors;
    public float ColorRange;
    public float ColorOffset;
    public float JitterAmount;

    [Space]
    public Material material;
    
    private ConwayPoly poly;
    
    void Start()
    {
        Generate();
    }

    private void OnValidate()
    {
        Generate();
    }
    
    private void Update()
    {
        // Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        switch (ShapeType)
        {
            case ShapeTypes.Wythoff:
                var wythoff = new WythoffPoly(PolyType, PrismP, PrismQ);
                wythoff.BuildFaces();
                poly = new ConwayPoly(wythoff);
                break;
            case ShapeTypes.Johnson:
                poly = JohnsonPoly.Build(JohnsonPolyType, PrismP);
                break;
            case ShapeTypes.Grid:
                poly = Grids.Grids.MakeGrid(GridType, GridShape, PrismP, PrismQ);
                break;
        }

        if (ApplyOp)
        {
            var o1 = new OpParams {valueA = op1Amount1, valueB = op1Amount2, facesel = op1Facesel};
            poly = poly.ApplyOp(op1, o1);
        }

        if (PreCanonicalize)
        {
            poly = poly.Canonicalize(0.01, 0.01);
        }
        
        if (Canonicalize)
        {
            poly = poly.Canonicalize(0.01, 0.01);
        }

        if (ApplyOp)
        {
            var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2, facesel = op2Facesel};
            poly = poly.ApplyOp(op2, o2);
            var o3 = new OpParams {valueA = op3Amount1, valueB = op3Amount2, facesel = op3Facesel};
            poly = poly.ApplyOp(op3, o3);
        }

        var points = poly.ListVerticesByPoints().ToList();
        if (JitterAmount > 0)
        {
            for (int i=0; i<points.Count(); i++)
            {
                var point = points[i];
                points[i] = new Vector3(
                    point.x + Random.value * JitterAmount,
                    point.y + Random.value * JitterAmount,
                    point.z + Random.value * JitterAmount
                );
            }
        }
        var faceIndices = poly.ListFacesByVertexIndices();
        
        // This whole mess is because I can't find a way to regenerate
        // a probuilder mesh and therefore have to continually create/destroy gameobjects
        // (which is a mess in edit mode)
        // If anyone can explain how to simply take an existing probuilder object clear it 
        // and pass in a list of Vector3's and lists of ordered indexes for faces
        // then please do.
        
        if (pbmesh != null && pbmesh.gameObject != null)
        {
            if (Application.isPlaying)
            {
                Destroy(pbmesh.gameObject);
            }
            else
            {
                var go = pbmesh.gameObject;
                UnityEditor.EditorApplication.delayCall+=()=>
                {
                    DestroyImmediate(go);
                };
            }
        }
        var colors = Enumerable.Range(0,8).Select(x => Colors.Evaluate(((x / 8f) * ColorRange + ColorOffset) % 1)).ToArray();
        pbmesh = ProBuilderMesh.Create(points, new List<Face>());
        var faces = new List<PBFace>();
        for (var i = 0; i < faceIndices.Length; i++)
        {
            var face = faceIndices[i];
            var result = AppendElements.CreatePolygon(pbmesh, face, false);
            if (result != null)
            {
                pbmesh.SetFaceColor(result, colors[(int)poly.FaceRoles[i]]);
                faces.Add(result);
            }
        }

        if (faces.Count < 1 || pbmesh==null)
        {
            return;
        }

        pbmesh.SetMaterial(faces, material);
        pbmesh.ToMesh();
        pbmesh.Refresh();
    }
    
    void OnDrawGizmos () {
        // GizmoHelper.DrawGizmos(poly, transform, true, false, false, false);
    }


}
