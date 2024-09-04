using System.Windows;
using System.Windows.Forms;

namespace BatInspector.Forms
{
  public delegate void dlgCreatePlot(ActivityData data);


  /// <summary>
  /// Interaction logic for frmActivity.xaml
  /// </summary>
  public partial class frmActivity : Window
  {
    private string _bmpName;
    public frmActivity(ViewModel model, string bmpName)
    {
      InitializeComponent();
      _ctlActivity.setup(model);
     _bmpName = bmpName;
     Title = $"{Properties.MyResources.ActivityDiagram}: {bmpName}";
    }



    public void setup()
    {
    }

    /// <summary>
    /// callback function after gathering of data is finished
    /// </summary>
    public void createPlot(ActivityData data)
    {
      if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        Dispatcher.BeginInvoke(new dlgCreatePlot(createPlot), data);
      }
      else
      {
        _ctlActivity.createPlot(data, _bmpName);
      }
    }

    private void _btnOK_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
