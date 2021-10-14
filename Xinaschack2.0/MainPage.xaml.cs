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
        private int WinWindowCounter = 0;

        public static MediaPlayer SoundEffectsPlop;
        public static MediaPlayer SoundEffectsMeteor;
        public static MediaPlayer SoundEffectsAlien;

        private readonly int DesignWidth = 1920;
        private readonly int DesignHeight = 1080;

        private int AmountOfPlayers { get; set; }
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
            // First time running game
            if (Game == null)
            {
                AmountOfPlayers = (int)e.Parameter;
                Game = new GameBoard(DesignWidth, DesignHeight, AmountOfPlayers);

            }
            //else if (e.Parameter != null && AmountOfPlayers != (int)e.Parameter) // not from settings + new amount of players
            //{
            //    AmountOfPlayers = (int)e.Parameter;
            //    Game = new GameBoard(DesignWidth, DesignHeight, AmountOfPlayers);
            //}
            // Add reset button
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
            Board = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/GameFieldCropped2.png"));
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

            SoundEffectsAlien.Volume = MainMenu.BackgroundAudio.Volume;
            SoundEffectsPlop.AutoPlay = false;
            SoundEffectsPlop.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/sounds/ballmovesound.wav"));

            SoundEffectsAlien.Volume = MainMenu.BackgroundAudio.Volume;
            SoundEffectsMeteor.AutoPlay = false;
            SoundEffectsMeteor.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/sounds/meteorandboom.wav"));

            SoundEffectsAlien.Volume = MainMenu.BackgroundAudio.Volume;
            SoundEffectsAlien.AutoPlay = false;
            SoundEffectsAlien.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/sounds/aliensound.wav"));
        }

        private void GameCanvas_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }

        /// <summary>
        /// Draws the board, planets, OKmoves and animates the moving planets, meteor and alien.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void GameCanvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            args.DrawingSession.DrawImage(Board, (DesignWidth - Board.SizeInPixels.Width) / 2, (DesignHeight - Board.SizeInPixels.Height) / 2);
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
            if (!Game.MoveAnimationComplete)
            {
                Game.DrawMoveAnimation(args);
            }
        }

        /// <summary>
        /// Updates positions for moving planets, aliens and meteors when they should be animating. Also checks win-condition, and plays sound effects.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void GameCanvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            if (Game.CheckIfWin())
            {
                // Debug.WriteLine($"{Game.Players[Game.CurrentPlayerIndex].PlanetColor} won");
                if (WinWindowCounter == 0)
                {
                    WinWindowCounter += 1;
                    var showDialogWindow = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ContentDialog1 dialog1 = new ContentDialog1(Game.Players[Game.CurrentPlayerIndex].PlanetColor);
                        dialog1.ShowAsync();
                    });
                }
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
            if (!Game.MoveAnimationComplete)
            {
                Game.UpdateMoveAnimation();

            }
            if (Game.MoveAnimationComplete && Game.TurnStarted)
            {
                Game.MoveComplete();
                var playSoundPlop = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    SoundEffectsPlop.Play();
                });
                // Checks SavedPosition to prevent next turn from happening when jumping back to start posistion
                if (!Game.OnlyDoubleJump && Game.SavedPosition == -1) 
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
        /// Sends a Point to CheckIfRect_Pressed everytime use clicks. No click recorded when animation is happening.
        /// Plays Plop-sound when selecting Planet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (Game.MoveAnimationComplete && !Game.MeteorStrike && !Game.AlienEncounter)
            {
                Game.CheckIfRect_Pressed(e.GetCurrentPoint(null).Position);

                if (Game.PlanetSelected != -1 && Game.MoveAnimationComplete)
                {
                    var playSound = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        SoundEffectsPlop.Play();
                    });

                }
            }
        }
    }
}