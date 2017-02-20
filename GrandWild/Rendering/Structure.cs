using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;
using Vulkan;

namespace org.flamerat.GrandWild.Rendering {
    public class Structure:IPart {

        public virtual void Draw(CommandBuffer commandBuffer) {
            Draw(commandBuffer, new mat4(1.0f));
        }

        public void Draw(CommandBuffer commandBuffer, mat4 parentModelMatrix) {
            foreach (var subpart in SubParts) if(subpart.Visible) commandBuffer.CmdDrawPartGw(subpart.Part, parentModelMatrix * subpart.ModelMatrix);
        }

        public IEnumerable<IGpuBuffer> GetIndexBuffers() {
            return SubParts.SelectMany(subpart => subpart.Part.GetIndexBuffers()).Distinct();
        }

        public IEnumerable<IGpuImage> GetTextureImages() {
            return SubParts.SelectMany(subpart => subpart.Part.GetTextureImages()).Distinct();
        }

        public IEnumerable<IGpuBuffer> GetVertexBuffers() {
            return SubParts.SelectMany(subpart => subpart.Part.GetVertexBuffers()).Distinct();
        }

        public struct SubPart {
            public IPart Part;
            public bool Visible;
            public vec3 Position; //<Object origin position in parent coordinate
            public vec3 Origin; //<Object origin position in obejct coordinate
            public vec3 Scale;
            public float XRotation;
            public float YRotation;
            public float ZRotation;
            public mat4 ModelMatrix {
                get {
                    mat4 result=new mat4(1.0F);
                    result=glm.translate(mat4.identity(), new vec3(-Origin.x,-Origin.y,-Origin.z))*result;
                    result=glm.scale(mat4.identity(),Scale) * result;
                    result=glm.rotate(glm.radians(XRotation), xAxis) * result;
                    result=glm.rotate(glm.radians(YRotation), yAxis) * result;
                    result=glm.rotate(glm.radians(ZRotation), zAxis) * result;
                    result=glm.translate(mat4.identity(), Position) * result;
                    return result;
                }
            }
            private static readonly vec3 xAxis = new vec3(1, 0, 0);
            private static readonly vec3 yAxis = new vec3(0, 1, 0);
            private static readonly vec3 zAxis = new vec3(0, 0, 1);
        }

        public SubPart[] SubParts;

    }
}
