///<summary>
///A class for maintain the IO of uniformed buffer so that it would be a bit easier to use.
///</summary>
///
/// Usage: Define an instance of this class with a struct type, 
/// and then you can use the Structure member to access the
/// in-memory copy of the union buffer data.
/// 
/// Use BufferInfo member to get the info required for creating descriptor sets.
/// 
/// Notice that the Structure data isn't automatically synchonized with the
/// actual union buffer. 
/// 
/// You should use Commit() and Clone() to commit to or clone from union buffer.
/// 
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

namespace org.flamerat.GrandWild {
    class MaintainedUniformBuffer<T> where T:struct {
        /// <summary>
        /// The Structure for storing the in-memory version of the buffer.
        /// </summary>
        public T Structure;

        /// <summary>
        /// The information for creating descriptor sets.
        /// </summary>
        public Vulkan.DescriptorBufferInfo BufferInfo { get; private set; }

        public MaintainedUniformBuffer(Vulkan.PhysicalDevice physicalDevice,Vulkan.Device device) {
            _PhysicalDevice = physicalDevice;
            _Device = device;

            Vulkan.BufferCreateInfo bufferInfo = new Vulkan.BufferCreateInfo();
            bufferInfo.Usage = Vulkan.BufferUsageFlags.UniformBuffer;
            bufferInfo.Size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            bufferInfo.SharingMode = Vulkan.SharingMode.Exclusive;

            _Buffer=_Device.CreateBuffer(bufferInfo);

            Vulkan.MemoryRequirements memoryRequirements = _Device.GetBufferMemoryRequirements(_Buffer);
            Vulkan.MemoryAllocateInfo memAllocInfo = new Vulkan.MemoryAllocateInfo();
            memAllocInfo.AllocationSize = memoryRequirements.Size;
            Vulkan.PhysicalDeviceMemoryProperties memoryProperty= _PhysicalDevice.GetMemoryProperties();
            uint i = 0;
            for (i = 0; i < memoryProperty.MemoryTypeCount; i++) {
                if (((memoryRequirements.MemoryTypeBits>>(int)i) & 1) == 1) {
                    if((memoryProperty.MemoryTypes[i].PropertyFlags&Vulkan.MemoryPropertyFlags.HostVisible)== Vulkan.MemoryPropertyFlags.HostVisible) {
                        break;
                    }
                }
            }

            _UniformBufferMemory = _Device.AllocateMemory(memAllocInfo);
            _MemorySize = memoryRequirements.Size;

            BufferInfo.Buffer = _Buffer;
            BufferInfo.Offset = 0;
            BufferInfo.Range = _MemorySize;

            Commit();
        }

        /// <summary>
        /// Commit Structure into uniform buffer
        /// </summary>
        public void Commit() {
            IntPtr pDeviceMemory=_Device.MapMemory(_UniformBufferMemory, 0, _MemorySize);
            System.Runtime.InteropServices.Marshal.StructureToPtr(Structure,pDeviceMemory,false);
            Vulkan.MappedMemoryRange memRange=new Vulkan.MappedMemoryRange();
            memRange.Memory = _UniformBufferMemory;
            memRange.Offset = 0;
            memRange.Size = _MemorySize;
            _Device.FlushMappedMemoryRange(memRange);
            _Device.UnmapMemory(_UniformBufferMemory);
        }

        /// <summary>
        /// Clone uniform buffer into Structure
        /// </summary>
        public void Clone() {
            IntPtr pDeviceMemory = _Device.MapMemory(_UniformBufferMemory, 0, _MemorySize);
            System.Runtime.InteropServices.Marshal.PtrToStructure(pDeviceMemory, Structure);
            Vulkan.MappedMemoryRange memRange = new Vulkan.MappedMemoryRange();
            memRange.Memory = _UniformBufferMemory;
            memRange.Offset = 0;
            memRange.Size = _MemorySize;
            _Device.FlushMappedMemoryRange(memRange);
            _Device.UnmapMemory(_UniformBufferMemory);
        }
        
        private Vulkan.PhysicalDevice _PhysicalDevice;
        private Vulkan.Device _Device;
        private Vulkan.DeviceSize _MemorySize;
        private Vulkan.DeviceMemory _UniformBufferMemory;
        private Vulkan.Buffer _Buffer;
    }
}
