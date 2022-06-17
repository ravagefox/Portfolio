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
using Wargame.Data.IO.Map;

namespace Wargame.Data.Gos.CameraTypes
{
    public sealed class FirstPersonCamera : Camera
    {
        public float FOVAngle { get; set; }
        public float FarDistance { get; set; }

        public bool IsMouseCentered { get; set; }
        public bool IsInverted { get; set; }



        private MouseLookComponent mouseLook;
        private KeyboardComponent keyboardComponent;


        public FirstPersonCamera() : base()
        {
            this.mouseLook = new MouseLookComponent();
            this.keyboardComponent = new KeyboardComponent();
        }

        protected override void OnDeserialize(object sender, MapReader reader)
        {
            this.FOVAngle = reader.ReadSingle();
            this.FarDistance = reader.ReadSingle();
            this.IsMouseCentered = reader.ReadBoolean();
            this.IsInverted = reader.ReadBoolean();

            base.OnDeserialize(sender, reader);
        }

        protected override void OnSerialize(object sender, MapWriter writer)
        {
            writer.Write(this.FOVAngle);
            writer.Write(this.FarDistance);
            writer.Write(this.IsMouseCentered);
            writer.Write(this.IsInverted);

            base.OnSerialize(sender, writer);
        }

        public override void Initialize()
        {
            this.AddComponent(this.mouseLook);
            this.AddComponent(this.keyboardComponent);

            base.Initialize();
        }

        public override void Update(Time frameTime)
        {
            this.mouseLook.IsMouseCenterScreen = this.IsMouseCentered;
            this.mouseLook.IsInverted = this.IsInverted;

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
