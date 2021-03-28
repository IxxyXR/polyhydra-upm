using System.Collections.Generic;
using System.Linq;
using Conway;
using NaughtyAttributes;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TactileGridsTest : ExampleBase
{
    [Range(1,93)]
    public int TilingType = 1;
    [ReadOnly] public string TilingName;
    [Range(0,2)]
    public int ExtraVerts = 0;
    public Vector2 NewVert1;
    public Vector2 NewVert2;
    [Range(0f, 1f)]
    public List<double> tilingParameters;
    public bool Weld=false;
    public Vector2 Size;

    private TactilePoly tactilePoly;
    private int previousTilingType = -1;

    void Start()
    {
        base.Start();
    }

    public override void Generate()
    {
        if (TilingType!=previousTilingType || tactilePoly==null)
        {
            tactilePoly = new TactilePoly(TilingType);
            TilingName = tactilePoly.TilingName;
            previousTilingType = TilingType;
            tilingParameters = tactilePoly.GetDefaultTilingParameters();
        }
        poly = tactilePoly.MakePoly(tilingParameters, ExtraVerts, NewVert1, NewVert2, Size);
        poly.ClearTags();
        if (Weld)
        {
            poly = poly.Weld(0.01f);
            poly.ClearTags();
        }

        base.Generate();
    }

}
