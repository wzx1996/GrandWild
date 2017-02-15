using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.flamerat.OnRailShooterDemo {
    public class Bullet : GrandWild.Entity.TimerBasedEntity {
        public float Damage { get; private set; }
        public Bullet(float power) {
            Damage = power;
        }
    }
}