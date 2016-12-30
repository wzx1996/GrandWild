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
        private Vulkan.PhysicalDevice _PhysicalDevice;
        private Vulkan.Device _Device;
        private uint _DeviceGraphicsQueueFamilyIndex = 0;
        private uint _DevicePresentQueueFamilyIndex = 0;
        private System.Windows.Forms.Form _DisplayForm;
        private Vulkan.SurfaceKhr _Surface;
        private Vulkan.SwapchainKhr _Swapchain;
        private Vulkan.Extent2D _SwapchainExtent;
        private Vulkan.PresentModeKhr _SwapchainPresentMode;
        private Vulkan.Image _SwapchainImages;
        private Vulkan.ImageView[] _SwapchainImagseView;
        private Vulkan.Image _DepthBufferImage;
        private Vulkan.ImageView _DepthBufferImageView;

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

            Vulkan.DeviceQueueCreateInfo queueInfo

        }
    }
}
