using System.Collections.Generic;
using System.Linq;
using Conway;
using UnityEngine;


namespace Grids
{
	public static class GridEnums
	{
		public enum GridShapes
		{
			Plane,
			Torus,
			Cylinder,
			Cone,
			Sphere,
			Polar,
			// TODO
			// Conic_Frustum,
			// Mobius,
			// Torus_Trefoil,
			// Klein,
			// Klein2,
			// Roman,
			// Roman_Boy,
			// Cross_Cap,
			// Cross_Cap2,
		}
	    
		public enum GridTypes
		{
			Square,
			Isometric,
			Hex,
			Polar,
			U_3_6_3_6,
			U_3_3_3_4_4,
			U_3_3_4_3_4,
			// U_3_3_3_3_6, TODO Fix
			U_3_12_12,
			U_4_8_8,
			U_3_4_6_4,
			U_4_6_12,
		}
		
		public enum KeplerTypes
		{
			K_3_3_3_3_3_3 = 100,
			K_4_4_4_4 = 101,
			K_6_6_6 = 102,
			K_3_3_3_3_6 = 0,
			K_3_3_3_4_4 = 1,
			K_3_3_4_3_4 = 2,
			K_3_4_6_4 = 3,
			K_3_6_3_6 = 4,
			K_3_12_12 = 5,
			K_4_6_12 = 6,
			K_4_8_8 = 7,
			K_3_3_4_12__3_3_3_3_3_3 = 8,
			K_3_3_6_6__3_6_3_6 = 9,
			K_3_4_3_12__3_12_12 = 10,
			K_3_4_4_6__3_6_3_6 = 11,
			Durer1 = 12,
			Durer2 = 13
			// Deca1
		}
		
	}

	public static class Grids
    {
	    public static ConwayPoly MakeGrid(GridEnums.GridTypes gridType, GridEnums.GridShapes gridShape, int p, int q, bool weld=true)
		{
			ConwayPoly conway = null;

			switch (gridType)
			{
				case GridEnums.GridTypes.Square:
					conway = MakeUnitileGrid(1, (int)gridShape, p, q);
					break;
				case GridEnums.GridTypes.Isometric:
					conway = MakeUnitileGrid(2, (int)gridShape, p, q);
					break;
				case GridEnums.GridTypes.Hex:
					conway = MakeUnitileGrid(3, (int)gridShape, p, q);
					break;

				case GridEnums.GridTypes.U_3_6_3_6:
					conway = MakeUnitileGrid(4, (int)gridShape, p, q);
					break;
				case GridEnums.GridTypes.U_3_3_3_4_4:
					conway = MakeUnitileGrid(5, (int)gridShape, p, q);
					break;
				case GridEnums.GridTypes.U_3_3_4_3_4:
					conway = MakeUnitileGrid(6, (int)gridShape, p, q);
					break;
	//			case GridEnums.GridTypes.U_3_3_3_3_6:
	//				conway = MakeUnitileGrid(7, (int)gridShape, p, q);
	//				break;
				case GridEnums.GridTypes.U_3_12_12:
					conway = MakeUnitileGrid(8, (int)gridShape, p, q);
					break;
				case GridEnums.GridTypes.U_4_8_8:
					conway = MakeUnitileGrid(9, (int)gridShape, p, q);
					break;
				case GridEnums.GridTypes.U_3_4_6_4:
					conway = MakeUnitileGrid(10, (int)gridShape, p, q);
					break;
				case GridEnums.GridTypes.U_4_6_12:
					conway = MakeUnitileGrid(11, (int)gridShape, p, q);
					break;

				case GridEnums.GridTypes.Polar:
					conway = MakePolarGrid(p, q);
					break;
			}

			// Welding only seems to work reliably on simpler shapes
			if (weld && gridShape != GridEnums.GridShapes.Plane) conway = conway.Weld(0.001f);

			return conway;
		}



