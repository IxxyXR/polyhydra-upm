using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wythoff;
using Debug = UnityEngine.Debug;
using Random = System.Random;


namespace Conway
{
	public partial class ConwayPoly
	{

		#region Properties

		private Random random;
		private const float TOLERANCE = 0.02f;
		private PointOctree<Vertex> octree;

		public struct BasePolyhedraInfo
		{
			public int P;
			public int Q;
		}

		public enum TagType
		{
			Introvert,
			Extrovert
		}

		public BasePolyhedraInfo basePolyhedraInfo = new BasePolyhedraInfo();

		public List<Roles> FaceRoles;
		public List<Roles> VertexRoles;
		public List<HashSet<Tuple<string, TagType>>> FaceTags;


		public MeshHalfedgeList Halfedges { get; private set; }
		public MeshVertexList Vertices { get; set; }
		public MeshFaceList Faces { get; private set; }

		public enum Roles
		{
			Ignored,
			Existing,
			New,
			NewAlt,
			ExistingAlt,
		}

		public bool IsValid
		{
			get
			{
				if (Halfedges.Count == 0)
				{
					return false;
				}

				if (Vertices.Count == 0)
				{
					return false;
				}

				if (Faces.Count == 0)
				{
					return false;
				}

				// TODO: beef this up (check for a valid mesh)

				return true;
			}
		}

		public int[] GetFaceCountsByType()
		{
			var faceCountsByType = new int[8];
			foreach (var face in Faces)
			{
				int sides = face.Sides;
				if (faceCountsByType.Length < sides + 1)
				{
					Array.Resize(ref faceCountsByType, sides + 1);
				}
				faceCountsByType[sides]++;
			}
			return faceCountsByType;
		}

		public (int v, int e, int f) vef
		{
			get
			{
				int v = Vertices.Count;
				int e = EdgeCount;
				int f = Faces.Count;
				return (v, e, f);
			}
		}

		public int EdgeCount
		{
			get
			{
				var nakedEdges = Halfedges.Count(x => x.Pair == null);
				var fullEdges = (Halfedges.Count - nakedEdges) / 2;
				return nakedEdges + fullEdges;
			}
		}
		
		// TODO
		//        public BoundingBox BoundingBox {
		//            get {
		//                if (!IsValid) {
		//                    return BoundingBox.Empty;
		//                }
		//
		//                List<Vector> points = new List<Vector>();
		//                foreach (BVertex v in this.Vertices) {
		//                    points.Add(v.Position);
		//                }
		//
		//                BoundingBox result = new BoundingBox(points);
		//                result.MakeValid();
		//                return result;
		//            }
		//        }

		#endregion

		#region Constructors

		public ConwayPoly()
		{
			Halfedges = new MeshHalfedgeList(this);
			Vertices = new MeshVertexList(this);
			Faces = new MeshFaceList(this);
			FaceRoles = new List<Roles>();
			VertexRoles = new List<Roles>();
			random = new Random();
			InitTags();
		}

		public ConwayPoly(WythoffPoly source, bool abortOnFailure = true) : this()
		{
			FaceRoles = new List<Roles>();
			VertexRoles = new List<Roles>();

			// Add vertices
			Vertices.Capacity = source.VertexCount;
			for (var i = 0; i < source.Vertices.Length; i++)
			{
				Vector p = source.Vertices[i];
				Vertices.Add(new Vertex(p.getVector3()));
				VertexRoles.Add(Roles.Existing);
			}

			// Add faces (and construct halfedges and store in hash table)
			for (var faceIndex = 0; faceIndex < source.faces.Count; faceIndex++)
			{
				var face = source.faces[faceIndex];
				var v = new Vertex[face.points.Count];

				for (int i = 0; i < face.points.Count; i++)
				{
					v[i] = Vertices[face.points[i]];
				}

				FaceRoles.Add((Roles) ((int) face.configuration % 5));


				if (!Faces.Add(v))
				{
					// Failed. Let's try flipping the face
					Array.Reverse(v);
					if (!Faces.Add(v))
					{
						if (abortOnFailure)
						{
							throw new InvalidOperationException("Failed even after flipping.");
						}
						else
						{
							Debug.LogWarning($"Failed even after flipping. ({v.Length} verts)");
							continue;
						}
					}
				}
			}

			// Find and link halfedge pairs
			Halfedges.MatchPairs();
			InitTags();
		}

