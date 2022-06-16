// Source: CascadeShadowRenderSystem
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
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Gos;
using Wargame.Data.Gos.Components;
using Wargame.Data.Graphics.Shaders;
using Wargame.Extensions;

namespace Wargame.Data.Graphics.DeferredContext
{
    public enum ShadowQuality
    {
        Low = 512,
        Medium = 1024,
        High = 2048,
        Ultra = 4096,
    }
    public enum FilterSize
    {
        Filter2x2,
        Filter3x3,
        Filter5x5,
        Filter7x7,
    }

    public sealed class CascadeShadowRenderSystem : RenderSystemBase
    {
        #region Properties
        public Vector3 LightDirection
        {
            get => this.lightDirection;
            set
            {
                var normalized = Vector3.Normalize(value);
                if (this.lightDirection != normalized)
                {
                    this.lightDirection = normalized;
                }
            }
        }

        public ShadowQuality Quality
        {
            get => this.quality;
            set
            {
                if (this.quality != value)
                {
                    this.quality = value;
                    this.CreateDepthTarget(ref this.depthTarget);
                }
            }
        }

        public FilterSize FilterQuality { get; set; }

        public RasterizerState ShadowRaster => new RasterizerState()
        {
            CullMode = CullMode.None,
            DepthClipEnable = false,
        };

        public SamplerState ShadowSamplerFilter => new SamplerState()
        {
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp,
            AddressW = TextureAddressMode.Clamp,
            Filter = TextureFilter.Linear,
            ComparisonFunction = CompareFunction.Less,
            FilterMode = TextureFilterMode.Comparison,
        };

        public Texture2D DepthTexture => this.depthTarget;
        public Texture2D SceneTexture => this.shadowSceneTarget;

        private Matrix TextureScaleBias
        {
            get
            {
                var texScaleBias = Matrix.CreateScale(0.5f, -0.5f, 1.0f);
                texScaleBias.Translation = new Vector3(0.5f, 0.5f, 0.0f);
                return texScaleBias;
            }
        }
        #endregion

        #region Fields

        private CascadeShadowMeshEffect meshEffect;
        private DepthEffect depthEffect;
        private RenderTarget2D depthTarget, shadowSceneTarget;
        private float[] cascadeSplits;
        private Vector3[] frustumCorners;
        private ShadowQuality quality;
        private Vector3 lightDirection;
        private Camera lightCamera;

        private IEnumerable<GameObject> actors;

        private static int cascadeCameras = 0;
        public const int NUM_CASCADES = 4;

        #endregion

        #region Constructors

        public CascadeShadowRenderSystem(GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
        }

        #endregion

        #region Public Methods

        protected override void OnInitialize(object sender, EventArgs e)
        {
            this.lightCamera = new Camera()
            {
                Name = "CascadeLightCamera_" + cascadeCameras++,
            };
            this.frustumCorners = new Vector3[8];
            this.cascadeSplits = new float[NUM_CASCADES]
            {
                0.05f, 0.15f, 0.50f, 1.0f,
            };
            this.meshEffect = new CascadeShadowMeshEffect();
            this.depthEffect = new DepthEffect();

            base.OnInitialize(sender, e);
        }

        public override void Begin()
        {
            if (this.actors == null || !this.actors.Any()) { return; }

            var globalShadowMatrix = this.MakeGlobalShadowMatrix(Camera.Current);
            this.meshEffect.ShadowMatrix = globalShadowMatrix;

            for (var cascadeIdx = 0; cascadeIdx < NUM_CASCADES; ++cascadeIdx)
            {
                this.GraphicsDevice.SetRenderTarget(this.depthTarget, cascadeIdx);
                this.GraphicsDevice.Clear(Color.White);
                this.InitializeCascadeByIdx(
                    cascadeIdx,
                    out var splitDist);

                this.RenderToShadowMap(this.actors);

                this.SetMeshEffectParameters(
                    globalShadowMatrix,
                    cascadeIdx,
                    splitDist);
            }
        }

