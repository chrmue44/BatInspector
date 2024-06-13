/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-18                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using BatInspector.Controls;
using libParser;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

public delegate void DlgCmd(string cmd);
public delegate void dlgAddTextLine(string text, Brush color);

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaktionslogik für CtlLog.xaml
  /// </summary>
  public partial class CtlLog : UserControl
  {

    DlgCmd _dlgCmd = null;
    bool _clearAfterReturn = false;

    public CtlLog()
    {
      InitializeComponent();
      _cbInfo.IsChecked = true;
      _cbErr.IsChecked = true;
      _cbWarn.IsChecked = true;
    }

    public bool CheckBoxesVisible
    {
      get 
      {
        return _cbErr.Visibility == Visibility.Visible;
      }
      set 
      {
        _cbDebug.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        _cbErr.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        _cbInfo.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        _cbWarn.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public void setup(DlgCmd dlg, bool clearAfterReturn)
    {
      _clearAfterReturn = clearAfterReturn;
      _dlgCmd = dlg;
    }

    public void log(stLogEntry entry)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new delegateLogEntry(log), entry);
        return;
      }
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

    public void addTextLine(string text, Brush color)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new dlgAddTextLine(addTextLine), text, color);
        return;
      }
      TextBlock tb = new TextBlock();
      tb.Text = text;
      tb.Foreground = color;
      tb.Visibility = Visibility.Visible;
      _spEntries.Children.Add(tb);
      _scrViewer.ScrollToBottom();
    }

    public bool checkMaxLogSize()
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        return (bool)Dispatcher.Invoke(new dlgCheckMaxLogSize(checkMaxLogSize));
      }
      if ((_spEntries != null) && (_spEntries.Children != null))
        return _spEntries.Children.Count > AppParams.MAX_LOG_COUNT;
      return false;
    }

    public void clearLog()
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new delegateLogClear(clearLog));
        return;
      }
      _spEntries.Children.Clear();
    }


      private void _btnClear_Click(object sender, RoutedEventArgs e)
    {
      _spEntries.Children.Clear();
    }

    private void _tbCmd_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Return)
      {
        string cmd = _tbCmd.Text;
        if (_dlgCmd != null)
          _dlgCmd(cmd);
        if (_clearAfterReturn)
          _tbCmd.Text = "";
      }
    }
  }
}
