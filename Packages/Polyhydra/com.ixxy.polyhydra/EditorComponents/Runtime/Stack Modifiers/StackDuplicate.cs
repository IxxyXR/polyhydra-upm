using System.Collections.Generic;
using Conway;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
public class StackDuplicate : BaseStackModifier
{
    public override Stack<ConwayPoly> Modify(Stack<ConwayPoly> polyStack)
    {
        if (polyStack.Count>0) polyStack.Push(polyStack.Peek());
        return polyStack;
    }
}
