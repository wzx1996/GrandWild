using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.flamerat.GrandWild.Entity;

namespace org.flamerat.OnRailShooterDemo {
    public class Creature : TimerBasedEntity, IHasHitbox {
        public int HitPoint {get;private set;}

        public void TakeDamage() {
            throw new System.NotImplementedException();
        }

        public bool TestIfHit(Entity hitSource) {
            throw new NotImplementedException();
        }
    }
}
