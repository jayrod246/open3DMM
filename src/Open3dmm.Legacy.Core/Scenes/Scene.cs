using Open3dmm;
using Open3dmm.Core.Actors;
using Open3dmm.Core.Data;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;

namespace Open3dmm.Core.Scenes
{
    public class Scene : ResolvableObject
    {
        public int StartFrame { get; set; }
        public int EndFrame { get; set; }
        public uint Transition { get; set; }
        public SceneNode SceneRoot { get; } = new SceneNode();
        public GenericGroup<SceneEvent> FrameEvents => ResolveReferenceOrDefault<GenericGroup<SceneEvent>>(new ReferenceIdentifier(0, Tags.GGFR));
        public GenericGroup<SceneEvent> StEvents => ResolveReferenceOrDefault<GenericGroup<SceneEvent>>(new ReferenceIdentifier(1, Tags.GGST));

        protected override void ResolveCore()
        {
            using var block = Metadata.GetBlock();
            if (!block.TryRead(out LogicalScene logicalScene))
                throw ThrowHelper.BadSection(Metadata.Key);
            StartFrame = logicalScene.Start;
            EndFrame = logicalScene.End;
            Transition = logicalScene.Transition;
        }

        public bool TryGetActor(int index, out Actor actor)
        {
            return TryResolveReference(new ReferenceIdentifier(index, Tags.ACTR), out actor);
        }

        public int CurrentFrame { get; set; }

        public BackgroundInfo GetBackground()
        {
            if (TagManager.Default.TryGetThumIdentifier<ChunkIdentifier>(BackgroundReference, out var identifier))
            {
                TagManager.Default.TryGetProduct(BackgroundReference.ProductKey, out var product);
                if (product.Get3cnModule().TryResolve<BackgroundInfo>(identifier, out var backgroundInfo))
                    return backgroundInfo;
            }
            return null;
        }

        public GlobalReference BackgroundReference { get; set; }
        public int CameraId { get; private set; }

        public void ChangeCamera(int index)
        {
            // TODO: Add camera change event.
            CameraId = index;
        }
    }
}
