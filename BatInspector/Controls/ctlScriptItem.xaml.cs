
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;


namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für ctlFilterItem.xaml
  /// </summary>
  public partial class ctlScriptItem : UserControl
  {
    int _index;
    dlgDelete _dlgDelete;
    ViewModel _model;
    
    public int Index {  get { return _index; } }
    public string ScriptName {  get { return _tbScriptName.Text; } } 
    public string Description { get { return _tbDescription.Text; } }

    public ctlScriptItem()
    {
      InitializeComponent();
    }

    public void setup(ScriptItem script, dlgDelete del, ViewModel model)
    {
      _model = model;
      _index = script.Index;
      _dlgDelete = del;
      _tbScriptName.Text = script.Name;
      _tbDescription.Text = script.Description;
      _lblIdx.Text = _index.ToString();
    }


    private void _btnSel_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = "Script files (*.scr)|*.scr|All files (*.*)|*.*";
      if (openFileDialog.ShowDialog() == true)
        _tbScriptName.Text = openFileDialog.FileName;
    }


    private void _btnDel_Click(object sender, RoutedEventArgs e)
    {
      _dlgDelete(_index);
    }

    private void _btnRun_Click(object sender, RoutedEventArgs e)
    {
      _model.executeScript(_tbScriptName.Text);
    }

    private void _btnEdit_Click(object sender, RoutedEventArgs e)
    {
      _model.editScript(_tbScriptName.Text);
    }
  }
}
