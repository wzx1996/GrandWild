/**GrandWildBackbone.h
 * GrandWild Backbone 0.1.1
 *
 * This is a wrapper class for wrapping native APIs to be used by the GrandWild engine.
 *
 * The current version mainly wraps Vulkan API and SDL2 as native graphical APIs. 
 * 
 * Built on Vulkan SDK 1.0.26.0.
 *
 * Copyright (C) FlameRat 2016-2017
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

 /*
 * Vulkan Windowed Program
 *
 * Copyright (C) 2016 Valve Corporation
 * Copyright (C) 2016 LunarG, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#pragma once

#include <SDL2/SDL.h>
#include <SDL2/SDL_syswm.h>
#include <vulkan/vulkan.hpp>
#include <GLFW\glfw3.h>

#include <iostream>
#include <vector>

 // Enable the WSI extensions
#if defined(__ANDROID__)
#define VK_USE_PLATFORM_ANDROID_KHR
#elif defined(__linux__)
#define VK_USE_PLATFORM_XLIB_KHR
#elif defined(_WIN32)
#define VK_USE_PLATFORM_WIN32_KHR
#endif

 // Tell SDL not to mess with main()
#define SDL_MAIN_HANDLED

namespace org::flamerat::grandwild{
ref class GrandWildBackbone
{
public:
	GrandWildBackbone();
	!GrandWildBackbone();
	void Launch(); ///After finishing the initialization, run this to start the loop. 
	               ///This funcion won't return until the program ends, thus if necessary, it'll be a good idea to fork a new thread beforehand

	property int ScreenWidth { //TODO add logic: if not launched, change the member value; otherwise attempt to change the window size.
		int get();
		void set(int value);
	}

	property int ScreenHeight {
		int get();
		void set(int value);
	}

	/////////////////Event List////////////////////////////
	//The main loop that is responsible of pulling SDL events would call these events

	delegate void MouseButtonEventHandler(int x, int y, int button);
	event MouseButtonEventHandler^ MouseDown;
	event MouseButtonEventHandler^ MouseUp;
	event MouseButtonEventHandler^ MousePress;

	delegate void MouseEventHandler(int x, int y);
	event MouseEventHandler^ MouseMove;

	enum class ScrollDirection {
		ScrollDown,
		ScrollUp
	};
	delegate void ScrollEventHandler(int x, int y, ScrollDirection direction);
	event ScrollEventHandler^ Scroll;

	[Flags] enum class KeyboardKeyModifier {
		Control = 1,
		Alt = 2,
		Shift = 4
	};
	delegate void KeyboardEventHandler(int key, KeyboardKeyModifier modifier);
	event KeyboardEventHandler^ KeyDown;
	event KeyboardEventHandler^ KeyUp;
	event KeyboardEventHandler^ KeyPress;

	delegate void JoystickButtonHandler(int joystick, int button);
	event JoystickButtonHandler^ JoyButtonDown;
	event JoystickButtonHandler^ JoyButtonUp;
	event JoystickButtonHandler^ JoyButtonPress;

	delegate void JoystickAxisHandler(int joystick, int axis, double position);
	event JoystickAxisHandler^ JoyAxisMove;

	delegate void TextInputHandler(System::Char character);
	event TextInputHandler^ CharacterInput;

	delegate void TimeEventHandler(double time);
	event TimeEventHandler^ EventTick; //SDL event loop successfully processed (called back) an event
	event TimeEventHandler^ IntervalPassed; //Usually would be considered, for example, in-game tick
	event TimeEventHandler^ FrameBufferSwapped; //Each time Vulkan swap the frame buffer this event will be called

	delegate void ExitEventHandler();
	event ExitEventHandler^ ProgramExit;

private:
	const vk::SampleCountFlagBits eNumSamples = vk::SampleCountFlagBits::e1;

	vk::Instance* _VulkanInstance=new vk::Instance;
	vk::SurfaceKHR* _VulkanSurface=new vk::SurfaceKHR;
	vk::Device* _VulkanDevice = new vk::Device;
	unsigned int _DeviceGraphicsQueueFamilyIndex = 0;
	unsigned int _DevicePresentQueueFamilyIndex = 0;
	unsigned int _SwapchainImageCount = 1;
	vk::SurfaceTransformFlagBitsKHR _SwapchainTransformMode = vk::SurfaceTransformFlagBitsKHR::eIdentity;
	vk::SurfaceFormatKHR* _SurfaceFormat = new vk::SurfaceFormatKHR(vk::Format::eB8G8R8A8Snorm);
	vk::Extent2D* _SwapchainExtent = new vk::Extent2D;
	vk::PresentModeKHR* _SwapchainPresentMode = new vk::PresentModeKHR(vk::PresentModeKHR::eFifo);
	vk::CommandPool* _VulkanCommandPool = new vk::CommandPool;
	vk::SwapchainKHR* _VulkanSwapchain = new vk::SwapchainKHR;
	std::vector<vk::Image> *_VulkanImageBuffers = new std::vector<vk::Image>;
	std::vector<vk::ImageView> *_VulkanViewBuffers = new std::vector<vk::ImageView>;
	vk::Image* _VulkanDepthBuffer = new vk::Image;
	vk::DeviceMemory* _VulkanDepthBufferMemory = new vk::DeviceMemory;
	vk::ImageView* _VulkanDepthBufferView = new vk::ImageView;
	vk::Buffer* _VulkanUniformBuffer = new vk::Buffer;
	vk::PipelineLayout* _VulkanPipelineLayout = new vk::PipelineLayout;
	std::vector<vk::ShaderModule>* _ShaderModules = new std::vector<vk::ShaderModule>;

	SDL_Window* _SDLWindow=nullptr;

	bool _IsLaunched = false;

	int _WindowWidth = 640;
	int _WindowHeight = 480;

	char* _AppName = nullptr;
	int _AppVersion = 1;

	void _InitVulkanInstance();
	void _CreateWindow();
	void _InitVulkanDevice();
	void _InitSwapChain();
	void _InitDepthBuffer();
	void _InitUnifiedBuffer();
	void _InitShaders();
	void _DefineVulkanPipeline();
	void _InitVulkanPipeline();

	/**Vulkan rendering thread
	 * This function contains the rendering logic as a thread separated from the main one.
	 * The reason for having a separated thread is so that the app don't have to run at 
	 */
	System::Threading::Thread^ _RenderingThread;
	void _RenderingThreadLogic();

	/////////////////////////// Code From Vulkan Templete /////////////////////////

	vk::SurfaceKHR createVulkanSurface(const vk::Instance& instance, SDL_Window* window)
	{
		SDL_SysWMinfo windowInfo;
		SDL_VERSION(&windowInfo.version);
		if (!SDL_GetWindowWMInfo(window, &windowInfo)) {
			throw std::system_error(std::error_code(), "SDK window manager info is not available.");
		}

		switch (windowInfo.subsystem) {

#if defined(SDL_VIDEO_DRIVER_ANDROID) && defined(VK_USE_PLATFORM_ANDROID_KHR)
		case SDL_SYSWM_ANDROID: {
			vk::AndroidSurfaceCreateInfoKHR surfaceInfo = vk::AndroidSurfaceCreateInfoKHR()
				.setWindow(windowInfo.info.android.window);
			return instance.createAndroidSurfaceKHR(surfaceInfo);
		}
#endif

#if defined(SDL_VIDEO_DRIVER_MIR) && defined(VK_USE_PLATFORM_MIR_KHR)
		case SDL_SYSWM_MIR: {
			vk::MirSurfaceCreateInfoKHR surfaceInfo = vk::MirSurfaceCreateInfoKHR()
				.setConnection(windowInfo.info.mir.connection)
				.setMirSurface(windowInfo.info.mir.surface);
			return instance.createMirSurfaceKHR(surfaceInfo);
		}
#endif

#if defined(SDL_VIDEO_DRIVER_WAYLAND) && defined(VK_USE_PLATFORM_WAYLAND_KHR)
		case SDL_SYSWM_WAYLAND: {
			vk::WaylandSurfaceCreateInfoKHR surfaceInfo = vk::WaylandSurfaceCreateInfoKHR()
				.setDisplay(windowInfo.info.wl.display)
				.setSurface(windowInfo.info.wl.surface);
			return instance.createWaylandSurfaceKHR(surfaceInfo);
		}
#endif

#if defined(SDL_VIDEO_DRIVER_WINDOWS) && defined(VK_USE_PLATFORM_WIN32_KHR)
		case SDL_SYSWM_WINDOWS: {
			vk::Win32SurfaceCreateInfoKHR surfaceInfo = vk::Win32SurfaceCreateInfoKHR()
				.setHinstance(GetModuleHandle(NULL))
				.setHwnd(windowInfo.info.win.window);
			return instance.createWin32SurfaceKHR(surfaceInfo);
		}
#endif

#if defined(SDL_VIDEO_DRIVER_X11) && defined(VK_USE_PLATFORM_XLIB_KHR)
		case SDL_SYSWM_X11: {
			vk::XlibSurfaceCreateInfoKHR surfaceInfo = vk::XlibSurfaceCreateInfoKHR()
				.setDpy(windowInfo.info.x11.display)
				.setWindow(windowInfo.info.x11.window);
			return instance.createXlibSurfaceKHR(surfaceInfo);
		}
#endif

		default:
			throw std::system_error(std::error_code(), "Unsupported window manager is in use.");
		}
	}

	std::vector<const char*> getAvailableWSIExtensions()
	{
		std::vector<const char*> extensions;
		extensions.push_back(VK_KHR_SURFACE_EXTENSION_NAME);

#if defined(VK_USE_PLATFORM_ANDROID_KHR)
		extensions.push_back(VK_KHR_ANDROID_SURFACE_EXTENSION_NAME);
#elif defined(VK_USE_PLATFORM_MIR_KHR)
		extensions.push_back(VK_KHR_MIR_SURFACE_EXTENSION_NAME);
#elif defined(VK_USE_PLATFORM_WAYLAND_KHR)
		extensions.push_back(VK_KHR_WAYLAND_SURFACE_EXTENSION_NAME);
#elif defined(VK_USE_PLATFORM_WIN32_KHR)
		extensions.push_back(VK_KHR_WIN32_SURFACE_EXTENSION_NAME);
#elif defined(VK_USE_PLATFORM_XLIB_KHR)
		extensions.push_back(VK_KHR_XLIB_SURFACE_EXTENSION_NAME);
#endif

		return extensions;
	}
};


}