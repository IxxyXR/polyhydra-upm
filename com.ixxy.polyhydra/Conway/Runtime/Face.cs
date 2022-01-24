using System;
using System.Collections.Generic;
using System.Linq;
using Wythoff;
using UnityEngine;


namespace Conway
{

    public class Face
    {

        #region constructors

        public Face(Halfedge edge)
        {
            Halfedge = edge;
            Name = Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        public Face()
        {
            Name = Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        #endregion

        #region properties

        public Halfedge Halfedge { get; set; }
        public String Name { get; private set; }

        private Vector3 _cachedNormal;
        // private bool _hasCachedNormal;

        public Vector3 GetPolarPoint(float angle, float position)
        {
            // Returns a point that lies on the face
            // And is measured in polar coordinates
            // (angle in degrees, position is 0 to 1 with 0 being on the centroid and 1 being on the edge
            // The face can be concave but the algorithm assumes so overhangs or "ears"
            // As it starts by doing a fan triangulation from the centroid.
            
            var verts = GetVertices();
            var centroid = Centroid;
            angle = angle % 360;
            float previousAngle = 0;
            Vector3 result = centroid;
            for (int i = 1; i <= verts.Count; i++)
            {
                Vector3 a = centroid;
                Vector3 b = verts[i % verts.Count].Position;
                Vector3 c = verts[i - 1].Position;
                float currentAngle = previousAngle + Vector3.Angle(a - b, a - c);
                if (currentAngle > angle)
                {
                    float angleInTri = angle - previousAngle;
                    float ratio = Mathf.InverseLerp(previousAngle, currentAngle, previousAngle + angleInTri);
                    Vector3 pointOnEdge = Vector3.Lerp(b, c, ratio);
                    result = Vector3.LerpUnclamped(a, pointOnEdge, position);
                    break;
                }
                previousAngle = currentAngle;
            }

            return result;
        }
        
        public float GetArea()
        {
            float area = 0;
            var verts = GetVertices();

            if (verts.Count == 3)
            {
                Vector3 v = Vector3.Cross(
                    verts[0].Position - verts[1].Position,
                    verts[0].Position - verts[2].Position
                );
                area += v.magnitude * 0.5f;
            }
            else
            {
                var centroid = Centroid;
                for (int i = 1; i < verts.Count; i ++)
                {
                    Vector3 a = centroid;
                    Vector3 b = verts[i % verts.Count].Position;
                    Vector3 c = verts[i - 1].Position;
                    Vector3 v = Vector3.Cross(a - b, a - c);
                    area += v.magnitude * 0.5f;
                }
            }

            return area;
        }

        public Vector3 Centroid
        {
            get
            {
                Vector3 avg = new Vector3();
                List<Vertex> vertices = GetVertices();
                var vcount = vertices.Count;
                for (var i = 0; i < vcount; i++)
                {
                    Vertex v = vertices[i];
                    avg.x += v.Position.x;
                    avg.y += v.Position.y;
                    avg.z += v.Position.z;
                }

                avg.x /= vcount;
                avg.y /= vcount;
                avg.z /= vcount;
                
                return avg;
            }
        }

        /// <summary>
        /// Get the face normal (unit vector).
        /// </summary>
        public Vector3 Normal
        {
            get
            {
                // TODO cache normals
                // if (!_hasCachedNormal)
                // {
                Vector normal = new Vector(0, 0, 0);
                var centroid = Centroid;
                Halfedge edge = Halfedge;
                do
                {
                    Vector3 crossTmp = Vector3.Cross(edge.Vector - centroid, edge.Next.Vector - centroid);
                    normal = normal.sum(new Vector(crossTmp.x, crossTmp.y, crossTmp.z));
                    edge = edge.Next; // move on to next halfedge
                } while (edge != Halfedge);

                _cachedNormal = new Vector3((float) normal.x, (float) normal.y, (float) normal.z).normalized;
                // _hasCachedNormal = true;
                // }
                return _cachedNormal;
            }
        }

        public Quaternion FacingDirection
        {
            get
            {
                return Quaternion.LookRotation(Normal, Vector3.up);
            }
        }

        public int Sides {
            get { return GetVertices().Count; }
        }

        #endregion

        #region methods
    
            public List<Vertex> GetVertices() {
                List<Vertex> vertices = new List<Vertex>();
                Halfedge edge = Halfedge;
                do {
                    vertices.Add(edge.Vertex); // add vertex to list
                    edge = edge.Next; // move on to next halfedge
                } while (edge != Halfedge);
    
                return vertices;
            }
    
            public List<Halfedge> GetHalfedges()
            {
                List<Halfedge> halfedges = new List<Halfedge>();
                Halfedge edge = Halfedge;
                do {
                    halfedges.Add(edge); // add halfedge to list
                    edge = edge.Next; // move on to next halfedge
                } while (edge != Halfedge);
    
                return halfedges;
            }

            public void Split(Vertex v1, Vertex v2, out Face f_new, out Halfedge he_new, out Halfedge he_new_pair) {

                Halfedge e1 = Halfedge;
                while (e1.Vertex != v1) {
                    e1 = e1.Next;
                }
    
                if (v2 == e1.Next.Vertex) {
                    throw new Exception("Vertices adjacent");
                }
    
                if (v2 == e1.Prev.Vertex) {
                    throw new Exception("Vertices adjacent");
                }
    
                f_new = new Face(e1.Next);
    
                Halfedge e2 = e1;
                while (e2.Vertex != v2) {
                    e2 = e2.Next;
                    e2.Face = f_new;
                }
    
                he_new = new Halfedge(v1, e1.Next, e2, f_new);
                he_new_pair = new Halfedge(v2, e2.Next, e1, this, he_new);
                he_new.Pair = he_new_pair;
    
                e1.Next.Prev = he_new;
                e1.Next = he_new_pair;
                e2.Next.Prev = he_new_pair;
                e2.Next = he_new;
            }

            public IEnumerable<Halfedge> NakedEdges()
            {
                return GetHalfedges().Where(i=>i.Pair==null);
            }

            public bool HasNakedEdge()
            {
                return GetHalfedges().Any(i=>i.Pair==null);
            }

        #endregion

        public ConwayPoly DetachCopy()
        {

            IEnumerable<Vector3> verts = GetVertices().Select(i => i.Position);
            IEnumerable<IEnumerable<int>> faces = new List<List<int>>
            {Enumerable.Range(0, verts.Count()).ToList()};
            IEnumerable<ConwayPoly.Roles> faceRoles = new List<ConwayPoly.Roles> {ConwayPoly.Roles.New};
            IEnumerable<ConwayPoly.Roles> vertexRoles = new List<ConwayPoly.Roles> {ConwayPoly.Roles.New};
            return new ConwayPoly(verts, faces, faceRoles, vertexRoles);
        }

        public Halfedge GetBestEdge()
        {
            // Useful for deciding on an orientation for the face
            // i.e. UV mapping etc.
            // Fairly arbitrary choice of "best"
            // I've gone with "So the edge that is at the top - of forwards if the face is flat"
            // The vector from the center to this edge midpoint 
            // will at least always point in a consistent direction.
            // TODO "highest midpoint by y coord" is a fairly poor interpretation of edge direction
            // Should probably calculate a Vector2 angle based on one pair of possible coords 
            var faceNormal = Normal;
            Halfedge bestEdge = null;
            float bestScore = -9999999;
            var list = GetHalfedges();
            // How nearly facing up or down are we?
            var upness = Mathf.Abs((new Vector3(Mathf.Abs(faceNormal.x), Mathf.Abs(faceNormal.y), Mathf.Abs(faceNormal.z)) - Vector3.up).magnitude);
            for (var j = 0; j < list.Count; j++)
            {
                var edge = list[j];
                var mid = edge.Midpoint;
                // Pick a desired direction. Up for most faces but "forwards" for nearly up or down faces
                var edgeCoord = (upness<.01f) ? mid.z : mid.y;
                // Add a bit of another vector as a "tie-break". Favour "left"
                edgeCoord += (-mid.x * .001f);
                if (edgeCoord > bestScore)
                {
                    bestScore = edgeCoord;
                    bestEdge = edge;
                }
            }

            return bestEdge;
        }

        public Halfedge GetHalfEdge(int index)
        {
            Halfedge edge = Halfedge;

            for (int i = 0; i < index % Sides; i++)
            {
                edge = edge.Next;
            }

            return edge;
        }
    }
}