        private void SetMeshEffectParameters(
            Matrix globalShadowMatrix,
            int cascadeIdx,
            float splitDist)
        {
            var shadowMatrix = this.lightCamera.View * this.lightCamera.Projection * this.TextureScaleBias;
            var boundingFrustum = Camera.Current.GetBoundingFrustum();

            var nearZ = boundingFrustum.Near.D;
            var farZ = -boundingFrustum.Far.D;
            var clipDist = farZ - nearZ;

            this.meshEffect.CascadeSplits[cascadeIdx] = nearZ + (splitDist * clipDist);

            var invCascadeMat = Matrix.Invert(shadowMatrix);
            var cascadeCorner = Vector4.Transform(Vector3.Zero, invCascadeMat).ToVector3();
            cascadeCorner = Vector4.Transform(cascadeCorner, globalShadowMatrix).ToVector3();

            var otherCorner = Vector4.Transform(Vector3.One, invCascadeMat).ToVector3();
            otherCorner = Vector4.Transform(otherCorner, globalShadowMatrix).ToVector3();

            var cascadeScale = Vector3.One / (otherCorner - cascadeCorner);
            this.meshEffect.CascadeOffsets[cascadeIdx] = new Vector4(-cascadeCorner, 0.0f);
            this.meshEffect.CascadeScales[cascadeIdx] = new Vector4(cascadeScale, 1.0f);
        }

        public void SetActors(IEnumerable<GameObject> actors)
        {
            this.actors = actors;
        }

        public void RenderCascadeScene()
        {
            this.GraphicsDevice.SetRenderTarget(this.shadowSceneTarget);
            this.GraphicsDevice.Clear(Color.TransparentBlack);

            this.SetGraphicsStates(
                RasterizerState.CullCounterClockwise,
                DepthStencilState.Default,
                BlendState.Opaque,
                new Tuple<int, SamplerState>(0, this.ShadowSamplerFilter));

            this.meshEffect.VisualizeCascades = false;
            this.meshEffect.FilterAcrossCascades = true;
            this.meshEffect.FilterSize = this.FilterQuality;
            this.meshEffect.Bias = 1 / 100.0f;
            this.meshEffect.OffsetScale = 0.0f;

            this.meshEffect.ShadowMap = this.DepthTexture;
            this.meshEffect.LightDirection = this.LightDirection;
            this.meshEffect.LightColor = new Vector3(3, 3, 3);

            this.actors.ToList().ForEach(actor =>
            {
                if (actor.GetComponent<ModelRenderer>() is ModelRenderer modelRenderer && modelRenderer.Model != null)
                {
                    if (modelRenderer.CanReceiveShadows &&
                        modelRenderer.IsVisible &&
                        modelRenderer.IsInViewSpace(Camera.Current.GetBoundingFrustum()))
                    {
                        var world = actor.GetComponent<Transform>().GetWorld();
                        var boneTransforms = modelRenderer.GetBoneTransforms();

                        foreach (var modelMesh in modelRenderer.Model.Meshes)
                        {
                            var boneTransform = boneTransforms.Any() ?
                                     boneTransforms[modelMesh.ParentBone.Index] :
                                     Matrix.Identity;

                            this.meshEffect.World = boneTransform * world;
                            this.meshEffect.DiffuseColor = Color.White.ToVector3();
                            var technique =
                                this.meshEffect.GetTechnique(this.meshEffect.GetTechniqueName());

                            this.meshEffect.Apply(technique);

                            modelRenderer.Render(modelMesh);
                        }
                    }
                }
            });

            //this.ResetGraphicsState();
        }

        public override void End()
        {
            this.GraphicsDevice.SetRenderTargets(null);
            this.ResetGraphicsState();
        }

        #endregion

        #region Render Helpers

