namespace com.ixxy.polyhydra.Polyhydra.Editor
{
public static class GizmoHelper
{
    public static void DrawGizmos(ConwayPoly poly, bool conwayVertexGizmos, bool faceCenterGizmos, bool faceGizmos)
    {
		float GizmoRadius = .03f;

		if (conwayVertexGizmos && poly!=null)
		{
			Gizmos.color = Color.white;
			if (poly.Vertices != null)
			{
				for (int i = 0; i < poly.Vertices.Count; i++)
				{
					Vector3 vert = poly.Vertices[i].Position;
					Vector3 pos = transform.TransformPoint(vert);
					Gizmos.DrawWireSphere(pos, GizmoRadius);
					Handles.Label(pos + new Vector3(0, .15f, 0), i.ToString());
				}
			}
		}

		if (faceCenterGizmos)
		{
			Gizmos.color = Color.green;
			foreach (var f in poly.Faces)
			{
				Gizmos.DrawWireSphere(transform.TransformPoint(f.Centroid), GizmoRadius);
				Gizmos.DrawRay(transform.TransformPoint(f.Centroid), f.Normal);
			}
		}
		if (faceGizmos)
		{
			int gizmoColor = 0;
			var faces = poly.Faces;
			var verts = poly.Vertices;
			for (int f = 0; f < faces.Count; f++)
			{
				Gizmos.color = Color.white;
				var face = faces[f];
				var faceVerts = face.GetVertices();
				for (int i = 0; i < faceVerts.Count; i++)
				{
					var edgeStart = faceVerts[i];
					var edgeEnd = faceVerts[(i + 1) % faceVerts.Count];
					Gizmos.DrawLine(
						transform.TransformPoint(edgeStart.Position),
						transform.TransformPoint(edgeEnd.Position)
					);
				}

				string label;

				label = $"{f}:{face.Normal}";
				Handles.Label(Vector3.Scale(face.Centroid, transform.lossyScale) + new Vector3(0, .03f, 0), label);
			}
		}


    }
}
}