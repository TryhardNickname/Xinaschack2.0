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

        public static MediaPlayer MediaPlayer;
        public MainMenu()
        {
            InitializeComponent();
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (MediaPlayer == null)
            {
                MediaPlayer = new MediaPlayer();
            }
            
            if (MediaPlayer.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
            {
                
                MediaPlayer.IsLoopingEnabled = true;
                MediaPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/sounds/SpaceSongQuieter.mp3"));
                MediaPlayer.Play();
                MediaPlayer.Volume = 0.5;
            }
        }
        private void Playbtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(amountofplayers), null);
        }

        private void Quitbtn_Click(object sender, RoutedEventArgs e)
        {           
            CoreApplication.Exit();
        }

        private void Scorebtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Highscorepage), null);
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