        private void RenderToShadowMap(IEnumerable<GameObject> objs)
        {
            this.SetGraphicsStates(
                this.ShadowRaster,
                DepthStencilState.Default,
                BlendState.Opaque);

            objs.ToList().ForEach(o =>
            {
                this.RenderActor(o);
            });

            this.ResetGraphicsState();
        }

        private void RenderActor(GameObject obj)
        {
            var modelRenderer = obj.GetComponent<ModelRenderer>();

            if (modelRenderer == null) { return; }
            if (modelRenderer.Model == null) { return; }
            if (!modelRenderer.IsShadowCaster) { return; }

            if (obj.GetComponent<Transform>() is Transform transform)
            {
                var world = transform.GetWorld();
                var bones = modelRenderer.GetBoneTransforms();

                foreach (var modelMesh in modelRenderer.Model.Meshes)
                {
                    var boneTransform = bones.Any() ? bones[modelMesh.ParentBone.Index] : Matrix.Identity;

                    this.depthEffect.World = boneTransform * world;
                    this.depthEffect.View = this.lightCamera.View;
                    this.depthEffect.Projection = this.lightCamera.Projection;

                    this.depthEffect.Apply(
                        this.depthEffect.GetCurrentTechnique());

                    modelRenderer.Render(modelMesh);
                }
            }
        }

        #endregion

        #region Cascade Methods

        private void InitializeCascadeByIdx(
            int cascadeIdx,
            out float splitDist)
        {
            this.ResetViewFrustumCorners();
            var frustumCenter = this.GetFrustumCornersByCascadeIdx(Camera.Current, cascadeIdx, out splitDist);

            this.BuildExtents(
                frustumCenter,
                out var minExtents,
                out var maxExtents);

            this.CalculateShadowCamera(
                frustumCenter,
                new BoundingBox(minExtents, maxExtents),
                out var shadowView,
                out var shadowProj);

            shadowProj = this.StabalizeCascades(shadowProj, shadowView);

            this.lightCamera.View = shadowView;
            this.lightCamera.Projection = shadowProj;

        }


        private Matrix StabalizeCascades(Matrix shadowProj, Matrix shadowView)
        {
            var shadowMatrixTemp = shadowView * shadowProj;
            var shadowOrigin = new Vector4(Vector3.Zero, 1.0f);
            shadowOrigin = Vector4.Transform(shadowOrigin, shadowMatrixTemp);
            shadowOrigin *= this.depthTarget.Width / 2.0f;

            var roundedOrigin = shadowOrigin.Round();
            var roundOffset = roundedOrigin - shadowOrigin;
            roundOffset *= 2.0f / this.depthTarget.Width;
            roundOffset.Z = 0.0f;
            roundOffset.W = 0.0f;

            var tmp = shadowProj;
            tmp.M41 += roundOffset.X;
            tmp.M42 += roundOffset.Y;
            tmp.M43 += roundOffset.Z;
            tmp.M44 += roundOffset.W;
            return tmp;
        }

        private void CalculateShadowCamera(
            Vector3 frustumCenter,
            BoundingBox cascadeExtents,
            out Matrix shadowView,
            out Matrix shadowProj)
        {
            var shadowCamPos = frustumCenter + (this.LightDirection * -cascadeExtents.Min.Z);

            shadowProj = Matrix.CreateOrthographicOffCenter(
                cascadeExtents.Min.X,
                cascadeExtents.Max.X,
                cascadeExtents.Min.Y,
                cascadeExtents.Max.Y,
                0.0f,
                (cascadeExtents.Max - cascadeExtents.Min).Z);
            shadowView = Matrix.CreateLookAt(shadowCamPos, frustumCenter, Vector3.Up);
        }

