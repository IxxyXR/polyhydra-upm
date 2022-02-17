using System.Collections.Generic;
using Conway;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
public class StackRotate : BaseStackModifier
{
    public override Stack<ConwayPoly> Modify(Stack<ConwayPoly> polyStack)
    {
        if (polyStack.Count >= 3)
        {
            var top = polyStack.Pop();
            var next = polyStack.Pop();
            var nextNext = polyStack.Pop();
            polyStack.Push(next);
            polyStack.Push(top);
            polyStack.Push(nextNext);
        }
        return polyStack;
    }
}
