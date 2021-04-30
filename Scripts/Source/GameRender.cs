﻿using System.Collections.Generic;
using System.Drawing;
using Top_Down_shooter.Properties;
using Top_Down_shooter.Scripts.Renders;
using Top_Down_shooter.Scripts.Controllers;

namespace Top_Down_shooter
{
    class GameRender
    {
        public readonly CameraController Camera;
        public readonly List<IRender> gameObjects;
        public readonly IRender player;

        public GameRender()
        {
            gameObjects = new List<IRender>();
            Camera = new CameraController();

            player = new CharacterRender(GameModel.Player, Resources.Player, 4, 2);
            gameObjects.Add(player);

            gameObjects.Add(new GunRender(GameModel.Player.Gun, Resources.Gun));

            gameObjects.Add(new BulletsRender(GameModel.Bullets, Resources.Bullet));

        }

        public void DrawObjects(Graphics g)
        {
            foreach (var obj in gameObjects)
            {
                obj.Draw(g);
            }
        }

        public void PlayAnimations()
        {
            foreach (var obj in gameObjects)
            {
                if (obj is CharacterRender character)
                {
                    character.ChangeTypeAnimation();
                    character.PlayAnimation();
                }
            }
        }
    }
}