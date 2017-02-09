///<summary>
///A class for allocating a uniform buffer array to be used in descriptor sets.
///</summary>
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
    public class UniformBuffer<T>:IDisposable where T:struct {

        /// <summary>
        /// The information for updating descriptor sets.
        /// </summary>
        public Vulkan.DescriptorBufferInfo[] BufferInfo { get; private set; }
        public uint Size {
            get { return _Size; }
        }

        public UniformBuffer(Vulkan.PhysicalDevice physicalDevice,Vulkan.Device device,uint size = 1) {
            _PhysicalDevice = physicalDevice;
            _Device = device;
            _Size = size;

            BufferInfo = new Vulkan.DescriptorBufferInfo[_Size];

            _Buffer = new Vulkan.Buffer[size];
            for (uint i = 0; i <= size - 1; i++) {
                Vulkan.BufferCreateInfo bufferCreateInfo = new Vulkan.BufferCreateInfo();
                bufferCreateInfo.Usage = Vulkan.BufferUsageFlags.UniformBuffer;
                bufferCreateInfo.Size = _StructureSize;
                bufferCreateInfo.SharingMode = Vulkan.SharingMode.Exclusive;
                _Buffer[i] = _Device.CreateBuffer(bufferCreateInfo);
            }

            Vulkan.MemoryRequirements memoryRequirements = _Device.GetBufferMemoryRequirements(_Buffer[0]);
            Vulkan.MemoryAllocateInfo memAllocInfo = new Vulkan.MemoryAllocateInfo();
            memAllocInfo.AllocationSize = memoryRequirements.Size*size;
            Vulkan.PhysicalDeviceMemoryProperties memoryProperty = _PhysicalDevice.GetMemoryProperties();

            uint selectedMemory = 0;
            for (uint i = 0; i < memoryProperty.MemoryTypeCount; i++) {
                if (((memoryRequirements.MemoryTypeBits>>(int)i) & 1) == 1) {
                    if((memoryProperty.MemoryTypes[i].PropertyFlags&Vulkan.MemoryPropertyFlags.HostVisible)== Vulkan.MemoryPropertyFlags.HostVisible) {
                        selectedMemory = i;
                        break;
                    }
                }
            }

            memAllocInfo.MemoryTypeIndex = selectedMemory;
            _UniformBufferMemory = _Device.AllocateMemory(memAllocInfo);

            _MemorySize = memoryRequirements.Size;
            for(uint i = 0; i <= _Size; i++) {
                _Device.BindBufferMemory(_Buffer[i], _UniformBufferMemory, 0);
                BufferInfo[i].Buffer = _Buffer[i];
                BufferInfo[i].Offset = 0;
                BufferInfo[i].Range = _StructureSize;
            }
        }

        /// <summary>
        /// Commit Structure into uniform buffer
        /// </summary>
        public void Commit(uint index,T data) {
            IntPtr pDeviceMemory=_Device.MapMemory(_UniformBufferMemory, index*_MemorySize, _MemorySize);
            System.Runtime.InteropServices.Marshal.StructureToPtr(data,pDeviceMemory,false);
            Vulkan.MappedMemoryRange memRange=new Vulkan.MappedMemoryRange();
            memRange.Memory = _UniformBufferMemory;
            memRange.Offset = index * _MemorySize;
            memRange.Size = _MemorySize;
            _Device.FlushMappedMemoryRange(memRange);
            _Device.UnmapMemory(_UniformBufferMemory);
        }

        private readonly ulong _StructureSize = (ulong)System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
        private uint _Size;
        private Vulkan.PhysicalDevice _PhysicalDevice;
        private Vulkan.Device _Device;
        private Vulkan.DeviceSize _MemorySize;
        private Vulkan.DeviceMemory _UniformBufferMemory;
        private Vulkan.Buffer[] _Buffer;

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    
                }
                _Device.FreeMemory(_UniformBufferMemory);
                foreach (var buffer in _Buffer) _Device.DestroyBuffer(buffer);

                disposedValue = true;
            }
        }

        ~UniformBuffer() {
            Dispose(false);
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose() {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}