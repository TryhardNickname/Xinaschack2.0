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
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
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
    public sealed partial class MainMenu : Page
    {

        public static MediaPlayer BackgroundAudio;
        public MainMenu()
        {
            InitializeComponent();
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
        }
        /// <summary>
        /// Starts Mediaplayer if it is not already playing background music
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (BackgroundAudio == null)
            {
                BackgroundAudio = new MediaPlayer();
            }

            if (BackgroundAudio.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
            {

                BackgroundAudio.IsLoopingEnabled = true;
                BackgroundAudio.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/sounds/SpaceSongQuieter.mp3"));
                BackgroundAudio.Play();
                BackgroundAudio.Volume = 0.5;
            }
        }

        /// <summary>
        /// All these methods below are buttons that do things such as starting the game, going to the highscore page and so on.
        /// </summary>

        private void Playbtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AmountOfPlayers), null);
        }

        private void Quitbtn_Click(object sender, RoutedEventArgs e)
        {
            CoreApplication.Exit();
        }

        private void Scorebtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HighScorePage), null);
        }

        private void Tutorialbtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Tutorial), null);
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Settings), "Menu");
        }
    }
}
