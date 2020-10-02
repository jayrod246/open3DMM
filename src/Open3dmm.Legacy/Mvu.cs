using Open3dmm.Core.Actors;
using Open3dmm.Core.Brender;
using Open3dmm.Core.Scenes;
using Open3dmm.Core.Veldrid;
using Open3dmm.MovieEditor;
using System;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace Open3dmm
{
    public partial class Mvu : Ddg
    {
        private bool mouseDown;
        private MvuState state = new MvuDoNothing();
        private BackgroundRenderer BackgroundRenderer { get; }
        public View Renderer { get; }
        public SceneNode SceneRoot { get; }
        public CameraInfo Camera
        {
            get
            {
                TryGetCameraInfo(out var cam);
                return cam;
            }
        }

        public new Movie Document => (Movie)base.Document;

        public Mvu(Movie movie, GobOptions options, int width, int height) : base(movie, options)
        {
            Renderer = new View(Application.Current.GraphicsContext, width, height)
            {
                RenderCallbackPre = RenderBackground,
            };
            BackgroundRenderer = new BackgroundRenderer(Application.Current.GraphicsDevice);
            UpdateRectangle(new(0, 0, width, height), null);
            SceneRoot = new SceneNode();
        }

        private bool OnMouseExit(Message m)
        {
            // If tool Listener (id == 45) is active, stops all sounds?
            return false;
        }

        private bool UpdateMovie(Message m)
        {
            Document.Field0x154 |= 32;
            return false;
        }

        private void RenderBackground(CommandList commandList)
        {
            if (TryGetCameraInfo(out var cameraInfo))
            {
                var ctx = Application.Current.GraphicsContext;
                ctx.PaletteSwap(commandList, cameraInfo.Mbmp);
                var colorMap = ctx.GetColorOutputTexture(cameraInfo.Mbmp);
                var depthMap = ctx.GetTexture(cameraInfo.Zbmp);
                BackgroundRenderer.Render(commandList, colorMap, depthMap);
            }
        }

        public override bool OnMouseEvent(Message m)
        {
            var input = (InputState)m.ParamC;
            var (x, y) = (Convert.ToInt32(m.ParamA), Convert.ToInt32(m.ParamB));
            if (m.Id == (int)KnownMessageValues.MousePressed)
            {
                mouseDown = true;
                state.MouseDown(x, y);
                CaptureMouse();
            }
            else
            {
                state.MouseCapture(x, y);
                if (!input.HasFlag(InputState.LeftButton) && mouseDown)
                {
                    mouseDown = false;
                    // Release mouse capture before calling MouseUp,
                    // that way the State can capture mouse again
                    // if necessary.
                    ReleaseMouse();
                    state.MouseUp(x, y);
                }
                else if (input.HasFlag(InputState.LeftButton) && !mouseDown)
                {
                    state.MouseDown(x, y);
                    mouseDown = true;
                }
            }

            return true;
        }

        public override void Draw(CommandList commandList, in RectangleF dest)
        {
            if (dest.Width > 128 && dest.Height > 128)
                Renderer.Resize((int)dest.Width, (int)dest.Height);
            if (Document.CurrentScene != null)
            {
                if (TryGetCameraInfo(out var camera))
                {
                    Renderer.SetCamera(camera);
                    Renderer.Draw(commandList, in dest, SceneRoot);
                }
            }
        }

        public override void UpdateRectangle(LTRB? newRect, Anchor? anchor)
        {
            base.UpdateRectangle(newRect, anchor);
            Renderer?.Resize(ActualRect.Width, ActualRect.Height);
        }

        private bool TryGetCameraInfo(out CameraInfo cameraInfo)
        {
            if (Document.CurrentScene.GetBackground() is BackgroundInfo background)
                return background.TryGetCameraInfo(Document.CurrentScene.CameraId, out cameraInfo);
            cameraInfo = null;
            return false;
        }

        public void ChangeState(MvuState newState)
        {
            state.CleanUp();
            state = newState;
            newState.SetContext(this);
            newState.Activate();
        }

        public bool CaptureMouse()
        {
            if (Application.Current.Exchange.Capture != null)
                return false;
            Application.Current.Exchange.Capture = this;
            return true;
        }

        public void ReleaseMouse()
        {
            if (Application.Current.Exchange.Capture == this)
                Application.Current.Exchange.Capture = null;
        }

        public void SetCursorPosition(Vector2 position)
        {
            Application.Current.SetMousePosition(position);
        }

        public Vector2 GetCenterOf(CoordinateSpace coordinateSpace)
        {
            var rc = GetRectangle(coordinateSpace);
            return new Vector2(rc.Left + rc.Width / 2, rc.Top + rc.Height / 2);
        }

        public void SetCursorVisible(bool visible)
            => Application.Current.SetCursorVisible(visible);

        public Actor CreateActor(Template template)
        {
            return null;
            //var ops = new MovieFileOperations(Document.Scope);
            //var label = template.Info.GetItem().Label;
            //var index = ops.NextFreeActor(Document.Identifier);
            //var block = new LogicalActor()
            //{
            //    MagicNumber = 0x03030001,
            //    Index = index,
            //    Start = 1,
            //    End = 1,
            //    Position = default,
            //    Reference = template.Info.GlobalReference,
            //};
            //var actor = ops.Resolve<Actor>(
            //    identifier: ops.CreateActor(
            //        scen: ops.GetScene(Document.Identifier, Document.CurrentSceneIndex),
            //        label,
            //        in block)
            //    );
            //ops.CreateActorInfo(
            //    mvie: Document.Identifier,
            //    label,
            //    block: new LogicalActorInfo()
            //    {
            //        Index = index,
            //        Flags = template.Flags,
            //        UseCount = 1,
            //        // TODO: Add reference to THUM
            //    });
            //return actor;
        }

        public bool HitTestBody(int x, int y, out Body body)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class MvuState
    {
        protected Mvu Context { get; private set; }

        public void SetContext(Mvu newContext)
        {
            Context = newContext;
        }

        public virtual void Activate()
        {
        }

        public virtual void MouseDown(int x, int y)
        {
        }

        public virtual void CleanUp()
        {
        }

        public virtual void MouseCapture(int x, int y)
        {
        }

        public virtual void MouseUp(int x, int y)
        {
        }
    }

    public class MvuDoNothing : MvuState
    {
    }

    public class MvuSpawn : MvuState
    {
        private readonly Template template;
        private readonly bool isWord;
        private readonly Action<bool> actorSpawnedDelegate;
        private Point startPos;
        private Body body;
        private Vector3 position;
        private Quaternion actualRot;
        private Vector2 trackBall = Vector2.UnitY * 0.5f;

        public MvuSpawn(Template template, Action<bool> actorSpawnedDelegate)
        {
            this.template = template;
            isWord = template.Flags.HasFlag(ActorFlags.IsWord);
            this.actorSpawnedDelegate = actorSpawnedDelegate;
        }

        static Quaternion CreateQuat(Vector3 u, Vector3 v)
        {
            float k_cos_theta = Vector3.Dot(u, v);
            float k = MathF.Sqrt(u.LengthSquared() * v.LengthSquared());

            if (k_cos_theta / k == -1)
            {
                return new Quaternion(Vector3.Normalize(Orthogonal180(u)), 0f);
            }

            return Quaternion.Normalize(new Quaternion(Vector3.Cross(u, v), k_cos_theta + k));
        }

        private static Vector3 Orthogonal180(Vector3 normal)
        {
            var tangent = Vector3.Normalize(Vector3.Cross(normal, new Vector3(-normal.Z, normal.X, normal.Y)));
            var bitangent = Vector3.Normalize(Vector3.Cross(normal, tangent));
            var angle = MathF.PI;
            return tangent * MathF.Sin(angle) + bitangent * MathF.Cos(angle);
        }

        static Quaternion CreateLookDir(Vector3 dir, Vector3 up)
        {
            var side = Vector3.Cross(dir, up);
            return CreateQuat(Vector3.UnitX, side);
        }

        public override void Activate()
        {
            Context.SetCursorPosition(Context.GetCenterOf(CoordinateSpace.Window));
            Context.SetCursorVisible(false);
            Context.CaptureMouse();
            var center = Context.GetCenterOf(CoordinateSpace.None);
            startPos = new Point((int)center.X, (int)center.Y);
            body = new Body(template);
            var r = new Random().Next(0, Context.Camera.SpawnPoints.Length);
            position = Context.Camera.SpawnPoints[r];
            var dir = new Vector3(Context.Camera.ViewMatrix.M31, Context.Camera.ViewMatrix.M32, Context.Camera.ViewMatrix.M33);
            dir = -Vector3.Normalize(dir);
            actualRot = CreateLookDir(dir, Vector3.UnitY);
            UpdateMatrix(actualRot);
            Context.SceneRoot.AddChild(body);
        }

        public override void MouseCapture(int x, int y)
        {
            var deltaF = new Vector2(x - startPos.X, y - startPos.Y);
            Vector2 arrowKey;
            var win = Context.GetWindow();
            var inputState = win.Input.State;
            var kb = win.Input.Keyboard;
            if (win.Input.Mouse.IsMouseDown(MouseButton.Right))
            {
                CancelSpawning();
                return;
            }

            if (kb.Events.Any(ev => ev.Key == Key.Up && (ev.Down || ev.Repeat)))
                arrowKey = Vector2.UnitY;
            else if (kb.Events.Any(ev => ev.Key == Key.Down && (ev.Down || ev.Repeat)))
                arrowKey = -Vector2.UnitY;
            else if (kb.Events.Any(ev => ev.Key == Key.Right && (ev.Down || ev.Repeat)))
                arrowKey = Vector2.UnitX;
            else if (kb.Events.Any(ev => ev.Key == Key.Left && (ev.Down || ev.Repeat)))
                arrowKey = -Vector2.UnitX;
            else if (deltaF == default)
                return;
            else
                arrowKey = Vector2.Zero;
            Context.SetCursorPosition(Context.GetCenterOf(CoordinateSpace.Window));
            var upAndDown = inputState.HasFlag(InputState.Alt);
            var camForward = Vector3.TransformNormal(Vector3.UnitZ, Context.Camera.ViewMatrix);
            camForward.Y = 0;
            var camUp = Vector3.UnitY;
            var camRight = -Vector3.Cross(camForward, camUp);
            Vector3 offsetPosition;
            if (upAndDown)
                offsetPosition = (camUp * -deltaF.Y) + (camRight * deltaF.X) + (camForward * -arrowKey.Y) + (camRight * arrowKey.X);
            else
                offsetPosition = (camForward * deltaF.Y) + (camRight * deltaF.X) + (camUp * arrowKey.Y) + (camRight * arrowKey.X);

            var magnitude = inputState.HasFlag(InputState.Shift) ? 0.1f : 1f;

            if (!isWord && deltaF.Length() != 0)
            {
                var d = Vector2.Normalize(deltaF);
                float dd;
                var dist = Vector2.Distance(trackBall, d);
                if (dist < 0.5f)
                    dd = 0.033f;
                else if (dist < 1f)
                    dd = 0.066f;
                else if (dist < 1.33f)
                    dd = 0.1f;
                else
                    dd = 0.33f;
                var dx = d.X - trackBall.X;
                var dy = d.Y - trackBall.Y;
                if (MathF.Abs(dx) > dd)
                    dx = MathF.Sign(dx) * dd;
                if (MathF.Abs(dy) > dd)
                    dy = MathF.Sign(dy) * dd;
                trackBall += new Vector2(dx, dy);

                if (trackBall.Length() != 0)
                {
                    var dir = -Vector3.TransformNormal(new Vector3(trackBall.X, 0, trackBall.Y), Context.Camera.ViewMatrix);
                    dir.Y = 0;
                    dir = Vector3.Normalize(dir);
                    actualRot = CreateLookDir(dir, Vector3.UnitY);
                }
            }

            magnitude *= 0.1f + Math.Min(Math.Max(Vector3.Distance(position, Context.Camera.ViewMatrix.Translation) - 30f, 0f) / 1000f, 1f);

            position += offsetPosition * magnitude;
            UpdateMatrix(actualRot);
        }

        private void CancelSpawning()
        {
            Context.ChangeState(new MvuReposition());
            actorSpawnedDelegate?.Invoke(false);
        }

        private void UpdateMatrix(Quaternion rot)
        {
            body.LocalTransform = BrenderHelper.MatrixFromEuler(body.Template.Rotation) * Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(position);
        }

        public override void MouseUp(int x, int y)
        {
            UpdateMatrix(actualRot);
            var actor = Context.CreateActor(template);
            Context.ChangeState(new MvuReposition());
            actorSpawnedDelegate?.Invoke(IsInView());
        }

        private bool IsInView()
        {
            // TODO: Check if actor is in view.
            return true;
        }

        public override void CleanUp()
        {
            base.CleanUp();
            body.Remove();
            Context.SetCursorVisible(true);
            Context.ReleaseMouse();
        }
    }

    public abstract class MvuMutateBase : MvuState
    {
    }

    public class MvuReposition : MvuMutateBase
    {
        private Point startPos;
        private Body body;

        public override void Activate()
        {
        }

        public override void MouseDown(int x, int y)
        {
            if (!Context.HitTestBody(x, y, out body))
                return;

            Context.SetCursorPosition(Context.GetCenterOf(CoordinateSpace.Window));
            Context.SetCursorVisible(false);
            Context.CaptureMouse();
            var center = Context.GetCenterOf(CoordinateSpace.None);
            startPos = new Point((int)center.X, (int)center.Y);
        }

        public override void MouseCapture(int x, int y)
        {
            if (body == null)
                return;

            Context.SetCursorPosition(Context.GetCenterOf(CoordinateSpace.Window));
            var delta = new Point(x - startPos.X, y - startPos.Y);
        }

        public override void MouseUp(int x, int y)
        {
            if (body == null)
                return;

            Context.SetCursorVisible(true);
        }
    }
}
