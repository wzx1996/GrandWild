using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.flamerat.GrandWild;
using org.flamerat.GrandWild.Scene;

namespace org.flamerat.OnRailShooterDemo {
    static class Launcher {
        public static int Main(string[] args) {
            GrandWildKernel engineKernal = new GrandWildKernel();
            engineKernal.AppName = "GrandWild 0.2 Demo: On Rain Shooter";
            engineKernal.AppVersion = 1;
            engineKernal.WindowWidth = 1280;
            engineKernal.WindowHeight = 720;
            engineKernal.WindowTitle = "GrandWild 0.2 Demo: On Rain Shooter";

            engineKernal.OnKeyDown += _OnKeyDownBehavior;

            Scene mainScene = new Scene();



            engineKernal.Launch();

            while (engineKernal.IsRunning) ;

            return 0;
        }

        private static void _OnKeyDownBehavior(GrandWildKernel sender, System.Windows.Forms.Keys key) {
            
            switch (key) {
                case System.Windows.Forms.Keys.W:
                    sender.FocusedScene.TargetWalkSpeedForward = 1;
                    break;

            }
        }
    }
}
