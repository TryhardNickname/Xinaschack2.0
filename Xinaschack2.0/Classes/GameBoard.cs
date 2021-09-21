using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Input;

namespace Xinaschack2._0.Classes
{
    class GameBoard
    {
        public List<Rect> RectList { get; set; }
        public List<Player> Players { get; set; }
        private int RectSelected { get; set; }
        private int PlanetSelectedFlag { get; set; }
        private int CurrentPlayerIndex { get; set; }
        private bool OnlyDoubleJump{ get; set; }

        private List<int> doubleJumps;
        private List<int> singleJumps;

        private readonly double XSameLevelDiff = 45;
        private readonly double XDiff = 22.5; // XSameLevelDiff / 2; // trigonometry
        private readonly double YDiff = 40; // rectsize + 5?
        private readonly double RectSize = 35;


        public GameBoard(int width, int height, int amountOfPlayers)
        {
            RectList = new List<Rect>();
            Players = new List<Player>();
            doubleJumps = new List<int>();
            singleJumps = new List<int>();

            MakeRectList(width, height);
            InitPlayerPlanets(amountOfPlayers);
            CurrentPlayerIndex = 0;
            PlanetSelectedFlag = -1;
            OnlyDoubleJump = false;
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

        /// <summary>
        /// Adds 10 indexes for where the planets start-positions is for the 2 players
        /// </summary>
        private void InitPlayerPlanets(int amountOfPlayers)
        {
            // NYI dynamic choosing lpayers and planets
            List<int> list = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(i);
            }
            Player p1 = new Player(PlanetEnum.Earth, list);
            list.Clear();

            for (int i = RectList.Count - 1; i >= RectList.Count - 10; i--)
            {
                list.Add(i);
            }
            Player p2 = new Player(PlanetEnum.Venus, list);
            Players.Add(p1);
            Players.Add(p2);

        }

