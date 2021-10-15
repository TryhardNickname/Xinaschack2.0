using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Xinaschack2._0
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        private double SliderValue { get; set; }
        private string ReturnTo { get; set; }

        public Settings()
        {
            this.InitializeComponent();
        }
        /// <summary>
        /// Back button returns to page that was active before.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back2Menu(object sender, RoutedEventArgs e)
        {
            if (ReturnTo == "Menu")
            {
                Frame.Navigate(typeof(MainMenu), null);
            }
            if (ReturnTo == "Game")
            {
                Frame.Navigate(typeof(MainPage), null); 
            }

        }
        /// <summary>
        /// Changes MainMenus static Mediaplayers Volume. Also affects MainPages Soundeffects.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            SliderValue = slider.Value / 100;
            MainMenu.BackgroundAudio.Volume = SliderValue;

            if (MainPage.SoundEffectsAlien != null)
            {
                MainPage.SoundEffectsAlien.Volume = SliderValue;
                MainPage.SoundEffectsMeteor.Volume = SliderValue;
                MainPage.SoundEffectsPlop.Volume = SliderValue;
            }
        }

        /// <summary>
        /// Saves ReturnTo to know what page we came from. Also sets the slider to active Volume position
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ReturnTo = e.Parameter.ToString();
            SliderValue = MainMenu.BackgroundAudio.Volume * 100;
            VolumeSlider.Value = SliderValue;

            base.OnNavigatedTo(e);
        }
    }
}
