using System.Collections.Generic;
using Conway;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
public class StackSwap : BaseStackModifier
{
    public override Stack<ConwayPoly> Modify(Stack<ConwayPoly> polyStack)
    {
        if (polyStack.Count >= 2)
        {
            var top = polyStack.Pop();
            var next = polyStack.Pop();
            polyStack.Push(top);
            polyStack.Push(next);
        }
        return polyStack;
    }    
}