        internal void DebugText(CanvasAnimatedDrawEventArgs args)
        {
            args.DrawingSession.DrawText(PlanetSelectedFlag.ToString(), 50, 100, Windows.UI.Color.FromArgb(255, 90, 255, 170));
            args.DrawingSession.DrawText(RectSelected.ToString(), 50, 120, Windows.UI.Color.FromArgb(255, 90, 255, 170));
            args.DrawingSession.DrawText(string.Join(" ", singleJumps), 50, 140, Windows.UI.Color.FromArgb(255, 90, 255, 170));
            args.DrawingSession.DrawText(string.Join(" ", doubleJumps), 50, 160, Windows.UI.Color.FromArgb(255, 90, 255, 170));
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

        public void DrawPlayerPlanets(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {

            // Loop through the Players list, which is a list of list of ints.
            // Each player has a list of ints that represents where their planets lie. 
            for (int i = 0; i < Players.Count; i++)
            {
                for (int posIndex = 0; posIndex < Players[i].PlayerPositions.Count; posIndex++)
                {
                    if (i == 0)
                        args.DrawingSession.DrawImage(Players[i].PlanetBitmap, RectList[Players[i].PlayerPositions[posIndex]]);
                    if (i == 1)
                        args.DrawingSession.DrawImage(Players[i].PlanetBitmap, RectList[Players[i].PlayerPositions[posIndex]]);
                }
            }
        }

        public void DrawOkayMoves(CanvasAnimatedDrawEventArgs args)
        {

            for (int i = 0; i < singleJumps.Count; i++)
            {
                args.DrawingSession.DrawRectangle(RectList[singleJumps[i]], Windows.UI.Color.FromArgb(255, 0, 255, 255), 2);
            }

            for (int i = 0; i < doubleJumps.Count; i++)
            {
                args.DrawingSession.DrawRectangle(RectList[doubleJumps[i]], Windows.UI.Color.FromArgb(255, 0, 0, 255), 2);
            }
        }

        public void DrawPlayerTurn(CanvasAnimatedDrawEventArgs args)
        {

            args.DrawingSession.DrawText((CurrentPlayerIndex + 1).ToString(), 50, 50, Windows.UI.Color.FromArgb(255, 255, 0, 0));
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
        /// Checks if selection is planet or empty "rectangle". If previous click was on a Planet (PlanetSelectedFlag is >0 (where value is the index of which planet selected,
        /// and this click is on empty rectange, check if the "move" is valid with CheckOKMoves()
        /// </summary>
        private void CheckSelection()
        {
            if (Players[CurrentPlayerIndex].PlayerPositions.Contains(RectSelected)) //rectangle contains a planet == planet is selected
            {
                if (PlanetSelectedFlag == RectSelected) // if doubleclick on same planet - move finished!
                {

                    NextTurn();
                }
                else
                {
                    PlanetSelectedFlag = RectSelected;
                    CheckOKMoves();
                }

            }
            else // rect is emtpy!!
            {
                if (PlanetSelectedFlag >= 0) // if planet was selected last click AND new click is empty rect AND OKmove == move
                {
                    if (singleJumps.Contains(RectSelected) || doubleJumps.Contains(RectSelected))
                    {
                        MovePlanet();
                    }
                    else // random rect slected == deselect Planet
                    {
                        doubleJumps.Clear();
                        singleJumps.Clear();
                        PlanetSelectedFlag = -1;
                    }
                }
            }
        }

        private void MovePlanet()
        {

            if (singleJumps.Contains(RectSelected) && !OnlyDoubleJump)
            {
                // move planet
                for (int i = 0; i < Players.Count; i++)
                {
                    for (int posIndex = 0; posIndex < 10; posIndex++)
                    {
                        if (Players[i].PlayerPositions[posIndex] == PlanetSelectedFlag)
                        {
                            Players[i].PlayerPositions[posIndex] = RectSelected;
                            NextTurn();
                        }
                    }
                }
                RectSelected = -1;
            }
            else if (doubleJumps.Contains(RectSelected))
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    for (int posIndex = 0; posIndex < 10; posIndex++)
                    {
                        if (Players[i].PlayerPositions[posIndex] == PlanetSelectedFlag)
                        {
                            Players[i].PlayerPositions[posIndex] = RectSelected;
                            PlanetSelectedFlag = RectSelected;
                            //NYI DISABLE SINGLEJUMP NEXT CLICK
                            OnlyDoubleJump = true;
                            singleJumps.Clear();
                            doubleJumps.Clear();

                        }
                    }
                }
            }
            // RectSelected = -1;
        }

        private void NextTurn()
        {
            CurrentPlayerIndex++;
            if (CurrentPlayerIndex >= Players.Count)
            {
                CurrentPlayerIndex = 0;
            }

            singleJumps.Clear();
            doubleJumps.Clear();
            PlanetSelectedFlag = -1;
            RectSelected = -1;

        }

        /// <summary>
        /// Check which places you can move with PlanetSelectedFlag. Looks at the 6 surrounding rectangles
        /// If planet is next to PlanetSelected => check if jump is possible
        /// </summary>
        /// <returns></returns>
        private void CheckOKMoves()
        {
            doubleJumps.Clear();
            singleJumps.Clear();

            // Each point represents a jump to each direction available on each rectangle. 
            List<Point> points = new List<Point> {
                new Point // top left
                {
                    X = RectList[PlanetSelectedFlag].X - XDiff,
                    Y = RectList[PlanetSelectedFlag].Y - YDiff
                },
                new Point //top right
                {
                    X = RectList[PlanetSelectedFlag].X + XDiff,
                    Y = RectList[PlanetSelectedFlag].Y - YDiff
                },
                new Point // right
                {
                    X = RectList[PlanetSelectedFlag].X + XSameLevelDiff,
                    Y = RectList[PlanetSelectedFlag].Y
                },
                new Point //bot right
                {
                    X = RectList[PlanetSelectedFlag].X + XDiff,
                    Y = RectList[PlanetSelectedFlag].Y + YDiff
                },
                new Point // bot left
                {
                    X = RectList[PlanetSelectedFlag].X - XDiff,
                    Y = RectList[PlanetSelectedFlag].Y + YDiff
                },
                new Point // left
                {
                    X = RectList[PlanetSelectedFlag].X - XSameLevelDiff,
                    Y = RectList[PlanetSelectedFlag].Y
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
                    if (RectList[i].Contains(points[j])) // Finds index for the six positions closest to PLanetSelectedFlag
                    {
                        // if true -> RectList[i] is one of the six singlejump positions

                        if (!PlayerPos.Contains(i)) // checks if ANY planets are blocking
                        {
                            singleJumps.Add(i);
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
                                            doubleJumps.Add(k);
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
                                            doubleJumps.Add(k);
                                        }
                                    }
                                    break;
                                case 2:
                                    jumpPoint.X += XSameLevelDiff;

                                    for (int k = i; k < RectList.Count; k++)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !PlayerPos.Contains(k))
                                        {
                                            doubleJumps.Add(k);
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
                                            doubleJumps.Add(k);
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
                                            doubleJumps.Add(k);
                                        }
                                    }
                                    break;
                                case 5:
                                    jumpPoint.X -= XDiff;

                                    for (int k = i; k >= 0; k--)
                                    {
                                        if (RectList[k].Contains(jumpPoint) && !PlayerPos.Contains(k))
                                        {
                                            doubleJumps.Add(k);
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
        }
    }



}