		public ConwayPoly(
			IEnumerable<Vector3> verticesByPoints,
			IEnumerable<IEnumerable<int>> facesByVertexIndices,
			IEnumerable<Roles> faceRoles,
			IEnumerable<Roles> vertexRoles
		) : this()
		{
			if (faceRoles.Count() != facesByVertexIndices.Count())
			{
				throw new ArgumentException(
					$"Incorrect FaceRole array: {faceRoles.Count()} instead of {facesByVertexIndices.Count()}",
					"faceRoles"
				);
			}
			FaceRoles = faceRoles.ToList();
			VertexRoles = vertexRoles.ToList();
			InitIndexed(verticesByPoints, facesByVertexIndices);

			CullUnusedVertices();
			InitTags();
		}

		private ConwayPoly(
			IEnumerable<Vector3> verticesByPoints,
			IEnumerable<IEnumerable<int>> facesByVertexIndices,
			IEnumerable<Roles> faceRoles,
			IEnumerable<Roles> vertexRoles,
			List<HashSet<Tuple<string, TagType>>> newFaceTags
		) : this(verticesByPoints, facesByVertexIndices, faceRoles, vertexRoles)
		{
			FaceTags = newFaceTags;
		}

		#endregion

		#region General Methods

		/// <summary>
		/// Removes all vertices that are currently not used by the Halfedge list.
		/// </summary>
		/// <returns>The number of unused vertices that were removed.</returns>
		public int CullUnusedVertices() {
			var orig = new List<Vertex>(Vertices);
			var origVertexRoles = new List<Roles>(VertexRoles);
			Vertices.Clear();
			VertexRoles.Clear();
			// re-add vertices which reference a halfedge
			for (var vertIndex = 0; vertIndex < orig.Count; vertIndex++)
			{
				var vertex = orig[vertIndex];
				if (vertex.Halfedge != null)
				{
					Vertices.Add(vertex);
					VertexRoles.Add(origVertexRoles[vertIndex]);
				}
			}
			return orig.Count - Vertices.Count;
		}


		/// <summary>
		/// A string representation of the mesh.
		/// </summary>
		/// <returns>a string representation of the mesh</returns>
		public override string ToString()
		{
			return base.ToString() + String.Format(" (V:{0} F:{1})", Vertices.Count, Faces.Count);
		}

		/// <summary>
		/// Gets the positions of all mesh vertices. Note that points are duplicated.
		/// </summary>
		/// <returns>a list of vertex positions</returns>
		public Vector3[] ListVerticesByPoints()
		{
			Vector3[] points = new Vector3[Vertices.Count];
			for (int i = 0; i < Vertices.Count; i++)
			{
				Vector3 pos = Vertices[i].Position;
				points[i] = new Vector3(pos.x, pos.y, pos.z);
			}

			return points;
		}

		public List<List<Halfedge>> FindBoundaries()
		{
			var looped = new HashSet<Halfedge>();
			var loops = new List<List<Halfedge>>();

			foreach (var startHalfedge in Halfedges)
			{
				// If it's not a bare edge or we've already checked it
				if (startHalfedge.Pair != null || looped.Contains(startHalfedge)) continue;

				var loop = new List<Halfedge>();
				var currLoopEdge = startHalfedge;
				int escapeClause = 0;
				do
				{
					loop.Add(currLoopEdge);
					looped.Add(currLoopEdge);
					Halfedge nextLoopEdge = null;
					var possibleEdges = currLoopEdge.Prev.Vertex.Halfedges;
					//possibleEdges.Reverse();
					foreach (var edgeToTest in possibleEdges)
					{
						if (currLoopEdge != edgeToTest && edgeToTest.Pair == null)
						{
							nextLoopEdge = edgeToTest;
							break;
						}

					}

					if (nextLoopEdge != null)
					{
						currLoopEdge = nextLoopEdge;
					}

					escapeClause++;
				} while (currLoopEdge != startHalfedge && escapeClause < 1000);

				if (loop.Count >= 3)
				{
					loops.Add(loop);
				}


			}

			return loops;
		}

//		public ConwayPoly ElongateHoles()
//		{
//
//		}
//
//		public ConwayPoly GyroElongateHoles()
//		{
//
//		}

