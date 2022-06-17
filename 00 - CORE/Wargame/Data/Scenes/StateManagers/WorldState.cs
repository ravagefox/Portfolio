// Source: WorldState
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

using System.Linq;
using Engine.Core;
using Engine.Data;
using Wargame.Data.Graphics;
using Wargame.Data.IO;
using Wargame.Data.IO.Map;
using Wargame.Data.IO.Map.Serializers;

namespace Wargame.Data.Scenes.StateManagers
{
    public sealed class WorldState : GameState
    {
        public string StartLevel { get; set; }


        private RenderManager renderManager;


        public WorldState(RenderManager renderManager) : base("WorldState")
        {
            this.renderManager = renderManager;
        }


        private void LoadLevel(string path)
        {
            this.IsLoading = true;
            if (this.GameObjects.Count > 0)
            {
                this.GameObjects.ToList().ForEach(a => a.Unload());
                this.GameObjects.Clear();
            }

            var loader = ServiceProvider.Instance.GetService<IStreamContainer>();
            if (loader.CanOpenStream(path))
            {
                using (var stream = loader.OpenStream(path))
                {
                    var sceneSerializer = new SceneSerializer();
                    var lightSerializer = new LightSerializer();

                    using (var reader = new MapReader(stream))
                    {
                        sceneSerializer.Deserialize(reader);
                        lightSerializer.Deserialize(reader);
                    }

                    sceneSerializer.Actors.ToList().ForEach(a => this.GameObjects.Add(a));
                    lightSerializer.Lights.ToList().ForEach(a => this.renderManager.LightSystem.Add(a));
                }
            }
        }

        public override void InitializeState()
        {
        }

        public override void Apply(Time frameTime)
        {
            if (this.IsLoading)
            {
                // TODO: implement loading screen attributes
                return;
            }

            if (this.Visible)
            {
                this.renderManager.RenderToTexture(this.GameObjects);
            }
        }

        public override void HandleInput()
        {
            if (this.Enabled && !this.BlockInput)
            {
                // TODO: implement input handling
            }
        }

        public override void Update(Time frameTime)
        {
            if (this.IsLoading)
            {
                if (!string.IsNullOrEmpty(this.StartLevel))
                {
                    this.LoadLevel(this.StartLevel);
                }

                // TODO: implement object loading attributes
                this.GameObjects.ToList().ForEach(a => a.Initialize());

                this.IsLoading = false;
                return;
            }

            this.GameObjects.ToList().ForEach(a =>
            {
                if (a.Enabled)
                {
                    a.Update(frameTime);
                }
            });

        }
    }
}
