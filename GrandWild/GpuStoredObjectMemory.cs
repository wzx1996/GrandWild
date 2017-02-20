using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan;

namespace org.flamerat.GrandWild {
    public interface IGpuStoredObject {
        void DisposeRamData();
        void SendToGpu(Device device, DeviceMemory memory, DeviceSize offset);
    }
    public interface IGpuBuffer : IGpuStoredObject {
        Vulkan.Buffer Buffer { get; }
    }
    public interface IGpuImage : IGpuStoredObject {
        Vulkan.Image Image { get; }
        Vulkan.ImageView ImageView { get; }
    }
    public abstract class GpuStoredObjectMemory:IDisposable {

        protected DeviceMemory _DeviceMemory;
        protected Device _Device;

        #region IDisposable Support
        private bool disposedValue = false; 

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                }
                _Device.FreeMemory(_DeviceMemory);

                disposedValue = true;
            }
        }

        ~GpuStoredObjectMemory() {
            Dispose(false);
        }
        
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
    public class GpuBufferMemory : GpuStoredObjectMemory {
        public GpuBufferMemory(PhysicalDevice physicalDevice, Device device, IGpuBuffer[] buffers) {
            _Device = device;
            DeviceSize[] bufferSize = new DeviceSize[buffers.Length];
            DeviceSize[] bufferOffset = new DeviceSize[buffers.Length];
            DeviceSize memorySize = 0;
            bufferOffset[0] = 0;
            for (uint i = 0; i <= buffers.Length - 1; i++) {
                bufferSize[i] = device.GetBufferMemoryRequirements(buffers[i].Buffer).Size;
                if (i != 0) bufferOffset[i] = bufferSize[i-1] + bufferOffset[i - 1];
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
                _Device.BindBufferMemory(buffers[i].Buffer, _DeviceMemory, bufferOffset[i]);
                buffers[i].SendToGpu(device, _DeviceMemory, bufferOffset[i]);
            }
        }
    }
    public class GpuImageMemory : GpuStoredObjectMemory {
        public GpuImageMemory(PhysicalDevice physicalDevice, Device device, IGpuImage[] images) {
            _Device = device;
            DeviceSize[] imageSize = new DeviceSize[images.Length];
            DeviceSize[] imageOffset = new DeviceSize[images.Length];
            DeviceSize memorySize = 0;
            imageOffset[0] = 0;
            for (uint i = 0; i <= images.Length - 1; i++) {
                imageSize[i] = device.GetImageMemoryRequirements(images[i].Image).Size;
                if (i != 0) imageOffset[i] = imageSize[i-1] + imageOffset[i - 1];
                memorySize += imageSize[i];
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
                _Device.BindImageMemory(images[i].Image, _DeviceMemory, imageOffset[i]);
                images[i].SendToGpu(device, _DeviceMemory, imageOffset[i]);
            }
        }
    }
}
