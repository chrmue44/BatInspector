/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

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

    public void showMessage(string title, string text, bool topmost = false)
    {
      this.Title = "BatInspector   " + title;
      if (topmost)
      {
        this.Topmost = true;
        this.Topmost = false;
      }
      _tbMsg.Text = text;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }
  }
}
