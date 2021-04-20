﻿using System;

namespace Top_Down_shooter.Scripts.GameObjects
{
    class Bullet
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Speed { get; set; }
        public float Angle { get; set; }

        public Bullet(int x, int y, int speed, float angle)
        {
            Speed = speed;
            X = x;
            Y = y;
            Angle = angle;
        }

        public void Move()
        {
            X += (int)(Speed * Math.Cos(Angle));
            Y += (int)(Speed * Math.Sin(Angle));
        }
    }
}
