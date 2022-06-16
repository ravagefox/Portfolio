// Source: CascadeShadowMeshEffect
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Gos;
using Wargame.Data.Graphics.DeferredContext;

namespace Wargame.Data.Graphics.Shaders
{
    public sealed class CascadeShadowMeshEffect : ShaderBase
    {

        public bool VisualizeCascades { get; set; }
        public bool FilterAcrossCascades { get; set; }
        public FilterSize FilterSize { get; set; }
        public float Bias { get; set; }
        public float OffsetScale { get; set; }
        public Texture2D ShadowMap { get; set; }
        public Vector3 LightDirection { get; set; }
        public Vector3 LightColor { get; set; }
        public Vector3 DiffuseColor { get; set; }
        public Matrix ShadowMatrix { get; internal set; }

        public Vector4[] CascadeScales { get; set; }
        public Vector4[] CascadeOffsets { get; set; }
        public float[] CascadeSplits { get; set; }

        private EffectParameter mWorldParam;
        private EffectParameter mViewProjParam;
        private EffectParameter mShadowMatParam;
        private EffectParameter mCamPosWSParam;
        private EffectParameter mCascadeSplitsParam;
        private EffectParameter mCascadeOffsetsParam;
        private EffectParameter mCascadeScalesParam;

        private EffectParameter mLightDirParam;
        private EffectParameter mLightColParam;
        private EffectParameter mDiffuseColorParam;

        private EffectParameter mBiasParam;
        private EffectParameter mOffsetScaleParam;
        private EffectParameter mShadowMapParam;


        public CascadeShadowMeshEffect()
            : base(LoadShaderBytecode("Shaders\\CascadeShadowMesh"))
        {
            this.mWorldParam = this.Effect.Parameters["World"];
            this.mViewProjParam = this.Effect.Parameters["ViewProjection"];
            this.mShadowMatParam = this.Effect.Parameters["ShadowMatrix"];
            this.mCamPosWSParam = this.Effect.Parameters["CameraPosWS"];
            this.mCascadeSplitsParam = this.Effect.Parameters["CascadeSplits"];
            this.mCascadeOffsetsParam = this.Effect.Parameters["CascadeOffsets"];
            this.mCascadeScalesParam = this.Effect.Parameters["CascadeScales"];

            this.mLightDirParam = this.Effect.Parameters["LightDirection"];
            this.mLightColParam = this.Effect.Parameters["LightColor"];
            this.mDiffuseColorParam = this.Effect.Parameters["DiffuseColor"];

            this.mBiasParam = this.Effect.Parameters["Bias"];
            this.mOffsetScaleParam = this.Effect.Parameters["OffsetScale"];

            this.mShadowMapParam = this.Effect.Parameters["ShadowMap"];

            this.CascadeSplits = new float[CascadeShadowRenderSystem.NUM_CASCADES];
            this.CascadeOffsets = new Vector4[CascadeShadowRenderSystem.NUM_CASCADES];
            this.CascadeScales = new Vector4[CascadeShadowRenderSystem.NUM_CASCADES];
        }

        protected override void OnApply()
        {
            var cam = Camera.Current;
            var cameraViewProj = cam.View * cam.Projection;

            this.mWorldParam?.SetValue(this.World);
            this.mViewProjParam?.SetValue(cameraViewProj);
            this.mShadowMatParam?.SetValue(this.ShadowMatrix);
            this.mCamPosWSParam?.SetValue(cam.Transform.Position);
            this.mCascadeSplitsParam?.SetValue(
                new Vector4(this.CascadeSplits[0], this.CascadeSplits[1], this.CascadeSplits[2], this.CascadeSplits[3]));
            this.mCascadeOffsetsParam?.SetValue(this.CascadeOffsets);
            this.mCascadeScalesParam?.SetValue(this.CascadeScales);

            this.mLightDirParam?.SetValue(this.LightDirection);
            this.mLightColParam?.SetValue(this.LightColor);
            this.mDiffuseColorParam?.SetValue(this.DiffuseColor);

            this.mBiasParam?.SetValue(this.Bias);
            this.mOffsetScaleParam?.SetValue(this.OffsetScale);
            this.mShadowMapParam?.SetValue(this.ShadowMap);
        }

        public string GetTechniqueName()
        {
            return
                "Visualize" + this.VisualizeCascades +
                "Filter" + this.FilterAcrossCascades +
                "FilterSize" + this.FilterSize;
        }
    }
}
