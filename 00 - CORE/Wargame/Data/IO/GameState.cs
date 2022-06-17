// Source: GameState
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
using Engine.Data;
using Engine.Graphics;

namespace Wargame.Data.IO
{
    public abstract class GameState : BaseObject, IUpdateable, IDrawable
    {
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

        public bool Enabled
        {
            get => this.mEnabled;
            set
            {
                if (this.mEnabled != value)
                {
                    this.mEnabled = value;
                    this.OnEnableChanged(this, EventArgs.Empty);
                }
            }
        }

        public GameObjectCollection GameObjects { get; protected set; }

        public bool IsLoading { get; protected set; }

        public bool BlockInput { get; protected set; }

        public int DrawOrder { get; set; }



        public event EventHandler VisibleChanged;
        public event EventHandler EnabledChanged;

        private bool mVisible, mEnabled;


        protected GameState(string name)
        {
            this.Id = AssetId.NewAssetId();
            this.Name = name;

            this.mVisible = true;
            this.mEnabled = true;
            this.DrawOrder = -1;

            this.GameObjects = new GameObjectCollection();
            this.IsLoading = true;
        }


        protected virtual void OnVisibleChanged(object sender, EventArgs e)
        {
            VisibleChanged?.Invoke(sender, e);
        }

        protected virtual void OnEnableChanged(object sender, EventArgs e)
        {
            EnabledChanged?.Invoke(sender, e);
        }

        public abstract void HandleInput();
        public abstract void InitializeState();
        public abstract void Apply(Time frameTime);
        public abstract void Update(Time frameTime);
    }
}
