///<summary>
///GrandWild 0.2 Kernel
///</summary>
///
/// Usage: Define an instance of this class, set up necessarily property and
/// call Launch().
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan.Windows;
using GlmNet;

namespace org.flamerat.GrandWild
{
    public class GrandWildKernel {
        private string _AppName = "GrandWild 0.2 Application";
        public string AppName {
            get { return _AppName; }
            set { if (!IsRunning) _AppName = value; }
        }
        private uint _AppVersion = 0;
        public uint AppVersion {
            get { return _AppVersion; }
            set { if (!IsRunning) _AppVersion = value; }
        }

        private uint _WindowWidth = 640;
        public uint WindowWidth {
            get { return _WindowWidth; }
            set {
                if (!IsRunning) {
                    System.Drawing.Size newWindowSize = _DisplayForm.ClientSize;
                    newWindowSize.Width = (int)value;
                    _DisplayForm.ClientSize = newWindowSize;
                    _WindowWidth = value;
                }
            }
        }
        private uint _WindowHeight = 480;
        public uint WindowHeight {
            get { return _WindowHeight; }
            set {
                if (!IsRunning) {
                    System.Drawing.Size newWindowSize = _DisplayForm.ClientSize;
                    newWindowSize.Height = (int)value;
                    _DisplayForm.ClientSize = newWindowSize;
                    _WindowHeight = value;
                }
            }
        }
        private bool _IsBorderLess = false;
        public bool IsBorderLess {
            get { return _IsBorderLess; }
            set {
                _IsBorderLess = value;
                if (value) {
                    _DisplayForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                } else {
                    _DisplayForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
                }
            }
        }
        public string WindowTitle {
            get { return _DisplayForm.Text; }
            set {
                _DisplayForm.Text = value;
            }
        }

        public bool IsRunning { get; private set; } = false;

        private Vulkan.Instance _Instance;
        private System.Windows.Forms.Form _DisplayForm;
        private Vulkan.SurfaceKhr _Surface;
        private Vulkan.PhysicalDevice _PhysicalDevice;
        private Vulkan.Device _Device;
        private uint _DeviceGraphicsQueueFamilyIndex = 0;
        private uint _DevicePresentQueueFamilyIndex = 0;
        private Vulkan.CommandPool _CommandPool;
        private uint _SwapchainImageCount = 1;
        private Vulkan.SwapchainKhr _Swapchain;
        private Vulkan.Extent2D _SwapchainExtent;
        private Vulkan.PresentModeKhr _SwapchainPresentMode;
        private Vulkan.SampleCountFlags _SampleCount = Vulkan.SampleCountFlags.Count1;
        private Vulkan.Image[] _SwapchainImages;
        private Vulkan.ImageView[] _SwapchainImagseView;
        private Vulkan.Format _SwapchainImageFormat;
        private Vulkan.Image _DepthBufferImage;
        private Vulkan.ImageView _DepthBufferImageView;
        private Vulkan.Format _DepthImageFormat;
        private Vulkan.DescriptorSetLayout _RenderingInfoSetLayout; //TEST_STUB
        private Vulkan.DescriptorPool _RenderingInfoPool; //TEST_STUB
        private Vulkan.DescriptorSet[] _RenderingInfoSets;
        private Vulkan.PipelineLayout _RenderingPipelineLayout;
        private Vulkan.RenderPass _RenderPass;


        private Vulkan.Semaphore _ImageAcquireSemaphore;

        //TEST_STUB
        public struct BasicRenderingInfo {
            public mat4 Model;
            public mat4 View;
            public mat4 Projection;
            public mat4 Clip;
            public vec3 GlobalLightDirection;
        }
        //TEST_STUB
        private UniformBuffer<BasicRenderingInfo> _RenderingInfoBuffer;


        public void Launch() {
            _InitInstance();
            _InitSurface();
            _InitDevice();
            _InitSwapchain();
            _InitDepthBuffer();
            _InitPipelineLayout();
            _InitDescriptorSets();
            _InitRenderPass();


        }

