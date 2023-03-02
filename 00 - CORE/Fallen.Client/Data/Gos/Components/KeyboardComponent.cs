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

using Client.Data.Managers;
using Client.Data.Net.Packets.Character;
using Client.Data.Scenes;
using Engine.Core;
using Engine.Core.Input;
using Engine.Data;
using Fallen.Common;

namespace Client.Data.Gos.Components
{
    internal class KeyboardComponent : GameObjectComponent
    {
        public DynamicWorldObject WorldObject => this.GameObject as DynamicWorldObject;

        private Keyboard keyboard;
        private MovementFlags oldMoveFlags;


        public override void Initialize()
        {
            this.keyboard = ServiceProvider.Instance.GetService<Keyboard>();
        }

        public override void Update(Time gameTime)
        {
            this.SendMovementUpdate();
        }

        private void SendMovementUpdate()
        {
            var newMoveFlags = this.HandleKeyInput();
            if (this.oldMoveFlags != newMoveFlags)
            {
                using (var p = new UpdatePacket())
                {
                    p.UpdateFlags = ObjUpdateFlags.Movement;
                    p.CharacterId = this.WorldObject.GameId.HighId;
                    p.ObjType = this.WorldObject.ObjectType;

                    var map = ServiceProvider.Instance.GetService<ISceneContainer>();
                    if (map.ActiveScene is GfxGameWorld gWorld)
                    {
                        p.MapId = gWorld.MapId;

                        p.Enqueue(() =>
                        {
                            p.Write((uint)newMoveFlags);
                        });

                        var netMgr = ServiceProvider.Instance.GetService<NetworkManager>();
                        p.Send(netMgr.WorldClient);
                    }
                }

                this.oldMoveFlags = newMoveFlags;
            }
        }

        private MovementFlags HandleKeyInput()
        {
            var flags = MovementFlags.None;
            if (this.keyboard.IsKeyDown("forward")) { flags |= MovementFlags.Forward; }
            if (this.keyboard.IsKeyDown("backward")) { flags |= MovementFlags.Backward; }
            if (this.keyboard.IsKeyDown("strafeLeft")) { flags |= MovementFlags.StrafeLeft; }
            if (this.keyboard.IsKeyDown("strafeRight")) { flags |= MovementFlags.StrafeRight; }
            if (this.keyboard.IsKeyDown("rotateLeft")) { flags |= MovementFlags.RotateLeft; }
            if (this.keyboard.IsKeyDown("rotateRight")) { flags |= MovementFlags.RotateRight; }

            return flags;
        }
    }
}
