using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan;
using GlmNet;

namespace org.flamerat.GrandWild {
    class TextureImage : IGpuImage {
        public Image Image {
            get {
                return _Image;
            }
        }

        public ImageView ImageView {
            get {
                return _ImageView;
            }
        }

        public TextureImage(Device device, byte[,,] data, bool optimalTiling) {
            _Data = data;
            _OptimalTiling = optimalTiling;
            ImageCreateInfo imageInfo = new ImageCreateInfo {
                Extent = new Extent3D {
                    Width = (uint)_Data.GetLength(1),
                    Height = (uint)_Data.GetLength(0),
                    Depth = 1
                },
                MipLevels = 1,
                ArrayLayers = 1,
                Format = Format.R8G8B8A8Snorm,
                Tiling = (optimalTiling?ImageTiling.Optimal:ImageTiling.Linear),
                InitialLayout = ImageLayout.Preinitialized,
                SharingMode = SharingMode.Exclusive,
                Usage = ImageUsageFlags.TransferSrc,
            };
            _Image=device.CreateImage(imageInfo);
            ImageViewCreateInfo imageViewInfo = new ImageViewCreateInfo {
                Image=_Image,
                ViewType=ImageViewType.View2D,
                Format=Format.R8G8B8A8Unorm,
                SubresourceRange=new ImageSubresourceRange {
                    AspectMask=ImageAspectFlags.Color,
                    BaseMipLevel=0,
                    LevelCount=1,
                    BaseArrayLayer=0,
                    LayerCount=1
                }
            };
            _ImageView = device.CreateImageView(imageViewInfo);
        }

        public void SendToGpu(Device device, DeviceMemory memory, DeviceSize offset) {
            if (_OptimalTiling) {
                var memReqs = device.GetImageMemoryRequirements(_Image);
                var imageSize = memReqs.Size;
                var imageSubresourceLayout = device.GetImageSubresourceLayout(_Image, new ImageSubresource { AspectMask = ImageAspectFlags.Color, MipLevel = 0, ArrayLayer = 0 });
                
                unsafe
                {
                    fixed (byte* pData = _Data) {
                        byte* pDeviceMemory = (byte*)device.MapMemory(memory, offset, imageSize);
                        int realWidth = _Data.GetLength(1);
                        int paddedWidth = (int)(UInt64)imageSubresourceLayout.RowPitch;
                        byte* currSrc = pData;
                        byte* currDst = pDeviceMemory;
                        for (int i = 0; i <= _Data.GetLength(0) - 1; i++) {
                            memcpy(pDeviceMemory + i * paddedWidth * 4, pData + i * realWidth * 4, (ulong)realWidth * 4);
                        }
                    }
                }
                MappedMemoryRange memRange = new Vulkan.MappedMemoryRange();
                memRange.Memory = memory;
                memRange.Offset = offset;
                memRange.Size = imageSize;
                device.FlushMappedMemoryRange(memRange);
                device.UnmapMemory(memory);
            }
        }

        [System.Runtime.InteropServices.DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, SetLastError = false)]
        [System.Security.SuppressUnmanagedCodeSecurity]
        private unsafe static extern void* memcpy(void* dst, void* src, ulong count);

        public void DisposeRamData() {
            _Data = null;
        }

        private bool _OptimalTiling;
        private byte[,,] _Data;
        private Image _Image;
        
        private ImageView _ImageView;
    }

}