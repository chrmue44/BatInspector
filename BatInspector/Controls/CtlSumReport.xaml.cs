/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-18                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using BatInspector.Properties;
using libParser;
using System;
using System.IO;
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
    ViewModel _model = null;
    string _filterExpression = "";

    public CtlSumReport()
    {
      InitializeComponent();
      int lblW = 150;
      _cbPeriod.SelectedIndex = 0;
      _ctlCsvReportName.setup(MyResources.CtlSumReportReportName, enDataType.STRING, 0, lblW, true);
      _ctlCsvReportName.setValue("sum_report.csv");
      _ctlWebReportName.setup(MyResources.CtlSumReportReportName, enDataType.STRING, 0, lblW, true);
      _ctlWebReportName.setValue("sum_report.md");
      _ctlActivityDiagName.setup(MyResources.CtlSumReportReportName, enDataType.STRING, 0, lblW, true);
      _ctlActivityDiagName.setValue("activity.pdf");
      _ctlRootDir.setup(MyResources.CtlSumReportRootDirectory, 150, true, "", initDestDir);
      _ctlDestDir.setup(MyResources.CtlSumReportDstDirectory, 150, true);

    }

    public void setup(Window parent, ViewModel model)
    {
      _parent = parent;
      _model = model;
      _dtEnd.SelectedDate = DateTime.Now;
      _dtStart.SelectedDate = new DateTime(DateTime.Now.Year, 1, 1);
    }

    public string FilterExpression { get { return _filterExpression; } }

    private void initDestDir()
    {
      _ctlDestDir.setValue(Path.Combine(_ctlRootDir.getValue(), "Auswertungen"));
    }

    private void bringParentToFront()
    {
      if (_parent != null)
      {
        _parent.Topmost = true;
        _parent.Topmost = false;
      }
    }


    private void _cbFilter_DropDownOpened(object sender, EventArgs e)
    {
      _model.Filter.TempFilter = null;
      _cbFilter.Items[1] = MyResources.MainFilterNew;
    }

    private void _cbFilter_DropDownClosed(object sender, EventArgs e)
    {
      DebugLog.log("Main: Filter dropdown closed", enLogType.DEBUG);
      bool apply;
      bool resetFilter;
      CtlScatter.handleFilterDropdown(out apply, out resetFilter, _model, _cbFilter);
      if (apply)
      {
        FilterItem filter = (_cbFilter.SelectedIndex == 1) ?
                   filter = _model.Filter.TempFilter : filter = _model.Filter.getFilter(_cbFilter.Text);
        if (filter != null)
          _filterExpression = filter.Expression;
        if (resetFilter)
          _filterExpression = "";
      }
    }
  }
}
