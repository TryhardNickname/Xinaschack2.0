using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Threading;
using Windows.Foundation;
using Windows.UI.Xaml.Input;

namespace Xinaschack2._0.Classes
{
    class GameBoard
    {
        public List<Rect> RectList { get; set; }
        public List<Player> Players { get; set; }
        private int RectSelected { get; set; }
        public bool DidMove { get; set; }
        public double XDistance { get; set; }
        public double YDistance { get; set; }
        public Point NewPos { get; set; }
        public Point OldPos { get; set; }
        public List<int> OldNewPos { get; set; }
        public int CurrentPlayerIndex { get; private set; }
        private bool OnlyDoubleJump { get; set; }
        private int PlanetSelected { get; set; }
        private int DoubleJumpSaved { get; set; }
        private int SavedPosition { get; set; }
        private List<int> DoubleJumps { get; set; }
        private List<int> SingleJumps { get; set; }
        private Dictionary<int, List<int>> StartPosDict { get; set; }


        private readonly double XSameLevelDiff = 45;
        private readonly double XDiff = 22.5; // XSameLevelDiff / 2; // trigonometry
        private readonly double YDiff = 40; // rectsize + 5?
        private readonly double RectSize = 35;

        public GameBoard(int width, int height, int amountOfPlayers)
        {
            RectList = new List<Rect>();
            Players = new List<Player>();

            DoubleJumps = new List<int>();
            SingleJumps = new List<int>();

            StartPosDict = new Dictionary<int, List<int>>();
            OldNewPos = new List<int>();


            SetupPlayerPosDict();
            MakeRectList(width, height);
            InitPlayerPlanets(amountOfPlayers);
            CurrentPlayerIndex = 0;
            OnlyDoubleJump = false;
            PlanetSelected = -1;
            DoubleJumpSaved = -1;
            SavedPosition = -1;
        }

        /// <summary>
        /// This methods draws rectangles so as to make a board that looks like the game field. 
        /// Relative values found in paint.net and these help build the field by knowing the distance between each rectangle on the x-axis for example.
        /// gameArray represents how many spots there are in each "level" of the field, from top to bottom.
        /// </summary>
        private void MakeRectList(int width, int height)
        {
            double XStart = (width / 2) - (RectSize / 2);
            double YStart = YDiff / 2;

            double XCurrent;
            double YCurrent;

            int[] gameArray = new int[] { 1, 2, 3, 4, 13, 12, 11, 10, 9, 10, 11, 12, 13, 4, 3, 2, 1 };

            for (int i = 0; i < gameArray.Length; i++)
            {
                YCurrent = YStart + (YDiff * i);
                XCurrent = XStart - (XDiff * (gameArray[i] - 1));

                for (int j = 0; j < gameArray[i]; j++)
                {
                    Rect newRect = new Rect(XCurrent, YCurrent, RectSize, RectSize);
                    RectList.Add(newRect);

                    XCurrent += XSameLevelDiff;
                }
            }
        }

        private void SetupPlayerPosDict()
        {

            StartPosDict.Add(0, new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            StartPosDict.Add(1, new List<int>() { 19, 20, 21, 22, 32, 33, 34, 44, 45, 55 });
            StartPosDict.Add(2, new List<int>() { 74, 84, 85, 95, 96, 97, 107, 108, 109, 110 });
            StartPosDict.Add(3, new List<int>() { 111, 112, 113, 114, 115, 116, 117, 118, 119, 120 });
            StartPosDict.Add(4, new List<int>() { 65, 75, 76, 86, 87, 88, 98, 99, 100, 101 });
            StartPosDict.Add(5, new List<int>() { 10, 11, 12, 13, 23, 24, 25, 35, 36, 46 });
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
                    Players.Add(new Player(PlanetEnum.Mercury, StartPosDict[5], StartPosDict[2]));
                    break;
                case 6:
                    Players.Add(new Player(PlanetEnum.Earth, StartPosDict[0], StartPosDict[3]));
                    Players.Add(new Player(PlanetEnum.Jupiter, StartPosDict[1], StartPosDict[4]));
                    Players.Add(new Player(PlanetEnum.Mars, StartPosDict[2], StartPosDict[5]));
                    Players.Add(new Player(PlanetEnum.Mercury, StartPosDict[3], StartPosDict[0]));
                    Players.Add(new Player(PlanetEnum.Neptune, StartPosDict[4], StartPosDict[1]));
                    Players.Add(new Player(PlanetEnum.Venus, StartPosDict[5], StartPosDict[2]));
                    break;
                default:
                    break;
            }

        }

