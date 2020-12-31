using System;
using System.Collections.Generic;
using System.Linq;
using Conway;
using UnityEngine;
using Wythoff;
using Face = Conway.Face;


namespace Johnson
{
    public static class JohnsonPoly
    {
        private static ConwayPoly _MakePolygon(int sides, bool flip=false, float angleOffset = 0, float heightOffset = 0, float radius=1)
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

        private static ConwayPoly _MakeCupola(int sides, float height, bool bi=false, bool gyro=true)
        {

	        sides = Mathf.Clamp(sides, 3, 64);

            ConwayPoly poly = _MakePolygon(sides * 2);
            Face bottom = poly.Faces[0];
            ConwayPoly cap1 = _MakePolygon(sides, true, 0.25f, height, _CalcCupolaCapRadius(sides));
            poly.Append(cap1);

            int i = 0;
            var squareSideFaces = new List<Face>();
            var edge1 = poly.Halfedges[0];
            var edge2 = poly.Halfedges[sides * 2];
            while (true)
            {
                var side1 = new List<Vertex>
                {
                    edge1.Next.Vertex,
                    edge1.Vertex,
                    edge2.Prev.Vertex
                };
                poly.Faces.Add(side1);
                poly.FaceRoles.Add(ConwayPoly.Roles.New);
                poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());

                var side2 = new List<Vertex>
                {
                    edge1.Vertex,
                    edge1.Prev.Vertex,
                    edge2.Vertex,
                    edge2.Prev.Vertex
                };
                poly.Faces.Add(side2);
                squareSideFaces.Add(poly.Faces.Last());
                poly.FaceRoles.Add(ConwayPoly.Roles.NewAlt);
                poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());

                i++;
                edge1 = edge1.Next.Next;
                edge2 = edge2.Prev;
                if (i == sides) break;
            }

            if (bi)
            {
	            float angleOffset = gyro ? 0.75f : 0.25f;
                ConwayPoly cap2 = _MakePolygon(sides, false, angleOffset, -height, _CalcCupolaCapRadius(sides));
                poly.Append(cap2);

                i = 0;
                var middleVerts = bottom.GetVertices();

                poly.Faces.Remove(bottom);
                poly.FaceRoles.RemoveAt(0);

                edge2 = poly.Faces.Last().Halfedge.Prev;
                int indexOffset = gyro ? 0 : -1;
                while (true)
                {
                    var side1 = new List<Vertex>
                    {
                        middleVerts[PolyUtils.ActualMod(i * 2 - 1 + indexOffset, sides * 2)],
                        middleVerts[PolyUtils.ActualMod(i * 2 + indexOffset, sides * 2)],
                        edge2.Vertex
                    };
                    poly.Faces.Add(side1);
                    poly.FaceRoles.Add(ConwayPoly.Roles.New);
                    poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());

                    var side2 = new List<Vertex>
                    {
                        middleVerts[PolyUtils.ActualMod(i * 2 + indexOffset, sides * 2)],
                        middleVerts[PolyUtils.ActualMod(i * 2 + 1 + indexOffset, sides * 2)],
                        edge2.Next.Vertex,
                        edge2.Vertex,
                    };
                    poly.Faces.Add(side2);
                    poly.FaceRoles.Add(ConwayPoly.Roles.NewAlt);
                    poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());

                    i++;
                    edge2 = edge2.Next;

