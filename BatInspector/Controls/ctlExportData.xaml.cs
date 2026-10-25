using BatInspector.Properties;
using System;
using System.Collections.Generic;
using System.Windows.Controls;


namespace BatInspector.Controls
{


  public delegate void dlgDeleteItem(int index);

  /// <summary>
  /// Interaction logic for ctlExportData.xaml
  /// </summary>
  public partial class ctlExportData : System.Windows.Controls.UserControl
  {
    int _index = 0;
    dlgDeleteItem? _dlgDelete = null;
    public ctlExportData()
    {
      InitializeComponent();
    }

    public void setup(ExportDataItem item, int index, dlgDeleteItem dlgDelete)
    {
      _index = index;
      _dlgDelete = dlgDelete;
      _ctlLocal.setup("Artname Deutsch", enDataType.STRING, 0, 120, true);
      _ctlLocal.setValue(item.SpeciesLocal);
      _ctlLatin.setup("Artname Latein", enDataType.STRING, 0, 120, true);
      _ctlLatin.setValue(item.SpeciesLatin);
      _ctlLat.setup(MyResources.FilterVarHelpLat, enDataType.DOUBLE, 6, 120, true);
      _ctlLat.setValue(item.Latitude);
      _ctlLon.setup(MyResources.FilterVarHelpLon, enDataType.DOUBLE, 6, 120, true);
      _ctlLon.setValue(item.Longitude);
      _ctlTemp.setup(MyResources.FrmRecTemperature, enDataType.DOUBLE, 1, 120, true);
      _ctlTemp.setValue(item.Temperature);
      _ctlHumid.setup(MyResources.FrmRecHumidity, enDataType.DOUBLE, 1, 120, true);
      _ctlHumid.setValue(item.Humidity);
      _ctlDate.setup(MyResources.Date, enDataType.STRING, 0, 80, true);
      _ctlDate.setValue(item.Date);
      _ctlComment.setup(MyResources.Comment, enDataType.STRING, 0, 120, true);
      _ctlPathWav.setup("WAV", enDataType.STRING, 0, 120, false);
      _ctlPathWav.setValue(item.PathToWav);
      _ctlName.setup("Name", enDataType.STRING, 0, 120, true);
      _ctlName.setValue(item.Name);
      _ctlLocation.setup("Fundort", enDataType.STRING, 0, 120, true);
      _ctlLocation.setValue(item.Location);
      _ctlPathPng.setup("PNG", enDataType.STRING, 0, 120, false);
      _ctlPathPng.setValue(item.PathToPng);
    }


    public ExportDataItem getData()
    {
      ExportDataItem retVal = new ExportDataItem();
      retVal.Comment = _ctlComment.getValue();
      retVal.Date = _ctlDate.getValue();
      retVal.Temperature = _ctlTemp.getDoubleValue();
      retVal.Humidity = _ctlHumid.getDoubleValue();
      retVal.SpeciesLatin = _ctlLatin.getValue();
      retVal.SpeciesLocal = _ctlLocal.getValue();
      retVal.PathToPng = _ctlPathPng.getValue();
      retVal.PathToWav = _ctlPathWav.getValue();
      retVal.Latitude = _ctlLat.getDoubleValue();
      retVal.Longitude = _ctlLon.getDoubleValue();
      retVal.Name = _ctlName.getValue();
      retVal.Location = _ctlLocation.getValue();
      return retVal;
    }

    private void _btnDelete_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      if (_dlgDelete != null)
      {
        _dlgDelete(_index);
      }
    }
  }
}
