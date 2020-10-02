using ImGuiNET;
using Open3dmm.Core;
using Open3dmm.Core.Actors;
using Open3dmm.Core.Brender;
using Open3dmm.Core.Data;
using Open3dmm.Core.Resolvers;
using Open3dmm.Core.Scenes;
using Open3dmm.Core.Veldrid;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace Open3dmm
{
    public class View
    {
        public VeldridGraphicsContext GraphicsContext { get; }
        public GraphicsDevice GraphicsDevice { get; }
        private bool isDisposed;
        private int width;
        private int height;
        private IntPtr imGuiBinding;
        private Texture colorTarget;
        private Texture depthTarget;
        private Framebuffer frameBuffer;
        public int Width => width;
        public int Height => height;
        public Texture Palette => GraphicsContext.Palette;
        protected Matrix4x4 ViewMatrix { get; set; } = Matrix4x4.Identity;

        private CameraInfo cameraInfo;
        protected float totalTime;
        private readonly CustomSceneVisitor sceneVisitor;
        private TexturedRenderer texturedRenderer;
        private CustomRenderer currentRenderer;
        private Action<CommandList> renderCallbackPre;

        public Matrix4x4 World { get; set; } = Matrix4x4.Identity;
        protected float FieldOfView => cameraInfo?.FieldOfView ?? BrAngle.AngleDegrees(60).ToRadians();

        public Action<CommandList> RenderCallbackPre {
            get => renderCallbackPre;
            set => renderCallbackPre = value ?? new Action<CommandList>(c => { });
        }

        public View(VeldridGraphicsContext graphicsContext, int width, int height)
        {
            GraphicsContext = graphicsContext;
            GraphicsDevice = graphicsContext.GraphicsDevice;
            Resize(width, height);
            CreateResources();
            sceneVisitor = new CustomSceneVisitor(this);
        }

        public virtual void Resize(int width, int height)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(View));
            if (this.width == width && this.height == height) return;
            this.width = width;
            this.height = height;
            var factory = GraphicsDevice.ResourceFactory;
            if (imGuiBinding != IntPtr.Zero)
            {
                Application.Current.RemoveImGuiBinding(colorTarget);
                imGuiBinding = IntPtr.Zero;
            }
            colorTarget?.Dispose();
            depthTarget?.Dispose();
            frameBuffer?.Dispose();
            colorTarget = factory.CreateTexture(TextureDescription.Texture2D((uint)width, (uint)height, 1, 1, PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.Sampled | TextureUsage.RenderTarget));
            depthTarget = factory.CreateTexture(TextureDescription.Texture2D((uint)width, (uint)height, 1, 1, PixelFormat.R32_Float, TextureUsage.Sampled | TextureUsage.DepthStencil));
            frameBuffer = factory.CreateFramebuffer(new FramebufferDescription(depthTarget, colorTarget));
            imGuiBinding = Application.Current.GetOrCreateImGuiBinding(colorTarget);
        }

        public virtual void Draw(CommandList commandList, in RectangleF dest, SceneNode root)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(View));
            var viewport = new Viewport(0, 0, width, height, cameraInfo?.NearClipPlane ?? 1f, cameraInfo?.FarClipPlane ?? 1000f);
            var proj = Matrix4x4.CreatePerspectiveFieldOfView(
                FieldOfView,
                viewport.Width / viewport.Height,
                viewport.MinDepth,
                viewport.MaxDepth);

            // Update buffers
            CalcTime(out _, out _, out var blend);
            currentRenderer.Blend.Set(commandList, blend);
            currentRenderer.Projection.Set(commandList, proj);
            currentRenderer.View.Set(commandList, ViewMatrix);
            currentRenderer.Light.Set(commandList, new Vector3(-1, -1, -1));

            // Clear framebuffer
            commandList.SetFramebuffer(frameBuffer);
            commandList.ClearColorTarget(0, RgbaFloat.Blue);
            commandList.ClearDepthStencil(1f);

            RenderCallbackPre(commandList);

            // Prepare to visit tree
            sceneVisitor.PrepareVisit(commandList, World);
            root.AcceptVisitor(sceneVisitor);

            // Output to ImGui
            var pMin = new Vector2(dest.Left, dest.Top);
            var pMax = new Vector2(dest.Right, dest.Bottom);
            var l = ImGui.GetBackgroundDrawList();
            l.AddImage(imGuiBinding, pMin, pMax);
        }

        protected void Tick()
        {
            totalTime += GameTime.DeltaSeconds;
        }

        public void CalcTime(out float t, out int cell, out float blend)
        {
            t = totalTime * 6.7f;
            cell = (int)Math.Truncate(t);
            blend = (float)(t - cell);
        }

        public void OriginTime()
        {
            totalTime = 0;
        }

        public void FrameBoundsToView(Bounds bounds, float x, float y, float z)
        {
            World = Matrix4x4.CreateRotationX(x) *
                    Matrix4x4.CreateRotationY(y) *
                    Matrix4x4.CreateRotationZ(z);
            FrameBoundsToView(bounds);
        }

        public void FrameBoundsToView(Bounds bounds)
        {
            var h = (bounds.Max.Y - bounds.Min.Y);
            var w = (bounds.Max.X - bounds.Min.X);
            float d;
            var viewAspect = (float)Width / Height;
            var objAspect = w / h;
            var fov = FieldOfView;
            if (objAspect < viewAspect)
            {
                d = (float)(h / 2 / Math.Tan(fov / 2));
            }
            else
            {
                fov *= viewAspect;
                d = (float)(w / 2 / Math.Tan(fov / 2));
            }

            d += (bounds.Max.Z - bounds.Min.Z) / 2;

            var CameraPos = new Vector3((bounds.Max.X + bounds.Min.X) / 2,
                       (bounds.Max.Y + bounds.Min.Y) / 2, ((bounds.Max.Z + bounds.Min.Z) / 2) + d);
            ViewMatrix = Matrix4x4.CreateLookAt(CameraPos, CameraPos - Vector3.UnitZ, Vector3.UnitY);
            //BWLD::FUN_004744f0(this->World, &local_30, uVar5, 65536000, 10920);
            return;
        }

        public void SetCamera(CameraInfo cameraInfo)
        {
            ViewMatrix = cameraInfo.ViewInverse;
            this.cameraInfo = cameraInfo;
        }

        private void CreateResources()
        {
            texturedRenderer = new TexturedRenderer(GraphicsDevice);
            currentRenderer = texturedRenderer;
        }

        private class CustomSceneVisitor : SceneNodeVisitor
        {
            Stack<Matrix4x4> matrixStack = new Stack<Matrix4x4>();
            private Matrix4x4 currentMatrix = Matrix4x4.Identity;
            private CommandList commandList;
            private readonly View view;

            public CustomSceneVisitor(View view)
            {
                this.view = view;
            }

            public void PrepareVisit(CommandList commandList, Matrix4x4 world)
            {
                this.commandList = commandList;
                currentMatrix = world;
            }

            public override bool VisitNode(SceneNode node)
            {
                if (node is Body body)
                {
                    matrixStack.Push(currentMatrix);
                    currentMatrix = (body.Owner?.CalculateTransform() ?? body.LocalTransform) * currentMatrix;
                }
                else if (node is BodyPart bodyPart)
                {
                    matrixStack.Push(currentMatrix);
                    currentMatrix = bodyPart.Transform * currentMatrix;
                    RenderBodyPart(bodyPart);
                }
                return true;
            }

            public override void LeaveNode(SceneNode node)
            {
                if (node is BodyPart || node is Body)
                {
                    currentMatrix = matrixStack.Pop();
                }
            }

            private void RenderBodyPart(BodyPart bodyPart)
            {
                var model = bodyPart.ModelOverride ?? bodyPart.Model;
                bodyPart.Material.Load(view.GraphicsDevice, commandList, view.Palette, view.currentRenderer);
                if (model == null) return;
                view.currentRenderer.World.Set(commandList, currentMatrix);
                var vertexBuffer = model.GetOrCreateVertexBuffer(view.GraphicsDevice);
                var indexBuffer = model.GetOrCreateIndexBuffer(view.GraphicsDevice);
                if (vertexBuffer == null || indexBuffer == null) return;
                if (model != bodyPart.Model)
                {
                    // Model is from costume, no vertex blending.
                    commandList.SetVertexBuffer(0, vertexBuffer);
                    commandList.SetVertexBuffer(1, vertexBuffer);
                }
                else
                {
                    // Model is from animation, perform vertex blending.
                    bodyPart.NextModel.Remap(view.GraphicsDevice, model);
                    var nextVertexBuffer = bodyPart.NextModel.GetOrCreateVertexBuffer(view.GraphicsDevice);
                    commandList.SetVertexBuffer(0, vertexBuffer);
                    commandList.SetVertexBuffer(1, nextVertexBuffer ?? vertexBuffer);
                }
                commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16, 0);
                commandList.DrawIndexed((uint)model.GetIndexCount());
            }
        }
    }
}
