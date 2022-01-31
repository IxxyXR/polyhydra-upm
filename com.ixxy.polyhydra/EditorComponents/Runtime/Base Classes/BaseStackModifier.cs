using System.Collections.Generic;
using System.Linq;
using Conway;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
public class BaseStackModifier : BaseComponent
{
    [MinValue(1)]public int Copies = 2;
    [MinValue(1)]public int Take = 1;
    public bool Weld;
    [ShowIf(nameof(Weld))] float WeldDistance = 0.01f;

    public ConwayPoly DoWeld(ConwayPoly mergedPoly)
    {
        if (Weld)
        {
            mergedPoly = mergedPoly.Weld(WeldDistance);
        }
        return mergedPoly;
    }

    public virtual Stack<ConwayPoly> Modify(Stack<ConwayPoly> polyStack)
    {
        var transforms = GetTransformList().ToList();
        var sourceStack = GetSourceStack(polyStack);

        // Remove them from stack
        polyStack = new Stack<ConwayPoly>();
        var mergedPoly = new ConwayPoly();
        for (int i = 0; i < Copies; i++)
        {
            mergedPoly.Append(sourceStack[i % sourceStack.Count], transforms[i % transforms.Count]);
        }
        polyStack.Push(DoWeld(mergedPoly));
        return polyStack;
    }

    private List<ConwayPoly> GetSourceStack(Stack<ConwayPoly> polyStack)
    {
        // Take repeated items from stack 
        var sourceStack = Enumerable.Repeat(polyStack.Take(Take), (Copies / Take) + 1)
            .SelectMany(x => x)
            .Take(Copies)
            .ToList();
        sourceStack.Reverse();
        return sourceStack;
    }

    public virtual IEnumerable<Matrix4x4> GetTransformList()
    {
        return Enumerable.Repeat(Matrix4x4.identity, Copies);
    }
}
