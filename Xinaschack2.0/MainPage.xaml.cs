﻿using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
        // private CanvasBitmap StartScreen { get; set; }
        private CanvasBitmap Board { get; set; }

        private readonly int DesignWidth = 1280;
        private readonly int DesignHeight = 720;

        GameBoard game;

        public MainPage()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(DesignWidth, DesignHeight);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            // For now we create GameBoard here => After menu is made, we can create 
            // GameBoard when the player presses PLAY
            game = new GameBoard(DesignWidth, DesignHeight, 2);

        }

        /// <summary>
        /// Loads pictures into Bitmaps that can be used in the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private async Task CreateResourcesAsync(CanvasAnimatedControl sender)
        {
            Board = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/spelplan3.png"));

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
            // args.DrawingSession.DrawImage(StartScreen);
            args.DrawingSession.DrawImage(Board);

            game.DrawSelectedRect(args);

            game.DrawPlayerPlanets(sender, args);

            // NYI DrawPlayerTurn
            game.DrawPlayerTurn(args);

        }

        private void GameCanvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            // NYI
            // foreach Player p in Players
            //     make move

            //check if win
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

            game.CheckIfRect_Pressed(e);

        }
    }
}
