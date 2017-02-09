///<summary>
///A class for loading a SPIR-V shader as a module and define a shader stage of that module.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Vulkan;

namespace org.flamerat.GrandWild {

    public class ShaderStage {
        public PipelineShaderStageCreateInfo Info { get; protected set; }


        public static implicit operator PipelineShaderStageCreateInfo(ShaderStage stage) { return stage.Info; }


        public ShaderStage(Device device, byte[] code, string name, ShaderStageFlags stage,SpecializationInfo specializationInfo=null) {
            CreateShaderStage(device, code, name, stage, specializationInfo);
        }


        /// <summary>
        /// Load a shader module from file and define a shader stage.
        /// 
        /// Auto fill "name" and "stage" if the shader file is named in the format of "[name].[stage].spv"
        /// where [stage] is one of the following: vert, tesc, tese, geom, frag, comp
        /// (please refer to shader stage specification to get the exact meaning).
        /// </summary>
        /// <param name="device"></param>
        /// <param name="fileName"></param>
        /// <param name="name"></param>
        /// <param name="stage"></param>
        /// <param name="specializationInfo"></param>
        public ShaderStage(Device device, string fileName, string name="", ShaderStageFlags stage=0, SpecializationInfo specializationInfo=null) {
            string pureFileName = Path.GetFileName(fileName);
            if (Path.GetExtension(pureFileName) == "spv") pureFileName = Path.GetFileNameWithoutExtension(fileName);
            if (stage != 0) {
                bool fileNameSpecifiedStage = true;
                switch (Path.GetExtension(pureFileName)) {
                    case "vert":
                        stage = ShaderStageFlags.Vertex;
                        break;
                    case "tesc":
                        stage = ShaderStageFlags.TessellationControl;
                        break;
                    case "tese":
                        stage = ShaderStageFlags.TessellationEvaluation;
                        break;
                    case "geom":
                        stage = ShaderStageFlags.Geometry;
                        break;
                    case "frag":
                        stage = ShaderStageFlags.Fragment;
                        break;
                    case "comp":
                        stage = ShaderStageFlags.Compute;
                        break;
                    default:
                        fileNameSpecifiedStage = false;
                        break;
                }
                if (fileNameSpecifiedStage) pureFileName = Path.GetFileNameWithoutExtension(pureFileName);
            }
            if (stage == 0) throw new Exception(fileName + ": Shader stage not specified.");
            if (name == "") name = pureFileName;

            byte[] code;
            code = File.ReadAllBytes(fileName);

            CreateShaderStage(device, code, name, stage, specializationInfo);
        }


        private void CreateShaderStage(Device device, byte[] code, string name, ShaderStageFlags stage, SpecializationInfo specializationInfo) {
            ShaderModuleCreateInfo moduleInfo = new ShaderModuleCreateInfo {
                CodeBytes = code,
            };
            ShaderModule shaderModule = device.CreateShaderModule(moduleInfo);
            Info = new PipelineShaderStageCreateInfo {
                Name = name,
                Stage = stage,
                Module = shaderModule,
                SpecializationInfo = specializationInfo
            };
        }
    }
}
