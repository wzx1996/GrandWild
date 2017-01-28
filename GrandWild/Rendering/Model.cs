using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;
using Vulkan;

namespace org.flamerat.GrandWild.Rendering {
    class Model:IPart {
        protected VertexBuffer<GrandWildKernel.Vertex> _VertexBufferObject;
        protected IndexBuffer _IndexBuffer;
        protected TextureImage _TextureImage; //TODO implement TextureImage class
        public void SendToGpu(Device device, DeviceMemory memory, DeviceSize offset) {

        }
        public void Draw(CommandBuffer commandBuffer) {

        }

        public void Draw(CommandBuffer commandBuffer, mat4 parentModelMatrix) {
            throw new NotImplementedException();
        }
    }
}
