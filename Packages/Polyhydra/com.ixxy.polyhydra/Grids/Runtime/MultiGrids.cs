using System.Collections.Generic;
using System.Linq;
using Conway;
using UnityEngine;


namespace Grids
{
    public class MultiGrid
    {

        public Gradient gradient;

        public Color lineColor = Color.green;
        public float lineWidth = 1f;

        public Color connectorColor = Color.blue;
        public Connector connectorType = Connector.None;
        public float connectorWidth = 1f;

        private int divisions, dimensions;
        private float offset;
        
        private float MinDistance;
        private float MaxDistance;
        
        private float colorRatio = 1.0f;
        private float colorIndex = 0.0f;
        private float colorIntersect = 0.0f;



        public MultiGrid(int divisions, int dimensions, float offset, float MinDistance, float MaxDistance, float colorRatio = 1.0f, float colorIndex = 0.0f, float colorIntersect = 0.0f)
        {
            this.divisions = divisions;
            this.dimensions = dimensions;
            this.offset = offset;

            this.MinDistance = MinDistance;
            this.MaxDistance = MaxDistance;

            this.colorRatio = colorRatio;
            this.colorIndex = colorIndex;
            this.colorIntersect = colorIntersect;
        }
        

        public (ConwayPoly poly, List<List<Vector2>> shapes, List<float> colors) Build(bool sharedVertices=true)
        {
            var (shapes, colors) = GenerateShapes(Vector2.one);
            
            var faceRoles = new List<ConwayPoly.Roles>();
            var vertexDictionary = new Dictionary<Vector2, int>();
            var faceIndices = new List<List<int>>();
            var vertices = new List<Vector2>();

            float colorMin = colors.Min();
            float colorMax = colors.Max();

            for (var shapeIndex = 0; shapeIndex < shapes.Count; shapeIndex++)
            {
                var shape = shapes[shapeIndex];
                var face = new List<int>();
                for (var vertIndex = 0; vertIndex < shape.Count; vertIndex++)
                {
                    Vector2 vert = shape[vertIndex];

                    if (sharedVertices)
                    {
                        if (!vertexDictionary.ContainsKey(vert))
                        {
                            vertexDictionary[vert] = vertexDictionary.Count;
                        }

                        face.Add(vertexDictionary[vert]);
                    }
                    else
                    {
                        vertices.Add(vert);
                        face.Add(vertices.Count - 1);
                    }
                }
                
                if (face.Count == 5)
                {
                    faceIndices.Add(new List<int>{face[0], face[1], face[2], face[3]});
                    float colorValue = Mathf.InverseLerp(colorMin, colorMax, colors[shapeIndex]);
                    colors[shapeIndex] = colorValue;  // Write the normalized value back into colors
                    int roleIndex = Mathf.FloorToInt(colorValue * 4f);
                    var role = (ConwayPoly.Roles) roleIndex;
                    faceRoles.Add(role);
                }
            }

            if (sharedVertices)
            {
                vertices = vertexDictionary.Keys.ToList();
            }
            
            var poly = new ConwayPoly(
                vertices.Select(v=>new Vector3(v.x, 0, v.y)),
                faceIndices,
                faceRoles,
                Enumerable.Repeat(ConwayPoly.Roles.New, vertices.Count)
            );
            return (poly, shapes, colors);
        }

