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
using Wargame.Data.IO;
using Wargame.Data.IO.Map;

namespace Wargame.Data.Gos.Components
{
    public sealed class Transform : GameObjectComponent, IObjectSerialization
    {
        public bool UseUniformScale { get; set; }


        public Vector3 Position { get; set; }

        public Quaternion Rotation => Quaternion.CreateFromYawPitchRoll(this.yaw, this.pitch, this.roll);

        public Vector3 Scale
        {
            get => this.UseUniformScale ? Vector3.One * this.uniformScale : new Vector3(this.scaleX, this.scaleY, this.scaleZ);
            set
            {
                if (this.UseUniformScale)
                {
                    this.uniformScale = value.X;
                }
                else
                {
                    this.scaleX = value.X;
                    this.scaleY = value.Y;
                    this.scaleZ = value.Z;
                }
            }
        }

        public Vector3 EulerAngles =>
            new Vector3(this.pitch,
                        this.yaw,
                        this.roll);

        private float scaleX = 1, scaleY = 1, scaleZ = 1;
        private float uniformScale = 1;
        private float yaw, pitch, roll;


        public Matrix GetWorld()
        {
            return Matrix.CreateTranslation(this.Position) *
                   Matrix.CreateFromQuaternion(this.Rotation) *
                   Matrix.CreateScale(this.Scale);
        }


        public void SetLocation(Vector3 position)
        {
            this.Position = position;
        }

        public void AddVelocity(Vector3 velocity)
        {
            this.Position += velocity;
        }

        public void SetRotation(float yaw, float pitch, float roll)
        {
            this.yaw = yaw;
            this.pitch = pitch;
            this.roll = roll;
        }

        public void AddRotation(float yaw, float pitch, float roll)
        {
            this.yaw += yaw;
            this.pitch += pitch;
            this.roll += roll;
        }

        public void SetScale(Vector3 scale)
        {
            this.scaleX = scale.X;
            this.scaleY = scale.Y;
            this.scaleZ = scale.Z;

            if (this.UseUniformScale) { this.SetUniformScale(scale.X); }
        }

        public void SetUniformScale(float scale)
        {
            this.uniformScale = scale;
        }


        public override void Initialize()
        {
            this.uniformScale = 1.0f;
            this.scaleX = 1.0f;
            this.scaleY = 1.0f;
            this.scaleZ = 1.0f;
        }

        public override void Update(Time gameTime)
        {
        }

        public void Deserialize(MapReader mapReader)
        {
            this.SetLocation(mapReader.ReadVector3());
            this.SetRotation(mapReader.ReadSingle(), mapReader.ReadSingle(), mapReader.ReadSingle());

            var useUniform = mapReader.ReadBoolean();
            if (useUniform)
            {
                this.UseUniformScale = useUniform;
                this.SetUniformScale(mapReader.ReadSingle());
            }
            else { this.SetScale(mapReader.ReadVector3()); }
        }

        public void Serialize(MapWriter mapWriter)
        {
            mapWriter.Write(this.Position);

            mapWriter.Write(this.yaw);
            mapWriter.Write(this.pitch);
            mapWriter.Write(this.roll);

            mapWriter.Write(this.UseUniformScale);
            if (this.UseUniformScale)
            {
                mapWriter.Write(this.uniformScale);
            }
            else { mapWriter.Write(this.Scale); }
        }
    }
}