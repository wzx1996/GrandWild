using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;

namespace org.flamerat.GrandWild.Entity {
    public class TimerBasedEntity : Entity, ITimerBased {

        public event Timer.OnTimeTickEvent OnTimeTick;
        public void TimeTickBehavior(Timer timer) {
            OnTimeTick?.Invoke(timer);
        }
    }
}