		public static ConwayPoly MakeUnitileGrid(int pattern, int gridShape, int rows = 5, int cols = 5, bool weld=false)
		{
			var ut = new Unitile(pattern, rows, cols, true);

			switch (gridShape)
			{
				case 0:
					ut.plane();
					break;
				case 1:
					ut.torus();
					break;
				case 2:
					ut.conic_frust(1);
					break;
				case 3:
					ut.conic_frust(0.00001f);
					break;
				case 4:
					ut.sphere();
					break;
				case 5:
					ut.conic_frust(0.00001f, cols, 0);
					break;

				// case 5:
				// 	ut.mobius();
				// 	break;
				// case 6:
				// 	ut.torus_trefoil();
				// 	break;
				// case 7:
				// 	ut.klein();
				// 	break;
				// case 8:
				// 	ut.klein2();
				// 	break;
				// case 9:
				// 	ut.roman();
				// 	break;
				// case 10:
				// 	ut.roman_boy();
				// 	break;
				// case 11:
				// 	ut.cross_cap();
				// 	break;
				// case 12:
				// 	ut.cross_cap2();
				// 	break;
			}
			var vertexRoles = Enumerable.Repeat(ConwayPoly.Roles.New, ut.raw_verts.Count);
			var faceRoles = new List<ConwayPoly.Roles>();
			int roleId, isEven, width, height, coloringOffset;

			for (int i = 0; i < ut.raw_faces.Count; i++)
			{
				switch (pattern)
				{
					case 1:
						isEven = cols % 2==0 ? (Mathf.FloorToInt(i / (float) cols)) % 2 : 0;
						roleId = ((i + isEven) % 2) + 2;
						faceRoles.Add((ConwayPoly.Roles)roleId);
						break;
					case 2:
						// int width = Mathf.CeilToInt((rows / Mathf.Sqrt(3))) * 2 + 1;
						// int height = ut.raw_faces.Count / width;
						// isEven = 0;
						// foo = ((i/4/width) + isEven) % 2;
						roleId = i < ut.raw_faces.Count / 2 ? 2 : 3;
						faceRoles.Add((ConwayPoly.Roles)roleId);
						break;
					case 3:
						width = Mathf.CeilToInt((rows / Mathf.Sqrt(3)));
						height = ut.raw_faces.Count / width;
						coloringOffset = i < ut.raw_faces.Count / 2 ? 0 : 1;
						roleId = i / (height / 2);
						if (coloringOffset==1 && width % 3 == 0) coloringOffset += 1;
						if (coloringOffset==1 && width % 3 == 2) coloringOffset += 2;
						roleId += coloringOffset;
						roleId = (roleId % 3) + 2;
						faceRoles.Add((ConwayPoly.Roles)roleId);
						break;
					case 4:
						roleId = ut.raw_faces[i].Count == 3 ? 2 : 3;
						faceRoles.Add((ConwayPoly.Roles)roleId);
						break;
					case 5:  // TODO
						width = rows;
						height = (Mathf.FloorToInt(cols / 4) + 1) * 4;
						roleId = i < ut.raw_faces.Count / 2 ? 2 : 3;
						faceRoles.Add((ConwayPoly.Roles)roleId);
						break;
					case 6:  // TODO
						roleId = ut.raw_faces[i].Count == 3 ? 2 : 3;
						faceRoles.Add((ConwayPoly.Roles)roleId);
						break;
					// case 7:
					// 	foo = i < ut.raw_faces.Count / 2 ? 0 : 1;
					// 	faceRoles.Add((Roles)foo);
					// 	break;
					case 8:  // TODO
						roleId = ut.raw_faces[i].Count == 3 ? 2 : 3;
						faceRoles.Add((ConwayPoly.Roles)roleId);
						break;
					case 9:  // TODO
						roleId = ut.raw_faces[i].Count == 8 ? 2 : 3;
						faceRoles.Add((ConwayPoly.Roles)roleId);
						break;
					case 10:
						switch (ut.raw_faces[i].Count)
						{
							case 3: roleId = 2; break;
							case 4: roleId = 3; break;
							case 6: roleId = 4; break;
							default: roleId = 5; break;
						};
						faceRoles.Add((ConwayPoly.Roles)roleId);
						break;
					case 11:
						switch (ut.raw_faces[i].Count)
						{
							case 4: roleId = 2; break;
							case 6: roleId = 3; break;
							case 12: roleId = 4; break;
							default: roleId = 5; break;
						};
						faceRoles.Add((ConwayPoly.Roles)roleId);
						break;
					default:
						faceRoles.Add((ConwayPoly.Roles)((i % 2) + 2));
						break;
				}
			}

			var poly = new ConwayPoly(ut.raw_verts, ut.raw_faces, faceRoles, vertexRoles);
			poly.Recenter();
			if (gridShape > 0 && weld) poly = poly.Weld(0.001f);
			if (gridShape == 5)
			{
				poly = poly.Rotate(Vector3.left, 90);
			}
			return poly;
		}