                    if (i == sides) break;

                }
            }

            poly.Halfedges.MatchPairs();
            return poly;
        }

        // Work in progress. Not working at the moment
        private static ConwayPoly _MakeRotunda(int sides, float height, bool bi=false)
        {

	        sides = Mathf.Clamp(sides, 3, 64);

            ConwayPoly poly = _MakePolygon(sides);
            Face bottom = poly.Faces[0];
            ConwayPoly cap1 = _MakePolygon(sides, true, 0.25f, height, _CalcCupolaCapRadius(sides));
            poly.Append(cap1);

            int i = 0;
//            var upperTriFaces = new List<Face>();
//            var LowerTriFaces = new List<Face>();
//            var SidePentFaces = new List<Face>();

            var edge1 = poly.Halfedges[0];
            var edge2 = poly.Halfedges[sides * 2];

            while (true)
            {
                poly.Vertices.Add(new Vertex(Vector3.Lerp(edge1.Vector, edge2.Vector, _CalcCupolaCapRadius(sides))));
                var newV1 = poly.Vertices.Last();
                poly.Vertices.Add(new Vertex(Vector3.Lerp(edge1.Prev.Vector, edge2.Next.Vector, 0.5f)));
                var newV2 = poly.Vertices.Last();

                var pentFace = new List<Vertex>
                {
                    edge1.Next.Vertex,
                    edge1.Vertex,
                    newV1,
                };
                poly.Faces.Add(pentFace);
                poly.FaceRoles.Add(ConwayPoly.Roles.New);
                poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());

                i++;
                edge1 = edge1.Next.Next;
                edge2 = edge2.Prev;
                if (i == sides) break;
            }

            poly.Halfedges.MatchPairs();
            return poly;
        }

        public static ConwayPoly ElongatedRotunda()
        {
	        int sides = 10;
	        float bodyHeight = _CalcSideLength(sides);
	        ConwayPoly poly = Rotunda();
	        poly = poly.Loft(new OpParams{valueB = bodyHeight, facesel = FaceSelections.FacingDown});
	        return poly;
        }

        public static ConwayPoly GyroelongatedRotunda()
        {
	        int sides = 10;
	        float bodyHeight = _CalcAntiprismHeight(sides);
	        ConwayPoly poly = Rotunda();
	        poly = poly.Lace(new OpParams{facesel = FaceSelections.FacingDown, valueA = 0, valueB = bodyHeight});
	        return poly;
        }

        public static ConwayPoly GyroelongatedBirotunda()
        {
	        int sides = 10;
	        float bodyHeight = _CalcAntiprismHeight(sides);
	        ConwayPoly poly = Rotunda();
	        ConwayPoly baseDome = poly.Duplicate();
	        var boundaryEdges1 = poly.Faces.Remove(poly.Faces.Last());

	        baseDome.Mirror(Vector3.up, -bodyHeight);
	        baseDome.Halfedges.Flip();
	        baseDome = baseDome.Rotate(Vector3.up, 36f / 2f);

	        poly.Append(baseDome);
	        var boundaryEdges2 = poly.Faces.Remove(poly.Faces.Last());
	        boundaryEdges2.Reverse();

	        for (var i = 0; i < boundaryEdges1.Count; i++)
	        {
		        var edge1 = boundaryEdges1[i];
		        var edge2 = boundaryEdges2[i];

		        var side1 = new List<Vertex>
		        {
			        edge1.Vertex,
			        edge1.Prev.Vertex,
			        edge2.Prev.Vertex
		        };
		        poly.Faces.Add(side1);
		        poly.FaceRoles.Add(ConwayPoly.Roles.New);
		        poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());

		        var side2 = new List<Vertex>
		        {
			        edge2.Vertex,
			        edge2.Prev.Vertex,
			        edge1.Prev.Vertex,
		        };
		        poly.Faces.Add(side2);
		        poly.FaceRoles.Add(ConwayPoly.Roles.NewAlt);
		        poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());

	        }
			poly.Halfedges.MatchPairs();
	        return poly;
        }

        public static ConwayPoly Prism(int sides)
        {
            float height = _CalcSideLength(sides);
            return _MakePrism(sides, height);
        }

        private static float _CalcAntiprismHeight(int sides)
        {
	        return _CalcSideLength(sides) * Mathf.Sqrt(0.75f);
        }

        public static ConwayPoly Antiprism(int sides)
        {
            float height = _CalcAntiprismHeight(sides);
            return _MakePrism(sides, height, true);
        }

        private static ConwayPoly _MakePrism(int sides, float height, bool anti=false)
        {
            ConwayPoly poly = _MakePolygon(sides);
            ConwayPoly cap = _MakePolygon(sides, true, anti?0.5f:0, height);
            poly.Append(cap);
            
            int i = 0;
            var edge1 = poly.Halfedges[0];
            var edge2 = poly.Halfedges[sides];
            while (true)
            {
	            if (anti)
                {
                    var side1 = new List<Vertex>
                    {
                        edge1.Vertex,
                        edge1.Prev.Vertex,
                        edge2.Vertex
                    };
                    poly.Faces.Add(side1);
                    poly.FaceRoles.Add(ConwayPoly.Roles.New);
                    poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());


                    var side2 = new List<Vertex>
                    {
                        edge1.Vertex,
                        edge2.Vertex,
                        edge2.Prev.Vertex
                    };
                    poly.Faces.Add(side2);
                    poly.FaceRoles.Add(ConwayPoly.Roles.NewAlt);
                    poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());

                }
                else
                {
                    var side = new List<Vertex>
                    {
                        edge1.Vertex,
                        edge1.Prev.Vertex,
                        edge2.Vertex,
                        edge2.Prev.Vertex
                    };
                    poly.Faces.Add(side);
                    poly.FaceRoles.Add(ConwayPoly.Roles.New);
                    poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());

                }

                i++;
                edge1 = edge1.Next;
                edge2 = edge2.Prev;

                if (i == sides) break;

            }
            
            poly.Halfedges.MatchPairs();
            return poly;
        }

        private static float _CalcPyramidHeight(float sides)
        {
            float height;

            // Try and make equilateral sides if we can
            // Otherwise just use the nearest valid side count
            sides = Mathf.Clamp(sides, 3, 5);

            float sideLength = _CalcSideLength(sides);
            height = Mathf.Sqrt(Mathf.Pow(sideLength, 2) - 1f);


            return height;
        }

        public static ConwayPoly Pyramid(int sides)
        {
            var height = _CalcPyramidHeight(sides);
            return _MakePyramid(sides, height);
        }
        
        public static ConwayPoly Pyramid(int sides, float height)
        {
	        return _MakePyramid(sides, height);
        }
        
        private static ConwayPoly _MakePyramid(int sides, float height)
        {
            ConwayPoly polygon = _MakePolygon(sides, true);
            var poly = polygon.Kis(new OpParams{valueA = height});
            var baseVerts = poly.Vertices.GetRange(0, sides);
            baseVerts.Reverse();
            poly.Faces.Insert(0, baseVerts);
            poly.FaceRoles.Insert(0, ConwayPoly.Roles.Existing);
            poly.FaceTags.Insert(0, new HashSet<Tuple<string, ConwayPoly.TagType>>());
            poly.Halfedges.MatchPairs();
            return poly;
        }

        // Base forms are pyramids, cupolae and rotundae.
        // For each base we have elongate and bi. For each bi form we can also gyroelongate
        // Bi can come in ortho and gyro flavours in most cases.
        // You can combine bases i.e. cupolarotunda. These also come in ortho and gyro flavours.
        // Gyrobifastigium is just trying to be weird
        // Prisms can be augmented and diminished. Also bi, tri, para and meta
        // Truncation is a thing.and can be combined with augment/diminish.
        // Phew! Then stuff gets weirder.

        public static ConwayPoly ElongatedPyramid(int sides)
        {
			float height = _CalcSideLength(sides);
			ConwayPoly poly = _MakePrism(sides, height);
			height = _CalcPyramidHeight(sides);
			poly = poly.Kis(new OpParams{valueA = height, facesel = FaceSelections.FacingUp});
			return poly;
        }

        public static ConwayPoly ElongatedDipyramid(int sides)
        {
			ConwayPoly poly = ElongatedPyramid(sides);
			float height = _CalcPyramidHeight(sides);
			poly = poly.Kis(new OpParams{facesel = FaceSelections.FacingDown, valueA = height});
			
			return poly;
        }

		public static ConwayPoly GyroelongatedPyramid(int sides)
		{
			float height = _CalcSideLength(sides);
			ConwayPoly poly = Antiprism(sides);

			height = _CalcPyramidHeight(sides);
			poly = poly.Kis(new OpParams{facesel = FaceSelections.FacingStraightUp, valueA = height});
			return poly;
		}

		public static ConwayPoly GyroelongatedDipyramid(int sides)
		{
			ConwayPoly poly = GyroelongatedPyramid(sides);
			float height = _CalcPyramidHeight(sides);
			poly = poly.Kis(new OpParams{facesel = FaceSelections.FacingStraightDown, valueA = height});
			return poly;
		}

        public static ConwayPoly ElongatedCupola(int sides)
        {
	        ConwayPoly poly = Cupola(sides);
	        float bodyHeight = _CalcSideLength(sides * 2);
	        poly = poly.Loft(new OpParams{facesel = FaceSelections.OnlyFirst, valueB = bodyHeight});
	        return poly;
        }

		public static ConwayPoly ElongatedBicupola(int sides, bool gyro)
		{
			ConwayPoly poly = ElongatedCupola(sides);
			Face bottom = poly.Faces[sides * 2];
			int i = 0;
			var middleVerts = bottom.GetVertices();

			poly.Faces.Remove(bottom);
			poly.FaceRoles.RemoveAt(poly.FaceRoles.Count - 1);
			poly.FaceTags.RemoveAt(poly.FaceRoles.Count - 1);

			float baseOffset = -(_CalcSideLength(sides * 2) + _CalcCupolaHeight(sides));
			float angleOffset = gyro ? 0.75f : 0.25f;
			ConwayPoly cap2 = _MakePolygon(sides, false, angleOffset, baseOffset, _CalcCupolaCapRadius(sides));
			poly.Append(cap2);
			var edge2 = poly.Faces.Last().Halfedge.Prev;

			int edgeOffset = gyro ? 0 : 1;

			while (true)
			{
					var side1 = new List<Vertex>
					{
						middleVerts[PolyUtils.ActualMod(i * 2 - 1 - edgeOffset, sides * 2)],
						middleVerts[PolyUtils.ActualMod(i * 2 - edgeOffset, sides * 2)],
						edge2.Vertex
					};
					poly.Faces.Add(side1);
					poly.FaceRoles.Add(ConwayPoly.Roles.New);
					poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());

				var side2 = new List<Vertex>
				{
					middleVerts[PolyUtils.ActualMod(i * 2 - edgeOffset, sides * 2)],
					middleVerts[PolyUtils.ActualMod(i * 2 + 1 - edgeOffset, sides * 2)],
					edge2.Next.Vertex,
					edge2.Vertex,
				};
				poly.Faces.Add(side2);
				poly.FaceRoles.Add(ConwayPoly.Roles.NewAlt);
				poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());

				i++;
				edge2 = edge2.Next;

				if (i == sides) break;
			}
			poly.Halfedges.MatchPairs();
			return poly;
	    }

	    public static ConwayPoly GyroelongatedCupola(int sides)
	    {
			ConwayPoly poly = Antiprism(sides * 2);
			Face topFace = poly.Faces[1];
			ConwayPoly cap1 = _MakePolygon(sides, true, 0f, _CalcCupolaHeight(sides) + _CalcAntiprismHeight(sides * 2), _CalcCupolaCapRadius(sides));
			poly.Append(cap1);

			int i = 0;
			var middleVerts = topFace.GetVertices();
			poly.Faces.Remove(topFace);
			poly.FaceRoles.RemoveAt(1);
			poly.FaceTags.RemoveAt(1);

			var edge2 = poly.Faces.Last().Halfedge.Prev;
			while (true)
			{
				var side1 = new List<Vertex>
				{
					middleVerts[PolyUtils.ActualMod(i * 2 - 1, sides * 2)],
					middleVerts[PolyUtils.ActualMod(i * 2, sides * 2)],
					edge2.Vertex
				};
				poly.Faces.Add(side1);
				poly.FaceRoles.Add(ConwayPoly.Roles.New);
				poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());

				var side2 = new List<Vertex>
				{
					middleVerts[PolyUtils.ActualMod(i * 2, sides * 2)],
					middleVerts[PolyUtils.ActualMod(i * 2 + 1, sides * 2)],
					edge2.Next.Vertex,
					edge2.Vertex,
				};
				poly.Faces.Add(side2);
				poly.FaceRoles.Add(ConwayPoly.Roles.NewAlt);
				poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());

				i++;
				edge2 = edge2.Next;
				if (i == sides) break;
			}

			poly.Halfedges.MatchPairs();
			return poly;
		}

	    // Gyro bool determines if the caps are gyro - not the elongattion
		public static ConwayPoly GyroelongatedBicupola(int sides, bool gyro)
		{

			ConwayPoly poly = GyroelongatedCupola(sides);
			Face bottomFace = poly.Faces[0];
			float angleOffset = gyro ? 0.75f : 0.25f;
			ConwayPoly cap2 = _MakePolygon(sides, false, angleOffset, -_CalcCupolaHeight(sides), _CalcCupolaCapRadius(sides));
			poly.Append(cap2);

			int i = 0;
			var middleVerts = bottomFace.GetVertices();
			poly.Faces.Remove(bottomFace);
			poly.FaceRoles.RemoveAt(0);
			poly.FaceTags.RemoveAt(0);
			var edge2 = poly.Faces.Last().Halfedge.Prev;
			while (true)
			{
				int indexOffset = gyro ? 0 : -1;
				var side1 = new List<Vertex>
				{
						middleVerts[PolyUtils.ActualMod(i * 2 - 1 + indexOffset, sides * 2)],
						middleVerts[PolyUtils.ActualMod(i * 2 + indexOffset, sides * 2)],
						edge2.Vertex
				};
				poly.Faces.Add(side1);
				poly.FaceRoles.Add(ConwayPoly.Roles.New);
				poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());
				var side2 = new List<Vertex>
				{
						middleVerts[PolyUtils.ActualMod(i * 2 + indexOffset, sides * 2)],
						middleVerts[PolyUtils.ActualMod(i * 2 + 1 + indexOffset, sides * 2)],
						edge2.Next.Vertex,
						edge2.Vertex,
				};
				poly.Faces.Add(side2);
				poly.FaceRoles.Add(ConwayPoly.Roles.NewAlt);
				poly.FaceTags.Add(new HashSet<Tuple<string, ConwayPoly.TagType>>());

				i++;
				edge2 = edge2.Next;

				if (i == sides) break;
			}

			poly.Halfedges.MatchPairs();
			return poly;
		}

        private static float _CalcSideLength(float sides)
        {
            return 2 * Mathf.Sin(Mathf.PI / sides);
        }

        public static ConwayPoly Dipyramid(int sides)
        {
            float height = _CalcPyramidHeight(sides);
            return _MakeDipyramid(sides, height);
        }
        
        private static ConwayPoly _MakeDipyramid(int sides, float height)
        {
            ConwayPoly poly = _MakePyramid(sides, height);
            poly = poly.Kis(new OpParams{valueA = height, facesel = FaceSelections.Existing});
            return poly;
        }

        private static float _CalcPolygonSide(int sides)
        {
	        return Mathf.Sin(Mathf.PI / sides) * 2f;
        }

        private static float _CalcPolygonInradius(int sides, float sideLength)
        {
	        return (sideLength / (2 * Mathf.Tan(Mathf.PI / sides)));
        }

        private static float _CalcCupolaHeight(int sides)
        {
	        sides = Mathf.Clamp(sides, 3, 5);
	        var baseSide = _CalcPolygonSide(sides * 2);
	        float radDelta = _CalcPolygonInradius(sides * 2, baseSide) - _CalcPolygonInradius(sides, baseSide);
	        return Mathf.Sqrt(Mathf.Pow(baseSide, 2) - Mathf.Pow(radDelta, 2));
        }

        private static float _CalcCupolaCapRadius(int sides)
        {
	        var baseSide = _CalcPolygonSide(sides * 2);
	        return baseSide / (Mathf.Sin(Mathf.PI / sides) * 2f);
        }

        public static ConwayPoly Cupola(int sides)
        {
            return _MakeCupola(sides, _CalcCupolaHeight(sides));
        }

        public static ConwayPoly OrthoBicupola(int sides)
        {
            return _MakeBicupola(sides, _CalcCupolaHeight(sides), false);
        }

        public static ConwayPoly GyroBicupola(int sides)
        {
	        return _MakeBicupola(sides, _CalcCupolaHeight(sides), true);
        }

        private static ConwayPoly _MakeBicupola(int sides, float height, bool gyro)
        {
	        if (sides < 3) sides = 3;
            ConwayPoly poly = _MakeCupola(sides, height, true, gyro);
            return poly;
        }

        public static ConwayPoly Rotunda()
        {
            var wythoffPoly = new WythoffPoly(Uniform.Uniforms[29].Wythoff);
            wythoffPoly.BuildFaces();

            var conwayPoly = new ConwayPoly(wythoffPoly);
            conwayPoly = conwayPoly.FaceRemove(new OpParams{facesel = FaceSelections.FacingDown});
            conwayPoly = conwayPoly.FillHoles();
            return conwayPoly;
        }

        public static ConwayPoly L_Shape()
			{
				var verts = new List<Vector3>();
				for (var i = -0.25f; i <= 0.25f; i+=0.5f)
				{
					verts.Add(new Vector3(0, i, 0));
					verts.Add(new Vector3(0.5f, i, 0));
					verts.Add(new Vector3(0.5f, i, -0.5f));
					verts.Add(new Vector3(-0.5f, i, -0.5f));
					verts.Add(new Vector3(-0.5f, i, 0.5f));
					verts.Add(new Vector3(0, i, 0.5f));
				}

				var faces = new List<List<int>>
				{
					new List<int>{0, 5, 4, 3},
					new List<int>{0, 3, 2, 1},
					new List<int>{6, 7, 8, 9},
					new List<int>{6, 9, 10, 11},
					new List<int>{3, 9, 8, 2},
					new List<int>{2, 8, 7, 1},
					new List<int>{1, 7, 6, 0},
					new List<int>{0, 6, 11, 5},
					new List<int>{5, 11, 10, 4},
					new List<int>{4, 10, 9, 3}
				};

				var faceRoles = Enumerable.Repeat(ConwayPoly.Roles.Existing, 10);
				var vertexRoles = Enumerable.Repeat(ConwayPoly.Roles.Existing, 12);

				return new ConwayPoly(verts, faces, faceRoles, vertexRoles);
			}

		public static ConwayPoly L_Alt_Shape()
		{
			var verts = new List<Vector3>();
			for (var i = -0.25f; i <= 0.25f; i+=0.5f)
			{
				verts.Add(new Vector3(0, i, 0));
				verts.Add(new Vector3(0.5f, i, 0));
				verts.Add(new Vector3(0.5f, i, -0.5f));
				verts.Add(new Vector3(0, i, -0.5f));
				verts.Add(new Vector3(-0.5f, i, -0.5f));
				verts.Add(new Vector3(-0.5f, i, 0));
				verts.Add(new Vector3(-0.5f, i, 0.5f));
				verts.Add(new Vector3(0, i, 0.5f));
			}

			var faces = new List<List<int>>
			{
				new List<int>{0, 7, 6, 5},
				new List<int>{0, 5, 4, 3},
				new List<int>{0, 3, 2, 1},
				new List<int>{8, 9, 10, 11},
				new List<int>{8, 11, 12, 13},
				new List<int>{8, 13, 14, 15},
				new List<int>{4, 12, 11, 10, 2, 3},
				new List<int>{2, 10, 9, 1},
				new List<int>{1, 9, 8, 0},
				new List<int>{0, 8, 15, 7},
				new List<int>{7, 15, 14, 6},
				new List<int>{6, 14, 13, 12, 4, 5}
			};

			var faceRoles = Enumerable.Repeat(ConwayPoly.Roles.Existing, 12);
			var vertexRoles = Enumerable.Repeat(ConwayPoly.Roles.Existing, 16);

			return new ConwayPoly(verts, faces, faceRoles, vertexRoles);
		}

		public static ConwayPoly C_Shape()
		{
			var conway = Grids.Grids.MakeUnitileGrid(1, 0, 2, 3);
			conway = conway._FaceRemove(new OpParams{filterFunc = f => f.index==4});
			return conway;
		}

		public static ConwayPoly H_Shape()
		{
			var conway = Grids.Grids.MakeUnitileGrid(1, 0, 3, 3);
			conway = conway.FaceRemove(new OpParams{filterFunc = f => f.index==3});
			conway = conway.FaceRemove(new OpParams{filterFunc = f => f.index==4});
			return conway;
		}

		public static ConwayPoly GriddedCube(int PrismP)
		{
			var conway = Grids.Grids.MakeUnitileGrid(1, 0, PrismP, PrismP);
			var copy = conway.Duplicate();
			copy.Mirror(Vector3.up, PrismP);
			conway.Append(copy);
			conway.Recenter();
			ConwayPoly xPair = conway.Rotate(Vector3.forward, 90);
			ConwayPoly zPair = conway.Rotate(Vector3.left, 90);
			conway.Append(xPair);
			conway.Append(zPair);
			conway = conway.Weld(0.001f);
			return conway;
		}

		public static ConwayPoly Polygon(int sides)
		{
			return _MakePolygon(sides, true);
		}

		public static ConwayPoly Test3Triangle()
		{
			var verts = new List<Vector3>();
			verts.Add(new Vector3(0.5f, 0, 0));
			verts.Add(new Vector3(-0.5f, 0, 0));
			verts.Add(new Vector3(0, 0, -0.5f));
			verts.Add(new Vector3(0, 0.5f, -0.5f));

			var faces = new List<List<int>>
			{
				new List<int>{0,1,2},
				new List<int>{3,1,0},
				new List<int>{0,2,3}
			};

			var faceRoles = Enumerable.Repeat(ConwayPoly.Roles.Existing, 3);
			var vertexRoles = Enumerable.Repeat(ConwayPoly.Roles.Existing, 4);
			return new ConwayPoly(verts, faces, faceRoles, vertexRoles);
		}

		public static ConwayPoly Test2Square()
		{
			var verts = new List<Vector3>();
			verts.Add(new Vector3(0.5f, 0, 0));
			verts.Add(new Vector3(-0.5f, 0, 0));
			verts.Add(new Vector3(0.5f, 0, -0.5f));
			verts.Add(new Vector3(-0.5f, 0, -0.5f));
			verts.Add(new Vector3(0.5f, 0.5f, -0.5f));
			verts.Add(new Vector3(-0.5f, 0.5f, -0.5f));

			var faces = new List<List<int>>
			{
				new List<int>{0,2,3,1},
				new List<int>{0,1,5,4}
			};

			var faceRoles = Enumerable.Repeat(ConwayPoly.Roles.Existing, 2);
			var vertexRoles = Enumerable.Repeat(ConwayPoly.Roles.Existing, 6);
			return new ConwayPoly(verts, faces, faceRoles, vertexRoles);
		}

		public static ConwayPoly UvSphere(int verticalLines = 24, int horizontalLines = 24, float hemi = 1)
		{

			var faceRoles = new List<ConwayPoly.Roles>();

			horizontalLines = Mathf.Clamp(horizontalLines, 3, 24);
			verticalLines = Mathf.Clamp(verticalLines, 3, 24);

			var verts = new List<Vector3>();
			for (float v = 0; v <= horizontalLines; v++)
			{
				for (float u = 0; u < verticalLines; u++)
				{
					var vv = v / horizontalLines;
					var uu = u / verticalLines;
					// Avoid coincident vertices at the tip
					// as this caused weird glitches on Lace
					if (vv == 0) vv = 0.0001f;

					float x = Mathf.Sin(Mathf.PI * vv) * Mathf.Cos(2f * Mathf.PI * uu);
					float y = Mathf.Sin(Mathf.PI * vv) * Mathf.Sin(2f * Mathf.PI * uu);
					float z = Mathf.Cos(Mathf.PI * vv);
					verts.Add(new Vector3(x, z, y));
				}
			}

			var faces = new List<List<int>>();
			for (int v = 0; v < horizontalLines * hemi; v += 1)
			{
				for (int u = 0; u < verticalLines; u += 1)
				{
					faces.Add(new List<int> {
						(v * verticalLines) + u,
						(v * verticalLines) + ((u + 1) % verticalLines),
						((v + 1) * verticalLines) + ((u + 1) % verticalLines),
						((v + 1) * verticalLines) + u
					});
					faceRoles.Add((u + v) % 2 == 0 ? ConwayPoly.Roles.New : ConwayPoly.Roles.NewAlt);
				}
			}

			var vertexRoles = Enumerable.Repeat(ConwayPoly.Roles.Existing, verts.Count);
			var poly = new ConwayPoly(verts, faces, faceRoles, vertexRoles);
			return poly;
		}

		public static ConwayPoly UvHemisphere(int verticalLines = 24, int horizontalLines = 24)
		{
			var poly = UvSphere(verticalLines, horizontalLines, 0.5f);
			poly = poly.FillHoles();
			return poly;
		}

		public static ConwayPoly Build(PolyHydraEnums.JohnsonPolyTypes johnsonPolyType, int sides)
		{
			ConwayPoly poly;
			
			switch (johnsonPolyType)
			{
                case PolyHydraEnums.JohnsonPolyTypes.Prism:
                    poly = Prism(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.Antiprism:
                    poly = Antiprism(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.Pyramid:
                    poly = Pyramid(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.ElongatedPyramid:
                    poly = ElongatedPyramid(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.GyroelongatedPyramid:
                    poly = GyroelongatedPyramid(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.Dipyramid:
                    poly = Dipyramid(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.ElongatedDipyramid:
                    poly = ElongatedDipyramid(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.GyroelongatedDipyramid:
                    poly = GyroelongatedDipyramid(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.Cupola:
                    poly = Cupola(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.ElongatedCupola:
                    poly = ElongatedCupola(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.GyroelongatedCupola:
                    poly = GyroelongatedCupola(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.OrthoBicupola:
                    poly = OrthoBicupola(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.GyroBicupola:
                    poly = GyroBicupola(sides);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.ElongatedOrthoBicupola:
                    poly = ElongatedBicupola(sides, false);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.ElongatedGyroBicupola:
                    poly = ElongatedBicupola(sides, true);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.GyroelongatedBicupola:
                    poly = GyroelongatedBicupola(sides, false);
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.Rotunda:
                    poly = Rotunda();
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.ElongatedRotunda:
                    poly = ElongatedRotunda();
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.GyroelongatedRotunda:
                    poly = GyroelongatedRotunda();
                    break;
                case PolyHydraEnums.JohnsonPolyTypes.GyroelongatedBirotunda:
                    poly = GyroelongatedBirotunda();
                    break;
                default:
	                poly = new ConwayPoly();
	                break;
			}
			return poly;
		}
		
		public static ConwayPoly BuildOther(PolyHydraEnums.OtherPolyTypes otherPolyType, int p, int q)
		{
			ConwayPoly poly = null;

			switch (otherPolyType)
			{
				case PolyHydraEnums.OtherPolyTypes.Polygon:
					poly = Polygon(p);
					break;
				case PolyHydraEnums.OtherPolyTypes.UvSphere:
					poly = UvSphere(p, q);
					break;
				case PolyHydraEnums.OtherPolyTypes.UvHemisphere:
					poly = UvHemisphere(p, q);
					break;
				case PolyHydraEnums.OtherPolyTypes.GriddedCube:
					poly = GriddedCube(p);
					break;
				case PolyHydraEnums.OtherPolyTypes.C_Shape:
					poly = C_Shape();
					break;
				case PolyHydraEnums.OtherPolyTypes.L_Shape:
					poly = L_Shape();
					break;
				case PolyHydraEnums.OtherPolyTypes.L_Alt_Shape:
					poly = L_Alt_Shape();
					break;
				case PolyHydraEnums.OtherPolyTypes.H_Shape:
					poly = H_Shape();
					break;
			}
			return poly;
		}

    }
}