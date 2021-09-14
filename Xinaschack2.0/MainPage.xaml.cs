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

        public MainPage()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(1280, 720);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            RectList = new List<Rect>();

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
            MakeRectList(sender, args);
            foreach (Rect rect in RectList)
            {
                args.DrawingSession.DrawRectangle(rect, Windows.UI.Color.FromArgb(255, 255, 0, 0), 2);
            }
            
            for (int i = 0; i < 10; i++)
            {
                args.DrawingSession.DrawImage(Earth, RectList[i]);
            }
        }

        private void GameCanvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {

        }

        private void GameCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            foreach (Rect rect in RectList.ToList())
            {
                if (rect.Contains( e.GetCurrentPoint(null).Position))
                {
                    Debug.Print(rect.X.ToString());
                    Debug.Print(rect.Y.ToString());
                }
            }
        }

        private void MakeRectList(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            double XStart = DesignWidth / 2;
            double YStart = 28;

            double XCurrent = 621;
            double YCurrent = 28;

            double XDiff = 22.5;
            double YDiff = 39;
            double XSameLevelDiff = 45;

            int[] gameArray = new int[] { 1, 2, 3, 4, 13, 12, 11, 10, 9, 10, 11, 12, 13, 4, 3, 2, 1};

            for (int i = 0; i < gameArray.Length; i++)
            {
                YCurrent = YStart + (YDiff * i);
                XCurrent = XStart - (XDiff * gameArray[i]);
                
                for (int j = 0; j < gameArray[i]; j++)
                {
                    Rect newRect = new Rect(XCurrent, YCurrent, 35, 35);
                    RectList.Add(newRect);

                    XCurrent += XSameLevelDiff;
                }
            }

            // ball size = 34x34
            // middle of 1280 = 640
            // bottom of board = 632
            // top of board = 67
            // Point middle_bottom_point = new Point(640, )
            // RectList.Add(new Rect(new Point(6), new Size()));
        }

    }
}