        public (List<List<Vector2>> shapes, List<float> colors) GenerateShapes(Vector2 size)
        {

            (Vector2 topLeft, Vector2 bottomRight) bounds = (Vector2.zero, -size);

            float diameter = size.magnitude;
            float scale = diameter / 2f / divisions;

            var rhombs = generateRhombs(divisions, dimensions , offset);

            var tf = new Vector2(size.x / 2f, size.y / 2f);
            tf *= scale;
            if (dimensions % 2 > 0)
            {
                // Rotate around origin?
                tf = Quaternion.Euler(0, (Mathf.PI / dimensions) * 0.5f, 0) * tf;
            }

            var shapes = new List<List<Vector2>>();
            var colors = new List<float>();
            
            for (int ii = 0; ii < rhombs.Count; ii++)
            {
                var rhomb = rhombs[ii];
                if (rhomb.shape.Any(o=>
                {
                    float distanceSqr = o.sqrMagnitude;
                    return distanceSqr > (MaxDistance*MaxDistance) || distanceSqr < (MinDistance*MinDistance);
                })) continue;
                List<Vector2> shape = rhomb.shape.Select(x=>x*tf).ToList();

                // Vector2 center = (shape[0] + shape[1] + shape[2] + shape[3]) / 4f;
                var lineWidthTransform = Vector2.one;
                
                // float scaleForLineWidth = Mathf.Abs(1f - (lineWidth/scale));
                // lineWidthTransform *= scaleForLineWidth;
                //
                // Vector2 scaledCenter = new Vector2(center.x * lineWidthTransform.x, center.y * lineWidthTransform.y);
                // lineWidthTransform = new Vector2(center.x - scaledCenter.x, center.y - scaledCenter.y);
                // lineWidthTransform *= scaleForLineWidth;
                //
                // shape = shape.Select(x => x * lineWidthTransform).ToList();
                
                // if (!Intersected(shape, bounds).isEmpty() && BoundingRect(shape).width > 0)
                if (true)
                {
                    var p = new List<Vector2>();
                    p.AddRange(shape.Select(x=>x * lineWidthTransform));
                    float gradientPos = 1;

                    float w1 = (shape[2] - shape[0]).magnitude;
                    float w2 = (shape[3] - shape[1]).magnitude;
                    float shapeRatio = Mathf.Min(w1, w2) / Mathf.Max(w1, w2);

                    float intersectRatio = rhomb.line1/dimensions;
                    intersectRatio += rhomb.line2/dimensions;
                    intersectRatio *= 0.5f;

                    float indexRatio = 1f - Mathf.Abs(rhomb.parallel1/divisions/2.0f);
                    indexRatio *= 1f - Mathf.Abs(rhomb.parallel2/divisions/2.0f);

                    if (colorRatio >= 0)
                    {
                        gradientPos *= 1f - (shapeRatio * colorRatio);
                    }
                    else
                    {
                        gradientPos *= 1f - ((1f - shapeRatio) * Mathf.Abs(colorRatio));
                    }

                    if (colorIntersect >= 0)
                    {
                        gradientPos *= 1f - (intersectRatio * colorIntersect);
                    }
                    else
                    {
                        gradientPos *= 1f - ((1f - intersectRatio) * Mathf.Abs(colorIntersect));
                    }

                    if (colorIndex >= 0)
                    {
                        gradientPos *= 1 - (indexRatio * colorIndex);
                    }
                    else
                    {
                        gradientPos *= 1 - ((1 - indexRatio) * Mathf.Abs(colorIndex));
                    }
                    colors.Add(float.IsNaN(gradientPos) ? 0 : gradientPos);

                    ////grad.colorAt(c, gradientPos);
                    ////gc.fillPainterPath(p, p.boundingRect().adjusted(-2, -2, 2, 2).toRect());

                    // if (connectorType != Connector.None)
                    // {
                    //     ////gc.setBackgroundColor(config.getColor("connectorColor"));
                    //     connectorWidth = connectorWidth * .5f;
                    //     var pConnect = new List<Vector2>();
                    //     float lower = connectorWidth / scale;
                    //
                    //     if (connectorType == Connector.Cross)
                    //     {
                    //         var cl = (shape[0], shape[1]).pointAt(0.5 - lower);
                    //         pConnect.moveTo(cl);
                    //         cl = (shape[0], shape[1]).pointAt(0.5 + lower);
                    //         pConnect.lineTo(cl);
                    //         cl = (shape[2], shape[3]).pointAt(0.5 - lower);
                    //         pConnect.lineTo(cl);
                    //         cl = (shape[2], shape[3]).pointAt(0.5 + lower);
                    //         pConnect.lineTo(cl);
                    //         pConnect.closeSubpath();
                    //
                    //         cl = (shape[1], shape[2]).pointAt(0.5 - lower);
                    //         pConnect.moveTo(cl);
                    //         cl = (shape[1], shape[2]).pointAt(0.5 + lower);
                    //         pConnect.lineTo(cl);
                    //         cl = (shape[3], shape[0]).pointAt(0.5 - lower);
                    //         pConnect.lineTo(cl);
                    //         cl = (shape[3], shape[0]).pointAt(0.5 + lower);
                    //         pConnect.lineTo(cl);
                    //         pConnect.closeSubpath();
                    //
                    //     }
                    //     else if (connectorType == Connector.CornerDot)
                    //     {
                    //         Vector2 cW(connectorWidth, connectorWidth);
                    //
                    //         QRectF dot(shape.at
                    //         (0) - cW, shape[0] + cW);
                    //         pConnect.addEllipse(dot);
                    //         dot = QRectF(shape[1] - cW, shape[1] + cW);
                    //         pConnect.addEllipse(dot);
                    //         dot = QRectF(shape[2] - cW, shape[2] + cW);
                    //         pConnect.addEllipse(dot);
                    //         dot = QRectF(shape[3] - cW, shape[3] + cW);
                    //         pConnect.addEllipse(dot);
                    //         pConnect = pConnect.intersected(p);
                    //
                    //     }
                    //     else if (connectorType == Connector.CenterDot)
                    //     {
                    //
                    //         QRectF dot(center
                    //         -Vector2(connectorWidth, connectorWidth), center + Vector2(connectorWidth, connectorWidth));
                    //         pConnect.addEllipse(dot);
                    //
                    //     }
                    //     else
                    //     {
                    //         for (int i = 1; i < shape.Count; i++)
                    //         {
                    //             Path pAngle;
                    //             Vector2 curPoint = shape[i];
                    //              l1(curPoint, shape.at
                    //             (i - 1));
                    //             Vector2 np;
                    //             if (i == 4)
                    //             {
                    //                 np = shape[1];
                    //             }
                    //             else
                    //             {
                    //                 np = shape[i]+ 1);
                    //             }
                    //
                    //              l2(curPoint, np);
                    //             float angleDiff = abs(fmod(abs(l1.angle() - l2.angle()) + 180, 360) - 180);
                    //
                    //             if (round(angleDiff) == 90)
                    //             {
                    //                 continue;
                    //             }
                    //
                    //             if (angleDiff > 90 && connectorType == Connector.Acute)
                    //             {
                    //                 continue;
                    //             }
                    //
                    //             if (angleDiff < 90 && connectorType == Connector.Obtuse)
                    //             {
                    //                 continue;
                    //             }
                    //
                    //             float length = (l1.length() * 0.5) - connectorWidth;
                    //             QRectF sweep(curPoint
                    //             -Vector2(length, length), curPoint + Vector2(length, length));
                    //             length = (l1.length() * 0.5) + connectorWidth;
                    //             QRectF sweep2(curPoint
                    //             -Vector2(length, length), curPoint + Vector2(length, length));
                    //
                    //             pAngle.moveTo(shape[i]);
                    //             pAngle.addEllipse(sweep2);
                    //             pAngle.addEllipse(sweep);
                    //             pAngle = pAngle.intersected(p);
                    //             pAngle.closeSubpath();
                    //             pConnect.addPath(pAngle);
                    //         }
                    //
                    //         pConnect.closeSubpath();
                    //
                    //     }
                    //
                    //     pConnect.setFillRule(Qt::WindingFill);
                    //     gc.fillPainterPath(pConnect);
                    //
                    // }

                }
                
                shapes.Add(shape);
                
            }
            
            return (shapes, colors);

        }


