/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-18                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using BatInspector.Properties;
using System;
using System.Windows;
using System.Windows.Controls;


namespace BatInspector.Controls
{
  /// <summary>
  /// Interaktionslogik für CtlSumReport.xaml
  /// </summary>
  public partial class CtlSumReport : UserControl
  {
    Window _parent = null;
    public CtlSumReport()
    {
      InitializeComponent();
      _cbPeriod.SelectedIndex = 0;
      _ctlCsvReportName.setup(MyResources.CtlSumReportReportName, enDataType.STRING, 0, 150, true);
      _ctlCsvReportName.setValue("sum_report.csv");
      _ctlWebReportName.setup(MyResources.CtlSumReportReportName, enDataType.STRING, 0, 150, true);
      _ctlWebReportName.setValue("sum_report.md");
      _ctlRootDir.setup(MyResources.CtlSumReportRootDirectory, 150, true);
      _ctlDestDir.setup(MyResources.CtlSumReportDstDirectory, 150, true);

      }

    public void setup( Window parent)
    {
      _parent = parent;
      _dtEnd.SelectedDate = DateTime.Now;
      _dtStart.SelectedDate = new DateTime(DateTime.Now.Year, 1, 1);
    }


    private void bringParentToFront()
    {
      if (_parent != null)
      {
        _parent.Topmost = true;
        _parent.Topmost = false;
      }
    }
  }
}
