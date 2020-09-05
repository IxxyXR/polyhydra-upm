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

using System;

namespace QuickHull3D
{
    /// <summary>
    /// A three-element vector. This class is actually a reduced version of the
    /// Vector3d class contained in the author's matlib package (which was partly
    /// inspired by javax.vecmath). Only a mininal number of methods
    /// which are relevant to convex hull generation are supplied here.
    /// </summary>
    /// 
    /// author: John E. Lloyd, Fall 2004
    public class Vector3d
    {
        /// <summary>
        /// Precision of a double.
        /// </summary>
        private const double DOUBLE_PREC = 2.2204460492503131e-16;

        /// <summary>
        /// First element
        /// </summary>
        public double x;

        /// <summary>
        /// Second element
        /// </summary>
        public double y;

        /// <summary>
        /// Third element
        /// </summary>
        public double z;

        /// <summary>
        /// Creates a 3-vector and initializes its elements to 0.
        /// </summary>
        public Vector3d() { }

        /// <summary>
        /// Creates a 3-vector by copying an existing one.
        /// </summary>
        /// <param name="v">vector to be copied</param>
        public Vector3d(Vector3d v)
            => Set(v);


        /// <summary>
        /// Creates a 3-vector with the supplied element values.
        /// </summary>
        /// 
        /// <param name="x">first element</param>
        /// <param name="y">second element</param>
        /// <param name="z">third element</param>
        public Vector3d(double x, double y, double z)
            => Set(x, y, z);


        #region Setter

        /// <summary>
        /// Gets/Sets a single element of this vector.
        /// Elements 0, 1, and 2 correspond to x, y, and z.
        /// </summary>
        /// 
        /// <param name="index">element index</param>
        /// <returns>element value throws IndexOutOfRangeException
        /// if i is not in the range 0 to 2.</returns>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        throw new IndexOutOfRangeException(index.ToString());
                }

            }

            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException(index.ToString());
                }

            }
        }


        /// <summary>
        /// Sets the values of this vector to those of v1.
        /// </summary>
        /// 
        /// <param name="v1">vector whose values are copied</param>
        public void Set(Vector3d v1)
            => Set(v1.x, v1.y, v1.z);


        /// <summary>
        /// Sets the elements of this vector to the prescribed values.
        /// </summary>
        /// 
        /// <param name="x">value for first element</param>
        /// <param name="y">value for second element</param>
        /// <param name="z">value for third element</param>
        public void Set(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Sets the elements of this vector to given value.
        /// </summary>
        /// <param name="value">the value to set</param>
        public void SetAll(double value)
            => Set(value, value, value);

        /// <summary>
        /// Sets the elements of this vector to uniformly distributed
        /// random values in a specified range, using a supplied
        /// random number generator.
        /// </summary>
        /// 
        /// <param name="lower">lower random value (inclusive)</param>
        /// <param name="upper">upper random value (exclusive)</param>
        /// <param name="generator">random number generator</param>
        public void SetRandom(double lower, double upper, Random generator)
        {
            double range = upper - lower;

            x = generator.NextDouble() * range + lower;
            y = generator.NextDouble() * range + lower;
            z = generator.NextDouble() * range + lower;
        }

        #endregion

        /// <summary>
        /// A string representation of this vector, consisting
        /// of the x, y, and z coordinates.
        /// </summary>
        /// 
        /// <return>string representation of the vector</returns>
        public override string ToString()
            => $"{x} {y} {z}";


        /// <summary>
        /// Adds vector v1 to v2 and places the result in this vector.
        /// </summary>
        /// 
        /// <param name="v1">left-hand vector</param>
        /// <param name="v2">right-hand vector</param>
        public void Add(Vector3d v1, Vector3d v2)
            => Set(v1.x + v2.x,
                    v1.y + v2.y,
                    v1.z + v2.z);

        /// <summary>
        /// Adds this vector to v1 and places the result in this vector.
        /// </summary>
        /// 
        /// <param name="v1">right-hand vector</param>
        public void Add(Vector3d v1)
            => Add(this, v1);

        /// <summary>
        /// Subtracts vector v1 from v2 and places the result in this vector.
        /// </summary>
        /// 
        /// <param name="v1">left-hand vector</param>
        /// <param name="v2">right-hand vector</param>
        public void Sub(Vector3d v1, Vector3d v2)
            => Set(v1.x - v2.x,
                    v1.y - v2.y,
                    v1.z - v2.z);

        /// <summary>
        /// Subtracts v1 from this vector and places the result in this vector.
        /// </summary>
        /// 
        /// <param name="v1">right-hand vector</param>
        public void Sub(Vector3d v1)
            => Sub(this, v1);

        /// <summary>
        /// Scales the elements of this vector by <code>s</code>.
        /// </summary>
        /// 
        /// <param name="s">scaling factor</param>
        public void Scale(double s)
            => Set(s * x, s * y, s * z);

        /// <summary>
        /// Scales the elements of vector v1 by <code>s</code> and places
        /// the results in this vector.
        /// </summary>
        /// 
        /// <param name="s">scaling factor</param>
        /// <param name="v1">vector to be scaled</param>
        public void Scale(double s, Vector3d v1)
            => Set(s * v1.x, s * v1.y, s * v1.z);

        /// <summary>
        /// The 2 norm of this vector. This is the square root of the
        /// sum of the squares of the elements.
        /// </summary>
        public double Norm => Math.Sqrt(NormSquared);

        /// <summary>
        /// The square of the 2 norm of this vector. This
        /// is the sum of the squares of the elements.
        /// </summary>
        public double NormSquared
            => Dot(this);

        /// <summary>
        /// Returns the Euclidean distance between this vector and vector v.
        /// </summary>
        /// 
        /// <returns>distance between this vector and v</return>
        public double Distance(Vector3d v)
            => Math.Sqrt(DistanceSquared(v));

        /// <summary>
        /// Returns the squared of the Euclidean distance between this vector
        /// and vector v.
        /// </summary>
        /// 
        /// <return>squared distance between this vector and v</returns>
        public double DistanceSquared(Vector3d v)
            => DeltaSqr(x, v.x) + DeltaSqr(y, v.y) + DeltaSqr(z, v.z);

        private static double DeltaSqr(double v0, double v1)
        {
            var d = v0 - v1;
            return d * d;
        }

        /// <summary>
        /// Returns the dot product of this vector and v1.
        /// </summary>
        /// 
        /// <param name="v1">right-hand vector</param>
        /// <return>dot product</returns>
        public double Dot(Vector3d v1)
            => x * v1.x + y * v1.y + z * v1.z;


        /// <summary>
        /// Normalizes this vector in place.
        /// </summary>
        public void Normalize()
        {
            double lenSqr = NormSquared;
            if (Math.Abs(lenSqr - 1) > 2 * DOUBLE_PREC)
            {
                Scale(1.0 / Math.Sqrt(lenSqr));
            }
        }

        /// <summary>
        /// Computes the cross product of v1 and v2 and places the result
        /// in this vector.
        /// </summary>
        /// 
        /// <param name="v1">left-hand vector</param>
        /// <param name="v2">right-hand vector</param>
        public void Cross(Vector3d v1, Vector3d v2)
            => Set(v1.y * v2.z - v1.z * v2.y,
                    v1.z * v2.x - v1.x * v2.z,
                    v1.x * v2.y - v1.y * v2.x);


    }
}
