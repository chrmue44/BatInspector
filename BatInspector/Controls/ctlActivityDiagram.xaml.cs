/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2024-08-21                                      
 *   Copyright (C) 2024: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using Pen = System.Drawing.Pen;
using Graphics = System.Drawing.Graphics;
using System.IO;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using libParser;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for ctlActivityDiagram.xaml
  /// </summary>
  public partial class ctlActivityDiagram : UserControl
  {
    const float X_BL = 50.0f;
    const float X_BR = 20.0f;
    const float Y_BT = 80.0f;
    const float Y_BB = 80.0f;

    System.Drawing.Brush COL_TEXT = System.Drawing.Brushes.Black;
    System.Drawing.Color COL_BACK = System.Drawing.Color.WhiteSmoke;
    System.Drawing.Pen COL_GRID_FRAME = new System.Drawing.Pen(System.Drawing.Brushes.Gray, 2);
    System.Drawing.Pen COL_GRID_MONTH = new System.Drawing.Pen(System.Drawing.Brushes.DarkGray, 2);
    System.Drawing.Pen COL_GRID_WEEK = new System.Drawing.Pen(System.Drawing.Brushes.Gray, 1);
    System.Drawing.Pen COL_GRID_DAY = new System.Drawing.Pen(System.Drawing.Brushes.LightGray, 1);
    System.Drawing.Pen COL_SUNSET = new System.Drawing.Pen(System.Drawing.Brushes.DarkRed, 3);
    System.Drawing.Pen COL_SUNRISE = new System.Drawing.Pen(System.Drawing.Brushes.DarkBlue, 3);
    System.Drawing.Brush COL_DATA = System.Drawing.Brushes.Green;

    ViewModel _model = null;
    ActivityData _data;
    List<string> _labelsY;
    List<string> _labelsX;
    double _lat;
    double _lon;
    Bitmap _bmp = null;
    float _width = 0;
    float _height = 0;
    Graphics _graphics;

    public ctlActivityDiagram()
    {
      InitializeComponent();
    }

    public void setup(ViewModel model)
    {
      _model = model;
      int lblW = 150;
      _ctrlTitle.setup(BatInspector.Properties.MyResources.DiagramTitle, enDataType.STRING, 0, lblW, true);
      _ctlStyle.setup(BatInspector.Properties.MyResources.ctlActivityDisplayStyle, 0, 80, 100);
      string[] items = new string[3];
      items[0] = BatInspector.Properties.MyResources.ctlActivityColored;
      items[1] = BatInspector.Properties.MyResources.Circle;
      items[2] = BatInspector.Properties.MyResources.Square;
      _ctlStyle.setItems(items);
    }


    public void createPlot(ActivityData hm, bool month, bool week, bool day, bool twilight, double lat, double lon)
    {
      try
      {
        _data = hm;
        _lat = lat;
        _lon = lon;
        _width = (float)_grpImg.ActualWidth - 20;
        _height = (float)_grpImg.ActualHeight - 20;
        _bmp = new Bitmap((int)_width, (int)_height);
        _graphics = Graphics.FromImage(_bmp);
        _graphics.Clear(COL_BACK);
        createGrid(month, week, day);
        if (twilight)
          createTwilightLines();
        createTitle(_ctrlTitle.getValue());
        createLabels(month, week, day);
        drawData();

        BitmapImage bitmapimage = new BitmapImage();
        using (MemoryStream memory = new MemoryStream())
        {
          _bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
          memory.Position = 0;
          bitmapimage.BeginInit();
          bitmapimage.StreamSource = memory;
          bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
          bitmapimage.EndInit();

        }
        _cnv.Source = bitmapimage;
      }
      catch (Exception ex) 
      {
        DebugLog.log("unable to create activity diagram: " + ex.ToString(), enLogType.ERROR);
      }
    }

    public void saveBitMap(string fileName)
    {
      if (_bmp != null)
      {
        System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Bmp;
        _bmp.Save(fileName, format);
        DebugLog.log($"activity diagram '{fileName}' successfully exported", enLogType.INFO);
      }
    }

    private void drawData()
    {
      int days = (int)(_data.EndDate.Date- _data.StartDate.Date).TotalDays;
      float dy = (_height - Y_BB - Y_BT) / 24;
      float dx = 1.0f / days * (_width - X_BL - X_BR);
      float maxDia = Math.Max(Math.Min(dy, dx) - 2, 1);
      float maxValue = 0;

      for (int i = 0; i < _data.Days.Count; i++)
      {
        for (int j = 0; j < _data.Days[i].Counter.Count; j++)
        {
          if (maxValue < _data.Days[i].Counter[j])
            maxValue = _data.Days[i].Counter[j];
        }
      }


      for(int i = 0; i < _data.Days.Count; i++)
      {
        int dayIdx = (int)(_data.Days[i].Date.Date - _data.StartDate.Date).TotalDays;
        if ((dayIdx >= 0) && (dayIdx < _data.Days.Count))
        {
          for (int j = 0; j < _data.Days[dayIdx].Counter.Count; j++)
          {
            float dotDia = (float)_data.Days[dayIdx].Counter[j] / maxValue * maxDia;
            float yPos = Y_BT + (_height - Y_BB - Y_BT) * j / _data.Days[dayIdx].Counter.Count;
            float xPos = (float)dayIdx / days * (_width - X_BL - X_BR) + X_BL + dx / 2;
            GraphHelper.createDot(_graphics, xPos, swapY(yPos), dotDia, COL_DATA);
          }
        }
        else
          DebugLog.log("drawData index error", enLogType.ERROR);
      }
    }


    private void createTwilightLines()
    {
      int days = _data.Days.Count;
      DateTime currDay = _data.StartDate;
      for (int i = 0; i < days; i++)
      {
        Sunrise.getSunSetSunRise(_lat, _lon, currDay, out int srH1, out int srM1, out int ssH1, out int ssM1);
        float sr1 = (float)srH1 + (float)srM1 / 60;
        float ss1 = (float)ssH1 + (float)ssM1 / 60;
        float x1 = (float)i / days * (_width - X_BL - X_BR) + X_BL;
        float yr1 = (float)sr1 / 24 * (_height - Y_BB - Y_BT) + Y_BT;
        float ys1 = (float)ss1 / 24 * (_height - Y_BB - Y_BT) + Y_BT;
        currDay = currDay.AddDays(1);
        Sunrise.getSunSetSunRise(_lat, _lon, currDay, out int srH2, out int srM2, out int ssH2, out int ssM2);
        float sr2 = (float)srH2 + (float)srM2 / 60;
        float ss2 = (float)ssH2 + (float)ssM2 / 60;
        float x2 = (float)(i + 1) / days * (_width - X_BL - X_BR) + X_BL;
        float yr2 = (float)sr2 / 24 * (_height - Y_BB - Y_BT) + Y_BT;
        float ys2 = (float)ss2 / 24 * (_height - Y_BB - Y_BT) + Y_BT;
        _graphics.DrawLine(COL_SUNSET, x1, swapY(ys1), x2, swapY(ys2));
        _graphics.DrawLine(COL_SUNRISE, x1, swapY(yr1), x2, swapY(yr2));
      }
    }


    private void createTitle(string title)
    {
      System.Drawing.Font font = new System.Drawing.Font("Tahoma", 14);
      SizeF f = _graphics.MeasureString(title, font);
      _graphics.DrawString(title, font, COL_TEXT, (_width - f.Width) / 2, 10.0f);

    }


    float swapY(float y)
    {
      float retVal = y;
      if ((y >= Y_BT) && (y <= (_height - Y_BB)))
      {
        retVal -= Y_BT;
        retVal = (_height - Y_BB - Y_BT) - retVal;
        retVal += Y_BT;
      }
      return retVal;
    }


    private void createLabels(bool months, bool weeks, bool days)
    {
      int dayCnt = (int)(_data.EndDate - _data.StartDate).TotalDays;

      // time of day
      for (int i = 0; i < 24; i++)
      {
        if (i % 6 == 0)
        {
          float yPos = Y_BT + 15 + (_height - Y_BB - Y_BT) * i / 24;
          string tStr = i.ToString() + ":00";
          GraphHelper.createText(_graphics, 5.0f, swapY(yPos), tStr, COL_TEXT);
        }
      }

      // days
      if (weeks && days || days & !months)
      {
        DateTime currDay = _data.StartDate;
        for (int i = 0; i < dayCnt; i++)
        {
          if (currDay.DayOfWeek == DayOfWeek.Monday)
          {
            float xPos = X_BL + (float)i / dayCnt * (_width - X_BL - X_BR);
            GraphHelper.createText(_graphics, xPos, _height - 5, currDay.ToString("dd.MM.yyyy"), COL_TEXT, -90);
          }
          currDay = currDay.AddDays(1);
        }
      }

      //month
      else if (months && days || months)
      {
        DateTime currDay = _data.StartDate;
        for (int i = 0; i < dayCnt; i++)
        {
          if (currDay.Day == 1)
          {
            float xPos = X_BL + (float)i / dayCnt * (_width - X_BL - X_BR);
            GraphHelper.createText(_graphics, xPos, _height - 5, currDay.ToString("dd.MM.yyyy"), COL_TEXT, -90);
          }
          currDay = currDay.AddDays(1);
        }
      }
    }


    private void createGrid(bool month, bool week, bool day)
    {
      // time of day lines
      for (int i = 0; i < 24; i++)
      {
        float yPos = Y_BT + (_height - Y_BB - Y_BT) * i / 24;
        Pen pen = (i % 6 == 0) ? COL_GRID_WEEK : COL_GRID_DAY;
        _graphics.DrawLine(pen, X_BL, yPos, _width - X_BR, yPos);

      }

      // day lines
      int days = (int)(_data.EndDate - _data.StartDate).TotalDays;
      if (day)
      {
        for (int i = 0; i < days; i++)
        {
          float posX = X_BL + (float)i / days * (_width - X_BL - X_BR);
          _graphics.DrawLine(COL_GRID_DAY, posX, _height - Y_BB, posX, Y_BT);
        }
      }
      if (week)
      {
        DateTime currDay = _data.StartDate;
        for (int i = 0; i < days; i++)
        {
          if (currDay.DayOfWeek == DayOfWeek.Monday)
          {
            float posX = X_BL + (float)i / days * (_width - X_BL - X_BR);
            _graphics.DrawLine(COL_GRID_WEEK, posX, _height - Y_BB, posX, Y_BT);
          }
          currDay = currDay.AddDays(1);
        }
      }
      if (month)
      {
        DateTime currDay = _data.StartDate;
        for (int i = 0; i < days; i++)
        {
          if (currDay.Day == 1)
          {
            float posX = X_BL + (float)i / days * (_width - X_BL - X_BR);
            _graphics.DrawLine(COL_GRID_MONTH, posX, _height - Y_BB, posX, Y_BT);
          }
          currDay = currDay.AddDays(1);
        }
      }
      // outer frame
      _graphics.DrawLine(COL_GRID_FRAME, X_BL, Y_BT, _width - X_BR, Y_BT);
      _graphics.DrawLine(COL_GRID_FRAME, X_BL, _height - Y_BB, _width - X_BR, _height - Y_BB);
      _graphics.DrawLine(COL_GRID_FRAME, X_BL, Y_BT, X_BL, _height - Y_BB);
      _graphics.DrawLine(COL_GRID_FRAME, _width - X_BR, Y_BT, _width - X_BR, _height - Y_BB);
    }
  }
}
