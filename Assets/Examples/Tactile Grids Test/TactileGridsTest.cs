using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TactileGridsTest : ExampleBase
{
    [Range(1,93)]
    public int TilingType = 1;
    [ReadOnly] public string TilingName;
    public List<Vector2> NewVerts;
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
        poly = tactilePoly.MakePoly(tilingParameters, NewVerts, Size);
        poly.ClearTags();
        if (Weld)
        {
            poly = poly.Weld(0.01f);
            poly.ClearTags();
        }

        base.Generate();
    }

}
