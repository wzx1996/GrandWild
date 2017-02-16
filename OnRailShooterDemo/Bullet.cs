using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.flamerat.OnRailShooterDemo {
    public class Bullet : GrandWild.Entity.TimerBasedEntity {
        public float Damage { get; private set; }
        public bool IsDestroyed { get; private set; }
        public float Speed { get; private set; }
        public Bullet(float power,float speed) {
            Damage = power;
            Speed = speed;
            OnTimeTick += _OnTimeTick;
            while (Program.Kernel == null) System.Threading.Thread.Sleep(1000) ;
            SubParts = new SubPart[1] {
                new SubPart {
                    Part=Program.Models["Bullet"],
                    Scale=new GlmNet.vec3(0.05F,0.05F,0.05F),
                    Visible=true
                }
            };
        }

        private void _OnTimeTick(GrandWild.Timer timer) {
            var targets = (BelongingScenes as IEnumerable<GameScene>).Where(scene=>scene!=null).SelectMany(scene => scene.Enemies).Distinct();
            foreach(var target in targets) {
                if (target.TestIfHit(this)) target.TakeDamage(Damage);
            }
            MoveFor(new GlmNet.vec3(0, 0, -Speed * (float)timer.ActualInteval.TotalSeconds));
        }
    }
}