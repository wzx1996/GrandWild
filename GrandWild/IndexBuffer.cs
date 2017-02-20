using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan;

namespace org.flamerat.GrandWild {
    public class IndexBuffer : IGpuBuffer {
        public static implicit operator Vulkan.Buffer(IndexBuffer indexBuffer) {
            return indexBuffer.Buffer;
        }
        public Vulkan.Buffer Buffer {
            get { return _Buffer; }
        }
        public uint Size { get; private set; }
        public UInt16[] Data { get { return _Data; } }
        public IndexBuffer(Device device, UInt16[] data) {
            _Data = data;
            Size =(uint) data.Length;
            BufferCreateInfo bufferInfo = new BufferCreateInfo {
                Usage = BufferUsageFlags.IndexBuffer,
                Size = sizeof(UInt16) * data.Length,
                SharingMode = SharingMode.Exclusive
            };
            _Buffer = device.CreateBuffer(bufferInfo);
        }
        public void SendToGpu(Device device, DeviceMemory memory, DeviceSize offset) {
            var memReqs = device.GetBufferMemoryRequirements(_Buffer);
            var bufferSize = memReqs.Size;
            var pDeviceMemory = device.MapMemory(memory, offset, bufferSize);
            unsafe
            {
                fixed(UInt16* pData = _Data) {
                    for(int i = 0; i <= _Data.Length - 1; i++) {
                        ((UInt16*)pDeviceMemory)[i] = pData[i];
                    }
                }
            }
            MappedMemoryRange memRange = new Vulkan.MappedMemoryRange();
            memRange.Memory = memory;
            memRange.Offset = offset;
            memRange.Size = bufferSize;
            device.FlushMappedMemoryRange(memRange);
            device.UnmapMemory(memory);
        }
        public void DisposeRamData() {
            _Data = null;
        }
        private UInt16[] _Data;
        private Vulkan.Buffer _Buffer;
    }
}
