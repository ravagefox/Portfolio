// Source: EditorRenderSystem
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
using System.Collections.Generic;
using Engine.Core;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Gos;
using Wargame.Data.Graphics.Editor;
using Wargame.Extensions;

namespace Wargame.Data.Graphics.DeferredContext
{
    public sealed class EditorRenderSystem : RenderSystemBase
    {
        public ObjectPicker Picker { get; }

        public IEnumerable<GameObject> Actors { get; set; }



        public Texture2D EditTexture => this.editTarget;

        private RenderTarget2D editTarget;
        private DebugDraw debugDraw;


        public EditorRenderSystem(GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            this.Picker = new ObjectPicker(this);
        }

        protected override void OnInitialize(object sender, EventArgs e)
        {
            this.debugDraw = new DebugDraw(this.GraphicsDevice);

            base.OnInitialize(sender, e);
        }

        protected override void OnResolutionChanged(object sender, EventArgs e)
        {
            this.RecreateRenderTarget2D(ref this.editTarget);

            base.OnResolutionChanged(sender, e);
        }


        public override void Begin()
        {
            this.GraphicsDevice.SetRenderTarget(this.editTarget);
            this.GraphicsDevice.Clear(Color.TransparentBlack);
            this.debugDraw.Begin(Camera.Current.View, Camera.Current.Projection);

            var raycast = this.Picker.CastRay(
                Camera.Current.View,
                Camera.Current.Projection);

            this.Picker.Select(raycast);
        }


        public void DrawHighlighted()
        {
            if (this.Picker.SelectedObject != null)
            {
                if (this.Picker.SelectedObject.GetComponent<GizmoComponent>() is GizmoComponent gizmo)
                {
                    this.debugDraw.DrawWireBox(gizmo.OBB, Color.Blue);
                }
            }
        }

        public void RenderGizmos()
        {
            foreach (var actor in this.GetCulledActors(this.Actors))
            {
                if (actor.GetComponent<GizmoComponent>() is GizmoComponent gizmo)
                {
                    gizmo.RenderGizmo(this.debugDraw);
                }
            }
        }


        public override void End()
        {
            this.debugDraw.End();
            this.GraphicsDevice.SetRenderTarget(null);
        }

        protected override void OnDispose()
        {
            this.debugDraw?.Dispose();
            this.editTarget?.Dispose();
        }


        public IEnumerable<GameObject> GetCulledActors(
            IEnumerable<GameObject> actors)
        {
            var frustum = Camera.Current.GetBoundingFrustum();
            foreach (var obj in actors)
            {
                if (obj.GetComponent<GizmoComponent>() is GizmoComponent gizmo)
                {
                    if (gizmo.OBB.Intersects(frustum) && !(obj is Camera))
                    {
                        yield return obj;
                    }
                }
            }
        }

    }
}
