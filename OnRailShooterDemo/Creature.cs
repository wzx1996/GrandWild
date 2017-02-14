﻿using System;
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

        private float _HitboxWidth = 0.0F;

        public bool TestIfHit(Entity hitSource) {
            return TestIfHit(hitSource.Position);
        }

        public bool TestIfHit(vec3 hitSourcePos) {
            var hitboxHalfWidth = _HitboxWidth / 2;
            if (Math.Abs(this.Position.x - hitSourcePos.x) <= hitboxHalfWidth) return true;
            if (Math.Abs(this.Position.y - hitSourcePos.y) <= hitboxHalfWidth) return true;
            if (Math.Abs(this.Position.z - hitSourcePos.z) <= hitboxHalfWidth) return true;
            return false;
        }
    }


}