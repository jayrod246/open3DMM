using Open3dmm;
using Open3dmm.Core.Data;
using Open3dmm.Core.Resolvers;

namespace Open3dmm.Core.Scenes
{
    public class BackgroundInfo : ResolvableObject
    {
        protected override void ResolveCore()
        {
        }

        public bool TryGetCameraInfo(int cameraIndex, out CameraInfo cameraInfo)
            => TryResolveReference(new ReferenceIdentifier(cameraIndex, Tags.CAM ), out cameraInfo);

        public GenericList<ColorBgra> Palette => ResolveReference<GenericList<ColorBgra>>(new ReferenceIdentifier(0, Tags.GLCR));
    }
}
