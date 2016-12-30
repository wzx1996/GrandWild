/**ShaderImporter.h
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

#pragma once
#include <SPIRV\GlslangToSpv.h>

using namespace Newtonsoft::Json;


namespace org::flamerat::GrandWild::ShaderImporter {
	enum class ShaderType {
		Undefined,
		Vertex,
		Fragment
	};
	public ref class Shader {
	public:
		System::String^ Name;
		array<System::Byte>^ Code;
		ShaderType Type=ShaderType::Undefined;
	};

	public ref class ShaderFile {
	public:
		System::String^ Name;
		System::String^ FileName;
		System::String^ Type; //"GLSL_Vertex", "GLSL_Fragment", "SPV_Vertex", "SPV_Fragment"
	};

	public ref class Shaders
	{
		property System::Collections::Generic::List<Shader^>^ ShaderList {
			System::Collections::Generic::List<Shader^>^ get() {
				return _Shaders;
			}
		}

		void Append(Shader^ shader);
		void ImportFromFiles(System::String^ ListFileName);

		static array<System::Byte>^ Glsl2Spv(ShaderType type, System::String^ GlslCode);

	private:

		System::Collections::Generic::List<Shader^>^ _Shaders;

		
	};

	//Copied from tutorial utility in Vulkan SDK
	void init_resources(TBuiltInResource &Resources);
}
