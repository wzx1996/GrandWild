using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.flamerat.OnRailShooterDemo {
    public class Bullet : GrandWild.Entity.TimerBasedEntity {
        public float Damage { get; private set; }
        public bool IsDestroyed { get; private set; }
        public Bullet(float power) {
            Damage = power;
            OnTimeTick += _OnTimeTick;
            while (Program.Kernel == null) ;
            SubParts=new SubPart[1] {
                new SubPart {
                    Part=
                }
            }
        }

        private void _OnTimeTick(GrandWild.Timer timer) {
            var targets = (BelongingScenes as IEnumerable<GameScene>).Where(scene=>scene!=null).SelectMany(scene => scene.Enemies).Distinct();
            foreach(var target in targets) {
                if (target.TestIfHit(this)) target.TakeDamage(Damage);
            }
        }
    }
}