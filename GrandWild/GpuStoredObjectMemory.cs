using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan;

namespace org.flamerat.GrandWild {
    interface IGpuStoredObject {
        void DisposeRamData();
        void SendToGpu(Device device, DeviceMemory memory, DeviceSize offset);
    }
    interface IGpuBuffer : IGpuStoredObject {
        Vulkan.Buffer Buffer { get; }
    }
    interface IGpuImage : IGpuStoredObject {
        Vulkan.Image Image { get; }
        Vulkan.ImageView ImageView { get; }
    }
    abstract class GpuStoredObjectMemory {
        ~GpuStoredObjectMemory() {
            _Device.FreeMemory(_DeviceMemory);
        }
        protected DeviceMemory _DeviceMemory;
        protected Device _Device;
    }
    class GpuBufferMemory : GpuStoredObjectMemory {
        public GpuBufferMemory(PhysicalDevice physicalDevice, Device device, IGpuBuffer[] buffers) {
            _Device = device;
            DeviceSize[] bufferSize = new DeviceSize[buffers.Length];
            DeviceSize[] bufferOffset = new DeviceSize[buffers.Length];
            DeviceSize memorySize = 0;
            bufferOffset[0] = 0;
            for (uint i = 0; i <= buffers.Length - 1; i++) {
                bufferSize[i] = device.GetBufferMemoryRequirements(buffers[i].Buffer).Size;
                if (i != 0) bufferOffset[i] = bufferSize[i] + bufferOffset[i - 1];
                memorySize += bufferSize[i];
            }

            Vulkan.PhysicalDeviceMemoryProperties memoryProperty = physicalDevice.GetMemoryProperties();
            var memoryTypes = device.GetBufferMemoryRequirements(buffers[0].Buffer).MemoryTypeBits;

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
            for (uint i = 0; i <= buffers.Length - 1; i++) {
                buffers[i].SendToGpu(device, _DeviceMemory, bufferOffset[i]);
            }
        }
    }
    class GpuImageMemory : GpuStoredObjectMemory {
        public GpuImageMemory(PhysicalDevice physicalDevice, Device device, IGpuImage[] images) {
            _Device = device;
            DeviceSize[] ImageSize = new DeviceSize[images.Length];
            DeviceSize[] ImageOffset = new DeviceSize[images.Length];
            DeviceSize memorySize = 0;
            ImageOffset[0] = 0;
            for (uint i = 0; i <= images.Length - 1; i++) {
                ImageSize[i] = device.GetImageMemoryRequirements(images[i].Image).Size;
                if (i != 0) ImageOffset[i] = ImageSize[i] + ImageOffset[i - 1];
                memorySize += ImageSize[i];
            }

            Vulkan.PhysicalDeviceMemoryProperties memoryProperty = physicalDevice.GetMemoryProperties();
            var memoryTypes = device.GetImageMemoryRequirements(images[0].Image).MemoryTypeBits;

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
            for (uint i = 0; i <= images.Length - 1; i++) {
                images[i].SendToGpu(device, _DeviceMemory, ImageOffset[i]);
            }
        }
    }
}
