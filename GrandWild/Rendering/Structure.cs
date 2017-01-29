using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;
using Vulkan;

namespace org.flamerat.GrandWild.Rendering {
    class Structure:IPart {

        public virtual void Draw(CommandBuffer commandBuffer) {
            Draw(commandBuffer, new mat4(1.0f));
        }

        public void Draw(CommandBuffer commandBuffer, mat4 parentModelMatrix) {
            foreach (var subpart in _SubParts) commandBuffer.CmdDrawPartGw(subpart.Part, parentModelMatrix * subpart.ModelMatrix);
        }

        public IEnumerable<IGpuBuffer> GetIndexBuffers() {
            var bufferArrays = from subpart in _SubParts select subpart.Part.GetIndexBuffers();
            var bufferList = new List<IGpuBuffer>();
            foreach (var bufferArray in bufferArrays) bufferList.Union(bufferArray);
            return bufferList;
        }

        public IEnumerable<IGpuImage> GetTextureImages() {
            var imageArrays = from subpart in _SubParts select subpart.Part.GetTextureImages();
            var imageList = new List<IGpuImage>();
            foreach (var imageArray in imageArrays) imageList.Union(imageArray);
            return imageList;
        }

        public IEnumerable<IGpuBuffer> GetVertexBuffers() {
            var bufferArrays = from subpart in _SubParts select subpart.Part.GetVertexBuffers();
            var bufferList = new List<IGpuBuffer>();
            foreach (var bufferArray in bufferArrays) bufferList.Union(bufferArray);
            return bufferList;
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
                    glm.translate(result, new vec3(-Origin.x,-Origin.y,-Origin.z));
                    glm.scale(result, Scale);
                    glm.rotate(result, glm.degrees(XRotation), xAxis);
                    glm.rotate(result, glm.degrees(YRotation), yAxis);
                    glm.rotate(result, glm.degrees(ZRotation), zAxis);
                    glm.translate(result, Position);
                    return result;
                }
            }
            private static readonly vec3 xAxis = new vec3(1, 0, 0);
            private static readonly vec3 yAxis = new vec3(0, 1, 0);
            private static readonly vec3 zAxis = new vec3(0, 0, 1);
        }

        protected SubPart[] _SubParts;

    }
}