		/// <summary>
		/// Gets the indices of vertices in each face loop (i.e. index face-vertex data structure).
		/// Used for duplication and conversion to other mesh types, such as Rhino's.
		/// </summary>
		/// <returns>An array of lists of vertex indices.</returns>
		public List<int>[] ListFacesByVertexIndices()
		{

			var fIndex = new List<int>[Faces.Count];
			var vlookup = new Dictionary<Guid, int>();

			for (int i = 0; i < Vertices.Count; i++)
			{
				vlookup.Add(Vertices[i].Name, i);
			}

			for (int i = 0; i < Faces.Count; i++)
			{
				var vertIndices = new List<int>();
				var vs = Faces[i].GetVertices();
				for (var vertIndex = 0; vertIndex < vs.Count; vertIndex++)
				{
					Vertex v = vs[vertIndex];
					vertIndices.Add(vlookup[v.Name]);
				}

				fIndex[i] = vertIndices;
			}

			return fIndex;
		}

		public bool HasNaked()
		{
			return Halfedges.Select((item, ii) => ii).Where(i => Halfedges[i].Pair == null).ToList().Count > 0;
		}

//        TODO
//        public List<Polyline> ToClosedPolylines() {
//            List<Polyline> polylines = new List<Polyline>(Faces.Count);
//            foreach (Face f in Faces) {
//                polylines.Add(f.ToClosedPolyline());
//            }
//
//            return polylines;
//        }
//
//        public List<Line> ToLines() {
//            return Halfedges.GetUnique().Select(h => new Rhino.Geometry.Line(h.Prev.BVertex.Position, h.BVertex.Position))
//                .ToList();
//        }

		private int FaceSelectionToSides(FaceSelections facesel)
		{
			switch (facesel)
			{
				case FaceSelections.ThreeSided:
					return 3;
				case FaceSelections.FourSided:
					return 4;
				case FaceSelections.FiveSided:
					return 5;
				case FaceSelections.SixSided:
					return 6;
				case FaceSelections.SevenSided:
					return 7;
				case FaceSelections.EightSided:
					return 8;
				case FaceSelections.NineSided:
					return 9;
				case FaceSelections.TenSided:
					return 10;
				case FaceSelections.ElevenSided:
					return 11;
				case FaceSelections.TwelveSided:
					return 12;
			}

			return 0;
		}

		public bool IncludeFace(int faceIndex, FaceSelections facesel,
			IEnumerable<Tuple<string, TagType>> tagList = null, 
			Func<FilterParams, bool> filterFunc = null)
		{
			bool include = true;
			if (tagList != null && tagList.Any())
			{  // Return true if any tag strings match
				include = tagList.Select(x=>x.Item1).Intersect(FaceTags[faceIndex].Select(x=>x.Item1)).Any();
			}
			filterFunc = filterFunc ?? FaceselToFaceFilterFunc(facesel);
			return include && filterFunc(new FilterParams(this, faceIndex));
		}

