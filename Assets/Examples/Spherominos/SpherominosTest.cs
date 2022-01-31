using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Conway;
using Grids;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SpherominosTest : MonoBehaviour
{
    public GridEnums.GridTypes GridType;
    public GridEnums.GridShapes GridShape;
    [Range(1,40)] public int width = 4;
    [Range(1,40)] public int depth = 3;
    public bool ColorBySides;

    [Multiline]
    public string bits;

    public int offsetX;
    public int offsetY;
    public float extrusionAmount = 0.1f;

    public bool ApplyOp;
    public Ops op;
    public FaceSelections facesel;
    public float opAmount = 0;
    public float op2Amount = 0;

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
        var poly = Grids.Grids.MakeGrid(GridType, GridShape, width, depth);
        var colorMethod = ColorBySides ? PolyHydraEnums.ColorMethods.BySides : PolyHydraEnums.ColorMethods.ByRole;
        if (ApplyOp)
        {
            var o = new OpParams {valueA = opAmount, valueB = op2Amount, facesel = facesel};
            poly = poly.ApplyOp(op, o);
        }

        var bitsJoined = Enumerable.Repeat("".PadRight(width, '1'), depth).ToArray();
        for (var i = 0;i < bits.Split(new[] {Environment.NewLine}, StringSplitOptions.None).Length; i++)
        {
            var line = bits.Split(new[] {Environment.NewLine}, StringSplitOptions.None)[i];
            bitsJoined[i]  = line.PadRight(width, '0').Substring(0, width);
        }
        Func<FilterParams, bool> filterFunc = f =>
        {
            int x = Mathf.FloorToInt(f.index / width);
            int y = f.index % width;
            Debug.Log($"{f.index} {x}x{y}");
            return !(x < bitsJoined[y].Length && bitsJoined[y].Substring(x, 1) == "1");
        };
        poly = poly.FaceRemove(new OpParams{filterFunc = filterFunc});
        poly = poly.Shell(new OpParams{valueA = extrusionAmount});

        var mesh = PolyMeshBuilder.BuildMeshFromConwayPoly(poly, false, null, colorMethod);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
