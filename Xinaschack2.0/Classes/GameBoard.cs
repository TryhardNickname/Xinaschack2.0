using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Xinaschack2._0.Classes
{
    class GameBoard
    {
        private List<Rect> RectList { get; set; }
        public List<Player> Players { get; private set; }
        private int RectSelected { get; set; }
        public int CurrentPlayerIndex { get; private set; }
        public bool OnlyDoubleJump { get; set; }
        public int PlanetSelected { get; private set; }
        private int DoubleJumpSaved { get; set; }
        public int SavedPosition { get; set; }

        private int MoveAnimationSpeed { get; set; }
        public Point NewPos { get; set; }
        public Point OldPos { get; set; }
        public bool MoveAnimationComplete { get; private set; }
        public bool TurnStarted { get; private set; }

        public Point OldPosMeteor { get; set; }
        public Point OldPosAlien { get; set; }
        private int AlienCounter { get; set; }
        public bool AlienEncounter { get; set; }
        public List<int> AlienInfoList { get; set; }
        private int AlienAnimationCounter { get; set; }
        private int AlienWhosTurn { get; set; }
        private List<Point> TravelPoints { get; set; }
        private List<int> PlayerIDs { get; set; }

        public List<int> UnavailableRects { get; set; }
        public bool MeteorStrike { get; set; }
        private double MeteorTiming { get; set; }

        private List<int> DoubleJumps { get; set; }
        private List<int> SingleJumps { get; set; }
        private Dictionary<int, List<int>> StartPosDict { get; set; }
        private int TurnCounter { get; set; }
        private int EventTurn { get; set; }

        private readonly double XSameLevelDiff = 45;
        private readonly double XDiff = 22.5; // XSameLevelDiff / 2; // trigonometry
        private readonly double YDiff = 40; // rectsize + 4?
        private readonly double RectSize = 36;
        

        public GameBoard(int width, int height, int amountOfPlayers)
        {
            RectList = new List<Rect>();
            Players = new List<Player>();
            PlayerIDs = new List<int>();
            DoubleJumps = new List<int>();
            SingleJumps = new List<int>();
            StartPosDict = new Dictionary<int, List<int>>();
            UnavailableRects = new List<int>();

            SetupPlayerPosDict();
            MakeRectList(width, height);
            InitPlayerPlanets(amountOfPlayers);

            CurrentPlayerIndex = 0;
            OnlyDoubleJump = false;
            PlanetSelected = -1;
            DoubleJumpSaved = -1;
            SavedPosition = -1;

            TurnCounter = 0;
            Random rnd = new Random();
            EventTurn = rnd.Next(10, 10);
            MeteorStrike = false;
            SetupPlayerID();

            TurnStarted = false;
            MoveAnimationComplete = true;
            MoveAnimationSpeed = 20;
        }

        /// <summary>
        /// This methods draws rectangles so as to make a board that looks like the game field. 
        /// Relative values found in paint.net and these help build the field by knowing the distance between each rectangle on the x-axis for example.
        /// gameArray represents how many spots there are in each "level" of the field, from top to bottom.
        /// </summary>
        private void MakeRectList(int width, int height)
        {

            int[] boardRows = new int[] { 1, 2, 3, 4, 13, 12, 11, 10, 9, 10, 11, 12, 13, 4, 3, 2, 1 };

            double XStart = (width / 2) - (RectSize / 2);
            double boardHeight = YDiff * boardRows.Length;
            double YStart = (height - boardHeight) / 2;

            double XCurrent;
            double YCurrent;

            for (int i = 0; i < boardRows.Length; i++)
            {
                YCurrent = YStart + (YDiff * i);
                XCurrent = XStart - (XDiff * (boardRows[i] - 1));

                for (int j = 0; j < boardRows[i]; j++)
                {
                    Rect newRect = new Rect(XCurrent, YCurrent, RectSize, RectSize);
                    RectList.Add(newRect);

                    XCurrent += XSameLevelDiff;
                }
            }
        }
        /// <summary>
        /// Init function for StarPosDict (for homes & goals)
        /// </summary>
        private void SetupPlayerPosDict()
        {
            StartPosDict.Add(0, new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            StartPosDict.Add(1, new List<int>() { 19, 20, 21, 22, 32, 33, 34, 44, 45, 55 });
            StartPosDict.Add(2, new List<int>() { 74, 84, 85, 95, 96, 97, 107, 108, 109, 110 });
            StartPosDict.Add(3, new List<int>() { 111, 112, 113, 114, 115, 116, 117, 118, 119, 120 });
            StartPosDict.Add(4, new List<int>() { 65, 75, 76, 86, 87, 88, 98, 99, 100, 101 });
            StartPosDict.Add(5, new List<int>() { 10, 11, 12, 13, 23, 24, 25, 35, 36, 46 });
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetupPlayerID()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                foreach (int startPosIndex in StartPosDict.Keys)
                {
                    if (Players[i].PlayerPositions.All(StartPosDict[startPosIndex].Contains))
                    {
                        PlayerIDs.Add(startPosIndex);
                    }
                }
            }
        }

        /// <summary>
        /// Adds 10 indexes for where the planets start-positions is for the 2 players
        /// </summary>
        private void InitPlayerPlanets(int amountOfPlayers)
        {
            switch (amountOfPlayers)
            {
                case 2:
                    Players.Add(new Player(PlanetEnum.Earth, StartPosDict[0], StartPosDict[3]));
                    Players.Add(new Player(PlanetEnum.Jupiter, StartPosDict[3], StartPosDict[0]));
                    break;
                case 3:
                    Players.Add(new Player(PlanetEnum.Earth, StartPosDict[0], StartPosDict[3]));
                    Players.Add(new Player(PlanetEnum.Jupiter, StartPosDict[2], StartPosDict[5]));
                    Players.Add(new Player(PlanetEnum.Mars, StartPosDict[4], StartPosDict[1]));
                    break;
                case 4:
                    Players.Add(new Player(PlanetEnum.Earth, StartPosDict[1], StartPosDict[4]));
                    Players.Add(new Player(PlanetEnum.Jupiter, StartPosDict[2], StartPosDict[5]));
                    Players.Add(new Player(PlanetEnum.Mars, StartPosDict[4], StartPosDict[1]));
                    Players.Add(new Player(PlanetEnum.Moon, StartPosDict[5], StartPosDict[2]));
                    break;
                case 6:
                    Players.Add(new Player(PlanetEnum.Earth, StartPosDict[0], StartPosDict[3]));
                    Players.Add(new Player(PlanetEnum.Jupiter, StartPosDict[1], StartPosDict[4]));
                    Players.Add(new Player(PlanetEnum.Mars, StartPosDict[2], StartPosDict[5]));
                    Players.Add(new Player(PlanetEnum.Moon, StartPosDict[3], StartPosDict[0]));
                    Players.Add(new Player(PlanetEnum.Neptune, StartPosDict[4], StartPosDict[1]));
                    Players.Add(new Player(PlanetEnum.Venus, StartPosDict[5], StartPosDict[2]));
                    break;
                default:
                    break;
            }

        }
        /// <summary>
        /// Used for debugging values while coding.
        /// </summary>
        /// <param name="args"></param>
        public void DebugText(CanvasAnimatedDrawEventArgs args)
        {
            if (PlanetSelected != -1 && PlanetSelected < Players[CurrentPlayerIndex].PlayerPositions.Count)
            {
                args.DrawingSession.DrawText("PlanetSelected Position = " + Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected].ToString(), 50, 100, Windows.UI.Color.FromArgb(255, 90, 255, 170));
            }

            args.DrawingSession.DrawText("RectSelected = " + RectSelected.ToString(), 50, 120, Windows.UI.Color.FromArgb(255, 90, 255, 170));
            args.DrawingSession.DrawText("PlanetSelected = " + PlanetSelected.ToString(), 50, 140, Windows.UI.Color.FromArgb(255, 90, 255, 170));
            args.DrawingSession.DrawText("DoubleJumpSaved = " + DoubleJumpSaved.ToString(), 50, 160, Windows.UI.Color.FromArgb(255, 50, 50, 50));
            args.DrawingSession.DrawText("singleJumps = " + string.Join(" ", SingleJumps.ToArray()), 30, 190, Windows.UI.Color.FromArgb(255, 90, 255, 170));
            args.DrawingSession.DrawText("doubleJumps = " + string.Join(" ", DoubleJumps.ToArray()), 30, 210, Windows.UI.Color.FromArgb(255, 90, 255, 170));

            args.DrawingSession.DrawText("Player1 Positions = " + string.Join(" ", Players[0].PlayerPositions.ToArray()), 10, 230, Windows.UI.Color.FromArgb(255, 90, 255, 170));
            args.DrawingSession.DrawText("Player2 Positions = " + string.Join(" ", Players[1].PlayerPositions.ToArray()), 10, 250, Windows.UI.Color.FromArgb(255, 90, 255, 170));

            args.DrawingSession.DrawText("OldPos = " + OldPos.ToString(), 10, 270, Windows.UI.Color.FromArgb(255, 90, 255, 170));
            args.DrawingSession.DrawText("NewPos = " + NewPos.ToString(), 10, 290, Windows.UI.Color.FromArgb(255, 90, 255, 170));

            //args.DrawingSession.DrawText("XDistance = " + XDistance.ToString(), 10, 340, Windows.UI.Color.FromArgb(255, 50, 50, 50));
            //args.DrawingSession.DrawText("YDistance = " + YDistance.ToString(), 10, 360, Windows.UI.Color.FromArgb(255, 50, 50, 50));
        }

        /// <summary>
        /// Draws green border on selected Rect
        /// </summary>
        /// <param name="args"></param>
        public void DrawSelectedRect(CanvasAnimatedDrawEventArgs args)
        {
            if (RectSelected != -1)
            {
                args.DrawingSession.DrawCircle((float)(RectList[RectSelected].X + RectSize / 2), (float)(RectList[RectSelected].Y + RectSize / 2), (float)RectSize / 2, Windows.UI.Color.FromArgb(128, 0, 255, 0), 5);
            }
        }

        /// <summary>
        /// Loop through the Players list, which is a list of list of ints.
        /// Each player has a list of ints that represents where their planets lie. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void DrawPlayerPlanets(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                for (int posIndex = 0; posIndex < Players[i].PlayerPositions.Count; posIndex++)
                {
                    args.DrawingSession.DrawImage(Players[i].PlanetBitmap, RectList[Players[i].PlayerPositions[posIndex]]);
                }
            }
        }

        /// <summary>
        /// Draws colored circles where the selected Planet is allowed to jump.
        /// </summary>
        /// <param name="args"></param>
        public void DrawOkayMoves(CanvasAnimatedDrawEventArgs args)
        {
            for (int i = 0; i < SingleJumps.Count; i++)
            {
                args.DrawingSession.DrawCircle((float)(RectList[SingleJumps[i]].X + RectSize / 2), (float)(RectList[SingleJumps[i]].Y + RectSize / 2), (float)RectSize / 2, Windows.UI.Color.FromArgb(128, 0, 255, 255), 5);
            }

            for (int i = 0; i < DoubleJumps.Count; i++)
            {
                args.DrawingSession.DrawCircle((float)(RectList[DoubleJumps[i]].X + RectSize / 2), (float)(RectList[DoubleJumps[i]].Y + RectSize / 2), (float)RectSize / 2, Windows.UI.Color.FromArgb(128, 0, 0, 255), 5);
            }
        }

        /// <summary>
        /// Draws the Planet.Color of the current player
        /// </summary>
        /// <param name="args"></param>
        public void DrawPlayerTurn(CanvasAnimatedDrawEventArgs args)
        {
            Rect playerturnRect = new Rect(135, 100, RectSize*2, RectSize*2);

            args.DrawingSession.DrawImage(Players[CurrentPlayerIndex].PlanetBitmap, playerturnRect);
        }
        /// <summary>
        /// Draws the moving planet depending on the updated OldPos.X & Y values (Updates in UpdateMoveAnimation)
        /// </summary>
        /// <param name="args"></param>
        public void DrawMoveAnimation(CanvasAnimatedDrawEventArgs args)
        {
            Rect moveRect = new Rect(OldPos.X, OldPos.Y, RectSize, RectSize);

            args.DrawingSession.DrawImage(Players[CurrentPlayerIndex].PlanetBitmap, moveRect);
        }

        /// <summary>
        /// Draws the fire where the meteor Landed. Uses a List of CanvasBitmap to animate flame.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="FireList"></param>
        public void DrawUnavailableRects(CanvasAnimatedDrawEventArgs args, List<CanvasBitmap> FireList)
        {
            MeteorTiming += args.Timing.ElapsedTime.TotalMilliseconds;
            double framedelay = 66.6664;

            for (int i = 0; i < UnavailableRects.Count; i++)
            {
                if (MeteorTiming < framedelay)
                {
                    args.DrawingSession.DrawImage(FireList[0], RectList[UnavailableRects[i]]);
                }
                else if (MeteorTiming < 2 * framedelay)
                {
                    args.DrawingSession.DrawImage(FireList[1], RectList[UnavailableRects[i]]);
                }
                else if (MeteorTiming < 3 * framedelay)
                {
                    args.DrawingSession.DrawImage(FireList[2], RectList[UnavailableRects[i]]);
                }
                else
                {
                    args.DrawingSession.DrawImage(FireList[3], RectList[UnavailableRects[i]]);
                }

            }
            if (MeteorTiming > 4 * framedelay)
            {
                MeteorTiming = 0;
            }

        }

        /// <summary>
        /// Draws the moving Meteor depending on the updated OldPosMeteor.X & Y values (Updates in UpdateMeteorAnimation)
        /// </summary>
        /// <param name="args"></param>
        /// <param name="comet"></param>
        public void DrawMeteor(CanvasAnimatedDrawEventArgs args, CanvasBitmap comet)
        {
            Rect moveRect = new Rect(OldPosMeteor.X, OldPosMeteor.Y, RectSize, RectSize);

            args.DrawingSession.DrawImage(comet, moveRect);
        }

        /// <summary>
        /// Draws the Alien through a series of points/destinations. Is Offset to appear over planet.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="alien"></param>
        public void DrawAlien(CanvasAnimatedDrawEventArgs args, CanvasBitmap alien)
        {
            int YOffset = 20;

            //planet lifting alien
            if (AlienAnimationCounter > 2 && AlienAnimationCounter < 5)
            {
                Rect planetRect = new Rect(OldPosAlien.X, OldPosAlien.Y, RectSize, RectSize);

                args.DrawingSession.DrawImage(Players[AlienInfoList[0]].PlanetBitmap, planetRect);
            }

            Rect alienRect = new Rect(OldPosAlien.X, OldPosAlien.Y - YOffset, RectSize, RectSize);

            args.DrawingSession.DrawImage(alien, alienRect);

        }

        public void UpdateMoveAnimation()
        {
            double XDistance = NewPos.X - OldPos.X;
            double YDistance = NewPos.Y - OldPos.Y;
            double moveAnimationDistance = Math.Sqrt((XDistance * XDistance) + (YDistance * YDistance));
            if (moveAnimationDistance > 1)
            {
                MoveAnimationComplete = false;
                OldPos = new Point(OldPos.X + (XDistance / MoveAnimationSpeed--), OldPos.Y + (YDistance / MoveAnimationSpeed--)); ;
            }
            else
            {
                MoveAnimationComplete = true;
            }
            if (MoveAnimationSpeed < 5)
            {
                MoveAnimationSpeed = 5;
            }
        }

        public void UpdateMeteor()
        {
            double XDistanceMeteor = RectList[UnavailableRects[0]].X - OldPosMeteor.X;
            double YDistanceMeteor = RectList[UnavailableRects[0]].Y - OldPosMeteor.Y;
            double distanceMeteor = Math.Sqrt((XDistanceMeteor * XDistanceMeteor) + (YDistanceMeteor * YDistanceMeteor));

            if (distanceMeteor > 1)
            {
                OldPosMeteor = new Point(OldPosMeteor.X + (XDistanceMeteor / MoveAnimationSpeed--), OldPosMeteor.Y + (YDistanceMeteor / MoveAnimationSpeed--)); ;
            }
            else
            {
                MeteorStrike = false; // animation complete
            }
            if (MoveAnimationSpeed < 5)
            {
                MoveAnimationSpeed = 5;
            }
        }

        public void UpdateAlien()
        {
            int speedAlien = 13;

            if (AlienAnimationCounter == 0)
            {
                double XDistanceAlien = TravelPoints[0].X - OldPosAlien.X;
                double YDistanceAlien = TravelPoints[0].Y - OldPosAlien.Y;
                double distanceAlien = Math.Sqrt((XDistanceAlien * XDistanceAlien) + (YDistanceAlien * YDistanceAlien));

                if (distanceAlien > 1)
                {
                    OldPosAlien = new Point(TravelPoints[0].X + (XDistanceAlien / speedAlien--), TravelPoints[0].Y + (YDistanceAlien / speedAlien--));
                }
                else
                {
                    // animation complete
                    OldPosAlien = TravelPoints[0];
                    AlienAnimationCounter++;
                }
                if (speedAlien < 5)
                {
                    speedAlien = 5;
                }
            }
            if (AlienAnimationCounter == 1)
            {
                double XDistanceAlien = TravelPoints[1].X - OldPosAlien.X;
                double YDistanceAlien = TravelPoints[1].Y - OldPosAlien.Y;
                double distanceAlien = Math.Sqrt((XDistanceAlien * XDistanceAlien) + (YDistanceAlien * YDistanceAlien));

                if (distanceAlien > 1)
                {
                    OldPosAlien = new Point(OldPosAlien.X + (XDistanceAlien / speedAlien--), OldPosAlien.Y + (YDistanceAlien / speedAlien--));
                }
                else
                {
                    // animation complete   
                    OldPosAlien = TravelPoints[1];
                    AlienAnimationCounter++;   

                }
                if (speedAlien < 5)
                {
                    speedAlien = 5;
                }
            }
            if (AlienAnimationCounter == 2)
            {
                double XDistanceAlien = TravelPoints[2].X - OldPosAlien.X;
                double YDistanceAlien = TravelPoints[2].Y - OldPosAlien.Y;
                double distanceAlien = Math.Sqrt((XDistanceAlien * XDistanceAlien) + (YDistanceAlien * YDistanceAlien));

                if (distanceAlien > 1)
                {
                    OldPosAlien = new Point(OldPosAlien.X + (XDistanceAlien / speedAlien--), OldPosAlien.Y + (YDistanceAlien / speedAlien--));
                }
                else
                {
                    // animation complete
                    Players[AlienInfoList[0]].PlayerPositions.RemoveAt(AlienInfoList[1]);
                    OldPosAlien = TravelPoints[2];
                    AlienAnimationCounter++;
                }
                if (speedAlien < 5)
                {
                    speedAlien = 5;
                }
            }
            if (AlienAnimationCounter == 3)
            {
                {
                    double XDistanceAlien = TravelPoints[3].X - OldPosAlien.X;
                    double YDistanceAlien = TravelPoints[3].Y - OldPosAlien.Y;
                    double distanceAlien = Math.Sqrt((XDistanceAlien * XDistanceAlien) + (YDistanceAlien * YDistanceAlien));

                    if (distanceAlien > 1)
                    {
                        OldPosAlien = new Point(OldPosAlien.X + (XDistanceAlien / speedAlien--), OldPosAlien.Y + (YDistanceAlien / speedAlien--));
                    }
                    else
                    {
                        // animation complete
                        OldPosAlien = TravelPoints[3];
                        AlienAnimationCounter++;
                    }
                    if (speedAlien < 5)
                    {
                        speedAlien = 5;
                    }
                }
            }
            if (AlienAnimationCounter == 4)
            {
                {
                    double XDistanceAlien = TravelPoints[4].X - OldPosAlien.X;
                    double YDistanceAlien = TravelPoints[4].Y - OldPosAlien.Y;
                    double distanceAlien = Math.Sqrt((XDistanceAlien * XDistanceAlien) + (YDistanceAlien * YDistanceAlien));

                    if (distanceAlien > 1)
                    {
                        OldPosAlien = new Point(OldPosAlien.X + (XDistanceAlien / speedAlien--), OldPosAlien.Y + (YDistanceAlien / speedAlien--));
                    }
                    else
                    {
                        // animation complete
                        OldPosAlien = TravelPoints[4];
                        AlienAnimationCounter++;
                        Players[AlienInfoList[0]].PlayerPositions.Insert(AlienInfoList[1], AlienInfoList[2]);
                    }
                    if (speedAlien < 5)
                    {
                        speedAlien = 5;
                    }
                }
            }
            if (AlienAnimationCounter == 5)
            {
                {
                    double XDistanceAlien = TravelPoints[5].X - OldPosAlien.X;
                    double YDistanceAlien = TravelPoints[5].Y - OldPosAlien.Y;
                    double distanceAlien = Math.Sqrt((XDistanceAlien * XDistanceAlien) + (YDistanceAlien * YDistanceAlien));

                    if (distanceAlien > 1)
                    {
                        OldPosAlien = new Point(OldPosAlien.X + (XDistanceAlien / speedAlien--), OldPosAlien.Y + (YDistanceAlien / speedAlien--));
                    }
                    else
                    {
                        OldPosAlien = TravelPoints[5];
                        AlienEncounter = false;
                        AlienAnimationCounter = 0;
                    }
                    if (speedAlien < 5)
                    {
                        speedAlien = 5;
                    }
                }
            }

        }

        /// <summary>
        /// Checks if mouse clicked on a Rectangle(On the game board)
        /// </summary>
        /// <param name="e"></param>
        public void CheckIfRect_Pressed(Point position)
        {
            for (int rectIndex = 0; rectIndex < RectList.Count; rectIndex++)
            {
                if (RectList[rectIndex].Contains(position))
                {
                    // Save which Rect was pressed in prop
                    RectSelected = rectIndex;
                    CheckSelection();
                    break;
                }
            }
        }

        /// <summary>
        /// Checks if selection is planet or empty "rectangle". If previous click was on a Planet (PlanetSelected is >0 (where value is the index of which planet selected,
        /// and this click is on empty rectange, check if the "move" is valid with CheckOKMoves()
        /// </summary>
        private void CheckSelection()
        {
            if (Players[CurrentPlayerIndex].PlayerPositions.Contains(RectSelected)) //rectangle contains a planet == planet is selected
            {
                if (PlanetSelected != -1 && Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected] == RectSelected) // if doubleclick on same planet - move finished!
                {
                    if (PlanetSelected == DoubleJumpSaved) // Only next turn if doubleclick on planet you moved
                    {
                        NextTurn();
                    }
                }
                else
                {
                    PlanetSelected = Players[CurrentPlayerIndex].PlayerPositions.IndexOf(RectSelected);
                    CheckOKMoves();
                }
            }
            else // If rect is emtpy
            {
                if (PlanetSelected != -1) // AND if planet was selected last click 
                {
                    if (SingleJumps.Contains(RectSelected) || DoubleJumps.Contains(RectSelected)) // AND OKmove == move
                    {
                        MovePlanet();
                    }
                    else // random rect slected == deselect Planet
                    {
                        DoubleJumps.Clear();
                        SingleJumps.Clear();
                        PlanetSelected = -1;
                    }
                }
            }
        }

        /// <summary>
        /// Checks in SingleJumps & DoubleJumps list if RectSelected exists in either of them. If singlejump -> Next turn
        /// If doublejump -> disable SingleJumps
        /// </summary>
        private void MovePlanet()
        {
            TurnStarted = true;

            OldPos = new Point(
                RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].X,
                RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].Y
                );

            if (SingleJumps.Contains(RectSelected))
            {
                // Animation starts
                NewPos = new Point(RectList[RectSelected].X, RectList[RectSelected].Y);
                MoveAnimationComplete = false;
                Players[CurrentPlayerIndex].PlayerPositions.RemoveAt(PlanetSelected);
                SavedPosition = -1; // for GameCanvas_Update flag for next turn so it doesnt do next turn with moving back to pos while double jumping.
            }
            else if (DoubleJumps.Contains(RectSelected))
            {
                if (OnlyDoubleJump && PlanetSelected == DoubleJumpSaved) // if onlydoublejump is true, selected HAS to be == doublejump saved (( to prevent doublejumping with other planets)
                {
                    // Animation starts
                    NewPos = new Point(RectList[RectSelected].X, RectList[RectSelected].Y);
                    MoveAnimationComplete = false;
                    Players[CurrentPlayerIndex].PlayerPositions.RemoveAt(PlanetSelected);
                    DoubleJumpSaved = PlanetSelected;

                    if (RectSelected == SavedPosition) // Planet is back to start pos
                    {
                        DoubleJumpSaved = -1;
                        OnlyDoubleJump = false;
                    }
                }
                else if (!OnlyDoubleJump)  // first doublejump
                {
                    SavedPosition = Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected];
                    OnlyDoubleJump = true;

                    // Animation starts
                    NewPos = new Point(RectList[RectSelected].X, RectList[RectSelected].Y);
                    MoveAnimationComplete = false;
                    Players[CurrentPlayerIndex].PlayerPositions.RemoveAt(PlanetSelected);
                    DoubleJumpSaved = PlanetSelected;
                }
            }
        }

        /// <summary>
        /// Increases CurrentPlayerIndex and resets properties. 
        /// Checks if Event is happening this turn.
        /// </summary>
        public void NextTurn()
        {
            CurrentPlayerIndex++;
            if (CurrentPlayerIndex >= Players.Count)
            {
                CurrentPlayerIndex = 0;
            }

            SingleJumps.Clear();
            DoubleJumps.Clear();
            RectSelected = -1;
            PlanetSelected = -1;
            OnlyDoubleJump = false;
            DoubleJumpSaved = -1;
            SavedPosition = -1;
            TurnStarted = false;
            MoveAnimationSpeed = 20;


            TurnCounter++;

            if (EventTurn == TurnCounter)
            {
                UnavailableRects.Clear();

                UnavailableRects.AddRange(CheckOKMovesMeteor());
                //Starts Meteor Animation
                OldPosMeteor = new Point(RectList[UnavailableRects[0]].X - 1000, RectList[UnavailableRects[0]].Y - 1000);
                MeteorStrike = true;

                Random rnd = new Random();
                EventTurn += rnd.Next(10, 11);
                AlienCounter += 1;
            }
            else if (AlienCounter == 2)
            {
                if (AlienWhosTurn == Players.Count)
                {
                    AlienWhosTurn = 0;
                }
                AlienCounter = 0;
                CheckOKAlienMoves();
                AlienWhosTurn++;
            }

        }

        private void CheckOKAlienMoves()
        {
            List<int> AllPlayerPositions = new List<int>();
            List<int> StartPosExcluded = new List<int>();
            Random rnd = new Random();
            int whichPlayer = AlienWhosTurn;
            int whichPlanet = rnd.Next(0, 10);
            int whereToGoBack = StartPosDict[PlayerIDs[whichPlayer]][rnd.Next(0, 10)];

            
            foreach (int index in PlayerIDs)
            {
                StartPosExcluded.AddRange(StartPosDict[index]);
            }

            if (Players[whichPlayer].PlayerPositions.All(position => StartPosExcluded.Contains(position)))
            {
                return;
            }

            while (StartPosExcluded.Contains(Players[whichPlayer].PlayerPositions[whichPlanet]))
            {
                whichPlanet = rnd.Next(0, 10);
                // REMOVED TRYCOUNTER
            }

            // This whole block runs if the spawn bases do not contain the chosen planet to move. 
            if (!StartPosExcluded.Contains(Players[whichPlayer].PlayerPositions[whichPlanet]))
            {
                // Add playerpositions to a list.
                foreach (Player player in Players)
                {
                    AllPlayerPositions.AddRange(player.PlayerPositions);
                }

                // Choose a new point until you hit an empty rectangle
                while (AllPlayerPositions.Contains(whereToGoBack))
                {
                    whereToGoBack = StartPosDict[PlayerIDs[whichPlayer]][rnd.Next(0, 10)];
                }

                // This is used in other places, such as in DrawAlien and UpdateAlien
                AlienInfoList = new List<int>() { whichPlayer, whichPlanet, whereToGoBack };

                OldPosAlien = new Point(RectList[Players[AlienInfoList[0]].PlayerPositions[AlienInfoList[1]]].X, RectList[Players[AlienInfoList[0]].PlayerPositions[AlienInfoList[1]]].Y);
                // Points for the alien to go through when it picks up and drops a planet
                TravelPoints = new List<Point>();
                TravelPoints.Add(new Point(0, 0));
                TravelPoints.Add(new Point(OldPosAlien.X, OldPosAlien.Y - 65));
                TravelPoints.Add(new Point(OldPosAlien.X, OldPosAlien.Y));
                TravelPoints.Add(new Point(RectList[AlienInfoList[2]].X, RectList[AlienInfoList[2]].Y - 65));
                TravelPoints.Add(new Point(RectList[AlienInfoList[2]].X, RectList[AlienInfoList[2]].Y));
                TravelPoints.Add(new Point(0, 0));

                AlienEncounter = true;
            }
            else
            {
                AlienEncounter = false;
            }

        }

        /// <summary>
        /// Randoms a position in rectlist and chekcs if nothing is blocking, until we find an OK position. 
        /// Can't crash due to there always being the previous position available if all else are blocked.
        /// </summary>
        /// <returns></returns>
        private List<int> CheckOKMovesMeteor()
        {
            List<int> FirePositions = new List<int>();
            List<int> StartPosExcluded = new List<int>();
            List<int> PlayerPos = new List<int>();
            List<int> BlockingRects = new List<int>();
            foreach (List<int> list in StartPosDict.Values)
            {
                StartPosExcluded.AddRange(list);
            }

            foreach (Player p in Players)
            {
                PlayerPos.AddRange(p.PlayerPositions);
            }
            BlockingRects.AddRange(StartPosExcluded);
            BlockingRects.AddRange(PlayerPos);

            Random rnd = new Random();

            int randomMeteorPos;
            bool _blocking = true;

            //while loop originally
            //HMM LOOK AT THIS
            for (int k = 0; k < 100; k++)
            {
                randomMeteorPos = rnd.Next(0, 120);
                List<Point> points = new List<Point> {
                new Point //top right
                {
                    X = RectList[randomMeteorPos].X + XDiff,
                    Y = RectList[randomMeteorPos].Y - YDiff
                },
                new Point // right
                {
                    X = RectList[randomMeteorPos].X + XSameLevelDiff,
                    Y = RectList[randomMeteorPos].Y
                },
                new Point //bot right
                {
                    X = RectList[randomMeteorPos].X + XDiff,
                    Y = RectList[randomMeteorPos].Y + YDiff
                }};

                FirePositions.Add(randomMeteorPos);
                for (int i = 0; i < RectList.Count; i++)
                {
                    for (int j = 0; j < points.Count; j++) // rnd.Next(0, points.Count)
                    {
                        if (RectList[i].Contains(points[j]))
                        {
                            FirePositions.Add(i);
                        }
                    }
                }

                if (BlockingRects.Any(FirePositions.Contains))
                {
                    _blocking = true;
                    FirePositions.Clear();
                }
                else
                {
                    _blocking = false;
                    break;
                }
            }
            return FirePositions;

        }

        /// <summary>
        /// Inserts planet that was being animated. Sets Turnstarted to False.
        /// </summary>
        public void MoveComplete()
        {
            if (Players[CurrentPlayerIndex].PlayerPositions.Count < 10)
            {
                Players[CurrentPlayerIndex].PlayerPositions.Insert(PlanetSelected, RectSelected);
            }
            TurnStarted = false;
        }

        /// <summary>
        /// Check which places you can move with PlanetSelected. Looks at the 6 surrounding rectangles
        /// If planet is next to PlanetSelected => check if jump is possible
        /// </summary>
        /// <returns></returns>
        public void CheckOKMoves()
        {
            DoubleJumps.Clear();
            SingleJumps.Clear();

            // Each point represents a jump to each direction available on each rectangle. 
            List<Point> points = new List<Point> {
                new Point // top left
                {
                    X = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].X - XDiff,
                    Y = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].Y - YDiff
                },
                new Point //top right
                {
                    X = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].X + XDiff,
                    Y = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].Y - YDiff
                },
                new Point // right
                {
                    X = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].X + XSameLevelDiff,
                    Y = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].Y
                },
                new Point //bot right
                {
                    X = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].X + XDiff,
                    Y = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].Y + YDiff
                },
                new Point // bot left
                {
                    X = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].X - XDiff,
                    Y = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].Y + YDiff
                },
                new Point // left
                {
                    X = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].X - XSameLevelDiff,
                    Y = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].Y
                }};

            List<int> PlayerPos = new List<int>();

            foreach (Player p in Players)
            {
                PlayerPos.AddRange(p.PlayerPositions);
            }


            // Loop through each rectangle in gamefield to find the rectangles that contain a Point. 
            // A Point is made by going one rectangle in each direction.
            // If Rectangle is not occupied by another Planet -> add to list.
            for (int i = 0; i < RectList.Count; i++)
            {
                for (int j = 0; j < points.Count; j++)
                {
                    if (RectList[i].Contains(points[j])) // Finds index for the six positions closest to PlanetSelected
                    {
                        // if true -> RectList[i] is one of the six singlejump positions

                        if (!PlayerPos.Contains(i) && !UnavailableRects.Contains(i)) // checks if ANY planets are blocking OR meteorblocking
                        {
                            SingleJumps.Add(i);
                        }
                        else
                        {
                            // If you have a planet that exists in the PlayerPos list, which contains indexes of positions of planets,
                            // do the switch case which does an operation to find out if the rectangle behind the current
                            // rectangle is available. ( a jump pver the planet that is "blocking" )

                            Point jumpPoint = points[j]; // by using "j" , we look in the same direction
                            // check if you can jump over this planet
                            switch (j)
                            {
                                case 0:
                                    jumpPoint.X -= XDiff;
                                    jumpPoint.Y -= YDiff;

                                    for (int k = i; k >= 0; k--)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !PlayerPos.Contains(k) && !UnavailableRects.Contains(k))
                                        {
                                            DoubleJumps.Add(k);
                                        }
                                    }
                                    break;

                                case 1:
                                    jumpPoint.X += XDiff;
                                    jumpPoint.Y -= YDiff;

                                    for (int k = i; k >= 0; k--)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !PlayerPos.Contains(k) && !UnavailableRects.Contains(k))
                                        {
                                            DoubleJumps.Add(k);
                                        }
                                    }
                                    break;
                                case 2:
                                    jumpPoint.X += XSameLevelDiff;

                                    for (int k = i; k < RectList.Count; k++)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !PlayerPos.Contains(k) && !UnavailableRects.Contains(k))
                                        {
                                            DoubleJumps.Add(k);
                                        }
                                    }
                                    break;
                                case 3:
                                    jumpPoint.X += XDiff;
                                    jumpPoint.Y += YDiff;

                                    for (int k = i; k < RectList.Count; k++)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !PlayerPos.Contains(k) && !UnavailableRects.Contains(k))
                                        {
                                            DoubleJumps.Add(k);
                                        }
                                    }
                                    break;
                                case 4:
                                    jumpPoint.X -= XDiff;
                                    jumpPoint.Y += YDiff;

                                    for (int k = i; k < RectList.Count; k++)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !PlayerPos.Contains(k) && !UnavailableRects.Contains(k))
                                        {
                                            DoubleJumps.Add(k);
                                        }
                                    }
                                    break;
                                case 5:
                                    jumpPoint.X -= XDiff;

                                    for (int k = i; k >= 0; k--)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !PlayerPos.Contains(k) && !UnavailableRects.Contains(k))
                                        {
                                            DoubleJumps.Add(k);
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }

                    }
                }
            }

            if (OnlyDoubleJump) // show no singleJumps if only doublejumps
            {
                SingleJumps.Clear();
                if (DoubleJumpSaved != PlanetSelected) // show no doublejumps if wrong planet selected
                {
                    DoubleJumps.Clear();
                }
            }

        }

        public bool CheckIfWin()
        {
            foreach (Player player in Players)
            {
                int correctPos = 0;
                for (int i = 0; i < player.PlayerPositions.Count; i++)
                {
                    if (player.WinPositions.Contains(player.PlayerPositions[i]))
                    {
                        correctPos++;
                    }
                }

                if (correctPos == 10)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
