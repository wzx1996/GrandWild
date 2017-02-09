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
            OnTimeTick(timer);
        }

        protected new vec3 _Position { get { return base._Position; } set { base._Position = value; } }
        protected new float _Scale { get { return base._Scale; } set { base._Scale = value; } }
        protected new float _XRotation { get { return base._XRotation; } set { base._XRotation = value; } }
        protected new float _YRotation { get { return base._YRotation; } set { base._YRotation = value; } }
        protected new float _ZRotation { get { return base._ZRotation; } set { base._ZRotation = value; } }
    }
}
