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
using System;
using System.Collections.Generic;
using System.IO;
using libParser;

namespace BatInspector
{
  public class MthdListScript : MethodList
  {
    Methods _methods;
    static MthdListScript _inst;
    List<HelpTabItem> _scriptHelpTab = new List<HelpTabItem>();

    public MthdListScript() : base()
    {
      _methods = null;

    }

    public override void initMthdTab()
    {
      _inst = this;
      _scriptHelpTab = new List<HelpTabItem>();
      addMethod(new FuncTabItem("setSampleRate", setSampleRate));
      _scriptHelpTab.Add(new HelpTabItem("setSampleRate", "sets the samplerate of a file",
                      new List<string> { "1: fileName","2:sample rate" }, new List<string> { "" }));
      addMethod(new FuncTabItem("getSampleRate", getSampleRate)); 
      _scriptHelpTab.Add(new HelpTabItem("getSampleRate", "returns the samplerate of a file",
                      new List<string> { "1: fileName" }, new List<string> { "1: sampling rate" }));
      addMethod(new FuncTabItem("rescaleSampleRate", rescaleSampleRate));
      _scriptHelpTab.Add(new HelpTabItem("rescaleSampleRate", "rescales the samplerate of a file",
                      new List<string> { "1: fileName", "2:factor" }, new List<string> { "1: new sampling rate" }));
    }


    static tParseError setSampleRate(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 2)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        string fName = argv[0].getString();
        argv[1].changeType(AnyType.tType.RT_INT64);
        long sampleRate = argv[1].getInt64();
        if (File.Exists(fName))
        {
          WavFile wav = new WavFile();
          wav.readFile(fName);
          wav.SamplingRate = (uint)sampleRate;
          wav.saveFile();
        }
        else
          err = tParseError.ARG1_OUT_OF_RANGE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }

    static tParseError rescaleSampleRate(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 2)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        string fName = argv[0].getString();
        argv[1].changeType(AnyType.tType.RT_FLOAT);
        double fact = argv[1].getFloat();
        if (File.Exists(fName))
        {
          WavFile wav = new WavFile();
          wav.readFile(fName);
          result.assignInt64((long)(fact * wav.SamplingRate));
          wav.SamplingRate = (uint)result.getInt64();
          wav.saveFile();
        }
        else
          err = tParseError.ARG1_OUT_OF_RANGE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }


    static tParseError getSampleRate(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 1)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        string fName = argv[0].getString();
        if (File.Exists(fName))
        {
          WavFile wav = new WavFile();
          wav.readFile(fName);
          result.assignInt64((long)wav.SamplingRate);
        }
        else
          err = tParseError.ARG1_OUT_OF_RANGE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }
      

    // liefert die Tabelle mit detaillierter Hilfe zu den Methoden
    public override List<HelpTabItem> getHelpTab()
    {
      return _scriptHelpTab;
    }

    // Ueberschrift fuer Hilfe zur Methodenliste
    public override string getMthdListHelp()
    {
      return "\nlist of math commands";
    }


  }
}