        private void _InitInstance() {
            Vulkan.ApplicationInfo appInfo = new Vulkan.ApplicationInfo(); 
            appInfo.ApplicationName = AppName;
            appInfo.ApplicationVersion = AppVersion;
            appInfo.EngineName = "GrandWild";
            appInfo.EngineVersion = 2;
            appInfo.ApiVersion = Vulkan.Version.Make(1, 0, 0);

            List<string> extensions = new List<string>();
            extensions.Add("VK_KHR_surface");
            extensions.Add("VK_KHR_swapchain");
            extensions.Add("VK_KHR_display");
            extensions.Add("VK_KHR_win32_surface");

            List<string> layers = new List<string>();
#if DEBUG
            {
                layers.Add("VK_LAYER_LUNARG_standard_validation");
            }
#endif

            Vulkan.InstanceCreateInfo instanceInfo = new Vulkan.InstanceCreateInfo();
            instanceInfo.ApplicationInfo = appInfo;
            instanceInfo.EnabledExtensionNames = extensions.ToArray();
            instanceInfo.EnabledLayerNames = layers.ToArray();

            try {
                _Instance = new Vulkan.Instance(instanceInfo);
            }catch(Exception e) {
                Console.Error.WriteLine("Could not create a Vulkan Instance: {0}", e.Message);
            }
        }

        private void _InitSurface() {
            Win32SurfaceCreateInfoKhr surfaceInfo = new Win32SurfaceCreateInfoKhr();
            surfaceInfo.Hinstance = System.Diagnostics.Process.GetCurrentProcess().Handle;
            surfaceInfo.Hwnd = _DisplayForm.Handle;
            try {
                _Surface = _Instance.CreateWin32SurfaceKHR(surfaceInfo);
            }catch(Exception e) {
                Console.Error.WriteLine("Couldn't create Vulkan surface {0}", e.Message);
            }
        }

        private void _InitDevice() {
            _PhysicalDevice = _Instance.EnumeratePhysicalDevices()[0];
            var queueFamilyProperties = _PhysicalDevice.GetQueueFamilyProperties();
            bool foundGraphicsBit = false;
            bool foundPresentSupport = false;
            for (uint i = 0; i < queueFamilyProperties.Length; i++) {
                if ((!foundGraphicsBit) && (queueFamilyProperties[i].QueueFlags.HasFlag(Vulkan.QueueFlags.Graphics))) {
                    foundGraphicsBit = true;
                    _DeviceGraphicsQueueFamilyIndex = i;
                }
                if ((!foundPresentSupport) && (_PhysicalDevice.GetSurfaceSupportKHR(i, _Surface))) {
                    foundPresentSupport = true;
                    _DevicePresentQueueFamilyIndex = i;
                }
            }
            if (!foundGraphicsBit) throw new Exception("No available graphics queue for first GPU");
            if (!foundPresentSupport) throw new Exception("No available present queue for first GPU");

            Vulkan.DeviceQueueCreateInfo queueInfo = new Vulkan.DeviceQueueCreateInfo();
            queueInfo.QueueFamilyIndex = _DeviceGraphicsQueueFamilyIndex;
            queueInfo.QueueCount = 1;
            queueInfo.QueuePriorities = new float[1] { 0F };

            Vulkan.DeviceCreateInfo deviceInfo = new Vulkan.DeviceCreateInfo();
            deviceInfo.QueueCreateInfos = new Vulkan.DeviceQueueCreateInfo[1] { queueInfo };

            _Device = _PhysicalDevice.CreateDevice(deviceInfo);


            Vulkan.CommandPoolCreateInfo commandPoolInfo = new Vulkan.CommandPoolCreateInfo();
            commandPoolInfo.QueueFamilyIndex = _DeviceGraphicsQueueFamilyIndex;

            _CommandPool = _Device.CreateCommandPool(commandPoolInfo);

            Vulkan.CommandBufferAllocateInfo commandBufferAllocInfo = new Vulkan.CommandBufferAllocateInfo();
            commandBufferAllocInfo.CommandPool = _CommandPool;
            commandBufferAllocInfo.Level = Vulkan.CommandBufferLevel.Primary;
            commandBufferAllocInfo.CommandBufferCount = 1;

            _Device.AllocateCommandBuffers(commandBufferAllocInfo);
        }

