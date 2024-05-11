/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-18                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System.Windows;
using System.Windows.Controls;
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
      int wLbl = 120;
      int wDat = 40;
      InitializeComponent();
      _ctlLocalName.setup("Local Name:", enDataType.STRING, 0, wLbl);
      _ctDuration.setup(MyResources.Duration + " [ms]:", enDataType.DOUBLE, 1, wLbl, wDat);
      _ctCallDist.setup(MyResources.CallDistance + " [ms]", enDataType.DOUBLE, 1, wLbl, wDat);
      _ctFreqC.setup(MyResources.CharFrequency + "[kHz]", enDataType.DOUBLE, 1, wLbl, wDat);
      _ctlFmin.setup(MyResources.MinFrequency + "[kHz]", enDataType.DOUBLE, 1, wLbl, wDat);
      _ctlFmax.setup(MyResources.MaxFrequency + "[kHz]", enDataType.DOUBLE, 1, wLbl, wDat);
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
