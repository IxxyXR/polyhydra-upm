using System.Collections.Generic;
using System.Linq;
using Conway;
using UnityEngine;

public class TactilePoly
{

    public int TilingType { get; private set; }
    public string TilingName { get; private set; }
    public int NumParams { get; private set; }
    private List<List<Vector2>> shapes;
    private IsohedralTiling tiling;
    
    public TactilePoly(int tilingType)
    {
        TilingType = tilingType;
        tiling = new IsohedralTiling(TilingType);
        TilingName = tiling.tilingName();
    }

    public ConwayPoly MakePoly(
        List<double> tilingParameters,
        int ExtraVerts,
        Vector2 NewVert1, Vector2 NewVert2,
        Vector2 Size
    )
    {
    
        var verts = new List<Vector3>();
        var faces = new List<List<int>>();
        var faceRoles = new List<ConwayPoly.Roles>();

        List<double> ps = tiling.getParameters();
        if (tiling.numParameters() > 1)
        {
            for (int i = 0; i < ps.Count; i++)
            {
                ps[i] = tilingParameters[i];
            }
        }

        tiling.setParameters(ps);


        // Make some edge shapes.  Note that here, we sidestep the
        // potential complexity of using .shape() vs. .parts() by checking
        // ahead of time what the intrinsic edge shape is and building
        // Bezier control points that have all necessary symmetries.

        var edges = new List<List<Vector2>>();

        var newVert1 = new Vector2(NewVert1.x + 0.1f, NewVert1.y);
        var newVert2 = new Vector2(NewVert2.x + 0.6f, NewVert2.y);

        for (int i = 0; i < tiling.numEdgeShapes(); ++i)
        {
            var ej = new List<Vector2>();
            EdgeShape shp = tiling.getEdgeShape(i);

            if (shp == EdgeShape.I)
            {
                // I Edges must look the same after both rotation and reflection
            }
            else if (shp == EdgeShape.J)
            {
                // J Edges can be a path of any shape
                if (ExtraVerts > 0)
                {
                    ej.Add(newVert1);
                    if (ExtraVerts > 1) ej.Add(newVert2);
                }
            }
            else if (shp == EdgeShape.S)
            {
                // S Edges must look the same after a 180° rotation
                if (ExtraVerts > 0)
                {
                    ej.Add(newVert1);
                    ej.Add(new Vector2(1.0f - ej[0].x, -ej[0].y));
                }
            }
            else if (shp == EdgeShape.U)
            {
                // U Edges must look the same after reflecting across their length
                if (ExtraVerts > 0)
                {
                    ej.Add(newVert1);
                    ej.Add(new Vector2(1.0f - ej[0].x, ej[0].y));
                }
            }

            edges.Add(ej);
        }
        
        
        
        
        
        
        
        
        // Yuk
        List<IEnumerable<IEnumerable<IsohedralTiling.Thing>>> tiles = tiling.fillRegionBounds(-Size.x/2f, -Size.y/2f, Size.x/2f, Size.y/2f).ToList();
        NumParams = tiling.numParameters();
        TilingName = tiling.tilingName();
        
        ConwayPoly.Roles[] availableRoles =
        {
            ConwayPoly.Roles.Existing,
            ConwayPoly.Roles.ExistingAlt,
            ConwayPoly.Roles.New,
            ConwayPoly.Roles.NewAlt,
            ConwayPoly.Roles.Ignored,
        };

        foreach (IEnumerable<IEnumerable<IsohedralTiling.Thing>> iii in tiles)
        {
            foreach (IEnumerable<IsohedralTiling.Thing> ii in iii)
            {
                foreach (IsohedralTiling.Thing i in ii)
                {
                    //var T = IsohedralTiling.mul(ST, i.T);
                    var T = i.T;
                    var colour = tiling.getColour(Mathf.FloorToInt(i.t1), Mathf.FloorToInt(i.t2), i.aspect);
                    var role = availableRoles[colour];

                    bool start = true;

                    var face = new List<int>();

                    foreach (var si in tiling.shape())
                    {
                        float[] S = IsohedralTiling.Multiply(T, si.T);
                        List<Vector2> seg = new[] {IsohedralTiling.Multiply(S, new Vector2(0, 0))}.ToList();
                        
                        if (si.shape != EdgeShape.I && ExtraVerts!=0)
                        {
                            var ej = edges[si.id];
                            seg.Add(IsohedralTiling.Multiply(S, ej[0]));
                            if (ej.Count > 1) seg.Add(IsohedralTiling.Multiply(S, ej[1]));
                        }

                        seg.Add(IsohedralTiling.Multiply(S, new Vector2(1.0f, 0.0f)));

                        if (si.rev)
                        {
                            seg.Reverse();
                        }

                        if (start)
                        {
                            start = false;
                            verts.Add(new Vector3(seg[0].x, 0, seg[0].y));
                            face.Add(verts.Count-1);
                        }
                        if (seg.Count == 2)
                        {
                            verts.Add(new Vector3(seg[1].x, 0, seg[1].y));
                            face.Add(verts.Count-1);
                        }
                        else
                        {
                            verts.Add(new Vector3(seg[1].x, 0, seg[1].y));
                            face.Add(verts.Count-1);

                            if (seg.Count > 2)
                            {
                                verts.Add(new Vector3(seg[2].x, 0, seg[2].y));
                                face.Add(verts.Count-1);
                            }

                            if (seg.Count > 3)
                            {
                                verts.Add(new Vector3(seg[3].x, 0, seg[3].y));
                                face.Add(verts.Count - 1);
                            }
                        }
                    }
                    
                    // Hacky way to fix normals
                    int normalSkip = face.Count / 3;
                    var normal = Vector3.Cross(verts[face[normalSkip]] - verts[face[0]], verts[face[normalSkip*2]] - verts[face[0]]);
                    if (normal.y < 0)
                    {
                        face.Reverse();
                    }

                    face = face.Take(face.Count-1).ToList();
                    faces.Add(face);
                    faceRoles.Add(role);
                }
            }
        }
        

            
        var vertexRoles = Enumerable.Repeat(ConwayPoly.Roles.Existing, verts.Count).ToList();
        
        return new ConwayPoly(verts, faces, faceRoles, vertexRoles);

    }



    public List<double> GetDefaultTilingParameters()
    {
        var defaultParameters = new List<double>();
        List<double> ps = tiling.getParameters();

        for (int i = 0; i < tiling.numParameters(); ++i)
        {
            if (i < tiling.numParameters())
            {
                defaultParameters.Add(ps[i]);
            }
            else
            {
                defaultParameters.Add(0);
            }
        }

        return defaultParameters;
    }
}