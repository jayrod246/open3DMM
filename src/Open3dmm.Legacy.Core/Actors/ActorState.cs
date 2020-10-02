using Open3dmm.Core;
using Open3dmm.Core.Brender;
using Open3dmm.Core.Data;
using Open3dmm.Core.IO;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Open3dmm.Core.Actors
{
    public struct ActorState
    {
        public Vector3 Position;
        public Vector3 Reposition;
        public Vector3 Jump;
        public Vector3 Stretch;
        public float Scale;
        public Matrix4x4 BaseMatrix;
        public Matrix4x4? RotationMatrix;
        public Vector3 PathPosition;
        public PathIndex PathIndex;
        public int EventIndex;

        public int Action;
        public int Cell;
        public bool IsActionPaused;
        public bool IsPlaced;
        public float PathSpeed;

        public static readonly ActorState Default = new ActorState()
        {
            Stretch = Vector3.One,
            Scale = 1f,
            PathIndex = default,
            BaseMatrix = Matrix4x4.Identity,
        };

        public void EvalEvent(int index, GenericGroup<LogicalActorEvent> events)
        {
            var actorEvent = events[index];
            using IReadOnlyStream block = BinaryStream.Create(events.GetPayload(index));
            Fixed scalar = default;
            FixedVector3 vec3 = default;
            BrEuler euler = default;
            FixedMatrix4x3 mat = default;
            switch (actorEvent.Type)
            {
                case ActorEventType.Start:
                    if (block.TryRead(out vec3))
                        block.TryRead(out euler);
                    Position = vec3;
                    BaseMatrix = BrenderHelper.MatrixFromEuler(euler);
                    RotationMatrix = null;
                    IsPlaced = true;
                    break;
                case ActorEventType.Action:
                    block.TryRead(out Action);
                    block.TryRead(out Cell);
                    break;
                case ActorEventType.Costume:
                    break;
                case ActorEventType.Stretch:
                    if (block.Remainder >= 4)
                        Stretch.X = block.Read<Fixed>();
                    if (block.Remainder >= 4)
                        Stretch.Y = block.Read<Fixed>();
                    if (block.Remainder >= 4)
                        Stretch.Z = block.Read<Fixed>();
                    break;
                case ActorEventType.Scale:
                    if (block.TryRead(out scalar))
                        Scale = scalar;
                    break;
                case ActorEventType.Sound:
                    break;
                case ActorEventType.Position:
                    if (block.TryRead(out vec3))
                        Reposition += vec3;
                    break;
                case ActorEventType.Animate:
                    IsActionPaused = block.Read<int>() != 0;
                    break;
                case ActorEventType.Jump:
                    if (block.TryRead(out vec3))
                        Jump = vec3;
                    break;
                case ActorEventType.Path:
                    PathSpeed = block.Read<Fixed>();
                    break;
                case ActorEventType.End:
                    IsPlaced = false;
                    break;
                case ActorEventType.Transform: // TODO: Investigate TransformActorEvent and it's behavior
                    if (block.TryRead(out mat))
                        RotationMatrix = mat * BaseMatrix;
                    break;
                case ActorEventType.Rotation:
                    if (block.TryRead(out mat))
                        RotationMatrix = mat;
                    break;
            }
        }

        public Matrix4x4 CreateTransform(Vector3 origin, BrEuler orientation)
        {
            return Matrix4x4.CreateScale(Stretch * Scale) * BrenderHelper.MatrixFromEuler(orientation) * (RotationMatrix ?? BaseMatrix) * Matrix4x4.CreateTranslation(origin + Position + PathPosition + Jump);
        }

        public void EvalFrame(int frame, GenericGroup<LogicalActorEvent> events, IList<PathStep> path, Action<int, GenericGroup<LogicalActorEvent>> evalExt)
        {
            Jump = Vector3.Zero;
            Reposition = Vector3.Zero;

            if (PathIndex.Base < path.Count - 1)
            {
                var a = path[PathIndex.Base].Position;
                var b = path[PathIndex.Base + 1].Position;
                if (a.X != b.X && a.Z != b.Z)
                    BaseMatrix = Matrix4x4.CreateRotationY((float)Math.Atan2(b.X - a.X, b.Z - a.Z));
            }

            if (events != null)
            {
                while (EventIndex < events.Count && events[EventIndex].Frame < frame)
                {
                    EventIndex++;
                }

                while (EventIndex < events.Count && events[EventIndex].Frame == frame)
                {
                    var e = events[EventIndex];
                    EvalEvent(EventIndex, events);
                    evalExt(EventIndex, events);
                    if (e.Type == ActorEventType.Path)
                        PathIndex = e.Path;
                    EventIndex++;
                }
            }

            if (PathIndex.Base < path.Count - 1)
            {
                var d = path[PathIndex.Base].Weight;
                var a = path[PathIndex.Base].Position;
                var b = path[PathIndex.Base + 1].Position;
                if (d == 0)
                    PathPosition = b;
                else
                    PathPosition = Vector3.Lerp(a, b, PathIndex.Tween / d);
            }
            else
            {
                PathPosition = path[path.Count - 1].Position;
            }

            Position += Reposition;
        }

        public bool EvalPathIndex(int frame, int index, GenericGroup<LogicalActorEvent> events)
        {
            ref var e = ref events[index];
            if (e.Path <= PathIndex)
            {
                if (e.Frame != frame)
                    e.Frame = frame;
                EvalEvent(index, events);
                return true;
            }
            return false;
        }
    }

    public abstract class ActorEvent
    {
        public abstract ActorEventType Type { get; }

        public static ActorEvent Create(ActorEventType type) => type switch
        {
            ActorEventType.Action => new ActionActorEvent(),
            ActorEventType.Animate => new AnimateActorEvent(),
            ActorEventType.Costume => new CostumeActorEvent(),
            ActorEventType.End => new EndActorEvent(),
            ActorEventType.Jump => new JumpActorEvent(),
            ActorEventType.Path => new PathActorEvent(),
            ActorEventType.Position => new PositionActorEvent(),
            ActorEventType.Rotation => new RotationActorEvent(),
            ActorEventType.Scale => new ScaleActorEvent(),
            ActorEventType.Sound => new SoundActorEvent(),
            ActorEventType.Start => new StartActorEvent(),
            ActorEventType.Stretch => new StretchActorEvent(),
            ActorEventType.Transform => new TransformActorEvent(),
            _ => new UnknownActorEvent(type),
        };

        public abstract void Evaluate(Actor actor);
    }

    public class ActionActorEvent : ActorEvent
    {
        public override ActorEventType Type => ActorEventType.Action;

        public int ActionIndex { get; set; }
        public int CellIndex { get; set; }

        public override void Evaluate(Actor actor)
        {
            actor.HandleActionEvent(this);
        }
    }

    public class AnimateActorEvent : ActorEvent
    {
        public override ActorEventType Type => ActorEventType.Animate;

        public bool IsPaused { get; set; }

        public override void Evaluate(Actor actor)
        {
            actor.HandleAnimateEvent(this);
        }
    }

    public class CostumeActorEvent : ActorEvent
    {
        public override ActorEventType Type => ActorEventType.Costume;

        public override void Evaluate(Actor actor)
        {
            actor.HandleCostumeEvent(this);
        }
    }

    public class EndActorEvent : ActorEvent
    {
        public override ActorEventType Type => ActorEventType.End;

        public override void Evaluate(Actor actor)
        {
            actor.HandleEndEvent(this);
        }
    }

    public class JumpActorEvent : ActorEvent
    {
        public override ActorEventType Type => ActorEventType.Jump;

        public Vector3 Offset { get; set; }

        public override void Evaluate(Actor actor)
        {
            actor.HandleJumpEvent(this);
        }
    }

    public class PathActorEvent : ActorEvent
    {
        public override ActorEventType Type => ActorEventType.Path;

        public float Speed { get; set; }

        public override void Evaluate(Actor actor)
        {
            actor.HandlePathEvent(this);
        }
    }

    public class PositionActorEvent : ActorEvent
    {
        public override ActorEventType Type => ActorEventType.Position;

        public Vector3 Offset { get; set; }

        public override void Evaluate(Actor actor)
        {
            actor.HandlePositionEvent(this);
        }
    }

    public class RotationActorEvent : ActorEvent
    {
        public override ActorEventType Type => ActorEventType.Rotation;

        public Matrix4x4 RotationMatrix { get; set; }

        public override void Evaluate(Actor actor)
        {
            actor.HandleRotationEvent(this);
        }
    }

    public class ScaleActorEvent : ActorEvent
    {
        public override ActorEventType Type => ActorEventType.Scale;

        float Factor { get; set; }

        public override void Evaluate(Actor actor)
        {
            actor.HandleScaleEvent(this);
        }
    }

    public class SoundActorEvent : ActorEvent
    {
        public override ActorEventType Type => ActorEventType.Sound;

        public override void Evaluate(Actor actor)
        {
            actor.HandleSoundEvent(this);
        }
    }

    public class StartActorEvent : ActorEvent
    {
        public override ActorEventType Type => ActorEventType.Start;

        public Vector3 Position { get; set; }
        public BrEuler Orientation { get; set; }

        public override void Evaluate(Actor actor)
        {
            actor.HandleStartEvent(this);
        }
    }

    public class StretchActorEvent : ActorEvent
    {
        public override ActorEventType Type => ActorEventType.Stretch;

        Vector3 Factor { get; set; }

        public override void Evaluate(Actor actor)
        {
            actor.HandleStretchEvent(this);
        }
    }

    public class TransformActorEvent : ActorEvent
    {
        public override ActorEventType Type => ActorEventType.Transform;

        public override void Evaluate(Actor actor)
        {
            actor.HandleTransformEvent(this);
        }
    }

    public class UnknownActorEvent : ActorEvent
    {
        public UnknownActorEvent(ActorEventType type)
        {
            this.Type = type;
        }

        public override ActorEventType Type { get; }

        public override void Evaluate(Actor actor)
        {
            actor.HandleUnknownEvent(this);
        }
    }
}
