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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BatInspector.Properties;

namespace BatInspector.Controls
{
  public delegate void dlgShowZoom(string species);
  /// <summary>
  /// Interaktionslogik für ctlSpeciesData.xaml
  /// </summary>
  public partial class ctlSpeciesData : UserControl
  {
    dlgShowZoom _dlg;
    public ctlSpeciesData()
    {
      InitializeComponent();
      _ctlLocalName.setup("Local Name:", enDataType.STRING, 0, 120, 200);
      _ctDuration.setup(MyResources.Duration + " [ms]:", enDataType.DOUBLE, 1, 120, 70);
      _ctCallDist.setup(MyResources.CallDistance + " [ms]", enDataType.DOUBLE, 1, 120, 70);
      _ctFreqC.setup(MyResources.CharFrequency + "[kHz]", enDataType.DOUBLE, 1, 120, 70);
      _ctlFmin.setup(MyResources.MinFrequency + "[kHz]", enDataType.DOUBLE, 1, 120, 70);
      _ctlFmax.setup(MyResources.MaxFrequency + "[kHz]", enDataType.DOUBLE, 1, 120, 70);
      _ctlSelPic.setup(MyResources.SelectCallType, 0, 150, 150, callTypeChanged);
    }

    public void setDelegate(dlgShowZoom dlg)
    {
      _dlg = dlg;
    }
    private void callTypeChanged(int idx, string val)
    {

    }


    private void _btnExample_Click(object sender, RoutedEventArgs e)
    {
      if(_dlg != null)
        _dlg(_ctlLocalName.getValue());
    }
  }
}
