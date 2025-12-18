using BatInspector.Forms;
using BatInspector.Properties;
using libParser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace BatInspector.Controls
{

  /// <summary>
  /// Interaction logic for CtlMySql.xaml
  /// </summary>
  public partial class CtlMySql : UserControl
  {
    bool _queryCollapsed = false;
    List<CtlMySqlFieldSelect> _listCb = new List<CtlMySqlFieldSelect>();
    string _columnsList;
    int _limitRows = 1000;
    string _filterExpression = "";
    List<sqlRow> _query = null;
    string[] _order = new string[5];

    const int IDX_NONE = 0;
    const int IDX_ALL = 1;
    const int IDX_BATINPECTOR = 2;
    const int IDX_CUSTOM = 3;
    public CtlMySql()
    {
      InitializeComponent();
      _cbLimit.Items.Clear();
      _cbLimit.Items.Add("10 " + MyResources.Rows);
      _cbLimit.Items.Add("100 " + MyResources.Rows);
      _cbLimit.Items.Add("1000 " + MyResources.Rows);
      _cbLimit.Items.Add("10000 " + MyResources.Rows);
      _cbLimit.Items.Add("50000 " + MyResources.Rows);
      //      _cbLimit.Items.Add("no limit");

      _ctlPreSelect.setup(MyResources.Preselection, 0, 100, 220, setPreSelection);
      string[] items = { MyResources.MainBtnNone, MyResources.MainBtnAll,
                         MyResources.CtlMySql_InspectWithBatIspector, MyResources.CtlMySql_Custom};
      _ctlPreSelect.setItems(items);

      _listCb.Clear();
      _sp1.Children.Clear();
      _sp2.Children.Clear();

      addCheckBox("Date", 1);
      addCheckBox(DBBAT.LOC, 1);
      addCheckBox("PrjCreator", 1);
      addCheckBox(DBBAT.PRJ_NOTES, 1);
      addCheckBox("RecordingDevice", 1);
      addCheckBox("MicrophoneId", 1);
      addCheckBox("Classifier", 1);
      addCheckBox("Model", 1);
      addCheckBox(DBBAT.PATH_TO_WAV, 1);
      addCheckBox(DBBAT.LAT, 2);
      addCheckBox(DBBAT.LON, 2);
      addCheckBox(DBBAT.WAV_FILE_NAME, 2);
      addCheckBox(DBBAT.SAMPLE_RATE, 2);
      addCheckBox(DBBAT.FILE_LENGTH, 2);
      addCheckBox(DBBAT.RECORDING_TIME, 2);
      addCheckBox(DBBAT.TEMP, 2);
      addCheckBox(DBBAT.HUMI, 2);
      addCheckBox(DBBAT.CALLNR, 2);
      addCheckBox(DBBAT.SNR, 2);
      addCheckBox(DBBAT.START_TIME, 3);
      addCheckBox(DBBAT.SPEC_MAN, 3);
      addCheckBox(DBBAT.SPEC_AUTO, 3);
      addCheckBox(DBBAT.PROB, 3);
      addCheckBox(DBBAT.FMIN, 3);
      addCheckBox(DBBAT.FMAX, 3);
      addCheckBox(DBBAT.FMAXAMP, 3);
      addCheckBox(DBBAT.CALL_LEN, 3);
      addCheckBox(DBBAT.CALL_DST, 3);
      addCheckBox($"calls.{DBBAT.REM}", 3);
      _lblStatus.Background = new SolidColorBrush(Colors.Red);
      init();
    }

    public void init()
    {
      for (int i = 0; i < _order.Length; i++)
        _order[i] = "";
      _filterExpression = "";
      _cbLimit.SelectedIndex = 2;
      _limitRows = 1000;
      _cbFilter.SelectedIndex = 0;
    }


    private void setPreSelection(int index, string val)
    {
      switch (_ctlPreSelect.SelectIndex)
      {
        case IDX_NONE:
          setFieldSelector(DBBAT.DATE, false, 0, false);
          setFieldSelector(DBBAT.LOC, false, 0, false);
          setFieldSelector(DBBAT.PRJCREATOR, false, 0, false);
          setFieldSelector(DBBAT.RECDEV, false, 0, false);
          setFieldSelector(DBBAT.MICID, false, 0, false);
          setFieldSelector(DBBAT.CLASSI, false, 0, false);
          setFieldSelector(DBBAT.MODEL, false, 0, false);
          setFieldSelector(DBBAT.PRJ_NOTES, false, 0, false);
          setFieldSelector(DBBAT.PATH_TO_WAV, false, 0, false);
          setFieldSelector(DBBAT.LAT, false, 0, false);
          setFieldSelector(DBBAT.LON, false, 0, false);
          setFieldSelector(DBBAT.WAV_FILE_NAME, false, 0, false);
          setFieldSelector(DBBAT.SAMPLE_RATE, false, 0, false);
          setFieldSelector(DBBAT.FILE_LENGTH, false, 0, false);
          setFieldSelector(DBBAT.RECORDING_TIME, false, 0, false);
          setFieldSelector(DBBAT.TEMP, false, 0, false);
          setFieldSelector(DBBAT.HUMI, false, 0, false);
          setFieldSelector(DBBAT.CALLNR, false, 0, false);
          setFieldSelector(DBBAT.SNR, false, 0, false);
          setFieldSelector(DBBAT.START_TIME, false, 0, false);
          setFieldSelector(DBBAT.SPEC_MAN, false, 0, false);
          setFieldSelector(DBBAT.SPEC_AUTO, false, 0, false);
          setFieldSelector(DBBAT.PROB, false, 0, false);
          setFieldSelector(DBBAT.FMIN, false, 0, false);
          setFieldSelector(DBBAT.FMAX, false, 0, false);
          setFieldSelector(DBBAT.FMAXAMP, false, 0, false);
          setFieldSelector(DBBAT.CALL_LEN, false, 0, false);
          setFieldSelector(DBBAT.CALL_DST, false, 0, false);
          setFieldSelector("calls.Remarks", false, 0, false);
          _tbQuery.Text = "";
          break;
        case IDX_ALL:
          setFieldSelector(DBBAT.DATE, true, 0, false);
          setFieldSelector(DBBAT.LOC, true, 0, false);
          setFieldSelector(DBBAT.PRJCREATOR, true, 0, false);
          setFieldSelector(DBBAT.PRJ_NOTES, true, 0, false);
          setFieldSelector(DBBAT.RECDEV, true, 0, false);
          setFieldSelector(DBBAT.MICID, true, 0, false);
          setFieldSelector(DBBAT.CLASSI, true, 0, false);
          setFieldSelector(DBBAT.MODEL, true, 0, false);
          setFieldSelector(DBBAT.PATH_TO_WAV, true, 0, false);
          setFieldSelector(DBBAT.LAT, true, 0, false);
          setFieldSelector(DBBAT.LON, true, 0, false);
          setFieldSelector(DBBAT.WAV_FILE_NAME, true, 0, false);
          setFieldSelector(DBBAT.FILE_LENGTH, true, 0, false);
          setFieldSelector(DBBAT.SAMPLE_RATE, true, 0, false);
          setFieldSelector(DBBAT.RECORDING_TIME, true, 0, false);
          setFieldSelector(DBBAT.TEMP, true, 0, false);
          setFieldSelector(DBBAT.HUMI, true, 0, false);
          setFieldSelector(DBBAT.CALLNR, true, 0, false);
          setFieldSelector(DBBAT.SNR, true, 0, false);
          setFieldSelector(DBBAT.START_TIME, true, 0, false);
          setFieldSelector(DBBAT.SPEC_MAN, true, 0, false);
          setFieldSelector(DBBAT.SPEC_AUTO, true, 0, false);
          setFieldSelector(DBBAT.PROB, true, 0, false);
          setFieldSelector(DBBAT.FMIN, true, 0, false);
          setFieldSelector(DBBAT.FMAX, true, 0, false);
          setFieldSelector(DBBAT.FMAXAMP, true, 0, false);
          setFieldSelector(DBBAT.CALL_LEN, true, 0, false);
          setFieldSelector(DBBAT.CALL_DST, true, 0, false);
          setFieldSelector($"calls.{DBBAT.REM}", true, 0, false);
          break;
        case IDX_BATINPECTOR:
          setFieldSelector(DBBAT.DATE, false, 0, false);
          setFieldSelector(DBBAT.LOC, false, 0, false);
          setFieldSelector(DBBAT.PRJCREATOR, false, 0, false);
          setFieldSelector(DBBAT.PRJ_NOTES, false, 0, false);
          setFieldSelector(DBBAT.RECDEV, true, 0, false);
          setFieldSelector(DBBAT.MICID, true, 0, false);
          setFieldSelector(DBBAT.CLASSI, true, 0, false);
          setFieldSelector(DBBAT.MODEL, true, 0, false);
          setFieldSelector(DBBAT.PATH_TO_WAV, true, 0, false);
          setFieldSelector(DBBAT.LAT, true, 0, false);
          setFieldSelector(DBBAT.LON, true, 0, false);
          setFieldSelector(DBBAT.WAV_FILE_NAME, true, 1, false);
          setFieldSelector(DBBAT.SAMPLE_RATE, true, 0, false);
          setFieldSelector(DBBAT.FILE_LENGTH, true, 0, false);
          setFieldSelector(DBBAT.RECORDING_TIME, true, 0, false);
          setFieldSelector(DBBAT.TEMP, true, 0, false);
          setFieldSelector(DBBAT.HUMI, true, 0, false);
          setFieldSelector(DBBAT.CALLNR, true, 2, false);
          setFieldSelector(DBBAT.SNR, true, 0, false);
          setFieldSelector(DBBAT.START_TIME, true, 0, false);
          setFieldSelector(DBBAT.SPEC_MAN, true, 0, false);
          setFieldSelector(DBBAT.SPEC_AUTO, true, 0, false);
          setFieldSelector(DBBAT.PROB, true, 0, false);
          setFieldSelector(DBBAT.FMIN, true, 0, false);
          setFieldSelector(DBBAT.FMAX, true, 0, false);
          setFieldSelector(DBBAT.FMAXAMP, true, 0, false);
          setFieldSelector(DBBAT.CALL_LEN, true, 0, false);
          setFieldSelector(DBBAT.CALL_DST, true, 0, false);
          setFieldSelector($"calls.{DBBAT.REM}", true, 0, false);
          break;
        default:
          break;

      }
      _cb_Click(this, null);
    }

    private void _dg_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      try
      {
        // https://blog.scottlogic.com/2008/12/02/wpf-datagrid-detecting-clicked-cell-and-row.html

        DependencyObject dep = (DependencyObject)e.OriginalSource;

        // iteratively traverse the visual tree
        while ((dep != null) && !(dep is DataGridCell))
          dep = VisualTreeHelper.GetParent(dep);

        if (dep == null)
          return;

        QueryItem it = null;
        if (dep is DataGridCell)
        {
          DataGridCell cell = dep as DataGridCell;
          // navigate further up the tree
          while ((dep != null) && !(dep is DataGridRow))
            dep = VisualTreeHelper.GetParent(dep);

          DataGridRow row = dep as DataGridRow;
          it = row.DataContext as QueryItem;
        }

        if (it != null)
        {
          double.TryParse(it.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat);
          double.TryParse(it.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon);
          List<string> species = Project.createSpeciesList(lat, lon);
          AnalysisFile f = App.Model.MySQL.DbBats.fillAnalysisFromQuery(it);
          App.MainWin.setZoom(it.WavFileName, f, it.PathToWavs, null, enModel.BAT_DETECT2, species);
          int callIdx = f.findCallIdx(it.CallNr);
          if (callIdx >= 0)
            App.MainWin.changeCallInZoom(callIdx);
        }
        DebugLog.log("MySql:Query double click", enLogType.DEBUG);
      }
      catch (Exception ex)
      {
        DebugLog.log("MySql:Query double click failed: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _btnCollapse_Click(object sender, RoutedEventArgs e)
    {
      collapse(!_queryCollapsed);
    }


    private void collapse(bool on)
    {
      if (on)
      {
        _grid.RowDefinitions[1].Height = new GridLength(25);
        _grid.RowDefinitions[2].Height = new GridLength(0);
        _queryCollapsed = true;
        _btnCollapse.Content = "v";
        _spQuery.Background = (SolidColorBrush)App.Current.Resources["colorBackGroundWindow"];
      }
      else
      {
        _grid.RowDefinitions[1].Height = new GridLength(320);
        _grid.RowDefinitions[2].Height = new GridLength(35);
        _queryCollapsed = false;
        _btnCollapse.Content = "<";
        _spQuery.Background = (SolidColorBrush)App.Current.Resources["colorBackGround"];

      }
    }


    private void _btnApply_Click(object sender, RoutedEventArgs e)
    {
      collapse(true);
      _query = App.Model.MySQL.execQuery(_tbQuery.Text);
      if (App.Model.MySQL.DbBats.IsOpen)
        App.Model.MySQL.DbBats.QueryResult = _query;
      if (App.Model.MySQL.DbBirds.IsOpen)
        App.Model.MySQL.DbBirds.QueryResult = _query;

      initDataSource(_query);
    }

    private void createQuery()
    {
      if (string.IsNullOrEmpty(_columnsList))
        return;
      SqlQueryBuilder b = new SqlQueryBuilder();
      b.init();
      b.addSelectStatement($"\n{_columnsList}\nFROM projects \nJOIN files ON projects.id = files.ProjectId \nJOIN calls ON files.id = FileId");
      if (!string.IsNullOrEmpty(_filterExpression))
        b.addWhereStatement(_filterExpression);
      string order = "";
      for (int i = 0; i < _order.Length; i++)
      {
        if (_order[i] != "")
        {
          if (order != "")
            order += ",";
          order += _order[i];
        }
      }
      if (order != "")
        b.addOrderStatement(order);
      b.addLimitStatement(_limitRows);
      _tbQuery.Text = b.Command;
    }


    /// <summary>
    /// conversion  WPF  |  MySQL
    ///             -------------
    ///            "___" | "."
    ///            "__"  | "_"
    /// WPF doesn't show '_' in Names on screen
    /// C# doesen't allow '.' in variable names
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private string convertNameSQLtoWPF(string name)
    {
      return name.Replace("_", "__").Replace(".", "___");
    }

    private string convertNameWPFtoSQL(string name)
    {
      return name.Replace("___", ".").Replace("__", "_");
    }

    private void _cbFilter_DropDownOpened(object sender, EventArgs e)
    {
      App.Model.Filter.TempFilter = null;
      _cbFilter.Items[1] = MyResources.MainFilterNew;
    }

    private void _cbFilter_DropDownClosed(object sender, EventArgs e)
    {
      DebugLog.log("Main: Filter dropdown closed", enLogType.DEBUG);
      bool apply;
      bool resetFilter;
      CtlScatter.handleFilterDropdown(out apply, out resetFilter, _cbFilter, false);
      if (apply)
      {
        FilterItem filter = (_cbFilter.SelectedIndex == 1) ?
                    App.Model.Filter.TempFilter : App.Model.Filter.getFilter(_cbFilter.Text);
        if (filter != null)
          _filterExpression = DataBase.translateFilterExpressionToMySQL(filter.Expression);
      }
      if (resetFilter)
        _filterExpression = "";
      createQuery();
      collapse(false);
    }

    private void setFieldSelector(string name, bool select, int order, bool reverse)
    {
      CtlMySqlFieldSelect ctl = null;
      foreach (CtlMySqlFieldSelect c in _listCb)
      {
        if (name == c.FieldName)
        {
          ctl = c;
          break;
        }
      }
      if (ctl != null)
      {
        ctl.Selected = select;
        ctl.Order = order;
        ctl.Reverse = reverse;
      }

    }

    private void _cb_Click(object sender, RoutedEventArgs e)
    {
      _columnsList = "";
      int colsPerLine = 0;
      foreach (CtlMySqlFieldSelect cb in _listCb)
      {
        if (cb.Selected)
        {
          colsPerLine++;
          if (_columnsList != "")
          {
            _columnsList += ",";
            if (colsPerLine > 5)
            {
              colsPerLine = 0;
              _columnsList += "\n";
            }
          }
          _columnsList += convertNameWPFtoSQL(cb.FieldName);
        }
        if (cb.Order > 0)
        {
          _order[cb.Order - 1] = cb.FieldName;
          if (cb.Reverse)
            _order[cb.Order - 1] += " DESC";
        }
        else
        {
          for (int i = 0; i < _order.Length; i++)
          {
            if (_order[i] == cb.FieldName)
              _order[i] = "";
          }
        }
      }
      if (sender != this)
        _ctlPreSelect.SelectIndex = IDX_CUSTOM;
      createQuery();
    }

    private void addCheckBox(string name, int col)
    {
      CtlMySqlFieldSelect c = new CtlMySqlFieldSelect();
      c.setup(name, _cb_Click);

      _listCb.Add(c);
      if (col == 1)
        _sp1.Children.Add(c);
      if (col == 2)
        _sp2.Children.Add(c);
      if (col == 3)
        _sp3.Children.Add(c);
    }

    private void initDataSource(List<sqlRow> dat)
    {
      try
      {
        _dg.ItemsSource = null;
        if (dat.Count == 0)
          return;

        List<QueryItem> list = new List<QueryItem>();
        int line = 0;
        foreach (sqlRow row in dat)
        {
          QueryItem it = new QueryItem();
          it.setValues(row);
          line++;
          it.line = line;
          list.Add(it);
        }

        _dg.ItemsSource = list;

        for (int i = 1; i < _dg.Columns.Count; i++)
        {
          _dg.Columns[i].Visibility = Visibility.Collapsed;
          _dg.Columns[i].IsReadOnly = true;
        }
        for (int j = 0; j < dat[0].Fields.Count; j++)
        {
          for (int i = 1; i < _dg.Columns.Count; i++)
          {
            if (convertNameWPFtoSQL((string)_dg.Columns[i].Header).IndexOf(dat[0].Fields[j].Name) >= 0)
              _dg.Columns[i].Visibility = Visibility.Visible;
          }
        }
      }
      catch (Exception ex)
      {
        DebugLog.log($"error query data: {ex.ToString()}", enLogType.ERROR);
      }
    }

    private void _cbLimit_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      switch (_cbLimit.SelectedIndex)
      {
        case 0:
          _limitRows = 10; break;
        case 1:
          _limitRows = 100; break;
        case 2:
          _limitRows = 1000; break;
        case 3:
          _limitRows = 10000; break;
        case 4:
          _limitRows = 50000; break;
      }
      collapse(false);
      createQuery();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      if ((_query != null) && (_query.Count > 0))
      {
        System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
        dlg.AddExtension = true;
        dlg.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
          string fName = dlg.FileName;
          App.Model.MySQL.exportQuery(_query, fName);
        }
      }
      else
        DebugLog.log("Export data: nothing to export, query is empty", enLogType.INFO);
    }

    private void _btnExportPrj_Click(object sender, RoutedEventArgs e)
    {
      frmCreatePrjFromQuery frm = new frmCreatePrjFromQuery();
      bool? ok = frm.ShowDialog();
      if (ok == true)
      {
        //TODO
      }
    }

    private void _btnConnect_Click(object sender, RoutedEventArgs e)
    {
      FrmConnectMysql frm = new FrmConnectMysql();
      if (!App.Model.MySQL.IsConnected)
      {
        bool? res = frm.ShowDialog();
        if (res == true)
        {
          App.Model.MySQL.connect(frm.Server, frm.DataBase, frm.User, frm.PassWord);
          if (App.Model.MySQL.IsConnected)
          {
            AppParams.Inst.MySqlServer = frm.Server;
            if (frm.IsBat)
            {
              AppParams.Inst.MySqlDbBats = frm.DataBase;
              App.Model.MySQL.DbBats.setIsOpen(true);
            }
            if (frm.IsBird)
            {
              AppParams.Inst.MySqlDbBirds = frm.DataBase;
              App.Model.MySQL.DbBirds.setIsOpen(true);
            }
            AppParams.Inst.MySqlUser = frm.User;
            AppParams.Inst.save();
            _tbStatus.Content = App.Model.MySQL.getStatus();
            _btnConnect.Content = MyResources.CtrlMySql_DisconnectFromDB;
          }
          if (App.Model.MySQL.IsConnected)
            _lblStatus.Background = new SolidColorBrush(Colors.Green);
          else
            _lblStatus.Background = new SolidColorBrush(Colors.Red);
        }
      }
      else
      {
        App.Model.MySQL.disconnect();
        if (!App.Model.MySQL.IsConnected)
        {
          _tbStatus.Content = App.Model.MySQL.getStatus();
          _btnConnect.Content = MyResources.CtlMySql_ConnectToDB;
          _lblStatus.Background = new SolidColorBrush(Colors.Red);
        }
      }
    }

    private void _btnUpdateDb_Click(object sender, RoutedEventArgs e)
    {
      App.Model.MySQL.DbBats.addProjectToDb(App.Model.Prj, false);
    }
  }
}
