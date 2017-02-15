using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.flamerat.GrandWild;
using org.flamerat.GrandWild.Scene;
using org.flamerat.GrandWild.Entity;

namespace org.flamerat.OnRailShooterDemo {
    public class GameScene : Scene, ITimerBased {
        public uint Killcount { get; private set; }
        public TimeSpan ElapsedTime { get; private set; }
        public bool IsPlayerAlive { get; private set; } = true;
        public bool IsPlayerInvincible = false;

        public Player Player { get; private set; }
        public List<Enemy.Enemy> Enemies { get; private set; }
        public List<Bullet> Bullets { get; private set; }

        private void _ShootBullet(float power) {
            var newBullet = new Bullet(power);
            newBullet.MoveTo(Player.Position);
            Bullets.Add(newBullet);
            AddEntity(newBullet);
        }

        private void _SpawnEnemy(Enemy.Enemy enemy) {
            Enemies.Add(enemy);
            enemy.MoveTo(new GlmNet.vec3((float)_Randomizer.NextDouble() * 3.0F - 1.5F, (float)_Randomizer.NextDouble() * 2.0F, -3));
            AddEntity(enemy);
        }

        private void _DespawnBullet(Bullet bullet) {
            Bullets.Remove(bullet);
            RemoveEntity(bullet);
        }
        
        private void _DespawnEnemy(Enemy.Enemy enemy) {
            Enemies.Remove(enemy);
            RemoveEntity(enemy);
        }

        private bool _IsEntityOutOfGameArea(Entity entity) {
            if (entity.Position.x > 1.6) return true;
            if (entity.Position.x < -1.6) return true;
            if (entity.Position.y > 2.1) return true;
            if (entity.Position.y > -0.1) return true;
            if (entity.Position.z > -1.1) return true;
            if (entity.Position.z > 3.1) return true;
            return false;
        }

        public enum Sign {
            Positive=1,
            Neutral=0,
            Negative=-1
        }
        public void SetPlayerMoveStatus(Sign? right=null,Sign? up=null,Sign? forward = null) {
            if (right.HasValue) _PlayerMoveStateRight = right.Value;
            if (up.HasValue) _PlayerMoveStateUp = up.Value;
            if (forward.HasValue) _PlayerMoveStateForward = forward.Value;
        }
        public void SetWeaponStatus(Sign? bullet=null,Sign? laser=null) {
            if (bullet.HasValue) _WeaponStateBullet = bullet.Value;
            if (laser.HasValue) _WeaponStateLaser = laser.Value;
        }
        private Sign _PlayerMoveStateRight=Sign.Neutral;
        private Sign _PlayerMoveStateUp = Sign.Neutral;
        private Sign _PlayerMoveStateForward = Sign.Neutral;
        private Sign _WeaponStateBullet = Sign.Neutral;
        private float _BulletCooldown = 0.5F;
        private Sign _WeaponStateLaser = Sign.Neutral;

        private float _PlayerMoveSpeed = 3.0F;
        private float _BulletInteval = 0.5F;

        private float _EnemySpawnInteval = 2.0F;
        private float _EnemySpawnCooldown = 0.0F;
        private float _ChaserRate = 0.05F;
        private float _EnemySpeed = 0.2F;
        private float _EnemyHP = 10.0F;

        public GameScene() {
            OnGetFocus += _GotFocus;
            MoveTo(new GlmNet.vec3(0, 1, -1));
            RotateTo(0, 0, 0);
        }

        private bool _Started = false;
        private void _GotFocus(Scene sender) {
            if (!_Started) {
                _Started = true;
                SetMainTimerInteval(new TimeSpan(0, 0, 0, 0, 10));
                StartMainTimer();
            }
        }

        private Random _Randomizer = new Random();

