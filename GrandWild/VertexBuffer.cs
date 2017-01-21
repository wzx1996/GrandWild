///<summary>
///A class for defining VBOs and a class to create the VRAM to send these VBOs into.
///</summary>
/// Usage: Create a List of VertexBuffer (represents a VBO) and then create a VertexBufferMemory of that List to send all the VBOs into the memory.
/// 
/// Copyright (C) FlameRat 2016-2017
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with this program.If not, see<http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan;

namespace org.flamerat.GrandWild {
    interface IVertexBuffer {
        Vulkan.Buffer VBO { get; }
        void SendToGpu(Device device, DeviceMemory memory, DeviceSize offset);
    }
    class VertexBuffer<T>:IVertexBuffer where T:struct {
        public Vulkan.Buffer VBO {
            get { return _VBO; }
        }
        public T[] Data { get { return _Data; } }
        public VertexBuffer(Device device,T[] data) {
            BufferCreateInfo bufferInfo = new BufferCreateInfo {
                Usage = BufferUsageFlags.VertexBuffer,
                Size = System.Runtime.InteropServices.Marshal.SizeOf(T[]),
                SharingMode = SharingMode.Exclusive
            };
            _VBO=device.CreateBuffer(bufferInfo);
        }
        public void SendToGpu(Device device, DeviceMemory memory, DeviceSize offset) {
            var memReqs = device.GetBufferMemoryRequirements(VBO);
            var bufferSize = memReqs.Size;
            var pDeviceMemory = device.MapMemory(memory, offset, bufferSize);
            System.Runtime.InteropServices.Marshal.StructureToPtr(_Data, pDeviceMemory, false);
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
        private T[] _Data;
        private Vulkan.Buffer _VBO;
    }
    class VertexBufferMemory {
        public VertexBufferMemory(PhysicalDevice physicalDevice,Device device,IVertexBuffer[] buffers) {
            _Device = device;
            DeviceSize[] bufferSize = new DeviceSize[buffers.Length];
            DeviceSize[] bufferOffset = new DeviceSize[buffers.Length];
            DeviceSize memorySize=0;
            bufferOffset[0] = 0;
            for(uint i = 0; i <= buffers.Length - 1; i++) {
                bufferSize[i] = device.GetBufferMemoryRequirements(buffers[i].VBO).Size;
                if (i != 0) bufferOffset[i] = bufferSize[i] + bufferOffset[i - 1];
                memorySize += bufferSize[i];
            }

            Vulkan.PhysicalDeviceMemoryProperties memoryProperty = physicalDevice.GetMemoryProperties();
            var memoryTypes = device.GetBufferMemoryRequirements(buffers[0]).MemoryTypeBits;

            uint selectedMemory = 0;
            for (uint i = 0; i < memoryProperty.MemoryTypeCount; i++) {
                if (((memoryTypes >> (int)i) & 1) == 1) {
                    if ((memoryProperty.MemoryTypes[i].PropertyFlags & Vulkan.MemoryPropertyFlags.HostVisible) == Vulkan.MemoryPropertyFlags.HostVisible) {
                        selectedMemory = i;
                        break;
                    }
                }
            }
            MemoryAllocateInfo memAllocInfo = new MemoryAllocateInfo {
                AllocationSize = memorySize,
                MemoryTypeIndex = selectedMemory

            };
            _DeviceMemory = device.AllocateMemory(memAllocInfo);
            for(uint i=0; i <= buffers.Length - 1; i++) {
                buffers[i].SendToGpu(device, _DeviceMemory, bufferOffset[i]);
            }
        }
        ~VertexBufferMemory() {
            _Device.FreeMemory(_DeviceMemory);
        }
        private DeviceMemory _DeviceMemory;
        private Device _Device;
    }
}
