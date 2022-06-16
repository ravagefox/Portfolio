// Source: FirstPersonCamera
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

using Engine.Core;
using Engine.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Gos.Components;

namespace Wargame.Data.Gos.CameraTypes
{
    public sealed class FirstPersonCamera : Camera
    {
        public float FOVAngle { get; set; }
        public float FarDistance { get; set; }

        public bool IsMouseCentered
        {
            get => this.mouseLook.IsMouseCenterScreen;
            set => this.mouseLook.IsMouseCenterScreen = value;
        }

        private MouseLookComponent mouseLook;
        private KeyboardComponent keyboardComponent;

        public FirstPersonCamera() : base()
        {
            this.mouseLook = new MouseLookComponent();
            this.keyboardComponent = new KeyboardComponent();
        }

        public override void Initialize()
        {
            this.AddComponent(this.mouseLook);
            this.AddComponent(this.keyboardComponent);

            base.Initialize();
        }

        public override void Update(Time frameTime)
        {
            var transformedReference = Vector3.Transform(Vector3.Backward,
                Matrix.CreateFromQuaternion(this.Transform.Rotation));
            var lookat = this.Transform.Position + transformedReference;

            this.View = Matrix.CreateLookAt(this.Transform.Position, lookat, Vector3.Up);
            this.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(this.FOVAngle),
                this.GetAspectRatio(),
                0.01f,
                this.FarDistance);

            base.Update(frameTime);
        }

        private float GetAspectRatio()
        {
            var gd = ServiceProvider.Instance.GetService<IGraphicsDeviceService>()
                .GraphicsDevice;

            var width = gd.PresentationParameters.BackBufferWidth;
            var height = gd.PresentationParameters.BackBufferHeight;

            return (float)width / height;
        }
    }
}
