using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VectorGraphics;
using UnityEngine;

public class SvgTest : MonoBehaviour
{
    [Multiline]
    public string svg;

    private List<VectorUtils.Geometry> AllGeo;
    
    // Start is called before the first frame update
    void Start()
    {
        string svgSimple =
            @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""100"" height=""20"">
                <rect width=""100"" height=""20"" />
                <circle cx=""50"" cy=""60"" r=""50"" />
            </svg>";
        SVGParser.SceneInfo sceneInfo = SVGParser.ImportSVG(new StringReader(svg));
        Scene scene = sceneInfo.Scene;
        // Shape rectShape = sceneInfo.Scene.Root.Children[0].Shapes[0];
        // Shape circleShape = sceneInfo.Scene.Root.Children[0].Shapes[1];
        // foreach (var item in scene.Root.Children)
        // {
        //     BezierContour contour = circleShape.Contours[0];
        //     Debug.Log(contour.Segments);
        // }

        VectorUtils.TessellationOptions options = new VectorUtils.TessellationOptions
        {
            StepDistance=10f,
            MaxCordDeviation=.5f,
            MaxTanAngleDeviation=.1f,
            SamplingStepSize=10f
        };
        AllGeo = VectorUtils.TessellateScene(scene, options);

    }

    private void OnDrawGizmos()
    {
        if (AllGeo==null || AllGeo.Count == 0) return;
        Debug.Log("gizmos");
        foreach (var geo in AllGeo)
        {
            Vector3? prev = null;
            Vector3? current = null;
            foreach (Vector2 point in geo.Vertices)
            {
                current = new Vector3(-point.x/10f, -point.y/10f, 0);
                if (prev != null)
                {
                    Gizmos.DrawLine(prev.Value, current.Value);
                    Gizmos.DrawCube(current.Value, Vector3.one * .2f);
                }
                prev = current;
            }
            var first = new Vector3(-geo.Vertices[0].x/10f, -geo.Vertices[0].y/10f, 0);
            Gizmos.DrawLine(prev.Value, first);
        }
    }
}
