using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.flamerat.OnRailShooterDemo.Enemy {
    [Description("Puncher will move towards a constant direction at constant speed.")]
    public class Puncher : Enemy {
        public Puncher(float hp, float speed,float pan=0,float pitch=0) : base(hp, speed) {
            RotateFor(pitch, pan, 0);
        }
    }
}
