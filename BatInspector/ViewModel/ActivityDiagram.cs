﻿/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2024-08-20                                       
 *   Copyright (C) 2024: Christian Müller chrmue44(at)gmail(dot)com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Controls;
using libParser;
using System;
using System.Drawing;


namespace BatInspector
{
  public class ActivityDiagram
  {
    const int BMP_WITH = 1800;
    const int BMP_HEIGHT = 900;
    const float X_BL = 50.0f;
    const float X_BR = 250.0f;
    const float Y_BT = 80.0f;
    const float Y_BB = 80.0f;

    DateTime DIAG_START_TIME = new DateTime(2024, 1, 1, 17, 0, 0);
    DateTime DIAG_END_TIME = new DateTime(2024, 1, 1, 9, 0, 0);

    System.Drawing.Brush COL_TEXT = System.Drawing.Brushes.Black;
    System.Drawing.Color COL_BACK = System.Drawing.Color.WhiteSmoke;
    System.Drawing.Pen COL_GRID_FRAME = new System.Drawing.Pen(System.Drawing.Brushes.Gray, 2);
    System.Drawing.Pen COL_GRID_MONTH = new System.Drawing.Pen(System.Drawing.Brushes.DarkGray, 2);
    System.Drawing.Pen COL_GRID_WEEK = new System.Drawing.Pen(System.Drawing.Brushes.Gray, 1);
    System.Drawing.Pen COL_GRID_DAY = new System.Drawing.Pen(System.Drawing.Brushes.LightGray, 1);
    System.Drawing.Pen COL_SUNSET = new System.Drawing.Pen(System.Drawing.Brushes.DarkRed, 2);
    System.Drawing.Pen COL_SUNRISE = new System.Drawing.Pen(System.Drawing.Brushes.DarkBlue, 2);
    System.Drawing.Brush COL_DATA = System.Drawing.Brushes.Green;
    System.Drawing.Brush COL_DATA2 = System.Drawing.Brushes.Red;

    ActivityData _data;
    Bitmap _bmp = null;
    float _width = 0;
    float _height = 0;
    Graphics _graphics;
    ColorTable _colorTable;
    float _maxDispValue;

    public ActivityDiagram(ColorTable colorTable)
    {
      _colorTable = colorTable;
    }
    
