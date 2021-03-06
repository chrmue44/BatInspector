using libParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaktionslogik für CtlLog.xaml
  /// </summary>
  public partial class CtlLog : UserControl
  {

    public CtlLog()
    {
      InitializeComponent();
      _cbInfo.IsChecked = true;
      _cbErr.IsChecked = true;
      _cbWarn.IsChecked = true;
    }

    public void log(stLogEntry entry, List<stLogEntry> list)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.Invoke(new delegateLogEntry(log), entry, list);
        return;
      }
      list.Add(entry);
      if (
          ((entry.Type == enLogType.ERROR) && _cbErr.IsChecked.Value) ||
          ((entry.Type == enLogType.WARNING) && _cbWarn.IsChecked.Value) ||
          ((entry.Type == enLogType.INFO) && _cbInfo.IsChecked.Value) ||
          ((entry.Type == enLogType.DEBUG) && _cbDebug.IsChecked.Value)
        )
      {
        TextBlock text = new TextBlock();
        text.Text = entry.Time.ToString() + "  " + entry.Type.ToString() + ": " + entry.Text;
        _spEntries.Children.Add(text);
        _scrViewer.ScrollToBottom();
      }
    }

    private void _btnClear_Click(object sender, RoutedEventArgs e)
    {
      _spEntries.Children.Clear();
    }
  }
}
