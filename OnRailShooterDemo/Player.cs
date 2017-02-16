using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.flamerat.OnRailShooterDemo {
    public class Player : Creature {
        public Player() {
            while (Program.Kernel == null) System.Threading.Thread.Sleep(1000);
            SubParts = new SubPart[2] {
                new SubPart {
                    Part=Program.Models["Player"],
                    Origin=new GlmNet.vec3(0,1,0),
                    Scale=new GlmNet.vec3(0.5F,0.5F,0.5F),
                    Visible=true
                },
                new SubPart {
                    Part=Program.Models["Laser"],
                    Origin=new GlmNet.vec3(0,0,0.5F),
                    Scale=new GlmNet.vec3(0.05F,0.05F,3F),
                    Visible=false
                }
            };
        }
    }
}
