using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Xinaschack2._0
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CanvasBitmap StartScreen { get; set; }
        private CanvasBitmap Board { get; set; }
        private CanvasBitmap Earth { get; set; }
        private CanvasBitmap Mars { get; set; }
        private readonly float DesignWidth = 1280;
        private readonly float DesignHeight = 720;
        private List<Rect> RectList { get; set; }
        public List<List<int>> Players { get; set; }
        private int RectSelected { get; set; }
        private int PlanetSelectedFlag { get; set; }


        private readonly double XSameLevelDiff = 45;
        private readonly double XDiff = 22.5; // XSameLevelDiff / 2; // trigonometry
        private readonly double YDiff = 40; // rectsize + 5?
        private readonly double RectSize = 35;


        public MainPage()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(DesignWidth, DesignHeight);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            RectList = new List<Rect>();
            Players = new List<List<int>>();

            PlanetSelectedFlag = -1;
            MakeRectList();
            InitPlayerPlanets();
            

        }

        private void InitPlayerPlanets()
        {
            Players.Add(new List<int>());
            Players.Add(new List<int>());
            for (int i = 0; i < 10; i++)
            {
                Players[0].Add(i);
            }
            for (int i = RectList.Count-1; i >= RectList.Count - 10; i--)
            {
                Players[1].Add(i);
            }
        }

        private async Task CreateResourcesAsync(CanvasAnimatedControl sender)
        {
            StartScreen = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/background.PNG"));
            Board = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/spelplan3.png"));
            Earth = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/earth_mini34x34.png"));
            Mars = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/mars.png"));
        }

        private void GameCanvas_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)  
        {
            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }

        private void GameCanvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            // args.DrawingSession.DrawImage(StartScreen);
            args.DrawingSession.DrawImage(Board);


            for (int rectIndex = 0; rectIndex < RectList.Count; rectIndex++)
            { 
                if ( RectSelected == rectIndex)
                {
                    args.DrawingSession.DrawRectangle(RectList[rectIndex], Windows.UI.Color.FromArgb(255, 0, 255, 0), 2);
                }
                else
                {
                    args.DrawingSession.DrawRectangle(RectList[rectIndex], Windows.UI.Color.FromArgb(255, 255, 0, 0), 0);
                }
            }

            for (int i = 0; i < Players.Count; i++)
            {
                for (int posIndex = 0; posIndex < Players[i].Count; posIndex++)
                {
                    if (i==0)
                        args.DrawingSession.DrawImage(Earth, RectList[Players[i][posIndex]]);
                    if (i == 1)
                        args.DrawingSession.DrawImage(Mars, RectList[Players[i][posIndex]]);
                }
            }

        }

        private void GameCanvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {

        }

        private void GameCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // every cklick on a rectangle, it is stored in RectSelected prop
            for (int rectIndex = 0; rectIndex < RectList.Count; rectIndex++)
            {
                if (RectList[rectIndex].Contains(e.GetCurrentPoint(null).Position))
                {
                    CheckSelection(rectIndex);
                }
            }
        }

        private void CheckSelection(int rectIndex)
        {
            RectSelected = rectIndex;

            if (Players[0].Contains(RectSelected) || Players[1].Contains(RectSelected)) //rectangle contains a planet == planet is selected
            {
                PlanetSelectedFlag = RectSelected;
            }
            else
            {
                if (PlanetSelectedFlag >= 0) // if planet was selected last click AND new click is empty rect == move
                {
                    // check if move is OK
                    List<int> OKMoves = CheckOKMoves();
                    
                    if ( OKMoves.Contains(RectSelected))
                    {
                        // move planet
                        for (int i = 0; i < Players.Count; i++)
                        {
                            for (int posIndex = 0; posIndex < Players[i].Count; posIndex++)
                            {
                                if (Players[i][posIndex] == PlanetSelectedFlag)
                                {
                                    Players[i][posIndex] = RectSelected;
                                }
                            }
                        }


                        RectSelected = -1;
                    }
                }
                PlanetSelectedFlag = -1;
            }
        }

        private List<int> CheckOKMoves()
        {
            // check which places you can move with PlanetSelectedFlag
            List<int> list = new List<int>();
            List<Point> points = new List<Point>();

            // top left
            points.Add(new Point
            {
                X = RectList[PlanetSelectedFlag].X - XDiff,
                Y = RectList[PlanetSelectedFlag].Y - YDiff
            });

            //top right
            points.Add(new Point
            {
                X = RectList[PlanetSelectedFlag].X + XDiff,
                Y = RectList[PlanetSelectedFlag].Y - YDiff
            });

            // right
            points.Add(new Point
            {
                X = RectList[PlanetSelectedFlag].X + XSameLevelDiff,
                Y = RectList[PlanetSelectedFlag].Y
            });

            //bot right
            points.Add(new Point
            {
                X = RectList[PlanetSelectedFlag].X + XDiff,
                Y = RectList[PlanetSelectedFlag].Y + YDiff
            });

            // bot left
            points.Add(new Point
            {
                X = RectList[PlanetSelectedFlag].X - XDiff,
                Y = RectList[PlanetSelectedFlag].Y + YDiff
            });

            // left
            points.Add(new Point
            {
                X = RectList[PlanetSelectedFlag].X - XSameLevelDiff,
                Y = RectList[PlanetSelectedFlag].Y
            });


            for (int i = 0; i < RectList.Count; i++)
            {

                for (int j = 0; j < points.Count; j++)
                {

                    if (RectList[i].Contains(points[j])) // && !Players[0].Contains(i))
                    {
                        foreach (var player in Players)
                        {
                            if (player.Contains(i))
                            {
                                Point jumpPoint = points[j];
                                //check if you can jump over this planet
                                switch (j)
                                {
                                    case 0:
                                        jumpPoint.X -= XDiff;
                                        jumpPoint.Y -= YDiff;

                                        for (int k = i; k >= 0; k--)
                                        {
                                            if (RectList[k].Contains(jumpPoint) && !player.Contains(k))
                                            {
                                                list.Add(k);
                                            }
                                        }
                                        break;

                                    case 1:
                                        jumpPoint.X += XDiff;
                                        jumpPoint.Y -= YDiff;

                                        for (int k = i; k >= 0; k--)
                                        {
                                            if (RectList[k].Contains(jumpPoint) && !player.Contains(k))
                                            {
                                                list.Add(k);
                                            }
                                        }
                                        break;
                                    case 2:
                                        jumpPoint.X += XSameLevelDiff;

                                        for (int k = i; k < RectList.Count; k++)
                                        {
                                            if (RectList[k].Contains(jumpPoint) && !player.Contains(k))
                                            {
                                                list.Add(k);
                                            }
                                        }
                                        break;
                                    case 3:
                                        jumpPoint.X += XDiff;
                                        jumpPoint.Y += YDiff;

                                        for (int k = i; k < RectList.Count; k++)
                                        {
                                            if (RectList[k].Contains(jumpPoint) && !player.Contains(k))
                                            {
                                                list.Add(k);
                                            }
                                        }
                                        break;
                                    case 4:
                                        jumpPoint.X -= XDiff;
                                        jumpPoint.Y += YDiff;

                                        for (int k = i; k < RectList.Count; k++)
                                        {
                                            if (RectList[k].Contains(jumpPoint) && !player.Contains(k))
                                            {
                                                list.Add(k);
                                            }
                                        }
                                        break;
                                    case 5:
                                        jumpPoint.X -= XDiff;

                                        for (int k = i; k >= 0; k--)
                                        {
                                            if (RectList[k].Contains(jumpPoint) && !player.Contains(k))
                                            {
                                                list.Add(k);
                                            }
                                        }
                                        break;
                                }
                            }
                            else // empty box, 
                            {
                                list.Add(i);
                            }
                        }
                        
                    }
                }
            }
            return list;
        }

        private void MakeRectList()
        {

            double XStart = (DesignWidth / 2) - (RectSize / 2);
            double YStart = YDiff/2;

            double XCurrent;
            double YCurrent;


            int[] gameArray = new int[] { 1, 2, 3, 4, 13, 12, 11, 10, 9, 10, 11, 12, 13, 4, 3, 2, 1};

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
    }
}
