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
                    glm.translate(result, new vec3(-Origin.x,-Origin.y,-Origin.z));
                    glm.scale(result, Scale);
                    glm.rotate(result, glm.radians(XRotation), xAxis);
                    glm.rotate(result, glm.radians(YRotation), yAxis);
                    glm.rotate(result, glm.radians(ZRotation), zAxis);
                    glm.translate(result, Position);
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
