using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;
using Vulkan;

namespace org.flamerat.GrandWild.Rendering {
    interface IPart:IRenderable {
        void Draw(CommandBuffer commandBuffer, mat4 parentModelMatrix);
    }

    static class VulkanCommandBufferDrawIPartExtension {
        public static void CmdDrawPartGw(this CommandBuffer commandBuffer, IPart part, mat4 modelMatrix) {
            part.Draw(commandBuffer, modelMatrix);
        }
    }
}
