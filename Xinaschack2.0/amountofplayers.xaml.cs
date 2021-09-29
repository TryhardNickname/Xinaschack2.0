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
    public sealed partial class amountofplayers : Page
    {
        public amountofplayers()
        {
            this.InitializeComponent();
        }
    

      private void Twoplayers(object sender, RoutedEventArgs e)
      {
        Frame.Navigate(typeof(MainPage), 2);
      }

      private void threeplayers(object sender, RoutedEventArgs e)
      {
        Frame.Navigate(typeof(MainPage), 3);
      }

      private void fourplayers(object sender, RoutedEventArgs e)
      {
        Frame.Navigate(typeof(MainPage), 4);
      }

      private void sixplayers(object sender, RoutedEventArgs e)
      {
        Frame.Navigate(typeof(MainPage), 6);
      }

    }

}
