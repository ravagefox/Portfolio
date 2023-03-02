// Source: WorldFrame
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

using Client.API;
using Engine.Core;
using Microsoft.Xna.Framework.Graphics;
using MoonSharp;

namespace Client.Data.Graphics
{
    [MoonSharpUserData]
    public partial class WorldFrame : SystemApi
    {
        // -----------------------------------------------
        // Universal variables
        // -----------------------------------------------
        #region Fields and Properties

        private RenderSystem renderSystem = new RenderSystem();

        public Texture2D WorldTexture { get; private set; }

        #endregion

        // -----------------------------------------------
        // Client message handlers
        // -----------------------------------------------
        #region Handler Messages

        #endregion

        // -----------------------------------------------
        // Public user methods
        // -----------------------------------------------
        #region Public API

        #endregion

        [MoonSharpHidden]
        public void RenderGameObjects(GameObjectCollection gameObjects)
        {
            this.renderSystem.Begin();
            this.renderSystem.RenderGameObjects(gameObjects);
            this.WorldTexture = this.renderSystem.End();
        }
    }
}
