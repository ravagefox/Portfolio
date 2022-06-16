// Source: ShaderBase
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
using Engine.Assets;
using Engine.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wargame.Data.Graphics.Shaders
{
    public abstract class ShaderBase : IDisposable, IEffectMatrices
    {
        public GraphicsDevice GraphicsDevice => this.Effect.GraphicsDevice;

        public Matrix Projection { get; set; }
        public Matrix View { get; set; }
        public Matrix World { get; set; }

        public int CurrentMipmapLevel { get; set; }

        public Matrix InvProjection => Matrix.Invert(this.Projection);

        public Vector2 HalfPixel
        {
            get
            {
                float w = this.GraphicsDevice.PresentationParameters.BackBufferWidth;
                float h = this.GraphicsDevice.PresentationParameters.BackBufferHeight;

                return new Vector2(0.5f / w, 0.5f / h);
            }
        }

        public bool MipmapsSupported { get; set; }

        protected Effect Effect { get; }


        public event EventHandler<EventArgs> Disposing;


        private EffectParameter mWorldParam;
        private EffectParameter mViewParam;
        private EffectParameter mProjParam;
        private EffectParameter mInvProjParam;
        private EffectParameter mHalfPixelParam;
        private EffectParameter mCurrentMipmapLevelParam;
        private EffectParameter mMipmapSupportedParam;


        public ShaderBase(Effect cloneSource)
        {
            this.Effect = cloneSource;

            this.mWorldParam = this.Effect.Parameters["mWorld"];
            this.mViewParam = this.Effect.Parameters["mView"];
            this.mProjParam = this.Effect.Parameters["mProjection"];
            this.mInvProjParam = this.Effect.Parameters["mInvProjection"];
            this.mHalfPixelParam = this.Effect.Parameters["mHalfPixel"];
            this.mCurrentMipmapLevelParam = this.Effect.Parameters["mMipMapLevel"];
            this.mMipmapSupportedParam = this.Effect.Parameters["mMipmapSupportedParam"];
        }

        public EffectTechnique GetTechnique(string name)
        {
            var technique = this.Effect.Techniques.FirstOrDefault(t => t.Name.ToLower().SequenceEqual(name.ToLower()));
            return technique ?? this.GetCurrentTechnique();
        }

        public EffectTechnique GetCurrentTechnique()
        {
            return this.Effect.CurrentTechnique;
        }


        protected abstract void OnApply();



        public void Apply(EffectTechnique technique, int passIdx = 0)
        {
            this.OnApply();

            this.mWorldParam?.SetValue(this.World);
            this.mViewParam?.SetValue(this.View);
            this.mProjParam?.SetValue(this.Projection);
            this.mInvProjParam?.SetValue(this.InvProjection);
            this.mHalfPixelParam?.SetValue(this.HalfPixel);

            this.mCurrentMipmapLevelParam?.SetValue(this.CurrentMipmapLevel);
            this.mMipmapSupportedParam?.SetValue(this.MipmapsSupported);

            technique.Passes[passIdx].Apply();
        }


        protected static Effect LoadShaderBytecode(string path)
        {
            var iloader = ServiceProvider.Instance.GetService<ILoader>();
            return iloader.Load<Effect>(path);
        }


        protected virtual void OnDispose()
        {
            Disposing?.Invoke(this, EventArgs.Empty);

            this.Effect?.Dispose();
        }


        #region IDisposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
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


        public static implicit operator Effect(ShaderBase shader) => shader.Effect;
    }
}
