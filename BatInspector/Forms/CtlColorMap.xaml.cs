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

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für CtlColorMap.xaml
  /// </summary>
  public partial class CtlColorMap : UserControl
  {
    public CtlColorMap()
    {
      InitializeComponent();
    }

    public void setup(string color, List<ColorItem> col)
    {
      _grp.Header = color;
      _ctlCol1.setup("Color Pt 1", enDataType.INT, 0);
      _ctlCol1._tb.Focusable = true;
      _ctlCol1.setValue(col[0].Color);
      _ctlCol2.setup("Color Pt 2", enDataType.INT, 0);
      _ctlCol2._tb.Focusable = true;
      _ctlCol2.setValue(col[1].Color);
      _ctlCol3.setup("Color Pt 3", enDataType.INT, 0);
      _ctlCol3.setValue(col[2].Color);
      _ctlCol3._tb.Focusable = true;
      _ctlCol4.setup("Color Pt 4", enDataType.INT, 0);
      _ctlCol4.setValue(col[3].Color);
      _ctlCol4._tb.Focusable = true;
      _ctlCol5.setup("Color Pt 5", enDataType.INT, 0);
      _ctlCol5.setValue(col[4].Color);
      _ctlCol5._tb.Focusable = true;

      _ctlVal1.setup("Value Pt 1", enDataType.INT, 0);
      _ctlVal1.setValue(col[0].Value);
      _ctlVal1._tb.Focusable = true;
      _ctlVal2.setup("Value Pt 2", enDataType.INT, 0);
      _ctlVal2.setValue(col[1].Value);
      _ctlVal2._tb.Focusable = true;
      _ctlVal3.setup("Value Pt 3", enDataType.INT, 0);
      _ctlVal3.setValue(col[2].Value);
      _ctlVal3._tb.Focusable = true;
      _ctlVal4.setup("Value Pt 4", enDataType.INT, 0);
      _ctlVal4.setValue(col[3].Value);
      _ctlVal4._tb.Focusable = true;
      _ctlVal5.setup("Value Pt 5", enDataType.INT, 0);
      _ctlVal5.setValue(col[4].Value);
      _ctlVal5._tb.Focusable = true;
    }
  }
}
