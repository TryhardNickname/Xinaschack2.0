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
        private List<int> DoubleJumps { get; set; }
        private List<int> SingleJumps { get; set; }
        private Dictionary<int, List<int>> StartPosDict { get; set; }
        private int TurnCounter { get; set; }
        private int EventTurn { get; set; }

        // Planet animation props
        private int MoveAnimationSpeed { get; set; }
        public Point NewPos { get; set; }
        public Point OldPos { get; set; }
        public bool MoveAnimationComplete { get; private set; }
        public bool TurnStarted { get; private set; }

        // Alien props
        public Point OldPosMeteor { get; set; }
        public Point OldPosAlien { get; set; }
        private int AlienEventCounter { get; set; }
        public bool AlienEncounter { get; set; }
        public List<int> AlienInfoList { get; set; }
        private int AlienAnimationCounter { get; set; }
        private int AlienWhosTurn { get; set; }
        private List<Point> TravelPoints { get; set; }
        private List<int> PlayerIDs { get; set; }

        // Meteor props
        public List<int> UnavailableRects { get; set; }
        public bool MeteorStrike { get; set; }
        private double MeteorTiming { get; set; }

        // NYI refactor Alien/Meteor to separate classes.
        // NYI refactor Move/Alien/Meteor Animation methods from 3 separate to 1.

        // Hardcoded for 1080p fullscreen. xDiff = xSameLevelDiff / 2 due to trigonometry. 
        private readonly double _xSameLevelDiff = 45;
        private readonly double _xDiff = 22.5;
        private readonly double _yDiff = 40;
        private readonly double _rectSize = 36;

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
            AlienWhosTurn = rnd.Next(0, amountOfPlayers);
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

            double xStart = (width / 2) - (_rectSize / 2);
            double boardHeight = _yDiff * boardRows.Length;
            double yStart = (height - boardHeight) / 2;

            double xCurrent;
            double yCurrent;

            for (int i = 0; i < boardRows.Length; i++)
            {
                yCurrent = yStart + (_yDiff * i);
                xCurrent = xStart - (_xDiff * (boardRows[i] - 1));

                for (int j = 0; j < boardRows[i]; j++)
                {
                    Rect newRect = new Rect(xCurrent, yCurrent, _rectSize, _rectSize);
                    RectList.Add(newRect);

                    xCurrent += _xSameLevelDiff;
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
        /// This method sets up a List of ints so that other methods can use the proper indexes of players rather than just the index when its their turn.
        /// For example, if you have two players then player two's index should be 3 since if it was 1, that player would be represented by the second base, which is not correct.
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
        /// Adds 10 indexes for where the planets start-positions is for the 2-6 players
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

            args.DrawingSession.DrawText("AlienAnimationCounter = " + AlienAnimationCounter.ToString(), 10, 340, Windows.UI.Color.FromArgb(255, 50, 50, 50));
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
                args.DrawingSession.DrawCircle((float)(RectList[RectSelected].X + _rectSize / 2), (float)(RectList[RectSelected].Y + _rectSize / 2), (float)_rectSize / 2, Windows.UI.Color.FromArgb(128, 0, 255, 0), 5);
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
                args.DrawingSession.DrawCircle((float)(RectList[SingleJumps[i]].X + _rectSize / 2), (float)(RectList[SingleJumps[i]].Y + _rectSize / 2), (float)_rectSize / 2, Windows.UI.Color.FromArgb(128, 0, 255, 255), 5);
            }

            for (int i = 0; i < DoubleJumps.Count; i++)
            {
                args.DrawingSession.DrawCircle((float)(RectList[DoubleJumps[i]].X + _rectSize / 2), (float)(RectList[DoubleJumps[i]].Y + _rectSize / 2), (float)_rectSize / 2, Windows.UI.Color.FromArgb(128, 0, 0, 255), 5);
            }
        }

        /// <summary>
        /// Draws the Planet.Color of the current player
        /// </summary>
        /// <param name="args"></param>
        public void DrawPlayerTurn(CanvasAnimatedDrawEventArgs args)
        {
            Rect playerTurnRect = new Rect(135, 100, _rectSize * 2, _rectSize * 2);

            args.DrawingSession.DrawImage(Players[CurrentPlayerIndex].PlanetBitmap, playerTurnRect);
        }

        /// <summary>
        /// Draws the moving planet depending on the updated OldPos.X & Y values (Updates in UpdateMoveAnimation)
        /// </summary>
        /// <param name="args"></param>
        public void DrawMoveAnimation(CanvasAnimatedDrawEventArgs args)
        {
            Rect moveRect = new Rect(OldPos.X, OldPos.Y, _rectSize, _rectSize);

            args.DrawingSession.DrawImage(Players[CurrentPlayerIndex].PlanetBitmap, moveRect);
        }

        /// <summary>
        /// Draws the fire where the meteor Landed. Uses a List of CanvasBitmap to animate flame.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="fireList"></param>
        public void DrawUnavailableRects(CanvasAnimatedDrawEventArgs args, List<CanvasBitmap> fireList)
        {
            MeteorTiming += args.Timing.ElapsedTime.TotalMilliseconds;
            double frameDelay = 66.6664;

            for (int i = 0; i < UnavailableRects.Count; i++)
            {
                if (MeteorTiming < frameDelay)
                {
                    args.DrawingSession.DrawImage(fireList[0], RectList[UnavailableRects[i]]);
                }
                else if (MeteorTiming < 2 * frameDelay)
                {
                    args.DrawingSession.DrawImage(fireList[1], RectList[UnavailableRects[i]]);
                }
                else if (MeteorTiming < 3 * frameDelay)
                {
                    args.DrawingSession.DrawImage(fireList[2], RectList[UnavailableRects[i]]);
                }
                else
                {
                    args.DrawingSession.DrawImage(fireList[3], RectList[UnavailableRects[i]]);
                }

            }
            if (MeteorTiming > 4 * frameDelay)
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
            Rect moveRect = new Rect(OldPosMeteor.X, OldPosMeteor.Y, _rectSize, _rectSize);

            args.DrawingSession.DrawImage(comet, moveRect);
        }

        /// <summary>
        /// Draws the Alien through a series of points/destinations. Is Offset to appear over planet.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="alien"></param>
        public void DrawAlien(CanvasAnimatedDrawEventArgs args, CanvasBitmap alien)
        {
            int yOffset = 20;

            // Planet lifting alien
            if (AlienAnimationCounter > 2 && AlienAnimationCounter < 5)
            {
                Rect planetRect = new Rect(OldPosAlien.X, OldPosAlien.Y, _rectSize, _rectSize);

                args.DrawingSession.DrawImage(Players[AlienWhosTurn].PlanetBitmap, planetRect);
            }

            Rect alienRect = new Rect(OldPosAlien.X, OldPosAlien.Y - yOffset, _rectSize, _rectSize);

            args.DrawingSession.DrawImage(alien, alienRect);

        }

        /// <summary>
        /// This method works by comparing two points and traveling from the Oldpos to the NewPos. The mathemathical formulas makes the animation smooth.
        /// </summary>
        public void UpdateMoveAnimation()
        {
            double xDistance = NewPos.X - OldPos.X;
            double yDistance = NewPos.Y - OldPos.Y;
            double moveAnimationDistance = Math.Sqrt((xDistance * xDistance) + (yDistance * yDistance));
            if (moveAnimationDistance > 1)
            {
                MoveAnimationComplete = false;
                OldPos = new Point(OldPos.X + (xDistance / MoveAnimationSpeed--), OldPos.Y + (yDistance / MoveAnimationSpeed--)); ;
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

        /// <summary>
        /// Works the same way as the above method.
        /// </summary>
        public void UpdateMeteor()
        {
            double xDistanceMeteor = RectList[UnavailableRects[0]].X - OldPosMeteor.X;
            double yDistanceMeteor = RectList[UnavailableRects[0]].Y - OldPosMeteor.Y;
            double distanceMeteor = Math.Sqrt((xDistanceMeteor * xDistanceMeteor) + (yDistanceMeteor * yDistanceMeteor));

            if (distanceMeteor > 1)
            {
                OldPosMeteor = new Point(OldPosMeteor.X + (xDistanceMeteor / MoveAnimationSpeed--), OldPosMeteor.Y + (yDistanceMeteor / MoveAnimationSpeed--)); ;
            }
            else
            {
                // Animation complete
                MeteorStrike = false; 
            }
            if (MoveAnimationSpeed < 5)
            {
                MoveAnimationSpeed = 5;
            }
        }

        /// <summary>
        /// This method uses the TravelPoints list created in CheckOKAlienMoves to know where to go and when. 
        /// This makes it look like the alien travels from outside the screen, to the planet and then to the destination.
        /// </summary>
        public void UpdateAlien()
        {
            int speedAlien = 13;

            double xDistanceAlien = TravelPoints[AlienAnimationCounter].X - OldPosAlien.X;
            double yDistanceAlien = TravelPoints[AlienAnimationCounter].Y - OldPosAlien.Y;
            double distanceAlien = Math.Sqrt((xDistanceAlien * xDistanceAlien) + (yDistanceAlien * yDistanceAlien));

            if (distanceAlien > 1)
            {
                if (AlienAnimationCounter == 0)
                {
                    OldPosAlien = new Point(TravelPoints[AlienAnimationCounter].X + (xDistanceAlien / speedAlien), TravelPoints[AlienAnimationCounter].Y + (yDistanceAlien / speedAlien));
                }
                else
                {
                    OldPosAlien = new Point(OldPosAlien.X + (xDistanceAlien / speedAlien--), OldPosAlien.Y + (yDistanceAlien / speedAlien--));
                }
            }
            else
            {
                if (AlienAnimationCounter == 2)
                {
                    Players[AlienWhosTurn].PlayerPositions.RemoveAt(AlienInfoList[0]);
                }
                if (AlienAnimationCounter == 4)
                {
                    Players[AlienWhosTurn].PlayerPositions.Insert(AlienInfoList[0], AlienInfoList[1]);
                }
                OldPosAlien = TravelPoints[AlienAnimationCounter];
                // Animation step complete    
                AlienAnimationCounter++;
                if (AlienAnimationCounter > 5)
                {
                    AlienAnimationCounter = 0;
                    AlienEncounter = false;
                }
            }
        }

        /// <summary>
        /// Checks if mouse clicked on a Rectangle(On the game board). If yes, 
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
            // Rectangle contains a planet == planet is selected
            if (Players[CurrentPlayerIndex].PlayerPositions.Contains(RectSelected))
            {
                if (PlanetSelected != -1 && Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected] == RectSelected) // if doubleclick on same planet - move finished!
                {
                    // Only next turn if doubleclick on planet you moved
                    if (PlanetSelected == DoubleJumpSaved)
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
            else
            {
                // If rect is emtpy

                // AND if planet was selected last click
                if (PlanetSelected != -1)
                {
                    // AND OKmove == move
                    if (SingleJumps.Contains(RectSelected) || DoubleJumps.Contains(RectSelected))
                    {
                        MovePlanet();
                    }
                    else
                    {
                        // random rect slected == deselect Planet
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

                // for GameCanvas_Update flag for next turn so it doesnt do next turn with moving back to pos while double jumping.
                SavedPosition = -1;
            }
            else if (DoubleJumps.Contains(RectSelected))
            {
                // If onlydoublejump is true, selected HAS to be == doublejump saved (( to prevent doublejumping with other planets)
                if (OnlyDoubleJump && PlanetSelected == DoubleJumpSaved)
                {
                    // Animation starts
                    NewPos = new Point(RectList[RectSelected].X, RectList[RectSelected].Y);
                    MoveAnimationComplete = false;
                    Players[CurrentPlayerIndex].PlayerPositions.RemoveAt(PlanetSelected);
                    DoubleJumpSaved = PlanetSelected;

                    // Planet is back to start pos
                    if (RectSelected == SavedPosition)
                    {
                        DoubleJumpSaved = -1;
                        OnlyDoubleJump = false;
                    }
                }
                else if (!OnlyDoubleJump)
                {
                    // First doublejump

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
                AlienEventCounter++;
            }
            else if (AlienEventCounter == 2)
            {
                AlienEventCounter = 0;
                AlienWhosTurn++;
                if (AlienWhosTurn == Players.Count)
                {
                    AlienWhosTurn = 0;
                }
                CheckOKAlienMoves();
            }
        }

        /// <summary>
        /// This method is dedicated to the alien event. It uses AlienWhosTurn to choose a player who is going to get abducted. 
        /// It randomizes which planet to abduct. If all of a players planets lie in the excluded zones, in the "bases", then the alien will not come.
        /// </summary>
        private void CheckOKAlienMoves()
        {
            List<int> allPlayerPositions = new List<int>();
            List<int> startAndGoalPosExcluded = new List<int>();
            Random rnd = new Random();
            int whichPlanet = rnd.Next(0, 10);
            int whereToGoBack = StartPosDict[PlayerIDs[AlienWhosTurn]][rnd.Next(0, 10)];


            foreach (int index in PlayerIDs)
            {
                startAndGoalPosExcluded.AddRange(StartPosDict[index]);
            }
            startAndGoalPosExcluded.AddRange(Players[AlienWhosTurn].WinPositions);

            if (Players[AlienWhosTurn].PlayerPositions.All(position => startAndGoalPosExcluded.Contains(position)))
            {
                return;
            }
            
            while (startAndGoalPosExcluded.Contains(Players[AlienWhosTurn].PlayerPositions[whichPlanet]))
            {
                whichPlanet = rnd.Next(0, 10);
            }

            // This whole block runs if the spawn bases do not contain the chosen planet to move. 
            if (!startAndGoalPosExcluded.Contains(Players[AlienWhosTurn].PlayerPositions[whichPlanet]))
            {
                // Add playerpositions to a list.
                foreach (Player player in Players)
                {
                    allPlayerPositions.AddRange(player.PlayerPositions);
                }

                // Choose a new point until you hit an empty rectangle
                while (allPlayerPositions.Contains(whereToGoBack))
                {
                    whereToGoBack = StartPosDict[PlayerIDs[AlienWhosTurn]][rnd.Next(0, 10)];
                }

                // This is used in other places, such as in DrawAlien and UpdateAlien
                AlienInfoList = new List<int>() { whichPlanet, whereToGoBack };

                OldPosAlien = new Point(RectList[Players[AlienWhosTurn].PlayerPositions[AlienInfoList[0]]].X, RectList[Players[AlienWhosTurn].PlayerPositions[AlienInfoList[0]]].Y);
                // Points for the alien to go through when it picks up and drops a planet
                TravelPoints = new List<Point>();
                TravelPoints.Add(new Point(0, 0));
                TravelPoints.Add(new Point(OldPosAlien.X, OldPosAlien.Y - 65));
                TravelPoints.Add(new Point(OldPosAlien.X, OldPosAlien.Y));
                TravelPoints.Add(new Point(RectList[AlienInfoList[1]].X, RectList[AlienInfoList[1]].Y - 65));
                TravelPoints.Add(new Point(RectList[AlienInfoList[1]].X, RectList[AlienInfoList[1]].Y));
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
            List<int> firePositions = new List<int>();
            List<int> startPosExcluded = new List<int>();
            List<int> playerPos = new List<int>();
            List<int> blockingRects = new List<int>();
            foreach (List<int> list in StartPosDict.Values)
            {
                startPosExcluded.AddRange(list);
            }

            foreach (Player p in Players)
            {
                playerPos.AddRange(p.PlayerPositions);
            }
            blockingRects.AddRange(startPosExcluded);
            blockingRects.AddRange(playerPos);

            Random rnd = new Random();

            int randomMeteorPos;
            bool _blocking = true;
            int tries = 0;
            // points.Count = 3
            int amountOfFire = 3;

            while (_blocking)
            { 
                randomMeteorPos = rnd.Next(0, 120);
                List<Point> points = new List<Point> {
                new Point //top right
                {
                    X = RectList[randomMeteorPos].X + _xDiff,
                    Y = RectList[randomMeteorPos].Y - _yDiff
                },
                new Point // right
                {
                    X = RectList[randomMeteorPos].X + _xSameLevelDiff,
                    Y = RectList[randomMeteorPos].Y
                },
                new Point //bot right
                {
                    X = RectList[randomMeteorPos].X + _xDiff,
                    Y = RectList[randomMeteorPos].Y + _yDiff
                }};

                firePositions.Add(randomMeteorPos);
                for (int i = 0; i < RectList.Count; i++)
                {
                    if (tries > 25 )
                    {
                        amountOfFire = 0;
                    }
                    for (int j = 0; j < rnd.Next(amountOfFire, points.Count); j++) // 
                    {
                        if (RectList[i].Contains(points[j]))
                        {
                            firePositions.Add(i);
                        }
                    }
                }

                if (blockingRects.Any(firePositions.Contains))
                {
                    _blocking = true;
                    tries++;
                    firePositions.Clear();

                }
                else
                {
                    _blocking = false;
                    break;
                }
            }
            return firePositions;

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
                    X = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].X - _xDiff,
                    Y = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].Y - _yDiff
                },
                new Point //top right
                {
                    X = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].X + _xDiff,
                    Y = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].Y - _yDiff
                },
                new Point // right
                {
                    X = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].X + _xSameLevelDiff,
                    Y = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].Y
                },
                new Point //bot right
                {
                    X = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].X + _xDiff,
                    Y = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].Y + _yDiff
                },
                new Point // bot left
                {
                    X = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].X - _xDiff,
                    Y = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].Y + _yDiff
                },
                new Point // left
                {
                    X = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].X - _xSameLevelDiff,
                    Y = RectList[Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected]].Y
                }};

            List<int> playerPos = new List<int>();

            foreach (Player p in Players)
            {
                playerPos.AddRange(p.PlayerPositions);
            }


            // Loop through each rectangle in gamefield to find the rectangles that contain a Point. 
            // A Point is made by going one rectangle in each direction.
            // If Rectangle is not occupied by another Planet -> add to list.
            for (int i = 0; i < RectList.Count; i++)
            {
                for (int j = 0; j < points.Count; j++)
                {
                    // Finds index for the six positions closest to PlanetSelected
                    if (RectList[i].Contains(points[j])) 
                    {
                        // If true -> RectList[i] is one of the six singlejump positions

                        // Checks if ANY planets are blocking OR meteorblocking
                        if (!playerPos.Contains(i) && !UnavailableRects.Contains(i))
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
                                    jumpPoint.X -= _xDiff;
                                    jumpPoint.Y -= _yDiff;

                                    for (int k = i; k >= 0; k--)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !playerPos.Contains(k) && !UnavailableRects.Contains(k))
                                        {
                                            DoubleJumps.Add(k);
                                        }
                                    }
                                    break;

                                case 1:
                                    jumpPoint.X += _xDiff;
                                    jumpPoint.Y -= _yDiff;

                                    for (int k = i; k >= 0; k--)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !playerPos.Contains(k) && !UnavailableRects.Contains(k))
                                        {
                                            DoubleJumps.Add(k);
                                        }
                                    }
                                    break;
                                case 2:
                                    jumpPoint.X += _xSameLevelDiff;

                                    for (int k = i; k < RectList.Count; k++)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !playerPos.Contains(k) && !UnavailableRects.Contains(k))
                                        {
                                            DoubleJumps.Add(k);
                                        }
                                    }
                                    break;
                                case 3:
                                    jumpPoint.X += _xDiff;
                                    jumpPoint.Y += _yDiff;

                                    for (int k = i; k < RectList.Count; k++)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !playerPos.Contains(k) && !UnavailableRects.Contains(k))
                                        {
                                            DoubleJumps.Add(k);
                                        }
                                    }
                                    break;
                                case 4:
                                    jumpPoint.X -= _xDiff;
                                    jumpPoint.Y += _yDiff;

                                    for (int k = i; k < RectList.Count; k++)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !playerPos.Contains(k) && !UnavailableRects.Contains(k))
                                        {
                                            DoubleJumps.Add(k);
                                        }
                                    }
                                    break;
                                case 5:
                                    jumpPoint.X -= _xDiff;

                                    for (int k = i; k >= 0; k--)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !playerPos.Contains(k) && !UnavailableRects.Contains(k))
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

            // Show no singleJumps if only doublejumps
            if (OnlyDoubleJump)
            {
                SingleJumps.Clear();

                // Show no doublejumps if wrong planet selected
                if (DoubleJumpSaved != PlanetSelected)
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