        public List<Rhomb> generateRhombs(int div, int lines, float offset)
        {
            var rhombs = new List<Rhomb>();
            var angles = new List<float>();

            int halfLines = div;
            int totalLines = (halfLines * 2) + 1;

            // Setup our imaginary lines...
            int dimensions = lines;
            for (int i = 0; i < dimensions; i++)
            {
                float angle = 2 * (Mathf.PI / lines) * i;
                angles.Add(angle);
            }

            for (int i = 0; i < angles.Count; i++)
            {
                float angle1 = angles[i];
                Vector2 p1 = new Vector2(totalLines * Mathf.Cos(angle1), -totalLines * Mathf.Sin(angle1));
                Vector2 p2 = -p1;

                for (int parallel1 = 0; parallel1 < totalLines; parallel1++)
                {
                    int index1 = halfLines - parallel1;

                    Vector2 offset1 = new Vector2((index1 + offset) * Mathf.Sin(angle1), (index1 + offset) * Mathf.Cos(angle1));
                    var l1 = (p1 + offset1, p2 + offset1);

                    for (int k = i + 1; k < angles.Count; k++)
                    {
                        float angle2 = angles[k];
                        var p3 = new Vector2(totalLines * Mathf.Cos(angle2), -totalLines * Mathf.Sin(angle2));
                        Vector2 p4 = -p3;

                        
                        for (int parallel2 = 0; parallel2 < totalLines; parallel2++)
                        {
                            int index2 = halfLines - parallel2;

                            var offset2 = new Vector2((index2 + offset) * Mathf.Sin(angle2), (index2 + offset) * Mathf.Cos(angle2));
                            var l2 = (p3 + offset2, p4 + offset2);
                            var intersect = new Vector2();

                            bool intersection = Intersects(l1, l2, ref intersect);
                            if (intersection)
                            {
                                List<int> indices = getIndicesFromPoint(intersect, angles, offset);
                                var shape = new List<Vector2>();
                                indices[i] = index1 + 1;
                                indices[k] = index2 + 1;
                                shape.Add(getVertex(indices, angles));
                                indices[i] = index1;
                                indices[k] = index2 + 1;
                                shape.Add(getVertex(indices, angles));
                                indices[i] = index1;
                                indices[k] = index2;
                                shape.Add(getVertex(indices, angles));
                                indices[i] = index1 + 1;
                                indices[k] = index2;
                                shape.Add(getVertex(indices, angles));
                                indices[i] = index1 + 1;
                                indices[k] = index2 + 1;
                                shape.Add(getVertex(indices, angles));

                                var rhomb = new Rhomb
                                {
                                    shape = shape,
                                    parallel1 = index1,
                                    parallel2 = index2,
                                    line1 = i,
                                    line2 = k,
                                };
                                rhombs.Add(rhomb);
                            }
                        }
                    }
                }
            }

            return rhombs;
        }
        
