﻿using System;
using System.Linq;
using Android.Util;
using GalaxyBlox.Models;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GalaxyBlox.Objects;
using GalaxyBlox.Static;
using GalaxyBlox.Utils;
using GalaxyBlox.EventArgsClasses;

namespace GalaxyBlox.Rooms
{
    class GameRoom : Room
    {
        private PlayingArena arena;
        private GameObject scoreBoard;
        private GameObject levelBoard;
        private ActorViewer nextActor;

        public GameRoom(RoomManager parent, string name, Size realSize, Size gameSize) : base(parent, name, realSize, gameSize)
        {
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
            Background = content.Load<Texture2D>("Backgrounds/game");
        }

        protected override void AddObjects()
        {
            GameObject objToAdd;
            var leftPanelWidth = 60;
            var padding = 15;

            var btnSize = new Vector2(75); //new Vector2(RoomSize.Width / 4 - 5);
            var btnCount = 4;
            var btnPartSize = (RoomSize.Width - 2f * padding) / btnCount;
            ///// ADDING BACKGROUNDS AND FRAMES /////
            objToAdd = new GameObject(this)
            {
                Size = new Vector2(RoomSize.Width + 10, btnSize.Y + 2 * padding + 5), // +5 correction
                Position = new Vector2(-5, RoomSize.Height - btnSize.Y - 2 * padding),
                BaseColor = Contents.Colors.BackgroundControlsColor,
                BackgroundImage = Contents.Textures.Pix,
                LayerDepth = 0.3f
            };
            objects.Add(objToAdd);

            ///// ADDING CONTROL BUTTONS //////
            objToAdd = new ControlButton(this) // LEFT BUTTON
            {
                Size = btnSize,
                Position = new Vector2(btnPartSize * 0 + padding + ((btnPartSize - btnSize.X) / 2f), RoomSize.Height - padding - btnSize.Y),
                BackgroundImage = Contents.Textures.ControlButton_left,
                LayerDepth = 0.5f
            };
            (objToAdd as Button).Hover += btnLeft_Hover;
            (objToAdd as Button).Release += btnLeft_Release;
            objects.Add(objToAdd);

            objToAdd = new ControlButton(this) // DOWN BUTTON
            {
                Size = btnSize,
                Position = new Vector2(btnPartSize * 1 + padding + ((btnPartSize - btnSize.X) / 2f), RoomSize.Height - padding - btnSize.Y),
                BackgroundImage = Contents.Textures.ControlButton_down,
                LayerDepth = 0.5f
            };
            (objToAdd as Button).Click += btnDown_Click;
            (objToAdd as Button).Hover += btnDown_Hover;
            (objToAdd as Button).Release += btnDown_Release;
            objects.Add(objToAdd);

            objToAdd = new ControlButton(this) // ROTATE BUTTON
            {
                Size = btnSize,
                Position = new Vector2(btnPartSize * 2 + padding + ((btnPartSize - btnSize.X) / 2f), RoomSize.Height - padding - btnSize.Y),
                BackgroundImage = Contents.Textures.ControlButton_rotate,
                LayerDepth = 0.5f
            };
            (objToAdd as Button).Click += btnRotate_Click;
            objects.Add(objToAdd);

            objToAdd = new ControlButton(this) // RIGHT BUTTON
            {
                Size = btnSize,
                Position = new Vector2(btnPartSize * 3 + padding + ((btnPartSize - btnSize.X) / 2f), RoomSize.Height - padding - btnSize.Y),
                BackgroundImage = Contents.Textures.ControlButton_right,
                LayerDepth = 0.5f
            };
            (objToAdd as Button).Hover += btnRight_Hover;
            (objToAdd as Button).Release += btnRight_Release;
            objects.Add(objToAdd);

            var plyArnPosY = padding + 65;
            
            arena = new PlayingArena(this,
                new Vector2(RoomSize.Width - leftPanelWidth - 2 * padding, RoomSize.Height - plyArnPosY - (btnSize.Y + 2 * padding)),
                new Vector2(0, plyArnPosY))
            {
                Name = "main_playground",
                LayerDepth = 0.5f
            };
            arena.ScoreChanged += Arena_ScoreChanged;
            arena.ActorsQueueChanged += Arena_ActorsQueueChanged;
            objects.Add(arena);

            // ADDING LEFT PANEL
            var objectsAlpha = 0.6f;

            ///// ADDING PAUSE BUTTON //////
            leftPanelWidth = (int)(RoomSize.Width - arena.Size.X - 2 * padding);
            var leftPanelPosY = (int)arena.Position.Y;
            objToAdd = new PauseButton(this)
            {
                Size = new Vector2(leftPanelWidth, leftPanelWidth),
                Position = new Vector2(RoomSize.Width - leftPanelWidth - padding, leftPanelPosY),
                BackgroundImage = Contents.Textures.ControlButton_pause,
                LayerDepth = 0.5f
            };
            (objToAdd as Button).Click += btnPause_Click;
            objects.Add(objToAdd);

            //// ADDING LABEL FOR SCORE
            leftPanelPosY += (int)(padding + objToAdd.Size.Y);
            objToAdd = new GameObject(this) // label for Score
            {
                Position = new Vector2(RoomSize.Width - leftPanelWidth - padding, leftPanelPosY),
                Size = new Vector2(leftPanelWidth, 35),
                LayerDepth = 0.5f,
                BackgroundImage = Contents.Textures.Pix,
                BaseColor = Contents.Colors.PanelHeaderBackgroundColor,
                Alpha = objectsAlpha,
                TextAlignment = TextAlignment.Center,
                TextSpriteFont = Contents.Fonts.PanelHeaderText,
                ShowText = true,
                TextColor = Color.White,
                Text = "Skore"
            };
            objects.Add(objToAdd);

            //// ADDING SCORE BOARD
            leftPanelPosY += (int)objToAdd.Size.Y;
            scoreBoard = new GameObject(this)
            {
                Position = new Vector2(RoomSize.Width - leftPanelWidth - padding, leftPanelPosY),
                Size = new Vector2(leftPanelWidth, 55),
                LayerDepth = 0.5f,
                TextAlignment = TextAlignment.Center,
                BackgroundImage = Contents.Textures.Pix,
                BaseColor = Contents.Colors.PanelContentBackgroundColor,
                Alpha = objectsAlpha,
                TextSpriteFont = Contents.Fonts.PanelContentText,
                ShowText = true,
                TextColor = Color.White,
                Text = arena.Score.ToString()
            };
            objects.Add(scoreBoard);

            //// ADDING LABEL FOR LEVEL
            leftPanelPosY += (int)(padding + scoreBoard.Size.Y);
            objToAdd = new GameObject(this) 
            {
                Position = new Vector2(RoomSize.Width - leftPanelWidth - padding, leftPanelPosY),
                Size = new Vector2(leftPanelWidth, 35),
                LayerDepth = 0.5f,
                BackgroundImage = Contents.Textures.Pix,
                BaseColor = Contents.Colors.PanelHeaderBackgroundColor,
                Alpha = objectsAlpha,
                TextAlignment = TextAlignment.Center,
                TextSpriteFont = Contents.Fonts.PanelHeaderText,
                ShowText = true,
                TextColor = Color.White,
                Text = "Level"
            };
            objects.Add(objToAdd);

            //// ADDING LEVEL BOARD
            leftPanelPosY += (int)objToAdd.Size.Y;
            levelBoard = new GameObject(this)
            {
                Position = new Vector2(RoomSize.Width - leftPanelWidth - padding, leftPanelPosY),
                Size = new Vector2(leftPanelWidth, 55),
                LayerDepth = 0.5f,
                TextAlignment = TextAlignment.Center,
                BackgroundImage = Contents.Textures.Pix,
                BaseColor = Contents.Colors.PanelContentBackgroundColor,
                Alpha = objectsAlpha,
                TextSpriteFont = Contents.Fonts.PanelContentText,
                ShowText = true,
                TextColor = Color.White,
                Text = arena.Level.ToString()
            };
            objects.Add(levelBoard);

            //// NEXT ACTOR LABEL
            leftPanelPosY += (int)(padding + levelBoard.Size.Y);
            objToAdd = new GameObject(this)
            {
                Position = new Vector2(RoomSize.Width - leftPanelWidth - padding, leftPanelPosY),
                Size = new Vector2(leftPanelWidth, 35),
                LayerDepth = 0.5f,
                BackgroundImage = Contents.Textures.Pix,
                BaseColor = Contents.Colors.PanelHeaderBackgroundColor,
                Alpha = objectsAlpha,
                TextAlignment = TextAlignment.Center,
                TextSpriteFont = Contents.Fonts.PanelHeaderText,
                ShowText = true,
                TextColor = Color.White,
                Text = "Další"
            };
            objects.Add(objToAdd);

            //// NEXT ACTOR BOARD
            leftPanelPosY += (int)objToAdd.Size.Y;
            nextActor = new ActorViewer(this, new Vector2(leftPanelWidth, leftPanelWidth), Contents.Colors.PanelContentBackgroundColor * 0f, arena.CubeSize)
            {
                Position = new Vector2(RoomSize.Width - leftPanelWidth - padding, leftPanelPosY),
                LayerDepth = 0.5f,
                Alpha = 1f
            };
            objects.Add(nextActor);

            //// ADDING BACKGROUND FOR ACTOR BOARD
            objToAdd = new GameObject(this)
            {
                Size = new Vector2(leftPanelWidth, leftPanelWidth),
                Position = new Vector2(RoomSize.Width - leftPanelWidth - padding, leftPanelPosY),
                BaseColor = Contents.Colors.PanelContentBackgroundColor,
                BackgroundImage = Contents.Textures.Pix,
                LayerDepth = 0.49f,
                Alpha = objectsAlpha,
            };
            objects.Add(objToAdd);

            //// ADDING LABEL BONUS
            leftPanelPosY += (int)(padding + nextActor.Size.Y);
            objToAdd = new GameObject(this)
            {
                Position = new Vector2(RoomSize.Width - leftPanelWidth - padding, leftPanelPosY),
                Size = new Vector2(leftPanelWidth, 35),
                LayerDepth = 0.5f,
                BackgroundImage = Contents.Textures.Pix,
                BaseColor = Contents.Colors.PanelHeaderBackgroundColor,
                Alpha = objectsAlpha,
                TextAlignment = TextAlignment.Center,
                TextSpriteFont = Contents.Fonts.PanelHeaderText,
                ShowText = true,
                TextColor = Color.White,
                Text = "Bonus"
            };
            objects.Add(objToAdd);

            //// ADDING BONUS BTN
            leftPanelPosY += (int)objToAdd.Size.Y;
            objToAdd = new GameObject(this)
            {
                Position = new Vector2(RoomSize.Width - leftPanelWidth - padding, leftPanelPosY),
                Size = new Vector2(leftPanelWidth, leftPanelWidth),
                LayerDepth = 0.5f,
                BackgroundImage = Contents.Textures.Pix,
                BaseColor = Contents.Colors.PanelContentBackgroundColor,
                Alpha = objectsAlpha,
            };
            objects.Add(objToAdd);
        }

