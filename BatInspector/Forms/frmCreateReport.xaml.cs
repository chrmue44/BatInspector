/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using BatInspector.Controls;
using BatInspector.Properties;
using libParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmCreateReport.xaml
  /// </summary>
  public partial class frmCreateReport : Window
  {
    ViewModel _model;
    string _formDataName;
    public frmCreateReport(ViewModel model)
    {
      InitializeComponent();
      _model = model;
      _ctlReport.setup(this, model);
    }

    private void setFormDataName(string s)
    {
      _formDataName = s;
    }

    private void _btnCreate_Click(object sender, RoutedEventArgs e)
    {

      if ((_ctlReport._dtStart.SelectedDate != null) && (_ctlReport._dtEnd.SelectedDate != null))
      {

        DateTime start = (DateTime)_ctlReport._dtStart.SelectedDate;
        DateTime end = (DateTime)_ctlReport._dtEnd.SelectedDate;
        enPeriod period = (enPeriod)_ctlReport._cbPeriod.SelectedIndex;
        if (_ctlReport._cbCsvFile.IsChecked == true)
        {
          _model.SumReport.createCsvReport(start, end, period, _ctlReport._ctlRootDir.getValue(),
                                      _ctlReport._ctlDestDir.getValue(),
                                      _ctlReport._ctlCsvReportName.getValue(), _model.SpeciesInfos);
        }
        if (_ctlReport._cbWebPage.IsChecked == true)
        {
          SumReportJson rep = _model.SumReport.createWebReport(start, end, period,
                                      _ctlReport._ctlRootDir.getValue(),
                                      _ctlReport._ctlDestDir.getValue(),
                                      _ctlReport._ctlWebReportName.getValue(), _model.SpeciesInfos);
          rep.save(Path.Combine(_ctlReport._ctlDestDir.getValue(), "sumReport.json"));
          frmReportAssistant frm = new frmReportAssistant(rep, setFormDataName);
          frm.WindowStartupLocation = WindowStartupLocation.Manual;
          frm.Left = 100;
          frm.Top = 10;
          bool? ok = frm.ShowDialog();
          if (ok == true)
            _model.SumReport.createWebPage(rep, _formDataName, _model.SpeciesInfos,
                      Path.Combine(_ctlReport._ctlDestDir.getValue(), _ctlReport._ctlWebReportName.getValue()));
        }
        if(_ctlReport._cbActivityDiagram.IsChecked == true)
        {
        
          frmActivity frm = new frmActivity(_model);
          frm.setup();
          _model.SumReport.createActivityDiagAsync(start, end, period, _ctlReport._ctlRootDir.getValue(),
          _ctlReport._ctlDestDir.getValue(), _ctlReport.FilterExpression, frm.createPlot);
          frm.Show();
        }
      }
      else
        MessageBox.Show("Please specify start and end date", "Attention", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        this.Visibility = Visibility.Hidden;
      }


      private void _btnCancel_Click(object sender, RoutedEventArgs e)
      {
        this.Visibility = Visibility.Hidden;
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
        winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
      }

  }
}