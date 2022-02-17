using System.Collections.Generic;
using System.Linq;
using Conway;
using NaughtyAttributes;
using UnityEngine;

[ExecuteInEditMode]
public class StackCloneToFaces : BaseStackModifier
{

    public bool RemoveTemplate;
    [Range(0,1)] public float Direction;
    public float ScaleStrength; 
    public Vector3 ScaleMin = Vector3.zero; 
    public Vector3 ScaleMax = Vector3.one;
    public float ScaleOffset = 1f;
    private ConwayPoly templatePoly;
    private List<float> faceAreas;
    private float maxFaceArea;
    private float minFaceArea;
    
    public override Stack<ConwayPoly> Modify(Stack<ConwayPoly> polyStack)
    {
        templatePoly = polyStack.Pop();
        if (ScaleStrength > 0)
        {
            faceAreas = templatePoly.Faces.Select(x => x.GetArea()).ToList();
            var validFaceAreas = faceAreas.Where(x => x > 0).ToList();
            // Find min/max ignoring degenerate faces
            minFaceArea = validFaceAreas.Min();
            maxFaceArea = validFaceAreas.Max();
        }
        polyStack = base.Modify(polyStack);
        if (!RemoveTemplate)
        {
            polyStack.Push(templatePoly);
        }
        return polyStack;
    }

    public override IEnumerable<Matrix4x4> GetTransformList()
    {
        return templatePoly.Faces.Select((face, i) =>
        {
            return Matrix4x4.TRS(
                face.Centroid,
                Quaternion.LookRotation(
                    Vector3.Lerp(
                        face.Normal == Vector3.zero ? Vector3.up : face.Normal, // Catch empty normals
                        face.Halfedge.Vertex.Position - face.Centroid,
                        Direction
                    ),
                Vector3.forward
                ),
                Vector3.LerpUnclamped(
                    ScaleMin,
                    ScaleMax,
                    ScaleStrength > 0 ?
                        Mathf.InverseLerp(minFaceArea, maxFaceArea, faceAreas[i]) * ScaleStrength + ScaleOffset :
                        ScaleOffset
                )
            );
        });
    }
}