        public void DebugText(CanvasAnimatedDrawEventArgs args)
        {
            if (PlanetSelected != -1)
            {
                args.DrawingSession.DrawText("PlanetSelected Position = " + Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected].ToString(), 50, 100, Windows.UI.Color.FromArgb(255, 90, 255, 170));
            }

            args.DrawingSession.DrawText("RectSelected = " + RectSelected.ToString(), 50, 120, Windows.UI.Color.FromArgb(255, 90, 255, 170));
            args.DrawingSession.DrawText("PlanetSelected = " + PlanetSelected.ToString(), 50, 140, Windows.UI.Color.FromArgb(255, 90, 255, 170));
            args.DrawingSession.DrawText("DoubleJumpSaved = " + DoubleJumpSaved.ToString(), 50, 160, Windows.UI.Color.FromArgb(255, 50, 50, 50));
            args.DrawingSession.DrawText("singleJumps = " + string.Join(" ", SingleJumps), 30, 190, Windows.UI.Color.FromArgb(255, 90, 255, 170));
            args.DrawingSession.DrawText("doubleJumps = " + string.Join(" ", DoubleJumps), 30, 210, Windows.UI.Color.FromArgb(255, 90, 255, 170));

            args.DrawingSession.DrawText("Player1 Positions = " + string.Join(" ", Players[0].PlayerPositions), 10, 230, Windows.UI.Color.FromArgb(255, 90, 255, 170));
            args.DrawingSession.DrawText("Player2 Positions = " + string.Join(" ", Players[1].PlayerPositions), 10, 250, Windows.UI.Color.FromArgb(255, 90, 255, 170));

            args.DrawingSession.DrawText("OldNewPos = " + string.Join(" ", OldNewPos), 10, 270, Windows.UI.Color.FromArgb(255, 90, 255, 170));
        }

