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

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaktionslogik für ctlSpeciesData.xaml
  /// </summary>
  public partial class ctlSpeciesData : UserControl
  {
    public ctlSpeciesData()
    {
      InitializeComponent();
      _ctlLocalName.setup("Local Name:", enDataType.STRING, 0, 120, 200);
      _ctDuration.setup("Duration [ms]:", enDataType.DOUBLE, 1, 120, 70);
      _ctFreqC.setup("char.Frequency [kHz]", enDataType.DOUBLE, 1, 120, 70);
      _ctlSelPic.setup("select call type", 0, 150, 150, callTypeChanged);
    }

    private void callTypeChanged(int idx, string val)
    {

    }
  }
}
