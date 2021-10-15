using System;
using System.Collections.Generic;
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
    public sealed partial class AmountOfPlayers : Page
    {
        public AmountOfPlayers()
        {
            this.InitializeComponent();
        }
    
      private void Twoplayers(object sender, RoutedEventArgs e)
      {
        Frame.Navigate(typeof(MainPage), 2);
      }

      private void ThreePlayers(object sender, RoutedEventArgs e)
      {
        Frame.Navigate(typeof(MainPage), 3);
      }

      private void FourPlayers(object sender, RoutedEventArgs e)
      {
        Frame.Navigate(typeof(MainPage), 4);
      }

      private void SixPlayers(object sender, RoutedEventArgs e)
      {
        Frame.Navigate(typeof(MainPage), 6);
      }

      private void Back2Menu(object sender, RoutedEventArgs e)
      {
            Frame.Navigate(typeof(MainMenu), null);
      }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }

}
