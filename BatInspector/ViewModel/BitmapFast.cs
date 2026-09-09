/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-07-31                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

/* old version

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
*/
 // improved by Gemini
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public class BitmapFast : IDisposable
{
  private readonly int _w;
  private readonly int _h;
  private readonly int[] _pixel;
  private GCHandle _gcHandle;
  private bool _disposed;

  public Bitmap Bmp { get; }

  public BitmapFast(int width, int height)
  {
    _w = width;
    _h = height;
    _pixel = new int[width * height];

    // Array im Speicher fixieren, damit der GC es nicht verschiebt
    _gcHandle = GCHandle.Alloc(_pixel, GCHandleType.Pinned);
    IntPtr addr = _gcHandle.AddrOfPinnedObject();

    // Format32bppArgb erzwingen
    Bmp = new Bitmap(_w, _h, _w * 4, PixelFormat.Format32bppArgb, addr);
  }

  public void setPixel(int x, int y, Color col)
  {
    if (x < 0 || x >= _w || y < 0 || y >= _h) return;

    // Force Alpha = 255 (0xFF000000), falls die Farbe Alpha = 0 hat
    int argb = col.ToArgb();
    if ((argb & 0xFF000000) == 0)
      argb |= unchecked((int)0xFF000000);

    _pixel[y * _w + x] = argb;
  }

  public void setPixel(int x, int y, int argb)
  {
    if (x < 0 || x >= _w || y < 0 || y >= _h) return;

    if ((argb & 0xFF000000) == 0)
      argb |= unchecked((int)0xFF000000);

    _pixel[y * _w + x] = argb;
  }

  public void Dispose()
  {
    if (!_disposed)
    {
      Bmp?.Dispose();
      if (_gcHandle.IsAllocated)
        _gcHandle.Free();
      _disposed = true;
    }
  }
}