        private void _InitSwapchain() {
            Vulkan.SurfaceCapabilitiesKhr surfaceCapabilities = _PhysicalDevice.GetSurfaceCapabilitiesKHR(_Surface);
            _SwapchainImageCount = surfaceCapabilities.MinImageCount;
            if (surfaceCapabilities.CurrentExtent.Width == 0xFFFFFFFF) {
                _SwapchainExtent.Width = _WindowWidth;
                _SwapchainExtent.Height = _WindowHeight;

                if (_SwapchainExtent.Width < surfaceCapabilities.MinImageExtent.Width) _SwapchainExtent.Width = surfaceCapabilities.MinImageExtent.Width;
                else if (_SwapchainExtent.Width > surfaceCapabilities.MaxImageExtent.Width) _SwapchainExtent.Width = surfaceCapabilities.MaxImageExtent.Width;

                if (_SwapchainExtent.Height < surfaceCapabilities.MinImageExtent.Height) _SwapchainExtent.Height = surfaceCapabilities.MinImageExtent.Height;
                else if (_SwapchainExtent.Height > surfaceCapabilities.MaxImageExtent.Height) _SwapchainExtent.Height = surfaceCapabilities.MaxImageExtent.Height;
            }

            Vulkan.SwapchainCreateInfoKhr swapchainInfo = new Vulkan.SwapchainCreateInfoKhr();
            swapchainInfo.Surface = _Surface;
            swapchainInfo.MinImageCount = _SwapchainImageCount;

            var surfaceFormat = _PhysicalDevice.GetSurfaceFormatsKHR(_Surface);
            if (surfaceFormat[0].Format != Vulkan.Format.Undefined) {
                _SwapchainImageFormat = surfaceFormat[0].Format;
            }else {
                _SwapchainImageFormat = Vulkan.Format.B8G8R8A8Snorm;
            }
            swapchainInfo.ImageFormat = _SwapchainImageFormat;
            swapchainInfo.ImageExtent = _SwapchainExtent;

            if (surfaceCapabilities.SupportedTransforms.HasFlag(Vulkan.SurfaceTransformFlagsKhr.Identity))
                swapchainInfo.PreTransform = surfaceCapabilities.CurrentTransform;
            else
                swapchainInfo.PreTransform = Vulkan.SurfaceTransformFlagsKhr.Identity;

            swapchainInfo.CompositeAlpha = Vulkan.CompositeAlphaFlagsKhr.Opaque;
            swapchainInfo.ImageArrayLayers = 1;

            swapchainInfo.PresentMode = Vulkan.PresentModeKhr.Fifo;
            var availablePresentModes = _PhysicalDevice.GetSurfacePresentModesKHR(_Surface);
            foreach(var mode in availablePresentModes) {
                if (mode == Vulkan.PresentModeKhr.Mailbox) {
                    swapchainInfo.PresentMode = Vulkan.PresentModeKhr.Mailbox;
                    break;
                }
            }

            swapchainInfo.Clipped = true;
            swapchainInfo.ImageColorSpace = Vulkan.ColorSpaceKhr.SrgbNonlinear;
            swapchainInfo.ImageUsage = Vulkan.ImageUsageFlags.ColorAttachment;
            swapchainInfo.ImageSharingMode = Vulkan.SharingMode.Exclusive;

            if (_DeviceGraphicsQueueFamilyIndex != _DevicePresentQueueFamilyIndex) {
                swapchainInfo.ImageSharingMode = Vulkan.SharingMode.Concurrent;
                swapchainInfo.QueueFamilyIndexCount = 2;
                swapchainInfo.QueueFamilyIndices = new uint[] { _DeviceGraphicsQueueFamilyIndex, _DevicePresentQueueFamilyIndex };
            }

            _Swapchain = _Device.CreateSwapchainKHR(swapchainInfo);

            _SwapchainImages = _Device.GetSwapchainImagesKHR(_Swapchain);
            _SwapchainImageCount = (uint)_SwapchainImages.Length;
            _SwapchainImagseView = new Vulkan.ImageView[2];
            Vulkan.ImageViewCreateInfo[] viewInfos = new Vulkan.ImageViewCreateInfo[_SwapchainImageCount];
            for (uint i = 0; i <= _SwapchainImageCount - 1; i++) {
                viewInfos[i].Image = _SwapchainImages[i];
                viewInfos[i].ViewType = Vulkan.ImageViewType.View2D;
                viewInfos[i].Format = _SwapchainImageFormat;
                viewInfos[i].Components = new Vulkan.ComponentMapping {
                    R = Vulkan.ComponentSwizzle.R,
                    G = Vulkan.ComponentSwizzle.G,
                    B = Vulkan.ComponentSwizzle.B,
                    A = Vulkan.ComponentSwizzle.A
                };

                viewInfos[i].SubresourceRange = new Vulkan.ImageSubresourceRange {
                    AspectMask = Vulkan.ImageAspectFlags.Color,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                };

                _SwapchainImagseView[i] = _Device.CreateImageView(viewInfos[i]);
            }
        }

