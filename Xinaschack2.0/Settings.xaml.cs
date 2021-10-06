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
        private double SliderValue{ get; set; }

        public Settings()
        {
            this.InitializeComponent();
        }
        private void Back2menu(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainMenu), null);
        }
        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            SliderValue = slider.Value / 100;
            MainMenu.MediaPlayer.Volume = SliderValue;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SliderValue = MainMenu.MediaPlayer.Volume * 100;
            VolumeSlider.Value = SliderValue;
            base.OnNavigatedTo(e);
        }
    }
}
