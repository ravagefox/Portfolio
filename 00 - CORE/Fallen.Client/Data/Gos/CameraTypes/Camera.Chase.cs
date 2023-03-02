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
    public sealed partial class Camera
    {
        public Matrix Follow => Matrix.CreateTranslation(this.followObj.Transform.Position);

        public float TrailDistance
        {
            get => this.mTrailDistance;
            set
            {
                if (this.mTrailDistance != value)
                {
                    this.mTrailDistance =
                        MathF.Clamp(this.mTrailDistance, MinTrailDistance, MaxTrailDistance);
                }
            }
        }

        private float mTrailDistance = 10.0f;
        private DynamicWorldObject followObj;

        private const float MinTrailDistance = 2.0f;
        private const float MaxTrailDistance = 20.0f;


        partial void UpdateChaseCamera(Time frameTime)
        {
            if (this.followObj == null) { return; }

            var lookAtOffset = Vector3.Transform(
                Vector3.Up + (this.Follow.Backward * -this.mTrailDistance), this.Transform.Rotation);
            var camLookAt = this.Follow.Translation;
            var camPosition = camLookAt + lookAtOffset;

            this.Transform.SetLocation(camPosition);
            this.View = Matrix.CreateLookAt(camPosition, camLookAt, Vector3.Up);
        }

        public void SetFollow(DynamicWorldObject obj)
        {
            this.followObj = obj;
        }

        public DynamicWorldObject GetFollowObject()
        {
            return this.followObj;
        }
    }
}
