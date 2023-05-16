using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for CtlDateTime.xaml
  /// </summary>
  public partial class CtlDateTime : UserControl
  {
    public CtlDateTime()
    {
      InitializeComponent();
    }

    public DateTime DateTime { get { return getDateTime(); } }
    public int Hour { get { return getHour(); } set { _hour.Text = limit(value, 0, 23).ToString(); } }
    public int Minute { get { return getMinute(); } set { _min.Text = limit(value, 0, 59).ToString(); } }

    DateTime getDateTime() 
    {
      DateTime retVal = (DateTime)_dp.SelectedDate;
      if(retVal == null)
        retVal = _dp.DisplayDate;
      retVal = retVal.AddHours(getHour());
      retVal = retVal.AddMinutes(getMinute());
      return retVal;
    }

    public void init(DateTime time)
    {
      _dp.DisplayDate = time.Date;
      _dp.Text = _dp.DisplayDate.ToString();
      _hour.Text = time.Hour.ToString(); 
      _min.Text = time.Minute.ToString();
    }

    int limit(int v, int min, int max)
    {
      int retVal = v;
      if (retVal < min)
        retVal = min;
      if (retVal > max)
        retVal = max;
      return retVal;
    }

    int getHour()
    {
      int.TryParse(_hour.Text, out int hour);
      return limit(hour,0,23);
    }

    int getMinute()
    {
      int.TryParse(_min.Text, out int minute);
      return limit(minute, 0, 59);
    }
  }
}
