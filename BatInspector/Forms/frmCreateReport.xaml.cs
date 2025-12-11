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
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Markup;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmCreateReport.xaml
  /// </summary>
  public partial class frmCreateReport : Window
  {
    string _formDataName;
    SumReportJson _report;
    DateTime _start;
    DateTime _end;
    string _rootDir;
    string _dstDir;
    string _reportName;
    int _selectedModelIndex;
    string _filterExpression;
    bool _skipReportGeneration = false;
    enPeriod _period;
    public frmCreateReport()
    {
      InitializeComponent();
      _ctlReport.setup(this);
    }

    private void setFormDataName(string s, dlgVoid callBack)
    {
      _formDataName = s;
    }

    private void threadCreateWebReport()
    {
      if (!_skipReportGeneration)
      {
        _report = App.Model.SumReport.createWebReport(_start, _end, _period,
                              _rootDir, _dstDir, _reportName,
                              _filterExpression,
                              App.Model.DefaultModelParams[_selectedModelIndex]);
        _report.save(Path.Combine(_dstDir, AppParams.SUM_REPORT_JSON));
      }
      else
      {
        _report = SumReportJson.loadFrom(Path.Combine(_dstDir, AppParams.SUM_REPORT_JSON));
      }
      showReportDialog(true, false);
    }

    private void threadCreateRichTextReport()
    {
      if (!_skipReportGeneration)
      {
        _report = App.Model.SumReport.createWebReport(_start, _end, _period,
                              _rootDir, _dstDir, _reportName,
                              _filterExpression,
                              App.Model.DefaultModelParams[_selectedModelIndex]);
        _report.save(Path.Combine(_dstDir, AppParams.SUM_REPORT_JSON));
      }
      else
      {
        _report =  SumReportJson.loadFrom(Path.Combine(_dstDir, AppParams.SUM_REPORT_JSON));
      }
      showReportDialog(false, true);
    }

    delegate void dlgShowReportDialog(bool web, bool rtf);
    private void showReportDialog(bool webPage, bool richText)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new dlgShowReportDialog(showReportDialog), webPage, richText);
      }
      else
      {
        frmReportAssistant frm = new frmReportAssistant(_report, setFormDataName);
        frm.WindowStartupLocation = WindowStartupLocation.Manual;
        frm.Left = 100;
        frm.Top = 10;
        bool? ok = frm.ShowDialog();
        if (ok == true)
        {
          if(webPage)
            App.Model.SumReport.createWebPage(_report, _formDataName, App.Model.SpeciesInfos,
                    Path.Combine(_ctlReport._ctlDestDir.getValue(),
                    _ctlReport._ctlWebReportName.getValue()));
          if (richText)
            App.Model.SumReport.createDocument(enDocType.HTML, _report, _formDataName, App.Model.SpeciesInfos,
                      Path.Combine(_ctlReport._ctlDestDir.getValue(),
                      _ctlReport._ctlRichTextName.getValue()));

        }
      }
    }

    private bool checkIfSkipReportGeneration(string dir, string fileName)
    {
      string path = Path.Combine(dir, fileName);
      if (File.Exists(path))
      {
        MessageBoxResult res = MessageBox.Show(MyResources.msgFrmReport,MyResources.msgQuestion,MessageBoxButton.YesNo,MessageBoxImage.Question);
        if(res == MessageBoxResult.Yes)
          return true;
      }
      return false;
    }

    private void _btnCreate_Click(object sender, RoutedEventArgs e)
    {

      if ((_ctlReport._dtStart.SelectedDate != null) && (_ctlReport._dtEnd.SelectedDate != null))
      {

        _start = (DateTime)_ctlReport._dtStart.SelectedDate;
        _end = (DateTime)_ctlReport._dtEnd.SelectedDate;
        _rootDir = _ctlReport._ctlRootDir.getValue();
        _dstDir = _ctlReport._ctlDestDir.getValue();
        _reportName = _ctlReport._ctlWebReportName.getValue();
        _selectedModelIndex = _ctlReport._cbModel.SelectedIndex;
        _filterExpression = _ctlReport.FilterExpression;
        //        enPeriod period = (enPeriod)_ctlReport._cbPeriod.SelectedIndex;
        _period = enPeriod.DAILY;
        if (_ctlReport._rbCsvFile.IsChecked == true)
        {
          App.Model.SumReport.createCsvReportAsync(_start, _end, _period, _ctlReport._ctlRootDir.getValue(),
                                      _ctlReport._ctlDestDir.getValue(),
                                      _ctlReport._ctlCsvReportName.getValue(), App.Model.SpeciesInfos, 
                                      _ctlReport.FilterExpression,
                                      App.Model.DefaultModelParams[_ctlReport._cbModel.SelectedIndex]);
        }
        if (_ctlReport._rbWebPage.IsChecked == true)
        {
          _skipReportGeneration = checkIfSkipReportGeneration(_dstDir, AppParams.SUM_REPORT_JSON);          
          Thread t = new Thread(threadCreateWebReport);
          t.Start();
        }
        if (_ctlReport._rbRichText.IsChecked == true)
        {
          _skipReportGeneration = checkIfSkipReportGeneration(_dstDir, AppParams.SUM_REPORT_JSON);
          Thread t = new Thread(threadCreateRichTextReport);
          t.Start();
        }
        if (_ctlReport._rbActivityDiagram.IsChecked == true)
        {
        
          App.Model.SumReport.createActivityDiagAsync(_start, _end, _period, _ctlReport._ctlRootDir.getValue(),
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