    public Bitmap createPlot(ActivityData hm, string title, int style, int maxDispValue, bool month, bool week, bool day, bool twilight, int offsetHours)
    {
      try
      {
        _data = hm;
        _maxDispValue = maxDispValue;
        _width = BMP_WITH;
        _height = BMP_HEIGHT;
        _bmp = new Bitmap((int)_width, (int)_height);
        _graphics = Graphics.FromImage(_bmp);
        _graphics.Clear(COL_BACK);
        int hours = 24 - DIAG_START_TIME.Hour + DIAG_END_TIME.Hour;
        createGrid(month, week, day, hours);
        if (twilight)
          createTwilightLines();
        createTitle(title);
        createLabels(month, week, day, hours);
        drawData(style, offsetHours);
        drawLegend(style);
      }
      catch (Exception ex)
      {
        DebugLog.log("unable to create activity diagram: " + ex.ToString(), enLogType.ERROR);
      }
      return _bmp;
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


    void drawLegend(int style)
    {
      int lh = 20;
      int bl = 20;
      int nights = _data.DaysWithData > 1 ? _data.DaysWithData - 1 : _data.DaysWithData;
      GraphHelper.createText(_graphics, _width - X_BR + bl, Y_BT, $"{BatInspector.Properties.MyResources.ActivityTotalCalls}: {_data.TotalCalls}", COL_TEXT);
      GraphHelper.createText(_graphics, _width - X_BR + bl, Y_BT + lh, $"{BatInspector.Properties.MyResources.ActivityNightsWithActivity}: {nights}", COL_TEXT);
      GraphHelper.createText(_graphics, _width - X_BR + bl, Y_BT + 2 * lh, $"{BatInspector.Properties.MyResources.ClassWidth}: {(60/_data.TicksPerHour).ToString()} min", COL_TEXT);
      GraphHelper.createText(_graphics, _width - X_BR + bl, Y_BT + 3 * lh, $"{BatInspector.Properties.MyResources.ActivityMaxCountsPerClass}: {(_data.MaxCount).ToString()}", COL_TEXT);

      GraphHelper.createText(_graphics, _width - X_BR + bl , Y_BT + 6 * lh, $"Position", COL_TEXT);
      GraphHelper.createText(_graphics, _width - X_BR + bl + 100 , Y_BT + 6 * lh, $"{Utils.LatitudeToString(_data.Latitude)}", COL_TEXT);
      GraphHelper.createText(_graphics, _width - X_BR + bl + 100, Y_BT + 7 * lh, $"{Utils.LongitudeToString(_data.Longitude)}", COL_TEXT);

      if (style == 0)
      {
        drawColorLegend(10 * lh, bl + 10);
      }
      else
      {
        GraphHelper.createText(_graphics, _width - X_BR + bl, Y_BT + 11*lh, $"{BatInspector.Properties.MyResources.ActivityMaxDisplayValue}: {(int)_maxDispValue}", COL_TEXT);
        GraphHelper.createText(_graphics, _width - X_BR + bl, Y_BT + 12*lh, $"{BatInspector.Properties.MyResources.ActivityValuesAbove}", COL_TEXT);
      }
    }
    

    void drawColorLegend(float bt,  float bl)
    {
      int barWidth = 20;
      float x = _width - X_BR + bl + barWidth / 2;
      float wColor = 40;
      GraphHelper.createText(_graphics, x - barWidth / 2, Y_BT + bt, $"{BatInspector.Properties.MyResources.Color}", COL_TEXT);
      GraphHelper.createText(_graphics, x + wColor, Y_BT + bt, $"{BatInspector.Properties.MyResources.Value}", COL_TEXT);
      bt += 40;
      for (int i = 0; i < 101; i++)
      {
        float y = Y_BT + bt + 2 * i;
        SolidBrush col = new SolidBrush(_colorTable.getColor(100 - i, 0, 100));
        GraphHelper.createRectangle(_graphics, x, y, barWidth, 2, col);
      }
      for (int i = 0; i < 5; i++)
      {
        float y = Y_BT + bt + 50 * i;
        _graphics.DrawLine(COL_GRID_WEEK, x + barWidth / 2, y, x + barWidth + 10, y);
        string val = ((int)(_maxDispValue * (4 - i) / 4)).ToString();
        GraphHelper.createText(_graphics, x + wColor, y - 8, val, COL_TEXT);
      }
      float xb = _width - X_BR + bl - 10;
      float yb = Y_BT + bt - 40 - 10;
      _graphics.DrawRectangle(COL_GRID_WEEK, xb, yb, 110, 270);
    }


    float getYCoord(int tick, int tOffset)
    {
      int hours = 24 - DIAG_START_TIME.Hour + DIAG_END_TIME.Hour;
      if (tick <= (DIAG_END_TIME.Hour * _data.TicksPerHour))
        tick += (24 - DIAG_START_TIME.Hour) * _data.TicksPerHour;
      if (tick >= (DIAG_START_TIME.Hour * _data.TicksPerHour))
        tick -= DIAG_START_TIME.Hour * _data.TicksPerHour;
      float yPos = Y_BT + (_height - Y_BB - Y_BT) * (tick + tOffset) / _data.TicksPerHour / hours;
      yPos = swapY(yPos);
      return yPos;
    }

    float getXCoord(DateTime d)
    {
      int days = (int)((_data.EndDate.Date - _data.StartDate).TotalDays);
      if (days == 0)
        days = 1;
      float dx = 1.0f / days * (_width - X_BL - X_BR);
      int dayIdx = (int)((d.Date - _data.StartDate.Date).TotalDays);
      float xPos = ((float)dayIdx + 0.5f) * dx + X_BL;
      return xPos;
    }

    float getYCoord(float hour, float offsetH)
    {
      return getYCoord((int)(hour * _data.TicksPerHour), (int)(offsetH * _data.TicksPerHour));
    }

    void calcMeanValue()
    {
      float sum = 0;
      int countEvents = 0;
      float meanValue = 0;
      for (int i = 0; i < _data.Days.Count; i++)
      {
        for (int j = 0; j < _data.Days[i].Counter.Count; j++)
        {
          if (_data.Days[i].Counter[j] > 0)
          {
            sum += _data.Days[i].Counter[j];
            countEvents++;
          }
        }
      }
      if (countEvents == 0)
        countEvents++;
      meanValue = sum / (float)countEvents;
      _maxDispValue = meanValue;
    }

    private void drawData(int style, int offsetHours)
    {
      int days = (int)(_data.EndDate.Date - _data.StartDate.Date).TotalDays;
      float dy = (_height - Y_BB - Y_BT) / 24;
      float dx = 1.0f / days * (_width - X_BL - X_BR);
      float maxDia = Math.Max(Math.Min(dy, dx) - 2, 1);


      float wBox = dx - 2;
      float hBox = dy / _data.TicksPerHour + 0.5f;
      int offset = offsetHours * _data.TicksPerHour;

      for (int i = 0; i < _data.Days.Count; i++)
      {
        int startTimeInTicks = DIAG_END_TIME.Hour * _data.TicksPerHour;
        for (int j = 0; j < _data.Days[i].Counter.Count; j++)
        {
          float xPos = getXCoord(_data.Days[i].Date);          
           if (j < startTimeInTicks)
            xPos -= dx;
          int tCountValue = _data.Days[i].Counter[j];
          switch (style)
          {
            case 0:
              if (tCountValue > 0)
              {
                Color col = _colorTable.getColor((double)tCountValue, 0, _maxDispValue);
                System.Drawing.Brush b = new System.Drawing.SolidBrush(col);
                GraphHelper.createRectangle(_graphics, xPos, getYCoord(j, offset), wBox, hBox, b);
              }
              break;

            case 1:
              wBox = (float)tCountValue / _maxDispValue * (dx - 2);
              if ((wBox > 0.01f) && (wBox < 2.0f))
                wBox = 2.0f;
              if (_data.Days[i].Counter[j] <= _maxDispValue)
                GraphHelper.createRectangle(_graphics, xPos, getYCoord(j, offset), wBox, hBox, COL_DATA);
              else
                GraphHelper.createRectangle(_graphics, xPos, getYCoord(j, offset), (dx - 2), hBox, COL_DATA2);
              break;

            case 2:
              float dotDia = (float)tCountValue / _maxDispValue * maxDia;
              if ((dotDia > 0.01f) && (dotDia < 3.0f))
                dotDia = 3.0f;
              if (tCountValue <= _maxDispValue)
                GraphHelper.createDot(_graphics, xPos, getYCoord(j, offset), dotDia, COL_DATA);
              else
                GraphHelper.createDot(_graphics, xPos, getYCoord(j, offset), (dx - 2), COL_DATA2);
              break;
          }
        }
      }
    }


    private void createTwilightLines()
    {
      DateTime currDay = _data.StartDate;
      while (currDay.AddDays(1) < _data.EndDate)
      {
        Sunrise.getSunSetSunRise(_data.Latitude, _data.Longitude, currDay, out int srH1, out int srM1, out int srS1, out int ssH1, out int ssM1, out int ssS1);
        float sr1 = (float)srH1 + (float)srM1 / 60 + (float)srS1 / 3600;
        float ss1 = (float)ssH1 + (float)ssM1 / 60 + (float)ssS1 / 3600;
        float x1 = getXCoord(currDay);
        float yr1 = getYCoord(sr1, 0);
        float ys1 = getYCoord(ss1, 0);
        currDay = currDay.AddDays(1);
        Sunrise.getSunSetSunRise(_data.Latitude, _data.Longitude, currDay, out int srH2, out int srM2, out int srS2, out int ssH2, out int ssM2, out int ssS2);
        float sr2 = (float)srH2 + (float)srM2 / 60 + (float)srS2 / 3600;
        float ss2 = (float)ssH2 + (float)ssM2 / 60 + (float)ssS2 / 3600;
        float x2 = getXCoord(currDay.Date);
        if ((sr1 <= DIAG_END_TIME.Hour) && (sr2 <= DIAG_END_TIME.Hour))
        {
          float yr2 = getYCoord(sr2, 0);
          _graphics.DrawLine(COL_SUNRISE, x1, yr1, x2, yr2);
        }
        if ((ss1 >= DIAG_START_TIME.Hour) && (ss2 >= DIAG_START_TIME.Hour))
        {
          float ys2 = getYCoord(ss2, 0);
          _graphics.DrawLine(COL_SUNSET, x1, ys1, x2, ys2);
        }
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


    private void createLabels(bool months, bool weeks, bool days, int hours)
    {
      int dayCnt = (int)(_data.EndDate - _data.StartDate).TotalDays;

      // time of day
      int offs = 7;
      for (float h = 0; h < 9.0; h += 2.0f)
      {
        float yPos = getYCoord(h, 0) - offs;
        GraphHelper.createText(_graphics, 5, yPos, ((int)h).ToString() + ":00", COL_TEXT);
      }
      for (float h = 18; h < 24.0; h += 2.0f)
      {
        float yPos = getYCoord(h, 0) - offs;
        GraphHelper.createText(_graphics, 5, yPos, ((int)h).ToString() + ":00", COL_TEXT);
      }

      // days
      if (weeks && days || days & !months)
      {
        DateTime currDay = _data.StartDate;
        for (int i = 0; i < dayCnt; i++)
        {
          if ((currDay.DayOfWeek == DayOfWeek.Monday) || (dayCnt < 10))
          {
            float xPos = X_BL + (float)i / dayCnt * (_width - X_BL - X_BR);
            GraphHelper.createText(_graphics, xPos, _height - 5, currDay.ToString("dd.MM.yy"), COL_TEXT, -90);
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
            GraphHelper.createText(_graphics, xPos, _height - 5, currDay.ToString("dd.MM.yy"), COL_TEXT, -90);
          }
          currDay = currDay.AddDays(1);
        }
      }
    }


    private void createGrid(bool month, bool week, bool day, int hours)
    {
      // time of day lines
      for (int i = 0; i < hours; i++)
      {
        float yPos = Y_BT + (_height - Y_BB - Y_BT) * i / hours;
        //       Pen pen = (i % 6 == 0) ? COL_GRID_WEEK : COL_GRID_DAY;
        Pen pen = COL_GRID_DAY;
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


