// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using QuickHull3D;
// using UnityEngine;
//
// namespace Conway {
//     
//     public static class PlanarizeNeighbourFaces {
//
//         public static List<List<Vertex>>  Run (this Mesh M, ref List<Line> dt_) {
//
//             Plane[] planes = M.GetNgonPlanes();
//             Polyline[] plines = M._Polylines();
//
//
//             //Instead of doing this per face do edges first.
//
//             Dictionary<int, int[]> efDict = M._EFDict();
//             Line[] efLines = new Line[efDict.Count];
//             Line[] efLinesOriginal = new Line[efDict.Count];
//             Dictionary<int, Line> efLinesDict = new Dictionary<int, Line>();
//             double[] angles = new double[efDict.Count];
//             var dt = new DataTree<Line>();
//
//             var debugPlanes0 = new List<Plane>();
//             var debugPlanes1 = new List<Plane>();
//
//             int counter = 0;
//             foreach (var pair in efDict) {
//
//                 int meshE = pair.Key;
//                 Line line = M.TopologyEdges.EdgeLine(meshE);
//                 Point3d mid = line.PointAt(0.5);
//                 int[] f = pair.Value;
//                 efLinesOriginal[counter] = line;
//
//                 Plane pl0 = planes[f[0]];
//                 Plane pl1 = Plane.Unset;
//
//
//
//
//                 //Normal - plane-plane intersection
//                 //This does not make sense
//                 if (f.Length == 2) {
//
//                     Plane zPlane = new Plane(planes[f[0]].Origin, planes[f[0]].ZAxis, planes[f[1]].ZAxis);
//
//
//                     double angle = Vector3d.VectorAngle(planes[f[0]].ZAxis, planes[f[1]].ZAxis, zPlane);//
//                     debugPlanes0.Add(planes[f[0]]);
//                     debugPlanes1.Add(planes[f[1]]);
//                     angles[counter] = angle;
//
//
//                     //If planes are co-planar
//                     /*
//                     if(  Math.Abs(angle) < tol) {//|| true
//
//                     pl1 = NGonsCore.PlaneUtil.BisectorPlane(planes[f[0]], planes[f[1]]);
//                     pl1.Origin = mid;
//
//                     Point3d p = mid;
//                     Vector3d dir = line.Direction;
//                     pl1 = new Plane(p, dir, pl0.ZAxis);
//                     //pl1.Bake(1);
//
//
//                     }else{
//                     pl1 = planes[f[1]];
//                     //pl1.Bake(1);
//                     }
//                     */
//                     pl1 = planes[f[1]];
//
//                 } else { //Naked - plane - edge planes
//                     angles[counter] = 0;
//                     Point3d p = mid;
//                     Vector3d dir = line.Direction;
//                     pl1 = new Plane(p, dir, pl0.ZAxis);
//
//                 }
//
//                 //do this only if angle is small
//                 //lines[j] = NGonsCore.PlaneUtil.PlanePlane(pl0, pl1);
//                 Line l = NGonsCore.PlaneUtil.PlanePlane(pl0, pl1);
//
//                 //If coplanar use the same edges ???
//                 if (Math.Abs(angles[counter]) < 0.01 && f.Length == 2)  {
//                     //Rhino.RhinoApp.WriteLine(angles[counter].ToString());
//                     //l.Bake();
//                     l = line;
//                 }
//
//
//                     // l.Transform(Transform.Translation(mid-l.PointAt(0.5)));
//                     l.Transform(Transform.Scale(l.PointAt(0.5), 1 / l.Length));
//                 efLines[counter++] = l;
//                 efLinesDict.Add(meshE, l);
//             }
//
//             //_Lines = efLines;
//             //_Angles = angles;
//
//
//
//
//             //Collect the lines and intersect them per face
//             //dtLines = new DataTree<Line>();
//             //var dtLinesO = new DataTree<Line>();
//             //var dtPlanes = new DataTree<Plane>();
//
//             Polyline[] plinesPlanar = new Polyline[plines.Length];
//             Line[][] faceLines = new Line[plines.Length][];
//             Line[][] faceLinesO = new Line[plines.Length][];
//
//             for (int i = 0; i < M.Ngons.Count; i++) {
//
//                 int[] fe = M._fe(i);
//
//                 Line[] lnToIntersect = new Line[fe.Length];
//                 for (int j = 0; j < fe.Length; j++) {
//                     lnToIntersect[j] = efLinesDict[fe[j]];
//                 }
//
//
//                 faceLines[i] = lnToIntersect;
//
//
//                 //Display
//                 //dtLines.AddRange(lnToIntersect, new GH_Path(i));
//                 //dtPlanes.Add(planes[i], new GH_Path(i));
//                 dt.AddRange(lnToIntersect, new Grasshopper.Kernel.Data.GH_Path(i));
//             }
//
//             for (int i = 0; i < M.Ngons.Count; i++) {
//
//                 int[] fe = M._fe(i);
//                 faceLinesO[i] = new Line[fe.Length];
//
//
//
//                 for (int j = 0; j < fe.Length; j++) {
//                     int[] feo = M._OppositeFE(i, j, -1);
//
//                     if (feo[0] != -1) {
//                         faceLinesO[i][j] = faceLines[feo[0]][feo[1]];
//                     } else {
//                         faceLinesO[i][j] = faceLines[i].next(j);
//                     }
//
//                 }
//
//                 //dtLinesO.AddRange(faceLinesO[i], new GH_Path(i));
//
//             }
//
//             for (int i = 0; i < M.Ngons.Count; i++) {
//                 plinesPlanar[i] = IntersectLines(faceLines[i], faceLinesO[i], planes[i]);
//             }
//
//             dt_ = dt;
//
//             return plinesPlanar;
//
//
//             //_lineIntersect = dtLines;
//             //_lineIntersectO = dtLinesO;
//             //_planeIntersect = dtPlanes;
//             //_dt = dt;
//             //_efLinesOriginal = efLinesOriginal;
//             //_Plines = plinesPlanar;
//             //_debugPlanes0 = debugPlanes0;
//             //_debugPlanes1 = debugPlanes1;
//             //
//         }
//
//         // <Custom additional code> 
//
//
//         public static Polyline IntersectLines(Line[] lines, Line[] linesO, Plane plane) {
//
//             Polyline pline = new Polyline(lines.Length + 1);
//
//             for (int i = 0; i < lines.Length; i++) {
//                 Line l0 = lines[i];
//                 Line l1 = lines[(i + 1) % lines.Length];
//                 //Line l1 = linesO[i];
//
//                 if (l0.Direction.IsParallelTo(l1.Direction, 0.1) != 0) {
//                     l1 = linesO[i];
//                 }
//
//                 if (l0.Direction.IsParallelTo(l1.Direction, 0.1) != 0 && false) {
//
//                     Plane plane_ = new Plane(plane);
//                     plane_.Origin = l0.PointAt(0.5);
//
//                     //Print("Colinear");
//
//                     //continue;
//                     Line lMid0 = new Line(l0.From, l0.To);
//                     lMid0 = lMid0.Rotate(90, plane_);
//
//                     Line lMid1 = new Line(l1.From, l1.To);
//                     lMid1 = lMid1.Rotate(90, plane_);
//
//                     Line lMid = new Line((lMid0.From + lMid1.From) * 0.5, (lMid0.To + lMid1.To) * 0.5);
//                     lMid = lMid1;
//
//
//                     Point3d pMid = NGonsCore.LineUtil.LineLine(l0, lMid);
//                     pline.Add(pMid);
//
//                     pMid = NGonsCore.LineUtil.LineLine(lMid, l1);
//                     pline.Add(pMid);
//
//                     //Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(l0);
//                     // Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(lMid);
//                 } else {
//
//                     Point3d p = NGonsCore.LineUtil.LineLine(l0, l1);
//                     pline.Add(p);
//                 }
//
//             }
//
//             pline.Close();
//
//             return pline;
//
//         }
//
//     }
// }
