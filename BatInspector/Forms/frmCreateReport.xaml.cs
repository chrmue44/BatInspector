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
    string _formDataName;
    public frmCreateReport()
    {
      InitializeComponent();
      _ctlReport.setup(this);
    }

    private void setFormDataName(string s, dlgVoid callBack)
    {
      _formDataName = s;
    }

    private void _btnCreate_Click(object sender, RoutedEventArgs e)
    {

      if ((_ctlReport._dtStart.SelectedDate != null) && (_ctlReport._dtEnd.SelectedDate != null))
      {

        DateTime start = (DateTime)_ctlReport._dtStart.SelectedDate;
        DateTime end = (DateTime)_ctlReport._dtEnd.SelectedDate;
        //        enPeriod period = (enPeriod)_ctlReport._cbPeriod.SelectedIndex;
        enPeriod period = enPeriod.DAILY;
        if (_ctlReport._rbCsvFile.IsChecked == true)
        {
          App.Model.SumReport.createCsvReportAsync(start, end, period, _ctlReport._ctlRootDir.getValue(),
                                      _ctlReport._ctlDestDir.getValue(),
                                      _ctlReport._ctlCsvReportName.getValue(), App.Model.SpeciesInfos, 
                                      _ctlReport.FilterExpression,
                                      App.Model.DefaultModelParams[_ctlReport._cbModel.SelectedIndex]);
        }
        if (_ctlReport._rbWebPage.IsChecked == true)
        {
          SumReportJson rep = App.Model.SumReport.createWebReport(start, end, period,
                                      _ctlReport._ctlRootDir.getValue(),
                                      _ctlReport._ctlDestDir.getValue(),
                                      _ctlReport._ctlWebReportName.getValue(),
                                      App.Model.SpeciesInfos, _ctlReport.FilterExpression, 
                                      App.Model.DefaultModelParams[_ctlReport._cbModel.SelectedIndex]);
          rep.save(Path.Combine(_ctlReport._ctlDestDir.getValue(), "sumReport.json"));
          frmReportAssistant frm = new frmReportAssistant(rep, setFormDataName);
          frm.WindowStartupLocation = WindowStartupLocation.Manual;
          frm.Left = 100;
          frm.Top = 10;
          bool? ok = frm.ShowDialog();
          if (ok == true)
            App.Model.SumReport.createWebPage(rep, _formDataName, App.Model.SpeciesInfos,
                      Path.Combine(_ctlReport._ctlDestDir.getValue(), 
                      _ctlReport._ctlWebReportName.getValue()));
        }
        if (_ctlReport._rbRichText.IsChecked == true)
        {
          SumReportJson rep = App.Model.SumReport.createWebReport(start, end, period,
                                      _ctlReport._ctlRootDir.getValue(),
                                      _ctlReport._ctlDestDir.getValue(),
                                      _ctlReport._ctlWebReportName.getValue(),
                                      App.Model.SpeciesInfos, _ctlReport.FilterExpression,
                                      App.Model.DefaultModelParams[_ctlReport._cbModel.SelectedIndex]);
          rep.save(Path.Combine(_ctlReport._ctlDestDir.getValue(), "sumReport.json"));
          frmReportAssistant frm = new frmReportAssistant(rep, setFormDataName);
          frm.WindowStartupLocation = WindowStartupLocation.Manual;
          frm.Left = 100;
          frm.Top = 10;
          bool? ok = frm.ShowDialog();
          if (ok == true)
            App.Model.SumReport.createRichText(rep, _formDataName, App.Model.SpeciesInfos,
                      Path.Combine(_ctlReport._ctlDestDir.getValue(),
                      _ctlReport._ctlWebReportName.getValue()));
        }
        if (_ctlReport._rbActivityDiagram.IsChecked == true)
        {
        
          App.Model.SumReport.createActivityDiagAsync(start, end, period, _ctlReport._ctlRootDir.getValue(),
          _ctlReport._ctlDestDir.getValue(), App.Model.DefaultModelParams[_ctlReport._cbModel.SelectedIndex], _ctlReport.FilterExpression,   //TODO find another way for multiple models
          _ctlReport._ctlActivityDiagName.getValue(), showActivityDiagram);
        }
      }
      else
        MessageBox.Show("Please specify start and end date", "Attention", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        this.Visibility = Visibility.Hidden;
      }

    private void showActivityDiagram(ActivityData data, string bmpName)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new dlgShowActivityDiag(showActivityDiagram), data, bmpName);
      }
      else
      {
        frmActivity frm = new frmActivity(bmpName);
        frm.setup();
        frm.Show();
        frm.createPlot(data);
      }
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