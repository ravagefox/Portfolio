// Source: BoundingBoxExtensions
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

using Microsoft.Xna.Framework;
using Wargame.Data.Gos.Components;

namespace Wargame.Extensions
{
    public static class BoundingBoxExtensions
    {
        public static BoundingBox Transform(this BoundingBox box, Transform transform)
        {
            var minRotation = Matrix.CreateTranslation(-box.Min) *
                              Matrix.CreateFromQuaternion(transform.Rotation) *
                              Matrix.CreateTranslation(box.Min);
            var maxRotation = Matrix.CreateTranslation(-box.Max) *
                              Matrix.CreateFromQuaternion(transform.Rotation) *
                              Matrix.CreateTranslation(box.Max);

            var min = Vector3.Transform(box.Min, minRotation);
            var max = Vector3.Transform(box.Max, maxRotation);

            return new BoundingBox(min, max);
        }

        private static Vector3 TransformPoint(Vector3 origin, Transform transform)
        {
            var m = Matrix.CreateTranslation(-origin) *
                    Matrix.CreateFromQuaternion(transform.Rotation) *
                    Matrix.CreateTranslation(transform.Position);

            return Vector3.Transform(origin, m);
        }

        public static Engine.Math.BoundingBox ToEngineBoundingBox(this BoundingBox bb)
        {
            return new Engine.Math.BoundingBox(bb.Min.X, bb.Min.Y, bb.Min.Z, bb.Max.X, bb.Max.Y, bb.Max.Z);
        }

        public static bool IntersectRay(this BoundingBox bb, Ray ray, out float? distance)
        {
            distance = bb.Intersects(ray);
            return distance.HasValue;
        }

        public static Vector3 GetCenter(this BoundingBox bb)
        {
            return (bb.Max - bb.Min) / 2;
        }
    }
}
