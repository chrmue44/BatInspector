using BatInspector.Properties;
using System.Windows;
using ZstdSharp.Unsafe;


namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmErrorLog.xaml
  /// </summary>
  public partial class frmErrorLog : Window
  {
    public TimeSpan TimeSpan
    {
      get
      {
        switch (_ctlSelTime.SelectIndex)
        {
          default:
          case 0:
            return new TimeSpan(0, 1, 0, 0);
          case 1:
            return new TimeSpan(0, 2, 0, 0);
          case 2:
            return new TimeSpan(0, 6, 0, 0);
          case 3:
            return new TimeSpan(1, 0, 0, 0);
          case 4:
            return new TimeSpan(2, 0, 0, 0);
        }
      }
    }

    public frmErrorLog()
    {
      InitializeComponent();
      _ctlSelTime.setup(MyResources.frmErrorLog_SelectTime, 0, 300, 120);
      string[] items = { MyResources.frmErrorLog_1Hour, MyResources.frmErrorLog_2Hours,
                         MyResources.frmErrorLog_6Hours, MyResources.frmErrorLog_1Day,
                         MyResources.frmErrorLog_2Days };
      _ctlSelTime.setItems(items);
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
      this .Close();
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
      this.Close();
    }
  }
}
