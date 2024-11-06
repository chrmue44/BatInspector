using libParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for FrmLocationView.xaml
  /// </summary>
  public partial class FrmLocationView : Window
  {
    public FrmLocationView()
    {
      InitializeComponent();
      this.Title = "Location";
    }

    public void navigate(string title, string location, int zoom)
    {
      this.Title = BatInspector.Properties.MyResources.FrmLocationLocationOfRecordingFor + " " + title;
      string[] loc = location.Split(' ');
      if (loc.Length > 1)
      {
        //string url = Utils.BingMapUrl(location, title, zoom);
        string url = Utils.OsmMapUrl(loc[0], loc[1], "", zoom);
        _wb.Source = new Uri(url);
      }     
    }
  }
}
