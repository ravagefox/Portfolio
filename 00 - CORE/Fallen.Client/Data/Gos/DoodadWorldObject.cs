// Source: DoodadWorldObject
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

using Client.Data.Gos.Components;
using Engine.Assets.IO;
using Fallen.Common;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Gos
{
    public sealed class DoodadWorldObject : WorldObject
    {
        public override WorldObjectType ObjectType => WorldObjectType.None;

        public WmoDoodad Doodad { get; private set; }


        public DoodadWorldObject(uint objId) : base(objId)
        {
        }

        public static DoodadWorldObject CreateFrom(WmoDoodad doodad)
        {
            var obj = new DoodadWorldObject(doodad.Id.HighId);
            var modelRenderer = new ModelRenderer();

            obj.Transform.SetLocation(doodad.PositionX, doodad.PositionY, doodad.PositionZ);
            obj.Transform.SetScale(doodad.ScaleX, doodad.ScaleY, doodad.ScaleZ);
            obj.Transform.SetRotation(doodad.RotationX, doodad.RotationY, doodad.RotationZ);

            obj.Name = doodad.Name;

            modelRenderer.Model = obj.Content.Load<Model>("Doodads\\stanford-bunny.m2x");
            modelRenderer.Texture = obj.Content.Load<Texture2D>("Textures\\missing.png");

            obj.AddComponent(modelRenderer);
            obj.Doodad = doodad;

            return obj;
        }
    }
}
