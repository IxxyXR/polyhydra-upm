using System;
using System.Collections.Generic;
using Conway;
using Johnson;
using NaughtyAttributes;
using UnityEngine;
using Wythoff;
using Face = Conway.Face;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class KeplerTilingTest : ExampleBase
{

    public Grids.Grids.KeplerTypes type;
    [Range(1, 24)] public int xRepeat;
    [Range(1, 24)] public int yRepeat;
    
    public override void Generate()
    {
        poly = Grids.Grids.MakeKepler(type, xRepeat, yRepeat);
        base.Generate();
    }
}