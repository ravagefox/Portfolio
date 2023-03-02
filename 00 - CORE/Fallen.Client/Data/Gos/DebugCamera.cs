// Source: DebugCamera
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

using System.Windows.Forms;
using Engine.Core;
using Engine.Core.Input;
using Engine.Data;
using Engine.Graphics.Linq;
using Fallen.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Gos
{
#if DEBUG
    internal class DebugCamera : DynamicWorldObject, ICamera
    {
        public Matrix View => this.m_view;

        public Matrix Projection => this.m_projection;

        public Vector3 CameraEye => this.Transform.Position;

        public int MoveSpeed { get; set; }

        public float AspectRatio
        {
            get
            {
                var g = ServiceProvider.Instance.GetService<IGraphicsDeviceService>()
                    .GraphicsDevice;

                return (float)g.GetBounds().Width / g.GetBounds().Height;
            }
        }

        public bool IsInverted { get; set; }
        public float MouseSpeed { get; set; }


        private Matrix m_view;
        private Matrix m_projection;
        private Keyboard m_keyboard;
        private Mouse m_mouse;
        private Vector3 velocity;

        private Vector2 mousePosition;
        private Vector2 oldMousePosition;
        private float pitch;
        private float yaw;


        public DebugCamera()
            : base(uint.MaxValue, WorldObjectType.GameObject)
        {
            this.MoveSpeed = 15;
            this.MouseSpeed = 0.8f;
        }


        public override void Initialize()
        {
            this.m_mouse = ServiceProvider.Instance.GetService<Mouse>();
            this.m_keyboard = ServiceProvider.Instance.GetService<Keyboard>();

            base.Initialize();
        }

        public BoundingFrustum GetBoundingFrustum()
        {
            return new BoundingFrustum(this.View * this.Projection);
        }

        public override void Update(Time frameTime)
        {
            this.velocity = Vector3.Zero;
            this.yaw = 0.0f;
            this.pitch = 0.0f;

            this.HandleKeyboardInput(frameTime);
            this.HandleMouseInput(frameTime);

            if (this.velocity != Vector3.Zero)
            {
                this.Transform.AddVelocity(this.velocity);
            }

            this.RecreateViewProjection();
            this.Transform.AddRotation(this.yaw, this.pitch);

            base.Update(frameTime);
        }

        private void RecreateViewProjection()
        {
            var transformedRef = Vector3.Transform(Vector3.Backward,
                Matrix.CreateFromQuaternion(this.Transform.Rotation));
            var lookat = this.Transform.Position + transformedRef;

            this.m_view = Matrix.CreateLookAt(this.Transform.Position, lookat, Vector3.Up);
            this.m_projection = Matrix.CreatePerspectiveFieldOfView(
                MathF.ToRadians(84.4f),
                this.AspectRatio,
                0.01f,
                100.0f);
        }

        private void HandleKeyboardInput(Time frameTime)
        {
            var direction = Vector3.Transform(Vector3.Backward,
                Matrix.CreateFromQuaternion(this.Transform.Rotation));
            var strafe = Vector3.Transform(Vector3.Right,
                Matrix.CreateFromQuaternion(this.Transform.Rotation));

            var l_speedConst = this.MoveSpeed / 100.0f;
            var speed = l_speedConst *
                        (float)frameTime.ElapsedFrameTime.TotalMilliseconds;

            if (this.m_keyboard.IsKeyDown("forward"))
            {
                this.velocity += direction * speed;
            }
            if (this.m_keyboard.IsKeyDown("backward"))
            {
                this.velocity -= direction * speed;
            }
            if (this.m_keyboard.IsKeyDown("strafeLeft"))
            {
                this.velocity += strafe * speed;
            }
            if (this.m_keyboard.IsKeyDown("strafeRight"))
            {
                this.velocity -= strafe * speed;
            }
        }

        private void HandleMouseInput(Time frameTime)
        {
            this.mousePosition = new Vector2(Mouse.Position.X, Mouse.Position.Y);

            if (this.mousePosition != this.oldMousePosition)
            {
                if (this.m_mouse.IsButtonDown(MouseButtons.Left) &&
                    this.m_mouse.IsButtonDown(MouseButtons.Right))
                {
                    if (this.CalculateYawAndPitch(out var yaw, out var pitch))
                    {
                        this.yaw += yaw;
                        this.pitch += pitch;
                    }
                }

                this.oldMousePosition = this.mousePosition;
            }
        }

        private bool CalculateYawAndPitch(out float yaw, out float pitch)
        {
            var diff = this.oldMousePosition - this.mousePosition;
            yaw = this.MouseSpeed * (diff.X / 100.0f);
            pitch = (this.IsInverted ? this.MouseSpeed : -this.MouseSpeed) * (diff.Y / 100.0f);

            return diff.Length() > 0.0f;
        }
    }
#endif
}