        private bool Intersects((Vector2 start , Vector2 end) A, (Vector2 start, Vector2 end) B, ref Vector2 intersect)
        {
            float tmp = (B.end.x - B.start.x) * (A.end.y - A.start.y) - (B.end.y - B.start.y) * (A.end.x - A.start.x);

            if (tmp == 0)
            {
                // No solution!
                return false;
            }
 
            float mu = ((A.start.x - B.start.x) * (A.end.y - A.start.y) - (A.start.y - B.start.y) * (A.end.x - A.start.x)) / tmp;
 
            intersect = new Vector2(
                B.start.x + (B.end.x - B.start.x) * mu,
                B.start.y + (B.end.y - B.start.y) * mu
            );
            return true;
        }

        public List<int> getIndicesFromPoint(Vector2 point, List<float> angles, float offset)
        {
            var indices = new List<int>();
            for (int a = 0; a < angles.Count; a++)
            {
                Vector2 p = point;
                float index = p.x * Mathf.Sin(angles[a]) + p.y * Mathf.Cos(angles[a]);
                indices.Add(Mathf.FloorToInt(index - offset + 1));
            }

            return indices;
        }

        public Vector2 getVertex(List<int> indices, List<float> angles)
        {
            if (indices==null || !indices.Any() || angles==null || !angles.Any())
            {
                Debug.LogError("error");
                return Vector2.zero;
            }

            float x = 0;
            float y = 0;

            for (int i = 0; i < indices.Count; i++)
            {
                x += indices[i] * Mathf.Cos(angles[i]);
                y += indices[i] * Mathf.Sin(angles[i]);
            }

            return new Vector2(x, y);
        }
    }

    public enum Connector
    {
        Cross,
        None
    }

    public struct Rhomb
    {
        public List<Vector2> shape;
        public int line1;
        public int parallel1;
        public int line2;
        public int parallel2;
    }
}