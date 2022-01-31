using System.Collections.Generic;
using Johnson;
using Conway;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateTactileGrid : BaseGenerator
{
    [Range(1,93)]
    public int TilingType = 1;
    [ReadOnly] public string TilingName;
    [ReadOnly] public string Symmetry;
    [ReadOnly] public string Grid;
    public List<Vector2> NewVerts;
    [Range(0f, 1f)]
    public List<double> tilingParameters;
    public bool Weld=false;
    public Vector2 Size;

    private TactilePoly tactilePoly;
    private int previousTilingType = -1;

    public override ConwayPoly Generate()
    {
        if (TilingType!=previousTilingType || tactilePoly==null)
        {
            tactilePoly = new TactilePoly(TilingType);
            TilingName = tactilePoly.TilingName;
            Symmetry = IsohedralTilingHelpers.tiling_types[tactilePoly.TilingType].symmetry_group;
            Grid = IsohedralTilingHelpers.tiling_types[tactilePoly.TilingType].grid;
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
        return poly;
    }
}