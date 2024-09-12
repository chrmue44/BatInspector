﻿/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows;
using System.Drawing;

namespace BatInspector
{
  public class winUtils
  {
    private const int GWL_STYLE = -16;
    private const int WS_SYSMENU = 0x80000;

    public static void hideCloseButton(IntPtr hwnd)
    {
      SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

  }

  public class WpfScreen
  {
    public static IEnumerable<WpfScreen> AllScreens()
    {
      foreach (Screen screen in System.Windows.Forms.Screen.AllScreens)
      {
        yield return new WpfScreen(screen);
      }
    }

    public static WpfScreen GetScreenFrom(Window window)
    {
      WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
      Screen screen = System.Windows.Forms.Screen.FromHandle(windowInteropHelper.Handle);
      WpfScreen wpfScreen = new WpfScreen(screen);
      return wpfScreen;
    }

    public static WpfScreen GetScreenFrom(System.Drawing.Point point)
    {
      int x = (int)Math.Round((double)point.X);
      int y = (int)Math.Round((double)point.Y);

      // are x,y device-independent-pixels ??
      System.Drawing.Point drawingPoint = new System.Drawing.Point(x, y);
      Screen screen = System.Windows.Forms.Screen.FromPoint(drawingPoint);
      WpfScreen wpfScreen = new WpfScreen(screen);

      return wpfScreen;
    }

    public static WpfScreen Primary
    {
      get { return new WpfScreen(System.Windows.Forms.Screen.PrimaryScreen); }
    }

    private readonly Screen screen;

    internal WpfScreen(System.Windows.Forms.Screen screen)
    {
      this.screen = screen;
    }

    public Rect DeviceBounds
    {
      get { return this.GetRect(this.screen.Bounds); }
    }

    public Rect WorkingArea
    {
      get { return this.GetRect(this.screen.WorkingArea); }
    }

    private Rect GetRect(Rectangle value)
    {
      // should x, y, width, height be device-independent-pixels ??
      return new Rect
      {
        X = value.X,
        Y = value.Y,
        Width = value.Width,
        Height = value.Height
      };
    }

    public bool IsPrimary
    {
      get { return this.screen.Primary; }
    }

    public string DeviceName
    {
      get { return this.screen.DeviceName; }
    }
  }
}