        private void _InitDepthBuffer() {
            Vulkan.ImageCreateInfo depthImageInfo = new Vulkan.ImageCreateInfo {
                ImageType = Vulkan.ImageType.Image2D,
                Format = Vulkan.Format.D16Unorm,
                Extent = new Vulkan.Extent3D {
                    Width = _WindowWidth,
                    Height = _WindowHeight,
                    Depth = 1
                },
                MipLevels = 1,
                ArrayLayers = 1,
                Samples = _SampleCount,
                InitialLayout = Vulkan.ImageLayout.Undefined,
                Usage = Vulkan.ImageUsageFlags.DepthStencilAttachment,
                SharingMode = Vulkan.SharingMode.Exclusive
            };
            _DepthBufferImage = _Device.CreateImage(depthImageInfo);

            var memoryRequirements = _Device.GetImageMemoryRequirements(_DepthBufferImage);
            Vulkan.MemoryAllocateInfo depthImageMemAllocInfo = new Vulkan.MemoryAllocateInfo {
                AllocationSize = memoryRequirements.Size,
                MemoryTypeIndex = 0
            };

            var depthImageMemory = _Device.AllocateMemory(depthImageMemAllocInfo);
            _Device.BindImageMemory(_DepthBufferImage, depthImageMemory, 0);

            _DepthImageFormat = Vulkan.Format.D16Unorm;

            Vulkan.ImageViewCreateInfo viewInfo = new Vulkan.ImageViewCreateInfo {
                Image = _DepthBufferImage,
                Format = Vulkan.Format.D16Unorm,
                Components = new Vulkan.ComponentMapping {
                    R = Vulkan.ComponentSwizzle.R,
                    G = Vulkan.ComponentSwizzle.G,
                    B = Vulkan.ComponentSwizzle.B,
                    A = Vulkan.ComponentSwizzle.A
                },
                SubresourceRange = new Vulkan.ImageSubresourceRange {
                    AspectMask = Vulkan.ImageAspectFlags.Color,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                },
                ViewType=Vulkan.ImageViewType.View2D
            };
            _DepthBufferImageView = _Device.CreateImageView(viewInfo);

        }

        //Uniform buffer ∈ Descriptor ∈ Descritor set ∈ Pipeline layout
        //TEST_STUB
        private void _InitPipelineLayout() {
            _RenderingInfoBuffer = new UniformBuffer<BasicRenderingInfo>(_PhysicalDevice, _Device, 1);
            Vulkan.DescriptorSetLayoutBinding[] renderingSetLayoutBindings = new Vulkan.DescriptorSetLayoutBinding[1];

            renderingSetLayoutBindings[0].Binding = 0;
            renderingSetLayoutBindings[0].DescriptorType = Vulkan.DescriptorType.UniformBuffer;
            renderingSetLayoutBindings[0].DescriptorCount = 1;
            renderingSetLayoutBindings[0].StageFlags = Vulkan.ShaderStageFlags.Vertex;

            Vulkan.DescriptorSetLayoutCreateInfo renderSetLayoutInfo = new Vulkan.DescriptorSetLayoutCreateInfo {
                BindingCount = 1,
                Bindings = renderingSetLayoutBindings
            };

            _RenderingInfoSetLayout = _Device.CreateDescriptorSetLayout(renderSetLayoutInfo);

            Vulkan.PipelineLayoutCreateInfo renderingPipelineLayoutInfo = new Vulkan.PipelineLayoutCreateInfo {
                PushConstantRangeCount = 0,
                SetLayoutCount = 1,
                SetLayouts = new Vulkan.DescriptorSetLayout[1] { _RenderingInfoSetLayout } 
            };
            _RenderingPipelineLayout = _Device.CreatePipelineLayout(renderingPipelineLayoutInfo);

        }

        private void _InitDescriptorSets() {
            Vulkan.DescriptorPoolCreateInfo renderingPoolInfo = new Vulkan.DescriptorPoolCreateInfo {
                MaxSets = 1,
                PoolSizeCount = 1,
                PoolSizes = new Vulkan.DescriptorPoolSize[1] {
                    new Vulkan.DescriptorPoolSize {
                        Type=Vulkan.DescriptorType.UniformBuffer,
                        DescriptorCount=1
                    }
                }
            };
            _RenderingInfoPool = _Device.CreateDescriptorPool(renderingPoolInfo);

            Vulkan.DescriptorSetAllocateInfo setAllocInfo = new Vulkan.DescriptorSetAllocateInfo {
                DescriptorPool = _RenderingInfoPool,
                DescriptorSetCount = 1,
                SetLayouts = new Vulkan.DescriptorSetLayout[1] { _RenderingInfoSetLayout }
            };
            _RenderingInfoSets = _Device.AllocateDescriptorSets(setAllocInfo);

            //TEST_STUB
            BasicRenderingInfo testInfo = new BasicRenderingInfo();
            testInfo.Model = new mat4(1.0F);
            testInfo.View = glm.lookAt(
                new vec3(3, 10, 0),
                new vec3(0, 0, 0),
                new vec3(0, -1, 0)
                );
            testInfo.Projection = glm.perspective(glm.radians(60), 1, 0.1F, 100);
            testInfo.Clip = new mat4(
                new vec4(1, 0, 0, 0),
                new vec4(0, -1, 0, 0),
                new vec4(0, 0, 0.5f, 0),
                new vec4(0, 0, 0.5f, 1)
                );
            testInfo.GlobalLightDirection = new vec3(1, -0.5f, 0.5f);
            _RenderingInfoBuffer.Commit(0, testInfo);

            Vulkan.WriteDescriptorSet[] writes = new Vulkan.WriteDescriptorSet[1] {
                new Vulkan.WriteDescriptorSet {
                    DstSet=_RenderingInfoSets[0],
                    DescriptorType=Vulkan.DescriptorType.UniformBuffer,
                    BufferInfo=new Vulkan.DescriptorBufferInfo[1] {_RenderingInfoBuffer.BufferInfo[0]},
                    DstArrayElement=0,
                    DstBinding=0
                }
            };
            _Device.UpdateDescriptorSets(writes, null);
        }

