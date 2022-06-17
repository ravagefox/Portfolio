// Source: KeyboardComponent
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
using Engine.Core.Input;
using Engine.Data;
using Microsoft.Xna.Framework;

namespace Wargame.Data.Gos.Components
{
    public sealed class KeyboardComponent : GameObjectComponent
    {
        public int MoveSpeed { get; set; } = 1;


        private Keyboard keyboard;
        private Vector3 direction;


        public override void Initialize()
        {
            this.keyboard = ServiceProvider.Instance.GetService<Keyboard>();
        }

        public override void Update(Time gameTime)
        {
            var velocityFactory = this.MoveSpeed / 100.0f;
            this.direction = Vector3.Zero;

            this.HandleKeyboardInput();

            var transform = this.GameObject.GetComponent<Transform>();
            if (transform != null)
            {
                var finalVelocity = this.direction * velocityFactory;

                var rotMatrix = Matrix.CreateFromYawPitchRoll(
                    transform.EulerAngles.Y,
                    0.0f,
                    0.0f);
                finalVelocity = Vector3.Transform(finalVelocity, rotMatrix);

                transform.AddVelocity(finalVelocity * (float)gameTime.ElapsedFrameTime.TotalMilliseconds);
            }
        }

        private void HandleKeyboardInput()
        {
            if (this.keyboard.IsKeyDown("forward")) { this.direction += Vector3.Backward; }
            if (this.keyboard.IsKeyDown("backward")) { this.direction += Vector3.Forward; }
            if (this.keyboard.IsKeyDown("strafeLeft")) { this.direction += Vector3.Right; }
            if (this.keyboard.IsKeyDown("strafeRight")) { this.direction += Vector3.Left; }

            // TODO: implement jump
        }

        public Vector3 GetVelocity()
        {
            return this.direction;
        }
    }
}
