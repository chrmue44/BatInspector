using BatInspector.Properties;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for ctlActivityDiagram.xaml
  /// </summary>
  public partial class ctlActivityDiagram : UserControl
  {
    ViewModel _model = null;

    public ctlActivityDiagram()
    {
      InitializeComponent();
    }

    public void setup(ViewModel model)
    {
      _model = model;
      int lblW = 150;
      _ctrlTitle.setup(BatInspector.Properties.MyResources.DiagramTitle, enDataType.STRING, 0, lblW, true);
    }


    public void createPlot(List<List<int>> hm)
    {
      
      _heatView.Model = _heatDiagram.createHeatMap(hm, _ctrlTitle.getValue());
      _heatView.InvalidatePlot(); 
    }
  }
}
