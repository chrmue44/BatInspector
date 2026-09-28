using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for ctlComponent.xaml
  /// </summary>
  public partial class ctlComponent : System.Windows.Controls.UserControl
  {
    public ctlComponent()
    {
      InitializeComponent();
    }

    public void setup(string text)
    {
      _lbl.Content = text;
      Ok = true;
    }


    public bool Ok
    {
      set 
      {
        if(value)
        {
          _imgOk.Visibility = Visibility.Visible;
          _imgNok.Visibility = Visibility.Collapsed;
        }
        else
        {
          _imgOk.Visibility = Visibility.Collapsed;
          _imgNok.Visibility= Visibility.Visible;
        }
      }
    }
  }
}
