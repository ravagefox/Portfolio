// Source: CascadeShadowContext
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
using Engine.Graphics.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Gos;
using Wargame.Data.Gos.Components;
using Wargame.Data.Graphics.Shaders;
using Wargame.Extensions;

namespace Wargame.Data.Graphics
{
    public sealed class CascadeShadowContext : IDisposable
    {
        #region Properties
        public GraphicsDevice GraphicsDevice { get; }

        public int Resolution
        {
            get => this.resolution;
            set
            {
                if (this.resolution != value)
                {
                    this.resolution = value;
                    this.resolution = MathUtil.Clamp(this.resolution, MIN_RESOLUTION, MAX_RESOLUTION);

                    this.recreateTarget = true;
                }
            }
        }

        public Color LightColor { get; set; }
        public Vector3 LightDirection { get; set; } = Vector3.Up + Vector3.Right;
        public RenderTarget2D ShadowMap
        {
            get
            {
                if (this.recreateTarget)
                {
                    this.CreateTarget(ref this.shadowMap);
                    this.recreateTarget = false;
                }

                return this.shadowMap;
            }
        }

        public Matrix TextureScaleBias
        {
            get
            {
                var texScaleBias = Matrix.CreateScale(0.5f, -0.5f, 1.0f);
                texScaleBias.Translation = new Vector3(0.5f, 0.5f, 0.0f);
                return texScaleBias;
            }
        }

        public RasterizerState ShadowRaster =>
            new RasterizerState()
            {
                CullMode = CullMode.None,
                DepthClipEnable = false,
            };

        #endregion

        #region Fields
        private RenderTarget2D shadowMap;
        private Camera lightCamera;
        private int resolution;
        private bool recreateTarget;
        private DepthEffect depthEffect;
        private CascadeShadowMeshEffect meshEffect;
        private Vector3[] frustumCorners;
        private BoundingFrustum cameraFrustum;
        private float[] cascadeSplits;


        private const int NUM_CASCADES = 4;
        private const int MIN_RESOLUTION = 256;
        private const int MAX_RESOLUTION = 4096;
        #endregion

        #region Constructors

        public CascadeShadowContext(GraphicsDevice graphicsDevice)
        {
            this.GraphicsDevice = graphicsDevice;
            this.depthEffect = new DepthEffect();
            this.meshEffect = new CascadeShadowMeshEffect();
            this.frustumCorners = new Vector3[8];
            this.lightCamera = new Camera();
            this.cascadeSplits = new float[NUM_CASCADES] { 0.05f, 0.15f, 0.50f, 1.0f };

            this.recreateTarget = true;
            this.resolution = 2048;
            this.CreateTarget(ref this.shadowMap);
        }
        #endregion

        #region Render Shadow Maps

        public void RenderShadowMaps(Camera camera, IEnumerable<GameObject> actors)
        {
            var globalShadowMatrix = this.MakeGlobalShadowMatrix(camera);
            this.meshEffect.ShadowMatrix = globalShadowMatrix;

            for (var cascadeIdx = 0; cascadeIdx < NUM_CASCADES; ++cascadeIdx)
            {
                this.GraphicsDevice.SetRenderTarget(this.shadowMap, cascadeIdx);
                this.GraphicsDevice.Clear(Color.White);

                this.ResetViewFrustumCorners();
                var frustumCenter = this.GetFrustumCornersByCascadeIdx(
                    camera,
                    cascadeIdx,
                    out var splitDist);

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
                this.RenderActorsToDepthMap(actors);

                var shadowMatrix = shadowView * shadowProj * this.TextureScaleBias;
                var boundingFrustum = new BoundingFrustum(camera.View * camera.Projection);
                var nearZ = boundingFrustum.Near.D;
                var farZ = boundingFrustum.Far.D;

                this.cameraFrustum = boundingFrustum;
                var clipDist = farZ - nearZ;

                this.meshEffect.CascadeSplits[cascadeIdx] = nearZ + (splitDist * clipDist);
                var invCascadeMat = Matrix.Invert(shadowMatrix);
                var cascadeCorner = Vector4.Transform(Vector3.Zero, invCascadeMat).ToVector3();
                cascadeCorner = Vector4.Transform(cascadeCorner, globalShadowMatrix).ToVector3();

                var otherCorner = Vector4.Transform(Vector3.Zero, invCascadeMat).ToVector3();
                otherCorner = Vector4.Transform(otherCorner, globalShadowMatrix).ToVector3();

                var cascadeScale = Vector3.One / (otherCorner - cascadeCorner);
                this.meshEffect.CascadeOffsets[cascadeIdx] = new Vector4(-cascadeCorner, 0.0f);
                this.meshEffect.CascadeScales[cascadeIdx] = new Vector4(cascadeScale, 1.0f);
            }

            this.GraphicsDevice.SetRenderTargets(null);
        }

        private void RenderActorsToDepthMap(IEnumerable<GameObject> actors)
        {
            this.GraphicsDevice.SetRasterizerState(
                this.ShadowRaster,
                out var oldRaster);
            this.GraphicsDevice.SetBlendState(
                BlendState.Opaque,
                out var oldBlend);
            this.GraphicsDevice.SetDepthStencilState(
                DepthStencilState.Default,
                out var oldDepth);

            foreach (var actor in actors)
            {
                this.RenderActor(actor);
            }

            this.GraphicsDevice.SetBlendState(oldBlend, out _);
            this.GraphicsDevice.SetRasterizerState(oldRaster, out _);
            this.GraphicsDevice.SetDepthStencilState(oldDepth, out _);
        }

