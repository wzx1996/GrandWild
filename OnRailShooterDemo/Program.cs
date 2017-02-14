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
