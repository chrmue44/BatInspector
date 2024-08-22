/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System;

namespace BatInspector
{
  public class ColorTable
  {
    Color[] _colorTable;
    Int32[] _intColorTable;


    public ColorTable()
    {
      _colorTable = new Color[100];
      _intColorTable = new Int32[100];
    }

    public Int32[] ColorTableInt32 { get { return _intColorTable;} }

    public void createColorLookupTable()
    {
      for (int i = 0; i < _colorTable.Length; i++)
      {
        int r = getColorFromGradient((double)i, AppParams.Inst.ColorGradientRed);
        int g = getColorFromGradient((double)i, AppParams.Inst.ColorGradientGreen);
        int b = getColorFromGradient((double)i, AppParams.Inst.ColorGradientBlue);
        _colorTable[i] = Color.FromArgb(r, g, b);
        _intColorTable[i] = r << 16 | g << 8 | b;
      }
    }

    int getColorFromGradient(double val, List<ColorItem> list)
    {
      double retVal = 100;
      for (int i = 0; i < (list.Count - 1); i++)
      {
        if ((val >= list[i].Value) && (val < list[i + 1].Value))
        {
          double m = (double)(list[i + 1].Color - list[i].Color) / (list[i + 1].Value - list[i].Value);
          double b = list[i].Color - m * list[i].Value;
          retVal = m * val + b;
        }
      }
      return (int)retVal;
    }
    public System.Windows.Media.Color getSwmColor(double val, double min, double max)
    {
      Color col = getColor(val, min, max);
      System.Windows.Media.Color retVal;
      retVal = System.Windows.Media.Color.FromArgb(col.A, col.R, col.G, col.B);
      return retVal;
    }


    public Color getColor(double val, double min, double max)
    {
      if (val < min)
        return _colorTable[0];
      else if (val > max)
        return _colorTable.Last();
      else
      {
        int i = (int)((val - min) / (max - min) * _colorTable.Length);
        if (i < _colorTable.Length)
          return _colorTable[i];
        else
          return _colorTable.Last();
      }
    }
  }
}
