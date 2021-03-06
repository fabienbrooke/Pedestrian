﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pedestrian.Engine;
using Pedestrian.Engine.Collision;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Pedestrian.Engine.Input;

namespace Pedestrian
{
    public class Scene
    {
        public const int SCOREBOARD_HEIGHT = 30;
        
        int Width { get { return PedestrianGame.VIRTUAL_WIDTH; } }
        int Height { get { return PedestrianGame.VIRTUAL_HEIGHT - SCOREBOARD_HEIGHT - 1; } }

        List<IEntity> entities = new List<IEntity>();
        Scoreboard scoreboard;
        SoundEffectInstance engineIdleSound;
        

        public Scene(int numPlayers)
        {
            var borderRectangle = new Rectangle(
                0,
                SCOREBOARD_HEIGHT,
                Width,
                Height
            );
            var gameArea = new PlayArea(borderRectangle, 32, 2);
            var roadArea = new RoadBounds(new Rectangle(
                    gameArea.Bounds.X + gameArea.SidewalkWidth,
                    gameArea.Bounds.Y,
                    gameArea.Bounds.Width - 2 * gameArea.SidewalkWidth,
                    gameArea.Bounds.Height
                )
            );
            entities.Add(gameArea);
            entities.Add(roadArea);

            // Initialize the players
            if (numPlayers >= 1)
            {
                var supportedInputs = new List<IPlayerInput> {
                    new ControllerPlayerInput(ControllerInputMap.Primary, PlayerIndex.One),
                    new KeyboardPlayerInput(KeyboardInputMap.Primary),
                };
                if (numPlayers == 1)
                {
                    // allow both key inputs (arrows and wasd) if single player
                    supportedInputs.Add(new KeyboardPlayerInput(KeyboardInputMap.Secondary));
                }
                var player1Position = new Vector2((int)(Width * 0.25), (int)(Height * 0.8));
                var player1 = new Player(player1Position, PlayerIndex.One, supportedInputs);
                entities.Add(player1);
            }
            if (numPlayers >= 2)
            {
                var supportedInputs = new List<IPlayerInput> {
                    new ControllerPlayerInput(ControllerInputMap.Primary, PlayerIndex.Two),
                    new KeyboardPlayerInput(KeyboardInputMap.Secondary),
                };
                var player2Position = new Vector2((int)(Width * 0.75), (int)(Height * 0.8));
                var player2 = new Player(player2Position, PlayerIndex.Two, supportedInputs)
                {
                    Color = new Color(70, 90, 100)
                };
                entities.Add(player2);
            }

            // Initialize the enemies - start them in specific positions
            var enemy1Position = new Vector2(
                (int)(gameArea.SidewalkWidth / 2 + gameArea.BorderWidth),
                (int)(Height * 0.2 + SCOREBOARD_HEIGHT + gameArea.BorderWidth)
            );
            var enemy1 = new Enemy(enemy1Position);

            var enemy2Position = new Vector2(
                (int)(Width - gameArea.BorderWidth - gameArea.SidewalkWidth / 2),
                (int)(Height * 0.2 + SCOREBOARD_HEIGHT + gameArea.BorderWidth)
            );
            var enemy2 = new Enemy(enemy2Position);
            entities.Add(enemy1);
            entities.Add(enemy2);

            // Have only one engine idle sound playing no mater how many players
            engineIdleSound = PedestrianGame.Instance.Content.Load<SoundEffect>("Audio/engine-idle").CreateInstance();
            engineIdleSound.IsLooped = true;
            engineIdleSound.Play();

            // Scoreboard displays player scores and time remaining in the game
            var scoreboardArea = new Rectangle(0, 0, Width, SCOREBOARD_HEIGHT);
            scoreboard = new Scoreboard(numPlayers, scoreboardArea);
            scoreboard.Margin = gameArea.SidewalkWidth + gameArea.BorderWidth;
            scoreboard.IsActive = true;

            PedestrianGame.Instance.Events.AddObserver(GameEvents.EnemyKilled, CreateTombstone);
        }

        public void Unload()
        {
            PedestrianGame.Instance.Events.RemoveObserver(GameEvents.EnemyKilled, CreateTombstone);
            scoreboard.RemoveObservers();
            engineIdleSound.Stop();
            entities.ForEach(e => e.Unload());
        }

        private void CreateTombstone(IEntity enemy)
        {
            entities.Add(new Tombstone(enemy.Position));
        }

        public void Update(GameTime gameTime)
        {
            foreach (var entity in entities)
            {
                if (!(entity is Player) || PedestrianGame.Instance.CurrentState != GameState.GameOver)
                {
                    entity.Update(gameTime);
                }
            }
            Collision.Update(entities);
            scoreboard.Update(gameTime);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            scoreboard.Draw(gameTime, spriteBatch);

            foreach (var entity in entities)
            {
                entity.Draw(gameTime, spriteBatch);
                if (PedestrianGame.DEBUG)
                {
                    entity.DrawDebug(gameTime, spriteBatch);
                }
            }
        }
    }
}
