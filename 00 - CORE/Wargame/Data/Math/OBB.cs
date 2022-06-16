// Source: OBB
/* 
   ---------------------------------------------------------------
                        CREXIUM PTY LTD
   ---------------------------------------------------------------

     The software is provided 'AS IS', without warranty of any kind,
   express or implied, including but not limited to the warrenties
   of merchantability, fitness for a particular purpose and
   noninfringement. In no event shall the authors or copyright
   holders be liable for any claim, damages, or other liability,
   whether in an action of contract, tort, or otherwise, arising
   from, out of or in connection with the software or the use of
   other dealings in the software.
*/

using System;
using Microsoft.Xna.Framework;
using static System.Math;

namespace Wargame.Data.Math
{
    /// <summary>
    /// Bounding volume using an oriented bounding box.
    /// </summary>
    public struct BoundingOrientedBox : IEquatable<BoundingOrientedBox>
    {
        #region Constants
        public const int CornerCount = 8;

        // Epsilon value used in ray tests, where a ray might hit the box almost edge-on.
        private const float RAY_EPSILON = 1e-20F;
        #endregion

        #region Fields
        public Vector3 Center;
        public Vector3 HalfExtent;
        public Quaternion Orientation;
        #endregion

        #region Constructors

        /// <summary>
        /// Create an oriented box with the given center, half-extents, and orientation.
        /// </summary>
        public BoundingOrientedBox(Vector3 center, Vector3 halfExtents, Quaternion orientation)
        {
            this.Center = center;
            this.HalfExtent = halfExtents;
            this.Orientation = orientation;
        }

        /// <summary>
        /// Create an oriented box from an axis-aligned box.
        /// </summary>
        public static BoundingOrientedBox CreateFromBoundingBox(BoundingBox box)
        {
            var mid = (box.Min + box.Max) * 0.5f;
            var halfExtent = (box.Max - box.Min) * 0.5f;
            return new BoundingOrientedBox(mid, halfExtent, Quaternion.Identity);
        }

        /// <summary>
        /// Transform the given bounding box by a rotation around the origin followed by a translation 
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="translation"></param>
        /// <returns>A new bounding box, transformed relative to this one</returns>
        public BoundingOrientedBox Transform(Quaternion rotation, Vector3 translation)
        {
            return new BoundingOrientedBox(Vector3.Transform(this.Center, rotation) + translation,
                                            this.HalfExtent,
                                            this.Orientation * rotation);
        }

        /// <summary>
        /// Transform the given bounding box by a uniform scale and rotation around the origin followed
        /// by a translation
        /// </summary>
        /// <returns>A new bounding box, transformed relative to this one</returns>
        public BoundingOrientedBox Transform(float scale, Quaternion rotation, Vector3 translation)
        {
            return new BoundingOrientedBox(Vector3.Transform(this.Center * scale, rotation) + translation,
                                            this.HalfExtent * scale,
                                            this.Orientation * rotation);
        }

        #endregion

        #region IEquatable implementation

        public bool Equals(BoundingOrientedBox other)
        {
            return this.Center == other.Center && this.HalfExtent == other.HalfExtent && this.Orientation == other.Orientation;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is BoundingOrientedBox)
            {
                var other = (BoundingOrientedBox)obj;
                return this.Center == other.Center && this.HalfExtent == other.HalfExtent && this.Orientation == other.Orientation;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.Center.GetHashCode() ^ this.HalfExtent.GetHashCode() ^ this.Orientation.GetHashCode();
        }

        public static bool operator ==(BoundingOrientedBox a, BoundingOrientedBox b) => Equals(a, b);

        public static bool operator !=(BoundingOrientedBox a, BoundingOrientedBox b) => !Equals(a, b);

        public override string ToString()
        {
            return "{Center:" + this.Center.ToString() +
                   " Extents:" + this.HalfExtent.ToString() +
                   " Orientation:" + this.Orientation.ToString() + "}";
        }

        #endregion

        #region Test vs. BoundingBox

