/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace BatInspector
{
  public class ColorTable
  {
    Color[] _colorTable;
    ViewModel _model;

    public ColorTable(ViewModel model)
    {
      _model = model;
    }

    public void createColorLookupTable()
    {
      _colorTable = new Color[100];
      for (int i = 0; i < _colorTable.Length; i++)
      {
        int r = getColorFromGradient((double)i, AppParams.Inst.ColorGradientRed);
        int g = getColorFromGradient((double)i, AppParams.Inst.ColorGradientGreen);
        int b = getColorFromGradient((double)i, AppParams.Inst.ColorGradientBlue);
        _colorTable[i] = Color.FromArgb(r, g, b);
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
