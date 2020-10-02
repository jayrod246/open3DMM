using Open3dmm.Core;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using Open3dmm.Core.Scenes;
using System;

namespace Open3dmm.MovieEditor
{
    [Obsolete]
    public class SceneDeprecated
    {
        public SceneDeprecated(GlobalReference background)
        {
            BackgroundReference = background;
        }

        public int FrameStart { get; set; }
        public int FrameEnd { get; set; }
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
        public int CameraId { get; set; }
        public int CameraThumId { get; internal set; }
    }
}