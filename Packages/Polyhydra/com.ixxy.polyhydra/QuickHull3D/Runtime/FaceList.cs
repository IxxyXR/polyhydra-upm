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
    /// <summary>
    /// Maintains a single-linked list of faces for use by QuickHull3D
    /// </summary>
    public class FaceList
    {
        private Face tail;

        /// <summary>
        /// Clears this list.
        /// </summary>
        public void Clear()
        {
            First = tail = null;
        }

        /// <summary>
        /// Adds a vertex to the end of this list.
        /// </summary>
        public void Add(Face vtx)
        {
            if (First == null)
            {
                First = vtx;
            }
            else
            {
                tail.Next = vtx;
            }
            vtx.Next = null;
            tail = vtx;
        }

        public Face First { get; private set; }

        /// <summary>
        /// true if this list is empty.
        /// </summary>
        public bool IsEmpty => First == null;
  
    }

}
