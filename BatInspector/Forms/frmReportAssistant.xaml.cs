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
      int wl = 200;
      _rep = rep;
      _ctlFormData.setup(BatInspector.Properties.MyResources.frmReportAssistant_FormData, wl + 5, false, "json files(*.json)|*.json|All files(*.*) |*.*", setFormData);
      _ctlTemplate.setup(BatInspector.Properties.MyResources.frmReportAssistant_TemplateFile, wl + 5, false,"markdown files(*.md)|*.md|All files(*.*)|*.*");
      _ctlAuthor.setup(BatInspector.Properties.MyResources.frmReportAssistant_Author, Controls.enDataType.STRING, 0, wl, true);
      _ctlLocDescription.setup(BatInspector.Properties.MyResources.frmReportAssistant_DescrLoc, Controls.enDataType.STRING, 0, wl, true);
      _lblComment.Text = BatInspector.Properties.MyResources.CtlWavRemarks;
      _grComment.ColumnDefinitions[0].Width = new GridLength(wl + 5);
      _ctlLocationName.setup(BatInspector.Properties.MyResources.frmReportAssistant_LocationName, Controls.enDataType.STRING, 0, wl, true);
      _ctlPageTitle.setup(BatInspector.Properties.MyResources.frmReportAssistant_PageTitle, Controls.enDataType.STRING, 0, wl, true);
      _ctlWeather.setup(BatInspector.Properties.MyResources.frmReportAssistant_Weather, Controls.enDataType.STRING, 0, wl, true);
      _ctlTimeSpan.setup(BatInspector.Properties.MyResources.frmReportAssistant_TimeSpan, Controls.enDataType.STRING, 0, wl, true);
      _ctlSelectWavFolder.setup(BatInspector.Properties.MyResources.frmReportAssistant_SelectWAVFolder, wl + 10, true);
      _ctlImgPortrait.setup(BatInspector.Properties.MyResources.frmReportAssistantImgPortrait, wl +10, false, "image files |*.png;*.jpg|All files(*.*) |*.*");
      _ctlImgLandscape.setup(BatInspector.Properties.MyResources.frmReportAssistantImgLandscape, wl +10, false, "image files |*.png;*.jpg|All files(*.*) |*.*");
      foreach (string spec in _rep.Species)
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
        if(info != null)
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
        _ctlImgLandscape.setValue(_formData.ImgLandscape);
        _ctlImgPortrait.setValue(_formData.ImgPortrait);
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
      _formData.ImgLandscape = _ctlImgLandscape.getValue();
      _formData.ImgPortrait = _ctlImgPortrait.getValue();
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
