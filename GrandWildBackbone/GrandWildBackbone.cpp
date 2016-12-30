/**GrandWildBackbone.cpp
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

#include "GrandWildBackbone.h"

namespace org::flamerat::grandwild {

	GrandWildBackbone::GrandWildBackbone()
	{
		if (SDL_Init(SDL_INIT_EVERYTHING) != 0) {
			std::cout << "Could not initialize SDL." << std::endl;
			exit(1);
		}
	}

	GrandWildBackbone::!GrandWildBackbone()
	{
		SDL_Quit();
		if (_SDLWindow) {
			delete _SDLWindow;
		}
		if (_VulkanSurface) {
			_VulkanInstance->destroySurfaceKHR(*_VulkanSurface);
			delete _VulkanSurface;
		}
		if (_VulkanInstance) {
			_VulkanInstance->destroy();
			delete _VulkanInstance;
		}
	}

	void GrandWildBackbone::Launch()
	{

		_InitVulkanInstance();
		_CreateWindow();
		_InitVulkanDevice();

		_RenderingThread = gcnew System::Threading::Thread(gcnew System::Threading::ThreadStart(&this->_RenderingThreadLogic));
		_RenderingThread->Start();

		// Poll for user input.
		bool stillRunning = true;
		while (stillRunning) {

			SDL_Event event;
			while (SDL_PollEvent(&event)) {

				switch (event.type) {

					//TODO for each event, execute the corresponding event callback list

				case SDL_QUIT:
					stillRunning = false;
					break;

				default:
					// Do nothing.
					break;
				}
			}

			SDL_Delay(10);
		}
	}

	void GrandWildBackbone::_InitVulkanInstance()
	{
		if (_AppName == nullptr) _AppName = "";
		vk::ApplicationInfo AppInfo = vk::ApplicationInfo()
			.setPApplicationName(_AppName)
			.setApplicationVersion(_AppVersion)
			.setPEngineName("GrandWild")
			.setEngineVersion(1)
			.setApiVersion(VK_API_VERSION_1_0);


		// Use validation layers if this is a debug build, and use WSI extensions regardless
		std::vector<const char*> extensions = getAvailableWSIExtensions();
		std::vector<const char*> layers;
#if defined(_DEBUG)
		layers.push_back("VK_LAYER_LUNARG_standard_validation");
#endif

		vk::InstanceCreateInfo InstanceInfo = vk::InstanceCreateInfo()
			.setFlags(vk::InstanceCreateFlags())
			.setPApplicationInfo(&AppInfo)
			.setEnabledExtensionCount(static_cast<uint32_t>(extensions.size()))
			.setPpEnabledExtensionNames(extensions.data())
			.setEnabledLayerCount(static_cast<uint32_t>(layers.size()))
			.setPpEnabledLayerNames(layers.data());

		try {
			*_VulkanInstance = vk::createInstance(InstanceInfo);
		}
		catch (const std::exception& e) {
			std::cout << "Could not create a Vulkan instance: " << e.what() << std::endl;
			exit(1);
		}



	}

	void GrandWildBackbone::_CreateWindow()
	{

		_SDLWindow = SDL_CreateWindow("Vulkan Window", SDL_WINDOWPOS_CENTERED,
			SDL_WINDOWPOS_CENTERED, _WindowWidth, _WindowHeight, SDL_WINDOW_OPENGL);
		if (_SDLWindow == nullptr) {
			std::cout << "Could not create SDL window." << std::endl;
			exit(1);
		}

		// Create a Vulkan surface for rendering
		try {
			*_VulkanSurface = createVulkanSurface(*_VulkanInstance, _SDLWindow);

		}
		catch (const std::exception& e) {
			std::cout << "Failed to create Vulkan surface: " << e.what() << std::endl;
			_VulkanInstance->destroy();
			exit(1);
		}
	}


	void GrandWildBackbone::_InitVulkanDevice()
	{
		std::vector<vk::PhysicalDevice> gpus = _VulkanInstance->enumeratePhysicalDevices();

		/////////////////Check capability////////////
		std::vector<vk::QueueFamilyProperties> queueFamilyProperty = gpus[0].getQueueFamilyProperties();
		vk::DeviceQueueCreateInfo queueCreateInfo = vk::DeviceQueueCreateInfo();

		bool foundGraphicsBit = false;
		bool foundPresentSupport = false;
		for (unsigned int i = 0;i < queueFamilyProperty.size;i++) {
			if ((!foundGraphicsBit) && (queueFamilyProperty[i].queueFlags&vk::QueueFlagBits::eGraphics)) {
				foundGraphicsBit = true;
				_DeviceGraphicsQueueFamilyIndex = i;
				queueCreateInfo.queueFamilyIndex = i;
			}
			if ((!foundPresentSupport) && (gpus[0].getSurfaceSupportKHR(i, *_VulkanSurface))) {
				foundPresentSupport = true;
				_DevicePresentQueueFamilyIndex = i;
			}
		}
		queueCreateInfo.queueCount = 1;
		float* queuePriorities = new float[1];
		queuePriorities[0] = 0.0;
		queueCreateInfo.pQueuePriorities = queuePriorities;
		if (!foundGraphicsBit) throw gcnew System::Exception(L"No available graphics queue for the first GPU!");
		if (!foundPresentSupport) throw gcnew System::Exception(L"No present supported queue for the first GPU!");

		std::vector<vk::SurfaceFormatKHR> supportedSurfaceFormat = gpus[0].getSurfaceFormatsKHR(*_VulkanSurface);
		if (supportedSurfaceFormat[0] != vk::Format::eUndefined) _SurfaceFormat->setFormat(supportedSurfaceFormat[0].format);

		vk::SurfaceCapabilitiesKHR surfaceCapabilities = gpus[0].getSurfaceCapabilitiesKHR(*_VulkanSurface);
		_SwapchainImageCount = surfaceCapabilities.minImageCount;
		if (surfaceCapabilities.currentExtent.width == 0xFFFFFFFF) {
			_SwapchainExtent->width = _WindowWidth;
			_SwapchainExtent->height = _WindowHeight;

			if (_SwapchainExtent->width < surfaceCapabilities.minImageExtent.width) _SwapchainExtent->width = surfaceCapabilities.minImageExtent.width;
			else if (_SwapchainExtent->width > surfaceCapabilities.maxImageExtent.width) _SwapchainExtent->width = surfaceCapabilities.maxImageExtent.width;

			if (_SwapchainExtent->height < surfaceCapabilities.minImageExtent.height) _SwapchainExtent->height = surfaceCapabilities.minImageExtent.height;
			else if (_SwapchainExtent->height > surfaceCapabilities.maxImageExtent.height) _SwapchainExtent->height = surfaceCapabilities.maxImageExtent.height;
		}
		else {
			*_SwapchainExtent = surfaceCapabilities.currentExtent;
		}

		if (!(surfaceCapabilities.supportedTransforms & vk::SurfaceTransformFlagBitsKHR::eIdentity)) _SwapchainTransformMode = surfaceCapabilities.currentTransform;

		std::vector<vk::PresentModeKHR> availablePresentModes = gpus[0].getSurfacePresentModesKHR(*_VulkanSurface);
		for (int i = 0;i < availablePresentModes.size;i++) {
			if (availablePresentModes[i] == vk::PresentModeKHR::eMailbox) {
				*_SwapchainPresentMode = vk::PresentModeKHR::eMailbox;
				break;
			}
		}


		////////////////Init//////////////
		vk::DeviceCreateInfo deviceCreateInfo = vk::DeviceCreateInfo();
		deviceCreateInfo.queueCreateInfoCount = 1;
		deviceCreateInfo.pQueueCreateInfos = &queueCreateInfo;
		deviceCreateInfo.enabledExtensionCount = 0;
		deviceCreateInfo.enabledLayerCount = 0;
		*_VulkanDevice = gpus[0].createDevice(deviceCreateInfo);

		vk::CommandPoolCreateInfo commandPoolInfo = vk::CommandPoolCreateInfo()
			.setQueueFamilyIndex(_DeviceGraphicsQueueFamilyIndex);

		*_VulkanCommandPool = _VulkanDevice->createCommandPool(commandPoolInfo);

		vk::CommandBufferAllocateInfo commandBufferAllocInfo = vk::CommandBufferAllocateInfo()
			.setCommandPool(*_VulkanCommandPool)
			.setLevel(vk::CommandBufferLevel::ePrimary)
			.setCommandBufferCount(1);
		_VulkanDevice->allocateCommandBuffers(commandBufferAllocInfo);

	}

	void GrandWildBackbone::_InitSwapChain()
	{
		vk::SwapchainCreateInfoKHR swapchainInfo = vk::SwapchainCreateInfoKHR()
			.setSurface(*_VulkanSurface)
			.setMinImageCount(_SwapchainImageCount)
			.setImageFormat(_SurfaceFormat->format)
			.setImageExtent(*_SwapchainExtent)
			.setPreTransform(_SwapchainTransformMode)
			.setCompositeAlpha(vk::CompositeAlphaFlagBitsKHR::eOpaque)
			.setImageArrayLayers(1)
			.setPresentMode(*_SwapchainPresentMode)
			.setClipped(true)
			.setImageColorSpace(vk::ColorSpaceKHR::eSrgbNonlinear)
			.setImageUsage(vk::ImageUsageFlagBits::eColorAttachment)
			.setImageSharingMode(vk::SharingMode::eExclusive)
			.setQueueFamilyIndexCount(0);
		if (_DeviceGraphicsQueueFamilyIndex != _DevicePresentQueueFamilyIndex) {
			unsigned int* queueFamilyIndices = new unsigned int[2];
			queueFamilyIndices[0] = _DeviceGraphicsQueueFamilyIndex;
			queueFamilyIndices[1] = _DevicePresentQueueFamilyIndex;

			swapchainInfo.imageSharingMode = vk::SharingMode::eConcurrent;
			swapchainInfo.queueFamilyIndexCount = 2;
			swapchainInfo.pQueueFamilyIndices = queueFamilyIndices;
		}

		*_VulkanSwapchain = _VulkanDevice->createSwapchainKHR(swapchainInfo);

		std::vector<vk::Image> swapchainImages = _VulkanDevice->getSwapchainImagesKHR(*_VulkanSwapchain);
		_SwapchainImageCount = swapchainImages.size;
		_VulkanImageBuffers->resize(_SwapchainImageCount);
		_VulkanViewBuffers->resize(_SwapchainImageCount);
		for (unsigned int i = 0;i < swapchainImages.size;i++) {
			vk::ImageViewCreateInfo viewInfo = vk::ImageViewCreateInfo()
				.setImage((*_VulkanImageBuffers)[i])
				.setViewType(vk::ImageViewType::e2D)
				.setFormat(_SurfaceFormat->format)
				.setComponents(vk::ComponentMapping(vk::ComponentSwizzle::eR,
					vk::ComponentSwizzle::eG,
					vk::ComponentSwizzle::eB,
					vk::ComponentSwizzle::eA
				))
				.setSubresourceRange(vk::ImageSubresourceRange(vk::ImageAspectFlagBits::eColor,
					0,
					1,
					0,
					1
				));
			(*_VulkanViewBuffers)[i] = _VulkanDevice->createImageView(viewInfo);

		}

	}

	void GrandWildBackbone::_InitDepthBuffer()
	{
		vk::ImageCreateInfo depthImageInfo = vk::ImageCreateInfo()
			.setImageType(vk::ImageType::e2D)
			.setFormat(vk::Format::eD16Unorm)
			.setExtent(vk::Extent3D(_WindowWidth, _WindowHeight, 1))
			.setMipLevels(1)
			.setArrayLayers(1)
			.setSamples(eNumSamples)
			.setInitialLayout(vk::ImageLayout::eUndefined)
			.setUsage(vk::ImageUsageFlagBits::eDepthStencilAttachment)
			.setQueueFamilyIndexCount(0)
			.setPQueueFamilyIndices(nullptr)
			.setSharingMode(vk::SharingMode::eExclusive);
		*_VulkanDepthBuffer = _VulkanDevice->createImage(depthImageInfo);

		vk::MemoryRequirements memoryRequirements = _VulkanDevice->getImageMemoryRequirements(*_VulkanDepthBuffer);
		vk::MemoryAllocateInfo depthImageMemAllocInfo = vk::MemoryAllocateInfo()
			.setAllocationSize(memoryRequirements.size)
			.setMemoryTypeIndex(0);

		*_VulkanDepthBufferMemory=_VulkanDevice->allocateMemory(depthImageMemAllocInfo);
		_VulkanDevice->bindImageMemory(*_VulkanDepthBuffer,*_VulkanDepthBufferMemory,0);

		vk::ImageViewCreateInfo depthBufferViewInfo = vk::ImageViewCreateInfo()
			.setImage(*_VulkanDepthBuffer)
			.setFormat(vk::Format::eD16Unorm)
			.setComponents(vk::ComponentMapping(
				vk::ComponentSwizzle::eR,
				vk::ComponentSwizzle::eG,
				vk::ComponentSwizzle::eB,
				vk::ComponentSwizzle::eA
			))
			.setSubresourceRange(vk::ImageSubresourceRange(
				vk::ImageAspectFlagBits::eDepth, 
				0, 
				1, 
				0, 
				1
			))
			.setViewType(vk::ImageViewType::e2D);
		*_VulkanDepthBufferView = _VulkanDevice->createImageView(depthBufferViewInfo);
	}

	void GrandWildBackbone::_DefineVulkanPipeline()
	{

	}

}