		public static ConwayPoly MakeGrid(int rows = 5, int cols = 5, float rowScale = .3f, float colScale = .3f)
		{
			var faceRoles = new List<ConwayPoly.Roles>();

			float rowOffset = rows * rowScale * 0.5f;
			float colOffset = cols * colScale * 0.5f;

			// Count fences not fence poles
			rows++;
			cols++;

			var vertexPoints = new List<Vector3>();
			var faceIndices = new List<List<int>>();


			for (int row = 0; row < rows; row++)
			{
				for (int col = 0; col < cols; col++)
				{
					var pos = new Vector3(-rowOffset + row * rowScale, 0, -colOffset + col * colScale);
					vertexPoints.Add(pos);
				}
			}

			for (int row = 1; row < rows; row++)
			{
				for (int col = 1; col < cols; col++)
				{
					int corner = (row * cols) + col;
					var face = new List<int>
					{
						corner,
						corner - 1,
						corner - cols - 1,
						corner - cols
					};
					faceIndices.Add(face);
					faceRoles.Add((row + col) % 2 == 0 ? ConwayPoly.Roles.New : ConwayPoly.Roles.NewAlt);
				}
			}

			var vertexRoles = Enumerable.Repeat(ConwayPoly.Roles.New, vertexPoints.Count);
			return new ConwayPoly(vertexPoints, faceIndices, faceRoles, vertexRoles);
		}

 		public static ConwayPoly MakeIsoGrid(int cols = 5, int rows = 5)
		{

			float colScale = 1;
			float rowScale = Mathf.Sqrt(3)/2;

			float colOffset = rows * colScale * 0.5f;
			float rowOffset = cols * rowScale * 0.5f;

			var vertexPoints = new List<Vector3>();
			var faceIndices = new List<List<int>>();

			for (int row = 0; row <= rows; row++)
			{
				for (int col = 0; col <= cols; col++)
				{
					var pos = new Vector3(-colOffset + col * colScale, 0, -rowOffset + row * rowScale);
					if (row % 2 > 0)
					{
						pos.x -= colScale/2f;
					}
					vertexPoints.Add(pos);
				}
			}

			for (int row = 0; row < rows; row++)
			{
				for (int col = 0; col < cols; col++)
				{
					int corner = row * (cols + 1) + col;

					if (row % 2 == 0)
					{
						var face1 = new List<int>
						{
							corner,
							corner + cols + 1,
							corner + cols + 2,
						};
						faceIndices.Add(face1);

						var face2 = new List<int>
						{
							corner,
							corner + cols + 2,
							corner + 1
						};
						faceIndices.Add(face2);

					}
					else
					{
						var face1 = new List<int>
						{
							corner,
							corner + cols + 1,
							corner + 1
						};
						faceIndices.Add(face1);

						var face2 = new List<int>
						{
							corner + 1,
							corner + cols + 1,
							corner + cols + 2
						};
						faceIndices.Add(face2);

					}
				}
			}

			var faceRoles = Enumerable.Repeat(ConwayPoly.Roles.New, faceIndices.Count);
			var vertexRoles = Enumerable.Repeat(ConwayPoly.Roles.New, vertexPoints.Count);
			var poly = new ConwayPoly(vertexPoints, faceIndices, faceRoles, vertexRoles);
			poly.Recenter();
			return poly;
		}

