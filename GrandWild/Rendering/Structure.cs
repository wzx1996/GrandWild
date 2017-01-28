using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;
using Vulkan;

namespace org.flamerat.GrandWild.Rendering {
    class Structure:IPart {
        public void DisposeRamData() {
            throw new NotImplementedException();
        }

        public virtual void Draw(CommandBuffer commandBuffer) {
            throw new NotImplementedException();
        }

        public void Draw(CommandBuffer commandBuffer, mat4 parentModelMatrix) {
            throw new NotImplementedException();
        }

        public void SendToGpu(Device device, DeviceMemory memory, DeviceSize offset) {
            throw new NotImplementedException();
        }

        public struct SubPart {
            public IPart Part;
            public vec3 Position; //<Object origin position in parent coordinate
            public vec3 Origin; //<Object origin position in obejct coordinate
            public vec3 Scale;
            public float XRotation;
            public float YRotation;
            public float ZRotation;
            public mat4 ModelMatrix {
                get {
                    mat4 result;
                    glm.translate(result, Origin);
                    glm.scale(result, Scale);
                    glm.rotate(result, XRotation, xAxis);
                    glm.rotate(result, YRotation, yAxis);
                    glm.rotate(result, ZRotation, zAxis);
                    glm.translate(result, Position);
                    return result;
                }
            }
            private static readonly vec3 xAxis = new vec3(1, 0, 0);
            private static readonly vec3 yAxis = new vec3(0, 1, 0);
            private static readonly vec3 zAxis = new vec3(0, 0, 1);
        }
        

    }
}
