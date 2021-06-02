﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Top_Down_shooter.Scripts.Components;
using Top_Down_shooter.Scripts.Controllers;
using Top_Down_shooter.Scripts.GameObjects;
using Top_Down_shooter.Scripts.Source;
using unvell.D2DLib;

namespace Top_Down_shooter
{
    public class Form1 : Form
    {
        private readonly int IntervalUpdateGameLoop = 30;

        private D2DGraphicsDevice device;
        private D2DMatrix3x2 defaultTransform;

        private Label countBulletsLabel;

        public Form1()
        { 
            DoubleBuffered = false;
            Size = new Size(GameSettings.ScreenWidth, GameSettings.ScreenHeight);
            //FormBorderStyle = FormBorderStyle.None;
            CenterToScreen();

            GameModel.Initialize();
            GameRender.Initialize();
         
            RunTimeInvoker(IntervalUpdateGameLoop, UpdateGameLoop);
            RunTimeInvoker(GameSettings.DelaySpawnNewMonster, GameModel.SpawnEnemy);
            RunTimeInvoker(GameSettings.BosCooldown, GameModel.SpawnFire);

            RunFunctionAsync(Controller.UpdateKeyboardHandler);
            RunFunctionAsync(Controller.UpdateMouseHandler);
            RunFunctionAsync(NavMesh.Update);
            RunFunctionAsync(GameRender.PlayAnimations);
            RunFunctionAsync(Physics.Update);

            AddControls();
        }
        protected override void OnLoad(EventArgs e)
        {
            device = new D2DGraphicsDevice(this);
            defaultTransform = device.Graphics.GetTransform();
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            device.BeginRender();

            device.Graphics.SetTransform(defaultTransform);
            device.Graphics.TranslateTransform(-GameRender.Camera.X, -GameRender.Camera.Y);
            GameRender.DrawScene(device);

            device.EndRender();
        }

        private void UpdateGameLoop()
        {           
            UpdatePlayer();

            UpdateEnemies();

            UpdateBullets();

            for (var fire = GameModel.MovingFires.First; !(fire is null); fire = fire.Next)
            {
                fire.Value.Move();

                if (fire.Value.IsCompleteMoving)
                    GameModel.MovingFires.Remove(fire);
            }

            Invalidate();
            
        }

        private void UpdatePlayer()
        {
            var mousePosition = PointToClient(MousePosition);

            GameRender.Camera.Move(GameModel.Player);

            GameModel.Player.Gun.Angle = (float)Math.Atan2(
                mousePosition.Y + GameRender.Camera.Y - GameModel.Player.Gun.Y,
                mousePosition.X + GameRender.Camera.X - GameModel.Player.Gun.X);

            GameModel.Player.Move();
            if (Physics.IsCollided(GameModel.Player, out var collisions))
            {
                foreach (var other in collisions)
                {
                    if (other is Fire fire && fire.CanKick)
                    {
                        fire.CanKick = false;
                        GameModel.Player.Health -= GameSettings.FireDamage;
                    }
                    if (other is Powerup powerup)
                    {
                        if (powerup is HP)
                        {
                            if (GameModel.Player.Health < GameSettings.PlayerHealth)
                            {
                                GameModel.Player.Health += powerup.Boost;
                                GameModel.RespawnStaticPowerup(powerup);
                            }
                        }
                        else
                        {
                            GameModel.Player.Gun.CountBullets += powerup.Boost;

                            if (powerup is BigLoot)
                            {
                                Physics.RemoveFromTrackingCollisions(powerup.Collider);
                                GameModel.Powerups.Remove(powerup);
                                GameRender.RemoveDynamicRenderFrom(powerup);
                            }
                            else GameModel.RespawnStaticPowerup(powerup);
                        }
                    }

                    if (other is Box || other is Block || other is Boss)
                    {
                        GameModel.Player.Move(isReverse: true);

                        GameModel.Player.MoveX();
                        if (GameModel.Player.Collider.IntersectsWith(other.Collider))
                            GameModel.Player.MoveX(isReverse: true);

                        GameModel.Player.MoveY();
                        if (GameModel.Player.Collider.IntersectsWith(other.Collider))
                            GameModel.Player.MoveY(isReverse: true);

                    }
                }
            }
        }

