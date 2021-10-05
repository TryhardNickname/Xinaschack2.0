using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Xinaschack2._0.Classes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Xinaschack2._0
{
    public enum PlanetEnum
    {
        Earth,
        Venus,
        Mars,
        Mercury,
        Jupiter,
        Neptune
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private void Back2menu(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainMenu), null);
        }
        // private CanvasBitmap StartScreen { get; set; }
        private CanvasBitmap Board { get; set; }
        private List<CanvasBitmap> FireList { get; set; }
        private CanvasBitmap Comet { get; set; }
        private CanvasBitmap Alien { get; set; }

        private readonly int DesignWidth = 1920;
        private readonly int DesignHeight = 1080;

        GameBoard game;
        private MediaPlayer mediaPlayer;
        public MainPage()
        {
            InitializeComponent();
            FireList = new List<CanvasBitmap>();
            // For now we create GameBoard here => After menu is made, we can create 
            // GameBoard when the player presses PLAY
            mediaPlayer = new MediaPlayer();
            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/sounds/SpaceSong.mp3"));
            mediaPlayer.Play();
            ApplicationView.PreferredLaunchViewSize = new Size(DesignWidth, DesignHeight);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            game = new GameBoard(DesignWidth, DesignHeight, (int)e.Parameter);
            base.OnNavigatedTo(e);
        }


        /// <summary>
        /// Loads pictures into Bitmaps that can be used in the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private async Task CreateResourcesAsync(CanvasAnimatedControl sender)
        {
            Board = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/spelplan3.png"));

            Comet = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/comet.png"));
            Alien = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/alien.png"));
            FireList.Add(await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/fire1.png")));
            FireList.Add(await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/fire2.png")));
            FireList.Add(await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/fire3.png")));
            FireList.Add(await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/fire4.png")));

            foreach (Player p in game.Players)
            {
                await p.LoadBitmapAsync(sender).AsAsyncAction();
            }
        }

        private void GameCanvas_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }

        /// <summary>
        /// Draws Rectangles from RectList ( which contains positions of all rectangles ) Also draws planets depending on Players (List of indexes)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void GameCanvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            args.DrawingSession.DrawImage(Board, 320, 0);
            game.DrawSelectedRect(args);
            game.DrawPlayerPlanets(sender, args);
            game.DrawOkayMoves(args);
            DrawPlayerTurn(args);
            if (game.MeteorStrike)
            {
                game.DrawMeteor(args, Comet);
            }
            else
            {
                game.DrawUnavailableRects(args, FireList);
            }

            if (game.AlienEncounter)
            {
                game.DrawAlien(args, Alien);
            }
            if (!game.AnimationComplete)
            {
                game.DrawAnimations(args);
            }
            //game.DebugText(args);
        }

        private void GameCanvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            if (game.CheckIfWin())
            {
                Debug.WriteLine($"{game.Players[game.CurrentPlayerIndex].PlanetColor} won");
            }
            if (game.MeteorStrike)
            {
                game.UpdateMeteor();
            }

            if (game.AlienEncounter)
            {
                game.UpdateAlien();
            }
            if (!game.AnimationComplete)
            {
                game.UpdateAnimation();
            }
            if (game.AnimationComplete && game.TurnStarted)
            {
                game.MoveComplete();
                if (!game.OnlyDoubleJump && game.SavedPosition == -1) // savedpos to prevcent next turn from happening when jumping back to start posistion
                {
                    game.NextTurn();
                }
                else
                {
                    game.CheckOKMoves();
                }
            }
        }

        /// <summary>
        /// Every click on a rectangle, index in RectList is stored in RectSelected prop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // NYI
            // check if click on buttons
            // if mute 
            // if give up 
            if (game.AnimationComplete && !game.MeteorStrike && !game.AlienEncounter) // to prevent HAX by moving while animation happens???
            {
                game.CheckIfRect_Pressed(e.GetCurrentPoint(null).Position);
            }







        }
        public void DrawPlayerTurn(CanvasAnimatedDrawEventArgs args)
        {
            //args.DrawingSession.DrawText((CurrentPlayerIndex + 1).ToString(), 50, 20, Windows.UI.Color.FromArgb(255, 255, 0, 0));
            var turn = 0;
            var player1Turns = true;
            // x, y
            Rect moveRect = new Rect(120, 20, 50, 50);
            turn++;
                if (player1Turns)
                {
                    args.DrawingSession.DrawImage(game.Players[0].PlanetBitmap,moveRect);

                }
                else
                {
                    args.DrawingSession.DrawImage(game.Players[1].PlanetBitmap, moveRect);

                }
                if (turn % 3 == 0)
                {
                    player1Turns = !player1Turns;
                }
            
        }



    }
}
