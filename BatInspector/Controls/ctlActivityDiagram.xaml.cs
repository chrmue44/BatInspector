/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2024-08-21                                      
 *   Copyright (C) 2024: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for ctlActivityDiagram.xaml
  /// </summary>
  public partial class ctlActivityDiagram : UserControl
  {
    const double X_BL = 50;
    const double X_BR = 20;
    const double Y_BT = 50;
    const double Y_BB = 70;
    SolidColorBrush COL_TEXT = new SolidColorBrush(Colors.Black);
    SolidColorBrush COL_GRID_MONTH = new SolidColorBrush(Colors.DarkGray);
    SolidColorBrush COL_GRID_WEEK = new SolidColorBrush(Colors.Gray);
    SolidColorBrush COL_GRID_DAY = new SolidColorBrush(Colors.LightGray);
    SolidColorBrush COL_SUNSET = new SolidColorBrush(Colors.DarkRed);
    SolidColorBrush COL_SUNRISE = new SolidColorBrush(Colors.DarkBlue);
    SolidColorBrush COL_GRID_FRAME = new SolidColorBrush(Colors.Gray);
    SolidColorBrush COL_DATA = new SolidColorBrush(Colors.Green);


        ViewModel _model = null;
    List<List<int>> _data;
    DateTime _start;
    List<string> _labelsY;
    List<string> _labelsX;
    double _lat;
    double _lon;

    public ctlActivityDiagram()
    {
      InitializeComponent();
    }

    public void setup(ViewModel model)
    {
      _model = model;
      int lblW = 150;
      _ctrlTitle.setup(BatInspector.Properties.MyResources.DiagramTitle, enDataType.STRING, 0, lblW, true);
    }


    public void createPlot(List<List<int>> hm, DateTime start, bool month, bool week, bool day, bool twilight, double lat, double lon)
    {
      _data = hm;
      _start = start;
      _lat = lat;
      _lon = lon;
      _cnv.Children.Clear();
      if(twilight)
        createTwilightLines();
      createTitle(_ctrlTitle.getValue());
      createLabels(month, week, day);
      createGrid(month, week, day);
      drawData();
    }


    private void drawData()
    {
      int days = _data.Count;
      DateTime currDay = _start;
      double dy = (_cnv.ActualHeight - Y_BB - Y_BT) / 24;
      double dx = 1.0 / days * (_cnv.ActualWidth - X_BL - X_BR);
      double maxDia = Math.Min(dy, dx);
      double maxValue = 0;
      
      for(int i = 0; i < days; i++)
      {
        for(int j = 0; j < _data[i].Count; j++)
        {
          if(maxValue < _data[i][j])
            maxValue = _data[i][j];
        }
      }

      for (int i = 0; i < days; i++)
      {
        for (int j = 0; j < _data[i].Count; j++)
        {
          double dotDia = (double)_data[i][j] / maxValue * maxDia;
          double yPos = Y_BT + (_cnv.ActualHeight - Y_BB - Y_BT) * j / _data[i].Count;
          double xPos = (double)i / days * (_cnv.ActualWidth - X_BL - X_BR) + X_BL - dx / 2;
          GraphHelper.createDot(_cnv, xPos, swapY(yPos), maxDia, COL_DATA);
        }
      }
    }


    private void createTwilightLines()
    {
      int days = _data.Count;
      DateTime currDay = _start;
      for (int i = 0; i < days; i++)
      {
        Sunrise.getSunSetSunRise(_lat, _lon, currDay, out int srH1, out int srM1, out int ssH1, out int ssM1);
        double sr1 = srH1 + (double)srM1 / 60;
        double ss1 = ssH1 + (double)ssM1 / 60;
        double x1 = (double)i / days * (_cnv.ActualWidth - X_BL - X_BR) + X_BL;
        double yr1 = sr1 / 24 * (_cnv.ActualHeight - Y_BB - Y_BT) + Y_BT;
        double ys1 = ss1 / 24 * (_cnv.ActualHeight - Y_BB - Y_BT) + Y_BT;
        currDay = currDay.AddDays(1);
        Sunrise.getSunSetSunRise(_lat, _lon, currDay, out int srH2, out int srM2, out int ssH2, out int ssM2);
        double sr2 = srH2 + (double)srM2 / 60;
        double ss2 = ssH2 + (double)ssM2 / 60;
        double x2 = (double)(i + 1) / days * (_cnv.ActualWidth - X_BL - X_BR) + X_BL;
        double yr2 = sr2 / 24 * (_cnv.ActualHeight - Y_BB - Y_BT) + Y_BT;
        double ys2 = ss2 / 24 * (_cnv.ActualHeight - Y_BB - Y_BT) + Y_BT;
        GraphHelper.createLine(_cnv, x1, swapY(ys1), x2, swapY(ys2), COL_SUNSET, 3);
        GraphHelper.createLine(_cnv, x1, swapY(yr1), x2, swapY(yr2), COL_SUNRISE, 3);
      }
    }


    private void createTitle(string title)
    {
      TextBlock textBlock = new TextBlock();
      textBlock.Text = title;
      textBlock.Foreground = COL_TEXT;
      textBlock.TextAlignment = TextAlignment.Right;
      textBlock.FontSize = 16;
      textBlock.FontWeight = FontWeights.Bold;
      Canvas.SetLeft(textBlock, 100);
      Canvas.SetTop(textBlock, 10);
      _cnv.Children.Add(textBlock);
    }


    double swapY(double y)
    {
      double retVal = y;
      if ((y >= Y_BT) && (y <= (_cnv.ActualHeight - Y_BB)))
      {
        retVal -= Y_BT;
        retVal = (_cnv.ActualHeight - Y_BB - Y_BT) - retVal;
        retVal += Y_BT;
      }
      return retVal;
    }


    private void createLabels(bool months, bool weeks, bool days) 
    {
      int dayCnt = _data.Count;
      
      // time of day
      for(int i = 0; i < 24; i++)
      {
        if (i % 6 == 0)
        {
          double yPos = Y_BT + 15 + (_cnv.ActualHeight - Y_BB - Y_BT) * i / 24;
          string tStr = i.ToString() + ":00";
          GraphHelper.createText(_cnv, 5, swapY(yPos), tStr, COL_TEXT);
        }
      }

      // days
      if (weeks && days || days)
      {
        DateTime currDay = _start;
        for (int i = 0; i < dayCnt; i++)
        {
          if (currDay.DayOfWeek == DayOfWeek.Monday)
          {
            double xPos = X_BL + (double)i / dayCnt * (_cnv.ActualWidth - X_BL - X_BR);
            GraphHelper.createText(_cnv, xPos, _cnv.ActualHeight - 5, currDay.ToString("dd.MM.yyyy"), COL_TEXT, 270);
          }
          currDay = currDay.AddDays(1);
        }
      }

      //month
      else if(months && days || months)
      {
        DateTime currDay = _start;
        for (int i = 0; i < dayCnt; i++)
        {
          if (currDay.Day == 1)
          {
            double xPos = X_BL + (double)i / dayCnt * (_cnv.ActualWidth - X_BL - X_BR);
            GraphHelper.createText(_cnv, xPos, _cnv.ActualHeight - 5, currDay.ToString("dd.MM.yyyy"), COL_TEXT, 270);
          }
          currDay = currDay.AddDays(1);
        }
      }
    }


    private void createGrid(bool month, bool week, bool day)
    {
      // outer frame
      GraphHelper.createLine(_cnv, X_BL, Y_BT, _cnv.ActualWidth - X_BR, Y_BT, COL_GRID_FRAME);
      GraphHelper.createLine(_cnv, X_BL, _cnv.ActualHeight - Y_BB, _cnv.ActualWidth - X_BR, _cnv.ActualHeight - Y_BB, COL_GRID_FRAME);
      GraphHelper.createLine(_cnv, X_BL, Y_BT, X_BL, _cnv.ActualHeight - Y_BB, COL_GRID_FRAME);
      GraphHelper.createLine(_cnv, _cnv.ActualWidth - X_BR, Y_BT, _cnv.ActualWidth - X_BR, _cnv.ActualHeight - Y_BB, COL_GRID_FRAME);


      // time of day lines
      for (int i = 0; i < 24; i++)
      {
        double yPos = Y_BT + (_cnv.ActualHeight - Y_BB - Y_BT) * i / 24;
        GraphHelper.createLine(_cnv, X_BL, yPos, _cnv.ActualWidth - X_BR, yPos, (i%6 == 0) ? COL_GRID_WEEK : COL_GRID_DAY);

      }

      // day lines
      int days = _data.Count;
      if (day)
      {
        for (int i = 0; i < days; i++)
        {
          double posX = X_BL + (double)i / days * (_cnv.ActualWidth - X_BL - X_BR);
          GraphHelper.createLine(_cnv, posX, _cnv.ActualHeight - Y_BB, posX, Y_BT, COL_GRID_DAY);
        }
      }
      if (week)
      {
        DateTime currDay = _start;
        for (int i = 0; i<days; i++) 
        {
          if(currDay.DayOfWeek == DayOfWeek.Monday)
          {
            double posX = X_BL + (double)i / days * (_cnv.ActualWidth - X_BL - X_BR);
            GraphHelper.createLine(_cnv, posX, _cnv.ActualHeight - Y_BB, posX, Y_BT, COL_GRID_WEEK);
          }
          currDay = currDay.AddDays(1);
        }
      }
      if (month)
      {
        DateTime currDay = _start;
        for (int i = 0; i < days; i++)
        {
          if (currDay.Day == 1)
          {
            double posX = X_BL + (double)i / days * (_cnv.ActualWidth - X_BL - X_BR);
            GraphHelper.createLine(_cnv, posX, _cnv.ActualHeight - Y_BB, posX, Y_BT, COL_GRID_MONTH, 2);
          }
          currDay = currDay.AddDays(1);
        }
      }
    }
  }
}
