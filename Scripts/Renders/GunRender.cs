﻿using System;
using System.Drawing;
using Top_Down_shooter.Scripts.GameObjects;

namespace Top_Down_shooter.Scripts.Renders
{
    class GunRender : IRender
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Size Size => image.Size;

        private readonly Gun gun;
        private readonly Bitmap image;

        public GunRender(Gun gun, Bitmap image)
        {
            
            this.gun = gun;
            this.image = image;
        }

        public void Draw(Graphics g)
        {
            X = gun.X - image.Width / 2;
            Y = gun.Y - image.Height / 2;
            g.TranslateTransform(gun.X, gun.Y);
            g.RotateTransform((float)(gun.Angle * 180 / Math.PI));
            g.TranslateTransform(-gun.X, -gun.Y);

            g.DrawImage(image,
                X, Y,
                new Rectangle(0, 0, image.Width, image.Height),
                GraphicsUnit.Pixel);

            g.TranslateTransform(gun.X, gun.Y);
            g.RotateTransform(-(float)(gun.Angle * 180 / Math.PI));
            g.TranslateTransform(-gun.X, -gun.Y);
        }
    }

}