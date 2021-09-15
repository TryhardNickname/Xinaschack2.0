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
        private readonly float DesignWidth = 1280;
        private readonly float DesignHeight = 720;
        private List<Rect> RectList { get; set; }
        private List<int> PlayerPositions { get; set; }
        private int RectSelected { get; set; }
        private int PlanetSelectedFlag { get; set; }

        public MainPage()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(1280, 720);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            RectList = new List<Rect>();
            PlayerPositions = new List<int>();
            PlanetSelectedFlag = -1;
            MakeRectList();
            InitPlayerPlanets();

        }

        private void InitPlayerPlanets()
        {
            for (int i = 0; i < 10; i++)
            {
                PlayerPositions.Add(i);
            }
        }

        private async Task CreateResourcesAsync(CanvasAnimatedControl sender)
        {
            StartScreen = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/background.PNG"));
            Board = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/GameFieldCentered.png"));
            Earth = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/earth_mini34x34.png"));
        }

        private void GameCanvas_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)  
        {
            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }

        private void GameCanvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            args.DrawingSession.DrawImage(StartScreen);
            args.DrawingSession.DrawImage(Board);


            for (int rectIndex = 0; rectIndex < RectList.Count; rectIndex++)
            { 
                if ( RectSelected == rectIndex)
                {
                    args.DrawingSession.DrawRectangle(RectList[rectIndex], Windows.UI.Color.FromArgb(255, 0, 255, 0), 2);
                }
                else
                {
                    args.DrawingSession.DrawRectangle(RectList[rectIndex], Windows.UI.Color.FromArgb(255, 255, 0, 0), 2);
                }
            }

            for (int posIndex = 0; posIndex < PlayerPositions.Count; posIndex++)
            {
                args.DrawingSession.DrawImage(Earth, RectList[PlayerPositions[posIndex]]);
            }

        }

        private void GameCanvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {

        }

        private void GameCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

            for (int rectIndex = 0; rectIndex < RectList.Count; rectIndex++)
            {
                if (RectList[rectIndex].Contains(e.GetCurrentPoint(null).Position))
                {
                    RectSelected = rectIndex;
                    if ( PlayerPositions.Contains(RectSelected))
                    {
                        PlanetSelectedFlag = RectSelected;
                    }
                    else
                    {
                        if ( PlanetSelectedFlag >= 0)
                        {
                            // move planet
                            for (int posIndex = 0; posIndex < PlayerPositions.Count; posIndex++)
                            {
                                if ( PlayerPositions[posIndex] == PlanetSelectedFlag)
                                {
                                    PlayerPositions[posIndex] = RectSelected;
                                }
                            }
                            
                        }
                        PlanetSelectedFlag = -1;
                    }
                }
            }
        }

        private void MakeRectList()
        {
            double XDiff = 22.5;
            double YDiff = 39;
            double XStart = (DesignWidth / 2) +5;
            double YStart = 28;
            double RectSize = 35;

            double XCurrent;
            double YCurrent;

            double XSameLevelDiff = 45;

            int[] gameArray = new int[] { 1, 2, 3, 4, 13, 12, 11, 10, 9, 10, 11, 12, 13, 4, 3, 2, 1};

            for (int i = 0; i < gameArray.Length; i++)
            {
                YCurrent = YStart + (YDiff * i);
                if (i == 0)
                {
                    XCurrent = XStart - (RectSize/2);
                }
                else
                {
                    XCurrent = XStart - (XDiff * gameArray[i]);
                }

                
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
