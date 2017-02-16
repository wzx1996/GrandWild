using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;

namespace org.flamerat.GrandWild.Resource {
    public class WaveformObj:IResourceLoader {
        public WaveformObj(string fileName,bool clockWise=false,bool flipTextureVertical=true,vec4? color=null) {
            if (!color.HasValue) color = new vec4(0, 0, 0, 0);
            var objFileStream = new System.IO.StreamReader(fileName);
            List<vec4> rawVertexes=new List<vec4>();
            List<vec4> rawNormals = new List<vec4>();
            List<vec2> rawTextures = new List<vec2>();
            List<_Face> rawFaces = new List<_Face>();
            vec4 currentVertex = new vec4();
            vec4 currentNormal = new vec4();
            vec2 currentTexture = new vec2();
            _Face currentFace = new _Face();
            var readingState = ReadingState.LineEnd;
            string lineBuffer = "";
            Queue<string> tokenBuffer = new Queue<string>();
            string currentToken = "";

            //Finite state machine for reading file
            while(!objFileStream.EndOfStream){
                switch (tokenBuffer.Count) {
                    case 0:
                        readingState = ReadingState.LineEnd;
                        break;
                    default:
                        currentToken=tokenBuffer.Dequeue();
                        break;
                }

                switch (readingState) {
                    case ReadingState.Exit:
                        goto EndOfReadingMachine;
                    case ReadingState.LineHeader:
                        switch (currentToken) {
                            case "v":
                                readingState = ReadingState.VertexBody1;
                                break;
                            case "vt":
                                readingState = ReadingState.TextureBody1;
                                break;
                            case "vn":
                                readingState = ReadingState.NormalBody1;
                                break;
                            case "f":
                                readingState = ReadingState.FaceBody;
                                break;
                            default:
                                tokenBuffer.Clear();
                                break;
                        }
                        break;
                    case ReadingState.VertexBody1:
                        rawVertexes.Add(new vec4(0, 0, 0, 1));
                        currentVertex = new vec4(0, 0, 0, 1);
                        currentVertex.x = float.Parse(currentToken);
                        readingState = ReadingState.VertexBody2;
                        break;
                    case ReadingState.VertexBody2:
                        currentVertex.y = float.Parse(currentToken);
                        readingState = ReadingState.VertexBody3;
                        break;
                    case ReadingState.VertexBody3:
                        currentVertex.z = float.Parse(currentToken);
                        rawVertexes[rawVertexes.Count-1] = currentVertex;
                        tokenBuffer.Clear();
                        break;
                    case ReadingState.TextureBody1:
                        rawTextures.Add(new vec2());
                        currentTexture = new vec2();
                        currentTexture.x = float.Parse(currentToken);
                        readingState = ReadingState.TextureBody2;
                        break;
                    case ReadingState.TextureBody2:
                        currentTexture.y = float.Parse(currentToken);
                        rawTextures[rawTextures.Count-1] = currentTexture;
                        tokenBuffer.Clear();
                        break;
                    case ReadingState.NormalBody1:
                        rawNormals.Add(new vec4(0, 0, 0, 0));
                        currentNormal = new vec4();
                        currentNormal.x = float.Parse(currentToken);
                        readingState = ReadingState.NormalBody2;
                        break;
                    case ReadingState.NormalBody2:
                        currentNormal.y = float.Parse(currentToken);
                        readingState = ReadingState.NormalBody3;
                        break;
                    case ReadingState.NormalBody3:
                        currentNormal.z = float.Parse(currentToken);
                        rawNormals[rawNormals.Count - 1] = currentNormal;
                        tokenBuffer.Clear();
                        break;
                    case ReadingState.FaceBody: {
                            currentFace = new _Face();
                            Func<string, _Vertex> vertex = token => {
                                var values = (from part in token.Split('/') select uint.Parse(part)).ToArray();
                                _Vertex result = new _Vertex();
                                switch (values.Length) {
                                    default:
                                    case 3:
                                        result.Normal = values[2];
                                        goto case 2;
                                    case 2:
                                        result.Texture = values[1];
                                        goto case 1;
                                    case 1:
                                        result.Vertex = values[0];
                                        break;
                                    case 0:
                                        break;
                                }
                                return result;
                            };
                            currentFace.vertexes = (from token in tokenBuffer select vertex(token)).ToArray();
                            if (clockWise) currentFace.vertexes = currentFace.vertexes.Reverse().ToArray();
                        }
                        rawFaces.Add(currentFace);
                        tokenBuffer.Clear();
                        break;
                    case ReadingState.LineEnd:
                        lineBuffer = objFileStream.ReadLine();
                        foreach(var token in lineBuffer.Split(' ','\t')) tokenBuffer.Enqueue(token);
                        readingState = ReadingState.LineHeader;
                        break;
                    default:
                        tokenBuffer.Clear();
                        break;
                }
            }
            EndOfReadingMachine: objFileStream.Close();

            if (flipTextureVertical) {
                rawTextures = rawTextures.Select(texture => new vec2(texture.x, 1.0F - texture.y)).ToList();
            }

            List<GrandWildKernel.Vertex> vertexList = new List<GrandWildKernel.Vertex>();
            List<UInt16> indexList = new List<ushort>();

            foreach(var face in rawFaces) {
                switch (face.vertexes.Length) {
                    case 0:
                    case 1:
                    case 2:
                        continue;
                    default: {
                            vertexList.Add(new GrandWildKernel.Vertex {
                                Position = rawVertexes[(int)face.vertexes[0].Vertex - 1],
                                Normal = rawNormals[(int)face.vertexes[0].Normal - 1],
                                Color = color.Value,
                                Texture = rawTextures[(int)face.vertexes[0].Vertex - 1],
                            });
                            UInt16 firstPoint =(UInt16) vertexList.Count;
                            vertexList.Add(new GrandWildKernel.Vertex {
                                Position = rawVertexes[(int)face.vertexes[1].Vertex - 1],
                                Normal = rawNormals[(int)face.vertexes[1].Normal - 1],
                                Color = color.Value,
                                Texture = rawTextures[(int)face.vertexes[1].Vertex - 1],
                            });
                            UInt16 secondPoint = (UInt16)vertexList.Count;
                            for (var i = 2; i <= face.vertexes.Length - 1; i++) {
                                vertexList.Add(new GrandWildKernel.Vertex {
                                    Position = rawVertexes[(int)face.vertexes[i].Vertex - 1],
                                    Normal = rawNormals[(int)face.vertexes[i].Normal - 1],
                                    Color = color.Value,
                                    Texture = rawTextures[(int)face.vertexes[i].Vertex - 1],
                                });
                                indexList.Add(firstPoint);
                                indexList.Add(secondPoint);
                                secondPoint =(UInt16) vertexList.Count;
                                indexList.Add(secondPoint);
                            }
                        }
                        break;
                }
            }
            Vertexes = vertexList.ToArray();
            Indexes = indexList.ToArray();
            
        }
        public GrandWildKernel.Vertex[] Vertexes { get; private set; }
        public UInt16[] Indexes { get; private set; }
        public void SetColor(vec4 color) {
            Vertexes = Vertexes.Select(x => new GrandWildKernel.Vertex { Position = x.Position, Texture = x.Texture, Color = color, Normal = x.Normal }).ToArray();
        }
        private vec3 _GetFaceNormal(vec3 p1,vec3 p2,vec3 p3) {
            vec3 side1 = p2 - p1;
            vec3 side2 = p3 - p1;
            return glm.normalize(glm.cross(side1, side2));
        }
        private struct _Face {
            public _Vertex[] vertexes;
        }
        private struct _Vertex {
            public uint Vertex;
            public uint Texture;
            public uint Normal;
        }
        private enum ReadingState {
            Exit,
            LineHeader,
            VertexBody1,
            VertexBody2,
            VertexBody3,
            TextureBody1,
            TextureBody2,
            NormalBody1,
            NormalBody2,
            NormalBody3,
            FaceBody,
            LineEnd
        }
    }
}
