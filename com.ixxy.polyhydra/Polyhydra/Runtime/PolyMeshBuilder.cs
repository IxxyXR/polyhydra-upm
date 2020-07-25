using System.Collections.Generic;
using System.Linq;
using Conway;
using UnityEngine;
using Wythoff;


public static class PolyMeshBuilder
{

    public static Color[] DefaultFaceColors =
    {
	    new Color(1.0f, 0.5f, 0.5f),
	    new Color(0.8f, 0.85f, 0.9f),
	    new Color(0.5f, 0.6f, 0.6f),
	    new Color(1.0f, 0.94f, 0.9f),
	    new Color(0.66f, 0.2f, 0.2f),
	    new Color(0.6f, 0.0f, 0.0f),
	    new Color(1.0f, 1.0f, 1.0f),
	    new Color(0.6f, 0.6f, 0.6f),
	    new Color(0.5f, 1.0f, 0.5f),
	    new Color(0.5f, 0.5f, 1.0f),
	    new Color(0.5f, 1.0f, 1.0f),
	    new Color(1.0f, 0.5f, 1.0f),
    };


    // 	new Color(1.0f, 0.75f, 0.75f),
    // 	new Color(1.0f, 0.5f, 0.5f),
    // 	new Color(0.8f, 0.4f, 0.4f),
    // 	new Color(0.8f, 0.8f, 0.8f),
    // 	new Color(0.5f, 0.6f, 0.6f),
    // 	new Color(0.6f, 0.0f, 0.0f),
    // 	new Color(1.0f, 1.0f, 1.0f),
    // 	new Color(0.6f, 0.6f, 0.6f),
    // 	new Color(0.5f, 1.0f, 0.5f),
    // 	new Color(0.5f, 0.5f, 1.0f),
    // 	new Color(0.5f, 1.0f, 1.0f),
    // 	new Color(1.0f, 0.5f, 1.0f),

