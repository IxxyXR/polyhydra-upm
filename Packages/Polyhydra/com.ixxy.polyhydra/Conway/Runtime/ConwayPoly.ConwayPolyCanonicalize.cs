using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Conway
{
    public partial class ConwayPoly
    {
        #region Canonicalize

        public void SetVertexPositions(List<Vector3> newPositions)
        {
            for (var i = 0; i < Vertices.Count; i++)
            {
                Vertices[i].Position = newPositions[i];
            }
        }

        private static double MAX_VERTEX_CHANGE = 1.0;

        /**
		 * A port of the "reciprocalN" function written by George Hart.
		 *
		 * @param poly The polyhedron to apply this canonicalization to.
		 * @return A list of the new vertices of the dual polyhedron.
		 */
        private static List<Vector3> ReciprocalVertices(ConwayPoly poly)
        {
            var newVertices = new List<Vector3>();

            foreach (var face in poly.Faces)
            {
                // Initialize values which will be updated in the loop below
                var centroid = face.Centroid;
                var normalSum = new Vector3();
                double avgEdgeDistance = 0.0;

                // Retrieve the indices of the vertices defining this face
                var faceVertices = face.GetVertices();

                // Keep track of the "previous" two vertices in CCW order
                var lastlastVertex = faceVertices[faceVertices.Count - 2];
                var lastVertex = faceVertices[faceVertices.Count - 1];

                for (var i = 0; i < faceVertices.Count; i++)
                {
                    var vertex = faceVertices[i];
                    // Compute the normal of the plane defined by this vertex and
                    // the previous two
                    var v1 = lastlastVertex.Position;
                    v1 -= lastVertex.Position;
                    var v2 = vertex.Position;
                    v2 -= lastVertex.Position;
                    var normal = Vector3.Cross(v1, v2);
                    normalSum += normal;

                    // Compute distance from edge to origin
                    avgEdgeDistance += PointLineDist(new Vector3(), lastlastVertex.Position, lastVertex.Position);

                    // Update the previous vertices for the next iteration
                    lastlastVertex = lastVertex;
                    lastVertex = vertex;
                }

                normalSum = normalSum.normalized;
                avgEdgeDistance /= faceVertices.Count;

                var resultingVector = new Vector3();
                resultingVector = Vector3.Dot(centroid, normalSum) * normalSum;
                resultingVector *= Mathf.Pow(1.0f / resultingVector.magnitude, 2);
                resultingVector *= (1.0f + (float) avgEdgeDistance) / 2.0f;
                newVertices.Add(resultingVector);
            }

            return newVertices;
        }

        private static double PointLineDist(Vector3 lineDir, Vector3 linePnt, Vector3 pnt)
        {
            lineDir.Normalize(); //this needs to be a unit vector
            var v = pnt - linePnt;
            var d = Vector3.Dot(v, lineDir);
            return (linePnt + lineDir * d).magnitude;
        }

        /**
		 * Modifies a polyhedron's vertices such that faces are closer to planar.
		 * The more iterations, the closer the faces are to planar. If a vertex
		 * moves by an unexpectedly large amount, or if the new vertex position
		 * has an NaN component, the algorithm automatically terminates.
		 *
		 * @param poly          The polyhedron whose faces to planarize.
		 * @param numIterations The number of iterations to planarize for.
		 */
        public static void Planarize(ConwayPoly poly, int numIterations)
        {
            var dual = poly.Dual();

            for (int i = 0; i < numIterations; i++)
            {
                var newDualPositions = ReciprocalVertices(poly);
                dual.SetVertexPositions(newDualPositions);
                var newPositions = ReciprocalVertices(dual);

                double maxChange = 0.0;

                for (int j = 0; j < poly.Vertices.Count; j++)
                {
                    var newPos = poly.Vertices[j].Position;
                    var diff = newPos - poly.Vertices[j].Position;
                    maxChange = Math.Max(maxChange, diff.magnitude);
                }

                // Check if an error occurred in computation. If so, terminate
                // immediately. This likely occurs when faces are already planar.
                if (Double.IsNaN(newPositions[0].x) || Double.IsNaN(newPositions[0].y) ||
                    Double.IsNaN(newPositions[0].z))
                {
                    break;
                }

                // Check if the position changed by a significant amount so as to
                // be erroneous. If so, terminate immediately
                if (maxChange > MAX_VERTEX_CHANGE)
                {
                    break;
                }

                poly.SetVertexPositions(newPositions);
            }
        }

        /**
		 * Modifies a polyhedron's vertices such that faces are closer to planar.
		 * When no vertex moves more than the given threshold, the algorithm
		 * terminates.
		 *
		 * @param poly      The polyhedron to canonicalize.
		 * @param threshold The threshold of vertex movement after an iteration.
		 * @return The number of iterations that were executed.
		 */
        public static int Planarize(ConwayPoly poly, double threshold)
        {
            return _Canonicalize(poly, threshold, true);
        }

        /**
		 * A port of the "reciprocalC" function written by George Hart. Reflects
		 * the centers of faces across the unit sphere.
		 *
		 * @param poly The polyhedron whose centers to invert.
		 * @return The list of inverted face centers.
		 */
        private static List<Vector3> ReciprocalCenters(ConwayPoly poly)
        {
            var faceCenters = new List<Vector3>();

            for (var i = 0; i < poly.Faces.Count; i++)
            {
                var newCenter = poly.Faces[i].Centroid;
                newCenter *= 1.0f / Mathf.Pow(newCenter.magnitude, 2);
                faceCenters.Add(newCenter);
            }

            return faceCenters;
        }

        /**
		 * Canonicalizes a polyhedron by adjusting its vertices iteratively.
		 *
		 * @param poly          The polyhedron whose vertices to adjust.
		 * @param numIterations The number of iterations to adjust for.
		 */
        public static void Adjust(ConwayPoly poly, int numIterations)
        {
            var dual = poly.Dual();

            for (int i = 0; i < numIterations; i++)
            {
                var newDualPositions = ReciprocalCenters(poly);
                dual.SetVertexPositions(newDualPositions);
                var newPositions = ReciprocalCenters(dual);
                poly.SetVertexPositions(newPositions);
            }
        }

        /**
		 * Canonicalizes a polyhedron by adjusting its vertices iteratively. When
		 * no vertex moves more than the given threshold, the algorithm terminates.
		 *
		 * @param poly      The polyhedron whose vertices to adjust.
		 * @param threshold The threshold of vertex movement after an iteration.
		 * @return The number of iterations that were executed.
		 */
        public static int Adjust(ConwayPoly poly, double threshold)
        {
            return _Canonicalize(poly, threshold, false);
        }

        /**
		 * A helper method for threshold-based termination in both planarizing and
		 * adjusting. If a vertex moves by an unexpectedly large amount, or if the
		 * new vertex position has an NaN component, the algorithm automatically
		 * terminates.
		 *
		 * @param poly      The polyhedron to canonicalize.
		 * @param threshold The threshold of vertex movement after an iteration.
		 * @param planarize True if we are planarizing, false if we are adjusting.
		 * @return The number of iterations that were executed.
		 */
        private static int _Canonicalize(ConwayPoly poly, double threshold, bool planarize)
        {
            var dual = poly.Dual();
            var currentPositions = poly.Vertices.Select(x => x.Position).ToList();

            int iterations = 0;

            while (true)
            {
                var newDualPositions = planarize ? ReciprocalVertices(poly) : ReciprocalCenters(poly);
                dual.SetVertexPositions(newDualPositions);
                var newPositions = planarize ? ReciprocalVertices(dual) : ReciprocalCenters(dual);

                double maxChange = 0.0;
                for (int i = 0; i < currentPositions.Count; i++)
                {
                    var newPos = poly.Vertices[i].Position;
                    var diff = newPos - currentPositions[i];
                    maxChange = Math.Max(maxChange, diff.magnitude);
                }

                // Check if an error occurred in computation. If so, terminate
                // immediately
                if (Double.IsNaN(newPositions[0].x) || Double.IsNaN(newPositions[0].y) ||
                    Double.IsNaN(newPositions[0].z))
                {
                    break;
                }

                // Check if the position changed by a significant amount so as to
                // be erroneous. If so, terminate immediately
                if (planarize && maxChange > MAX_VERTEX_CHANGE)
                {
                    break;
                }

                poly.SetVertexPositions(newPositions);

                if (maxChange < threshold)
                {
                    break;
                }

                currentPositions = poly.Vertices.Select(x => x.Position).ToList();
                iterations++;
            }

            return iterations;
        }

        /**
		* Canonicalizes this polyhedron for the given number of iterations.
		* See util.Canonicalize for more details. Performs "adjust" followed
		* by "planarize".
		*
		* @param iterationsAdjust    The number of iterations to "adjust" for.
		* @param iterationsPlanarize The number of iterations to "planarize" for.
		* @return The canonicalized version of this polyhedron.
		*/
        public ConwayPoly Canonicalize(int iterationsAdjust, int iterationsPlanarize)
        {
            var previousFaceRoles = FaceRoles;
            var canonicalized = Duplicate();
            if (iterationsAdjust > 0) Adjust(canonicalized, iterationsAdjust);
            if (iterationsPlanarize > 0) Planarize(canonicalized, iterationsPlanarize);
            canonicalized.FaceRoles = previousFaceRoles;
            return canonicalized;
        }

        /**
		 * Canonicalizes this polyhedron until the change in position does not
		 * exceed the given threshold. That is, the algorithm terminates when no vertex
		 * moves more than the threshold after one iteration.
		 *
		 * @param thresholdAdjust    The threshold for change in one "adjust"
		 *                           iteration.
		 * @param thresholdPlanarize The threshold for change in one "planarize"
		 *                           iteration.
		 * @return The canonicalized version of this polyhedron.
		 */
        public ConwayPoly Canonicalize(double thresholdAdjust, double thresholdPlanarize)
        {
            var previousFaceRoles = FaceRoles;
            var previousVertexRoles = VertexRoles;
            ConwayPoly canonicalized = Duplicate();
            if (thresholdAdjust > 0) Adjust(canonicalized, thresholdAdjust);
            if (thresholdPlanarize > 0) Planarize(canonicalized, thresholdPlanarize);
            canonicalized.FaceRoles = previousFaceRoles;
            canonicalized.VertexRoles = previousVertexRoles;
            return canonicalized;
        }

        public ConwayPoly Hinge(float amount)
        {
            // Rotate singly connected faces around the connected edge
            foreach (Face f in Faces)
            {
                Halfedge hinge = null;

                // Find a single connected edge
                foreach (Halfedge e in f.GetHalfedges())
                {
                    if (e.Pair != null) // This edge is connected
                    {
                        if (hinge == null) // Our first connected edge
                        {
                            // Record the first connected edge and keep looking
                            hinge = e;
                        }
                        else // We already found a hinge for this face
                        {
                            // Therefore this Face has more than 1 connected edge
                            hinge = null;
                            break;
                        }
                    }
                }

                if (hinge != null) // We found a single hinge for this face
                {
                    Vector3 axis = hinge.Vector;
                    Quaternion rotation = Quaternion.AngleAxis(amount, axis);

                    var vs = f.GetVertices();
                    for (var i = 0; i < vs.Count; i++)
                    {
                        Vertex v = vs[i];
                        // Only rotate vertices that aren't part of the hinge
                        if (v != hinge.Vertex && v != hinge.Pair.Vertex)
                        {
                            v.Position -= hinge.Vertex.Position;
                            v.Position = rotation * v.Position;
                            v.Position += hinge.Vertex.Position;
                        }
                    }
                }
            }

            return this;
        }

        public ConwayPoly Spherize(float amount, FaceSelections vertexsel)
        {
            // TODO - preserve planar faces

            var vertexPoints = new List<Vector3>();
            var faceIndices = ListFacesByVertexIndices();

            for (var vertexIndex = 0; vertexIndex < Vertices.Count; vertexIndex++)
            {
                var vertex = Vertices[vertexIndex];
                if (IncludeVertex(vertexIndex, vertexsel))
                {
                    vertexPoints.Add(Vector3.LerpUnclamped(vertex.Position, vertex.Position.normalized, amount));
                    VertexRoles[vertexIndex] = Roles.Existing;
                }
                else
                {
                    vertexPoints.Add(vertex.Position);
                    VertexRoles[vertexIndex] = Roles.Ignored;
                }
            }

            var conway = new ConwayPoly(vertexPoints, faceIndices, FaceRoles, VertexRoles, FaceTags);
            return conway;
        }

        #endregion Canonicalize
    }
}