        private void RenderActor(GameObject actor)
        {
            var modelRenderer = actor.GetComponent<ModelRenderer>();
            if (modelRenderer != null && modelRenderer.IsShadowCaster)
            {
                var world = actor.GetComponent<Transform>().GetWorld();
                var bones = modelRenderer.GetBoneTransforms();

                foreach (var modelMesh in modelRenderer.Model.Meshes)
                {
                    var boneTransform = bones.Any() ? bones[modelMesh.ParentBone.Index] : Matrix.Identity;

                    this.depthEffect.World = boneTransform * world;
                    this.depthEffect.Source = this.lightCamera;
                    this.depthEffect.Apply();

                    modelRenderer.Render(modelMesh);
                }
            }
        }


        #endregion

        #region Util Methods

        public Camera GetLightCamera()
        {
            return this.lightCamera;
        }

        private Matrix StabalizeCascades(Matrix shadowProj, Matrix shadowView)
        {
            var shadowMatrixTemp = shadowView * shadowProj;
            var shadowOrigin = new Vector4(Vector3.Zero, 1.0f);
            shadowOrigin = Vector4.Transform(shadowOrigin, shadowMatrixTemp);
            shadowOrigin *= this.resolution / 2.0f;

            var roundedOrigin = shadowOrigin.Round();
            var roundOffset = roundedOrigin - shadowOrigin;
            roundOffset *= (2.0f / this.resolution);
            roundOffset.Z = 0.0f;
            roundOffset.W = 0.0f;

            shadowProj.M41 += roundOffset.X;
            shadowProj.M42 += roundOffset.Y;
            shadowProj.M43 += roundOffset.Z;
            shadowProj.M44 += roundOffset.W;
            return shadowProj;
        }

        private void CalculateShadowCamera(
            Vector3 frustumCenter,
            BoundingBox cascadeExtents,
            out Matrix shadowView,
            out Matrix shadowProj)
        {
            var shadowCamPos = frustumCenter + this.LightDirection * -cascadeExtents.Min.Z;

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
            this.frustumCorners.ToList().ForEach(c =>
            {
                var dist = (c - frustumCenter).Length();
                sphereRadius = Math.Max(sphereRadius, dist);
            });
            sphereRadius = (float)Math.Ceiling(sphereRadius * 16.0f) / 16.0f;
            maxExtents = new Vector3(sphereRadius);
            minExtents = -maxExtents;
        }

        private void CreateTarget(ref RenderTarget2D target)
        {
            target?.Dispose();
            target = new RenderTarget2D(
                this.GraphicsDevice,
                this.resolution,
                this.resolution,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24,
                1,
                RenderTargetUsage.DiscardContents,
                false,
                NUM_CASCADES);
        }


        private Vector3 GetFrustumCornersByCascadeIdx(Camera camera, int cascadeIdx, out float splitDist)
        {
            var prevSplitDist = cascadeIdx == 0 ? 0.0f : this.cascadeSplits[cascadeIdx - 1];
            splitDist = this.cascadeSplits[cascadeIdx];

            this.GetFrustumCorners(camera.View * camera.Projection);
            for (var i = 0; i < 4; i++)
            {
                var cornerRay = this.frustumCorners[i + 4] - this.frustumCorners[i];
                var nearCornerRay = cornerRay * prevSplitDist;
                var farCornerRay = cornerRay * splitDist;
                this.frustumCorners[i + 4] = this.frustumCorners[i] + farCornerRay;
                this.frustumCorners[i] = this.frustumCorners[i] + nearCornerRay;
            }

            return this.CalculateFrustumCenter();
        }

        private Matrix MakeGlobalShadowMatrix(Camera camera)
        {
            this.ResetViewFrustumCorners();
            this.GetFrustumCorners(camera.View * camera.Projection);

            var frustumCenter = this.CalculateFrustumCenter();

            var shadowCamPosition = frustumCenter + this.LightDirection * -0.5f;
            var shadowProj = Matrix.CreateOrthographicOffCenter(
                -0.5f, -0.5f, 0.5f, 0.5f, 0.0f, 1.0f);
            var shadowView = Matrix.CreateLookAt(shadowCamPosition, frustumCenter, Vector3.Up);

            return shadowView * shadowProj * this.TextureScaleBias;
        }

        private void GetFrustumCorners(Matrix viewProj)
        {
            var invViewProj = Matrix.Invert(viewProj);
            for (var i = 0; i < this.frustumCorners.Length; i++)
            {
                this.frustumCorners[i] = Vector4.Transform(this.frustumCorners[i], invViewProj).ToVector3();
            }
        }

        private Vector3 CalculateFrustumCenter()
        {
            var center = Vector3.Zero;
            this.frustumCorners.ToList().ForEach(c => center += c);
            center /= 8.0f;
            return center;
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
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.shadowMap?.Dispose();
                    this.depthEffect?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CascadeShadowContext()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