		public static ConwayPoly MakeHexGrid(int cols = 4, int rows = 4)
		{
			// Flag if the number of columns is odd.
			bool oddCols = cols % 2 == 1;

			// We measure rows by the 6 triangles in each hex
			cols = ((cols * 3) / 2) + (oddCols?3:1);
			rows = rows * 2 + 2;

			float colScale = 1f;
			float rowScale = Mathf.Sqrt(3)/2;

			float colOffset = rows * colScale * 0.5f;
			float rowOffset = cols * rowScale * 0.5f;

			var vertexPoints = new List<Vector3>();
			var faceIndices = new List<List<int>>();

			for (int row = 0; row <= rows + 2; row++)
			{
				for (int col = 0; col <= cols; col++)
				{
					var pos = new Vector3(-colOffset + col * colScale, 0, -rowOffset + row * rowScale);
					if (row % 2 > 0)
					{
						pos.x -= colScale/2f;
					}
					vertexPoints.Add(pos);
				}
			}

			for (int row = 0; row < rows - 3; row += 2)
			{
				for (int col = 0; col < cols - 3; col += 3)
				{
					int corner = row * (cols + 1) + col;

					var hex1 = new List<int>
					{
						corner,
						corner + cols + 1,
						corner + cols + cols + 2,
						corner + cols + cols + 3,
						corner + cols + 3,
						corner + 1
					};
					faceIndices.Add(hex1);

					if (oddCols && col == cols - 4) continue;
					corner += cols + 3;
					var hex2 = new List<int>
					{
						corner,
						corner + cols,
						corner + cols + cols + 2,
						corner + cols + cols + 3,
						corner + cols + 2,
						corner + 1
					};
					faceIndices.Add(hex2);
				}
			}

			var faceRoles = Enumerable.Repeat(ConwayPoly.Roles.New, faceIndices.Count);
			var vertexRoles = Enumerable.Repeat(ConwayPoly.Roles.New, vertexPoints.Count);
			var poly = new ConwayPoly(vertexPoints, faceIndices, faceRoles, vertexRoles);
			poly.Recenter();
			return poly;
		}

