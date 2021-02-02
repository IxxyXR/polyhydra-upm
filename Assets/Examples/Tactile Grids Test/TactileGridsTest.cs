using System.Collections.Generic;
using System.Linq;
using Conway;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TactileGridsTest : MonoBehaviour
{
    [Range(1,93)]
    public int TilingType = 1;
    [ReadOnly] public string TilingName;
    [ReadOnly] public int NumParams;
    [Range(0,2)]
    public int ExtraVerts = 0;
    public Vector2 NewVert1;
    public Vector2 NewVert2;
    [Range(0f, 1f)]
    public float[] tilingParameters;
    public bool Weld=false;

    public Vector2 Size;

    public bool ApplyOp;
    public Ops op1;
    public FaceSelections op1Facesel;
    public float op1Amount1 = 0;
    public float op1Amount2 = 0;
    public Ops op2;
    public FaceSelections op2Facesel;
    public float op2Amount1 = 0;
    public float op2Amount2 = 0;
    public Ops op3;
    public FaceSelections op3Facesel;
    public float op3Amount1 = 0;
    public float op3Amount2 = 0;

    public PolyHydraEnums.ColorMethods ColorMethod;
    public ConwayPoly.TagType TagType;

    public bool vertexGizmos;
    public bool faceGizmos;
    public bool edgeGizmos;
    public bool faceCenterGizmos;
    
    public Gradient Colors;
    public float ColorRange;
    public float ColorOffset;

    private ConwayPoly poly;
    private List<List<Vector2>> shapes;
    private int PreviousTilingType = 1;
    
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

        var verts = new List<Vector3>();
        var faces = new List<List<int>>();
        var faceRoles = new List<ConwayPoly.Roles>();

        void drawTiling() {

            var (tiling, edges) = makeTiling();

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
        }

        (IsohedralTiling, List<List<Vector2>>) makeTiling() {
            
            if (!IsohedralTiling.tilingTypes.Contains(TilingType))
            {
                Debug.LogWarning("Invalid tiling type");
                TilingType = PreviousTilingType;
            }
            
            var tiling = new IsohedralTiling(TilingType);

            if (TilingType != PreviousTilingType)
            {
                tiling.Reset(TilingType);
                List<double> ps = tiling.getParameters();
                tilingParameters = new float[tiling.numParameters()];
                for (int i = 0; i < tiling.numParameters(); ++i)
                {
                    if (i < tiling.numParameters())
                    {
                        tilingParameters[i] = (float)ps[i];
                    }
                    else
                    {
                        tilingParameters[i] = 0;
                    }
                }
                PreviousTilingType = TilingType;
            }
            else
            {
                List<double> ps = tiling.getParameters();
                if (tiling.numParameters() > 1)
                {
                    for (int i = 0; i < ps.Count; i++)
                    {
                        ps[i] = tilingParameters[i];
                    }
                }

                tiling.setParameters(ps);
            }


            // Make some edge shapes.  Note that here, we sidestep the
            // potential complexity of using .shape() vs. .parts() by checking
            // ahead of time what the intrinsic edge shape is and building
            // Bezier control points that have all necessary symmetries.

            var edges = new List<List<Vector2>>();

            var newVert1 = new Vector2(NewVert1.x + 0.1f, NewVert1.y);
            var newVert2 = new Vector2(NewVert2.x + 0.6f, NewVert2.y);
            
            for (int i = 0; i < tiling.numEdgeShapes(); ++i) {
                
                var ej = new List<Vector2>();
                EdgeShape shp = tiling.getEdgeShape(i);

                if (shp == EdgeShape.I)
                {
                    // I Edges must look the same after both rotation and reflection
                } else if (shp == EdgeShape.J)
                {
                    // J Edges can be a path of any shape
                    if (ExtraVerts > 0) 
                    {
                        ej.Add(newVert1);
                        if (ExtraVerts > 1) ej.Add(newVert2);
                    }
                } else if (shp == EdgeShape.S) {
                    // S Edges must look the same after a 180° rotation
                    if (ExtraVerts > 0)
                    {
                        ej.Add(newVert1);
                        ej.Add(new Vector2(1.0f - ej[0].x, -ej[0].y));
                    }
                } else if (shp == EdgeShape.U) {
                    // U Edges must look the same after reflecting across their length
                    if (ExtraVerts > 0)
                    {
                        ej.Add(newVert1);
                        ej.Add(new Vector2(1.0f - ej[0].x, ej[0].y));
                    }
                }

                edges.Add(ej);
            }

            return (tiling, edges);
        }

        drawTiling();
            
        var vertexRoles = Enumerable.Repeat(ConwayPoly.Roles.Existing, verts.Count).ToList();
        poly = new ConwayPoly(verts, faces, faceRoles, vertexRoles);
        poly.ClearTags();
        if (Weld)
        {
            poly = poly.Weld(0.01f);
            poly.ClearTags();
        }
        
        if (ApplyOp)
        {
            var o1 = new OpParams {valueA = op1Amount1, valueB = op1Amount2, facesel = op1Facesel};
            poly = poly.ApplyOp(op1, o1);
            var o2 = new OpParams {valueA = op2Amount1, valueB = op2Amount2, facesel = op2Facesel};
            poly = poly.ApplyOp(op2, o2);
            var o3 = new OpParams {valueA = op3Amount1, valueB = op3Amount2, facesel = op3Facesel};
            poly = poly.ApplyOp(op3, o3);
        }

        var colors = Enumerable.Range(0,8).Select(x => Colors.Evaluate(((x / 8f) * ColorRange + ColorOffset) % 1)).ToArray();
        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, colors, ColorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void OnDrawGizmos ()
    {
        GizmoHelper.DrawGizmos(poly, transform, vertexGizmos, faceGizmos, edgeGizmos, faceCenterGizmos, 0.3f);
    }

}
