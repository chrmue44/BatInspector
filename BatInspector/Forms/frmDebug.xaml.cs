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
  /// Interaction logic for frmDebug.xaml
  /// </summary>
  public partial class frmDebug : Window
  {
    ViewModel _model;
    public frmDebug(ViewModel model)
    {
      InitializeComponent();
      _model = model;
      _ctlVarTable.setup(_model.Scripter.VarList);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }

    private void _btnClose_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
    }
  }
}