		public static ConwayPoly MakePolarGrid(int sides = 6, int divisions = 4)
		{

			var vertexPoints = new List<Vector3>();
			var faceIndices = new List<List<int>>();

			var faceRoles = new List<ConwayPoly.Roles>();

			float theta = Mathf.PI * 2 / sides;

			int start, end, inc;

			start = 0;
			end = sides;
			inc = 1;
			float radiusStep = 1f / divisions;

			vertexPoints.Add(Vector3.zero);

			for (float radius = radiusStep; radius < 1f + radiusStep; radius += radiusStep)
			{
				for (int i = start; i != end; i += inc)
				{
					float angle = theta * i + theta;
					vertexPoints.Add(new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
				}
			}

			for (int i = 0; i < sides; i++)
			{
				faceIndices.Add(new List<int>{0, (i + 1) % sides + 1, i + 1});
				if (sides % 2 == 0) // Even sides
				{
					faceRoles.Add((ConwayPoly.Roles)(i % 2) + 2);
				}
				else
				{
					int lastCellMod = (i == sides - 1 && sides % 3 == 1) ? 1 : 0;  //  Fudge the last cell to stop clashes in some cases
					faceRoles.Add((ConwayPoly.Roles)((i + lastCellMod) % 3) + 2);
				}
			}

			for (int d = 0; d < divisions - 1; d++)
			{
				for (int i = 0; i < sides; i++)
				{
					int rowStart = d * sides + 1;
					int nextRowStart = (d + 1) * sides + 1;
					faceIndices.Add(new List<int>
					{
						rowStart + i,
						rowStart + (i + 1) % sides,
						nextRowStart + (i + 1) % sides,
						nextRowStart + i
					});
					if (sides % 2 == 0) // Even sides
					{
						faceRoles.Add((ConwayPoly.Roles)((i + d) % 2) + 2);
					}
					else
					{
						int lastCellMod = (i == sides - 1 && sides % 3 == 1) ? 1 : 0;  //  Fudge the last cell to stop clashes in some cases
						faceRoles.Add((ConwayPoly.Roles)((i + d + lastCellMod + 1) % 3) + 2);
					}
				}
			}

			var vertexRoles = Enumerable.Repeat(ConwayPoly.Roles.New, vertexPoints.Count);
			return new ConwayPoly(vertexPoints, faceIndices, faceRoles, vertexRoles);

		}


		
		public static ConwayPoly MakeKepler(GridEnums.KeplerTypes type, GridEnums.GridShapes gridShape, int xRepeats, int yRepeats)
		{

			Vector3 xOffset, yOffset;
			ConwayPoly tile, poly;
			bool offsetAlternateRows = true;
			bool alternateRows = false;

			List<List<List<ConwayPoly.Roles>>> roleSet = null;
			
			switch (type)
			{
				case GridEnums.KeplerTypes.K_3_3_3_3_3_3:
					tile = ConwayPoly._MakePolygon(3, true);
					tile = tile.Rotate(Vector3.up, 30);
					tile.AugmentFace(0, 0, 3);
					xOffset = tile.Vertices[0].Position - tile.Vertices[2].Position;
					yOffset = tile.Vertices[3].Position - tile.Vertices[0].Position;
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() { ConwayPoly.Roles.Existing, ConwayPoly.Roles.New},
					}};
					break;
				case GridEnums.KeplerTypes.K_4_4_4_4:
					tile = ConwayPoly._MakePolygon(4, true);
					tile = tile.Rotate(Vector3.up, 45);
					xOffset = tile.Vertices[0].Position - tile.Vertices[3].Position;
					yOffset = tile.Vertices[0].Position - tile.Vertices[1].Position;
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() { ConwayPoly.Roles.Existing, ConwayPoly.Roles.New},
						new() { ConwayPoly.Roles.New, ConwayPoly.Roles.Existing},
					}};
					break;
				case GridEnums.KeplerTypes.K_6_6_6:
					tile = ConwayPoly._MakePolygon(6, true);
					tile = tile.Rotate(Vector3.up, 30);
					xOffset = tile.Vertices[1].Position - tile.Vertices[5].Position;
					yOffset = tile.Vertices[1].Position - tile.Vertices[3].Position;	
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() { ConwayPoly.Roles.Existing, ConwayPoly.Roles.NewAlt, ConwayPoly.Roles.New},
						new() { ConwayPoly.Roles.New, ConwayPoly.Roles.Existing, ConwayPoly.Roles.NewAlt},
						new() { ConwayPoly.Roles.NewAlt, ConwayPoly.Roles.New, ConwayPoly.Roles.Existing},
					}};
					break;
				case GridEnums.KeplerTypes.K_3_3_3_3_6:
					tile = ConwayPoly._MakePolygon(6, true);
					tile = tile.Rotate(Vector3.up, -19);
					tile.AugmentFace(0, 0, 3);
					tile.AugmentFace(0, 1, 3);
					tile.AugmentFace(0, 2, 3);
					tile.AugmentFace(1, 2, 3);
					tile.AugmentFace(1, 3, 3);
					tile.AugmentFace(2, 2, 3);
					tile.AugmentFace(2, 3, 3);
					tile.AugmentFace(3, 2, 3);
					xOffset = tile.Vertices[8].Position - tile.Vertices[10].Position;
					yOffset = tile.Vertices[10].Position - tile.Vertices[4].Position;
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() {
							ConwayPoly.Roles.ExistingAlt,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt
						}}};
					break;
				case GridEnums.KeplerTypes.K_3_3_3_4_4:
					tile = ConwayPoly._MakePolygon(4, true);
					tile = tile.Rotate(Vector3.up, -45);
					tile.AugmentFace(0, 1, 3);
					tile.AugmentFace(0, 3, 3);
					xOffset = tile.Vertices[2].Position - tile.Vertices[3].Position;
					yOffset = tile.Vertices[4].Position - tile.Vertices[2].Position;
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() {
							ConwayPoly.Roles.Ignored,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt,
						},
						new() {
							ConwayPoly.Roles.Existing,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt,
						},
					}};
					offsetAlternateRows = false;
					break;
				case GridEnums.KeplerTypes.K_3_3_4_3_4:
					tile = ConwayPoly._MakePolygon(3, true);
					tile = tile.Rotate(Vector3.up, 30);
					tile.AugmentFace(0, 2, 4);
					tile.AugmentFace(0, 1, 4);
					tile.AugmentFace(1, 2, 3);
					tile.AugmentFace(2, 0, 3);
					tile.AugmentFace(2, 3, 3);
					xOffset = tile.Vertices[5].Position - tile.Vertices[4].Position;
					yOffset = tile.Vertices[5].Position - tile.Vertices[7].Position;	
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() {
							ConwayPoly.Roles.ExistingAlt,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.Ignored,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.Existing,
						}}};
					break;
				case GridEnums.KeplerTypes.K_3_4_6_4:
					tile = ConwayPoly._MakePolygon(6, true);
					tile = tile.Rotate(Vector3.up, 30);
					tile.AugmentFace(0, 0, 4);
					tile.AugmentFace(0, 1, 4);
					tile.AugmentFace(0, 2, 4);
					tile.AugmentFace(1, 0, 3);
					tile.AugmentFace(2, 0, 3);
					xOffset = tile.Vertices[11].Position - tile.Vertices[4].Position;
					yOffset = tile.Vertices[9].Position - tile.Vertices[3].Position;
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() {
							ConwayPoly.Roles.ExistingAlt,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.Ignored,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.Existing,
						}}};
					break;
				case GridEnums.KeplerTypes.K_3_6_3_6:
					tile = ConwayPoly._MakePolygon(6, true);
					tile = tile.Rotate(Vector3.up, 30);
					tile.AugmentFace(0, 0, 3);
					tile.AugmentFace(0, 1, 3);
					xOffset = tile.Vertices[1].Position - tile.Vertices[4].Position;
					yOffset = tile.Vertices[7].Position - tile.Vertices[2].Position;	
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() {
							ConwayPoly.Roles.ExistingAlt,
							ConwayPoly.Roles.Existing,
							ConwayPoly.Roles.New,
						},
						new() {
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.Existing,
							ConwayPoly.Roles.New,
						},
					}};
					offsetAlternateRows = false;
					break;
				case GridEnums.KeplerTypes.K_3_12_12:
					tile = ConwayPoly._MakePolygon(12, true);
					tile = tile.Rotate(Vector3.up, 45);
					tile.AugmentFace(0, 7, 3);
					tile.AugmentFace(0, 9, 3);
					xOffset = tile.Vertices[4].Position - tile.Vertices[9].Position;
					yOffset = tile.Vertices[2].Position - tile.Vertices[7].Position;
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() {
							ConwayPoly.Roles.ExistingAlt,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt,
						}}};
					break;
				case GridEnums.KeplerTypes.K_4_6_12:
					tile = ConwayPoly._MakePolygon(12, true);
					tile = tile.Rotate(Vector3.up, 45);
					tile.AugmentFace(0, 0, 4);
					tile.AugmentFace(0, 2, 4);
					tile.AugmentFace(0, 4, 4);
					tile.AugmentFace(1, 4, 6);
					tile.AugmentFace(2, 4, 6);
					xOffset = tile.Vertices[16].Position - tile.Vertices[10].Position;
					yOffset = tile.Vertices[15].Position - tile.Vertices[7].Position;	
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() {
							ConwayPoly.Roles.ExistingAlt,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.NewAlt,
						}}};
					break;
				case GridEnums.KeplerTypes.K_4_8_8:
					tile = ConwayPoly._MakePolygon(8, true);
					tile = tile.Rotate(Vector3.up, -22.5f);
					tile.AugmentFace(0, 1, 4);
					xOffset = tile.Vertices[2].Position - tile.Vertices[8].Position;
					yOffset = tile.Vertices[9].Position - tile.Vertices[4].Position;
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() {
							ConwayPoly.Roles.ExistingAlt,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt,
						},
						new() {
							ConwayPoly.Roles.Existing,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt,
						},
					}};
					offsetAlternateRows = false;
					break;
				case GridEnums.KeplerTypes.K_3_3_4_12__3_3_3_3_3_3:
					tile = ConwayPoly._MakePolygon(12, true);
					tile = tile.Rotate(Vector3.up, 15);
					tile.AugmentFace(0, 0, 3);
					tile.AugmentFace(0, 1, 4);
					tile.AugmentFace(0, 2, 3);
					tile.AugmentFace(0, 3, 4);
					tile.AugmentFace(0, 4, 3);
					tile.AugmentFace(0, 5, 4);
					tile.AugmentFace(0, 6, 3);
					tile.AugmentFace(0, 8, 3);
					tile.AugmentFace(0, 10, 3);

					tile.AugmentFace(2, 0, 3);
					tile.AugmentFace(2, 2, 3);

					tile.AugmentFace(4, 0, 3);
					tile.AugmentFace(4, 2, 3);

					tile.AugmentFace(6, 0, 3);
					tile.AugmentFace(6, 2, 3);

					xOffset = tile.Vertices[20].Position - tile.Vertices[10].Position;
					yOffset = tile.Vertices[17].Position - tile.Vertices[8].Position;
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() {
							ConwayPoly.Roles.ExistingAlt,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.Ignored,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.Ignored,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.Ignored,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.NewAlt,
						}}};
					break;
				case GridEnums.KeplerTypes.K_3_3_6_6__3_6_3_6:
					tile = ConwayPoly._MakePolygon(6, true);
					tile = tile.Rotate(Vector3.up, 0);
					tile.AugmentFace(0, 5, 3);
					tile.AugmentFace(0, 0, 3);
					xOffset = tile.Vertices[2].Position - tile.Vertices[5].Position;
					yOffset = tile.Vertices[1].Position - tile.Vertices[3].Position;
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() {
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.Existing,
							ConwayPoly.Roles.Ignored,
						},
						new() {
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.Existing,
							ConwayPoly.Roles.Ignored,
						},
					}};
					offsetAlternateRows = false;
					alternateRows = true;
					break;
				case GridEnums.KeplerTypes.K_3_4_3_12__3_12_12:
					tile = ConwayPoly._MakePolygon(12, true);
					tile = tile.Rotate(Vector3.up, 15);
					tile.AugmentFace(0, 1, 3);
					tile.AugmentFace(0, 0, 3);
					tile.AugmentFace(1, 2, 4);
					tile.AugmentFace(3, 0, 3);
					tile.AugmentFace(3, 3, 3);
					xOffset = tile.Vertices[5].Position - tile.Vertices[10].Position;
					yOffset = tile.Vertices[2].Position - tile.Vertices[7].Position;
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() {
							ConwayPoly.Roles.Existing,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.Ignored,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.NewAlt,
						},
						new() {
							ConwayPoly.Roles.ExistingAlt,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.Ignored,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.NewAlt,
						},
					}};
					offsetAlternateRows = false;
					alternateRows = true;
					break;
				case GridEnums.KeplerTypes.K_3_4_4_6__3_6_3_6:
					tile = ConwayPoly._MakePolygon(6, true);
					tile = tile.Rotate(Vector3.up, 0);
					tile.AugmentFace(0, 5, 3);
					tile.AugmentFace(0, 0, 3);
					tile.AugmentFace(0, 1, 4);
					tile.AugmentFace(2, 0, 4);
					xOffset = tile.Vertices[2].Position - tile.Vertices[5].Position;
					yOffset = tile.Vertices[9].Position - tile.Vertices[3].Position;
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() {
							ConwayPoly.Roles.ExistingAlt,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.Existing,
							ConwayPoly.Roles.ExistingAlt,
						}}};
					break;
				case GridEnums.KeplerTypes.Durer1:
					tile = ConwayPoly._MakePolygon(5, true);
					tile = tile.Rotate(Vector3.up, 54);
					tile.AugmentFace(0, 5, 5);
					tile.AddKite(0, 3, 1, 1);
					xOffset = tile.Vertices[1].Position - tile.Vertices[3].Position;
					yOffset = tile.Vertices[7].Position - tile.Vertices[2].Position;
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() {
							ConwayPoly.Roles.ExistingAlt,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt,
						}}};
					break;
				case GridEnums.KeplerTypes.Durer2:
					tile = ConwayPoly._MakePolygon(5, true);
					tile = tile.Rotate(Vector3.up, 54);
					tile.AugmentFace(0, 5, 5);
					tile.AddKite(0, 3, 1, 1);
					tile.AddRhombus(0, 2, 72);
					xOffset = tile.Vertices[1].Position - tile.Vertices[3].Position;
					yOffset = tile.Vertices[6].Position - tile.Vertices[2].Position;
					roleSet = new List<List<List<ConwayPoly.Roles>>>
					{new() {
						new() {
							ConwayPoly.Roles.ExistingAlt,
							ConwayPoly.Roles.New,
							ConwayPoly.Roles.NewAlt,
							ConwayPoly.Roles.NewAlt,
						}}};
					break;
				// case GridEnums.KeplerTypes.Deca1:
				// 	tile = ConwayPoly._MakePolygon(10, true);
				// tile = tile.Rotate(Vector3.up, angle);
				// 	for (int i = 0; i < 10; i++)
				// 	{
				// 		tile.AugmentFace(0, i, 5);
				// 	}
				// 	for (int i = 1; i < 11; i+=2)
				// 	{
				// 		tile.AugmentFace(i, 3, 10);
				// 	}
				// 	xOffset = tile.Vertices[56].Position - tile.Vertices[35].Position;
				// 	yOffset = tile.Vertices[48].Position - tile.Vertices[64].Position;
				// 	break;
				default:
					xOffset = yOffset = Vector3.zero;
					tile = new ConwayPoly();
					break;
			}
			
			poly = new ConwayPoly();
			var newFaceRoles = new List<ConwayPoly.Roles>();
			var xCentering = (xOffset * (xRepeats - 1)) / 2f;
			var yCentering = (yOffset * (yRepeats - 1)) / 2f;

			int roleCountY = roleSet.Count;
			int roleCountX = roleSet[0].Count;
			int tileCount = tile.Faces.Count;
			for (int y=0; y<yRepeats; y++)
			{
				var rowOffset = offsetAlternateRows ? y % roleCountX : 0;
				for (int x=0; x<xRepeats; x++)
				{
					var colOffset = alternateRows && y%2==0 ? 1 : 0;
					poly.Append(tile, (xOffset * x - xCentering) + (yOffset * y - yCentering));
					newFaceRoles.AddRange(roleSet[y%roleCountY][(x+colOffset)%roleCountX].GetRange(rowOffset, tileCount));
				}
			}

			poly.FaceRoles = newFaceRoles;

			float width = xRepeats * xOffset.x;
			float heightScale = (1f/width) * Mathf.PI;
			float maxHeight = poly.Vertices.Max(x => x.Position.z) * 2f * heightScale;
			
			switch (gridShape)
			{
				case GridEnums.GridShapes.Polar:
				case GridEnums.GridShapes.Cylinder:
				case GridEnums.GridShapes.Cone:
				case GridEnums.GridShapes.Sphere:
					poly.Scale(new Vector3(1f/width, 1, 1));
					poly = ShapeWrap(poly, gridShape, heightScale, maxHeight);
					break;
				case GridEnums.GridShapes.Plane:
					poly = poly.Weld(0.01f);
					break;
			}
			
			return poly;

		}
		
		public static ConwayPoly ShapeWrap(ConwayPoly grid, GridEnums.GridShapes gridShape, float heightScale, float maxHeight)
		{
		
			// Cylinder
			for (var i = 0; i < grid.Vertices.Count; i++)
			{
				var vert = grid.Vertices[i];
				var newPos = vert.Position;
				newPos = new Vector3(
					Mathf.Cos(newPos.x * Mathf.PI * 2) / 2f,
					newPos.z * heightScale,
					Mathf.Sin(newPos.x * Mathf.PI * 2) / 2f
				);
				vert.Position = newPos;
			}
			// Weld cylinder edges.
			// Other shapes might not work with welding tips etc
			grid = grid.Weld(0.01f);  

			// Change cylinder profile for cone etc
			if (gridShape != GridEnums.GridShapes.Plane)
			{
				var heightRange = (maxHeight / 2f);

				for (var i = 0; i < grid.Vertices.Count; i++)
				{
					var vert = grid.Vertices[i];
					var newPos = vert.Position;
					if (gridShape != GridEnums.GridShapes.Cylinder)
					{
						float pinch = 1f;
						var y = 1 - Mathf.InverseLerp(0, heightRange, newPos.y);
						switch (gridShape)
						{
							case GridEnums.GridShapes.Polar:
							case GridEnums.GridShapes.Cone:
								pinch = 1 - y;
								break;
							case GridEnums.GridShapes.Sphere:
								pinch = Mathf.Sqrt(1f - Mathf.Pow(-1 + 2 * y, 2));
								y -= 0.5f;
								break;
						}

						newPos = new Vector3(
							newPos.x * pinch,
							y,
							newPos.z * pinch
						);
					}

					if (gridShape == GridEnums.GridShapes.Polar)
					{
						newPos.y = 0;
					}
					vert.Position = newPos;
				}
			}
			
			return grid;
		}
    }
    

    
}