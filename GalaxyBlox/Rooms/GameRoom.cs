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
using Microsoft.Xna.Framework.Input;
using static GalaxyBlox.Static.SettingOptions;
using System.Collections.Generic;
using GalaxyBlox.Objects.PlayingArenas;

namespace GalaxyBlox.Rooms
{
    class GameRoom : Room
    {
        private PlayingArena arena;

        private GameObject lblScore;
        private GameObject lblLevel;

        private ActorViewer actorViewer;

        private Button btnPause;
        private Button btnControlLeft;
        private Button btnControlRight;
        private Button btnControlFall;
        private Button btnControlRotate;

        private GameObject pnlBonusBtns;
        private List<GameObject> bonusObjs;

        private SettingOptions.GameMode gameMode;

        public GameRoom(Room parent, string name, Vector2 size, Vector2 position) : base(parent, name, size, position)
        {
            Initialize();
        }

        public GameRoom(string name, Vector2 size, Vector2 position) : base(name, size, position)
        {
            Initialize();
        }

        protected override void Initialize()
        {
            gameMode = Settings.Game.UserSettings.LastGameMode;
            FullScreen = true;
            Background = Contents.Textures.BackgroundGame;

            GameObject lastObj;

            var playingArenaPadding = 16;

            // ADDING FIRST PANEL
            // PANEL BACKGROUND
            Objects.Add(new GameObject(this)
            {
                Size = new Vector2(712, 132),
                Position = new Vector2(4, 4),
                BackgroundImage = Contents.Textures.GameUI_top_background,
                LayerDepth = 0.02f
            });
            lastObj = Objects.Last();
            var playingArenaStart = lastObj.Position.Y + lastObj.Size.Y + playingArenaPadding;

            // ACTOR VIEWER
            var viewerPos = new Vector2(lastObj.Position.X + 16, lastObj.Position.Y + 16);
            var viewerSize = new Vector2(116, 100);
            Objects.Add(new DynamicBackgroundObject(this, Contents.Textures.Dialog_inside, 4)
            {
                Size = viewerSize,
                Position = viewerPos,
                LayerDepth = 0.045f
            });
            actorViewer = new ActorViewer(this, viewerSize, Contents.Colors.ActorViewerBackgroundColor)
            {
                Position = viewerPos,
                LayerDepth = 0.05f
            };
            Objects.Add(actorViewer);

            var btnPauseSize = new Vector2(116, 100);
            // PAUSE BUTTON
            btnPause = Bank.Buttons.GetPauseButton(this);
            btnPause.Size = btnPauseSize;
            btnPause.Position = new Vector2(lastObj.Position.X + lastObj.Size.X - btnPauseSize.X - 16, lastObj.Position.Y + 16);
            btnPause.Click += btnPause_Click;
            Objects.Add(btnPause);
            
            // SETTINGS 
            var lblScoreSize = new Vector2(416, 40);
            var lblLevelSize = new Vector2(416, 24);
            var scoreLineSize = new Vector2(284, 4);

            // SCORE
            lblScore = new GameObject(this)
            {
                Size = lblScoreSize,
                Position = new Vector2(148, lastObj.Position.Y + 24),
                TextSpriteFont = Contents.Fonts.PixelArtTextFont,
                Text = "SCORE",
                TextHeight = (int)lblScoreSize.Y,
                TextAlignment = TextAlignment.Center,
                TextColor = Color.White,
                ShowText = true,
                LayerDepth = 0.05f
            };
            Objects.Add(lblScore);
            
            // LINE BETWEEN SCORE AND LEVEL
            Objects.Add(new GameObject(this)
            {
                BackgroundImage = Contents.Textures.Pix,
                BaseColor = Color.White,
                Size = scoreLineSize,
                Position = new Vector2(148 + (416 - scoreLineSize.X) / 2, lastObj.Position.Y + 72),
                LayerDepth = 0.05f
            });
            
            // LEVEL
            lblLevel = new GameObject(this)
            {
                Size = lblLevelSize,
                Position = new Vector2(148, lastObj.Position.Y + 88),
                TextSpriteFont = Contents.Fonts.PixelArtTextFont,
                Text = "LEVEL",
                TextHeight = (int)lblLevelSize.Y,
                TextAlignment = TextAlignment.Center,
                TextColor = Color.White,
                ShowText = true,
                LayerDepth = 0.05f
            };
            Objects.Add(lblLevel);

            // ADDING SECOND PANEL
            // SETTINGS
            var btnSize = new Vector2(152, 112);

            // PANEL BACKGROUND
            float playingArenaEndY = 0;
            float controlButtonsStartY = 0;
            if (gameMode == SettingOptions.GameMode.Classic)
            {
                Objects.Add(new GameObject(this)
                {
                    Size = new Vector2(712, 144),
                    Position = new Vector2(4, Size.Y - 144 - 4),
                    BackgroundImage = Contents.Textures.GameUI_bottom_classic_background,
                    LayerDepth = 0.02f
                });
                lastObj = Objects.Last();
                playingArenaEndY = lastObj.Position.Y - playingArenaPadding;
                controlButtonsStartY = lastObj.Position.Y + 16;
            }
            else
            {
                Objects.Add(new GameObject(this)
                {
                    Size = new Vector2(712, 224),
                    Position = new Vector2(4, Size.Y - 224 - 4),
                    BackgroundImage = Contents.Textures.GameUI_bottom_normal_background,
                    LayerDepth = 0.02f
                });
                lastObj = Objects.Last();
                playingArenaEndY = lastObj.Position.Y - playingArenaPadding;
                controlButtonsStartY = lastObj.Position.Y + 96;

                // ADDING BONUS PANEL
                pnlBonusBtns = new GameObject(this)
                {
                    Size = new Vector2(680, 60),
                    Position = new Vector2(lastObj.Position.X + 16, lastObj.Position.Y + 16),
                    LayerDepth = 0.02f,
                    //BackgroundImage = Contents.Textures.Pix,
                    //BaseColor = Color.Red
                };
                Objects.Add(pnlBonusBtns);
                bonusObjs = new List<GameObject>();
            }

            // CONTROL BUTTON LEFT
            btnControlLeft = Bank.Buttons.GetControlButton(this);
            btnControlLeft.BackgroundImage = Contents.Textures.ControlButton_left;
            btnControlLeft.Size = btnSize;
            btnControlLeft.Position = new Vector2(lastObj.Position.X + 16, controlButtonsStartY);
            btnControlLeft.Release += btnLeft_Release;
            btnControlLeft.Hover += btnLeft_Hover;
            Objects.Add(btnControlLeft);

            // CONTROL BUTTON FALL
            btnControlFall = Bank.Buttons.GetControlButton(this);
            btnControlFall.BackgroundImage = Contents.Textures.ControlButton_fall;
            btnControlFall.Size = btnSize;
            btnControlFall.Position = new Vector2(lastObj.Position.X + 192, controlButtonsStartY);
            btnControlFall.Click += btnDown_Click;
            btnControlFall.Hover += btnDown_Hover;
            btnControlFall.Release += btnDown_Release;
            Objects.Add(btnControlFall);

            // CONTROL BUTTON ROTATE
            btnControlRotate = Bank.Buttons.GetControlButton(this);
            btnControlRotate.BackgroundImage = Contents.Textures.ControlButton_rotate;
            btnControlRotate.Size = btnSize;
            btnControlRotate.Position = new Vector2(lastObj.Position.X + 368, controlButtonsStartY);
            btnControlRotate.Click += btnRotate_Click;
            Objects.Add(btnControlRotate);

            // CONTROL BUTTON RIGHT
            btnControlRight = Bank.Buttons.GetControlButton(this);
            btnControlRight.BackgroundImage = Contents.Textures.ControlButton_right;
            btnControlRight.Size = btnSize;
            btnControlRight.Position = new Vector2(lastObj.Position.X + 544, controlButtonsStartY);
            btnControlRight.Release += btnRight_Release;
            btnControlRight.Hover += btnRight_Hover;
            Objects.Add(btnControlRight);

            // ADDING PLAYING ARENA
            var arenaSize = new Vector2(Size.X - 2 * playingArenaPadding, playingArenaEndY - playingArenaStart);
            var arenaPosition = new Vector2(playingArenaPadding, playingArenaStart);
            switch (gameMode)
            {
                case GameMode.Classic:
                    {
                        arena = new PlayingArena_Classic(this, arenaSize, arenaPosition);
                    } break;
                case GameMode.Normal:
                    {
                        arena = new PlayingArena_Normal(this, arenaSize, arenaPosition);
                        (arena as PlayingArena_Normal).AvailableBonusesChanged += Arena_AvailableBonusesChanged;
                        (arena as PlayingArena_Normal).ActiveBonusChanged += Arena_ActiveBonusChanged;
                    } break;
                case GameMode.Extreme:
                    {
                        arena = new PlayingArena_Extreme(this, arenaSize, arenaPosition);
                        (arena as PlayingArena_Extreme).AvailableBonusesChanged += Arena_AvailableBonusesChanged;
                        (arena as PlayingArena_Extreme).ActiveBonusChanged += Arena_ActiveBonusChanged;
                    } break;
            }

            arena.LayerDepth = 0.05f;
            arena.ActorsQueueChanged += Arena_ActorsQueueChanged;
            arena.ScoreChanged += Arena_ScoreChanged;
            arena.GameEnded += Arena_GameEnded;
            arena.StartNewGame();
            Objects.Add(arena);

            // adding border for arena
            var borderOffset = new Vector2(12);
            Objects.Add(new DynamicBackgroundObject(this, Contents.Textures.GameUI_playingArena_border, 4)
            {
                Position = new Vector2(arena.Position.X - borderOffset.X, arena.Position.Y - borderOffset.Y),
                Size = new Vector2(arena.Size.X + 2 * borderOffset.X, arena.Size.Y + 2 * borderOffset.Y),
                LayerDepth = 0.051f
            });

            //// ADDING STAR SYSTEM ////
            Objects.Add(new StarSystem(this, new Vector2(680, 1200), new Vector2(20, 40)));
            lastObj = Objects.Last();
            lastObj.LayerDepth = 0.019f;
            (lastObj as StarSystem).Start(218884, 1, 3, 3, 5, 15);
            (lastObj as StarSystem).MaxTimer = 5000;
            (lastObj as StarSystem).Alpha = 0.8f;
        }

