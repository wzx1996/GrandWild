using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;
using Vulkan;
using org.flamerat.GrandWild;

namespace org.flamerat.GrandWild.Rendering {
    public class Model:IPart {
        public VertexBuffer<GrandWildKernel.Vertex> VertexBuffer;
        public IndexBuffer IndexBuffer;
        public TextureImage TextureImage;
        public virtual void Draw(CommandBuffer commandBuffer) {
            Draw(commandBuffer, new mat4(1));
        }

        public void Draw(CommandBuffer commandBuffer, mat4 parentModelMatrix) {
            

            commandBuffer.CmdSetModelMatrixGw(parentModelMatrix);
            commandBuffer.CmdSelectTextureIamgeGw(TextureImage);
            commandBuffer.CmdBindVertexBuffer(0, VertexBuffer.Buffer, 0);
            commandBuffer.CmdBindIndexBuffer(IndexBuffer, 0, IndexType.Uint16);
            commandBuffer.CmdDrawIndexed(IndexBuffer.Size, 1, 0, 0, 0);
            
        }


        IEnumerable<IGpuBuffer> IRenderable.GetVertexBuffers() {
            yield return VertexBuffer;
        }

        IEnumerable<IGpuBuffer> IRenderable.GetIndexBuffers() {
            yield return IndexBuffer;
        }

        IEnumerable<IGpuImage> IRenderable.GetTextureImages() {
            yield return TextureImage;
        }
    }
}
