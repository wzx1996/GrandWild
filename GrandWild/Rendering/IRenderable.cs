using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;
using Vulkan;

namespace org.flamerat.GrandWild.Rendering {
    interface IRenderable:IGpuStoredObject {
        void Draw(CommandBuffer commandBuffer);
    }

    static class VulkanCommandBufferDrawIRenderableExtension {
        static void CmdDrawRenderableGw(this CommandBuffer commandBuffer,IRenderable target) {
            target.Draw(commandBuffer);
        }
    }
}
