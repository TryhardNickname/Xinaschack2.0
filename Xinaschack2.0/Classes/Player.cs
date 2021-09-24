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
            //OldPlayerPositions = new List<int>();
        }

        public async Task LoadBitmapAsync(CanvasAnimatedControl sender)
        {
            //remember to make png smaller OR use transform2deffet etc
            switch(PlanetColor)
            {
                case PlanetEnum.Earth:
                    PlanetBitmap = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/earth_mini34x34.png"));
                    break;
                case PlanetEnum.Venus:
                    PlanetBitmap = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/venus.png"));
                    break;
                case PlanetEnum.Mars:
                    PlanetBitmap = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/mars.png"));
                    break;
                case PlanetEnum.Jupiter:
                    PlanetBitmap = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/jupiter.png"));
                    break;
                case PlanetEnum.Mercury:
                    PlanetBitmap = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/mercury.png"));
                    break;
                case PlanetEnum.Neptune:
                    PlanetBitmap = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Images/neptune.png"));
                    break;
            }
        }

    }
}
