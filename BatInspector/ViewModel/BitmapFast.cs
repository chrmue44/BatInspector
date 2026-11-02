/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-07-31                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public delegate void dlgSetPixel(int x, int y, Color col);

public class Font
{
  static int fontHeight = 9;
  static int[] font = {
    '0',5,
    0b011111110,
    0b100000001,
    0b100000001,
    0b100000001,
    0b011111110,
    '1',3,
    0b100000010,
    0b111111111,
    0b100000000,
    '2',5,
    0b110000010,
    0b101000001,
    0b100100001,
    0b100100001,
    0b100011110,
    '3',5,
    0b010000010,
    0b100000001,
    0b100010001,
    0b100010001,
    0b011101110,
    '4',5,
    0b001111000,
    0b001000100,
    0b001000010,
    0b111111111,
    0b001000000,
    '5',5,
    0b010011111,
    0b100010001,
    0b100010001,
    0b100010001,
    0b011100001,
    '6',5,
    0b011111110,
    0b100010001,
    0b100010001,
    0b100010001,
    0b011100010,
    '7',5,
    0b000000001,
    0b111100001,
    0b000010001,
    0b000001001,
    0b000000111,
    '8',5,
    0b011101110,
    0b100010001,
    0b100010001,
    0b100010001,
    0b011101110,
    '9',5,
    0b010001110,
    0b100010001,
    0b100010001,
    0b100010001,
    0b011111110,
    'k',4,
    0b111111111,
    0b001100000,
    0b010010000,
    0b100001000,
    'H',5,
    0b111111111,
    0b000100000,
    0b000100000,
    0b000100000,
    0b111111111,
    'z',4,
    0b111000100,
    0b100100100,
    0b100010100,
    0b100001100,
    's',4,
    0b010011000,
    0b100100100,
    0b100100100,
    0b011001000,
    '[',3,
    0b111111111,
    0b100000001,
    0b100000001,
    ']',3,
    0b100000001,
    0b100000001,
    0b111111111,
    '.',2,
    0b110000000,
    0b110000000,
    ',',2,
    0b111000000,
    0b011000000,
    0xFFFF,1,
    0
  };

  public static void printString(string str, int x, int y, Color fontColor, dlgSetPixel dlgSetPixel)
  {
    int xPos = x;
    for (int i = 0; i < str.Length; i++)
    {
      int advance = printChar(str[i], xPos, y, fontColor, dlgSetPixel);
      xPos += advance;
    }
  }

  static int printChar(char c, int x, int y, Color fontColor, dlgSetPixel dlgSetPixel)
  {
    int i = 0;
    int advance = 1;
    while (font[i] != 0xFFFF)
    {
      if (c == font[i])
      {
        advance = font[i + 1];
        break;
      }
      i += font[i+1] + 2;
    }

    i += 2;
    for (int xl = 0; xl < advance; xl++)
    {
      int row = font[i + xl];
      for (int yl = 0; yl < fontHeight; yl++)
      {
        if ((row & (1 << yl)) != 0)
          dlgSetPixel(x + xl, y + yl, fontColor);
      }
    }
    return advance + 1;
  }
}

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

  public void print(string str, int x, int y, Color color)
  {
    Font.printString(str, x, y, color, setPixel);
  }

  public void printLineX(int x, int y, int length, Color color, int dotDist = 0)
  {
    for (int i = 0; i < length; i++)
    {
      if (dotDist == 0 || i % (dotDist + 1) == 0)
        setPixel(x + i, y, color);
    }
  }

  public void printLineY(int x, int y, int length, Color color, int dotDist = 0)
  {
    for (int i = 0; i < length; i++)
    {
      if (dotDist == 0 || i % (dotDist + 1) == 0)
        setPixel(x, y + i, color);
    }
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
