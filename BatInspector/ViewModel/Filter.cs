using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatInspector
{
  public delegate void dlgDelete(int index);
  public class FilterItem
  {
    public int Index { get; set; }
    public string Name { get; set; }
    public string Expression { get; set; }
  }
  
  public class Filter
  {
    List<FilterItem> _list;

    public List<FilterItem> Items { get { return _list; } }

    public Filter()
    {
      _list = new List<FilterItem>();
    }
  }
}
