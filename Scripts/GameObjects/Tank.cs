﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Top_Down_shooter.Scripts.GameObjects;
using Top_Down_shooter.Scripts.Components;
using Top_Down_shooter.Scripts.Source;

namespace Top_Down_shooter.Scripts.GameObjects
{
    class Tank : Enemy
    {
        public Stack<Point> path = new Stack<Point>();

        private Point nextCheckPoint;
        private int resetPath;

        private static Random randGenerator = new Random();

        public Tank(int x, int y)
        {
            X = x;
            Y = y;

            resetPath = randGenerator.Next(GameSettings.TankResetPathMin, GameSettings.TankResetPathMax);
            if (randGenerator.NextDouble() > 1 - GameSettings.ProbabilitiSpeedMax)
                Speed = randGenerator.Next(GameSettings.PlayerSpeed, GameSettings.TankSpeedMax);
            else
                Speed = randGenerator.Next(GameSettings.TankSpeedMin, GameSettings.PlayerSpeed - 1);
            Health = GameSettings.TankHealth;

            Collider = new Collider(this, localX: 0, localY: 30, width: 60, height: 60);
            HitBox = new Collider(this, localX: 0, localY: 0, width: 60, height: 90);

            nextCheckPoint = GameModel.Player.Transform;
        }

        public override void Move(bool isReverse = false)
        {
            if (path.Count < resetPath)
            { 
                path = NavMeshAgent.GetPath(Transform, GameModel.Player.Transform);
                resetPath = randGenerator.Next(GameSettings.TankResetPathMin, GameSettings.TankResetPathMax);
            }
            if (path.Count > 0)
                nextCheckPoint = path.Pop();

            var q = MoveTowards(Transform, nextCheckPoint, Speed);
            LookAt(GameModel.Player.Transform);

            X = q.X;
            Y = q.Y;
        }


        
    }
}
