using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;
using Vulkan;

namespace org.flamerat.GrandWild.Rendering {
    interface IRenderable {
        void Draw(CommandBuffer commandBuffer);
        IEnumerable<IGpuBuffer> GetVertexBuffers();
        IEnumerable<IGpuBuffer> GetIndexBuffers();
        IEnumerable<IGpuImage> GetTextureImages();
    }


}

namespace org.flamerat.GrandWild {
    static class VulkanCommandBufferDrawIRenderableExtension {
        static void CmdDrawRenderableGw(this CommandBuffer commandBuffer, Rendering.IRenderable target) {
            target.Draw(commandBuffer);
        }
    }
}
