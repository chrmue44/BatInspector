using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatInspector
{
  public class AppParams
  {
    public static int WaterfallHeight { get; set; } = 256;
    public static int WaterfallWidth { get; set; } = 512;
    public static uint FftWidth { get; set; } = 512;
    public static Color ColorXtLine { get; set; } = Color.Black;
    public static Color ColorXtBackground { get; set; } = Color.LightGray;
  }
}
