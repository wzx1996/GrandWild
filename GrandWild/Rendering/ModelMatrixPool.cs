using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan;
using GlmNet;

namespace org.flamerat.GrandWild.Rendering {
    class ModelMatrixPool {


        public ModelMatrixPool(PhysicalDevice physicalDevice, Device device, uint size=65536) {
            _PhysicalDevice = physicalDevice;
            _Device = device;
            _Size = size;
            _CurrentIndex = 0;
            _buffer = new UniformBuffer<mat4>(_PhysicalDevice, _Device, 1);
            DescriptorPoolCreateInfo poolInfo = new DescriptorPoolCreateInfo {
                MaxSets = _Size,
                PoolSizeCount = 1,
                PoolSizes = new DescriptorPoolSize[1] {
                    new DescriptorPoolSize {
                        Type=DescriptorType.UniformBuffer,
                        DescriptorCount=_Size
                    }
                }
            };
            _Device.CreateDescriptorPool(poolInfo);

            DescriptorSetLayoutCreateInfo layoutInfo = new DescriptorSetLayoutCreateInfo {
                BindingCount = 1,
                Bindings = new DescriptorSetLayoutBinding[1] {
                    new DescriptorSetLayoutBinding {
                        Binding=0,
                        DescriptorType=DescriptorType.UniformBuffer,
                        DescriptorCount=1,
                        StageFlags=ShaderStageFlags.Vertex
                    }
                }
            };
            _DescriptorSetLayout = _Device.CreateDescriptorSetLayout(layoutInfo);
            DescriptorSetLayout[] setLayouts = new DescriptorSetLayout[_Size];
            for (uint i = 0; i <= _Size - 1; i++) setLayouts[i] = _DescriptorSetLayout;
            DescriptorSetAllocateInfo allocInfo = new DescriptorSetAllocateInfo {
                DescriptorPool = _DescriptorPool,
                DescriptorSetCount = _Size,
                SetLayouts = setLayouts
            };
            _DescriptorSets = _Device.AllocateDescriptorSets(allocInfo);

        }
        public DescriptorSet Put(mat4 matrix) {
            _buffer.Commit(1, matrix);
            WriteDescriptorSet writeInfo = new WriteDescriptorSet {
                DstSet = _DescriptorSets[_CurrentIndex],
                DescriptorCount = 1,
                DescriptorType = DescriptorType.UniformBuffer,
                BufferInfo = new DescriptorBufferInfo[] { _buffer.BufferInfo[0] },
                DstArrayElement = 0,
                DstBinding = 0
            };
            _Device.UpdateDescriptorSet(writeInfo,null);
            var currentIndex = _CurrentIndex;
            _CurrentIndex++;
            _CurrentIndex %= _Size;
            return _DescriptorSets[currentIndex];
        }

        
        private UniformBuffer<mat4> _buffer;
        public Device _Device { get; private set; }
        private PhysicalDevice _PhysicalDevice;
        private uint _Size;
        private uint _CurrentIndex;
        public DescriptorSetLayout _DescriptorSetLayout;
        private DescriptorSet[] _DescriptorSets;
        private DescriptorPool _DescriptorPool;
    }
}

namespace org.flamerat.GrandWild {
    static class CommandBufferModelMatrixPoolExtension {
        public static Dictionary<CommandBuffer, Rendering.ModelMatrixPool> ModelMatrixPools;
        public static Dictionary<CommandBuffer, PipelineLayout> PipelineLayouts;
        public static Dictionary<CommandBuffer, uint> SetIds;
        public static void CmdAllocateModelMatrixPoolGw(this CommandBuffer commandBuffer,PhysicalDevice physicalDevice,Device device,PipelineLayout pipelineLayout,uint setId,uint size=65536) {
            if (!ModelMatrixPools.ContainsKey(commandBuffer)) {
                PipelineLayouts[commandBuffer] = pipelineLayout;
                ModelMatrixPools[commandBuffer] = new Rendering.ModelMatrixPool(physicalDevice,device,size);
                SetIds[commandBuffer] = setId;
            }
        }
        public static void CmdSetModelMatrixGw(this CommandBuffer commandBuffer,mat4 modelMatrix) {
            var currentSet = ModelMatrixPools[commandBuffer].Put(modelMatrix);

            commandBuffer.CmdBindDescriptorSet(PipelineBindPoint.Graphics, PipelineLayouts[commandBuffer], SetIds[commandBuffer], currentSet, null);

        }
    }
}
