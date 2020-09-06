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
    /// Maintains a double-linked list of vertices for use by QuickHull3D
    /// </summary>
    class VertexList
    {
        private Vertex tail;

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
        public void Add(Vertex vtx)
        {
            if (First == null)
            {
                First = vtx;
            }
            else
            {
                tail.Next = vtx;
            }
            vtx.Prev = tail;
            vtx.Next = null;
            tail = vtx;
        }

        /// <summary>
        /// Adds a chain of vertices to the end of this list.
        /// </summary>
        public void AddRange(Vertex vtx)
        {
            if (First == null)
            {
                First = vtx;
            }
            else
            {
                tail.Next = vtx;
            }
            vtx.Prev = tail;
            while (vtx.Next != null)
            {
                vtx = vtx.Next;
            }
            tail = vtx;
        }

        /// <summary>
        /// Removes a vertex from this list.
        /// </summary>
        public void Remove(Vertex vtx)
        {
            if (vtx.Prev == null)
            {
                First = vtx.Next;
            }
            else
            {
                vtx.Prev.Next = vtx.Next;
            }
            if (vtx.Next == null)
            {
                tail = vtx.Prev;
            }
            else
            {
                vtx.Next.Prev = vtx.Prev;
            }
        }

        /// <summary>
        /// Removes a chain of vertices from this list.
        /// </summary>
        public void Remove(Vertex vtx1, Vertex vtx2)
        {
            if (vtx1.Prev == null)
            {
                First = vtx2.Next;
            }
            else
            {
                vtx1.Prev.Next = vtx2.Next;
            }
            if (vtx2.Next == null)
            {
                tail = vtx1.Prev;
            }
            else
            {
                vtx2.Next.Prev = vtx1.Prev;
            }
        }

        /// <summary>
        /// Inserts a vertex into this list before another
        /// specificed vertex.
        /// </summary>
        public void InsertBefore(Vertex vtx, Vertex next)
        {
            vtx.Prev = next.Prev;
            if (next.Prev == null)
            {
                First = vtx;
            }
            else
            {
                next.Prev.Next = vtx;
            }
            vtx.Next = next;
            next.Prev = vtx;
        }

        /// <summary>
        /// The first element in this list.
        /// </summary>
        public Vertex First { get; private set; }

        /// <summary>
        /// True if this list is empty.
        /// </summary>
        public bool IsEmpty => First == null;

    }
}
