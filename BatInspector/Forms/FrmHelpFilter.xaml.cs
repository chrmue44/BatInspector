/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using System.Windows;
using System.Windows.Interop;

namespace BatInspector
{
  /// <summary>
  /// Interaktionslogik für Window1.xaml
  /// </summary>
  public partial class FrmHelpFilter : Window
  {
    public FrmHelpFilter()
    {
      InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }
  }
}
