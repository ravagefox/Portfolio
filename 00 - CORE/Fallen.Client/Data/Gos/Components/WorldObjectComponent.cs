// Source: WorldObjectComponent
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
using Engine.Core;

namespace Client.Data.Gos.Components
{
    public abstract class WorldObjectComponent : GameObjectComponent, Engine.Graphics.IDrawable
    {
        public WorldObject WorldObject => (WorldObject)this.GameObject;

        public bool Visible
        {
            get => this.mVisible;
            set
            {
                if (this.mVisible != value)
                {
                    this.mVisible = value;
                    this.OnVisibleChanged(this, EventArgs.Empty);
                }
            }
        }

        public int DrawOrder { get; set; }


        public event EventHandler VisibleChanged;

        private bool mVisible = true;


        protected virtual void OnVisibleChanged(object sender, EventArgs e)
        {
            this.VisibleChanged?.Invoke(sender, e);
        }

        public virtual void Apply(Time frameTime)
        {
        }
    }
}
