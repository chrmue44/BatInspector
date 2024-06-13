using System.Windows;
using System.Windows.Media.TextFormatting;
using BatInspector.Controls;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmReportAssistant.xaml
  /// </summary>
  public partial class frmReportAssistant : Window
  {
    SumReportJson _rep;
    WebReportDataJson _formData = null;
    DlgCmd _dlgSetFormDataName = null;
    public frmReportAssistant(SumReportJson rep, DlgCmd dlgSetFormDataName)
    {
      InitializeComponent();
      _dlgSetFormDataName = dlgSetFormDataName;
      int wl = 120;
      _rep = rep;
      _ctlFormData.setup("Form Data", wl + 5, false, "json files(*.json)|*.json|All files(*.*) |*.*", setFormData);
      _ctlTemplate.setup("Template file", wl + 5, false,"markdown files(*.md)|*.md|All files(*.*)|*.*");
      _ctlAuthor.setup("Author", Controls.enDataType.STRING, 0, wl, true);
      _ctlLocDescription.setup("Description Location", Controls.enDataType.STRING, 0, wl, true);
      _lblComment.Text = "Remarks";
      _grComment.ColumnDefinitions[0].Width = new GridLength(wl + 5);
      _ctlLocationName.setup("Location Name", Controls.enDataType.STRING, 0, wl, true);
      _ctlPageTitle.setup("Page Title", Controls.enDataType.STRING, 0, wl, true);
      _ctlWeather.setup("Weather", Controls.enDataType.STRING, 0, wl, true);
      _ctlTimeSpan.setup("Time span", Controls.enDataType.STRING, 0, wl, true);
      _ctlSelectWavFolder.setup("select WAV folder", wl + 10, true);
      foreach(string spec in _rep.Species)
      {
        if((spec != "?") && (spec.ToLower() != "social"))
        {
          ctlWebRepSpecies ctl = new ctlWebRepSpecies(_spFoundSpecies.Children.Count, delSpecies);
          ctl.Species = spec;
          _spFoundSpecies.Children.Add(ctl);
        }
      }
    }

    private void delSpecies(int idx)
    {
      if ((idx >= 0) && (idx < _spFoundSpecies.Children.Count))
      {
        ctlWebRepSpecies ctl = _spFoundSpecies.Children[idx] as ctlWebRepSpecies;
        SpeciesWebInfo info = _formData.findSpecies(ctl.Species);
        info.Show = false;
        _spFoundSpecies.Children.RemoveAt(idx);
      }
    }

    private void setFormData()
    {
      _formData = WebReportDataJson.load(_ctlFormData.getValue());
      if (_formData != null)
      {
        _ctlPageTitle.setValue(_formData.PageName);
        _ctlLocationName.setValue(_formData.LocationName);
        _ctlTemplate.setValue(_formData.Template);
        _ctlAuthor.setValue(_formData.Author);
        _ctlLocDescription.setValue(_formData.LocationDescription);
        _ctlWeather.setValue(_formData.Weather);
        _ctlTimeSpan.setValue(_formData.TimeSpan);
        _tbComment.Text = _formData.Comment;
        _ctlSelectWavFolder.setValue(_formData.WavFolder);
        for (int i = 0; i < _spFoundSpecies.Children.Count; i++)
        {
          ctlWebRepSpecies ctl = _spFoundSpecies.Children[i] as ctlWebRepSpecies;
          SpeciesWebInfo info = _formData.findSpecies(ctl.Species);
          if (info != null)
          {
            ctl.Confusion = info.Confusion;
            ctl.Comment = info.Comment;
            info.Show = true;
          }
        }
      }
    }

    private void save()
    {
      if(_formData == null)
        _formData = new WebReportDataJson();
      _formData.PageName =_ctlPageTitle.getValue();
      _formData.LocationName =_ctlLocationName.getValue();
      _formData.Template = _ctlTemplate.getValue();
      _formData.Author = _ctlAuthor.getValue();
      _formData.LocationDescription = _ctlLocDescription.getValue();
      _formData.Weather = _ctlWeather.getValue();
      _formData.TimeSpan = _ctlTimeSpan.getValue();
      _formData.Comment = _tbComment.Text;
      _formData.WavFolder = _ctlSelectWavFolder.getValue();
      foreach (ctlWebRepSpecies ctl in _spFoundSpecies.Children)
      {
        SpeciesWebInfo info = _formData.findSpecies(ctl.Species);
        if (info != null)
        {
          info.Confusion = ctl.Confusion;
          info.Comment = ctl.Comment;
          info.Name = ctl.Species;
        }
      }

      if (_formData != null ) 
        _formData.save(_ctlFormData.getValue());
    }
    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
      this.Close();
    }

    private void _btnOK_Click(object sender, RoutedEventArgs e)
    {
      save();
      if(_dlgSetFormDataName != null)
        _dlgSetFormDataName(_ctlFormData.getValue());
      this.DialogResult = true;
      this.Close();
    }
  }
}
