using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan;

namespace org.flamerat.GrandWild {
    public static class CommandBufferSelectTextureImageExtension {
        private class SamplerPool {
            public bool IsFull() {
                return _Size == _CurrentIndex;
            }
            public void Reset() {
                _CurrentIndex = 0;
            }
            public DescriptorSet SetTexture(TextureImage texture) {
                for(uint i = 0; i <= _CurrentIndex - 1; i++) {
                    if (texture == _Images[i]) return _Sets[i];
                }

                if (_CurrentIndex == _Size) Reset();

                _Images[_CurrentIndex] = texture;

                _Device.UpdateDescriptorSet(
                    pDescriptorCopie: null,
                    pDescriptorWrite: new WriteDescriptorSet {
                        DescriptorCount = 1,
                        DescriptorType = DescriptorType.CombinedImageSampler,
                        DstSet = _Sets[_CurrentIndex],
                        DstBinding = 0,
                        DstArrayElement = 0,
                        ImageInfo = new DescriptorImageInfo[1] {
                            new DescriptorImageInfo {
                                ImageLayout=texture.Layout,
                                ImageView=texture.ImageView,
                                Sampler=_Samplers[_CurrentIndex]
                            }
                        }
                    });

                _CurrentIndex++;
                return _Sets[_CurrentIndex - 1];


            }
            public SamplerPool(Device device,DescriptorSetLayout setLayout) {
                _Device = device;
                SetLayout = setLayout;
                var poolInfo = new DescriptorPoolCreateInfo() {
                    MaxSets = _Size,
                    PoolSizeCount = 1,
                    PoolSizes = new DescriptorPoolSize[1] {
                        new DescriptorPoolSize {
                            DescriptorCount=1,
                            Type=DescriptorType.CombinedImageSampler
                        }
                    }
                };

                _Pool = _Device.CreateDescriptorPool(poolInfo);
                var setInfo = new DescriptorSetAllocateInfo {
                    DescriptorPool = _Pool,
                    DescriptorSetCount = _Size,
                    SetLayouts = (from i in new int[_Size] select SetLayout).ToArray()
                };

                _Sets = _Device.AllocateDescriptorSets(setInfo);

                var samplerInfo = new SamplerCreateInfo {
                    MinFilter = Filter.Linear,
                    MagFilter = Filter.Nearest,
                    AnisotropyEnable = true,
                    MaxAnisotropy = 16,
                    AddressModeU = SamplerAddressMode.Repeat,
                    AddressModeV = SamplerAddressMode.Repeat,
                    AddressModeW = SamplerAddressMode.Repeat,
                    BorderColor = BorderColor.IntOpaqueBlack,
                    UnnormalizedCoordinates = false,
                    CompareEnable = false,
                    CompareOp = CompareOp.Always,
                    MipmapMode = SamplerMipmapMode.Linear,
                    MipLodBias = 0.0F,
                    MinLod = 0.0F,
                    MaxLod = 0.0F
                };

                _Samplers = (from i in new int[_Size] select _Device.CreateSampler(samplerInfo)).ToArray();

                _Images = new TextureImage[_Size];
            }
            private TextureImage[] _Images;
            private Device _Device;
            public PipelineLayout PipelineLayout;
            private DescriptorSetLayout SetLayout;
            public uint SetIndex;
            public Queue GraphicsQueue;
            private Sampler[] _Samplers;
            private uint _Size=256;
            private uint _CurrentIndex=0;
            private DescriptorSet[] _Sets;
            private DescriptorPool _Pool;
        }
        private static Dictionary<CommandBuffer,SamplerPool> _Pools=new Dictionary<CommandBuffer, SamplerPool>();
        public static void InitSelectTextureImageGw(this CommandBuffer commandBuffer,
                                                    Device device,PipelineLayout pipelineLayout,
                                                    DescriptorSetLayout setLayout,uint setIndex,Queue graphicsQueue) {
            if (!_Pools.ContainsKey(commandBuffer)) {
                var pool = new SamplerPool(device,setLayout);
                pool.PipelineLayout = pipelineLayout;
                pool.SetIndex = setIndex;
                pool.GraphicsQueue = graphicsQueue;
                _Pools.Add(commandBuffer, pool);
            }
        }
        public static bool CmdSelectTextureIamgeGw(this CommandBuffer commandBuffer, TextureImage texture) {
            var pool = _Pools[commandBuffer];
            commandBuffer.CmdBindDescriptorSet(PipelineBindPoint.Graphics, pool.PipelineLayout, pool.SetIndex, pool.SetTexture(texture), null);
            if (pool.IsFull()) return true;
            else return false;
        }
        public static void FlushTextureImageGw(this CommandBuffer commandBufer) {
            _Pools[commandBufer].Reset();
        }
        public static bool IsSamplerPoolFullGw(this CommandBuffer commandBuffer) {
            return _Pools[commandBuffer].IsFull();
        }
    }
}
