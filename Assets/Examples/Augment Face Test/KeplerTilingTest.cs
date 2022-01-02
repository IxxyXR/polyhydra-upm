using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class KeplerTilingTest : ExampleBase
{

    public Grids.Grids.KeplerTypes type;
    [Range(1, 64)] public int xRepeat;
    [Range(1, 64)] public int yRepeat;
    public PolyHydraEnums.GridShapes gridShape;

    public override void Generate()
    {
        poly = Grids.Grids.MakeKepler(type, xRepeat, yRepeat, gridShape);
        base.Generate();
    }
}