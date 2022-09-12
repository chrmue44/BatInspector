/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: Christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/
using System.Collections.Generic;
using System.Windows;
using libParser;

namespace BatInspector
{
  /// <summary>
  /// Interaktionslogik für Log.xaml
  /// </summary>
  public partial class FrmLog : Window
  {
    List<stLogEntry> _entries;

    public FrmLog()
    {
      InitializeComponent();
      _entries = new List<stLogEntry>();
    }
  }
}
