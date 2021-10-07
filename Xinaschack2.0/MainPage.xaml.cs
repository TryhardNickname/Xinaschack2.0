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
using Windows.Storage;
using Windows.UI.Core;
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
        Moon,
        Jupiter,
        Neptune
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CanvasBitmap Board { get; set; }
        private List<CanvasBitmap> FireList { get; set; }
        private CanvasBitmap Comet { get; set; }
        private CanvasBitmap Alien { get; set; }

        public static MediaPlayer SoundEffectsPlop;
        public static MediaPlayer SoundEffectsMeteor;
        public static MediaPlayer SoundEffectsAlien;

        private readonly int DesignWidth = 1920;
        private readonly int DesignHeight = 1080;

        private GameBoard Game { get; set; }
        public MainPage()
        {
            InitializeComponent();
            FireList = new List<CanvasBitmap>();

            SoundEffectsPlop = new MediaPlayer();
            SoundEffectsMeteor = new MediaPlayer();
            SoundEffectsAlien = new MediaPlayer();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (Game == null)
            {
                Game = new GameBoard(DesignWidth, DesignHeight, (int)e.Parameter);
            }
            base.OnNavigatedTo(e);
        }
        private void Back2menu(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainMenu), null);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Settings), "Game");
        }


        /// <summary>
        /// Loads pictures into Bitmaps that can be used in the canvas. And loads SoundEffects.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private async Task CreateResourcesAsync(CanvasAnimatedControl sender)
        {
            Board = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/GameFieldCropped.png"));
            Comet = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/comet.png"));
            Alien = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/alien.png"));

            FireList.Add(await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/fire1.png")));
            FireList.Add(await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/fire2.png")));
            FireList.Add(await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/fire3.png")));
            FireList.Add(await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/fire4.png")));

            foreach (Player p in Game.Players)
            {
                await p.LoadBitmapAsync(sender).AsAsyncAction();
            }

            SoundEffectsAlien.Volume = MainMenu.MediaPlayer.Volume;
            SoundEffectsPlop.AutoPlay = false;
            SoundEffectsPlop.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/sounds/ballmovesound.wav"));

            SoundEffectsAlien.Volume = MainMenu.MediaPlayer.Volume;
            SoundEffectsMeteor.AutoPlay = false;
            SoundEffectsMeteor.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/sounds/meteorandboom.wav"));

            SoundEffectsAlien.Volume = MainMenu.MediaPlayer.Volume;
            SoundEffectsAlien.AutoPlay = false;
            SoundEffectsAlien.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/sounds/aliensound.wav"));
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
            args.DrawingSession.DrawImage(Board, ((DesignWidth - Board.SizeInPixels.Width) / 2) - 3, ((DesignHeight - Board.SizeInPixels.Height) / 2) - 4);
            Game.DrawSelectedRect(args);
            Game.DrawPlayerPlanets(sender, args);
            Game.DrawOkayMoves(args);
            Game.DrawPlayerTurn(args);
            if (Game.MeteorStrike)
            {
                Game.DrawMeteor(args, Comet);
            }
            else
            {
                Game.DrawUnavailableRects(args, FireList);
            }
            if (Game.AlienEncounter)
            {
                Game.DrawAlien(args, Alien);
            }
            if (!Game.AnimationComplete)
            {
                Game.DrawAnimations(args);
            }
        }

        private void GameCanvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            if (Game.CheckIfWin())
            {
                Debug.WriteLine($"{Game.Players[Game.CurrentPlayerIndex].PlanetColor} won");
            }
            if (Game.MeteorStrike)
            {
                Game.UpdateMeteor();

                var playSoundMeteor = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    SoundEffectsMeteor.Play();
                });
            }
            if (Game.AlienEncounter)
            {
                Game.UpdateAlien();
                var playSoundAlien = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    SoundEffectsAlien.Play();
                });
            }
            if (!Game.AnimationComplete)
            {
                Game.UpdateAnimation();

            }
            if (Game.AnimationComplete && Game.TurnStarted)
            {
                Game.MoveComplete();
                var playSoundPlop = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    SoundEffectsPlop.Play();
                });

                if (!Game.OnlyDoubleJump && Game.SavedPosition == -1) // savedpos to prevcent next turn from happening when jumping back to start posistion
                {
                    Game.NextTurn();
                }
                else
                {
                    Game.CheckOKMoves();
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
            if (Game.AnimationComplete && !Game.MeteorStrike && !Game.AlienEncounter) // to prevent HAX by moving while animation happens???
            {
                Game.CheckIfRect_Pressed(e.GetCurrentPoint(null).Position);

                if (Game.PlanetSelected != -1 && Game.AnimationComplete)
                {
                    var playSound = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        SoundEffectsPlop.Play();
                    });

                }
            }
        }
              

       
        public void DrawPlayerTurn(CanvasAnimatedDrawEventArgs args)
        {
            //args.DrawingSession.DrawText((CurrentPlayerIndex + 1).ToString(), 50, 20, Windows.UI.Color.FromArgb(255, 255, 0, 0));
            
            Rect moveRect = new Rect(135, 100, 66, 66);
                               
             args.DrawingSession.DrawImage(Game.Players[Game.CurrentPlayerIndex].PlanetBitmap,moveRect);

        }

    }
}