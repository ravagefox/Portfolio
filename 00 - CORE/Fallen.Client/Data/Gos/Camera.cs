// Source: Camera
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
using Microsoft.Xna.Framework;

namespace Client.Data.Gos
{
    public enum CameraType
    {
        Free,
        Chase,
        Fixed,
        Cinematic,
    }

    public sealed partial class Camera : DynamicWorldObject
    {
        public static Camera Current { get; private set; }


        public Matrix View { get; set; }

        public Matrix Projection { get; set; }

        public float FOVAngle { get; set; } = 84.4f;

        public float AspectRatio
        {
            get
            {
                var p = this.GraphicsDevice.PresentationParameters;
                return (float)p.BackBufferWidth / p.BackBufferHeight;
            }
        }

        public float ViewDistance
        {
            get => this.mViewDistance;
            set
            {
                if (this.mViewDistance != value)
                {
                    this.mViewDistance =
                        MathF.Clamp(this.mViewDistance, MinViewDistance, MaxViewDistance);

                    this.CreateProjection();
                }
            }
        }

        public CameraType CameraType { get; set; }


        private float mViewDistance;
        private const float MinViewDistance = 100.0f;
        private const float MaxViewDistance = 1000.0f;



        public Camera()
            : base(uint.MaxValue)
        {

        }

        public override void Initialize()
        {
            this.mViewDistance = GameSettings.ViewDistance;

            base.Initialize();
        }

        public void SetCurrent()
        {
            if (Current != this)
            {
                Current = this;
            }
        }

        private void CreateProjection()
        {
            this.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathF.ToRadians(this.FOVAngle),
                this.AspectRatio,
                0.01f,
                this.mViewDistance);
        }

        public override void Update(Time frameTime)
        {
            switch (this.CameraType)
            {
                default:
                case CameraType.Free:
                    {
                        this.UpdateFreeCamera(frameTime);
                    }
                    break;

                case CameraType.Chase:
                    {
                        this.UpdateChaseCamera(frameTime);
                    }
                    break;

                case CameraType.Cinematic:
                    {
                        this.UpdateCinematicCamera(frameTime);
                    }
                    break;
            }

            base.Update(frameTime);
        }


        partial void UpdateFreeCamera(Time frameTime);
        partial void UpdateChaseCamera(Time frameTime);
        partial void UpdateCinematicCamera(Time frameTime);
    }
}
