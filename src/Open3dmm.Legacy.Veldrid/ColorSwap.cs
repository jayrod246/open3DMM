using System;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace Open3dmm.Core.Veldrid
{
    public class ColorSwap
    {
        const string ComputeCode = @"
#version 450

layout(set = 0, binding = 0, rgba8) uniform image2D PaletteTex;
layout(set = 0, binding = 1, r8) uniform image2D IndexedTex;
layout(set = 0, binding = 2, rgba8) uniform image2D OutputTex;

layout(local_size_x = 16, local_size_y = 16, local_size_z = 1) in;

void main()
{
    float index = imageLoad(IndexedTex, ivec2(gl_GlobalInvocationID.xy)).x;
    vec4 color;
    if(index != 0)
        color = imageLoad(PaletteTex, ivec2(index * 256, 0));
    imageStore(OutputTex, ivec2(gl_GlobalInvocationID.xy), color);
}
";
        private ResourceFactory factory;
        private ResourceLayout computeLayout;
        private Pipeline computePipeline;
        private Shader computeShader;

        public ColorSwap(GraphicsDevice graphicsDevice)
        {
            CreateResources(graphicsDevice.ResourceFactory);
        }

        private void CreateResources(ResourceFactory factory)
        {
            this.factory = factory;
            computeLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("PaletteTex", ResourceKind.TextureReadWrite, ShaderStages.Compute),
                new ResourceLayoutElementDescription("IndexedTex", ResourceKind.TextureReadWrite, ShaderStages.Compute),
                new ResourceLayoutElementDescription("OutputTex", ResourceKind.TextureReadWrite, ShaderStages.Compute)));

            var computeShaderDesc = new ShaderDescription(
                ShaderStages.Compute,
                Encoding.UTF8.GetBytes(ComputeCode),
                "main");

            computeShader = factory.CreateFromSpirv(computeShaderDesc);

            var computePipelineDesc = new ComputePipelineDescription(
                computeShader,
                computeLayout,
                16, 16, 1);

            computePipeline = factory.CreateComputePipeline(ref computePipelineDesc);
        }

        public ColorSwapToken CreateToken(Texture palette, Texture indexed, Texture output)
        {
            var width = indexed.Width;
            var height = indexed.Height;
            if (width != output.Width || height != output.Height)
                throw new InvalidOperationException("indexed texture and output texture must be the same size");
            var computeResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                computeLayout,
                palette,
                indexed,
                output));
            return new ColorSwapToken()
            {
                ComputeResourceSet = computeResourceSet,
                Width = width / 16 + 16,
                Height = height / 16 + 16
            };
        }

        public void Swap(CommandList commandList, ColorSwapToken token)
        {
            commandList.SetPipeline(computePipeline);
            commandList.SetComputeResourceSet(0, token.ComputeResourceSet);
            commandList.Dispatch(token.Width, token.Height, 1);
        }

        public struct ColorSwapToken
        {
            public ResourceSet ComputeResourceSet;
            public uint Width;
            public uint Height;
        }
    }
}
