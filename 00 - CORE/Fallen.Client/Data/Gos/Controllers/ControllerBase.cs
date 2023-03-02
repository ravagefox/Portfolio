// Source: ControllerBase
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

namespace Client.Data.Gos.Controllers
{
    public abstract class ControllerBase<WorldObjectType> :
        Engine.Core.IUpdateable,
        Engine.Graphics.IDrawable
        where WorldObjectType : WorldObject
    {
        public bool Enabled
        {
            get => this.mEnabled;
            set
            {
                if (this.mEnabled != value)
                {
                    this.mEnabled = value;
                    this.OnEnabledChanged(this, EventArgs.Empty);
                }
            }
        }

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

        public event EventHandler EnabledChanged;
        public event EventHandler VisibleChanged;

        private bool mEnabled, mVisible;


        protected virtual void OnEnabledChanged(object sender, EventArgs e)
        {
            EnabledChanged?.Invoke(sender, e);
        }

        protected virtual void OnVisibleChanged(object sender, EventArgs e)
        {
            VisibleChanged?.Invoke(sender, e);
        }


        public virtual void Update(Time gameTime) { }
        public virtual void Apply(Time gameTime) { }
    }
}
