using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.flamerat.OnRailShooterDemo {
    public interface IHasHitbox {
        bool TestIfHit(org.flamerat.GrandWild.Entity.Entity hitSource);
        bool TestIfHit(GlmNet.vec3 hitSourcePos);
    }
}