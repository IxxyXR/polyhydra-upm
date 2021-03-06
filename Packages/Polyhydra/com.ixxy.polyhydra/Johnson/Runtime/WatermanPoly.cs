/*
* HE_Mesh  Frederik Vanhoutte - www.wblut.com
*
* https://github.com/wblut/HE_Mesh
* A Processing/Java library for creating and manipulating polygonal meshes.
*
* Public Domain: http://creativecommons.org/publicdomain/zero/1.0/
*/

using System.Collections.Generic;
using System.Linq;
using Conway;
using UnityEngine;

namespace Johnson
{
	public static class WatermanPoly
	{
		public static ConwayPoly Build(float R = 1.0f, int root = 2, int c = 0, bool mergeFaces=false)
		{
			ConwayPoly conway;
			
			Vector3[] centers = new Vector3[]
			{
				new Vector3(0, 0, 0),
				new Vector3(0.5f, 0.5f, 0.0f),
				new Vector3(1.0f / 3.0f, 1.0f / 3.0f, 2.0f / 3.0f),
				new Vector3(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f),
				new Vector3(0.5f, 0.5f, 0.5f),
				new Vector3(0.0f, 0.0f, 0.5f),
				new Vector3(1.0f, 0.0f, 0.0f)
			};
			
			if (root < 1)
			{
				return new ConwayPoly();
			}

			if (R == 0)
			{
				return new ConwayPoly();
			}

			Vector3 center = centers[c];
			float radius2;
			float radius;
			switch (c)
			{
				case 0:
					radius2 = 2 * root;
					radius = Mathf.Sqrt(radius2);
					break;
				case 1:
					radius = 2 + 4 * root;
					radius = 0.5f * Mathf.Sqrt(radius);
					break;
				case 2:
					radius = 6 * (root + 1);
					radius = Mathf.Sqrt(radius) / 3.0f;
					break;
				case 3:
					radius = 3 + 6 * root;
					radius = Mathf.Sqrt(radius) / 3.0f;
					break;
				case 4:
					radius = 3 + 8 * (root - 1);
					radius = 0.5f * Mathf.Sqrt(radius);
					break;
				case 5:
					radius = 1 + 4 * root;
					radius = 0.5f * Mathf.Sqrt(radius);
					break;
				case 6:
					radius = 1 + 2 * (root - 1);
					radius = Mathf.Sqrt(radius);
					break;

				default:
					radius = 2 * root;
					radius = Mathf.Sqrt(radius);
					break;
			}

			radius2 = (radius + Mathf.Epsilon) * (radius + Mathf.Epsilon);
			float scale = (float) (R / radius);
			int IR = (int) (radius + 1);

			var points = new List<double>();
			float R2x, R2y, R2;
			for (int i = -IR; i <= IR; i++)
			{
				R2x = (i - center.x) * (i - center.x);
				if (R2x > radius2)
				{
					continue;
				}

				for (int j = -IR; j <= IR; j++)
				{
					R2y = R2x + (j - center.y) * (j - center.y);
					if (R2y > radius2)
					{
						continue;
					}

					for (int k = -IR; k <= IR; k++)
					{
						if ((i + j + k) % 2 == 0)
						{
							R2 = R2y + (k - center.z) * (k - center.z);
							if (R2 <= radius2 && R2 > radius2 - 400)
							{
								var scaledVector = new Vector3(i, j, k) * scale;
								points.AddRange(new[]{(double)scaledVector.x, (double)scaledVector.y, (double)scaledVector.z});
							}
						}
					}
				}
			}
			
			var verts = new List<Vector3>();
			var faces = new List<int[]>();
			
			var hull = new QuickHull3D.Hull();
			hull.Build(points.ToArray(), points.Count / 3);
			verts = hull.GetVertices().Select(v => new Vector3((float)v.x, (float)v.y, (float)v.z)).ToList();
			faces = hull.GetFaces().ToList();
			var faceRoles = Enumerable.Repeat(ConwayPoly.Roles.New, faces.Count);
			var vertexRoles = Enumerable.Repeat(ConwayPoly.Roles.New, verts.Count);
			conway = new ConwayPoly(verts, faces, faceRoles, vertexRoles);
			if (mergeFaces)
			{
				conway = conway.MergeCoplanarFaces(.01f, 2000);
			}

			return conway;
		}
	}
}