		public Func<FilterParams, bool> FaceselToFaceFilterFunc(FaceSelections facesel)
		{
			switch (facesel)
			{
				case FaceSelections.All:
					return p => true;
				case FaceSelections.EvenSided:
					return p => p.poly.Faces[p.index].Sides % 2 == 0;
				case FaceSelections.OddSided:
					return p => p.poly.Faces[p.index].Sides % 2 != 0;
				case FaceSelections.PSided:
					return p => p.poly.Faces[p.index].Sides == basePolyhedraInfo.P;
				case FaceSelections.QSided:
					return p => p.poly.Faces[p.index].Sides == basePolyhedraInfo.Q;
				case FaceSelections.FacingUp:
					return p => p.poly.Faces[p.index].Normal.y > TOLERANCE;
				case FaceSelections.FacingStraightUp:
					return p => Vector3.Angle(Vector3.up, p.poly.Faces[p.index].Normal) < TOLERANCE;
				case FaceSelections.FacingForward:
					return p => p.poly.Faces[p.index].Normal.z > TOLERANCE;
				case FaceSelections.FacingBackward:
					return p => p.poly.Faces[p.index].Normal.z < -TOLERANCE;
				case FaceSelections.FacingStraightForward:
					return p => Vector3.Angle(Vector3.forward, p.poly.Faces[p.index].Normal) < TOLERANCE;
				case FaceSelections.FacingStraightBackward:
					return p => Vector3.Angle(Vector3.back, p.poly.Faces[p.index].Normal) < TOLERANCE;
				case FaceSelections.FacingLevel:
					return p => Math.Abs(p.poly.Faces[p.index].Normal.y) < TOLERANCE;
				case FaceSelections.FacingDown:
					return p => p.poly.Faces[p.index].Normal.y < -TOLERANCE;
				case FaceSelections.FacingStraightDown:
					return p => Vector3.Angle(Vector3.down, p.poly.Faces[p.index].Normal) < TOLERANCE;
				case FaceSelections.FacingCenter:
					return p =>
					{
						var face = p.poly.Faces[p.index];
						var angle = Vector3.Angle(face.Normal, face.Centroid);
						return Math.Abs(angle) < TOLERANCE || Math.Abs(angle - 180) < TOLERANCE;
					};
				case FaceSelections.FacingIn:
					return p => Vector3.Angle(-p.poly.Faces[p.index].Normal, p.poly.Faces[p.index].Centroid) <
					            90 + TOLERANCE;
				case FaceSelections.FacingOut:
					return p => Vector3.Angle(-p.poly.Faces[p.index].Normal, p.poly.Faces[p.index].Centroid) >
					            90 - TOLERANCE;
				case FaceSelections.TopHalf:
					return p => p.poly.Faces[p.index].Centroid.y > 0;
				case FaceSelections.Existing:
					return p => p.poly.FaceRoles[p.index] == Roles.Existing ||
					            p.poly.FaceRoles[p.index] == Roles.ExistingAlt;
				case FaceSelections.Ignored:
					return p => p.poly.FaceRoles[p.index] == Roles.Ignored;
				case FaceSelections.New:
					return p => p.poly.FaceRoles[p.index] == Roles.New;
				case FaceSelections.NewAlt:
					return p => p.poly.FaceRoles[p.index] == Roles.NewAlt;
				case FaceSelections.AllNew:
					return p => p.poly.FaceRoles[p.index] == Roles.New || p.poly.FaceRoles[p.index] == Roles.NewAlt;
				case FaceSelections.Odd:
					return p => p.index % 2 == 1;
				case FaceSelections.Even:
					return p => p.index % 2 == 0;
				case FaceSelections.OnlyFirst:
					return p => p.index == 0;
				case FaceSelections.ExceptFirst:
					return p => p.index != 0;
				case FaceSelections.OnlyLast:
					return p => p.index == p.poly.Faces.Count - 1;
				case FaceSelections.ExceptLast:
					return p => p.index != p.poly.Faces.Count - 1;
				case FaceSelections.Inner:
					return p => !p.poly.Faces[p.index].HasNakedEdge();
				case FaceSelections.Outer:
					return p => p.poly.Faces[p.index].HasNakedEdge();
				case FaceSelections.Smaller:
					return p => p.poly.Faces[p.index].GetArea() <= 0.05f;
				case FaceSelections.Larger:
					return p => p.poly.Faces[p.index].GetArea() > 0.05f;
				case FaceSelections.Random:
					return p => random.NextDouble() < 0.5;
				case FaceSelections.None:
					return p => false;
			}

			return p => (p.poly.Faces[p.index].Sides == FaceSelectionToSides(facesel));

		}

