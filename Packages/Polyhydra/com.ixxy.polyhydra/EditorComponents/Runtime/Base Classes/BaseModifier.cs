using System;
using Conway;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
public abstract class BaseModifier : BaseComponent
{
    [NonSerialized] public float animMultiplier;
    [Foldout("More")] public bool Animate;
    public abstract ConwayPoly Modify(ConwayPoly poly);
}
