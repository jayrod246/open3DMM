namespace Open3dmm
{
    partial class NativeAbstraction
    {
        static partial void ApplyPatches()
        {
            var patches = new Patch[] {
                new SkipLoadingScreensPatch()
            };

            foreach (var patch in patches)
                patch.ApplyPatch();
        }
    }
}
