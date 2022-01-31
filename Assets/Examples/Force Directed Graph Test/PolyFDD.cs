using System;
using System.Collections.Generic;
using System.Linq;
using Conway;
using Forces;
using Grids;
using Johnson;
using NaughtyAttributes;
using UnityEngine;
using Wythoff;


public class PolyFDD : ExampleBase
{
    
    private List<Node> Nodes;
    private List<Edge> Edges;
    private List<Force> Forces;
    private static float alpha;
    private float AlphaMin;
    private float AlphaDecay;
    private float AlphaTarget;
    private float VelocityDecay;
    private bool Built = false;
    public bool Run = false;

    public float SphereRadius = 0.02f;
    
    public enum ShapeTypes
    {
        Johnson,
        Grid,
        Wythoff,
        Other
    }

    [Serializable]
    public class FaceCollapseSetting
    {
        public bool enabled = true;
        public int FaceIndex = 0;
        public int EdgeIndex = 3;
    }
    
    // Only used to hide inspector fields with "ShowIf"
    private bool ShapeIsWythoff() => ShapeType == ShapeTypes.Wythoff;
    private bool ShapeIsJohnson() => ShapeType == ShapeTypes.Johnson;
    private bool ShapeIsGrid() => ShapeType == ShapeTypes.Grid;
    private bool ShapeIsOther() => ShapeType == ShapeTypes.Other;
    private bool UsesPandQ() => ShapeType == ShapeTypes.Grid || ShapeType == ShapeTypes.Other;
    
    public ShapeTypes ShapeType;

    [Range(1,40)] public int PrismP = 4;
    [ShowIf("UsesPandQ")] [Range(1,40)]
    public int PrismQ = 4;

    
    [ShowIf("ShapeIsWythoff")]
    public PolyTypes PolyType;
    [ShowIf("ShapeIsJohnson")]
    public PolyHydraEnums.JohnsonPolyTypes JohnsonPolyType;
    [ShowIf("ShapeIsGrid")]
    public GridEnums.GridTypes GridType;
    [ShowIf("ShapeIsGrid")]
    public GridEnums.GridShapes GridShape;
    [ShowIf("ShapeIsOther")]
    public PolyHydraEnums.OtherPolyTypes Other;

    [BoxGroup("Pre Op")] public Ops preop;
    [BoxGroup("Pre Op")] public FaceSelections preopFacesel;
    [BoxGroup("Pre Op")] public float preopAmount1 = 0;
    [BoxGroup("Pre Op")] public float preopAmount2 = 0;
 
    public override void Generate()
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
            case ShapeTypes.Other:
                poly = JohnsonPoly.BuildOther(Other, PrismP, PrismQ);
                break;
        }
        
        poly = poly.ApplyOp(preop, new OpParams {
            valueA = preopAmount1,
            valueB = preopAmount2,
            facesel = preopFacesel
        });
        
        base.Generate();

        FddInit();
        
    }
    
    private void FddInit()
    {
        Built = false;
        Nodes = new List<Node>();
        Edges = new List<Edge>();
        Forces = new List<Force>();
        alpha = 1f;
        AlphaMin = 0.01f;
        AlphaDecay = 1 - (float) Math.Pow(AlphaMin, 1f / 300f);
        AlphaTarget = 0f;
        VelocityDecay = 0.6f;

        var nodeMap = new Dictionary<Guid, Node>();

        foreach (var vertex in poly.Vertices)
        {
            Nodes.Add(new Node
            {
                Position = transform.TransformPoint(vertex.Position),
                Velocity = Vector3.zero
            });
            nodeMap[vertex.Name] = Nodes.Last();
        }

        foreach (var edge in poly.Halfedges)
        {
            Edges.Add(new Edge {Source=nodeMap[edge.Vertex.Name], Target=nodeMap[edge.Next.Vertex.Name]});
        }

        Forces.Add(new ExpansionForce {Nodes = Nodes, Edges = Edges});
        Forces.Add(new LinkForce {Nodes = Nodes, Edges = Edges});
    }

    private void OnDrawGizmos()
    {
        
        // DoDrawGizmos();
        
        if (Nodes != null)
        {
            Nodes.ForEach(node => Gizmos.DrawSphere(node.Position, SphereRadius));
        }

        if (Edges != null)
        {
            Edges.ForEach(edge => Gizmos.DrawLine(edge.Source.Position, edge.Target.Position));
        }
    }

    private void Update()
    {

        if (Run) FddUpdate();
            
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("Initing");
            FddInit();
        }
        
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("Updating");
            FddUpdate();
        }
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("Building");
            FddBuild();
        }
    }

    private void FddBuild()
    {
        for (var i = 0; i < Nodes.Count; i++)
        {
            var v = Nodes[i];
            poly.Vertices[i].Position = v.Position;
        }
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, ColorMethod, UVMethod);
        GetComponent<MeshFilter>().mesh = mesh;
        Built = true;
    }
    
    private void FddUpdate()
    {
        if (alpha < AlphaMin)
        {
            Debug.Log("Auto Building");
            if (!Built)
            {
                FddBuild();
            }
            return;
        }

        alpha += (AlphaTarget - alpha) * AlphaDecay;

        Forces.ForEach(force => force.ApplyForce(alpha));
        Nodes.ForEach(node => node.Position += node.Velocity *= VelocityDecay);
    }
}