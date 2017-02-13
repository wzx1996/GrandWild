using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.flamerat.GrandWild;
using org.flamerat.GrandWild.Scene;
using org.flamerat.GrandWild.Entity;

namespace org.flamerat.OnRailShooterDemo {
    class GameScene : Scene, ITimerBased {
        private TimerBasedEntity _Player;

        public enum Sign {
            Positive=1,
            Neutral=0,
            Negative=-1
        }
        public void SetPlayerMoveStatus(Sign? right=null,Sign? up=null,Sign? forward = null) {

        }
        public void SetWeaponStatus(Sign? bullet=null,Sign? laser=null) {

        }
        private Sign _PlayerMoveStateRight=Sign.Neutral;
        private Sign _PlayerMoveStateUp = Sign.Neutral;
        private Sign _PlayerMoveStateForward = Sign.Neutral;
        private Sign _WeaponStateBullet = Sign.Neutral;
        private Sign _WeaponStateLaser = Sign.Neutral;

        private float _PlayerMoveSpeed = 1.0F;
        private float _BulletSpeed = 0.5F;

        public void TimeTickBehavior(Timer timer) {
            throw new NotImplementedException();
        }
    }
}
