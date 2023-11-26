/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System;
using System.Windows.Controls;

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

    public void init(DateTime time, bool dateVisible = true, string label = "", int width = 80 )
    {
      _dp.Visibility = dateVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
      _lbl.Visibility = !dateVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
      _lbl.Text = label;
      _lbl.Width = width;
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
