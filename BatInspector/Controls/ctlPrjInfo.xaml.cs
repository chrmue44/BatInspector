/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-18                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Properties;
using System.IO;
using System.Windows.Controls;


namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for ctlPrjInfo.xaml
  /// </summary>
  public partial class ctlPrjInfo : UserControl
  {
    Project _prj = null;

    public ctlPrjInfo()
    {
      InitializeComponent();
    }

    private void _tbNotes_TextChanged(object sender, TextChangedEventArgs e)
    {
      if(_prj != null) 
      {
        _prj.Notes = _tbNotes.Text;
      }
    }

    public void setup(Project prj) 
    {
      _prj = prj;
   //   _tbCreated.Text = _prj.Created;
      _tbNotes.Text = _prj.Notes;
      _lblPrj.Content = MyResources.ctlProjectInfo + " [" + Path.GetFileName(_prj.Name) + "]";
    }
  }
}
