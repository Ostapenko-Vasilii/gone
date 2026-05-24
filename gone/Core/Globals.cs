using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gone.Core
{
    public class Globals
    {
        public static int ScreenW { get; set; } = 1920;
        public static int ScreenH { get; set; } = 1080;

        public static ScreensEnum currentScreen { get; set; } = ScreensEnum.Start;
        
    }
}
