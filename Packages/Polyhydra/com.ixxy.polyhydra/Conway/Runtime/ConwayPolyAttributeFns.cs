using System;
using System.Collections.Generic;
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

    }
}