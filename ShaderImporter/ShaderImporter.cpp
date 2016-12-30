/**ShaderImporter.cpp
* org.flamerat.ShaderImporter
* A package for importing SPIR-V shaders into a list of shaders. Part of GrandWild engine project.
*
* compile option: /cli
*
* Copyright (C) 2016-2017 FlameRat
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*     http://www.apache.org/licenses/LICENSE-2.0

* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

/*
* Vulkan Samples
*
* Copyright (C) 2015-2016 Valve Corporation
* Copyright (C) 2015-2016 LunarG, Inc.
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
#include "stdafx.h"

#include "ShaderImporter.h"

void org::flamerat::GrandWild::ShaderImporter::Shaders::Append(Shader ^ shader)
{
	_Shaders->Add(shader);
}

void org::flamerat::GrandWild::ShaderImporter::Shaders::ImportFromFiles(System::String ^ ListFileName)
{
	System::String^ originalDir = System::IO::Directory::GetCurrentDirectory();
	JsonTextReader^ reader = gcnew JsonTextReader(gcnew System::IO::StreamReader(ListFileName));
	JsonSerializer^ serializer = gcnew JsonSerializer();

	System::IO::Directory::SetCurrentDirectory(gcnew System::IO::FileInfo(ListFileName)->DirectoryName);
	while (true) {
		if (!reader->Read()) break;
		ShaderFile^ currShaderFile=serializer->Deserialize<ShaderFile^>(reader);
		Shader^ currShader = gcnew Shader;
		currShader->Name = currShaderFile->Name;
		
		if (currShaderFile->Type == "GLSL_Vertex") {
			System::IO::StreamReader^ shaderReader = gcnew System::IO::StreamReader(currShaderFile->FileName);
			currShader->Type = ShaderType::Vertex;
			currShader->Code = Glsl2Spv(ShaderType::Vertex, shaderReader->ReadToEnd());
			shaderReader->Close();
		}
		else if (currShaderFile->Type == "GLSL_Fragment") {
			System::IO::StreamReader^ shaderReader = gcnew System::IO::StreamReader(currShaderFile->FileName);
			currShader->Type = ShaderType::Fragment;
			currShader->Code = Glsl2Spv(ShaderType::Fragment, shaderReader->ReadToEnd());
			shaderReader->Close();
		}
		else if (currShaderFile->Type == "SPV_Vertex") {
			currShader->Type = ShaderType::Vertex;
			System::IO::FileStream^ shaderReader = System::IO::File::OpenRead(currShaderFile->FileName);
			array<System::Byte>^ shaderCode = gcnew array<System::Byte>(shaderReader->Length);
			shaderReader->Read(shaderCode, 0, shaderReader->Length);
			shaderReader->Close();
			currShader->Code = shaderCode;
		}
		else if (currShaderFile->Type == "SPV_Fragment"){
			currShader->Type = ShaderType::Fragment;
			System::IO::FileStream^ shaderReader = System::IO::File::OpenRead(currShaderFile->FileName);
			array<System::Byte>^ shaderCode = gcnew array<System::Byte>(shaderReader->Length);
			shaderReader->Read(shaderCode, 0, shaderReader->Length);
			shaderReader->Close();
			currShader->Code = shaderCode;
		}
		else {
			System::Console::Error->WriteLine("Unsupported shader file type: {0} , it will be ignored.", currShaderFile->Type);
			continue;
		}
		_Shaders->Add(currShader);
	}
	System::IO::Directory::SetCurrentDirectory(originalDir);
	delete serializer;
}

array<System::Byte>^ org::flamerat::GrandWild::ShaderImporter::Shaders::Glsl2Spv(ShaderType type, System::String ^ GlslCode) {
	using namespace System::Runtime::InteropServices;
	EShLanguage shaderType;
	switch (type) {
	case ShaderType::Fragment:
		shaderType = EShLangFragment;
		break;
	case ShaderType::Vertex:
		shaderType = EShLangVertex;
		break;
	default:
		throw gcnew System::Exception(L"Unsupported shader type!");
	}

	std::vector<unsigned int> compiledCode;
	glslang::TProgram* sourceProgram = new glslang::TProgram();
	glslang::TShader* sourceShader = new glslang::TShader(shaderType);
	TBuiltInResource resource;
	init_resources(resource);

	if (!(sourceShader->parse(&resource, 130, false, (EShMessages)(EShMsgSpvRules | EShMsgVulkanRules))))
		throw gcnew System::Exception(L"Glsl2Spv error 1");

	//Convert source code string to null-terminated string
	System::IntPtr unmanagedSourceShader = Marshal::StringToHGlobalAnsi(GlslCode);
	const char *shaderStrings[1];
	shaderStrings[0] = new char[unmanagedSourceShader.Size + 1];
	memcpy((void*)shaderStrings[0], (const void*)unmanagedSourceShader.ToPointer(), unmanagedSourceShader.Size);

	sourceShader->setStrings(shaderStrings, 1);
	sourceProgram->addShader(sourceShader);
	if (!(sourceProgram->link((EShMessages)(EShMsgSpvRules | EShMsgVulkanRules))))
		throw gcnew System::Exception(L"Glsl2Spv error 2");

	//compile and convert result to managed array
	glslang::GlslangToSpv(*sourceProgram->getIntermediate(shaderType), compiledCode);
	array<System::Byte>^ result = gcnew array<System::Byte>(compiledCode.size()*sizeof(unsigned int));
	Marshal::Copy(System::IntPtr(compiledCode.data()), result, 0, result->Length);
	for (unsigned int i = 0;i < compiledCode.size();i++) result[i] = compiledCode[i];

	delete[] shaderStrings[0];
	delete sourceProgram;
	delete sourceShader;
	return result;
}

//Copied from tutorial utility in Vulkan SDK
void org::flamerat::GrandWild::ShaderImporter::init_resources(TBuiltInResource & Resources) {
	Resources.maxLights = 32;
	Resources.maxClipPlanes = 6;
	Resources.maxTextureUnits = 32;
	Resources.maxTextureCoords = 32;
	Resources.maxVertexAttribs = 64;
	Resources.maxVertexUniformComponents = 4096;
	Resources.maxVaryingFloats = 64;
	Resources.maxVertexTextureImageUnits = 32;
	Resources.maxCombinedTextureImageUnits = 80;
	Resources.maxTextureImageUnits = 32;
	Resources.maxFragmentUniformComponents = 4096;
	Resources.maxDrawBuffers = 32;
	Resources.maxVertexUniformVectors = 128;
	Resources.maxVaryingVectors = 8;
	Resources.maxFragmentUniformVectors = 16;
	Resources.maxVertexOutputVectors = 16;
	Resources.maxFragmentInputVectors = 15;
	Resources.minProgramTexelOffset = -8;
	Resources.maxProgramTexelOffset = 7;
	Resources.maxClipDistances = 8;
	Resources.maxComputeWorkGroupCountX = 65535;
	Resources.maxComputeWorkGroupCountY = 65535;
	Resources.maxComputeWorkGroupCountZ = 65535;
	Resources.maxComputeWorkGroupSizeX = 1024;
	Resources.maxComputeWorkGroupSizeY = 1024;
	Resources.maxComputeWorkGroupSizeZ = 64;
	Resources.maxComputeUniformComponents = 1024;
	Resources.maxComputeTextureImageUnits = 16;
	Resources.maxComputeImageUniforms = 8;
	Resources.maxComputeAtomicCounters = 8;
	Resources.maxComputeAtomicCounterBuffers = 1;
	Resources.maxVaryingComponents = 60;
	Resources.maxVertexOutputComponents = 64;
	Resources.maxGeometryInputComponents = 64;
	Resources.maxGeometryOutputComponents = 128;
	Resources.maxFragmentInputComponents = 128;
	Resources.maxImageUnits = 8;
	Resources.maxCombinedImageUnitsAndFragmentOutputs = 8;
	Resources.maxCombinedShaderOutputResources = 8;
	Resources.maxImageSamples = 0;
	Resources.maxVertexImageUniforms = 0;
	Resources.maxTessControlImageUniforms = 0;
	Resources.maxTessEvaluationImageUniforms = 0;
	Resources.maxGeometryImageUniforms = 0;
	Resources.maxFragmentImageUniforms = 8;
	Resources.maxCombinedImageUniforms = 8;
	Resources.maxGeometryTextureImageUnits = 16;
	Resources.maxGeometryOutputVertices = 256;
	Resources.maxGeometryTotalOutputComponents = 1024;
	Resources.maxGeometryUniformComponents = 1024;
	Resources.maxGeometryVaryingComponents = 64;
	Resources.maxTessControlInputComponents = 128;
	Resources.maxTessControlOutputComponents = 128;
	Resources.maxTessControlTextureImageUnits = 16;
	Resources.maxTessControlUniformComponents = 1024;
	Resources.maxTessControlTotalOutputComponents = 4096;
	Resources.maxTessEvaluationInputComponents = 128;
	Resources.maxTessEvaluationOutputComponents = 128;
	Resources.maxTessEvaluationTextureImageUnits = 16;
	Resources.maxTessEvaluationUniformComponents = 1024;
	Resources.maxTessPatchComponents = 120;
	Resources.maxPatchVertices = 32;
	Resources.maxTessGenLevel = 64;
	Resources.maxViewports = 16;
	Resources.maxVertexAtomicCounters = 0;
	Resources.maxTessControlAtomicCounters = 0;
	Resources.maxTessEvaluationAtomicCounters = 0;
	Resources.maxGeometryAtomicCounters = 0;
	Resources.maxFragmentAtomicCounters = 8;
	Resources.maxCombinedAtomicCounters = 8;
	Resources.maxAtomicCounterBindings = 1;
	Resources.maxVertexAtomicCounterBuffers = 0;
	Resources.maxTessControlAtomicCounterBuffers = 0;
	Resources.maxTessEvaluationAtomicCounterBuffers = 0;
	Resources.maxGeometryAtomicCounterBuffers = 0;
	Resources.maxFragmentAtomicCounterBuffers = 1;
	Resources.maxCombinedAtomicCounterBuffers = 1;
	Resources.maxAtomicCounterBufferSize = 16384;
	Resources.maxTransformFeedbackBuffers = 4;
	Resources.maxTransformFeedbackInterleavedComponents = 64;
	Resources.maxCullDistances = 8;
	Resources.maxCombinedClipAndCullDistances = 8;
	Resources.maxSamples = 4;
	Resources.limits.nonInductiveForLoops = 1;
	Resources.limits.whileLoops = 1;
	Resources.limits.doWhileLoops = 1;
	Resources.limits.generalUniformIndexing = 1;
	Resources.limits.generalAttributeMatrixVectorIndexing = 1;
	Resources.limits.generalVaryingIndexing = 1;
	Resources.limits.generalSamplerIndexing = 1;
	Resources.limits.generalVariableIndexing = 1;
	Resources.limits.generalConstantMatrixVectorIndexing = 1;
}
