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
using System.Linq;
using Client.Data.Gos;
using Engine.Graphics.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Graphics
{
    public abstract class RenderSystemBase : IDisposable
    {
        #region Properties
        public GraphicsDevice GraphicsDevice { get; }

        public Point Resolution
        {
            get => this.m_resolution;
            set
            {
                if (this.m_resolution != value)
                {
                    this.m_resolution = value;
                    this.RecreateRenderTarget(ref this.m_renderTarget);

                    this.OnResolutionChanged(this, EventArgs.Empty);
                }
            }
        }

        public ICamera Camera { get; set; }

        public Color ClearColor { get; set; }
        #endregion

        #region Fields
        public event EventHandler ResolutionChanged;
        public event EventHandler Initialized;
        public event EventHandler Disposing;
        public event EventHandler DeviceReset;

        private Point m_resolution;
        private DepthStencilState m_oldDepthState;
        private RasterizerState m_oldRasterState;
        private BlendState m_oldBlendState;
        private Dictionary<int, SamplerState> m_oldSamplerStates;
        private RenderTarget2D m_renderTarget;
        #endregion

        #region Constructors

        public RenderSystemBase(GraphicsDevice graphicsDevice)
        {
            this.GraphicsDevice = graphicsDevice;
            this.m_oldSamplerStates = new Dictionary<int, SamplerState>();
            // this.m_resolution = graphicsDevice.GetBounds().Size;
            graphicsDevice.DeviceReset += this.OnDeviceReset;

            this.ClearColor = Color.TransparentBlack;
        }
        #endregion

        #region Public Methods
        public void Initialize()
        {
            this.OnInitialized(this, EventArgs.Empty);
        }

        public void SetGraphicsStates(
            BlendState blendState = null,
            RasterizerState rasterizerState = null,
            DepthStencilState depthStencilState = null,
            Tuple<int, SamplerState> samplerState = null)
        {
            if (rasterizerState != null) { this.GraphicsDevice.SetRasterizerState(rasterizerState, out this.m_oldRasterState); }
            if (blendState != null) { this.GraphicsDevice.SetBlendState(blendState, out this.m_oldBlendState); }
            if (depthStencilState != null) { this.GraphicsDevice.SetDepthStencilState(depthStencilState, out this.m_oldDepthState); }

            if (samplerState != null)
            {
                var idx = samplerState.Item1;
                this.GraphicsDevice.SetSamplerState(idx, samplerState.Item2, out var oldSampler);
                this.m_oldSamplerStates[idx] = oldSampler;
            }
        }

        public void ResetGraphicsStates()
        {
            if (this.m_oldBlendState != null) { this.GraphicsDevice.SetBlendState(this.m_oldBlendState, out _); }
            if (this.m_oldDepthState != null) { this.GraphicsDevice.SetDepthStencilState(this.m_oldDepthState, out _); }
            if (this.m_oldRasterState != null) { this.GraphicsDevice.SetRasterizerState(this.m_oldRasterState, out _); }
            if (this.m_oldSamplerStates.Count > 0)
            {
                foreach (var pair in this.m_oldSamplerStates)
                {
                    this.GraphicsDevice.SetSamplerState(pair.Key, pair.Value, out _);
                }
            }
        }

        public virtual RenderTarget2D GetRenderTarget2D()
        {
            // Useful for when a default or custom render target is to be returned.
            return this.m_renderTarget;
        }
        #endregion

        #region Protected Methods
        protected virtual void RecreateRenderTarget(ref RenderTarget2D target)
        {
            target?.Dispose();
            target = new RenderTarget2D(
                this.GraphicsDevice,
                this.m_resolution.X,
                this.m_resolution.Y,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8);
        }

        protected virtual void OnInitialized(object sender, EventArgs e)
        {
            Initialized?.Invoke(sender, e);
        }

        protected virtual void OnDeviceReset(object sender, EventArgs e)
        {
            DeviceReset?.Invoke(sender, e);
        }

        protected virtual void OnResolutionChanged(object sender, EventArgs e)
        {
            ResolutionChanged?.Invoke(sender, e);
        }

        protected virtual void OnDisposing(object sender, EventArgs e)
        {
            Disposing?.Invoke(sender, e);
        }
        #endregion

        #region Abstract Methods

        public abstract void End();
        public abstract void Begin();
        #endregion

        #region IDisposable
        public void Dispose()
        {
            this.OnDisposing(this, EventArgs.Empty);

            this.GraphicsDevice.DeviceReset -= this.OnDeviceReset;

            this.m_resolution = Point.Zero;
            this.m_oldSamplerStates.Values.ToList().ForEach(s => s.Dispose());

            this.m_oldSamplerStates.Clear();
            this.m_oldSamplerStates = null;

            this.DeviceReset = null;
            this.Initialized = null;
            this.ResolutionChanged = null;

            this.m_oldBlendState?.Dispose();
            this.m_oldRasterState?.Dispose();
            this.m_oldDepthState?.Dispose();
        }
        #endregion

    }
}
