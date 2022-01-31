using System;
using Conway;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public partial class GeometryModifier : BaseModifier
{
    public Ops op;
    [ShowIf(nameof(UsesAmount))]public float Value1 = 0;
    [ShowIf(nameof(UsesAmount2))]public float Value2 = 0;
    [ShowIf(nameof(UsesRandomize)), Foldout("More")] public bool Randomize = false;
    [Foldout("More"), Range(1, 5)] public int iterations = 1;
    
    private bool UsesAmount() => PolyHydraEnums.OpConfigs[op].usesAmount;
    private bool UsesAmount2() => PolyHydraEnums.OpConfigs[op].usesAmount2;
    private bool UsesFaces() => PolyHydraEnums.OpConfigs[op].usesFaces;
    private bool UsesRandomize() => PolyHydraEnums.OpConfigs[op].usesRandomize;

    public override ConwayPoly Modify(ConwayPoly poly)
    {
        Func<FilterParams, bool> filter = EnableFilter ? GetFilter(poly) : null;
        for (var i = 0; i < iterations; i++)
        {
            poly = poly.ApplyOp(op, new()
            {
                valueA = Value1 * (Animate ? animMultiplier : 1),
                valueB = Value2,
                filterFunc = filter,
                randomize = Randomize
            });
        }

        return poly;
    }
}