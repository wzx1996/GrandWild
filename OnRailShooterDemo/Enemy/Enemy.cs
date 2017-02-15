using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.flamerat.OnRailShooterDemo.Enemy {
    public class Enemy : Creature {
        public Enemy(float hp, float speed) : base(hp, speed) {
            RotateTo(0, 180, 0);
        }
    }
}
