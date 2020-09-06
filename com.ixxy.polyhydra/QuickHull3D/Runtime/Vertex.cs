﻿/*
 * Copyright John E. Lloyd, 2004. All rights reserved. Permission to use,
 * copy, modify and redistribute is granted, provided that this copyright
 * notice is retained and the author is given credit whenever appropriate.
 *
 * This  software is distributed "as is", without any warranty, including 
 * any implied warranty of merchantability or fitness for a particular
 * use. The author assumes no responsibility for, and shall not be liable
 * for, any special, indirect, or consequential damages, or any damages
 * whatsoever, arising out of or in connection with the use of this
 * software.
 */

namespace QuickHull3D
{
    /// </summary>
    /// Represents vertices of the hull, as well as the points from
    /// which it is formed.
    /// </summary>
    /// author John E. Lloyd, Fall 2004
    /// 
    public class Vertex
    {
        /// <summary>
        /// Spatial point associated with this vertex.
        /// </summary>
        public Point3d Point { get; set; }

        /// <summary>
        /// Back index into an array.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// List forward link.
        /// </summary>
        public Vertex Prev { get; set; }

        /// <summary>
        /// List backward link.
        /// </summary>
        public Vertex Next { get; set; }

        /// <summary>
        /// Current face that this vertex is outside of.
        /// </summary>
        public Face Face { get; set; }

        /// <summary>
        /// Constructs a vertex and sets its coordinates to 0.
        /// </summary>
        public Vertex()
        {
            Point = new Point3d();
        }

        /// <summary>
        /// Constructs a vertex with the specified coordinates
        /// and index.
        /// </summary>
        public Vertex(float x, float y, float z, int idx)
        {
            Point = new Point3d(x, y, z);
            Index = idx;
        }

    }
}