        private void UpdateEnemies()
        {
            GameModel.Boss.Update();
            foreach (var enemy in GameModel.Enemies)
            {
                enemy.Move();

                if (enemy is Tank tank && Physics.IsCollided(enemy, out var collisions))
                {
                    foreach (var other in collisions)
                    {

                        if (other is Fire fire && fire.CanKick)
                        {
                            fire.CanKick = false;
                            enemy.Health -= GameSettings.FireDamage;
                            if (enemy.Health < 1)
                                GameModel.RespawnEnemy(enemy);
                        }

                        if (other is Player && tank.CanKick)
                        {
                            GameModel.Player.Health -= GameSettings.TankDamage;
                            tank.CanKick = false;
                        }
                    }
                }
            }
        }

        private void UpdateBullets()
        {
            GameModel.Player.Gun.CountBullets -= Controller.SpawnedBullets.Count;
            while (Controller.SpawnedBullets.Count > 0)
            {
                var bullet = Controller.SpawnedBullets.Dequeue();

                GameModel.Bullets.AddLast(bullet);

                GameRender.AddDynamicRenderFor(bullet);
            }

            countBulletsLabel.Text = GameModel.Player.Gun.CountBullets.ToString();

            for (var bullet = GameModel.Bullets.First; !(bullet is null); bullet = bullet.Next)
            {
                bullet.Value.Move();

                if (Physics.IsCollided(bullet.Value, out var collisions))
                {
                    var willBeDestroyed = false;

                    foreach (var other in collisions)
                    {
                        if (other is Block)
                            willBeDestroyed = true;

                        if (other is Box box)
                        {
                            box.Health -= bullet.Value.Damage;
                            if (box.Health == 0)
                            {
                                GameModel.ChangeBoxToGrass(box);
                                Physics.RemoveFromTrackingCollisions(box.Collider);
                                GameRender.RemoveDynamicRenderFrom(box);
                            }

                            willBeDestroyed = true;
                        }

                        if (other is Enemy enemy)
                        {
                            enemy.Health -= bullet.Value.Damage;
                            if (!(enemy is Boss) && enemy.Health < 1)
                                GameModel.RespawnEnemy(enemy);

                            willBeDestroyed = true;
                        }
                    }

                    if (willBeDestroyed)
                    {
                        GameModel.Bullets.Remove(bullet);
                        GameRender.RemoveDynamicRenderFrom(bullet.Value);
                        Physics.RemoveFromTrackingCollisions(bullet.Value.Collider);
                    }
                }
            }
        }

        private void RunTimeInvoker(int interval, Action func)
        {
            var timer = new System.Windows.Forms.Timer();

            timer.Interval = interval;
            timer.Tick += (sender, args) => func();

            timer.Start();
        }

        private void RunFunctionAsync(Action func)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += (sender, args) => func();
            worker.RunWorkerAsync();
        }

        private void AddControls()
        {
            var gameTimer = new System.Windows.Forms.Timer();
            var time = GameSettings.TimeToEnd;
            var timeLabel = new Label()
            {
                Size = new Size(200, 200),
                Font = new Font("Arial Rounded MT Bold", 30),
                BackColor = Color.Transparent,
                Location = new Point(1100, 20)
            };

            gameTimer.Tick += (sender, obj) =>
            {
                time -= TimeSpan.FromSeconds(1);
                timeLabel.Text = time.ToString(@"m\:ss");
            };
            gameTimer.Interval = 1000;
            gameTimer.Start();
            Controls.Add(timeLabel);

            countBulletsLabel = new Label()
            {
                Size = new Size(200, 200),
                Font = new Font("Arial Rounded MT Bold", 30),
                BackColor = Color.Transparent,
                Location = new Point(1160, 666)
            };
            Controls.Add(countBulletsLabel);
        }
    }
}