		public bool IncludeVertex(int vertexIndex, FaceSelections vertexsel,
			IEnumerable<Tuple<string, TagType>> tagList = null, Func<FilterParams, bool> filterFunc = null)
		{
			bool include = true;
			if (tagList != null && tagList.Any())
			{  // Return true if any tags match
				var vert = Vertices[vertexIndex];
				foreach (var face in vert.GetVertexFaces())
				{
					// Bit clunky and slow
					include = include & tagList.Intersect(FaceTags[Faces.IndexOf(face)]).Any();
				}
			}
			filterFunc = filterFunc ?? VertexselToVertexFilterFunc(vertexsel);
			return include && filterFunc(new FilterParams(this, vertexIndex));
		}
		
		public Func<FilterParams, bool> VertexselToVertexFilterFunc(FaceSelections vertexsel)
		{
			switch (vertexsel)
			{
				case FaceSelections.All:
					return p => true;
				// TODO
				case FaceSelections.PSided:
					return p=>Vertices[p.index].Halfedges.Count == basePolyhedraInfo.P;
				case FaceSelections.QSided:
					return p=>Vertices[p.index].Halfedges.Count == basePolyhedraInfo.Q;
				case FaceSelections.ThreeSided:
					return p=>Vertices[p.index].Halfedges.Count <= 3; // Weird but it will do for now
				case FaceSelections.FourSided:
					return p=>Vertices[p.index].Halfedges.Count == 4;
				case FaceSelections.FiveSided:
					return p=>Vertices[p.index].Halfedges.Count == 5;
				case FaceSelections.SixSided:
					return p=>Vertices[p.index].Halfedges.Count == 6;
				case FaceSelections.SevenSided:
					return p=>Vertices[p.index].Halfedges.Count == 7;
				case FaceSelections.EightSided:
					return p=>Vertices[p.index].Halfedges.Count == 8;
				case FaceSelections.FacingUp:
					return p=>Vertices[p.index].Normal.y > TOLERANCE;
				case FaceSelections.FacingLevel:
					return p=>Math.Abs(Vertices[p.index].Normal.y) < TOLERANCE;
				case FaceSelections.FacingDown:
					return p=>Vertices[p.index].Normal.y < -TOLERANCE;
				case FaceSelections.FacingCenter:
					return p=>
					{
						var vertex = p.poly.Vertices[p.index];
						var angle = Vector3.Angle(-Vertices[p.index].Normal, Vertices[p.index].Position);
						return Math.Abs(angle) < TOLERANCE || Math.Abs(angle - 180) < TOLERANCE;
					};
				case FaceSelections.FacingIn:
					return p=>Vector3.Angle(-Vertices[p.index].Normal, Vertices[p.index].Position) >
					       90 - TOLERANCE;
				case FaceSelections.FacingOut:
					return p=>Vector3.Angle(-Vertices[p.index].Normal, Vertices[p.index].Position) <
					       90 + TOLERANCE;
				case FaceSelections.Existing:
					return p=>VertexRoles[p.index] == Roles.Existing;
				case FaceSelections.Ignored:
					return p=>VertexRoles[p.index] == Roles.Ignored;
				case FaceSelections.New:
					return p=>VertexRoles[p.index] == Roles.New;
				case FaceSelections.NewAlt:
					return p=>VertexRoles[p.index] == Roles.NewAlt;
				case FaceSelections.AllNew:
					return p=>VertexRoles[p.index] == Roles.New || VertexRoles[p.index] == Roles.NewAlt;
				case FaceSelections.Odd:
					return p=>p.index % 2 == 1;
				case FaceSelections.Even:
					return p=>p.index % 2 == 0;
				case FaceSelections.OnlyFirst:
					return p=>p.index == 0;
				case FaceSelections.ExceptFirst:
					return p=>p.index != 0;
				case FaceSelections.OnlyLast:
					return p=>p.index == p.poly.Vertices.Count - 1;
				case FaceSelections.ExceptLast:
					return p=>p.index != p.poly.Vertices.Count - 1;
				case FaceSelections.Inner:
					return p=>!Vertices[p.index].HasNakedEdge();
				case FaceSelections.Outer:
					return p=>Vertices[p.index].HasNakedEdge();
				case FaceSelections.Random:
					return p=>random.NextDouble() < 0.5;
			}

			return p => p.poly.Vertices[p.index].Halfedges.Count == FaceSelectionToSides(vertexsel);
		}