        public void DrawSelectedRect(CanvasAnimatedDrawEventArgs args)
        {
            // If a rectangle is selected, make it have a green border.
            // Permanent red borders turned off for now. 
            for (int rectIndex = 0; rectIndex < RectList.Count; rectIndex++)
            {
                if (RectSelected == rectIndex)
                {
                    args.DrawingSession.DrawRectangle(RectList[rectIndex], Windows.UI.Color.FromArgb(255, 0, 255, 0), 2);
                }
                else
                {
                    args.DrawingSession.DrawRectangle(RectList[rectIndex], Windows.UI.Color.FromArgb(255, 255, 0, 0), 0);
                }
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

        public void DrawOkayMoves(CanvasAnimatedDrawEventArgs args)
        {

            for (int i = 0; i < SingleJumps.Count; i++)
            {
                args.DrawingSession.DrawRectangle(RectList[SingleJumps[i]], Windows.UI.Color.FromArgb(255, 0, 255, 255), 2);
            }

            for (int i = 0; i < DoubleJumps.Count; i++)
            {
                args.DrawingSession.DrawRectangle(RectList[DoubleJumps[i]], Windows.UI.Color.FromArgb(255, 0, 0, 255), 2);
            }
        }

        public void DrawPlayerTurn(CanvasAnimatedDrawEventArgs args)
        {

            args.DrawingSession.DrawText((CurrentPlayerIndex + 1).ToString(), 50, 50, Windows.UI.Color.FromArgb(255, 255, 0, 0));
        }

        internal void DrawAnimations(CanvasAnimatedDrawEventArgs args)
        {
            NewPos = new Point(RectList[OldNewPos[OldNewPos.Count - 1]].X, RectList[OldNewPos[OldNewPos.Count - 1]].Y);
            Rect moveRect = new Rect(OldPos.X, OldPos.Y, RectSize, RectSize);

            args.DrawingSession.DrawRectangle(moveRect, Windows.UI.Color.FromArgb(255, 1, 1, 1), 2);

        }

        /// <summary>
        /// Checks if mouse clicked on a Rectangle(On the game board)
        /// </summary>
        /// <param name="e"></param>
        public void CheckIfRect_Pressed(PointerRoutedEventArgs e)
        {
            for (int rectIndex = 0; rectIndex < RectList.Count; rectIndex++)
            {
                if (RectList[rectIndex].Contains(e.GetCurrentPoint(null).Position))
                {
                    // Save which Rect was pressed in prop
                    RectSelected = rectIndex;
                    CheckSelection();
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
            Players[CurrentPlayerIndex].OldPlayerPositions = new List<int>(Players[CurrentPlayerIndex].PlayerPositions);
            

            if (SingleJumps.Contains(RectSelected))
            {
                Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected] = RectSelected;

                for (int i = 0; i < Players[CurrentPlayerIndex].PlayerPositions.Count; i++)
                {
                    if (Players[CurrentPlayerIndex].OldPlayerPositions[i] != Players[CurrentPlayerIndex].PlayerPositions[i])
                    {
                        OldNewPos.Add(Players[CurrentPlayerIndex].OldPlayerPositions[i]);
                        OldNewPos.Add(Players[CurrentPlayerIndex].PlayerPositions[i]);
                        OldPos = new Point(RectList[OldNewPos[OldNewPos.Count - 2]].X, RectList[OldNewPos[OldNewPos.Count - 2]].Y);
                    }
                }

                NextTurn();
            }
            else if (DoubleJumps.Contains(RectSelected))
            {
                if (OnlyDoubleJump && PlanetSelected == DoubleJumpSaved) // if onlydoublejump is tru, selected HAS to be == doublejump saved
                {
                    Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected] = RectSelected;
                    DoubleJumpSaved = PlanetSelected;
                    CheckOKMoves();

                    if (RectSelected == SavedPosition) // Planet is back to start pos
                    {
                        DoubleJumpSaved = -1;
                        OnlyDoubleJump = false;
                    }
                }
                else if (!OnlyDoubleJump)
                {
                    SavedPosition = Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected];
                    Players[CurrentPlayerIndex].PlayerPositions[PlanetSelected] = RectSelected;
                    DoubleJumpSaved = PlanetSelected;
                    OnlyDoubleJump = true;
                    CheckOKMoves();
                }
            }
        }

        /// <summary>
        /// Increases CurrentPlayerIndex and resets properties
        /// </summary>
        private void NextTurn()
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
            DidMove = true;
        }

        /// <summary>
        /// Check which places you can move with PlanetSelected. Looks at the 6 surrounding rectangles
        /// If planet is next to PlanetSelected => check if jump is possible
        /// </summary>
        /// <returns></returns>
        private void CheckOKMoves()
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

                        if (!PlayerPos.Contains(i)) // checks if ANY planets are blocking
                        {
                            SingleJumps.Add(i);
                        }
                        else
                        {
                            //     If you have a planet that exists in the PlayerPos list, which contains indexes of positions of planets,
                            //     do the switch case which does an operation to find out if the rectangle behind the current
                            //     rectangle is available. ( a jump pver the planet that is "blocking"

                            Point jumpPoint = points[j]; // by using "j" , we look in the same direction
                            // check if you can jump over this planet
                            switch (j)
                            {
                                case 0:
                                    jumpPoint.X -= XDiff;
                                    jumpPoint.Y -= YDiff;

                                    for (int k = i; k >= 0; k--)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !PlayerPos.Contains(k))
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
                                        if (RectList[k].Contains(jumpPoint) && !PlayerPos.Contains(k))
                                        {
                                            DoubleJumps.Add(k);
                                        }
                                    }
                                    break;
                                case 2:
                                    jumpPoint.X += XSameLevelDiff;

                                    for (int k = i; k < RectList.Count; k++)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !PlayerPos.Contains(k))
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
                                        if (RectList[k].Contains(jumpPoint) && !PlayerPos.Contains(k))
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
                                        if (RectList[k].Contains(jumpPoint) && !PlayerPos.Contains(k))
                                        {
                                            DoubleJumps.Add(k);
                                        }
                                    }
                                    break;
                                case 5:
                                    jumpPoint.X -= XDiff;

                                    for (int k = i; k >= 0; k--)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !PlayerPos.Contains(k))
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
    }



}