        private void BuildExtents(
            Vector3 frustumCenter,
            out Vector3 minExtents,
            out Vector3 maxExtents)
        {
            var sphereRadius = 0.0f;
            for (var i = 0; i < 8; ++i)
            {
                var dist = (this.frustumCorners[i] - frustumCenter).Length();
                sphereRadius = Math.Max(sphereRadius, dist);
            };
            sphereRadius = (float)Math.Ceiling(sphereRadius * 16.0f) / 16.0f;
            maxExtents = new Vector3(sphereRadius);
            minExtents = -maxExtents;
        }
        private void ResetViewFrustumCorners()
        {
            this.frustumCorners[0] = new Vector3(-1.0f, 1.0f, 0.0f);
            this.frustumCorners[1] = new Vector3(1.0f, 1.0f, 0.0f);
            this.frustumCorners[2] = new Vector3(1.0f, -1.0f, 0.0f);
            this.frustumCorners[3] = new Vector3(-1.0f, -1.0f, 0.0f);
            this.frustumCorners[4] = new Vector3(-1.0f, 1.0f, 1.0f);
            this.frustumCorners[5] = new Vector3(1.0f, 1.0f, 1.0f);
            this.frustumCorners[6] = new Vector3(1.0f, -1.0f, 1.0f);
            this.frustumCorners[7] = new Vector3(-1.0f, -1.0f, 1.0f);
        }


        private Matrix MakeGlobalShadowMatrix(Camera camera)
        {
            this.ResetViewFrustumCorners();

            this.GetFrustumCorners(camera.View * camera.Projection);
            var center = this.CalculateFrustumCenter();

            var shadowCamPos = center + (this.LightDirection * -0.5f);
            var shadowProj = Matrix.CreateOrthographicOffCenter(
                -0.5f, 0.5f, -0.5f, 0.5f, 0.0f, 10.0f);

            var shadowView = Matrix.CreateLookAt(shadowCamPos, center, Vector3.Up);
            return shadowView * shadowProj * this.TextureScaleBias;
        }

        private Vector3 GetFrustumCornersByCascadeIdx(Camera camera, int cascadeIdx, out float splitDist)
        {
            var prevSplitDist = cascadeIdx == 0 ? 0.0f : this.cascadeSplits[cascadeIdx - 1];
            splitDist = this.cascadeSplits[cascadeIdx];

            this.GetFrustumCorners(camera.View * camera.Projection);
            for (var i = 0; i < 4; ++i)
            {
                var cornerRay = this.frustumCorners[i + 4] - this.frustumCorners[i];
                var nearCornerRay = cornerRay * prevSplitDist;
                var farCornerRay = cornerRay * splitDist;

                this.frustumCorners[i + 4] = this.frustumCorners[i] + farCornerRay;
                this.frustumCorners[i] = this.frustumCorners[i] + nearCornerRay;
            }

            return this.CalculateFrustumCenter();
        }

        private void GetFrustumCorners(Matrix viewProj)
        {
            var invViewProj = Matrix.Invert(viewProj);
            for (var i = 0; i < 8; ++i)
            {
                this.frustumCorners[i] = Vector4.Transform(this.frustumCorners[i], invViewProj).ToVector3();
            }
        }

        private Vector3 CalculateFrustumCenter()
        {
            var center = Vector3.Zero;
            for (var i = 0; i < 8; ++i)
            {
                center += this.frustumCorners[i];
            }

            center /= 8.0f;
            return center;
        }
        #endregion

        #region Private Methods
        private void CreateDepthTarget(ref RenderTarget2D target)
        {
            target?.Dispose();
            target = new RenderTarget2D(
                this.GraphicsDevice,
                (int)this.quality,
                (int)this.quality,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24,
                1,
                RenderTargetUsage.DiscardContents,
                false,
                NUM_CASCADES);
        }

        #endregion

        #region Protected Methods

        protected override void OnResolutionChanged(object sender, EventArgs e)
        {
            this.RecreateRenderTarget2D(ref this.shadowSceneTarget);
            base.OnResolutionChanged(sender, e);
        }

        protected override void OnDispose()
        {
            this.meshEffect?.Dispose();
            this.depthEffect?.Dispose();
            this.shadowSceneTarget?.Dispose();
            this.depthTarget?.Dispose();
            this.shadowSceneTarget?.Dispose();
        }
        #endregion
    }
}
