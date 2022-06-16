// Source: RenderSystemBase
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
using System.Collections.Generic;
using Engine.Graphics.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wargame.Data.Graphics
{
    public abstract class RenderSystemBase : IDisposable
    {
        public GraphicsDevice GraphicsDevice { get; }

        public Point Resolution
        {
            get => this._resolution;
            set
            {
                if (this._resolution != value)
                {
                    this._resolution = value;
                    this.OnResolutionChanged(this, EventArgs.Empty);
                }
            }
        }


        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> ResolutionChanged;
        public event EventHandler<EventArgs> Initialize;


        private Point _resolution;
        private DepthStencilState _oldDepth;
        private RasterizerState _oldRaster;
        private BlendState _oldBlend;
        private Dictionary<int, SamplerState> _oldSamplers;



        protected RenderSystemBase(GraphicsDevice graphicsDevice)
        {
            this.GraphicsDevice = graphicsDevice;
            this.GraphicsDevice.DeviceReset += this.OnGraphicsDeviceReset;

            this._oldSamplers = new Dictionary<int, SamplerState>();
        }


        public void InitializeSystem()
        {
            this.OnInitialize(this, EventArgs.Empty);
        }

        public void SetGraphicsStates(
            RasterizerState rasterizerState = null,
            DepthStencilState depthState = null,
            BlendState blendState = null,
            Tuple<int, SamplerState> newSampler = null)
        {
            if (rasterizerState != null)
            {
                this.GraphicsDevice.SetRasterizerState(rasterizerState, out this._oldRaster);
            }

            if (depthState != null)
            {
                this.GraphicsDevice.SetDepthStencilState(depthState, out this._oldDepth);
            }

            if (blendState != null)
            {
                this.GraphicsDevice.SetBlendState(blendState, out this._oldBlend);
            }

            if (newSampler != null)
            {
                var idx = newSampler.Item1;

                this.GraphicsDevice.SetSamplerState(idx, newSampler.Item2, out var oldSampler);
                this._oldSamplers[idx] = oldSampler;
            }
        }

        public void ResetGraphicsState()
        {
            if (this._oldRaster != null) { this.GraphicsDevice.RasterizerState = this._oldRaster; }
            if (this._oldDepth != null) { this.GraphicsDevice.DepthStencilState = this._oldDepth; }
            if (this._oldBlend != null) { this.GraphicsDevice.BlendState = this._oldBlend; }

            if (this._oldSamplers.Count > 0)
            {
                foreach (var pair in this._oldSamplers)
                {
                    this.GraphicsDevice.SamplerStates[pair.Key] = pair.Value;
                }
            }
        }


        public abstract void Begin();
        public abstract void End();



        protected virtual void OnInitialize(object sender, EventArgs e)
        {
            Initialize?.Invoke(sender, e);
        }

        protected virtual void RecreateRenderTarget2D(ref RenderTarget2D target)
        {
            target?.Dispose();
            target = new RenderTarget2D(
                this.GraphicsDevice,
                this.Resolution.X,
                this.Resolution.Y,
                true,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8);
        }

        protected virtual void OnResolutionChanged(object sender, EventArgs e)
        {
            ResolutionChanged?.Invoke(sender, e);
        }

        protected virtual void OnGraphicsDeviceReset(object sender, EventArgs e)
        {
            this.DeviceReset?.Invoke(sender, e);
        }


        #region IDisposable

        protected abstract void OnDispose();

        private bool disposedValue;

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.OnDispose();
                }

                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