        public void TimeTickBehavior(Timer timer) {
            ElapsedTime = timer.CurrentTimeFromStart;
            var timeInteval= (float)timer.ActualInteval.TotalSeconds;

            #region State machines
            switch (_PlayerMoveStateForward) {
                case Sign.Positive:
                    if (Player.Position.z > -3) {
                        Player.MoveFor(new GlmNet.vec3(0, 0, -_PlayerMoveSpeed / timeInteval));
                    }
                    break;
                case Sign.Negative:
                    if (Player.Position.z < 0) {
                        Player.MoveFor(new GlmNet.vec3(0, 0, _PlayerMoveSpeed / timeInteval));
                    }
                    break;
            }

            switch (_PlayerMoveStateRight) {
                case Sign.Positive:
                    if (Player.Position.x < 1.5) {
                        Player.MoveFor(new GlmNet.vec3(_PlayerMoveSpeed / timeInteval, 0, 0));
                        MoveFor(new GlmNet.vec3(_PlayerMoveSpeed / (timeInteval * 2), 0, 0));
                    }
                    break;
                case Sign.Negative:
                    if (Player.Position.x > -1.5) {
                        Player.MoveFor(new GlmNet.vec3(-_PlayerMoveSpeed / timeInteval, 0, 0));
                        MoveFor(new GlmNet.vec3(-_PlayerMoveSpeed / (timeInteval * 2), 0, 0));
                    }
                    break;
            }

            switch (_PlayerMoveStateUp) {
                case Sign.Positive:
                    if (Player.Position.y < 2) {
                        Player.MoveFor(new GlmNet.vec3(0,_PlayerMoveSpeed / timeInteval, 0));
                    }
                    break;
                case Sign.Negative:
                    if (Player.Position.y > 0) {
                        Player.MoveFor(new GlmNet.vec3(0,-_PlayerMoveSpeed / timeInteval, 0));
                    }
                    break;
            }

            switch (_WeaponStateBullet) {
                case Sign.Positive:
                    _BulletCooldown += timeInteval;
                    if (_BulletCooldown > _BulletInteval) {
                        _BulletCooldown -= _BulletInteval;
                        _ShootBullet(20.0F);
                    }
                    break;
            }

            switch (_WeaponStateLaser) {
                case Sign.Positive:
                    _PlayerMoveSpeed = 1.5F;
                    _BulletInteval = 1.0F;
                    foreach(var enemy in Enemies) {
                        if (enemy.Position.z < Player.Position.z) {
                            if (enemy.TestIfHit(new GlmNet.vec3(Player.Position.x, Player.Position.y, enemy.Position.z))) {
                                enemy.TakeDamage(10.0F / timeInteval);
                            }
                        }
                    }
                    break;
                case Sign.Neutral:
                case Sign.Negative:
                    _PlayerMoveSpeed = 3.0F;
                    _BulletInteval = 1.0F;
                    break;
            }
            #endregion

            #region Player death handling
            foreach(var enemy in Enemies) {
                if (enemy.TestIfHit(Player)) {
                    StopMainTimer();
                    IsPlayerAlive = false;
                    break;
                }
            }
            #endregion

            #region Enemy spawning
            _EnemySpawnCooldown += timeInteval;
            if (_EnemySpawnCooldown > _EnemySpawnInteval) {
                _EnemySpawnCooldown -= _EnemySpawnInteval;
                if (_Randomizer.NextDouble() < _ChaserRate) {
                    _SpawnEnemy(new Enemy.Chaser(_EnemyHP, _EnemySpeed));
                }else {
                    _SpawnEnemy(new Enemy.Puncher(_EnemyHP, _EnemySpeed));
                }
            }

            #endregion

            #region Dead/out-of-game-area enemy/bullet despawning
            foreach(var bullet in Bullets) {
                if (bullet.IsDestroyed || _IsEntityOutOfGameArea(bullet)) _DespawnBullet(bullet);
            }
            foreach(var enemy in Enemies) {
                if (enemy.IsDead() || _IsEntityOutOfGameArea(enemy)) _DespawnEnemy(enemy); 
            }
            #endregion
        }
    }
}
