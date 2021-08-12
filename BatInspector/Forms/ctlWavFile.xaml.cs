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

namespace BatInspector
{
  /// <summary>
  /// Interaktionslogik für ctlWavFile.xaml
  /// </summary>
  public partial class ctlWavFile : UserControl
  {
    public ctlWavFile()
    {
      InitializeComponent();
    }

    public void setFileInformations(string Name, int Calls)
    {
      _grp.Header = Name;
      List<ListItem> list = new List<ListItem>();
      //      ListItem item = new ListItem("Filename", Name);
      //      list.Add(item);
      ListItem  item = new ListItem("Nr. of Calls", Calls);
      list.Add(item);
      _lvFileInfo.ItemsSource = list;
    }
  }

  public class ListItem
  {
    public string Name { get; set; }
    public object Value { get; set; }

    public ListItem(string name, object value)
    {
      Name = name;
      Value = value;
    }

    public override string ToString()
    {
      return Name + ": " + Value.ToString();
    }
  }
}