        /// <summary>
        /// Determine if box A intersects box B.
        /// </summary>
        public bool Intersects(ref BoundingBox box)
        {
            var boxCenter = (box.Max + box.Min) * 0.5f;
            var boxHalfExtent = (box.Max - box.Min) * 0.5f;

            var mb = Matrix.CreateFromQuaternion(this.Orientation);
            mb.Translation = this.Center - boxCenter;

            return ContainsRelativeBox(ref boxHalfExtent, ref this.HalfExtent, ref mb) != ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determine if this box contains, intersects, or is disjoint from the given BoundingBox.
        /// </summary>
        public ContainmentType Contains(ref BoundingBox box)
        {
            var boxCenter = (box.Max + box.Min) * 0.5f;
            var boxHalfExtent = (box.Max - box.Min) * 0.5f;

            // Build the 3x3 rotation matrix that defines the orientation of 'other' relative to this box
            Quaternion.Conjugate(ref this.Orientation, out var relOrient);

            var relTransform = Matrix.CreateFromQuaternion(relOrient);
            relTransform.Translation = Vector3.TransformNormal(boxCenter - this.Center, relTransform);

            return ContainsRelativeBox(ref this.HalfExtent, ref boxHalfExtent, ref relTransform);
        }

        /// <summary>
        /// Determine if box A contains, intersects, or is disjoint from box B.
        /// </summary>
        public static ContainmentType Contains(ref BoundingBox boxA, ref BoundingOrientedBox oboxB)
        {
            var boxA_halfExtent = (boxA.Max - boxA.Min) * 0.5f;
            var boxA_center = (boxA.Max + boxA.Min) * 0.5f;
            var mb = Matrix.CreateFromQuaternion(oboxB.Orientation);
            mb.Translation = oboxB.Center - boxA_center;

            return BoundingOrientedBox.ContainsRelativeBox(ref boxA_halfExtent, ref oboxB.HalfExtent, ref mb);
        }

        #endregion

        #region Test vs. BoundingOrientedBox

        /// <summary>
        /// Returns true if this box intersects the given other box.
        /// </summary>
        public bool Intersects(ref BoundingOrientedBox other)
        {
            return this.Contains(ref other) != ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determine whether this box contains, intersects, or is disjoint from
        /// the given other box.
        /// </summary>
        public ContainmentType Contains(ref BoundingOrientedBox other)
        {
            // Build the 3x3 rotation matrix that defines the orientation of 'other' relative to this box
            Quaternion.Conjugate(ref this.Orientation, out var invOrient);
            Quaternion.Multiply(ref invOrient, ref other.Orientation, out var relOrient);

            var relTransform = Matrix.CreateFromQuaternion(relOrient);
            relTransform.Translation = Vector3.Transform(other.Center - this.Center, invOrient);

            return ContainsRelativeBox(ref this.HalfExtent, ref other.HalfExtent, ref relTransform);
        }

        #endregion

        #region Test vs. BoundingFrustum

        /// <summary>
        /// Determine whether this box contains, intersects, or is disjoint from
        /// the given frustum.
        /// </summary>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            // Convert this bounding box to an equivalent BoundingFrustum, so we can rely on BoundingFrustum's
            // implementation. Note that this is very slow, since BoundingFrustum builds various data structures
            // for this test that it caches internally. To speed it up, you could convert the box to a frustum
            // just once and re-use that frustum for repeated tests.
            var temp = this.ConvertToFrustum();
            return temp.Contains(frustum);
        }

        /// <summary>
        /// Returns true if this box intersects the given frustum.
        /// </summary>
        public bool Intersects(BoundingFrustum frustum)
        {
            return this.Contains(frustum) != ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determine whether the given frustum contains, intersects, or is disjoint from
        /// the given oriented box.
        /// </summary>
        public static ContainmentType Contains(BoundingFrustum frustum, ref BoundingOrientedBox obox)
        {
            return frustum.Contains(obox.ConvertToFrustum());
        }

        #endregion

        #region Test vs. BoundingSphere

        /// <summary>
        /// Test whether this box contains, intersects, or is disjoint from the given sphere
        /// </summary>
        public ContainmentType Contains(ref BoundingSphere sphere)
        {
            // Transform the sphere into local box space
            var iq = Quaternion.Conjugate(this.Orientation);
            var localCenter = Vector3.Transform(sphere.Center - this.Center, iq);

            // (dx,dy,dz) = signed distance of center of sphere from edge of box
            var dx = Abs(localCenter.X) - this.HalfExtent.X;
            var dy = Abs(localCenter.Y) - this.HalfExtent.Y;
            var dz = Abs(localCenter.Z) - this.HalfExtent.Z;

            // Check for sphere completely inside box
            var r = sphere.Radius;
            if (dx <= -r && dy <= -r && dz <= -r)
            {
                return ContainmentType.Contains;
            }

            // Compute how far away the sphere is in each dimension
            dx = Max(dx, 0.0f);
            dy = Max(dy, 0.0f);
            dz = Max(dz, 0.0f);

            return (dx * dx) + (dy * dy) + (dz * dz) >= r * r ? ContainmentType.Disjoint : ContainmentType.Intersects;
        }

        /// <summary>
        /// Test whether this box intersects the given sphere
        /// </summary>
        public bool Intersects(ref BoundingSphere sphere)
        {
            // Transform the sphere into local box space
            var iq = Quaternion.Conjugate(this.Orientation);
            var localCenter = Vector3.Transform(sphere.Center - this.Center, iq);

            // (dx,dy,dz) = signed distance of center of sphere from edge of box
            var dx = Abs(localCenter.X) - this.HalfExtent.X;
            var dy = Abs(localCenter.Y) - this.HalfExtent.Y;
            var dz = Abs(localCenter.Z) - this.HalfExtent.Z;

            // Compute how far away the sphere is in each dimension
            dx = Max(dx, 0.0f);
            dy = Max(dy, 0.0f);
            dz = Max(dz, 0.0f);
            var r = sphere.Radius;

            return (dx * dx) + (dy * dy) + (dz * dz) < r * r;
        }

        /// <summary>
        /// Test whether a BoundingSphere contains, intersects, or is disjoint from a BoundingOrientedBox
        /// </summary>
        public static ContainmentType Contains(ref BoundingSphere sphere, ref BoundingOrientedBox box)
        {
            // Transform the sphere into local box space
            var iq = Quaternion.Conjugate(box.Orientation);
            var localCenter = Vector3.Transform(sphere.Center - box.Center, iq);
            localCenter.X = Abs(localCenter.X);
            localCenter.Y = Abs(localCenter.Y);
            localCenter.Z = Abs(localCenter.Z);

            // Check for box completely inside sphere
            var rSquared = sphere.Radius * sphere.Radius;
            if ((localCenter + box.HalfExtent).LengthSquared() <= rSquared)
            {
                return ContainmentType.Contains;
            }

            // (dx,dy,dz) = signed distance of center of sphere from edge of box
            var d = localCenter - box.HalfExtent;

            // Compute how far away the sphere is in each dimension
            d.X = Max(d.X, 0.0f);
            d.Y = Max(d.Y, 0.0f);
            d.Z = Max(d.Z, 0.0f);

            return d.LengthSquared() >= rSquared ? ContainmentType.Disjoint : ContainmentType.Intersects;
        }

        #endregion

        #region Test vs. 0/1/2d primitives

        /// <summary>
        /// Returns true if this box contains the given point.
        /// </summary>
        public bool Contains(ref Vector3 point)
        {
            // Transform the point into box-local space and check against
            // our extents.
            var qinv = Quaternion.Conjugate(this.Orientation);
            var plocal = Vector3.Transform(point - this.Center, qinv);

            return Abs(plocal.X) <= this.HalfExtent.X &&
                   Abs(plocal.Y) <= this.HalfExtent.Y &&
                   Abs(plocal.Z) <= this.HalfExtent.Z;
        }

        /// <summary>
        /// Determine whether the given ray intersects this box. If so, returns
        /// the parametric value of the point of first intersection; otherwise
        /// returns null.
        /// </summary>
        public float? Intersects(ref Ray ray)
        {
            var R = Matrix.CreateFromQuaternion(this.Orientation);

            var TOrigin = this.Center - ray.Position;

            var t_min = -float.MaxValue;
            var t_max = float.MaxValue;

            // X-case
            var axisDotOrigin = Vector3.Dot(R.Right, TOrigin);
            var axisDotDir = Vector3.Dot(R.Right, ray.Direction);

            if (axisDotDir >= -RAY_EPSILON && axisDotDir <= RAY_EPSILON)
            {
                if ((-axisDotOrigin - this.HalfExtent.X) > 0.0 || (-axisDotOrigin + this.HalfExtent.X) > 0.0f)
                {
                    return null;
                }
            }
            else
            {
                var t1 = (axisDotOrigin - this.HalfExtent.X) / axisDotDir;
                var t2 = (axisDotOrigin + this.HalfExtent.X) / axisDotDir;

                if (t1 > t2)
                {
                    (t2, t1) = (t1, t2);
                }

                if (t1 > t_min)
                {
                    t_min = t1;
                }

                if (t2 < t_max)
                {
                    t_max = t2;
                }

                if (t_max < 0.0f || t_min > t_max)
                {
                    return null;
                }
            }

            // Y-case
            axisDotOrigin = Vector3.Dot(R.Up, TOrigin);
            axisDotDir = Vector3.Dot(R.Up, ray.Direction);

            if (axisDotDir >= -RAY_EPSILON && axisDotDir <= RAY_EPSILON)
            {
                if ((-axisDotOrigin - this.HalfExtent.Y) > 0.0 || (-axisDotOrigin + this.HalfExtent.Y) > 0.0f)
                {
                    return null;
                }
            }
            else
            {
                var t1 = (axisDotOrigin - this.HalfExtent.Y) / axisDotDir;
                var t2 = (axisDotOrigin + this.HalfExtent.Y) / axisDotDir;

                if (t1 > t2)
                {
                    (t2, t1) = (t1, t2);
                }

                if (t1 > t_min)
                {
                    t_min = t1;
                }

                if (t2 < t_max)
                {
                    t_max = t2;
                }

                if (t_max < 0.0f || t_min > t_max)
                {
                    return null;
                }
            }

            // Z-case
            axisDotOrigin = Vector3.Dot(R.Forward, TOrigin);
            axisDotDir = Vector3.Dot(R.Forward, ray.Direction);

            if (axisDotDir >= -RAY_EPSILON && axisDotDir <= RAY_EPSILON)
            {
                if ((-axisDotOrigin - this.HalfExtent.Z) > 0.0 || (-axisDotOrigin + this.HalfExtent.Z) > 0.0f)
                {
                    return null;
                }
            }
            else
            {
                var t1 = (axisDotOrigin - this.HalfExtent.Z) / axisDotDir;
                var t2 = (axisDotOrigin + this.HalfExtent.Z) / axisDotDir;

                if (t1 > t2)
                {
                    (t2, t1) = (t1, t2);
                }

                if (t1 > t_min)
                {
                    t_min = t1;
                }

                if (t2 < t_max)
                {
                    t_max = t2;
                }

                if (t_max < 0.0f || t_min > t_max)
                {
                    return null;
                }
            }

            return t_min;
        }

        /// <summary>
        /// Classify this bounding box as entirely in front of, in back of, or
        /// intersecting the given plane.
        /// </summary>
        public PlaneIntersectionType Intersects(ref Plane plane)
        {
            var dist = plane.DotCoordinate(this.Center);

            // Transform the plane's normal into this box's space
            var localNormal = Vector3.Transform(plane.Normal, Quaternion.Conjugate(this.Orientation));

            // Project the axes of the box onto the normal of the plane.  Half the
            // length of the projection (sometime called the "radius") is equal to
            // h(u) * abs(n dot b(u))) + h(v) * abs(n dot b(v)) + h(w) * abs(n dot b(w))
            // where h(i) are extents of the box, n is the plane normal, and b(i) are the 
            // axes of the box.
            var r = Abs(this.HalfExtent.X * localNormal.X)
                    + Abs(this.HalfExtent.Y * localNormal.Y)
                    + Abs(this.HalfExtent.Z * localNormal.Z);

            return dist > r ? PlaneIntersectionType.Front : dist < -r ? PlaneIntersectionType.Back : PlaneIntersectionType.Intersecting;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Return the 8 corner positions of this bounding box.
        ///
        ///     ZMax    ZMin
        ///    0----1  4----5
        ///    |    |  |    |
        ///    |    |  |    |
        ///    3----2  7----6
        ///
        /// The ordering of indices is a little strange to match what BoundingBox.GetCorners() does.
        /// </summary>
        public Vector3[] GetCorners()
        {
            var corners = new Vector3[CornerCount];
            this.GetCorners(corners, 0);
            return corners;
        }

        /// <summary>
        /// Return the 8 corner positions of this bounding box.
        ///
        ///     ZMax    ZMin
        ///    0----1  4----5
        ///    |    |  |    |
        ///    |    |  |    |
        ///    3----2  7----6
        ///
        /// The ordering of indices is a little strange to match what BoundingBox.GetCorners() does.
        /// </summary>
        /// <param name="corners">Array to fill with the eight corner positions</param>
        /// <param name="startIndex">Index within corners array to start writing positions</param>
        public void GetCorners(Vector3[] corners, int startIndex)
        {
            var m = Matrix.CreateFromQuaternion(this.Orientation);
            var hX = m.Left * this.HalfExtent.X;
            var hY = m.Up * this.HalfExtent.Y;
            var hZ = m.Backward * this.HalfExtent.Z;

            var i = startIndex;
            corners[i++] = this.Center - hX + hY + hZ;
            corners[i++] = this.Center + hX + hY + hZ;
            corners[i++] = this.Center + hX - hY + hZ;
            corners[i++] = this.Center - hX - hY + hZ;
            corners[i++] = this.Center - hX + hY - hZ;
            corners[i++] = this.Center + hX + hY - hZ;
            corners[i++] = this.Center + hX - hY - hZ;
            corners[i++] = this.Center - hX - hY - hZ;
        }


        /// <summary>
        /// Determine whether the box described by half-extents hA, axis-aligned and centered at the origin, contains
        /// the box described by half-extents hB, whose position and orientation are given by the transform matrix mB.
        /// The matrix is assumed to contain only rigid motion; if it contains scaling or perpsective the result of
        /// this method will be incorrect.
        /// </summary>
        /// <param name="hA">Half-extents of first box</param>
        /// <param name="hB">Half-extents of second box</param>
        /// <param name="mB">Position and orientation of second box relative to first box</param>
        /// <returns>ContainmentType enum indicating whether the boxes are disjoin, intersecting, or
        /// whether box A contains box B.</returns>
        public static ContainmentType ContainsRelativeBox(ref Vector3 hA, ref Vector3 hB, ref Matrix mB)
        {
            var mB_T = mB.Translation;
            var mB_TA = new Vector3(Abs(mB_T.X), Abs(mB_T.Y), Abs(mB_T.Z));

            // Transform the extents of B
            // TODO: check which coords Right/Up/Back refer to and access them directly. This looks dumb.
            var bX = mB.Right;      // x-axis of box B
            var bY = mB.Up;         // y-axis of box B
            var bZ = mB.Backward;   // z-axis of box B
            var hx_B = bX * hB.X;   // x extent of box B
            var hy_B = bY * hB.Y;   // y extent of box B
            var hz_B = bZ * hB.Z;   // z extent of box B

            // Check for containment first.
            var projx_B = Abs(hx_B.X) + Abs(hy_B.X) + Abs(hz_B.X);
            var projy_B = Abs(hx_B.Y) + Abs(hy_B.Y) + Abs(hz_B.Y);
            var projz_B = Abs(hx_B.Z) + Abs(hy_B.Z) + Abs(hz_B.Z);
            if (mB_TA.X + projx_B <= hA.X && mB_TA.Y + projy_B <= hA.Y && mB_TA.Z + projz_B <= hA.Z)
            {
                return ContainmentType.Contains;
            }

            // Check for separation along the faces of the other box,
            // by projecting each local axis onto the other boxes' axes
            // http://www.cs.unc.edu/~geom/theses/gottschalk/main.pdf
            //
            // The general test form, given a choice of separating axis, is:
            //      sizeA = abs(dot(A.e1,axis)) + abs(dot(A.e2,axis)) + abs(dot(A.e3,axis))
            //      sizeB = abs(dot(B.e1,axis)) + abs(dot(B.e2,axis)) + abs(dot(B.e3,axis))
            //      distance = abs(dot(B.center - A.center),axis))
            //      if distance >= sizeA+sizeB, the boxes are disjoint
            //
            // We need to do this test on 15 axes:
            //      x, y, z axis of box A
            //      x, y, z axis of box B
            //      (v1 cross v2) for each v1 in A's axes, for each v2 in B's axes
            //
            // Since we're working in a space where A is axis-aligned and A.center=0, many
            // of the tests and products simplify away.

            // Check for separation along the axes of box A
            if (mB_TA.X >= hA.X + Abs(hx_B.X) + Abs(hy_B.X) + Abs(hz_B.X))
            {
                return ContainmentType.Disjoint;
            }

            if (mB_TA.Y >= hA.Y + Abs(hx_B.Y) + Abs(hy_B.Y) + Abs(hz_B.Y))
            {
                return ContainmentType.Disjoint;
            }

            if (mB_TA.Z >= hA.Z + Abs(hx_B.Z) + Abs(hy_B.Z) + Abs(hz_B.Z))
            {
                return ContainmentType.Disjoint;
            }

            // Check for separation along the axes box B, hx_B/hy_B/hz_B
            if (Abs(Vector3.Dot(mB_T, bX)) >= Abs(hA.X * bX.X) + Abs(hA.Y * bX.Y) + Abs(hA.Z * bX.Z) + hB.X)
            {
                return ContainmentType.Disjoint;
            }

            if (Abs(Vector3.Dot(mB_T, bY)) >= Abs(hA.X * bY.X) + Abs(hA.Y * bY.Y) + Abs(hA.Z * bY.Z) + hB.Y)
            {
                return ContainmentType.Disjoint;
            }

            if (Abs(Vector3.Dot(mB_T, bZ)) >= Abs(hA.X * bZ.X) + Abs(hA.Y * bZ.Y) + Abs(hA.Z * bZ.Z) + hB.Z)
            {
                return ContainmentType.Disjoint;
            }

            // Check for separation in plane containing an axis of box A and and axis of box B
            //
            // We need to compute all 9 cross products to find them, but a lot of terms drop out
            // since we're working in A's local space. Also, since each such plane is parallel
            // to the defining axis in each box, we know those dot products will be 0 and can
            // omit them.
            Vector3 axis;

            // a.X ^ b.X = (1,0,0) ^ bX
            axis = new Vector3(0, -bX.Z, bX.Y);
            if (Abs(Vector3.Dot(mB_T, axis)) >= Abs(hA.Y * axis.Y) + Abs(hA.Z * axis.Z) + Abs(Vector3.Dot(axis, hy_B)) + Abs(Vector3.Dot(axis, hz_B)))
            {
                return ContainmentType.Disjoint;
            }

            // a.X ^ b.Y = (1,0,0) ^ bY
            axis = new Vector3(0, -bY.Z, bY.Y);
            if (Abs(Vector3.Dot(mB_T, axis)) >= Abs(hA.Y * axis.Y) + Abs(hA.Z * axis.Z) + Abs(Vector3.Dot(axis, hz_B)) + Abs(Vector3.Dot(axis, hx_B)))
            {
                return ContainmentType.Disjoint;
            }

            // a.X ^ b.Z = (1,0,0) ^ bZ
            axis = new Vector3(0, -bZ.Z, bZ.Y);
            if (Abs(Vector3.Dot(mB_T, axis)) >= Abs(hA.Y * axis.Y) + Abs(hA.Z * axis.Z) + Abs(Vector3.Dot(axis, hx_B)) + Abs(Vector3.Dot(axis, hy_B)))
            {
                return ContainmentType.Disjoint;
            }

            // a.Y ^ b.X = (0,1,0) ^ bX
            axis = new Vector3(bX.Z, 0, -bX.X);
            if (Abs(Vector3.Dot(mB_T, axis)) >= Abs(hA.Z * axis.Z) + Abs(hA.X * axis.X) + Abs(Vector3.Dot(axis, hy_B)) + Abs(Vector3.Dot(axis, hz_B)))
            {
                return ContainmentType.Disjoint;
            }

            // a.Y ^ b.Y = (0,1,0) ^ bY
            axis = new Vector3(bY.Z, 0, -bY.X);
            if (Abs(Vector3.Dot(mB_T, axis)) >= Abs(hA.Z * axis.Z) + Abs(hA.X * axis.X) + Abs(Vector3.Dot(axis, hz_B)) + Abs(Vector3.Dot(axis, hx_B)))
            {
                return ContainmentType.Disjoint;
            }

            // a.Y ^ b.Z = (0,1,0) ^ bZ
            axis = new Vector3(bZ.Z, 0, -bZ.X);
            if (Abs(Vector3.Dot(mB_T, axis)) >= Abs(hA.Z * axis.Z) + Abs(hA.X * axis.X) + Abs(Vector3.Dot(axis, hx_B)) + Abs(Vector3.Dot(axis, hy_B)))
            {
                return ContainmentType.Disjoint;
            }

            // a.Z ^ b.X = (0,0,1) ^ bX
            axis = new Vector3(-bX.Y, bX.X, 0);
            if (Abs(Vector3.Dot(mB_T, axis)) >= Abs(hA.X * axis.X) + Abs(hA.Y * axis.Y) + Abs(Vector3.Dot(axis, hy_B)) + Abs(Vector3.Dot(axis, hz_B)))
            {
                return ContainmentType.Disjoint;
            }

            // a.Z ^ b.Y = (0,0,1) ^ bY
            axis = new Vector3(-bY.Y, bY.X, 0);
            if (Abs(Vector3.Dot(mB_T, axis)) >= Abs(hA.X * axis.X) + Abs(hA.Y * axis.Y) + Abs(Vector3.Dot(axis, hz_B)) + Abs(Vector3.Dot(axis, hx_B)))
            {
                return ContainmentType.Disjoint;
            }

            // a.Z ^ b.Z = (0,0,1) ^ bZ
            axis = new Vector3(-bZ.Y, bZ.X, 0);
            return Abs(Vector3.Dot(mB_T, axis)) >= Abs(hA.X * axis.X) + Abs(hA.Y * axis.Y) + Abs(Vector3.Dot(axis, hx_B)) + Abs(Vector3.Dot(axis, hy_B))
                ? ContainmentType.Disjoint
                : ContainmentType.Intersects;
        }

        /// <summary>
        /// Convert this BoundingOrientedBox to a BoundingFrustum describing the same volume.
        ///
        /// A BoundingFrustum is defined by the matrix that carries its volume to the
        /// box from (-1,-1,0) to (1,1,1), so we just need a matrix that carries our box there.
        /// </summary>
        public BoundingFrustum ConvertToFrustum()
        {
            Quaternion.Conjugate(ref this.Orientation, out var invOrientation);
            var sx = 1.0f / this.HalfExtent.X;
            var sy = 1.0f / this.HalfExtent.Y;
            var sz = .5f / this.HalfExtent.Z;
            Matrix.CreateFromQuaternion(ref invOrientation, out var temp);
            temp.M11 *= sx; temp.M21 *= sx; temp.M31 *= sx;
            temp.M12 *= sy; temp.M22 *= sy; temp.M32 *= sy;
            temp.M13 *= sz; temp.M23 *= sz; temp.M33 *= sz;
            temp.Translation = (Vector3.Backward * 0.5f) + Vector3.TransformNormal(-this.Center, temp);

            return new BoundingFrustum(temp);
        }
        #endregion
    }
}
