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
  class QueryItem
  {
    public int line { get; set; }
    public string Date {  get; set; }
    public string Location { get; set; }
    public string RecordingDevice { get; set; }
    public string MicrophoneId { get; set; }
    public string PrjCreator { get; set; }
    public string projects___Notes {get;set;}
    public string Classifier { get; set; }
    public string Model { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public string WavFileName { get; set; }
    public string Temperature { get; set; }
    public string Humidity { get; set; }
    public int CallNr { get; set; }
    public string SNR { get; set; }

    public string SpeciesMan { get; set; }
    public string SpeciesAuto { get; set; }
    public string Probability { get; set; }
    public string FreqMin { get; set; }
    public string FreqMax { get; set; }
    public string FreqMaxAmp { get; set; }
    public string DurationCall { get; set; }
    public string CallInterval { get; set; }

    public string calls___Remarks { get; set; }
    public void setValues(sqlRow row)
    {
      foreach (sqlField f in row.Fields)
      {
        if (f.Name == "Date")
          Date = f.getDateAsString();
        else if (f.Name == "Location")
          Location = f.getString();
        else if (f.Name == "PrjCreator")
          PrjCreator = f.getString();
        else if (f.Name == "Classifier")
          Classifier = f.getString();
        else if (f.Name == "RecordingDevice")
          RecordingDevice = f.getString();
        else if (f.Name == "MicrophoneId")
          MicrophoneId = f.getString();
        else if (f.Name == "Model")
          Model = f.getString();
        else if (f.Name == "Latitude")
          Latitude = f.getFloat().ToString("0.000000", CultureInfo.InvariantCulture);
        else if (f.Name == "Longitude")
          Longitude = f.getFloat().ToString("0.000000", CultureInfo.InvariantCulture);
        else if (f.Name == "WavFileName")
          WavFileName = f.getString();
        else if (f.Name == "SNR")
          SNR = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == "SpeciesMan")
          SpeciesMan = f.getString();
        else if (f.Name == "SpeciesAuto")
          SpeciesAuto = f.getString();
        else if (f.Name == "Probability")
          Probability = f.getFloat().ToString("0.00", CultureInfo.InvariantCulture);
        else if (f.Name == "FreqMin")
          FreqMin = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == "FreqMax")
          FreqMax = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == "FreqMaxAmp")
          FreqMaxAmp = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == "DurationCall")
          DurationCall = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == "CallInterval")
          CallInterval = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == "CallNr")
          CallNr = f.getInt32();
        else if (f.Name == "Temperature")
          Temperature = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == "Humidity")
          Humidity = f.getFloat().ToString("0.0", CultureInfo.InvariantCulture);
        else if (f.Name == "Notes")
          projects___Notes = f.getString();
        else if (f.Name == "Remarks")
          calls___Remarks = f.getString();
      }
    }

  }
  /// <summary>
  /// Interaction logic for CtlMySql.xaml
  /// </summary>
  public partial class CtlMySql : UserControl
  {
    bool _queryCollapsed = false;
    List<CheckBox> _listCb = new List<CheckBox>();
    string _columnsList;
    int _limitRows =1000;
    string _filterExpression = "";
    List<sqlRow> _query = null;
    public CtlMySql()
    {
      InitializeComponent();
      _cbLimit.Items.Clear();
      _cbLimit.Items.Add("10 " + MyResources.Rows);
      _cbLimit.Items.Add("100 " + MyResources.Rows);
      _cbLimit.Items.Add("1000 " + MyResources.Rows);
      _cbLimit.Items.Add("10000 " + MyResources.Rows);
      _cbLimit.Items.Add("no limit");

      _listCb.Clear();
      _sp1.Children.Clear();
      _sp2.Children.Clear();

      addCheckBox("Date", 1);
      addCheckBox("Location", 1);
      addCheckBox("PrjCreator", 1);
      addCheckBox("projects.Notes", 1);
      addCheckBox("RecordingDevice", 1);
      addCheckBox("MicrophoneId", 1);
      addCheckBox("Classifier", 1);
      addCheckBox("Model", 1);
      addCheckBox("Latitude", 2);
      addCheckBox("Longitude", 2);
      addCheckBox("WavFileName", 2);
      addCheckBox("Temperature", 2);
      addCheckBox("Humidity", 2);
      addCheckBox("CallNr", 2);
      addCheckBox("SNR", 2);
      addCheckBox("SpeciesMan", 2);
      addCheckBox("SpeciesAuto", 3);
      addCheckBox("Probability", 3);
      addCheckBox("FreqMin", 3);
      addCheckBox("FreqMax", 3);
      addCheckBox("FreqMaxAmp", 3);
      addCheckBox("DurationCall", 3);
      addCheckBox("CallInterval", 3);
      addCheckBox("calls.Remarks", 3);
      init();
    }

    public void init()
    {
      _filterExpression = "";
      _cbLimit.SelectedIndex = 2;
      _limitRows = 1000;
      _cbFilter.SelectedIndex = 0;
    }

    private void _dg_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
    
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
        _grid.RowDefinitions[1].Height = new GridLength(180);
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
      initDataSource(_query);
    }

    private void createQuery()
    {
      if (string.IsNullOrEmpty(_columnsList))
        return;
      SqlQueryBuilder b = new SqlQueryBuilder();
      b.init();
      b.addSelectStatement($"{_columnsList} FROM projects \nJOIN files ON projects.id = files.ProjectId \nJOIN calls ON files.id = FileId");
      if (!string.IsNullOrEmpty(_filterExpression))
        b.addWhereStatement(_filterExpression);
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
      return name.Replace("_", "__").Replace(".","___");
    }

    private string convertNameWPFtoSQL(string name)
    { 
     return name.Replace("___",".").Replace("__","_");
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
        if(filter != null)
          _filterExpression = DataBase.translateFilterExpressionToMySQL(filter.Expression);
      }
      if (resetFilter)
        _filterExpression = "";
      createQuery();
      collapse(false);
    }

    private void _cb_Click(object sender, RoutedEventArgs e)
    {
      _columnsList = "";
      foreach (CheckBox cb in _listCb)
      {
        if (cb.IsChecked == true)
        {
          if (_columnsList != "")
            _columnsList += ",";
          _columnsList += convertNameWPFtoSQL((string)cb.Content);
        }
      }
      createQuery();
    }

    private void addCheckBox(string name, int col)
    {
      CheckBox c = new CheckBox();
      c.Content = convertNameSQLtoWPF(name);
      c.Click += _cb_Click;
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
          _dg.Columns[i].Visibility = Visibility.Collapsed;
        for(int j = 0; j < dat[0].Fields.Count; j++)
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
      switch(_cbLimit.SelectedIndex)
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
          _limitRows = 0; break;
          
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
  }
}
