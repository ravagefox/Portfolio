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

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Engine.Core;
using Engine.Core.Input;
using Engine.Data;
using Engine.Graphics;
using Microsoft.Xna.Framework;

namespace Wargame.Data.Gos.Components
{
    public sealed class MouseLookComponent : GameObjectComponent
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);


        public bool IsMouseCenterScreen
        {
            get => this.isMouseCentered && this.IsWindowActivated();
            set
            {
                if (this.isMouseCentered != value)
                {
                    this.isMouseCentered = value;
                }
            }
        }

        public bool IsInverted { get; set; }


        private bool isMouseCentered;
        private Vector2 mousePosition, newPosition;
        private float mouseSpeed = 0.75f;
        private float pitch;
        private float yaw;


        public override void Initialize()
        {
            this.mousePosition = Vector2.Zero;
            this.newPosition = Vector2.Zero;
        }


        public override void Update(Time gameTime)
        {
            if (!(this.GameObject.GetComponent<Transform>() is Transform transform)) { return; }

            if (!this.IsMouseCenterScreen) { return; }

            this.mousePosition = this.PointToClient(new Point(Mouse.Position.X, Mouse.Position.Y)).ToVector2();
            this.newPosition = this.SetCenterMousePosition();

            if (this.newPosition != this.mousePosition)
            {
                if (this.CalculateYawAndPitch(out var yaw, out var pitch))
                {
                    this.yaw -= yaw;
                    this.pitch -= pitch;

                    var maxPitchRadians = (MathUtil.PI / 2) - MathHelper.ToRadians(10.0f);
                    this.pitch = MathUtil.Clamp(this.pitch, -maxPitchRadians, maxPitchRadians);

                    transform.SetRotation(
                        this.yaw,
                        this.pitch,
                        transform.EulerAngles.Z);
                }
            }
            else { this.newPosition = Vector2.Zero; }

            this.mousePosition = this.newPosition;
        }

        private bool CalculateYawAndPitch(out float yaw, out float pitch)
        {
            var diff = this.mousePosition - this.newPosition;

            yaw = this.mouseSpeed * (diff.X / 100);
            pitch = (this.IsInverted ? this.mouseSpeed : -this.mouseSpeed) * (diff.Y / 100);

            return diff.Length() > 0.0f;
        }

        private Point PointToClient(Point point)
        {
            var deviceControl = ServiceProvider.Instance.GetService<GraphicsDeviceControl>();
            var result = deviceControl.PointToScreen(new System.Drawing.Point(point.X, point.Y));

            return new Point(result.X, result.Y);
        }

        private Vector2 SetCenterMousePosition()
        {
            var deviceControl = ServiceProvider.Instance.GetService<GraphicsDeviceControl>();

            var center = new Vector2(deviceControl.Width / 2, deviceControl.Height / 2);
            var point = this.PointToClient(center.ToPoint());

            Cursor.Position = new System.Drawing.Point(point.X, point.Y);
            return new Vector2(Cursor.Position.X, Cursor.Position.Y);
        }

        private bool IsWindowActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            GetWindowThreadProcessId(activatedHandle, out var activeProcId);

            return activeProcId == procId;
        }
    }
}
