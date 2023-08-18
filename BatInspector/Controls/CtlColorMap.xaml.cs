/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2022: Christian Müller chrmue44(at)gmail(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/
using System.Collections.Generic;
using System.Windows.Controls;

using BatInspector.Properties;
namespace BatInspector.Controls
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
      _ctlCol1.setup(MyResources.ColorMapColorPt + " 1", enDataType.INT, 0);
      _ctlCol1._tb.Focusable = true;
      _ctlCol1.setValue(col[0].Color);
      _ctlCol2.setup(MyResources.ColorMapColorPt + " 2", enDataType.INT, 0);
      _ctlCol2._tb.Focusable = true;
      _ctlCol2.setValue(col[1].Color);
      _ctlCol3.setup(MyResources.ColorMapColorPt + " 3", enDataType.INT, 0);
      _ctlCol3.setValue(col[2].Color);
      _ctlCol3._tb.Focusable = true;
      _ctlCol4.setup(MyResources.ColorMapColorPt + " 4", enDataType.INT, 0);
      _ctlCol4.setValue(col[3].Color);
      _ctlCol4._tb.Focusable = true;
      _ctlCol5.setup(MyResources.ColorMapColorPt + " 5", enDataType.INT, 0);
      _ctlCol5.setValue(col[4].Color);
      _ctlCol5._tb.Focusable = true;

      _ctlVal1.setup(MyResources.ColorMapValuePt + " 1", enDataType.INT, 0);
      _ctlVal1.setValue(col[0].Value);
      _ctlVal1._tb.Focusable = true;
      _ctlVal2.setup(MyResources.ColorMapValuePt + " 2", enDataType.INT, 0);
      _ctlVal2.setValue(col[1].Value);
      _ctlVal2._tb.Focusable = true;
      _ctlVal3.setup(MyResources.ColorMapValuePt + " 3", enDataType.INT, 0);
      _ctlVal3.setValue(col[2].Value);
      _ctlVal3._tb.Focusable = true;
      _ctlVal4.setup(MyResources.ColorMapValuePt + " 4", enDataType.INT, 0);
      _ctlVal4.setValue(col[3].Value);
      _ctlVal4._tb.Focusable = true;
      _ctlVal5.setup(MyResources.ColorMapValuePt + " 5", enDataType.INT, 0);
      _ctlVal5.setValue(col[4].Value);
      _ctlVal5._tb.Focusable = true;
    }
  }
}