        private void Arena_ActiveBonusChanged(object sender, EventArgs e)
        {
            var eventArgs = (ActiveBonusChangedEventArgs)e;
            if (eventArgs.ActiveBonus != null)
                PrepareInterfaceForBonus(eventArgs.ActiveBonus.Type);
            else
                PrepareInterfaceForBonus(BonusType.None);
        }

        private void Arena_AvailableBonusesChanged(object sender, EventArgs e)
        {
            var eventArgs = (AvailableBonusesChangeEventArgs)e;
            RefreshBonusButtons(eventArgs.GameBonuses);
        }

        private void RefreshBonusButtons(List<GameBonus> newBonuses)
        {
            // remove old bonus buttons
            foreach (var obj in bonusObjs)
                Objects.Remove(obj);
            bonusObjs.Clear();

            // add new bonus buttons
            var btnBonusSize = new Vector2(160, 60);
            var btnBonusTextHeight = (int)(btnBonusSize.Y * 0.35f);
            var btnMargin = (pnlBonusBtns.Size.X - (newBonuses.Count * btnBonusSize.X)) / (newBonuses.Count + 1);

            var i = 0;
            foreach (var bonus in newBonuses)
            {
                if (bonus.Type != BonusType.None && bonus.Progress >= 100)
                {
                    var btn = Bank.Buttons.GetEmptyButton(this);
                    btn.Name = "BonusButton_" + i.ToString();
                    btn.Size = btnBonusSize;
                    btn.Position = new Vector2(pnlBonusBtns.Position.X + btnMargin + i * (btnBonusSize.X + btnMargin), pnlBonusBtns.Position.Y);
                    btn.BackgroundImage = Contents.Textures.Button_bonus;
                    //btn.LayerDepth = 0.052f;
                    btn.TextHeight = btnBonusTextHeight;
                    btn.Text = bonus.SpecialText;
                    btn.Data = newBonuses[i];
                    btn.Click += Btn_Click;

                    Objects.Add(btn);
                    bonusObjs.Add(btn);
                }
                else
                {
                    var btnBack = new GameObject(this)
                    {
                        Size = btnBonusSize,
                        Position = new Vector2(pnlBonusBtns.Position.X + btnMargin + i * (btnBonusSize.X + btnMargin), pnlBonusBtns.Position.Y),
                        BackgroundImage = Contents.Textures.Pix,
                        BaseColor = new Color(11, 42, 96),
                        LayerDepth = 0.05f,
                        TextSpriteFont = Contents.Fonts.PixelArtTextFont,
                        TextColor = new Color(44, 110, 224),
                        TextHeight = btnBonusTextHeight,
                        TextAlignment = TextAlignment.Center,
                        Text = bonus.Progress.ToString() + "%",
                        ShowText = true
                    };
                    Objects.Add(btnBack);
                    bonusObjs.Add(btnBack);
                }
                i++;
            }

            //if (newBonuses.Count > 1)
            //{
            //    var lineWidth = 8;
            //    var linesMargin = (pnlBonusBtns.Size.X - ((newBonuses.Count - 1) * lineWidth)) / (newBonuses.Count);
            //    for (int index = 0; index < newBonuses.Count; index++)
            //    {
            //        var line = new GameObject(this)
            //        {
            //            Size = new Vector2(lineWidth, pnlBonusBtns.Size.Y),
            //            Position = new Vector2(pnlBonusBtns.Position.X + linesMargin + index * (lineWidth + linesMargin), pnlBonusBtns.Position.Y),
            //            BackgroundImage = Contents.Textures.Pix,
            //            BaseColor = new Color(11, 42, 96),
            //            LayerDepth = 0.05f
            //    };
            //        Objects.Add(line);
            //        bonusObjs.Add(line);
            //    }
            //}
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            var btn = (sender as Button);
            if (btn != null)
            {
                var bonus = (btn.Data as GameBonus);
                PrepareInterfaceForBonus(bonus.Type);

                int? position = null;
                int tmp;
                if (!String.IsNullOrEmpty(btn.Name) && btn.Name.StartsWith("BonusButton_") && int.TryParse(btn.Name.Replace("BonusButton_", ""), out tmp))
                    position = tmp;

                (arena as PlayingArena_Normal).Control_ActivateBonus(bonus, position);
            }
                
        }

