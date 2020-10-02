using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace Open3dmm.Core.Veldrid
{
    public class CustomUniform<T> where T : struct
    {
        private readonly DeviceBuffer uniformBuffer;
        private readonly uint offset;
        private T value;

        public CustomUniform(DeviceBuffer uniformBuffer, uint offset = 0)
        {
            this.uniformBuffer = uniformBuffer;
            this.offset = offset;
        }

        public void Set(CommandList commandList, T newValue)
        {
            value = newValue;
            commandList.UpdateBuffer(uniformBuffer, offset, newValue);
        }

        public T Get() => value;
    }

    public abstract class RendererBase
    {
        protected RendererBase(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            CreateResources();

        }

        public GraphicsDevice GraphicsDevice { get; }

        protected static void CreateShaders(ResourceFactory factory, ref Shader[] shaders, string vertexCode, string fragmentCode)
        {
            var vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(vertexCode),
                "main");

            var fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(fragmentCode),
                "main");

            shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
        }

        protected static void CreatePipeline(ResourceFactory factory, ref Pipeline pipeline, GraphicsPipelineDescription pipelineDescription)
        {
            pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
        }

        protected static OutputDescription StandardOutput { get; } = new OutputDescription(new OutputAttachmentDescription(PixelFormat.R32_Float), new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm));

        protected static RasterizerStateDescription StandardRasterizerState { get; } = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.CounterClockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: true);

        protected static DepthStencilStateDescription StandardDepthStencilState { get; } = new DepthStencilStateDescription(
                    depthTestEnabled: true,
                    depthWriteEnabled: true,
                    comparisonKind: ComparisonKind.LessEqual);

        protected abstract void CreateResources();

    }

    public abstract class CustomRenderer : RendererBase
    {
        private SharedResources shared;

        public CustomUniform<Matrix4x4> Projection => Shared.uniformProjection;
        public CustomUniform<Matrix4x4> View => Shared.uniformView;
        public CustomUniform<Matrix4x4> World => Shared.uniformWorld;
        public CustomUniform<float> Blend => Shared.uniformBlend;
        public CustomUniform<Matrix4x4> TextureTransform => Shared.uniformTextureTransform;
        public CustomUniform<Vector3> Light => Shared.uniformLight;
        public CustomUniform<Vector2> PaletteSlice => Shared.uniformPaletteSlice;
        public ColorSwap ColorSwap => Shared.colorSwap;
        public SharedResources Shared => shared ??= new SharedResources(GraphicsDevice);
        public CustomRenderer(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
        }

        public ResourceSet CreateTextureSet(Texture tex, Sampler sampler)
        {
            return GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                Shared.textureLayout,
                tex,
                sampler));
        }

        public class SharedResources
        {
            public ColorSwap colorSwap;
            public DeviceBuffer projectionBuffer;
            public DeviceBuffer viewBuffer;
            public DeviceBuffer worldBuffer;
            public DeviceBuffer blendBuffer;
            public DeviceBuffer textureTransformBuffer;
            public DeviceBuffer lightBuffer;
            public ResourceLayout cameraLayout;
            public ResourceLayout worldLayout;
            public ResourceLayout textureLayout;
            public ResourceLayout texturedModelLayout;
            public ResourceLayout shadedModelLayout;
            public ResourceSet cameraSet;
            public ResourceSet worldSet;
            public ResourceSet texturedModelSet;
            public ResourceSet shadedModelSet;
            public VertexLayoutDescription vertexLayout;
            public CustomUniform<Matrix4x4> uniformProjection;
            public CustomUniform<Matrix4x4> uniformView;
            public CustomUniform<Matrix4x4> uniformWorld;
            public CustomUniform<float> uniformBlend;
            public CustomUniform<Matrix4x4> uniformTextureTransform;
            public DeviceBuffer useShadedBuffer;
            public CustomUniform<float> uniformUseShaded;
            public DeviceBuffer paletteSliceBuffer;
            public CustomUniform<Vector2> uniformPaletteSlice;
            public CustomUniform<Vector3> uniformLight;

            public SharedResources(GraphicsDevice graphicsDevice)
            {
                var factory = graphicsDevice.ResourceFactory;
                colorSwap = new ColorSwap(graphicsDevice);
                vertexLayout = new VertexLayoutDescription(new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                                                               new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                                                               new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

                projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
                uniformProjection = new CustomUniform<Matrix4x4>(projectionBuffer);
                viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
                uniformView = new CustomUniform<Matrix4x4>(viewBuffer);
                worldBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
                uniformWorld = new CustomUniform<Matrix4x4>(worldBuffer);
                blendBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
                uniformBlend = new CustomUniform<float>(blendBuffer);
                lightBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
                uniformLight = new CustomUniform<Vector3>(lightBuffer);
                textureTransformBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
                uniformTextureTransform = new CustomUniform<Matrix4x4>(textureTransformBuffer);
                useShadedBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
                uniformUseShaded = new CustomUniform<float>(useShadedBuffer);
                paletteSliceBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
                uniformPaletteSlice = new CustomUniform<Vector2>(paletteSliceBuffer);
                cameraLayout = factory.CreateResourceLayout(
                    new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                        new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

                worldLayout = factory.CreateResourceLayout(
                     new ResourceLayoutDescription(
                         new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                         new ResourceLayoutElementDescription("BlendBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                         new ResourceLayoutElementDescription("LightBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

                textureLayout = factory.CreateResourceLayout(
                    new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("Tex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("Samp", ResourceKind.Sampler, ShaderStages.Fragment)));

                texturedModelLayout = factory.CreateResourceLayout(
                    new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("TextureTransformBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

                shadedModelLayout = factory.CreateResourceLayout(
                    new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("UseShadedBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("PaletteSliceBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

                cameraSet = factory.CreateResourceSet(new ResourceSetDescription(
                    cameraLayout,
                    projectionBuffer,
                    viewBuffer));

                worldSet = factory.CreateResourceSet(new ResourceSetDescription(
                    worldLayout,
                    worldBuffer,
                    blendBuffer,
                    lightBuffer));

                texturedModelSet = factory.CreateResourceSet(new ResourceSetDescription(
                    texturedModelLayout,
                    textureTransformBuffer));

                shadedModelSet = factory.CreateResourceSet(new ResourceSetDescription(
                    shadedModelLayout,
                    useShadedBuffer,
                    paletteSliceBuffer));
            }
        }

        public abstract void Use(CommandList commandList, bool shaded);
    }

    public class TexturedRenderer : CustomRenderer
    {
        private const string TexturedVertexCode = @"
#version 450

layout(set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
};
layout(set = 0, binding = 1) uniform ViewBuffer
{
    mat4 View;
};
layout(set = 1, binding = 0) uniform WorldBuffer
{
    mat4 World;
};
layout(set = 1, binding = 1) uniform BlendBuffer
{
    float Blend;
};
layout(set = 3, binding = 0) uniform TextureTransformBuffer
{
    mat4 TextureTransform;
};
layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 TexCoord;
layout(location = 3) in vec3 Position2;
layout(location = 4) in vec3 Normal2;

layout(location = 0) out vec2 fsin_TexCoord;
layout(location = 1) out vec3 fsin_Normal;

void main()
{
    vec4 worldPosition = World * vec4(Position + (Position2 - Position) * Blend, 1);
    vec4 viewPosition = View * worldPosition;
    vec4 clipPosition = Projection * viewPosition;
    gl_Position = clipPosition;
    fsin_Normal = normalize(vec3(transpose(inverse(World)) * vec4(Normal + (Normal2 - Normal) * Blend, 1)));
    fsin_TexCoord = vec2(TextureTransform * vec4(TexCoord, 1.0, 1.0));
    fsin_TexCoord.y = 1.0 - fsin_TexCoord.y;
}";

        private const string TexturedFragmentCode = @"
#version 450

layout(set = 1, binding = 3) uniform LightBuffer
{
    vec3 Light;
};

layout(set = 2, binding = 0) uniform texture2D Tex;
layout(set = 2, binding = 1) uniform sampler Samp;

layout(set = 4, binding = 0) uniform UseShadedBuffer
{
    float UseShaded;
};

layout(set = 4, binding = 1) uniform PaletteSliceBuffer
{
    vec2 PaletteSlice;
};

layout(location = 0) in vec2 fsin_TexCoord;
layout(location = 1) in vec3 fsin_Normal;

layout(location = 0) out vec4 fsout_Color;

void main()
{
    vec3 lightDir = normalize(Light);
    float value = (dot(fsin_Normal, lightDir) + 1) / 2;
    if(UseShaded == 0)
    {
        float intensity = mix(0.67, 1.067, value);
        vec4 texColor = texture(sampler2D(Tex, Samp), fsin_TexCoord);
        fsout_Color = texColor * intensity;
    }
    else
    {
        float index = (PaletteSlice.x + PaletteSlice.y * value) / 256.0;
        fsout_Color = texture(sampler2D(Tex, Samp), vec2(index,0.0));
    }
}";
        private Shader[] texturedShaders;
        private Pipeline texturedPipeline;

        public TexturedRenderer(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
        }

        protected override void CreateResources()
        {
            var factory = GraphicsDevice.ResourceFactory;

            CreateShaders(factory, ref texturedShaders, TexturedVertexCode, TexturedFragmentCode);
            CreatePipeline(factory, ref texturedPipeline, new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = StandardDepthStencilState,
                RasterizerState = StandardRasterizerState,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = new[] { Shared.cameraLayout, Shared.worldLayout, Shared.textureLayout, Shared.texturedModelLayout, Shared.shadedModelLayout },
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] { Shared.vertexLayout, Shared.vertexLayout },
                    shaders: texturedShaders),
                Outputs = StandardOutput,
            });
        }

        public override void Use(CommandList commandList, bool shaded)
        {
            commandList.SetPipeline(texturedPipeline);
            commandList.SetGraphicsResourceSet(0, Shared.cameraSet);
            commandList.SetGraphicsResourceSet(1, Shared.worldSet);
            commandList.SetGraphicsResourceSet(3, Shared.texturedModelSet);
            commandList.SetGraphicsResourceSet(4, Shared.shadedModelSet);
            Shared.uniformUseShaded.Set(commandList, shaded ? 1 : 0);
        }
    }

    public class BackgroundRenderer : RendererBase
    {
        private const string VertexCode = @"
#version 450

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 TexCoord;

layout(location = 0) out vec2 fsin_TexCoord;

void main()
{
    gl_Position = vec4(Position, 1);
    fsin_TexCoord = TexCoord;
}
";

        private const string FragmentCode = @"
#version 450

layout(set = 0, binding = 0) uniform texture2D Color;
layout(set = 0, binding = 1) uniform texture2D Depth;
layout(set = 0, binding = 2) uniform sampler Sampler;

layout(location = 0) in vec2 fsin_TexCoord;

layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = texture(sampler2D(Color, Sampler), fsin_TexCoord);
    vec4 d = texture(sampler2D(Depth, Sampler), fsin_TexCoord);
    gl_FragDepth = clamp(d.x + 0.5, 0, 1);
}
";
        private Shader[] shaders;
        private Pipeline pipeline;
        private VertexLayoutDescription vertexLayout;
        private ResourceLayout textureLayout;
        private Dictionary<TextureSetBinding, ResourceSet> textureSetBindings = new Dictionary<TextureSetBinding, ResourceSet>();
        private DeviceBuffer vertexBuffer;
        private DeviceBuffer indexBuffer;

        public BackgroundRenderer(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
        }

        protected override void CreateResources()
        {
            var factory = GraphicsDevice.ResourceFactory;
            CreateShaders(factory, ref shaders, VertexCode, FragmentCode);
            CreatePipeline(factory, ref pipeline, shaders);
            this.vertexBuffer = factory.CreateBuffer(new BufferDescription(VertexPositionTexture.SizeInBytes * 4, BufferUsage.VertexBuffer));
            this.indexBuffer = factory.CreateBuffer(new BufferDescription(2 * 6, BufferUsage.IndexBuffer));
            int min = -1;
            int yInverted = 1;
            Span<VertexPositionTexture> vertices = stackalloc VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(min, min, 0), new Vector2(0, yInverted)),
                new VertexPositionTexture(new Vector3(1, min, 0), new Vector2(1, yInverted)),
                new VertexPositionTexture(new Vector3(min, 1, 0), new Vector2(0, 1 - yInverted)),
                new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 1 - yInverted)),
            };
            Span<ushort> indices = stackalloc ushort[]
            {
                0,
                1,
                2,
                1,
                3,
                2,
            };
            GraphicsDevice.UpdateBuffer(vertexBuffer, 0, ref vertices[0], vertexBuffer.SizeInBytes);
            GraphicsDevice.UpdateBuffer(indexBuffer, 0, ref indices[0], indexBuffer.SizeInBytes);
        }

        private void CreatePipeline(ResourceFactory factory, ref Pipeline pipeline, Shader[] shaders)
        {
            vertexLayout = new VertexLayoutDescription(new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                                                           new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

            textureLayout = factory.CreateResourceLayout(
                    new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("Color", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("Depth", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            var pipelineDescription = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = StandardDepthStencilState,
                RasterizerState = StandardRasterizerState,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = new[] { textureLayout },
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                    shaders: shaders),
                Outputs = StandardOutput,
            };

            pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
        }

        public void Render(CommandList commandList, Texture colorMap, Texture depthMap)
        {
            var textureSet = GetOrCreateBinding(colorMap, depthMap);
            commandList.SetPipeline(pipeline);
            commandList.SetGraphicsResourceSet(0, textureSet);
            commandList.SetVertexBuffer(0, vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
            commandList.DrawIndexed(6);
        }

        private ResourceSet GetOrCreateBinding(Texture colorMap, Texture depthMap)
        {
            var binding = new TextureSetBinding(colorMap, depthMap);
            if (!textureSetBindings.TryGetValue(binding, out var textureSet))
                textureSetBindings[binding] = textureSet = CreateTextureSet(colorMap, depthMap, GraphicsDevice.LinearSampler);
            return textureSet;
        }

        private ResourceSet CreateTextureSet(Texture colorMap, Texture depthMap, Sampler sampler)
        {
            return GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                textureLayout,
                colorMap,
                depthMap,
                sampler));
        }

        private struct TextureSetBinding
        {
            public Texture ColorMap;
            public Texture DepthMap;

            public TextureSetBinding(Texture colorMap, Texture depthMap)
            {
                this.ColorMap = colorMap;
                this.DepthMap = depthMap;
            }

            public override bool Equals(object obj)
            {
                return obj is TextureSetBinding binding &&
                       EqualityComparer<Texture>.Default.Equals(this.ColorMap, binding.ColorMap) &&
                       EqualityComparer<Texture>.Default.Equals(this.DepthMap, binding.DepthMap);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(this.ColorMap, this.DepthMap);
            }
        }
    }
}