        private void _InitRenderPass() {
            Vulkan.AttachmentDescription[] attachments = new Vulkan.AttachmentDescription[2] {
                new Vulkan.AttachmentDescription {
                    Format=_SwapchainImageFormat,
                    Samples=_SampleCount,
                    LoadOp=Vulkan.AttachmentLoadOp.Clear,
                    StoreOp=Vulkan.AttachmentStoreOp.Store,
                    StencilLoadOp=Vulkan.AttachmentLoadOp.DontCare,
                    StencilStoreOp=Vulkan.AttachmentStoreOp.DontCare,
                    InitialLayout=Vulkan.ImageLayout.ColorAttachmentOptimal,
                    FinalLayout=Vulkan.ImageLayout.PresentSrcKhr
                },
                new Vulkan.AttachmentDescription {
                    Format=_DepthImageFormat,
                    Samples=_SampleCount,
                    LoadOp=Vulkan.AttachmentLoadOp.Clear,
                    StoreOp=Vulkan.AttachmentStoreOp.DontCare,
                    StencilLoadOp=Vulkan.AttachmentLoadOp.DontCare,
                    StencilStoreOp=Vulkan.AttachmentStoreOp.DontCare,
                    InitialLayout=Vulkan.ImageLayout.DepthStencilAttachmentOptimal,
                    FinalLayout=Vulkan.ImageLayout.DepthStencilAttachmentOptimal
                }
            };

            Vulkan.AttachmentReference colorReference = new Vulkan.AttachmentReference {
                Attachment = 0,
                Layout = Vulkan.ImageLayout.ColorAttachmentOptimal
            };
            Vulkan.AttachmentReference depthReference = new Vulkan.AttachmentReference {
                Attachment = 1,
                Layout = Vulkan.ImageLayout.DepthStencilAttachmentOptimal
            };

            Vulkan.SubpassDescription subpass = new Vulkan.SubpassDescription {
                PipelineBindPoint = Vulkan.PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                ColorAttachments = new Vulkan.AttachmentReference[1] { colorReference },
                DepthStencilAttachment = depthReference
            };

            Vulkan.RenderPassCreateInfo renderPassInfo = new Vulkan.RenderPassCreateInfo {
                AttachmentCount = 2,
                Attachments = attachments,
                SubpassCount = 1,
                Subpasses = new Vulkan.SubpassDescription[1] { subpass }
            };
            _RenderPass = _Device.CreateRenderPass(renderPassInfo);
        }

        private void _InitShaders() {

        }

        private void _InitPipeline() {

        }

        private void _InitSemaphore() {
            Vulkan.SemaphoreCreateInfo info = new Vulkan.SemaphoreCreateInfo();
            _ImageAcquireSemaphore = _Device.CreateSemaphore(info);
        }

        private void _DrawFrame() {
            var currentImage = _Device.AcquireNextImageKHR(_Swapchain, ulong.MaxValue, _ImageAcquireSemaphore);
            _SwapchainImages[currentImage].TransitLayout(Vulkan.ImageAspectFlags.Color, Vulkan.ImageLayout.Undefined, Vulkan.ImageLayout.ColorAttachmentOptimal);

        }


    }

    static class VulkanImageLayoutTransitExtension {
        public static void TransitLayout(this Vulkan.Image image, Vulkan.ImageAspectFlags aspectMask, Vulkan.ImageLayout from, Vulkan.ImageLayout to) {

        }
    }
}
