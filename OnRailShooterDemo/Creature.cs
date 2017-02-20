using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;
using org.flamerat.GrandWild.Entity;

namespace org.flamerat.OnRailShooterDemo {
    public class Creature : TimerBasedEntity, IHasHitbox {
        public float HitPoint {get;private set;}
        public bool IsDead() {
            if (HitPoint <= 0) return true;
            else return false;
        }


        public void TakeDamage(float damage) {
            HitPoint -= damage;
        }

        private float _MoveSpeed = 0.0F;
        public float MoveSpeed {
            get { return _MoveSpeed; }
            set { if (value >= 0) _MoveSpeed = value; }
        }

        public Creature(float hp=100.0F,float moveSpeed = 0.0F) {
            HitPoint = hp;
            MoveSpeed = moveSpeed;
        }

        public float HitboxWidth = 0.0F;

        public bool TestIfHit(Entity hitSource) {
            return TestIfHit(hitSource.Position);
        }

        public bool TestIfHit(vec3 hitSourcePos) {
            var hitboxHalfWidth = HitboxWidth / 2;
            if (Math.Abs(this.Position.x - hitSourcePos.x) <= hitboxHalfWidth) 
            if (Math.Abs(this.Position.y - hitSourcePos.y) <= hitboxHalfWidth) 
            if (Math.Abs(this.Position.z - hitSourcePos.z) <= hitboxHalfWidth) return true;
            return false;
        }
    }


}
