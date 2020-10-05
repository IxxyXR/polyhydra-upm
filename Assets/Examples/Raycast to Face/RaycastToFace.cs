using System;
using System.Collections.Generic;
using Conway;
using Johnson;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using Wythoff;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class RaycastToFace : MonoBehaviour
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

    public ShapeTypes ShapeType;
    
    [ShowIf("ShapeIsWythoff")]
    public PolyTypes PolyType;
    [ShowIf("ShapeIsJohnson")]
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [ShowIf("ShapeIsGrid")]
    public PolyHydraEnums.GridTypes GridType;
    [ShowIf("ShapeIsGrid")]
    public PolyHydraEnums.GridShapes GridShape;

    [Range(1,40)] public int P = 6;
    [ShowIf("ShapeIsGrid")] [Range(1,40)] public int Q = 6;

    [BoxGroup("Op 1"), Label("")] public Ops op1;
    [BoxGroup("Op 1"), Label("Faces")] public FaceSelections op1Facesel;
    [BoxGroup("Op 1"), Range(0, .5f), Label("Amount")] public float op1Amount1;
    [BoxGroup("Op 1"), Range(0, .5f), Label("Amount2")] public float op1Amount2;
    
    [BoxGroup("Op 2"), Label("")] public Ops op2;
    [BoxGroup("Op 2"), Range(0, .5f), Label("Amount")] public float op2Amount1;
    [BoxGroup("Op 2"), Range(0, .5f), Label("Amount2")] public float op2Amount2;
    
    [Space]
    public PolyHydraEnums.ColorMethods ColorMethod;
    public bool HighlightSelection;
    public bool AttemptToFillHoles;

    private ConwayPoly polyBeforeOp;
    private ConwayPoly polyAfterOp;
    private List<Vector4> miscUVs2;
    [ReadOnly] public List<int> SelectedFaces;
    private Dictionary<string, string> prevValues;


    void Start()
    {
        Generate();
        SelectedFaces = new List<int>();
    }

    private void OnValidate()
    {
        if (prevValues == null)
        {
            prevValues = ComponentRecorder.RecordComponent(this);
        }
        else
        {
            if (
                prevValues["PolyType"] != PolyType.ToString()
                || prevValues["ShapeType"] != ShapeType.ToString()
                || prevValues["JohnsonPolyType"] != JohnsonPolyType.ToString() 
                || prevValues["GridType"] != GridType.ToString() 
                || prevValues["GridShape"] != GridShape.ToString() 
                || prevValues["op1"] != op1.ToString()
                )
            {
                // Topology has changed so clear the face selection
                SelectedFaces.Clear();
                prevValues = ComponentRecorder.RecordComponent(this);
            }
        }
        Generate();
    }

    void Update()
    {
        Vector3 p0, p1, p2;
        Transform hitTransform;

        if (HighlightSelection)
        {
            foreach (var faceIndex in SelectedFaces)
            {
                foreach (var edge in polyBeforeOp.Faces[faceIndex].GetHalfedges())
                {
                    p0 = edge.Vertex.Position;
                    p1 = edge.Next.Vertex.Position;
                    hitTransform = gameObject.transform;
                    p0 = hitTransform.TransformPoint(p0);
                    p1 = hitTransform.TransformPoint(p1);
                    Debug.DrawLine(p0, p1);
                }
            }
        }
        
        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            return;
        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null)
            return;

        hitTransform = hit.collider.transform;
        int hitFaceIndex = Mathf.FloorToInt(miscUVs2[hit.triangleIndex * 3][3]);
        
        foreach (var edge in polyBeforeOp.Faces[hitFaceIndex].GetHalfedges())
        {
            p0 = edge.Vertex.Position;
            p1 = edge.Next.Vertex.Position;
            hitTransform = hitTransform.transform;
            p0 = hitTransform.TransformPoint(p0);
            p1 = hitTransform.TransformPoint(p1);
            Debug.DrawLine(p0, p1, Color.cyan);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (SelectedFaces.Contains(hitFaceIndex))
            {
                SelectedFaces.Remove(hitFaceIndex);
            }
            else
            {
                SelectedFaces.Add(hitFaceIndex);
            }
            Generate();
        }

    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        switch (ShapeType)
        {
            case ShapeTypes.Wythoff:
                var wythoff = new WythoffPoly(PolyType, P, Q);
                wythoff.BuildFaces();
                polyBeforeOp = new ConwayPoly(wythoff);
                break;
            case ShapeTypes.Johnson:
                polyBeforeOp = JohnsonPoly.Build(JohnsonPolyType, P);
                break;
            case ShapeTypes.Grid:
                polyBeforeOp = Grids.Grids.MakeGrid(GridType, GridShape, P, Q);
                break;
        }

        var o1 = new OpParams {valueA = op1Amount1, valueB = op1Amount2, facesel = op1Facesel};
        polyBeforeOp = polyBeforeOp.ApplyOp(op1, o1);

        // Collision Mesh
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(polyBeforeOp, false, null, ColorMethod);
        GetComponent<MeshCollider>().sharedMesh = mesh;
        miscUVs2 = new List<Vector4>();
        mesh.GetUVs(4, miscUVs2);

        if (PolyHydraEnums.OpConfigs[op2].usesFaces)
        {
            var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2, filterFunc = x=>SelectedFaces.Contains(x.index)};
            polyAfterOp = polyBeforeOp.ApplyOp(op2, o2);
        }
        else
        {
            var (excluded, included) = polyBeforeOp.Split(new OpParams { filterFunc = x=>SelectedFaces.Contains(x.index) });
            var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2};
            polyAfterOp = included.ApplyOp(op2, o2);
            polyAfterOp.Append(excluded);
            if (AttemptToFillHoles)
            {
                polyAfterOp = polyAfterOp.Weld(0.1f);
                polyAfterOp = polyAfterOp.FillHoles();
            }
        }


        // Final Mesh
        mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(polyAfterOp, false, null, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}

