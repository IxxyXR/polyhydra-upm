using System.Collections.Generic;
using System.Linq;
using Conway;
using Grids;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TilerTest : ExampleBase
{

    public GridEnums.KeplerTypes type;
    [Range(1, 64)] public int xRepeat;
    [Range(1, 64)] public int yRepeat;
    public GridEnums.GridShapes gridShape;

    public override void Generate()
    {
        // poly = ConwayPoly._MakePolygon(6, true);
        // List<int> a = poly.AugmentFace(0, Enumerable.Range(0, 6).ToList(), 4);
        // poly.AugmentFace(a, 0, 3);
        // List<int> c = poly.AugmentFace(a, 3, 6);
        // // poly.Tile(a);

        poly = ConwayPoly._MakePolygon(12, true);
        poly.VertexStellate(new OpParams(.8f));
        // List<int> a = poly.AugmentFace(0, Enumerable.Range(0, 4), 3);
        // poly.Append(poly, new Vector3(xTile, 0, yTile));
        // poly.AugmentFace(a.Where((x, i) => i % 2 == 0), 2, 4);

        
        // poly = ConwayPoly._MakePolygon(8, true);
        // poly = poly.VertexScale(new OpParams(0.13f, FaceSelections.Even));
        // List<int> a = poly.AugmentFace(0, Enumerable.Range(0, 8).ToList(), 3);
        // poly.AugmentFace(a.Where((x, i) => i % 2 == 0), 2, 4);
        // List<int> c = poly.AugmentFace(a, 3, 6);
        // poly.Tile(a);
        
        base.Generate();
    }
}