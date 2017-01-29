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

    class VertexBuffer<T>:IGpuBuffer where T:struct {
        public static implicit operator Vulkan.Buffer(VertexBuffer<T> vertexBuffer) {
            return vertexBuffer.Buffer;
        }
        public Vulkan.Buffer Buffer {
            get { return _VBO; }
        }
        public uint Size { get; private set; }
        public T[] Data { get { return _Data; } }
        public VertexBuffer(Device device,T[] data) {
            Size = (uint)data.Length;
            BufferCreateInfo bufferInfo = new BufferCreateInfo {
                Usage = BufferUsageFlags.VertexBuffer,
                Size = System.Runtime.InteropServices.Marshal.SizeOf<T>()*data.Length,
                SharingMode = SharingMode.Exclusive
            };
            _VBO=device.CreateBuffer(bufferInfo);
        }
        public void SendToGpu(Device device, DeviceMemory memory, DeviceSize offset) {
            var memReqs = device.GetBufferMemoryRequirements(_VBO);
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



}
