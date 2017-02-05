///<summary>
///An extension to simplify the process pushing Model and ViewProjectionClip matrixes into command buffer push constants.
///</summary>
///Usage: call CommandBuffer.InitMvpcManagementGw() first, then use
///CmdSetVpcMatrixGw() and CmdSetModelMatrixGw() to record command for
///setting these matrixes into the buffer. 
///
///Reason for separating Model and VPC matrix is for potential optimization posibility
///by using pre-baked command buffer (for stationary object rendering, for example)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;
using Vulkan;
using System.Runtime.InteropServices;
namespace org.flamerat.GrandWild {
    static class CommandBufferPushConstantMvpcMatrixExtension {
        private static Dictionary<CommandBuffer, uint> _ModelMatrixPushOffset;
        private static Dictionary<CommandBuffer, uint> _VpcMatrixPushOffset;
        private static Dictionary<CommandBuffer, PipelineLayout> _PipelineLayout;
        private const uint _SizeOfMat4 = 16 * 4;
        /// <summary>
        /// Prepare the environment to use the PushConstantMvpcMatrixGw extension
        /// </summary>
        /// <param name="commandBuffer"></param>
        /// <param name="mvpcMatrixPushOffset"></param>
        public static void InitMvpcManagementGw(this CommandBuffer commandBuffer,PipelineLayout pipelineLayout,uint modelMatrixPushOffset = 0,uint vpcMatrixPushOffset=16*4) {
            if (!_ModelMatrixPushOffset.ContainsKey(commandBuffer)) {
                _ModelMatrixPushOffset[commandBuffer] = modelMatrixPushOffset;
                _VpcMatrixPushOffset[commandBuffer] = vpcMatrixPushOffset;
                _PipelineLayout[commandBuffer] = pipelineLayout;
            }
        }

        /// <summary>
        /// Command for setting the ViewProjectionClip matrix for the command buffer. 
        /// </summary>
        /// <param name="commandBuffer"></param>
        /// <param name="vpc"></param>
        public static void CmdSetVpcMatrixGw(this CommandBuffer commandBuffer,mat4 vpc) {
            IntPtr pVpc=new IntPtr();
            Marshal.StructureToPtr(vpc, pVpc, false);
            commandBuffer.CmdPushConstants(
                layout: _PipelineLayout[commandBuffer],
                stageFlags: ShaderStageFlags.Vertex,
                offset: _VpcMatrixPushOffset[commandBuffer],
                size: _SizeOfMat4,
                pValues: pVpc
            );
            System.Runtime.InteropServices.Marshal.DestroyStructure(pVpc, typeof(mat4));
        }

        /// <summary>
        /// Command for setting the model matrix for the command buffer. 
        /// </summary>
        /// <param name="commandBuffer"></param>
        /// <param name="vpc"></param>
        public static void CmdSetModelMatrixGw(this CommandBuffer commandBuffer, mat4 m) {
            IntPtr pM = new IntPtr();
            Marshal.StructureToPtr(m, pM, false);
            commandBuffer.CmdPushConstants(
                layout: _PipelineLayout[commandBuffer],
                stageFlags: ShaderStageFlags.Vertex,
                offset: _ModelMatrixPushOffset[commandBuffer],
                size: _SizeOfMat4,
                pValues: pM
            );
            Marshal.DestroyStructure(pM, typeof(mat4));
        }
    }
}
