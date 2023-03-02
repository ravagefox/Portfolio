// Source: TransformComponent
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

using Client.Data.IO.Extensions;
using Engine.Core;
using Microsoft.Xna.Framework;

namespace Client.Data.Gos.Components
{
    public sealed class Transform : WorldObjectComponent
    {
        public Vector3 Position { get; set; }

        public Quaternion Rotation
        {
            get => Quaternion.CreateFromYawPitchRoll(this.yaw, this.pitch, this.roll);
            set
            {
                if (this.Rotation != value)
                {
                    var eulerAngles = value.GetEulerAngles();

                    this.yaw = eulerAngles.Y;
                    this.pitch = eulerAngles.X;
                    this.roll = eulerAngles.Z;
                }
            }
        }

        public Vector3 Scale { get; set; }


        private readonly float MaxPitchRadians = MathF.ToRadians(75.5f);


        private float roll;
        private float pitch;
        private float yaw;


        public Matrix GetWorld()
        {
            return Matrix.CreateTranslation(this.Position) *
                   Matrix.CreateFromQuaternion(this.Rotation) *
                   Matrix.CreateScale(this.Scale);
        }

        public void AddRotation(float yaw, float pitch, float roll)
        {
            this.yaw += yaw;
            this.pitch += pitch;
            this.roll += roll;

            this.pitch = MathF.Clamp(this.pitch, -this.MaxPitchRadians, this.MaxPitchRadians);
        }

        public void AddVelocity(Vector3 velocity)
        {
            this.Position += velocity;
        }

        public void AddScale(Vector3 scale)
        {
            this.Scale += scale;
        }

        public void SetLocation(Vector3 location)
        {
            this.Position = location;
        }

        public void SetLocation(float positionX, float positionY, float positionZ)
        {
            this.Position = new Vector3(positionX, positionY, positionZ);
        }

        public void SetRotation(float yaw, float pitch, float roll)
        {
            this.yaw = yaw;
            this.pitch = pitch;
            this.roll = roll;

            this.pitch = MathF.Clamp(this.pitch, -this.MaxPitchRadians, this.MaxPitchRadians);
        }

        public void SetScale(Vector3 scale)
        {
            this.Scale = scale;
        }

        public void SetScale(float scaleX, float scaleY, float scaleZ)
        {
            this.Scale = new Vector3(scaleX, scaleY, scaleZ);
        }




        public override void Initialize()
        {
        }

        public override void Update(Time gameTime)
        {
        }

    }
}