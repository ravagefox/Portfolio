// Source: MouseLookComponent
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
using Engine.Data.IO;
using Microsoft.Xna.Framework;

namespace Client.Data.Gos.Components
{
    public sealed class MouseLookComponent : WorldObjectComponent
    {
        public float MouseSensitivity
        {
            get => this.mSensitivity;
            set
            {
                if (this.mSensitivity != value)
                {
                    this.mSensitivity = value;
                }
            }
        }

        public bool IsMouseEnabled { get; private set; }

        private const float sensitivityOffset = 47.5f;

        private Mouse mouse;
        private float mSensitivity;
        private bool invertLook;
        private bool enableEasing;
        private float easeSpeed;
        private Vector2 curPosition, lastMousePosition;
        private bool noInterference;
        private float yaw, pitch;


        public override void Initialize()
        {
            this.mouse = ServiceProvider.Instance.GetService<Mouse>();
            var cfg = new WtfConfig("Data\\Config.wtf");

            this.mSensitivity = cfg.GetValue<float>("mouseSensitivity");
            this.invertLook = cfg.GetValue<int>("invertMouseLook") == 1;
            this.enableEasing = cfg.GetValue<int>("mouseeasing") == 1;
            this.easeSpeed = cfg.GetValue<float>("easeSpeed");
            this.IsMouseEnabled = cfg.GetValue<int>("mouseInput") == 1;
        }

        public override void Update(Time gameTime)
        {
            this.curPosition = new Vector2(Mouse.Position.X, Mouse.Position.Y);
            this.noInterference = true;
            if (this.IsMouseRotationAllowed() && this.IsMouseEnabled)
            {
                this.CreatePitchAndYaw(this.curPosition, out this.yaw, out this.pitch);

                this.WorldObject.Transform.AddRotation(this.yaw, this.pitch, 0.0f);
            }

            this.lastMousePosition = this.curPosition;
        }

        private bool IsMouseRotationAllowed()
        {
            return this.mouse.IsButtonDown(MouseButtons.Left) &&
                   this.noInterference;
        }

        private void CreatePitchAndYaw(
            Vector2 curPosition,
            out float yaw,
            out float pitch)
        {
            var sensitivity = this.mSensitivity / sensitivityOffset;
            var pitchAmount = (curPosition.Y - this.lastMousePosition.Y) * sensitivity;
            pitchAmount = this.invertLook ? -pitchAmount : pitchAmount;

            yaw = -(curPosition.X - this.lastMousePosition.X) * sensitivity;
            pitch = pitchAmount;
        }
    }
}