		public void InitOctree()
		{
			octree = new PointOctree<Vertex>(1, Vector3.zero, 32);
			for (var i = 0; i < Vertices.Count; i++)
			{
				var v = Vertices[i];
				octree.Add(v, v.Position);
			}
		}

		public Vertex[] FindNeighbours(Vertex v, float distance)
		{
			return octree.GetNearby(v.Position, distance);
		}

		private void InitIndexed(IEnumerable<Vector3> verticesByPoints,
			IEnumerable<IEnumerable<int>> facesByVertexIndices)
		{
			var newRoles = new List<Roles>();
			
			// Add vertices
			foreach (Vector3 p in verticesByPoints)
			{
				Vertices.Add(new Vertex(p));
			}

			// Add faces
			var faces = facesByVertexIndices.ToList();
			for (int counter=0; counter<faces.Count(); counter++)
			{
				List<int> indices = faces[counter].ToList();
				
				bool faceAdded;
				
				faceAdded = Faces.Add(indices.Select(i => Vertices[i]));
				
				if (!faceAdded)
				{
					indices.Reverse();
					faceAdded = Faces.Add(indices.Select(i => Vertices[i]));
				}

				if (faceAdded)
				{
					newRoles.Add(FaceRoles[counter]);
				}
			}

			// Find and link halfedge pairs
			Halfedges.MatchPairs();
			FaceRoles = newRoles;
		}

