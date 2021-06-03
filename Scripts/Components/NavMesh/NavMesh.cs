﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Top_Down_shooter.Scripts.Controllers;
using Top_Down_shooter.Scripts.GameObjects;
using Top_Down_shooter.Scripts.Source;

namespace Top_Down_shooter.Scripts.Components
{
    static class NavMesh
    {
        public static readonly Node[,] Map;
        public static readonly LinkedList<NavMeshAgent> Agents = new LinkedList<NavMeshAgent>();
        public static readonly Dictionary<GameObject, List<Node>> Obstacles = new Dictionary<GameObject, List<Node>>();

        public static readonly int Width;
        public static readonly int Height;
        public static readonly int DistanceFromObstacle = 30;
        public static readonly int StepAgent = 16;
        public static readonly int TimeUpdate = 200;

        public static readonly int CostOrthogonalPoint = 10;

        static NavMesh()
        {
            Width = GameSettings.MapWidth / StepAgent;
            Height = GameSettings.MapHeight / StepAgent;

            Map = new Node[Width, Height];
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    Map[x, y] = new Node(new Point(x * StepAgent, y * StepAgent));
                }
            }
        }

        public static void Bake()
        {
            foreach (var node in Map)
                node.IsObstacle = false;

            foreach (var collider in Physics.Colliders.Where(collider => !collider.IsIgnoreNavMesh && !collider.IsTrigger))
            {
                var rect = collider.Transform;

                var offset = DistanceFromObstacle - DistanceFromObstacle % StepAgent;

                var xLeft = rect.X - offset;
                if (xLeft < 0) xLeft = 0;

                var yTop = rect.Y - offset;
                if (yTop < 0) yTop = 0;

                var xRight = rect.X + rect.Width + offset;
                if (xRight >= GameSettings.MapWidth)
                    xRight = GameSettings.MapWidth - 1;

                var yBottom = rect.Y + rect.Height + offset;
                if (yBottom >= GameSettings.MapHeight)
                    yBottom = GameSettings.MapHeight - 1;

                for (var x = xLeft; x <= xRight; x += StepAgent)
                {
                    for (var y = yTop; y <= yBottom; y += StepAgent)
                    {
                        Map[x / StepAgent, y / StepAgent].Parent = collider;
                        Map[x / StepAgent, y / StepAgent].IsObstacle = true;
                    }
                }
            }
        }

        public static void Update()
        {
            while (true)
            {
                Bake();

                for (var agent = Agents.First; !(agent is null); agent = agent.Next)
                    agent.Value.ComputePath();

                Thread.Sleep(TimeUpdate);
            }
        }

        public static void AddAgent(NavMeshAgent agent)
        {
            Agents.AddLast(agent);
        }
    }
}
