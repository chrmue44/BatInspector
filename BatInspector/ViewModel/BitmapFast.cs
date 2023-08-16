/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2023-07-31                                       
 *   Copyright (C) 2023: christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/

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
    Bitmap _bmp;  

    public Bitmap Bmp { get { return _bmp; } }

    public BitmapFast(int width, int height)
    {
      _w = width;
      _h = height;
      PixelFormat fmt = PixelFormat.Format32bppRgb;

      int pixelFormatSize = Image.GetPixelFormatSize(fmt);
      _stride = width * pixelFormatSize;
      int padding = 32 - (_stride % 32);
      if (padding < 32)
        _stride += padding;
      _pixel = new int[(_stride / 32) * _h];
      IntPtr addr = Marshal.UnsafeAddrOfPinnedArrayElement(_pixel, 0);
      _bmp = new Bitmap(_w, _h, _stride / 8, fmt, addr);
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
