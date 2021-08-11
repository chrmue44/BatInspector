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
using System.Windows.Shapes;


namespace BatInspector
{
  /// <summary>
  /// Interaktionslogik für Log.xaml
  /// </summary>
  public partial class FrmLog : Window
  {
    List<stLogEntry> _entries;

    public FrmLog()
    {
      InitializeComponent();
      _entries = new List<stLogEntry>();
    }

    public void log(stLogEntry entry)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
         Dispatcher.Invoke(new delegateLogEntry(log), entry);
         return;
      }
      _entries.Add(entry);
      TextBlock text = new TextBlock();
      text.Text = entry.Time.ToString() + "  " + entry.Type.ToString() + ": " + entry.Text;
      
      _spEntries.Children.Add(text);
    }
  }
}