        private void PrepareInterfaceForBonus(BonusType bonus)
        {
            switch(bonus)
            {
                case BonusType.None:
                case BonusType.TimeSlowdown:
                    {
                        btnControlLeft.Enabled = true;
                        btnControlRight.Enabled = true;
                        btnControlFall.Enabled = true;
                        btnControlRotate.Enabled = true;
                    } break;
                case BonusType.Laser:
                    {
                        btnControlRotate.Enabled = false;
                    } break;
                case BonusType.SwipeCubes:
                    {
                        btnControlRotate.Enabled = false;
                        btnControlFall.Enabled = false;
                    } break;
            }
        }
        
        private void Arena_GameEnded(object sender, EventArgs e)
        {
            if (Parent != null)
                this.End();
            else
            {
                arena.StartNewGame();
            }
        }

        private void Arena_ActorsQueueChanged(object sender, EventArgs e)
        {
            var args = (e as QueueChangeEventArgs);
            actorViewer.SetActor(args.NewActor, args.NewActorsColor);
        }

        private void Arena_ScoreChanged(object sender, EventArgs e)
        {
            if (lblScore != null)
            {
                if (arena.Score > 0)
                    lblScore.Text = Strings.ScoreToLongString(arena.Score); //Strings.ScoreToString(arena.Score, 3); 
                else
                    lblScore.Text = "SCORE";
            }

            if (lblLevel != null)
            {
                if (arena.Level > 0)
                    lblLevel.Text = arena.Level.ToString();
                else
                    lblLevel.Text = "LEVEL";
            }
        }

        protected override void HandleBackButton()
        {
            btnPause_Click(this, new EventArgs());
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (Parent != null)
                Close();
        }

        private void btnRotate_Click(object sender, EventArgs e)
        {
            arena.ControlRotate_Click();
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            var btn = (sender as Button);
            if (btn.HoverTime < 150)
                arena.ControlDown_Click();
        }

        private void btnDown_Hover(object sender, EventArgs e)
        {
            arena.ControlDown_Down();
        }

        private void btnDown_Release(object sender, EventArgs e)
        {
            arena.ControlDown_Up();
        }

        private void btnRight_Hover(object sender, EventArgs e)
        {
            arena.ControlRight_Down();
        }

        private void btnLeft_Hover(object sender, EventArgs e)
        {
            arena.ControlLeft_Down();
        }

        private void btnRight_Release(object sender, EventArgs e)
        {
            arena.ControlRight_Up();
        }

        private void btnLeft_Release(object sender, EventArgs e)
        {
            arena.ControlLeft_Up();
        }
    }
}