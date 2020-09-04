using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Conway
{
    public partial class ConwayPoly
    {
        public ConwayPoly AddMirrored(OpParams o, Vector3 axis)
        {
            float amount = o.GetValueA(this, 0);
            var original = FaceKeep(o);
            var mirror = original.Duplicate();
            mirror.Mirror(axis, amount);
            mirror = mirror.FaceKeep(o);
            mirror.Halfedges.Flip();
            original.Append(mirror);
            original.Recenter();
            return original;
        }

        public void Mirror(Vector3 axis, float offset)
        {
            Vector3 offsetVector = offset * axis;
            for (var i = 0; i < Vertices.Count; i++)
            {
                Vertices[i].Position -= offsetVector;
            }

            for (var i = 0; i < Vertices.Count; i++)
            {
                var v = Vertices[i];
                v.Position = Vector3.Reflect(v.Position, axis);
            }
        }

        public ConwayPoly AddCopy(Vector3 axis, float amount, FaceSelections facesel = FaceSelections.All,
            string tags = "")
        {
            amount /= 2.0f;
            var original = Duplicate(axis * -amount, Quaternion.identity, 1.0f);
            var copy = Duplicate(axis * amount, Quaternion.identity, 1.0f);
            copy = copy.FaceKeep(facesel, tags);
            original.Append(copy);
            return original;
        }

        public ConwayPoly Stack(Vector3 axis, float offset, float scale, float limit = 0.1f,
            FaceSelections facesel = FaceSelections.All, string tags = "")
        {
            scale = Mathf.Abs(scale);
            scale = Mathf.Clamp(scale, 0.0001f, 0.99f);
            Vector3 offsetVector = axis * offset;

            var original = Duplicate();
            var copy = FaceKeep(facesel, tags);

            int copies = 0;
            while (scale > limit && copies < 64) // TODO make copies configurable
            {
                original.Append(copy.Duplicate(offsetVector, Quaternion.identity, scale));
                scale *= scale;
                offsetVector += axis * offset;
                offset *= Mathf.Sqrt(scale);  // Not sure why but sqrt *looks* right.
                copies++;
            }

            return original;
        }

        public ConwayPoly Transform(Vector3 pos, Vector3 rot, Vector3 scale)
        {
            var qrot = Quaternion.Euler(rot.x, rot.y, rot.z);
            var matrix = Matrix4x4.TRS(pos, qrot, scale);

            var copy = Duplicate();
            for (var i = 0; i < copy.Vertices.Count; i++)
            {
                var v = copy.Vertices[i];
                v.Position = matrix.MultiplyPoint3x4(v.Position);
            }

            return copy;
        }

        public ConwayPoly Rotate(Vector3 axis, float amount)
        {
            var copy = Duplicate();
            for (var i = 0; i < copy.Vertices.Count; i++)
            {
                var v = copy.Vertices[i];
                v.Position = Quaternion.AngleAxis(amount, axis) * v.Position;
            }

            return copy;
        }

        public Vector3 GetCentroid()
        {
            if (Vertices.Count == 0) return Vector3.zero;

            return new Vector3(
                Vertices.Average(x => x.Position.x),
                Vertices.Average(x => x.Position.y),
                Vertices.Average(x => x.Position.z)
            );
        }

        public void Recenter()
        {
            Vector3 newCenter = GetCentroid();
            for (var i = 0; i < Vertices.Count; i++)
            {
                Vertices[i].Position -= newCenter;
            }
        }

        public ConwayPoly SitLevel(float faceFactor = 0)
        {
            int faceIndex = Mathf.FloorToInt(Faces.Count * faceFactor);
            faceIndex = Mathf.Clamp(faceIndex, 0, 1);
            var vertexPoints = new List<Vector3>();
            var faceIndices = ListFacesByVertexIndices();

            for (var vertexIndex = 0; vertexIndex < Vertices.Count; vertexIndex++)
            {
                var rot = Quaternion.LookRotation(Faces[faceIndex].Normal);
                var rotForwardToDown = Quaternion.FromToRotation(Vector3.down, Vector3.forward);
                vertexPoints.Add(Quaternion.Inverse(rot * rotForwardToDown) * Vertices[vertexIndex].Position);
            }

            var conway = new ConwayPoly(vertexPoints, faceIndices, FaceRoles, VertexRoles, FaceTags);
            return conway;
        }

        public ConwayPoly Stretch(float amount)
        {
            var vertexPoints = new List<Vector3>();
            var faceIndices = ListFacesByVertexIndices();

            for (var vertexIndex = 0; vertexIndex < Vertices.Count; vertexIndex++)
            {
                var vertex = Vertices[vertexIndex];
                float y;
                if (vertex.Position.y < 0.1)
                {
                    y = vertex.Position.y - amount;
                }
                else if (vertex.Position.y > -0.1)
                {
                    y = vertex.Position.y + amount;
                }
                else
                {
                    y = vertex.Position.y;
                }


                var newPos = new Vector3(vertex.Position.x, y, vertex.Position.z);
                vertexPoints.Add(newPos);
            }

            var conway = new ConwayPoly(vertexPoints, faceIndices, FaceRoles, VertexRoles, FaceTags);
            return conway;
        }

        public ConwayPoly FaceSlide(OpParams o)
        {
            var tagList = StringToTagList(o.tags);
            var poly = Duplicate();
            for (var faceIndex = 0; faceIndex < Faces.Count; faceIndex++)
            {
                var face = poly.Faces[faceIndex];
                if (!IncludeFace(faceIndex, o.facesel, tagList, o.filterFunc)) continue;
                var faceNormal = face.Normal;
                //var amount = amount * (float) (randomize ? random.NextDouble() : 1);
                var faceVerts = face.GetVertices();
                for (var vertexIndex = 0; vertexIndex < faceVerts.Count; vertexIndex++)
                {
                    float amount = o.GetValueA(this, vertexIndex);
                    float direction = o.GetValueB(this, vertexIndex);
                    var vertexPos = faceVerts[vertexIndex].Position;

                    Vector3 tangent, tangentLeft, tangentUp, t1, t2;

                    t1 = Vector3.Cross(faceNormal, Vector3.forward);
                    t2 = Vector3.Cross(faceNormal, Vector3.left);
                    if (t1.magnitude > t2.magnitude)
                    {
                        tangentUp = t1;
                    }
                    else
                    {
                        tangentUp = t2;
                    }

                    t2 = Vector3.Cross(faceNormal, Vector3.up);
                    if (t1.magnitude > t2.magnitude)
                    {
                        tangentLeft = t1;
                    }
                    else
                    {
                        tangentLeft = t2;
                    }

                    tangent = Vector3.SlerpUnclamped(tangentUp, tangentLeft, direction);

                    var vector = tangent * (amount * (float) (o.randomize ? random.NextDouble() : 1));
                    var newPos = vertexPos + vector;
                    faceVerts[vertexIndex].Position = newPos;
                }
            }

            return poly;
        }

        public ConwayPoly VertexScale(OpParams o)
        {
            var tagList = StringToTagList(o.tags);
            var vertexPoints = new List<Vector3>();
            var faceIndices = ListFacesByVertexIndices();

            for (var vertexIndex = 0; vertexIndex < Vertices.Count; vertexIndex++)
            {
                float scale = o.GetValueA(this, vertexIndex);
                var _scale = scale * (o.randomize ? random.NextDouble() : 1) + 1;
                var vertex = Vertices[vertexIndex];
                var includeVertex = IncludeVertex(vertexIndex, o.facesel, tagList, o.filterFunc);
                vertexPoints.Add(includeVertex ? vertex.Position * (float) _scale : vertex.Position);
            }

            return new ConwayPoly(vertexPoints, faceIndices, FaceRoles, VertexRoles, FaceTags);
        }

        public ConwayPoly VertexFlex(OpParams o)
        {
            var tagList = StringToTagList(o.tags);
            var poly = Duplicate();
            for (var faceIndex = 0; faceIndex < Faces.Count; faceIndex++)
            {
                var face = poly.Faces[faceIndex];
                if (!IncludeFace(faceIndex, o.facesel, tagList, o.filterFunc)) continue;
                var faceCentroid = face.Centroid;
                var faceVerts = face.GetVertices();
                for (var vertexIndex = 0; vertexIndex < faceVerts.Count; vertexIndex++)
                {
                    float scale = o.GetValueA(this, vertexIndex);
                    var vertexPos = faceVerts[vertexIndex].Position;
                    float _scale = scale * (o.randomize ? (float) random.NextDouble() : 1f) + 1f;
                    var newPos = vertexPos + (vertexPos - faceCentroid) * _scale;
                    faceVerts[vertexIndex].Position = newPos;
                }
            }

            return poly;
        }

        public ConwayPoly VertexRotate(OpParams o)
        {
            var tagList = StringToTagList(o.tags);
            var poly = Duplicate();
            for (var faceIndex = 0; faceIndex < Faces.Count; faceIndex++)
            {
                float amount = o.GetValueA(this, faceIndex);
                var face = poly.Faces[faceIndex];
                if (!IncludeFace(faceIndex, o.facesel, tagList, o.filterFunc)) continue;
                var faceCentroid = face.Centroid;
                var direction = face.Normal;
                amount = amount * (float) (o.randomize ? random.NextDouble() : 1);
                var _angle = (360f / face.Sides) * amount;
                var faceVerts = face.GetVertices();
                for (var vertexIndex = 0; vertexIndex < faceVerts.Count; vertexIndex++)
                {
                    var vertexPos = faceVerts[vertexIndex].Position;
                    var rot = Quaternion.AngleAxis(_angle, direction);
                    var newPos = faceCentroid + rot * (vertexPos - faceCentroid);
                    faceVerts[vertexIndex].Position = newPos;
                }
            }

            return poly;
        }

        public ConwayPoly FaceScale(OpParams o)
        {
            var tagList = StringToTagList(o.tags);
            var vertexPoints = new List<Vector3>();
            var faceIndices = new List<IEnumerable<int>>();

            var faceRoles = new List<Roles>();
            var vertexRoles = new List<Roles>();

            for (var faceIndex = 0; faceIndex < Faces.Count; faceIndex++)
            {
                float scale = o.GetValueA(this, faceIndex);
                var _scale = scale * (o.randomize ? random.NextDouble() : 1) + 1;
                var face = Faces[faceIndex];
                var includeFace = IncludeFace(faceIndex, o.facesel, tagList, o.filterFunc);
                int c = vertexPoints.Count;

                vertexPoints.AddRange(face.GetVertices()
                    .Select(v =>
                        includeFace ? Vector3.LerpUnclamped(face.Centroid, v.Position, (float) _scale) : v.Position));
                var faceVerts = new List<int>();
                for (int ii = 0; ii < face.GetVertices().Count; ii++)
                {
                    faceVerts.Add(c + ii);
                }

                faceIndices.Add(faceVerts);
                faceRoles.Add(includeFace ? FaceRoles[faceIndex] : Roles.Ignored);
                var vertexRole = includeFace ? Roles.Existing : Roles.Ignored;
                vertexRoles.AddRange(Enumerable.Repeat(vertexRole, faceVerts.Count));
            }

            return new ConwayPoly(vertexPoints, faceIndices, faceRoles, vertexRoles, FaceTags);
        }

        public ConwayPoly FaceRotate(OpParams o, int axis = 1)
        {
            var tagList = StringToTagList(o.tags);
            var vertexPoints = new List<Vector3>();
            var faceIndices = new List<IEnumerable<int>>();

            var faceRoles = new List<Roles>();
            var vertexRoles = new List<Roles>();


            for (var faceIndex = 0; faceIndex < Faces.Count; faceIndex++)
            {
                float amount = o.GetValueA(this, faceIndex);
                var face = Faces[faceIndex];
                amount = amount * (float) (o.randomize ? random.NextDouble() : 1);
                var _angle = (360f / face.Sides) * amount;

                var includeFace = IncludeFace(faceIndex, o.facesel, tagList, o.filterFunc);

                int c = vertexPoints.Count;
                var faceVertices = new List<int>();

                c = vertexPoints.Count;

                var pivot = face.Centroid;
                Vector3 direction = face.Normal;
                switch (axis)
                {
                    case 1:
                        direction = Vector3.Cross(face.Normal, Vector3.up);
                        break;
                    case 2:
                        direction = Vector3.Cross(face.Normal, Vector3.forward);
                        break;
                }

                var rot = Quaternion.AngleAxis((float) _angle, direction);

                vertexPoints.AddRange(
                    face.GetVertices().Select(
                        v => includeFace ? pivot + rot * (v.Position - pivot) : v.Position
                    )
                );
                faceVertices = new List<int>();
                for (int ii = 0; ii < face.GetVertices().Count; ii++)
                {
                    faceVertices.Add(c + ii);
                }

                faceIndices.Add(faceVertices);
                faceRoles.Add(includeFace ? FaceRoles[faceIndex] : Roles.Ignored);
                var vertexRole = includeFace ? Roles.Existing : Roles.Ignored;
                vertexRoles.AddRange(Enumerable.Repeat(vertexRole, faceVertices.Count));
            }

            return new ConwayPoly(vertexPoints, faceIndices, faceRoles, vertexRoles, FaceTags);
        }

        public ConwayPoly VertexRemove(OpParams o, bool invertLogic)
        {
            var tagList = StringToTagList(o.tags);
            var allFaceIndices = new List<List<int>>();
            var faceRoles = new List<Roles>();
            var vertexRoles = new List<Roles>();

            int vertexCount = 0;

            var faces = ListFacesByVertexIndices();
            for (var i = 0; i < faces.Length; i++)
            {
                var oldFaceIndices = faces[i];
                var newFaceIndices = new List<int>();
                for (var idx = 0; idx < oldFaceIndices.Count; idx++)
                {
                    var vertexIndex = oldFaceIndices[idx];
                    bool keep = IncludeVertex(vertexIndex, o.facesel, tagList, o.filterFunc);
                    keep = invertLogic ? !keep : keep;
                    if (!keep)
                    {
                        newFaceIndices.Add(vertexIndex);
                        vertexCount++;
                    }
                }

                if (newFaceIndices.Count > 2)
                {
                    allFaceIndices.Add(newFaceIndices);
                }
            }

            faceRoles.AddRange(Enumerable.Repeat(Roles.Existing, allFaceIndices.Count));
            vertexRoles.AddRange(Enumerable.Repeat(Roles.Existing, vertexCount));
            return new ConwayPoly(Vertices.Select(x => x.Position), allFaceIndices, faceRoles, vertexRoles, FaceTags);
        }

        public ConwayPoly Collapse(FaceSelections vertexsel, bool invertLogic, Func<FilterParams, bool> filter = null)
        {
            var poly = VertexRemove(new OpParams {facesel = vertexsel}, invertLogic);
            poly.FillHoles();
            return poly;
        }

        public ConwayPoly Layer(OpParams o, int layers)
        {
            var poly = Duplicate();
            var layer = Duplicate();
            for (int i = 0; i <= layers; i++)
            {
                var newLayer = layer.Duplicate();
                newLayer = newLayer.FaceScale(o);
                newLayer = newLayer.Offset(o);
                poly.Append(newLayer);
                layer = newLayer;
            }

            return poly;
        }

        public ConwayPoly FaceRemove(OpParams o)
        {
            return _FaceRemove(o, false);
        }

        public ConwayPoly FaceKeep(OpParams o)
        {
            return _FaceRemove(o, true);
        }

        public ConwayPoly FaceRemove(FaceSelections facesel, string tags)
        {
            return _FaceRemove(new OpParams {facesel = facesel, tags = tags}, false);
        }

        public ConwayPoly FaceKeep(FaceSelections facesel, string tags)
        {
            return _FaceRemove(new OpParams {facesel = facesel, tags = tags}, true);
        }

        public ConwayPoly FaceRemove(bool invertLogic, List<int> faceIndices)
        {
            Func<FilterParams, bool> filter = x => faceIndices.Contains(x.index);
            return _FaceRemove(new OpParams {filterFunc = filter}, invertLogic);
        }

        public ConwayPoly _FaceRemove(OpParams o, bool invertLogic = false)
        {
            var tagList = StringToTagList(o.tags);
            var faceRoles = new List<Roles>();
            var vertexRoles = new List<Roles>();
            var facesToRemove = new List<Face>();
            var newPoly = Duplicate();
            var faceIndices = ListFacesByVertexIndices();
            var existingFaceRoles = new Dictionary<Vector3, Roles>();
            var existingVertexRoles = new Dictionary<Vector3, Roles>();

            for (int faceIndex = 0; faceIndex < Faces.Count; faceIndex++)
            {
                var face = Faces[faceIndex];
                bool removeFace;
                removeFace = IncludeFace(faceIndex, o.facesel, tagList, o.filterFunc);
                removeFace = invertLogic ? !removeFace : removeFace;
                if (removeFace)
                {
                    facesToRemove.Add(newPoly.Faces[faceIndex]);
                }
                else
                {
                    existingFaceRoles[face.Centroid] = FaceRoles[faceIndex];
                    var verts = face.GetVertices();
                    for (var vertIndex = 0; vertIndex < verts.Count; vertIndex++)
                    {
                        var vert = verts[vertIndex];
                        existingVertexRoles[vert.Position] = VertexRoles[faceIndices[faceIndex][vertIndex]];
                    }
                }
            }

            var newFaceTags = new List<HashSet<Tuple<string, TagType>>>();

            for (var i = 0; i < facesToRemove.Count; i++)
            {
                var face = facesToRemove[i];
                newPoly.Faces.Remove(face);
            }

            newPoly.CullUnusedVertices();

            for (var faceIndex = 0; faceIndex < newPoly.Faces.Count; faceIndex++)
            {
                var face = newPoly.Faces[faceIndex];
                faceRoles.Add(existingFaceRoles[face.Centroid]);
                newFaceTags.Add(FaceTags[faceIndex]);
            }

            newPoly.FaceRoles = faceRoles;

            for (var i = 0; i < newPoly.Vertices.Count; i++)
            {
                var vert = newPoly.Vertices[i];
                vertexRoles.Add(existingVertexRoles[vert.Position]);
            }

            newPoly.VertexRoles = vertexRoles;
            newPoly.FaceTags = newFaceTags;
            return newPoly;
        }
        

        public (ConwayPoly newPoly1, ConwayPoly newPoly2) Split(OpParams o)
        {
            
            // Essentially the same code as _FaceRemove but we keep both polys
            // Both methods could probably be combined into a single one
            // but it didn't seem worth the extra complexity
            // Of course - you could just do:
            // var poly1 = _FaceRemove(o);
            // var poly2 = _FaceRemove(o, true);
            // return (poly1, poly2);
            // But that's slower - especially on complex polys.
            
            var tagList = StringToTagList(o.tags);
            var existingFaceIndices = ListFacesByVertexIndices();
            var existingFaceRoles = new Dictionary<Vector3, Roles>();
            var existingVertexRoles = new Dictionary<Vector3, Roles>();

            var newPoly1 = Duplicate();
            var faceList1 = new List<Face>();
            var faceRoles1 = new List<Roles>();
            var vertexRoles1 = new List<Roles>();
            var newFaceTags1 = new List<HashSet<Tuple<string, TagType>>>();

            var newPoly2 = Duplicate();
            var faceList2 = new List<Face>();
            var faceRoles2 = new List<Roles>();
            var vertexRoles2 = new List<Roles>();
            var newFaceTags2 = new List<HashSet<Tuple<string, TagType>>>();
            
            for (int faceIndex = 0; faceIndex < Faces.Count; faceIndex++)
            {
                var face = Faces[faceIndex];
                existingFaceRoles[face.Centroid] = FaceRoles[faceIndex];
                var verts = face.GetVertices();
                for (var vertIndex = 0; vertIndex < verts.Count; vertIndex++)
                {
                    var vert = verts[vertIndex];
                    existingVertexRoles[vert.Position] = VertexRoles[existingFaceIndices[faceIndex][vertIndex]];
                }
                
                if (IncludeFace(faceIndex, o.facesel, tagList, o.filterFunc))
                {
                    faceList1.Add(newPoly1.Faces[faceIndex]);
                }
                else
                {
                    faceList2.Add(newPoly2.Faces[faceIndex]);
                }
            }

            foreach (var t in faceList1) newPoly1.Faces.Remove(t);
            newPoly1.CullUnusedVertices();

            foreach (var t in faceList2) newPoly2.Faces.Remove(t);
            newPoly2.CullUnusedVertices();
            
            for (var faceIndex = 0; faceIndex < newPoly1.Faces.Count; faceIndex++)
            {
                faceRoles1.Add(existingFaceRoles[newPoly1.Faces[faceIndex].Centroid]);
                newFaceTags1.Add(FaceTags[faceIndex]);
            }
            newPoly1.FaceRoles = faceRoles1;

            for (var faceIndex = 0; faceIndex < newPoly2.Faces.Count; faceIndex++)
            {
                faceRoles2.Add(existingFaceRoles[newPoly2.Faces[faceIndex].Centroid]);
                newFaceTags2.Add(FaceTags[faceIndex]);
            }
            newPoly2.FaceRoles = faceRoles2;
            
            for (var i = 0; i < newPoly1.Vertices.Count; i++)
            {
                var vert = newPoly1.Vertices[i];
                vertexRoles1.Add(existingVertexRoles[vert.Position]);
            }
            newPoly1.VertexRoles = vertexRoles1;
            newPoly1.FaceTags = newFaceTags1;
            
            for (var i = 0; i < newPoly2.Vertices.Count; i++)
            {
                var vert = newPoly2.Vertices[i];
                vertexRoles2.Add(existingVertexRoles[vert.Position]);
            }
            newPoly2.VertexRoles = vertexRoles2;
            newPoly2.FaceTags = newFaceTags2;

            return (newPoly1, newPoly2);
        }

        /// <summary>
        /// Offsets a mesh by moving each vertex by the specified distance along its normal vector.
        /// </summary>
        /// <param name="offset">Offset distance</param>
        /// <returns>The offset mesh</returns>
        public ConwayPoly Offset(float offset, bool randomize)
        {
            var offsetList = Enumerable.Range(0, Vertices.Count).Select(i => offset).ToList();
            return Offset(offsetList, randomize);
        }

        public ConwayPoly Offset(OpParams o)
        {
            // This will only work if the faces are split and don't share vertices

            var tagList = StringToTagList(o.tags);
            var offsetList = new List<float>();

            for (var faceIndex = 0; faceIndex < Faces.Count; faceIndex++)
            {
                float offset = o.GetValueA(this, faceIndex);
                if (o.randomize) offset = (float) random.NextDouble() * offset;
                var vertexOffset = IncludeFace(faceIndex, o.facesel, tagList, o.filterFunc) ? offset : 0;
                for (var i = 0; i < Faces[faceIndex].GetVertices().Count; i++)
                {
                    offsetList.Add(vertexOffset);
                }
            }

            return Offset(offsetList, o.randomize);
        }

        public ConwayPoly FaceMerge(OpParams o)
        {
            // TODO Breaks if the poly already has holes.
            var newPoly = Duplicate();
            newPoly = newPoly.FaceRemove(o);
            // Why do we do this?
            newPoly = newPoly.FaceRemove(new OpParams {facesel = FaceSelections.Outer});
            newPoly = newPoly.FillHoles();
            return newPoly;
        }

        public ConwayPoly Offset(List<float> offset, bool randomize)
        {
            Vector3[] points = new Vector3[Vertices.Count];
            float _offset;
            var faceOffsets = new Dictionary<string, float>();

            for (int i = 0; i < Vertices.Count && i < offset.Count; i++)
            {
                var vert = Vertices[i];
                if (randomize)
                {
                    if (faceOffsets.ContainsKey(vert.Halfedge.Face.Name))
                    {
                        _offset = faceOffsets[vert.Halfedge.Face.Name];
                    }
                    else
                    {
                        _offset = (float) random.NextDouble() * offset[i];
                        faceOffsets[vert.Halfedge.Face.Name] = _offset;
                    }
                }
                else
                {
                    _offset = offset[i];
                }

                points[i] = vert.Position + Vertices[i].Normal * (float) _offset;
            }

            return new ConwayPoly(points, ListFacesByVertexIndices(), FaceRoles, VertexRoles, FaceTags);
        }

        /// <summary>
        /// Gives thickness to mesh faces by offsetting the mesh and connecting naked edges with new faces.
        /// </summary>
        /// <param name="distance">Distance to offset the mesh (thickness)</param>
        /// <param name="symmetric">Whether to extrude in both (-ve and +ve) directions</param>
        /// <returns>The extruded mesh (always closed)</returns>
        public ConwayPoly Shell(float distance)
        {
            var offsetList = Enumerable.Repeat(distance, Vertices.Count).ToList();
            return Shell(new OpParams {funcA = x => offsetList[x.index]});
        }

        public ConwayPoly Shell(OpParams o, bool symmetric = true)
        {
            var newFaceTags = new List<HashSet<Tuple<string, TagType>>>();

            ConwayPoly result, top;

            if (symmetric)
            {
                if (o.funcA == null)
                {
                    result = Offset(new OpParams {randomize = o.randomize, valueA = -0.5f * o.valueA});
                    top = Offset(new OpParams {randomize = o.randomize, valueA = 0.5f * o.valueA});
                }
                else
                {
                    result = Offset(new OpParams {randomize = o.randomize, funcA = x => -0.5f * o.funcA(x)});
                    top = Offset(new OpParams {randomize = o.randomize, funcA = x => 0.5f * o.funcA(x)});
                }
            }
            else
            {
                result = Duplicate();
                top = Offset(o);
            }

            result.FaceRoles = Enumerable.Repeat(Roles.Existing, result.Faces.Count).ToList();
            result.VertexRoles = Enumerable.Repeat(Roles.Existing, result.Vertices.Count).ToList();
            newFaceTags.AddRange(FaceTags);

            result.Halfedges.Flip();

            // append top to ext (can't use Append() because copy would reverse face loops)
            foreach (var v in top.Vertices) result.Vertices.Add(v);
            foreach (var h in top.Halfedges) result.Halfedges.Add(h);
            for (var topFaceIndex = 0; topFaceIndex < top.Faces.Count; topFaceIndex++)
            {
                var f = top.Faces[topFaceIndex];
                result.Faces.Add(f);
                result.FaceRoles.Add(Roles.New);
                result.VertexRoles.AddRange(Enumerable.Repeat(Roles.New, f.Sides));
                newFaceTags.Add(new HashSet<Tuple<string, TagType>>(FaceTags[topFaceIndex]));
            }


            // get indices of naked halfedges in source mesh
            var naked = Halfedges.Select((item, index) => index).Where(i => Halfedges[i].Pair == null).ToList();

            if (naked.Count > 0)
            {
                int n = Halfedges.Count;
                int failed = 0;
                foreach (var i in naked)
                {
                    var newFaceTagSet = new HashSet<Tuple<string, TagType>>();
                    Vertex[] vertices =
                    {
                        result.Halfedges[i].Vertex,
                        result.Halfedges[i].Prev.Vertex,
                        result.Halfedges[i + n].Vertex,
                        result.Halfedges[i + n].Prev.Vertex
                    };

                    if (result.Faces.Add(vertices) == false)
                    {
                        failed++;
                    }
                    else
                    {
                        result.FaceRoles.Add(Roles.NewAlt);
                        int prevFaceIndex = result.Faces.IndexOf(result.Halfedges[i].Face);
                        var prevFaceTagSet = FaceTags[prevFaceIndex];
                        newFaceTagSet.UnionWith(prevFaceTagSet.Where(t => t.Item2 == TagType.Extrovert));
                        newFaceTags.Add(newFaceTagSet);
                    }
                }
            }

            result.FaceTags = newFaceTags;
            result.Halfedges.MatchPairs();

            return result;
        }

        public void ScalePolyhedra(float scale = 1)
        {
            if (Vertices.Count > 0)
            {
                // Find the furthest vertex
                Vertex max = Vertices.OrderByDescending(x => x.Position.sqrMagnitude).FirstOrDefault();
                float unitScale = 1.0f / max.Position.magnitude;
                for (var i = 0; i < Vertices.Count; i++)
                {
                    Vertices[i].Position *= unitScale * scale;
                }
            }
        }

        public ConwayPoly Slice(float lower, float upper, string tags = "")
        {
            if (lower > upper) (upper, lower) = (lower, upper);
            float yMax = Vertices.Max(v => v.Position.y);
            float yMin = Vertices.Min(v => v.Position.y);
            lower = Mathf.Lerp(yMin, yMax, lower);
            upper = Mathf.Lerp(yMin, yMax, upper);
            Func<FilterParams, bool> slice = x =>
                x.poly.Faces[x.index].Centroid.y > lower && x.poly.Faces[x.index].Centroid.y < upper;
            return _FaceRemove(new OpParams {facesel = FaceSelections.All, tags = tags, filterFunc = slice}, true);
        }

        public ConwayPoly Weld(float distance)
        {
            if (distance < .00001f)
                distance = .00001f; // We always weld by a very small amount. Disable the op if you don't want to weld at all.
            var vertexPoints = new List<Vector3>();
            var faceIndices = new List<IEnumerable<int>>();
            var faceRoles = new List<Roles>();
            var vertexRoles = new List<Roles>();
            var reverseDict = new Dictionary<Vertex, int>();
            var vertexReplacementDict = new Dictionary<int, int>();

            var groups = new List<Vertex[]>();
            var checkedVerts = new HashSet<Vertex>();

            InitOctree();

            for (var i = 0; i < Vertices.Count; i++)
            {
                var v = Vertices[i];
                reverseDict[v] = i;
                if (checkedVerts.Contains(v)) continue;
                checkedVerts.Add(v);
                var neighbours = FindNeighbours(v, distance);
                if (neighbours.Length < 1) continue;
                groups.Add(neighbours);
                checkedVerts.UnionWith(neighbours);
            }

            foreach (var group in groups)
            {
                vertexPoints.Add(@group[0].Position);
                int VertToKeep = -1;
                for (var i = 0; i < @group.Length; i++)
                {
                    var vertIndex = reverseDict[@group[i]];
                    if (i == 0)
                    {
                        VertToKeep = vertexPoints.Count - 1;
                    }

                    vertexReplacementDict[vertIndex] = VertToKeep;
                }
            }

            foreach (var faceVertIndices in ListFacesByVertexIndices())
            {
                var newFaceVertIndices = new List<int>();
                foreach (var vertIndex in faceVertIndices)
                {
                    newFaceVertIndices.Add(vertexReplacementDict[vertIndex]);
                }

                faceIndices.Add(newFaceVertIndices);
            }

            faceRoles = Enumerable.Repeat(Roles.New, faceIndices.Count).ToList();
            vertexRoles = Enumerable.Repeat(Roles.New, vertexPoints.Count).ToList();

            return new ConwayPoly(vertexPoints, faceIndices, faceRoles, vertexRoles, FaceTags);
        }

        public ConwayPoly FillHoles()
        {
            var result = Duplicate();
            var boundaries = result.FindBoundaries();
            foreach (var boundary in boundaries)
            {
                if (boundary.Count < 3) continue;
                var success = result.Faces.Add(boundary.Select(x => x.Vertex));
                if (!success)
                {
                    boundary.Reverse();
                    success = result.Faces.Add(boundary.Select(x => x.Vertex));
                }

                if (success)
                {
                    result.FaceRoles.Add(Roles.New);
                    result.FaceTags.Add(new HashSet<Tuple<string, TagType>>());
                }
            }
            result.Halfedges.MatchPairs();
            return result;
        }

        public ConwayPoly AppendMany(ConwayPoly stashed, FaceSelections facesel, string tags = "", float scale = 1,
            float angle = 0, float offset = 0, bool toFaces = true, Func<FilterParams, bool> filter = null)
        {
            var tagList = StringToTagList(tags);
            var result = Duplicate();

            if (toFaces)
            {
                for (var i = 0; i < Faces.Count; i++)
                {
                    var face = Faces[i];
                    if (IncludeFace(i, facesel, tagList, filter))
                    {
                        Vector3 transform = face.Centroid + face.Normal * offset;
                        var rot = Quaternion.AngleAxis(angle, face.Normal);
                        result.Append(stashed, transform, rot, scale);
                    }
                }
            }
            else
            {
                for (var vertexIndex = 0; vertexIndex < Vertices.Count; vertexIndex++)
                {
                    var vert = Vertices[vertexIndex];
                    if (IncludeVertex(vertexIndex, facesel, tagList, filter))
                    {
                        Vector3 transform = vert.Position + vert.Normal * offset;
                        var rot = Quaternion.AngleAxis(angle, vert.Normal);
                        result.Append(stashed, transform, rot, scale);
                    }
                }
            }

            return result;
        }

        public ConwayPoly PolyArray(List<Vector3> positionList, List<Vector3> directionList, List<Vector3> scaleList)
        {
            var result = new ConwayPoly();

            for (var i = 0; i < positionList.Count; i++)
            {
                Vector3 transform = positionList[i];
                var rot = Quaternion.AngleAxis(0, directionList[i]);
                result.Append(this, transform, rot, scaleList[i]);
            }

            return result;
        }

        /// <summary>
        /// Appends a copy of another mesh to this one.
        /// </summary>
        /// <param name="other">Mesh to append to this one.</param>
        public void Append(ConwayPoly other)
        {
            Append(other, Vector3.zero, Quaternion.identity, 1.0f);
        }

        public void Append(ConwayPoly other, Vector3 transform, Quaternion rotation, float scale)
        {
            Append(other, transform, rotation, Vector3.one * scale);
        }

        public void Append(ConwayPoly other, Vector3 transform, Quaternion rotation, Vector3 scale)
        {
            if (other == null) return;
            ConwayPoly dup = other.Duplicate(transform, rotation, scale);

            Vertices.AddRange(dup.Vertices);
            for (var i = 0; i < dup.Halfedges.Count; i++)
            {
                Halfedges.Add(dup.Halfedges[i]);
            }

            for (var i = 0; i < dup.Faces.Count; i++)
            {
                Faces.Add(dup.Faces[i]);
            }

            FaceRoles.AddRange(dup.FaceRoles);
            VertexRoles.AddRange(dup.VertexRoles);
            FaceTags.AddRange(dup.FaceTags);
        }

        public ConwayPoly Duplicate()
        {
            // Export to face/vertex and rebuild
            return new ConwayPoly(ListVerticesByPoints(), ListFacesByVertexIndices(), FaceRoles, VertexRoles, FaceTags);
        }

        public ConwayPoly Duplicate(Vector3 transform, Quaternion rotation, float scale)
        {
            IEnumerable<Vector3> verts;

            if (transform == Vector3.zero && rotation == Quaternion.identity && scale == 1.0f)
            {
                // Fast path
                verts = ListVerticesByPoints();
            }
            else
            {
                verts = ListVerticesByPoints().Select(i => rotation * i * scale + transform);
            }

            return new ConwayPoly(verts, ListFacesByVertexIndices(), FaceRoles, VertexRoles, FaceTags);
        }

        public ConwayPoly Duplicate(Vector3 transform, Quaternion rotation, Vector3 scale)
        {
            return Transform(transform, rotation.eulerAngles, scale);
        }

        public void _ExtendBoundaries(List<List<Halfedge>> boundaries, float scale, float angle=0)
        {
            FaceRoles = Enumerable.Repeat(Roles.Existing, Faces.Count).ToList();
            var newFaceTags = new List<HashSet<Tuple<string, TagType>>>();
            newFaceTags.AddRange(FaceTags);

            foreach (var boundary in boundaries)
            {
                int firstNewVertexIndex = Vertices.Count;
                for (var edgeIndex = 0; edgeIndex < boundary.Count; edgeIndex++)
                {
                    var edge1 = boundary[edgeIndex];
                    var direction1 = edge1.Midpoint - edge1.Face.Centroid;
                    var edge2 = boundary[(edgeIndex + boundary.Count - 1) % boundary.Count];
                    var direction2 = edge2.Midpoint - edge2.Face.Centroid;
                    var direction = direction1 == direction2 ? direction1 : direction1 + direction2;
                    var normal = (edge1.Face.Normal + edge2.Face.Normal) / 2f;
                    direction = Vector3.LerpUnclamped(direction, normal, angle / 90f).normalized;
                    Vertices.Add(new Vertex(edge1.Vertex.Position + (direction * scale)));
                    VertexRoles.Add(Roles.New);
                }

                for (var edgeIndex = 0; edgeIndex < boundary.Count; edgeIndex++)
                {
                    var edge = boundary[edgeIndex];
                    bool success = Faces.Add(new[]
                    {
                        edge.Vertex,
                        edge.Prev.Vertex,
                        Vertices[firstNewVertexIndex + ((edgeIndex + 1) % boundary.Count)],
                        Vertices[firstNewVertexIndex + edgeIndex],
                    });
                    if (success)
                    {
                        FaceRoles.Add(Roles.New);
                        var prevFaceTagSet = FaceTags[Faces.IndexOf(edge.Face)];
                        FaceTags.Add(prevFaceTagSet);
                    }
                }
            }
        }

        public ConwayPoly MergeCoplanarFaces(float threshold, int failSafeLimit=500)
        {
            // Sledgehammer approach. 
            // Could be vastly improved
            // Basically loops through deleting coplanar faces and filling the gaps
            // Gets very slow for big inputs
            
            var mergedPoly = Duplicate();
            int failSafe = 0;
            bool finished = false;
            int currentFaceIndex = 0;
            var faceIndicesToMerge = new List<int>();
            bool foundCoplanar = false;
            while (!finished)
            {
                var currentFace = mergedPoly.Faces[currentFaceIndex];
                foreach (var edge in currentFace.GetHalfedges())
                {
                    if (edge.Pair == null) continue;
                    var face = edge.Pair.Face;
                    if (Vector3.Angle(currentFace.Normal, face.Normal) <= threshold)
                    {
                        if (faceIndicesToMerge.Count==0) faceIndicesToMerge.Add(currentFaceIndex);
                        faceIndicesToMerge.Add(mergedPoly.Faces.IndexOf(face));  // TODO
                        foundCoplanar = true;
                    }
                }
                
                currentFaceIndex++;
                bool facesMerged = false;
                if (foundCoplanar)
                {
                    int faceCountBefore = mergedPoly.Faces.Count;
                    mergedPoly = mergedPoly.FaceRemove(false, faceIndicesToMerge);
                    mergedPoly = mergedPoly.FillHoles();
                    faceIndicesToMerge = new List<int>();
                    int faceCountAfter = mergedPoly.Faces.Count;
                    facesMerged = faceCountAfter < faceCountBefore;
                    if (facesMerged) currentFaceIndex = 0;
                }
                failSafe++;
                if (!foundCoplanar || !facesMerged)
                {
                    if (currentFaceIndex >= mergedPoly.Faces.Count)
                    {
                        // We've done an entire scan of the poly and not merged anything
                        finished = true;
                    }
                }
        
                if (failSafe > failSafeLimit) break;  // Avoid infinite loops
            }
        
            return mergedPoly;
        }

        // public ConwayPoly MergeCoplanarFaces(float threshold, int failSafeLimit=500)
        // {
        //     // Another aborted attempt
        //     // Promising but we need a way to construct a single face from a set of coplanar faces
        //     // Storing either face GUIDs or face indices is useless because they become invalid
        //     // after the first merge
        //     
        //     var mergedPoly = Duplicate();
        //     int failSafe = 0;
        //     bool finished = false;
        //     int currentFaceIndex = 0;
        //     var faceIndicesToMerge = new List<int>();
        //     bool foundCoplanar = false;
        //     var mergeList = new Dictionary<Vector3, HashSet<int>>();
        //     for (var faceIndex = 0; faceIndex < Faces.Count; faceIndex++)
        //     {
        //         var face = Faces[faceIndex];
        //         if (!mergeList.ContainsKey(face.Normal))
        //         {
        //             mergeList[face.Normal] = new HashSet<int>();
        //         }
        //         else
        //         {
        //             mergeList[face.Normal].Add(faceIndex);
        //         }
        //     }
        //
        //     foreach (var mergeSet in mergeList.Values)
        //     {
        //         // TODO
        //     }
        //
        //     return mergedPoly;
        // }

        // Initial sketch.
        // Put to one side in favour of a sledgehammer approach above
        // public ConwayPoly MergeCoplanarFaces(float threshold)
        // {
        //     var newFaces = new List<List<int>>();
        //     // Only handles triangular faces sharing one edge
        //     foreach (var face in Faces)
        //     {
        //         bool merge = false;
        //         var newFace = new List<int>();
        //         foreach (var edge in face.GetHalfedges())
        //         {
        //             if (edge.Pair == null) continue;
        //             if (Vector3.Angle(edge.Face.Normal, edge.Pair.Face.Normal) < threshold)
        //             {
        //
        //             }
        //         }
        //
        //         if (merge)
        //         {
        //             newFaces.Add(faceIndices);
        //         }
        //         else
        //         {
        //             newFaces.Add(faceIndices);
        //         }
        //     }
        //     return new ConwayPoly();
        // }

        public ConwayPoly ExtendBoundaries(OpParams o)
        {
            float amount = o.GetValueA(this, 0);
            float angle = o.GetValueB(this, 0);
            var poly = Duplicate();
            var boundaries = poly.FindBoundaries();
            poly._ExtendBoundaries(boundaries, amount, angle);
            return poly;
        }

        public ConwayPoly SplitLoop(List<Tuple<int, int>> loop)
        {
            var poly = Duplicate();
            var vertexRoles = poly.VertexRoles;
            var newFaceTags = poly.FaceTags;
            var faces = poly.Faces;
            int firstNewVertexIndex = poly.Vertices.Count;
            var newVertLookup = new Dictionary<(Guid, Guid)?, int>();
            foreach (var loopItem in loop)
            {
                var face = faces[loopItem.Item1];
                var edge = face.GetHalfedges()[loopItem.Item2];
                Vector3 pos = edge.Midpoint;
                var newVert = new Vertex(pos);
                poly.Vertices.Add(newVert);
                vertexRoles.Add(Roles.New);
                newVertLookup[edge.PairedName] = poly.Vertices.Count - 1;
            }

            if (loop.Count > 0)
            {
                var lastFace = faces[loop.Last().Item1];
                int lastEdgeIndex = loop.Last().Item2;
                lastEdgeIndex = ConwayPoly.ActualMod(lastEdgeIndex + (lastFace.Sides / 2), lastFace.Sides);
                var lastEdge = lastFace.GetHalfedges()[lastEdgeIndex];
                if (lastEdge.Pair == null)
                {
                    var lastNewVert = new Vertex(lastEdge.Midpoint);
                    poly.Vertices.Add(lastNewVert);
                    vertexRoles.Add(Roles.New);
                    newVertLookup[lastEdge.PairedName] = poly.Vertices.Count - 1;
                }
            }

            var facesToRemove = new List<int>();
            var facesToAdd = new List<List<int>>();
            var newFaceRoles = new List<Roles>();
            for (var loopIndex = 0; loopIndex < loop.Count; loopIndex++)
            {
                var loopItem = loop[loopIndex];
                int nextLoopIndex = (loopIndex + 1) % loop.Count;
                var nextLoopItem = loop[nextLoopIndex];

                var face = faces[loopItem.Item1];
                var initialEdge = face.GetHalfedges()[loopItem.Item2];
                var currentEdge = initialEdge;
                var nextFace = faces[nextLoopItem.Item1];
                var nextEdge = nextFace.GetHalfedges()[nextLoopItem.Item2];

                var faceHalfEdges = face.GetHalfedges();
                var newVert1 = poly.Vertices[firstNewVertexIndex + loopIndex];
                var newVert2 = poly.Vertices[firstNewVertexIndex + ActualMod(loopIndex + 1, loop.Count)];

                var newFace1 = new List<int>();
                int counter = 0;
                bool finished = false;
                while (!finished)
                {
                    newFace1.Add(poly.Vertices.IndexOf(currentEdge.Vertex));
                    currentEdge = currentEdge.Next;
                    counter++;
                    finished = counter >= face.Sides / 2;
                }

                if (newVertLookup.ContainsKey(currentEdge.PairedName))
                {
                    newFace1.Add(newVertLookup[currentEdge.PairedName]);
                }

                if (newVertLookup.ContainsKey(initialEdge.PairedName))
                {
                    newFace1.Add(newVertLookup[initialEdge.PairedName]);
                }

                initialEdge = currentEdge;
                var newFace2 = new List<int>();
                finished = false;
                while (!finished)
                {
                    newFace2.Add(poly.Vertices.IndexOf(currentEdge.Vertex));
                    currentEdge = currentEdge.Next;
                    counter++;
                    finished = counter >= face.Sides;
                }

                if (newVertLookup.ContainsKey(currentEdge.PairedName))
                {
                    newFace2.Add(newVertLookup[currentEdge.PairedName]);
                }

                if (newVertLookup.ContainsKey(initialEdge.PairedName))
                {
                    newFace2.Add(newVertLookup[initialEdge.PairedName]);
                }

                facesToRemove.Add(loopItem.Item1);

                if (newFace1.Count >= 3)
                {
                    facesToAdd.Add(newFace1);
                    newFaceRoles.Add(Roles.New);
                    var prevFaceTagSet = poly.FaceTags[loopItem.Item1];
                    newFaceTags.Add(new HashSet<Tuple<string, TagType>>(prevFaceTagSet));
                }

                if (newFace2.Count >= 3)
                {
                    facesToAdd.Add(newFace2);
                    newFaceRoles.Add(Roles.New);
                    var prevFaceTagSet = poly.FaceTags[loopItem.Item1];
                    newFaceTags.Add(new HashSet<Tuple<string, TagType>>(prevFaceTagSet));
                }
            }

            //var prevTags = new Dictionary<int, HashSet<Tuple<string, TagType>>>();
            var allVertices = poly.Vertices.Select(v => v.Position).ToList();
            var faceIndices = new List<List<int>>();
            var facesVertList = poly.ListFacesByVertexIndices();
            for (var faceIndex = 0; faceIndex < facesVertList.Length; faceIndex++)
            {
                if (!facesToRemove.Contains(faceIndex)) faceIndices.Add(facesVertList[faceIndex]);
            }
            var faceRoles = Enumerable.Repeat(Roles.Existing, faceIndices.Count).ToList();
            faceIndices.AddRange(facesToAdd);
            faceRoles.AddRange(newFaceRoles);

            return new ConwayPoly(allVertices, faceIndices, faceRoles, vertexRoles);
        }

        public List<Tuple<int,int>> GetFaceLoop(Halfedge startingEdge)
        {
            var loop = new List<Tuple<int,int>>();
            if (startingEdge.Face.Sides % 2 != 0) return loop;
            var faceLookup = new  Dictionary<string, int>();
            for (var faceIndex = 0; faceIndex < Faces.Count; faceIndex++)
            {
                var face = Faces[faceIndex];
                faceLookup[face.Name] = faceIndex;
            }
            
            bool finished = false;
            int failsafe = 0;
            var currentEdge = startingEdge;
            int edgeCounter = 0;
            while (!finished)
            {
                loop.Add(new Tuple<int, int>(
                    faceLookup[currentEdge.Face.Name],
                    currentEdge.Face.GetHalfedges().IndexOf(currentEdge)
                ));
                var currentFace = currentEdge.Face;
                edgeCounter = 0;
                while (edgeCounter < currentFace.Sides / 2)
                {
                    currentEdge = currentEdge.Next;
                    edgeCounter++;
                }
                currentEdge = currentEdge.Pair;
                failsafe++;
                finished = currentEdge == null ||
                           currentEdge.Face.Sides % 2 != 0 ||
                           currentEdge == startingEdge ||
                           failsafe > 100;
            }

            if (currentEdge == null && startingEdge.Pair!=null)
            {
                currentEdge = startingEdge.Pair;
                finished = false;
                
                while (!finished)
                {
                    var currentFace = currentEdge.Face;
                    edgeCounter = 0;
                    while (edgeCounter < currentFace.Sides / 2)
                    {
                        currentEdge = currentEdge.Next;
                        edgeCounter++;
                    }
                    loop.Insert(0, new Tuple<int, int>(
                        faceLookup[currentEdge.Face.Name],
                        currentEdge.Face.GetHalfedges().IndexOf(currentEdge)
                    ));
                    currentEdge = currentEdge.Pair;
                    failsafe++;
                    finished = currentEdge == null ||
                               currentEdge.Face.Sides % 2 != 0 ||
                               currentEdge == startingEdge ||
                               failsafe > 100;
                    
                }
                
            }

            return loop;
        }

        public ConwayPoly Dome(FaceSelections facesel, float height=1f, int segments = 4, float curve1=0.1f, float curve2=0.1f)
        {
            
            // Bit hacky.
            // Would be better to extend Loft to accept an "interations" param
            
            var poly = Duplicate();
            for (float segmentIndex = 0; segmentIndex <= segments; segmentIndex++)
            {
                float i = (float)segmentIndex / segments;
                poly = poly.Loft(new OpParams
                {
                    valueA = 0.001f + (Mathf.Abs(Mathf.Sin(i * Mathf.PI * curve1))*curve2),
                    // valueB = height/segments,
                    valueB = (0.001f + (Mathf.Abs(Mathf.Cos(i * Mathf.PI * curve1))*0.05f) * height),
                    facesel = facesel,
                });
                
                facesel = FaceSelections.Existing;
            }
            return poly;
        }

        /// <summary>
		/// Thickens each mesh edge in the plane of the mesh surface.
		/// </summary>
		/// <param name="offset">Distance to offset edges in plane of adjacent faces</param>
		/// <param name="boundaries">If true, attempt to ribbon boundary edges</param>
		/// <returns>The ribbon mesh</returns>
		// public ConwayPoly Ribbon(float offset, Boolean boundaries, float smooth)
		// {
		//
		// 	ConwayPoly ribbon = Duplicate();
		// 	var orig_faces = ribbon.Faces.ToArray();
		//
		// 	List<List<Halfedge>> incidentEdges = ribbon.Vertices.Select(v => v.Halfedges).ToList();
		//
		// 	// create new "vertex" faces
		// 	List<List<Vertex>> all_new_vertices = new List<List<Vertex>>();
		// 	for (int k = 0; k < Vertices.Count; k++)
		// 	{
		// 		Vertex v = ribbon.Vertices[k];
		// 		List<Vertex> new_vertices = new List<Vertex>();
		// 		List<Halfedge> halfedges = incidentEdges[k];
		// 		Boolean boundary = halfedges[0].Next.Pair != halfedges[halfedges.Count - 1];
		//
		// 		// if the edge loop around this vertex is open, close it with 'temporary edges'
		// 		if (boundaries && boundary)
		// 		{
		// 			Halfedge a, b;
		// 			a = halfedges[0].Next;
		// 			b = halfedges[halfedges.Count - 1];
		// 			if (a.Pair == null)
		// 			{
		// 				a.Pair = new Halfedge(a.Prev.Vertex) {Pair = a};
		// 			}
		//
		// 			if (b.Pair == null)
		// 			{
		// 				b.Pair = new Halfedge(b.Prev.Vertex) {Pair = b};
		// 			}
		//
		// 			a.Pair.Next = b.Pair;
		// 			b.Pair.Prev = a.Pair;
		// 			a.Pair.Prev = a.Pair.Prev ?? a; // temporary - to allow access to a.Pair's start/end vertices
		// 			halfedges.Add(a.Pair);
		// 		}
		//
		// 		foreach (Halfedge edge in halfedges)
		// 		{
		// 			if (halfedges.Count < 2)
		// 			{
		// 				continue;
		// 			}
		//
		// 			Vector3 normal = edge.Face != null ? edge.Face.Normal : Vertices[k].Normal;
		// 			Halfedge edge2 = edge.Next;
		//
		// 			var o1 = new Vertex(Vector3.Cross(normal, edge.Vector).normalized * offset);
		// 			var o2 = new Vertex(Vector3.Cross(normal, edge2.Vector).normalized * offset);
		//
		// 			if (edge.Face == null)
		// 			{
		// 				// boundary condition: create two new vertices in the plane defined by the vertex normal
		// 				Vertex v1 = new Vertex(v.Position + (edge.Vector * (1 / edge.Vector.magnitude) * -offset) +
		// 				                       o1.Position);
		// 				Vertex v2 = new Vertex(v.Position + (edge2.Vector * (1 / edge2.Vector.magnitude) * offset) +
		// 				                       o2.Position);
		// 				ribbon.Vertices.Add(v2);
		// 				ribbon.Vertices.Add(v1);
		// 				new_vertices.Add(v2);
		// 				new_vertices.Add(v1);
		// 				Halfedge c = new Halfedge(v2, edge2, edge, null);
		// 				edge.Next = c;
		// 				edge2.Prev = c;
		// 			}
		// 			else
		// 			{
		// 				// internal condition: offset each edge in the plane of the shared face and create a new vertex where they intersect eachother
		//
		// 				Vector3 start1 = edge.Vertex.Position + o1.Position;
		// 				Vector3 end1 = edge.Prev.Vertex.Position + o1.Position;
		// 				Line l1 = new Line(start1, end1);
		//
		// 				Vector3 start2 = edge2.Vertex.Position + o2.Position;
		// 				Vector3 end2 = edge2.Prev.Vertex.Position + o2.Position;
		// 				Line l2 = new Line(start2, end2);
		//
		// 				Vector3 intersection;
		// 				l1.Intersect(out intersection, l2);
		// 				ribbon.Vertices.Add(new Vertex(intersection));
		// 				new_vertices.Add(new Vertex(intersection));
		// 			}
		// 		}
		//
		// 		if ((!boundaries && boundary) == false) // only draw boundary node-faces in 'boundaries' mode
		// 			ribbon.Faces.Add(new_vertices);
		// 		all_new_vertices.Add(new_vertices);
		// 	}
		//
		// 	// change edges to reference new vertices
		// 	for (int k = 0; k < Vertices.Count; k++)
		// 	{
		// 		Vertex v = ribbon.Vertices[k];
		// 		if (all_new_vertices[k].Count < 1)
		// 		{
		// 			continue;
		// 		}
		//
		// 		int c = 0;
		// 		foreach (Halfedge edge in incidentEdges[k])
		// 		{
		// 			if (!ribbon.Halfedges.SetVertex(edge, all_new_vertices[k][c++]))
		// 				edge.Vertex = all_new_vertices[k][c];
		// 		}
		//
		// 		//v.Halfedge = null; // unlink from halfedge as no longer in use (culled later)
		// 		// note: new vertices don't link to any halfedges in the mesh until later
		// 	}
		//
		// 	// cull old vertices
		// 	ribbon.Vertices.RemoveRange(0, Vertices.Count);
		//
		// 	// use existing edges to create 'ribbon' faces
		// 	MeshHalfedgeList temp = new MeshHalfedgeList();
		// 	for (int i = 0; i < Halfedges.Count; i++)
		// 	{
		// 		temp.Add(ribbon.Halfedges[i]);
		// 	}
		//
		// 	List<Halfedge> items = temp.GetUnique();
		//
		// 	foreach (Halfedge halfedge in items)
		// 	{
		// 		if (halfedge.Pair != null)
		// 		{
		// 			// insert extra vertices close to the new 'vertex' vertices to preserve shape when subdividing
		// 			if (smooth > 0.0)
		// 			{
		// 				if (smooth > 0.5)
		// 				{
		// 					smooth = 0.5f;
		// 				}
		//
		// 				Vertex[] newVertices = new Vertex[]
		// 				{
		// 					new Vertex(halfedge.Vertex.Position + (-smooth * halfedge.Vector)),
		// 					new Vertex(halfedge.Prev.Vertex.Position + (smooth * halfedge.Vector)),
		// 					new Vertex(halfedge.Pair.Vertex.Position + (-smooth * halfedge.Pair.Vector)),
		// 					new Vertex(halfedge.Pair.Prev.Vertex.Position + (smooth * halfedge.Pair.Vector))
		// 				};
		// 				ribbon.Vertices.AddRange(newVertices);
		// 				Vertex[] new_vertices1 = new Vertex[]
		// 				{
		// 					halfedge.Vertex,
		// 					newVertices[0],
		// 					newVertices[3],
		// 					halfedge.Pair.Prev.Vertex
		// 				};
		// 				Vertex[] new_vertices2 = new Vertex[]
		// 				{
		// 					newVertices[1],
		// 					halfedge.Prev.Vertex,
		// 					halfedge.Pair.Vertex,
		// 					newVertices[2]
		// 				};
		// 				ribbon.Faces.Add(newVertices);
		// 				ribbon.Faces.Add(new_vertices1);
		// 				ribbon.Faces.Add(new_vertices2);
		// 			}
		// 			else
		// 			{
		// 				Vertex[] newVertices = new Vertex[]
		// 				{
		// 					halfedge.Vertex,
		// 					halfedge.Prev.Vertex,
		// 					halfedge.Pair.Vertex,
		// 					halfedge.Pair.Prev.Vertex
		// 				};
		//
		// 				ribbon.Faces.Add(newVertices);
		// 			}
		// 		}
		// 	}
		//
		// 	// remove original faces, leaving just the ribbon
		// 	//var orig_faces = Enumerable.Range(0, Faces.Count).Select(i => ribbon.Faces[i]);
		// 	foreach (Face item in orig_faces)
		// 	{
		// 		ribbon.Faces.Remove(item);
		// 	}
		//
		// 	// search and link pairs
		// 	ribbon.Halfedges.MatchPairs();
		//
		// 	return ribbon;
		// }
    }
}