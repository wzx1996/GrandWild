using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace org.flamerat.OnRailShooterDemo {
    public partial class DebugForm : Form {
        public DebugForm() {
            InitializeComponent();
            _Timer = new Timer();
            _Timer.Interval = 20;
            _Timer.Tick += _Timer_Tick;
            _Timer.Start();

            txtInfo.Location = new Point(0, 0);

            SizeChanged += DebugForm_SizeChanged;

            Show();
        }

        private void DebugForm_SizeChanged(object sender, EventArgs e) {
            txtInfo.Size = this.ClientSize;
        }

        private void _Timer_Tick(object sender, EventArgs e) {
            StringBuilder stringBuilder = new StringBuilder();
            var scene = Program.Scene;
            stringBuilder.AppendLine(string.Format("ElapsedTime: {0}", scene.ElapsedTime));
            stringBuilder.AppendLine(string.Format("KillCount: {0}", scene.Killcount));
            stringBuilder.AppendLine(string.Format("Player: [{0}, {1}, {2}], alive={3}, laser={4}",
                scene.Player.Position.x, scene.Player.Position.y, scene.Player.Position.z,
                scene.IsPlayerAlive, scene.Player.SubParts[1].Visible
                )
            );
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Enemies:");
            foreach(var enemy in scene.Enemies.ToArray()) {
                stringBuilder.AppendLine(string.Format("{0}: [{1}, {2}, {3}], HP={4}",
                    enemy.GetType().Name,
                    enemy.Position.x, enemy.Position.y, enemy.Position.z, enemy.HitPoint
                    )
                );
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Bullets:");
            foreach (var bullet in scene.Bullets.ToArray()) {
                stringBuilder.AppendLine(string.Format("{0}: [{1}, {2}, {3}]",
                    bullet.GetType().Name,
                    bullet.Position.x, bullet.Position.y, bullet.Position.z
                    )
                );
            }

            txtInfo.Text = stringBuilder.ToString();
        }

        private Timer _Timer;
    }
}
