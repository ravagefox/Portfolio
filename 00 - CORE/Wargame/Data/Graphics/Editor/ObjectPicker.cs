// Source: ObjectPicker
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

using System.Linq;
using Engine.Core;
using Engine.Core.Input;
using Engine.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Graphics.DeferredContext;

namespace Wargame.Data.Graphics.Editor
{
    public sealed class ObjectPicker
    {
        public GraphicsDevice GraphicsDevice => this.EditSystem.GraphicsDevice;
        public EditorRenderSystem EditSystem { get; }


        public GameObject SelectedObject { get; private set; }



        public ObjectPicker(EditorRenderSystem editSystem)
        {
            this.EditSystem = editSystem;
        }


        public void Select(Ray raycast)
        {
            float? closestIntersection = float.MaxValue;
            GameObject result = null;

            this.EditSystem.GetCulledActors(this.EditSystem.Actors).ToList().ForEach(a =>
            {
                if (a.GetComponent<GizmoComponent>() is GizmoComponent renderer)
                {

                    var bb = renderer.OBB;
                    var intersectionDistance = bb.Intersects(ref raycast);

                    if (intersectionDistance.HasValue)
                    {
                        if (intersectionDistance < closestIntersection)
                        {
                            closestIntersection = intersectionDistance;
                            result = a;
                        }
                    }
                }
            });

            this.SelectedObject = result;
        }

        public Ray CastRay(
            Matrix view,
            Matrix proj)
        {
            var renderMgr = ServiceProvider.Instance.GetService<RenderManager>();

            var mousePoint = new Point(Mouse.Position.X, Mouse.Position.Y);
            var position = renderMgr.PointToClient(mousePoint);

            var posX = position.X;
            var posY = position.Y;

            var nearsource = new Vector3(posX, posY, 0.0f);
            var farsource = new Vector3(posX, posY, 1.0f);

            var world = Matrix.CreateTranslation(Vector3.Zero);

            var nearPoint = this.GraphicsDevice.Viewport.Unproject(
                nearsource,
                proj,
                view,
                world);
            var farPoint = this.GraphicsDevice.Viewport.Unproject(
                farsource,
                proj,
                view,
                world);

            var direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }

    }
}