		public ConwayPoly ApplyOp(Ops op, OpParams opParams)
		{

			ConwayPoly polyResult = null;

			switch (op)
			{
				case Ops.Identity:
					polyResult = Duplicate();
					break;
				case Ops.Kis:
					polyResult = Kis(opParams);
					break;
				case Ops.Dual:
					polyResult = Dual();
					break;
				case Ops.Ambo:
					polyResult = Ambo();
					break;
				case Ops.Zip:
					polyResult = Zip(opParams);
					break;
				case Ops.Expand:
					polyResult = Expand(opParams);
					break;
				case Ops.Bevel:
					polyResult = Bevel(opParams);
					break;
				case Ops.Join:
					polyResult = Join(opParams);
					break;
				case Ops.Needle:
					polyResult = Needle(opParams);
					break;
				case Ops.Ortho:
					polyResult = Ortho(opParams);
					break;
				case Ops.Meta:
					polyResult = Meta(opParams);
					break;
				case Ops.Truncate:
					polyResult = Truncate(opParams);
					break;
				case Ops.Gyro:
					polyResult = Gyro(opParams);
					break;
				case Ops.Snub:
					polyResult = Gyro(opParams);
					polyResult = polyResult.Dual();
					break;
				case Ops.Exalt:
					polyResult = Exalt(opParams);
					break;
				case Ops.Yank:
					polyResult = Yank(opParams);
					break;
				case Ops.Subdivide:
					polyResult = Subdivide(opParams);
					break;
				case Ops.Loft:
					polyResult = Loft(opParams);
					break;
				case Ops.Chamfer:
					polyResult = Chamfer(opParams);
					break;
				case Ops.Quinto:
					polyResult = Quinto(opParams);
					break;
				case Ops.JoinedLace:
					polyResult = JoinedLace(opParams);
					break;
				case Ops.OppositeLace:
					polyResult = OppositeLace(opParams);
					break;
				case Ops.Lace:
					polyResult = Lace(opParams);
					break;
				case Ops.JoinKisKis:
					polyResult = JoinKisKis(opParams);
					break;
				case Ops.Stake:
					polyResult = Stake(opParams);
					break;
				case Ops.JoinStake:
					polyResult = Stake(opParams, true);
					break;
				case Ops.Medial:
					polyResult = Medial(opParams);
					break;
				case Ops.EdgeMedial:
					polyResult = EdgeMedial(opParams);
					break;
				// case Ops.JoinedMedial:
				// 	conway = conway.JoinedMedial((int)0, amount2);
				// 	break;
				case Ops.Propeller:
					polyResult = Propeller(opParams);
					break;
				case Ops.Whirl:
					polyResult = Whirl(opParams);
					break;
				case Ops.Volute:
					polyResult = Volute(opParams);
					break;
				case Ops.Cross:
					polyResult = Cross(opParams);
					break;
				case Ops.Squall:
					polyResult = Squall(opParams, false);
					break;
				case Ops.JoinSquall:
					polyResult = Squall(opParams, true);
					break;
				case Ops.SplitFaces:
					polyResult = SplitFaces(opParams, 0);
					break;
				case Ops.Gable:
					polyResult = Gable(opParams, 0);
					break;
				case Ops.Shell:
					polyResult = Shell(opParams, true);
					break;
				case Ops.Skeleton:
					// polyResult = FaceRemove(new OpParams{tags = tags});
					// if ((faceSelections==FaceSelections.New || faceSelections==FaceSelections.NewAlt) && op == PolyHydraEnums.Ops.Skeleton)
					// {
					// 	// Nasty hack until I fix extrude
					// 	// Produces better results specific for PolyMidi
					// 	polyResult = FaceScale(new OpParams{valueA = 0f, facesel = FaceSelections.All});
					// }
					// polyResult = Shell(opParams.valueA, false, randomize);
					break;
				case Ops.Segment:
					polyResult = Segment(opParams);
					break;
				case Ops.Extrude:
					opParams.funcB = opParams.funcA;
					opParams.funcA = null;
					opParams.valueB = opParams.valueA;
					opParams.valueA = 0;
					polyResult = Loft(opParams);
					break;
				case Ops.VertexScale:
					polyResult = VertexScale(opParams);
					break;
				case Ops.FaceSlide:
					polyResult = FaceSlide(opParams);
					break;
				case Ops.FaceMerge:
					polyResult = FaceMerge(opParams);
					break;
				case Ops.VertexRotate:
					polyResult = VertexRotate(opParams);
					break;
				case Ops.VertexFlex:
					polyResult = VertexFlex(opParams);
					break;
				case Ops.FaceOffset:
					// TODO Faceroles ignored. Vertex Roles
					var origRoles = FaceRoles;
					polyResult = FaceScale(new OpParams());
					polyResult.FaceRoles = origRoles;
					polyResult = Offset(opParams);
					break;
				case Ops.FaceScale:
					polyResult = FaceScale(opParams);
					break;
				case Ops.FaceRotate:
					polyResult = FaceRotate(opParams, 0);
					break;
	//					case Ops.Ribbon:
	//						conway = conway.Ribbon(new OpParams{false, 0.1f);
	//						break;
	//					case Ops.FaceTranslate:
	//						conway = conway.FaceTranslate(new OpParams{filterFunc = faceFilter});
	//						break;
				case Ops.FaceRotateX:
					polyResult = FaceRotate(opParams, 2);
					break;
				case Ops.FaceRotateY:
					polyResult = FaceRotate(opParams, 1);
					break;
				case Ops.FaceRemove:
					polyResult = FaceRemove(opParams);
					break;
				case Ops.FaceKeep:
					polyResult = FaceKeep(opParams);
					break;
				case Ops.VertexRemove:
					polyResult = VertexRemove(opParams, false);
					break;
				case Ops.VertexKeep:
					polyResult = VertexRemove(opParams, true);
					break;
				case Ops.FillHoles:
					polyResult = FillHoles();
					break;
				case Ops.ExtendBoundaries:
					polyResult = ExtendBoundaries(opParams);
					break;
				case Ops.ConnectFaces:
					polyResult = ConnectFaces(opParams);
					break;
				case Ops.Hinge:
					polyResult = Hinge(opParams.valueA);
					break;
				case Ops.AddDual:
					polyResult = AddDual(opParams.valueA);
					break;
				case Ops.AddCopyX:
					polyResult = AddCopy(Vector3.right, opParams.valueA, opParams.facesel, opParams.tags);
					break;
				case Ops.AddCopyY:
					polyResult = AddCopy(Vector3.up, opParams.valueA, opParams.facesel, opParams.tags);
					break;
				case Ops.AddCopyZ:
					polyResult = AddCopy(Vector3.forward, opParams.valueA, opParams.facesel, opParams.tags);
					break;
				case Ops.AddMirrorX:
					polyResult = AddMirrored(opParams, Vector3.right);
					break;
				case Ops.AddMirrorY:
					polyResult = AddMirrored(opParams, Vector3.up);
					break;
				case Ops.AddMirrorZ:
					polyResult = AddMirrored(opParams, Vector3.forward);
					break;
				case Ops.TagFaces:
					polyResult = Duplicate();
					polyResult.TagFaces(opParams.tags, opParams.facesel);
					break;
				case Ops.Layer:
					opParams.valueA = 1f - opParams.valueA;
					opParams.valueB = opParams.valueB / 10f;
					polyResult = Layer(opParams, 4);
					break;
				case Ops.Canonicalize:
					polyResult = Canonicalize(0.1f, 0.1f);
					break;
				case Ops.ConvexHull:
					polyResult = ConvexHull();
					break;
				case Ops.Spherize:
					polyResult = Spherize(opParams);
					break;
				case Ops.Cylinderize:
					polyResult = Cylinderize(opParams);
					break;
				case Ops.Recenter:
					polyResult = Duplicate();
					polyResult.Recenter();
					break;
				case Ops.SitLevel:
					polyResult = SitLevel(opParams.valueA);
					break;
				case Ops.Stretch:
					polyResult = Stretch(opParams.valueA);
					break;
				case Ops.Slice:
					polyResult = Slice(opParams.valueA, opParams.valueB);
					break;
				case Ops.Stack:
					polyResult = Stack(Vector3.up, opParams.valueA, opParams.valueB, 0.1f, opParams.facesel, opParams.tags);
					polyResult.Recenter();
					break;
				case Ops.Weld:
					polyResult = Weld(opParams.valueA);
					break;
			}

			if (Application.isEditor)
			{
				if(polyResult.Faces.Count != polyResult.FaceRoles.Count) Debug.LogError("Incorrect FaceRoles");
				if(polyResult.Faces.Count != polyResult.FaceTags.Count) Debug.LogError("Incorrect FaceTags");
				if(polyResult.Vertices.Count != polyResult.VertexRoles.Count) Debug.LogError("Incorrect VertexRoles");
			}
			return polyResult;

		}

		public void SetFaceRoles(Roles role)
		{
			FaceRoles = Enumerable.Repeat(role, Faces.Count).ToList();
		}
		
		public void SetVertexRoles(Roles role)
		{
			VertexRoles = Enumerable.Repeat(role, Vertices.Count).ToList();
		}
		
		#endregion General Methods

		public List<Face> GetFaces(OpParams o, int limit=-1)
		{
			var matchedFaces = new List<Face>();
			for (var faceIndex = 0; faceIndex < Faces.Count; faceIndex++)
			{
				var face = Faces[faceIndex];
				if (IncludeFace(faceIndex, o.facesel, o.TagListFromString(), o.filterFunc))
				{
					matchedFaces.Add(face);
				}
				
				if (limit!=-1 && matchedFaces.Count >= limit) break;
			}

			return matchedFaces;
		}

		public Face GetFace(OpParams o, int index)
		{
			var faces = GetFaces(o, index - 1);
			return index < faces.Count ? faces[index] : null;
		}

		public Bounds GetBounds()
		{
			var bounds = new Bounds();
			foreach (var vert in Vertices)
			{
				bounds.Encapsulate(vert.Position);
			}
			return bounds;
		}
	}
}