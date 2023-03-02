// Source: DynamicWorldObject
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
using System.Linq;
using Client.Data.Database;
using Client.Data.Gos.Components;
using Client.Data.Managers;
using Engine.Assets.IO;
using Engine.Assets.IO.Database;
using Engine.Core;
using Engine.Data;
using Fallen.Common;
//using static Engine.Assets.IO.MapData;

namespace Client.Data.Gos
{
    public class DynamicWorldObject : GameObject
    {
        public GameID GameId { get; }

        public Transform Transform { get; }

        public virtual WorldObjectType ObjectType { get; }


        public event Action<Time> OnNetworkUpdate;


        public DynamicWorldObject(
            uint objId,
            WorldObjectType objType = WorldObjectType.DynamicObject)
        {
            this.ObjectType = objType;

            this.GameId = new GameID(objId);
            this.Transform = new Transform();
            this.Transform.Scale = 1.0f;

            this.AddComponent(this.Transform);
        }



        //public static DynamicWorldObject FromMapEntry(MapEntry mapEntry)
        //{
        //    var obj = new DynamicWorldObject(mapEntry.Id.HighId, WorldObjectType.Object);
        //    obj.Transform.SetLocation(mapEntry.positionX, mapEntry.positionY, mapEntry.positionZ);
        //    obj.Transform.SetRotation(mapEntry.rotationY, mapEntry.rotationX, mapEntry.rotationZ);
        //    obj.Transform.Scale = mapEntry.scaleX;
        //    obj.Name = mapEntry.Name;

        //    if (mapEntry.modelReference != uint.MaxValue)
        //    {
        //        var dbManager = ServiceProvider.Instance.GetService<DatabaseManager>();
        //        var ids = Query.Select<ContentReferenceTable>(dbManager[nameof(ContentReferenceTable)], (r) =>
        //        {
        //            return r.Id.HighId.CompareTo(mapEntry.modelReference) == 0;
        //        });

        //        if (ids.Any())
        //        {
        //            var f = ids.First();
        //            var texturePath = !string.IsNullOrEmpty(f.TextureRelativePath) ?
        //                              f.TextureRelativePath :
        //                              "Doodads\\Textures\\";

        //            var modelRenderer = new ModelRenderer()
        //            {
        //                TextureRelativeDirectory = texturePath,
        //                AssetPath = f.Name,
        //            };
        //            modelRenderer.VisibleChanged += (o, s) =>
        //            {
        //                var renderManager = ServiceProvider.Instance.GetService<RenderManager>();
        //                if (modelRenderer.Visible)
        //                {
        //                    renderManager.DeferredSystem.ObjectsToRender.Add(obj);
        //                }
        //                else { renderManager.DeferredSystem.ObjectsToRender.Remove(obj); }
        //            };

        //            obj.Enabled = true;
        //            modelRenderer.Visible = true;
        //            obj.AddComponent(modelRenderer);
        //        }
        //    }

        //    return obj;
        //}


        public virtual void NetworkUpdate(Time frameTime)
        {
            OnNetworkUpdate?.Invoke(frameTime);
        }
    }
}
