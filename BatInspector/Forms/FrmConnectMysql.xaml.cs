using System.Windows;
using BatInspector.Controls;
using BatInspector.Properties;


namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for FrmConnectMysql.xaml
  /// </summary>
  public partial class FrmConnectMysql : Window
  {
    public string Server = "";
    public string DataBase = "";
    public string User = "";
    public string PassWord = "";
    public bool IsBat = true;
    public bool IsBird = false;
    public FrmConnectMysql()
    {
      InitializeComponent();
      int offs = 120;
      _ctlSelDb.setup("Vorauswahl", 0, offs, 100, preselectSettings);
      string[] items = { MyResources.FrmConnectMysql_Bats, MyResources.FrmConnectMysql_Birds };
      _ctlSelDb.setItems(items);
      _ctlSelDb.SelectIndex = 0;
      preselectSettings(0, "");
      _ctlServer.setup("Server", enDataType.STRING, 0, offs, true);
      _ctlDataBase.setup("Database", enDataType.STRING, 0, offs, true);
      _ctlUser.setup("User Name", enDataType.STRING, 0, offs, true);
    }

    /*
    private void valueChanged(enDataType type, object val)
    {
      _btnOk.IsEnabled = (_ctlDataBase.getValue() != "") && (_ctlServer.getValue() != "") &&
                         (_ctlUser.getValue() != "") && (_ctlPassWord.getValue() != "");
    } */

    private void preselectSettings(int idx, string val)
    {
      if (_ctlSelDb.SelectIndex == 0)
      {
        _ctlDataBase.setValue(AppParams.Inst.MySqlDbBats);
        IsBat = true;
        IsBird = false;
      }
      else
      {
        _ctlDataBase.setValue(AppParams.Inst.MySqlDbBirds);
        IsBat = false;
        IsBird = true;
      }
      _ctlServer.setValue(AppParams.Inst.MySqlServer);
      _ctlUser.setValue(AppParams.Inst.MySqlUser);
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
      Close();
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      Server = _ctlServer.getValue();
      DataBase = _ctlDataBase.getValue();
      User = _ctlUser.getValue();
      PassWord = _tbPassword.Password;
      this.DialogResult = true;
      Close();
    }
  }
}
