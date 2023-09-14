using System.Windows;
using System.Windows.Interop;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for FrmMessage.xaml
  /// </summary>
  public partial class FrmMessage : Window
  {
    public FrmMessage()
    {
      InitializeComponent();
    }

    public void showMessage(string title, string text)
    {
      this.Title = "BatInspector   " + title;
      this.Topmost = true;
      _tbMsg.Text = text;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }
  }
}
