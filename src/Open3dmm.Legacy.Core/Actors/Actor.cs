using Open3dmm;
using Open3dmm.Core.Data;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Open3dmm.Core.Actors
{
    public class Actor : ResolvableObject
    {
        private ActorState state = ActorState.Default;
        private GlobalReference tmplReference;
        private Body body;
        public Template Template {
            get {
                Template template;
                if (tmplReference.ProductKey == 0)
                    TryResolveReference(new ReferenceIdentifier(0, Tags.TMPL), out template);
                else
                    TagManager.Default.TryResolve3cn(tmplReference, out template);
                return template;
            }
        }
        public Body Body => body ??= new Body(Template, this);
        public GenericGroup<LogicalActorEvent> Events => ResolveReference<GenericGroup<LogicalActorEvent>>(new ReferenceIdentifier(0, Tags.GGAE));
        public IList<PathStep> Path => ResolveReference<GenericList<PathStep>>(new ReferenceIdentifier(0, Tags.PATH));
        public int StartFrame { get; set; }
        public int EndFrame { get; set; }
        public Vector3 Position1 { get; set; }
        public Vector3 Position2 { get; set; }
        public Vector3 FinalPosition { get; set; }
        public uint Index { get; set; }
        public float Scale { get; set; }
        public Vector3 Stretch { get; set; }
        public Matrix4x4 RotationMatrix { get; set; } = Matrix4x4.Identity;
        public Vector3 PathPosition { get; set; }
        public ref ActorState CurrentState => ref state;

        public void ChangeTemplate(GlobalReference tmplReference)
        {
            this.tmplReference = tmplReference;
            body?.Remove();
            body = null;
        }

        public Matrix4x4 CalculateTransform()
        {
            return state.CreateTransform(Position1, Body.Template.Rotation);
            //return Matrix4x4.CreateScale(Stretch * Scale)
            //       * RotationMatrix
            //       * Matrix4x4.CreateTranslation(FinalPosition + PathPosition);
        }

        public void RecalcPosition()
        {
            FinalPosition = Position1 + Position2;
        }

        public void EvalFrame(int fr)
        {
            Body.LoadDefaultCostume();
            state = ActorState.Default;
            if (StartFrame <= fr)
            {
                state.EvalFrame(StartFrame, Events, Path, handleCostumeChange);
                for (int i = StartFrame + 1; i <= fr; i++)
                {
                    if (!state.IsActionPaused)
                    {
                        ++state.Cell;
                    }

                    if (state.PathSpeed != 0)
                    {
                        state.RotationMatrix = null;
                    }

                    NextPathIndex(ref state);
                    state.EvalFrame(i, Events, Path, handleCostumeChange);
                }
                Body.LoadAction(state.Action, state.Cell);
            }
        }

        private void handleCostumeChange(int index, GenericGroup<LogicalActorEvent> events)
        {
            if (events[index].Type == ActorEventType.Costume)
            {
                using IReadOnlyStream block = BinaryStream.Create(events.GetPayload(index));
                block.TryRead(out int f0x0);
                block.TryRead(out int f0x4);
                block.TryRead(out int f0x8);
                if (f0x8 == 0)
                {
                    block.TryRead(out GlobalReference reference);
                }
                else
                {
                    Body.LoadCostume(f0x4);
                }
            }
        }

        private void NextPathIndex(ref ActorState state)
        {
            if (state.PathSpeed != 0)
            {
                float tween = state.PathIndex.Tween;
                float speed;
                if (state.PathSpeed == -1)
                {
                    if (!Body.Template.TryGetOrCreateActionInfo(state.Action, out var actionInfo))
                        throw new InvalidOperationException("Couldn't find action Id: " + state.Action);
                    speed = actionInfo.Cells[state.Cell % actionInfo.Cells.Count].Item1;
                }
                else
                    speed = state.PathSpeed;
                tween += speed * state.Scale;

                float stepLength;
                while (tween > (stepLength = Path[state.PathIndex.Base].Weight))
                {
                    tween -= stepLength;
                    if (++state.PathIndex.Base >= Path.Count)
                    {
                        state.PathIndex.Base = Path.Count - 1;
                        break;
                    }
                }
                state.PathIndex.Tween = (Fixed)tween;
            }
        }

        protected override void ResolveCore()
        {
            using var block = Metadata.GetBlock();
            if (!block.MagicNumber()
            || !block.TryRead(out FixedVector3 position)
            || !block.TryRead(out uint actorIndex)
            || !block.TryRead(out int startFrame)
            || !block.TryRead(out int endFrame)
            || !block.TryRead(out tmplReference))
                throw ThrowHelper.BadSection(Metadata.Key);
            Index = actorIndex;
            Position1 = position;
            StartFrame = startFrame;
            EndFrame = endFrame;
        }

        public void EvaluateEvent(ActorEvent actorEvent) => actorEvent.Evaluate(this);

        internal void HandleUnknownEvent(UnknownActorEvent unknownActorEvent)
        {
            throw new InvalidOperationException("Unknown actor event: " + unknownActorEvent.Type);
        }

        internal void HandleStartEvent(StartActorEvent startActorEvent)
        {
            throw new NotImplementedException();
        }

        internal void HandleActionEvent(ActionActorEvent actionActorEvent)
        {
            throw new NotImplementedException();
        }

        internal void HandleAnimateEvent(AnimateActorEvent animateActorEvent)
        {
            throw new NotImplementedException();
        }

        internal void HandleCostumeEvent(CostumeActorEvent costumeActorEvent)
        {
            throw new NotImplementedException();
        }

        internal void HandleEndEvent(EndActorEvent endActorEvent)
        {
            throw new NotImplementedException();
        }

        internal void HandleJumpEvent(JumpActorEvent jumpActorEvent)
        {
            throw new NotImplementedException();
        }

        internal void HandlePathEvent(PathActorEvent pathActorEvent)
        {
            throw new NotImplementedException();
        }

        internal void HandlePositionEvent(PositionActorEvent positionActorEvent)
        {
            throw new NotImplementedException();
        }

        internal void HandleRotationEvent(RotationActorEvent rotationActorEvent)
        {
            throw new NotImplementedException();
        }

        internal void HandleScaleEvent(ScaleActorEvent scaleActorEvent)
        {
            throw new NotImplementedException();
        }

        internal void HandleSoundEvent(SoundActorEvent soundActorEvent)
        {
            throw new NotImplementedException();
        }

        internal void HandleStretchEvent(StretchActorEvent stretchActorEvent)
        {
            throw new NotImplementedException();
        }

        internal void HandleTransformEvent(TransformActorEvent transformActorEvent)
        {
            throw new NotImplementedException();
        }
    }
}
