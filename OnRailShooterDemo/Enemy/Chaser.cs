using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;

namespace org.flamerat.OnRailShooterDemo.Enemy {
    [Description("Chaser will chase towards player at it's move speed.", Author = "FlameRat")]
    public class Chaser : Enemy {
        public Chaser(float hp, float speed) : base(hp, speed) {
            OnTimeTick += _OnTimeTick;
        }

        private void _OnTimeTick(GrandWild.Timer timer) {
            var player = (BelongingScenes as IEnumerable<GameScene>).Where(scene => scene != null).First().Player;
            var direction = player.Position - Position;
            var yr = -glm.acos(direction.x / (float)Math.Sqrt(direction.x * direction.x + direction.z * direction.z));
            var xr = glm.acos(direction.y / (float)Math.Sqrt(direction.y * direction.y + direction.z * direction.z));
            RotateTo(glm.degrees(xr), glm.degrees(yr), 0);
            MoveForSelfCoordinate(MoveSpeed * (float)timer.ActualInteval.TotalSeconds,0,0);
        }
    }
}
