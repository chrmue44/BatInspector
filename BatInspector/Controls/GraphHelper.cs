﻿
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Globalization;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace BatInspector.Controls
{
  public class GraphHelper
  {
    public static void createLine(Canvas ca, double x1, double y1, double x2, double y2, Brush brush, int thickness = 1)
    {
      Line li = new Line();
      li.X1 = x1;
      li.X2 = x2;
      li.Y1 = y1;
      li.Y2 = y2;
      li.Stroke = brush;
      li.StrokeThickness = thickness;
      ca.Children.Add(li);
    }

    public static void createBox(Canvas ca, double x1, double y1, double x2, double y2, int stroke, Brush brushBorder, Brush brushFill)
    {
      if ((x2 > x1) && (y2 > y1))
      {
        Rectangle r = new Rectangle();
        r.Width = x2 - x1;
        r.Height = y2 - y1;
        r.Stroke = brushBorder;
        r.StrokeThickness = stroke;
        r.Fill = brushFill;
        Canvas.SetLeft(r, x1);
        Canvas.SetTop(r, y1);
        ca.Children.Add(r);
      }
    }

    public static void createText(Canvas can, double x, double y, string text, Color color)
    {
      TextBlock textBlock = new TextBlock();
      textBlock.Text = text;
      textBlock.Foreground = new SolidColorBrush(color);
      textBlock.TextAlignment = TextAlignment.Right;
      Canvas.SetLeft(textBlock, x);
      Canvas.SetTop(textBlock, y);
      can.Children.Add(textBlock);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="can">canvas</param>
    /// <param name="x">x position of ruler on canvas</param>
    /// <param name="y">y position of ruler on canvas</param>
    /// <param name="width">width of ruler</param>
    /// <param name="height">height of ruler</param>
    /// <param name="min">minimum value for y axis</param>
    /// <param name="max">maximum value for y axis</param>
    /// <param name="nrTicks">nr of ticks</param>
    public static void createRulerY(Canvas can, double x, double y, double height, double min, double max, int nrTicks = 9, string nrFmt = "0.#")
    {
      if (max == min)
        max += 1;
      double[] fTicks = GraphHelper.createTicks(nrTicks, min, max);
      nrTicks = fTicks.Length;
      GraphHelper.createLine(can, x , y, x , y + height, Brushes.Black);
      double span = max - min;
      for (int i = 0; i < nrTicks; i++)
      {
        double yp =y + height - ((fTicks[i] - min) / span * height);
        GraphHelper.createLine(can,x - 5, yp, x, yp, Brushes.Black);
        string str = fTicks[i].ToString(nrFmt, CultureInfo.InvariantCulture);
        GraphHelper.createText(can, x - 32, yp - 9, str, Colors.Black);
      }
      double ymax = y;
      GraphHelper.createLine(can, x - 5, ymax, x, ymax, Brushes.Black);
      GraphHelper.createText(can, x - 32, ymax - 9, max.ToString(nrFmt, CultureInfo.InvariantCulture), Colors.Black);
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="can">canvas</param>
    /// <param name="x">x position of ruler on canvas</param>
    /// <param name="y">y position of ruler on canvas</param>
    /// <param name="width">width of ruler</param>
    /// <param name="height">height of ruler</param>
    /// <param name="min">minimum value for y axis</param>
    /// <param name="max">maximum value for y axis</param>
    /// <param name="nrTicks">nr of ticks</param>
    public static void createRulerX(Canvas can, double x, double y, double width, double min, double max, int nrTicks = 9, string nrFmt = "0.#")
    {
      double[] tTicks = GraphHelper.createTicks(nrTicks, min, max);
      nrTicks = tTicks.Length;
      GraphHelper.createLine(can, x, y + 3, x + width, y + 3, Brushes.Black);
      double span = max - min;
      double min1 = min;
      for (int i = 0; i < nrTicks; i++)
      {
        double xp = x + width * (tTicks[i] - min1) / span;
        GraphHelper.createLine(can, xp, y + 3, xp, y + 10, Brushes.Black);
        string str = tTicks[i].ToString(nrFmt, CultureInfo.InvariantCulture);
        GraphHelper.createText(can, xp - 10,y + 15, str, Colors.Black);
      }
      double xmax = x + width;
      GraphHelper.createLine(can, xmax, y + 3, xmax, y + 10, Brushes.Black);
      GraphHelper.createText(can, xmax - 10, y + 15, max.ToString(nrFmt, CultureInfo.InvariantCulture), Colors.Black);
    }

    public static void createRulerX(Canvas can, double x, double y, double width, double min, double max, double[] tTicks, string nrFmt = "0.#")
    {
      GraphHelper.createLine(can, x, y + 3, x + width, y + 3, Brushes.Black);
      double span = max - min;
      double min1 = min;
      for (int i = 0; i < tTicks.Length; i++)
      {
        double xp = x + width * i / tTicks.Length;
        GraphHelper.createLine(can, xp, y + 3, xp, y + 10, Brushes.Black);
        string str = tTicks[i].ToString(nrFmt, CultureInfo.InvariantCulture);
        GraphHelper.createText(can, xp - 10, y + 15, str, Colors.Black);
      }
      double xmax = x + width;
      GraphHelper.createLine(can, xmax, y + 3, xmax, y + 10, Brushes.Black);
      GraphHelper.createText(can, xmax - 10, y + 15, max.ToString(nrFmt, CultureInfo.InvariantCulture), Colors.Black);
    }


    public static double[] createTicks(int n, double min, double max)
    {
      double[] retVal = new double[n];

      double w = max - min;
      double step = nextStepDown(w / n);
      double min1 = nextStepUp(min, step);
      while (n > 2)
      {
        if (step * n > 3 * w / 4)
        {
          retVal = new double[n];
          for (int i = 0; i < n; i++)
          {
            retVal[i] = i * step + min1;
          }
          break;
        }
        else
        {
          n--;
          step = nextStepDown(w / n);
          min1 = nextStepUp(min, step);
        }
      }
      return retVal;
    }

    public static double nextStepDown(double step)
    {
      double expd = Math.Log10(step);
      int expi = (int)expd;
      if (expd < 0)
        expi--;

      double b = 5 * Math.Pow(10, expi);
      if (step > b)
        return b;
      b = 2 * Math.Pow(10, expi);
      if (step > b)
        return b;
      else
        return Math.Pow(10, expi);
    }

    public static double nextStepUp(double val, double step)
    {
      double retVal = val + step;
      retVal -= retVal % step;
      return retVal;
    }
  }
}