using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.flamerat.GrandWild;
using Forms = System.Windows.Forms;

namespace org.flamerat.OnRailShooterDemo {
    public static class Program {
        public static GrandWildKernel Kernel {
            get {
                if (_Kernel == null || !_Kernel.IsRunning) {
                    throw new KernelNotLaunchedException();
                }else {
                    return _Kernel;
                }
            }
        }
        private static GrandWildKernel _Kernel = null;
        public static GameScene Scene { get; private set; }

        public static Dictionary<string, GrandWild.VertexBuffer<GrandWild.GrandWildKernel.Vertex>> VertexBuffers;
        public static Dictionary<string, IndexBuffer> IndexBuffers;
        public static Dictionary<string, TextureImage> Textures;
        public static Dictionary<string, GrandWild.Rendering.Model> Models;

        public static int Main(string[] args) {
            _Kernel = new GrandWildKernel();
            _Kernel.AppName = "GrandWild 0.2 Demo: On Rain Shooter";
            _Kernel.AppVersion = 1;
            _Kernel.WindowWidth = 1280;
            _Kernel.WindowHeight = 720;
            _Kernel.WindowTitle = "GrandWild 0.2 Demo: On Rain Shooter";

            _Kernel.OnKeyDown += _OnKeyDownBehavior;
            _Kernel.OnKeyUp += _OnKeyUpBehavior;

            Scene = new GameScene();

            _Kernel.Launch();

            var objFile = new GrandWild.Resource.WaveformObj(@"Resource\dart.obj");
            VertexBuffers.Add("Chaser",new VertexBuffer<GrandWild.GrandWildKernel.Vertex>(Kernel.Device, objFile.Vertexes));
            IndexBuffers.Add("Chaser", new IndexBuffer(Kernel.Device, objFile.Indexes));

            objFile = new GrandWild.Resource.WaveformObj(@"Resource\pony.obj");
            VertexBuffers.Add("Player", new VertexBuffer<GrandWildKernel.Vertex>(Kernel.Device, objFile.Vertexes));
            IndexBuffers.Add("Player", new IndexBuffer(Kernel.Device, objFile.Indexes));

            objFile = new GrandWild.Resource.WaveformObj(@"Resource\puzzle_cube.obj");
            VertexBuffers.Add("Puncher", new VertexBuffer<GrandWildKernel.Vertex>(Kernel.Device, objFile.Vertexes));
            IndexBuffers.Add("Puncher", new IndexBuffer(Kernel.Device, objFile.Indexes));
            objFile.SetColor(new GlmNet.vec4(1, 1, 1, 1));
            VertexBuffers.Add("Bullet", new VertexBuffer<GrandWildKernel.Vertex>(Kernel.Device, objFile.Vertexes));
            IndexBuffers.Add("Bullet", new IndexBuffer(Kernel.Device, objFile.Indexes));
            objFile.SetColor(new GlmNet.vec4(1, 0.2F, 0.2F, 1));
            VertexBuffers.Add("Laser", new VertexBuffer<GrandWildKernel.Vertex>(Kernel.Device, objFile.Vertexes));
            IndexBuffers.Add("Laser", new IndexBuffer(Kernel.Device, objFile.Indexes));

            var textureFile = new GrandWild.Resource.Image(@"Resource\dart.PNG");
            Textures.Add("Chaser", new TextureImage(Kernel.Device, textureFile.Data));

            textureFile = new GrandWild.Resource.Image(@"Resource\pony.PNG");
            Textures.Add("Chaser", new TextureImage(Kernel.Device, textureFile.Data));

            textureFile = new GrandWild.Resource.Image(@"Resource\puzzle_cube.PNG");
            Textures.Add("Chaser", new TextureImage(Kernel.Device, textureFile.Data));

            Models.Add("Chaser", new GrandWild.Rendering.Model {_VertexBufferObject=VertexBuffers["Chaser"],_IndexBuffer=IndexBuffers["Chaser"],_TextureImage=Textures["Chaser"] });
            Models.Add("Player", new GrandWild.Rendering.Model());
            Models.Add("Puncher", new GrandWild.Rendering.Model());
            Models.Add("Bullet", new GrandWild.Rendering.Model());
            Models.Add("Laser", new GrandWild.Rendering.Model());

            Models["Chaser"]

            _Kernel.FocusedScene = Scene;

            while (_Kernel.IsRunning) ;

            return 0;
        }



        private static void _OnKeyDownBehavior(GrandWildKernel sender, System.Windows.Forms.Keys key) {
            switch (key) {
                case Forms.Keys.W:
                    Scene.SetPlayerMoveStatus(up: GameScene.Sign.Positive);
                    break;
                case Forms.Keys.S:
                    Scene.SetPlayerMoveStatus(up: GameScene.Sign.Negative);
                    break;
                case Forms.Keys.A:
                    Scene.SetPlayerMoveStatus(right: GameScene.Sign.Negative);
                    break;
                case Forms.Keys.D:
                    Scene.SetPlayerMoveStatus(right: GameScene.Sign.Positive);
                    break;
                case Forms.Keys.Space:
                    Scene.SetPlayerMoveStatus(forward: GameScene.Sign.Positive);
                    break;
                case Forms.Keys.LShiftKey:
                    Scene.SetPlayerMoveStatus(forward: GameScene.Sign.Negative);
                    break;
                case Forms.Keys.J:
                    Scene.SetWeaponStatus(bullet: GameScene.Sign.Positive);
                    break;
                case Forms.Keys.K:
                    Scene.SetWeaponStatus(laser: GameScene.Sign.Positive);
                    break;
            }
        }

        private static void _OnKeyUpBehavior(GrandWildKernel sender, Forms.Keys key) {
            switch (key) {
                case Forms.Keys.W:
                    Scene.SetPlayerMoveStatus(up: GameScene.Sign.Neutral);
                    break;
                case Forms.Keys.S:
                    Scene.SetPlayerMoveStatus(up: GameScene.Sign.Neutral);
                    break;
                case Forms.Keys.A:
                    Scene.SetPlayerMoveStatus(right: GameScene.Sign.Neutral);
                    break;
                case Forms.Keys.D:
                    Scene.SetPlayerMoveStatus(right: GameScene.Sign.Neutral);
                    break;
                case Forms.Keys.Space:
                    Scene.SetPlayerMoveStatus(forward: GameScene.Sign.Neutral);
                    break;
                case Forms.Keys.LShiftKey:
                    Scene.SetPlayerMoveStatus(forward: GameScene.Sign.Neutral);
                    break;
                case Forms.Keys.J:
                    Scene.SetWeaponStatus(bullet: GameScene.Sign.Neutral);
                    break;
                case Forms.Keys.K:
                    Scene.SetWeaponStatus(laser: GameScene.Sign.Neutral);
                    break;
            }
        }
    }


    [Serializable]
    public class KernelNotLaunchedException : Exception {
        public override string Message {
            get {
                return "Accessing kernel failed: kernel not created or not launched";
            }
        }
    }

    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class DescriptionAttribute : Attribute {
        readonly string _Content;
        public DescriptionAttribute(string content) {
            this._Content = content;
            throw new NotImplementedException();
        }
        public string Content {
            get { return _Content; }
        }

        public string Author { get; set; }
    }
}
