// Source: PlayerObject
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
using Engine.Core;
using Engine.Net.Messaging;
using Fallen.Common;

namespace Client.Data.Gos
{
    public sealed class PlayerObject : WorldObject
    {
        public override WorldObjectType ObjectType => WorldObjectType.Player;


        private ModelRenderer modelDisplayComponent;
        private KeyboardComponent keyboardComponent;


        public PlayerObject(uint objId) : base(objId)
        {
            this.modelDisplayComponent = new ModelRenderer();
            this.keyboardComponent = new KeyboardComponent();
        }

        public override void Initialize()
        {
            this.AddComponent(this.modelDisplayComponent);
            this.AddComponent(this.keyboardComponent);

            base.Initialize();
        }

        public override void Update(Time frameTime)
        {
            this.keyboardComponent.MakeNetComponent = true;
            base.Update(frameTime);
        }

        public void OnNetworkReceive(NetMessage msg, ObjUpdateFlags flags)
        {
            if (flags == ObjUpdateFlags.Create)
            {
                this.Name = msg.PacketReader.ReadString();
                this.Transform.SetLocation(msg.PacketReader.ReadSingle(), msg.PacketReader.ReadSingle(), msg.PacketReader.ReadSingle());
                this.Transform.SetRotation(msg.PacketReader.ReadSingle(), 0.0f, 0.0f);
                var scale = msg.PacketReader.ReadSingle();
                this.Transform.SetScale(scale, scale, scale);
                this.MapId = (MapId)msg.PacketReader.ReadUInt32();

            }
        }
    }
}
