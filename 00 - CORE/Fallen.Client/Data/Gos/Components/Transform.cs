// Source: Transform
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

namespace Client.Data.Gos.Components
{
    public sealed class Transform : GameObjectComponent
    {
        public Vector3 Position { get; set; }

        public Quaternion Rotation =>
            Quaternion.CreateFromYawPitchRoll(this.m_yaw, this.m_pitch, this.m_roll);

        public float Scale { get; set; }

        public Vector3 EulerAngles => new Vector3(this.m_pitch, this.m_yaw, this.m_roll);


        private float m_yaw, m_pitch, m_roll;
        private Vector3 continuousVelocity;
        private Vector3 continuousRotation;


        public Matrix GetWorld()
        {
            return Matrix.CreateScale(this.Scale) *
                   Matrix.CreateFromQuaternion(this.Rotation) *
                   Matrix.CreateTranslation(this.Position);
        }

        public void AddVelocity(Vector3 velocity)
        {
            this.Position += velocity;
        }

        public void SetLocation(float x, float y, float z)
        {
            this.Position = new Vector3(x, y, z);
        }

        public void SetLocation(Vector3 pos)
        {
            this.Position = pos;
        }

        public void SetRotation(float yaw, float pitch, float roll)
        {
            this.m_yaw = yaw;
            this.m_pitch = pitch;
            this.m_roll = roll;
        }

        public void AddRotation(Vector3 rotation)
        {
            this.AddRotation(rotation.X, rotation.Y, rotation.Z);
        }

        public void AddRotation(float yaw = 0.0f, float pitch = 0.0f, float roll = 0.0f)
        {
            this.m_yaw += yaw;
            this.m_pitch += pitch;
            this.m_roll += roll;
        }

        public void SetVelocity(float x, float y, float z)
        {
            this.continuousVelocity = new Vector3(x, y, z);
        }

        public void SetRotationMomentum(
            float yaw = 0.0f,
            float pitch = 0.0f,
            float roll = 0.0f)
        {
            this.continuousRotation = new Vector3(pitch, yaw, roll);
        }

        private void OnNetworkUpdate(Time frameTime)
        {
            if (this.continuousVelocity != Vector3.Zero)
            {
                this.AddVelocity(this.continuousVelocity);
            }

            if (this.continuousRotation != Vector3.Zero)
            {
                this.AddRotation(this.continuousRotation);
            }
        }


        public override void Initialize()
        {
            if (this.GameObject is DynamicWorldObject dynObj)
            {
                dynObj.OnNetworkUpdate += this.OnNetworkUpdate;
            }
        }

        public override void Update(Time gameTime)
        {
        }

    }
}
