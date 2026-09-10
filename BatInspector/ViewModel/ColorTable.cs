/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Properties;
using libParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

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

    public Int32[] ColorTableInt32 { get { return _intColorTable; } }

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

    static int getColorFromGradient(double val, List<ColorItem> list)
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
    public System.Windows.Media.Color getSwmColor(double val, double min, double max, double blackLevel)
    {
      Color col = getColor(val, min, max, blackLevel);
      System.Windows.Media.Color retVal;
      retVal = System.Windows.Media.Color.FromArgb(col.A, col.R, col.G, col.B);
      return retVal;
    }


    public Color getColor(double val, double min, double max, double blackLevel)
    {
      if (val < (min + (max - min) * blackLevel / 100.0))
        return _colorTable[0];
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

  [DataContract]
  public class ColorPreset
  {
    [DataMember]
    public string Name { get; set; } = "";

    [DataMember]
    [LocalizedCategory("SetCatColorGradient"),
    LocalizedDescription("SpecDescColorRed"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Browsable(false)]
    public List<ColorItem> ColorGradientRed { get; set; } = new List<ColorItem>();

    [DataMember]
    [LocalizedCategory("SetCatColorGradient"),
    LocalizedDescription("SpecDescColorGreen"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Browsable(false)]
    public List<ColorItem> ColorGradientGreen { get; set; } = new List<ColorItem>();

    [DataMember]
    [LocalizedCategory("SetCatColorGradient"),
    LocalizedDescription("SpecDescColorBlue"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Browsable(false)]
    public List<ColorItem> ColorGradientBlue { get; set; } = new List<ColorItem>();
  }

  [DataContract]
  public class ColorPresetCollection
  {
    [DataMember]
    [LocalizedCategory("SetCatColorGradient"),
    LocalizedDescription("SpecDescColorRed"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Browsable(false)]
    
    List<ColorPreset> _presets = new List<ColorPreset>();

    public enum enColorGrading
    {
      GREEN_BLUE,
      GREEN_BLACK,
      BLUE_WHITE,
      RED_BLUE,
      MAGENTA_BLACK,
    }

    public ColorPresetCollection() 
    {
      init();
    }

    public int Count { get { return _presets.Count; }   }

    public string getName(int i)
    {
      if ((i < _presets.Count) && (i >= 0))
        return _presets[i].Name;
      else
        return "";
    }

    public ColorPreset? getPresetCopy(int i)
    {
      if ((i < 0) || (i >= _presets.Count))
        return null;

      ColorPreset retVal = new ColorPreset();
      retVal.Name = _presets[i].Name;
      retVal.ColorGradientBlue = getGradientCopy(_presets[i].ColorGradientBlue);
      retVal.ColorGradientGreen = getGradientCopy(_presets[i].ColorGradientGreen);
      retVal.ColorGradientRed = getGradientCopy(_presets[i].ColorGradientRed);
      return retVal;
    }

    private static List<ColorItem> getGradientCopy(List<ColorItem> gradient)
    { 
      List<ColorItem> retVal = new List<ColorItem> ();
      foreach (ColorItem item in gradient)
      {
        retVal.Add(new ColorItem(item));
      }
      return retVal;
    }

    public static ColorPreset initColorGradient(enColorGrading type)
    {
      ColorPreset p = new ColorPreset();
      switch (type)
      {
        case enColorGrading.GREEN_BLUE:
        p.Name = MyResources.ColGradientBlueToGreen;
          p.ColorGradientBlue = new List<ColorItem>
          {
            new ColorItem(0, 0),
            new ColorItem(100, 20),
            new ColorItem(0, 40),
            new ColorItem(0, 75),
            new ColorItem(40, 100)
          };
          p.ColorGradientGreen = new List<ColorItem>
          {
            new ColorItem(0, 0),
            new ColorItem(0, 20),
            new ColorItem(200, 60),
            new ColorItem(200, 75),
            new ColorItem(0, 100)
          };
          p.ColorGradientRed = new List<ColorItem>
          {
            new ColorItem(0, 0),
            new ColorItem(0, 20),
            new ColorItem(100, 60),
            new ColorItem(200, 75),
            new ColorItem(255, 100)
          };
          break;

        case enColorGrading.GREEN_BLACK:
          p.Name = MyResources.ColGradientBlackToGreen;
          p.ColorGradientBlue = new List<ColorItem>
          {
            new ColorItem(0, 0),
            new ColorItem(0, 30),
            new ColorItem(0, 70),
            new ColorItem(0, 75),
            new ColorItem(40, 100)
          };
          p.ColorGradientGreen = new List<ColorItem>
          {
            new ColorItem(0, 0),
            new ColorItem(70, 30),
            new ColorItem(200, 70),
            new ColorItem(200, 75),
            new ColorItem(0, 100)
          };
          p.ColorGradientRed = new List<ColorItem>
          {
            new ColorItem(0, 0),
            new ColorItem(0, 30),
            new ColorItem(200, 70),
            new ColorItem(200, 75),
            new ColorItem(255, 100)
          };
          break;

        case enColorGrading.BLUE_WHITE:
          p.Name = MyResources.ColGradientWhiteToBlue;
          p.ColorGradientBlue = new List<ColorItem>
          {
            new ColorItem(255, 0),
            new ColorItem(255, 30),
            new ColorItem(255, 50),
            new ColorItem(255, 75),
            new ColorItem(255, 100)
          };
          p.ColorGradientGreen = new List<ColorItem>
          {
            new ColorItem(255, 0),
            new ColorItem(230, 30),
            new ColorItem(150, 50),
            new ColorItem(100, 75),
            new ColorItem(0, 100)
          };
          p.ColorGradientRed = new List<ColorItem>
          {
            new ColorItem(255, 0),
            new ColorItem(230, 30),
            new ColorItem(150, 50),
            new ColorItem(100, 75),
            new ColorItem(0, 100)
          };
          break;

        case enColorGrading.MAGENTA_BLACK:
          p.Name = MyResources.ColGradientBlackToMagenta;
          p.ColorGradientBlue = new List<ColorItem>
          {
            new ColorItem(50,  0),
            new ColorItem(150, 20),
            new ColorItem(0,   50),
            new ColorItem(40,  75),
            new ColorItem(255, 100)
          };
          p.ColorGradientGreen = new List<ColorItem>
          {
            new ColorItem(0, 0),
            new ColorItem(255, 20),
            new ColorItem(240, 50),
            new ColorItem(0, 75),
            new ColorItem(0, 100)
          };
          p.ColorGradientRed = new List<ColorItem>
          {
            new ColorItem(0, 0),
            new ColorItem(150, 20),
            new ColorItem(240, 50),
            new ColorItem(255, 75),
            new ColorItem(255, 100)
          };
          break;

        case enColorGrading.RED_BLUE:
          p.Name = MyResources.ColGradientBlueToRed;
          p.ColorGradientBlue = new List<ColorItem>
          {
            new ColorItem(0,  0),
            new ColorItem(255, 10),
            new ColorItem(0,   50),
            new ColorItem(0,  75),
            new ColorItem(255, 100)
          };
          p.ColorGradientGreen = new List<ColorItem>
          {
            new ColorItem(0, 0),
            new ColorItem(0, 10),
            new ColorItem(255, 50),
            new ColorItem(0, 75),
            new ColorItem(255, 100)
          };
          p.ColorGradientRed = new List<ColorItem>
          {
            new ColorItem(0, 0),
            new ColorItem(0, 10),
            new ColorItem(150, 50),
            new ColorItem(255, 75),
            new ColorItem(255, 100)
          };
          break;

      }
      return p;
    }

    public void saveAs(string fName)
    {
      try
      {
        using (StreamWriter file = new StreamWriter(fName))
        {
          using (MemoryStream stream = new MemoryStream())
          {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ColorPresetCollection));
            ser.WriteObject(stream, this);
            StreamReader sr = new StreamReader(stream);
            stream.Seek(0, SeekOrigin.Begin);
            string str = sr.ReadToEnd();
            file.Write(JsonHelper.FormatJson(str));
            file.Close();
            DebugLog.log("settings saved to '" + fName + "'", enLogType.INFO);
          }
        }
      }
      catch (Exception e)
      {
        DebugLog.log("failed to write colr preset collection" + fName + ": " + e.ToString(), enLogType.ERROR);
      }
    }



    public static void load()
    {

    }

    private void init()
    {
      _presets = new List<ColorPreset>();
      ColorPreset preset = initColorGradient(enColorGrading.GREEN_BLUE) ;
      _presets.Add(preset);
      preset = initColorGradient(enColorGrading.GREEN_BLACK);
      _presets.Add(preset);
      preset = initColorGradient(enColorGrading.MAGENTA_BLACK);
      _presets.Add(preset);
      preset = initColorGradient(enColorGrading.BLUE_WHITE);
      _presets.Add(preset);
      preset = initColorGradient(enColorGrading.RED_BLUE);
      _presets.Add(preset);
    }
  }
}
