/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-07-31                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/


using DSPLib;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BatInspector
{
  public class BitmapFast
  {
    int _w;
    int _h;
    int[] _pixel;
    int _stride;
    PixelFormat _fmt;

    public Bitmap Bmp 
    { get
      {
        lock (this)
        {
          IntPtr addr = Marshal.UnsafeAddrOfPinnedArrayElement(_pixel, 0);
          return new Bitmap(_w, _h, _stride / 8, _fmt, addr);
        }
      }
    }

    public BitmapFast(int width, int height)
    {
      _w = width;
      _h = height;
      _fmt = PixelFormat.Format32bppRgb;

      int pixelFormatSize = Image.GetPixelFormatSize(_fmt);
      _stride = width * pixelFormatSize;
      int padding = 32 - (_stride % 32);
      if (padding < 32)
        _stride += padding;
      _pixel = new int[(_stride / 32) * _h];
    }

    public void setPixel(int x, int y, Color col)
    {
      int idx = y * _w + x;
      _pixel[idx] = col.B | (col.G << 8) | (col.R << 16);
    }

    public void setPixel(int x, int y, int col)
    {
      int idx = y * _w + x;
      _pixel[idx] = col;
    }

    public Color getPixel(int x, int y)
    {
      int idx = y * _w + x;
      return Color.FromArgb((int)_pixel[idx]);
    }
  }
}
