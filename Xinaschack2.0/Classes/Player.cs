using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xinaschack2._0.Classes
{
    class Player
    {
        public List<int> PlayerPositions { get; set; }
        public List<int> WinPositions { get; set; }
        public PlanetEnum PlanetColor { get; set; }
        public CanvasBitmap PlanetBitmap{ get; set; }

        public Player(PlanetEnum color, List<int> startPos, List<int> winPos)
        {
            PlanetColor = color;
            PlayerPositions = new List<int>(startPos);
            WinPositions = new List<int>(winPos);
        }

        public async Task LoadBitmapAsync(CanvasAnimatedControl sender)
        {
            switch(PlanetColor)
            {
                case PlanetEnum.Earth:
                    PlanetBitmap = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/earth256.png"));
                    break;
                case PlanetEnum.Venus:
                    PlanetBitmap = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/venustry3.png"));
                    break;
                case PlanetEnum.Mars:
                    PlanetBitmap = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/marstry5.png"));
                    break;
                case PlanetEnum.Jupiter:
                    PlanetBitmap = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/jupitertry2.png"));
                    break;
                case PlanetEnum.Moon:
                    PlanetBitmap = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/moontry2.png"));
                    break;
                case PlanetEnum.Neptune:
                    PlanetBitmap = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/neptunetry2.png"));
                    break;
            }
        }

    }
}