        private void Arena_ActorsQueueChanged(object sender, EventArgs e)
        {
            var args = (e as QueueChangeEventArgs);
            nextActor.SetActor(args.NewActor, args.NewActorsColor);
        }

        private void Arena_ScoreChanged(object sender, EventArgs e)
        {
            if (scoreBoard != null)
                scoreBoard.Text = Strings.ScoreToString(arena.Score, 3); // TODO přepočítat na hodnoty, které nebudou mít více než 3 čísla a 1 znak (např.: 128K)

            if (levelBoard != null)
                levelBoard.Text = arena.Level.ToString();
        }

        public override void AfterChangeEvent(string args)
        {
            base.AfterChangeEvent(args);

            if (args == "newGame")
            {
                arena.StartNewGame();
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            ParentRoomManager.ChangeRooms(args: "pause");
        }

        private void btnRotate_Click(object sender, EventArgs e)
        {
            arena.Rotate();
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            var btn = (sender as Button);
            if (btn.HoverTime < 150)
                arena.MakeActorFall();
        }

        private void btnDown_Hover(object sender, EventArgs e)
        {
            arena.MakeActorSpeedup();
        }

        private void btnDown_Release(object sender, EventArgs e)
        {
            arena.SlowDownActor();
        }

        private void btnRight_Hover(object sender, EventArgs e)
        {
            arena.MoveRight();
        }

        private void btnLeft_Hover(object sender, EventArgs e)
        {
            arena.MoveLeft();
        }

        private void btnRight_Release(object sender, EventArgs e)
        {
            arena.StopMovingRight();
        }

        private void btnLeft_Release(object sender, EventArgs e)
        {
            arena.StopMovingLeft();
        }
    }
}