    public static Mesh BuildMeshFromConwayPoly(
		ConwayPoly conway,
		bool generateSubmeshes,
		Color[] colors = null,
		PolyHydraEnums.ColorMethods colorMethod = PolyHydraEnums.ColorMethods.ByRole)
	{

		Vector2 calcUV(Vector3 point, Vector3 xAxis, Vector3 yAxis)
		{
			float u, v;
			u = Vector3.Project(point, xAxis).magnitude;
			u *= Vector3.Dot(point, xAxis) > 0 ? 1 : -1;
			v = Vector3.Project(point, yAxis).magnitude;
			v *= Vector3.Dot(point, yAxis) > 0  ? 1 : -1;
			return new Vector2(u, v);
		}

		if (colors == null) colors = DefaultFaceColors;
		var target = new Mesh();
		var meshTriangles = new List<int>();
		var meshVertices = new List<Vector3>();
		var meshNormals = new List<Vector3>();
		var meshColors = new List<Color32>();
		var meshUVs = new List<Vector2>();
		var edgeUVs = new List<Vector2>();
		var barycentricUVs = new List<Vector3>();
		var miscUVs = new List<Vector4>();

		List<ConwayPoly.Roles> uniqueRoles = null;
		List<int> uniqueSides = null;
		List<string> uniqueTags = null;

		var submeshTriangles = new List<List<int>>();

		// TODO
		// var hasNaked = conway.HasNaked();

		// Strip down to Face-Vertex structure
		var points = conway.ListVerticesByPoints();
		var faceIndices = conway.ListFacesByVertexIndices();

		// Add faces
		int index = 0;

		if (generateSubmeshes)
		{
			switch (colorMethod)
			{
				case PolyHydraEnums.ColorMethods.ByRole:
					uniqueRoles = new HashSet<ConwayPoly.Roles>(conway.FaceRoles).ToList();
					for (int i = 0; i < uniqueRoles.Count; i++) submeshTriangles.Add(new List<int>());
					break;
				case PolyHydraEnums.ColorMethods.BySides:
					for (int i = 0; i < 16; i++) submeshTriangles.Add(new List<int>());
					break;
				case PolyHydraEnums.ColorMethods.ByTags:
					var flattenedTags = conway.FaceTags.SelectMany(d => d.Select(i => i.Item1));
					uniqueTags = new HashSet<string>(flattenedTags).ToList();
					for (int i = 0; i < uniqueTags.Count + 1; i++) submeshTriangles.Add(new List<int>());
					break;
			}
		}

		for (var i = 0; i < faceIndices.Length; i++)
		{

			var faceIndex = faceIndices[i];
			var face = conway.Faces[i];
			var faceNormal = face.Normal;
			var faceCentroid = face.Centroid;

			ConwayPoly.Roles faceRole = conway.FaceRoles[i];

			// Axes for UV mapping
			var xAxis = face.Halfedge.Vector;
			var yAxis = Vector3.Cross(xAxis, faceNormal);

			Color32 color;

			switch (colorMethod)
			{
				case PolyHydraEnums.ColorMethods.ByRole:
					color = colors[(int)faceRole];
					break;
				case PolyHydraEnums.ColorMethods.BySides:
					color = colors[face.Sides % colors.Length];
					break;
				case PolyHydraEnums.ColorMethods.ByTags:
					var c = new Color();
					if (conway.FaceTags[i].Count > 0)
					{
						string htmlColor = conway.FaceTags[i].First(t => t.Item1.StartsWith("#")).Item1;
						if (!(ColorUtility.TryParseHtmlString(htmlColor, out c)))
						{
							if (!ColorUtility.TryParseHtmlString(htmlColor.Replace("#", ""), out c))
							{
								c = Color.white;
							}
						}
						color = c;
					}
					else
					{
						color = Color.white;
					}
					break;
				default:
					color = Color.white;
					break;
			}

			float faceScale = 0;
			foreach (var v in face.GetVertices())
			{
				faceScale += Vector3.Distance(v.Position, faceCentroid);
			}
			faceScale /= face.Sides;

			var miscUV = new Vector4(faceScale, face.Sides, faceCentroid.magnitude, ((float)i)/faceIndices.Length);

			var faceTris = new List<int>();

			if (face.Sides > 3)
			{
				for (var edgeIndex = 0; edgeIndex < faceIndex.Count; edgeIndex++)
				{

					meshVertices.Add(faceCentroid);
					meshUVs.Add(calcUV(meshVertices[index], xAxis, yAxis));
					faceTris.Add(index++);
					edgeUVs.Add(new Vector2(0, 0));
					barycentricUVs.Add(new Vector3(0, 0, 1));

					meshVertices.Add(points[faceIndex[edgeIndex]]);
					meshUVs.Add(calcUV(meshVertices[index], xAxis, yAxis));
					faceTris.Add(index++);
					edgeUVs.Add(new Vector2(1, 1));
					barycentricUVs.Add(new Vector3(0, 1, 0));

					meshVertices.Add(points[faceIndex[(edgeIndex + 1) % face.Sides]]);
					meshUVs.Add(calcUV(meshVertices[index], xAxis, yAxis));
					faceTris.Add(index++);
					edgeUVs.Add(new Vector2(1, 1));
					barycentricUVs.Add(new Vector3(1, 0, 0));

					meshNormals.AddRange(Enumerable.Repeat(faceNormal, 3));
					meshColors.AddRange(Enumerable.Repeat(color, 3));
					miscUVs.AddRange(Enumerable.Repeat(miscUV, 3));
				}
			}
			else
			{

				meshVertices.Add(points[faceIndex[0]]);
				meshUVs.Add(calcUV(meshVertices[index], xAxis, yAxis));
				faceTris.Add(index++);
				barycentricUVs.Add(new Vector3(0, 0, 1));

				meshVertices.Add(points[faceIndex[1]]);
				meshUVs.Add(calcUV(meshVertices[index], xAxis, yAxis));
				faceTris.Add(index++);
				barycentricUVs.Add(new Vector3(0, 1, 0));

				meshVertices.Add(points[faceIndex[2]]);
				meshUVs.Add(calcUV(meshVertices[index], xAxis, yAxis));
				faceTris.Add(index++);
				barycentricUVs.Add(new Vector3(1, 0, 0));

				edgeUVs.AddRange(Enumerable.Repeat(new Vector2(1, 1), 3));
				meshNormals.AddRange(Enumerable.Repeat(faceNormal, 3));
				meshColors.AddRange(Enumerable.Repeat(color, 3));
				miscUVs.AddRange(Enumerable.Repeat(miscUV, 3));
			}

			if (generateSubmeshes)
			{
				switch (colorMethod)
				{
					case PolyHydraEnums.ColorMethods.ByRole:
						int uniqueRoleIndex = uniqueRoles.IndexOf(faceRole);
						submeshTriangles[uniqueRoleIndex].AddRange(faceTris);
						break;
					case PolyHydraEnums.ColorMethods.BySides:
						submeshTriangles[face.Sides].AddRange(faceTris);
						break;
					case PolyHydraEnums.ColorMethods.ByTags:
						if (conway.FaceTags[i].Count > 0)
						{
							string htmlColor = conway.FaceTags[i].First(t => t.Item1.StartsWith("#")).Item1;
							int uniqueTagIndex = uniqueTags.IndexOf(htmlColor);
							submeshTriangles[uniqueTagIndex + 1].AddRange(faceTris);
						}
						else
						{
							submeshTriangles[0].AddRange(faceTris);

						}
						break;
				}

			}
			else
			{
				meshTriangles.AddRange(faceTris);
			}
		}

		target.vertices = meshVertices.Select(x => Jitter(x)).ToArray();
		target.normals = meshNormals.ToArray();
		if (generateSubmeshes)
		{
			target.subMeshCount = submeshTriangles.Count;
			for (var i = 0; i < submeshTriangles.Count; i++)
			{
				target.SetTriangles(submeshTriangles[i], i);
			}
		}
		else
		{
			target.triangles = meshTriangles.ToArray();
		}

		target.colors32 = meshColors.ToArray();
		target.SetUVs(0, meshUVs);
		target.SetUVs(1, edgeUVs);
		target.SetUVs(2, barycentricUVs);
		target.SetUVs(3, miscUVs);

		target.RecalculateTangents();
		return target;
	}


