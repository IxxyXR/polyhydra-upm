using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Conway
{
    public partial class ConwayPoly
    {

        public List<Vector3> GetFaceCentroids(OpParams o)
        {
            var centroids = new List<Vector3>();
            for (var faceIndex = 0; faceIndex < Faces.Count; faceIndex++)
            {
                if (!IncludeFace(faceIndex, o.facesel, o.TagListFromString(), o.filterFunc)) continue;
                centroids.Add(Faces[faceIndex].Centroid);
            }

            return centroids;
        }

        public static ConwayPoly _MakePolygon(int sides, bool flip=false, float angleOffset = 0, float heightOffset = 0, float radius=1)
        {
            var faceIndices = new List<int[]>();
            var vertexPoints = new List<Vector3>();
            var faceRoles = Enumerable.Repeat(ConwayPoly.Roles.Existing, 1);
            var vertexRoles = Enumerable.Repeat(ConwayPoly.Roles.Existing, sides);
            
            faceIndices.Add(new int[sides]);
            
            float theta = Mathf.PI * 2 / sides;
            
            int start, end, inc;
            
            if (flip)
            {
                start = sides - 1;
                end = -1;
                inc = -1;
            }
            else
            {
                start = 0;
                end = sides;
                inc = 1;
            }
            
            for (int i = start; i != end; i += inc)
            {
                float angle = theta * i + (theta * angleOffset);
                vertexPoints.Add(new Vector3(Mathf.Cos(angle) * radius, heightOffset, Mathf.Sin(angle) * radius));
                faceIndices[0][i] = i;
            }

            return new ConwayPoly(vertexPoints, faceIndices, faceRoles, vertexRoles);
        }
    }
}