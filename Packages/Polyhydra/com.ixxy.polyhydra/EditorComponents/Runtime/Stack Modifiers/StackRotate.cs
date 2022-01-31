using System.Collections.Generic;
using Conway;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
public class StackRotate : BaseStackModifier
{
    public override Stack<ConwayPoly> Modify(Stack<ConwayPoly> polyStack)
    {
        return polyStack;
    }
}