    public static Mesh BuildMeshFromWythoffPoly(WythoffPoly source, Color[] colors)
    {
	    if (colors == null) colors = DefaultFaceColors;
	    var meshVertices = new List<Vector3>();
	    var meshTriangles = new List<int>();
	    var MeshVertexToVertex = new List<int>(); // Mapping of mesh vertices to poly vertices (one to many as we duplicate verts)
	    var meshColors = new List<Color>();
	    var meshUVs = new List<Vector2>();

	    var mesh = new Mesh();
	    int meshVertexIndex = 0;

	    foreach (Wythoff.Face face in source.faces) {
		    face.CalcTriangles();
	    }

	    for (int faceType = 0; faceType < source.FaceTypeCount; faceType++) {
		    foreach (Wythoff.Face face in source.faces) {
			    if (face.configuration == source.FaceSidesByType[faceType])
			    {
				    var v0 = source.Vertices[face.points[0]].getVector3();
				    var v1 = source.Vertices[face.points[1]].getVector3();
				    var v2 = source.Vertices[face.points[2]].getVector3();
				    var normal = Vector3.Cross(v1 - v0, v2 - v0);
				    var c = face.center.getVector3();
				    var yAxis = c - v0;
				    var xAxis = Vector3.Cross(yAxis, normal);

				    var faceColor = colors[(int) ((face.configuration + 2) % colors.Length)];
				    // Vertices
				    for (int i = 0; i < face.triangles.Length; i++) {
					    Vector3 vcoords = source.Vertices[face.triangles[i]].getVector3();
					    meshVertices.Add(vcoords);
					    meshColors.Add(faceColor);

					    var u = Vector3.Project(vcoords, xAxis).magnitude;
					    var v = Vector3.Project(vcoords, yAxis).magnitude;
					    meshUVs.Add(new Vector2(u, v));

					    meshTriangles.Add(meshVertexIndex);
					    MeshVertexToVertex.Add(face.triangles[i]);
					    meshVertexIndex++;
				    }
			    }
		    }
	    }

	    mesh.vertices = meshVertices.ToArray();
	    mesh.triangles = meshTriangles.ToArray();
	    mesh.colors = meshColors.ToArray();
	    mesh.uv = meshUVs.ToArray();
	    mesh.RecalculateNormals();
	    mesh.RecalculateTangents();
	    mesh.RecalculateBounds();
	    return mesh;

    }



    private static Vector3 Jitter(Vector3 val)
    {
	    // Used to reduce Z fighting for coincident faces
	    float jitter = 0.0002f;
	    return val + new Vector3(Random.value * jitter, Random.value * jitter, Random.value * jitter);
    }

}
