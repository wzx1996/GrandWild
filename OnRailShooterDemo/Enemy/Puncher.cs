using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.flamerat.OnRailShooterDemo.Enemy {
    [Description("Puncher will move towards a constant direction at constant speed.")]
    public class Puncher : Enemy {
        public Puncher(float hp, float speed,float pan=0,float pitch=0) : base(hp, speed) {
            OnTimeTick += _OnTimeTick;
            RotateFor(pitch, pan, 0);
            HitboxWidth = 0.4F;
            while (Program.Kernel == null) System.Threading.Thread.Sleep(1000);
            SubParts = new SubPart[1] {
                new SubPart {
                    Part=Program.Models["Puncher"],
                    Scale=new GlmNet.vec3(0.4F,0.4F,0.4F),
                    Visible=true
                }
            };
        }

        private void _OnTimeTick(GrandWild.Timer timer) {
            //MoveForSelfCoordinate(MoveSpeed * (float)timer.ActualInteval.TotalSeconds, 0, 0);
            MoveForSelfCoordinate(MoveSpeed * (float)timer.TargetInteval.TotalSeconds, 0, 0);
        }
    }
}
