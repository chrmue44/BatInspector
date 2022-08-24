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
using libScripter;

namespace BatInspector
{
  public class MthdListScript : MethodList
  {
    static MthdListScript _inst;
    List<Csv> _listCsv;
    List<HelpTabItem> _scriptHelpTab = new List<HelpTabItem>();
    ViewModel _model;
    string _wrkDir;

    public MthdListScript(ViewModel model, string wrkDir) : base()
    {
      _listCsv = new List<Csv>();
      _model = model;
      _wrkDir = wrkDir;
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
      addMethod(new FuncTabItem("openCsv", openCsv));
      _scriptHelpTab.Add(new HelpTabItem("openCsv", "opens a csv file and reads content to memory",
                      new List<string> { "1: fileName", "2: 1=with header (optional)","3: separator (optional)" }, new List<string> { "1: csv file handle" })) ;
      addMethod(new FuncTabItem("closeCsv", closeCsv));
      _scriptHelpTab.Add(new HelpTabItem("closeCsv", "closes a csv file and releases the memory",
                      new List<string> { "1: handle", "2: 1=write (optional)" }, new List<string> { "1: 0=OK" }));
      addMethod(new FuncTabItem("getCell", getCell));
      _scriptHelpTab.Add(new HelpTabItem("getCell", "get a cell content of a previously opened csv file",
                      new List<string> { "1: handle", "2: row nr", "3: col name or nr" }, new List<string> { "1: cell content" }));
      addMethod(new FuncTabItem("setCell", setCell));
      _scriptHelpTab.Add(new HelpTabItem("setCell", "set a cell content of a previously opened csv file",
                      new List<string> { "1: handle", "2: row nr", "3: col name or nr", "4: value" }, new List<string> { "1: cell content" }));
      addMethod(new FuncTabItem("getRowCount", getRowCount));
      _scriptHelpTab.Add(new HelpTabItem("getRowCount", "get row count of a previously opened csv file",
                      new List<string> { "1: handle"}, new List<string> { "1: nr of rows (including header)" }));
    }

    static tParseError openCsv(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 1)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        string fName = argv[0].getString();
        if((fName.IndexOf("/") < 0) && (fName.IndexOf("\\") < 0))
          fName = _inst._wrkDir + "/" + fName;

        string sep = ";";
        bool withHeader = false;
        if (argv.Count >= 2)
        {
          argv[1].changeType(AnyType.tType.RT_INT64);
          withHeader = (argv[1].getInt64() == 1);
        }
        if (argv.Count >= 3)
        {
          argv[1].changeType(AnyType.tType.RT_STR);
          sep = argv[2].getString();
        }
        if (File.Exists(fName))
        {
          Csv csv = new Csv();
          csv.read(fName, sep, withHeader);
          _inst._listCsv.Add(csv);
          result.assignInt64(_inst._listCsv.Count - 1);
        }
        else
          err = tParseError.ARG1_OUT_OF_RANGE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }

    static tParseError closeCsv(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 1)
      {
        argv[0].changeType(AnyType.tType.RT_INT64);
        int handle = (int)argv[0].getInt64();
        bool write = false;
        if (argv.Count >= 2)
        {
          argv[1].changeType(AnyType.tType.RT_INT64);
          write = (argv[1].getInt64() == 1);
        }
        if (handle < _inst._listCsv.Count)
        {
          Csv csv = _inst._listCsv[handle];
          if (File.Exists(csv.FileName))
          {
            if (write)
              csv.write();
            _inst._listCsv.Remove(csv);
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;

      return err;
    }

    static tParseError getRowCount(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 1)
      {
        argv[0].changeType(AnyType.tType.RT_INT64);
        int handle = (int)argv[0].getInt64();
        if (handle < _inst._listCsv.Count)
        {
          Csv csv = _inst._listCsv[handle];
          result.assignInt64(csv.RowCnt);
        }
        else
          err = tParseError.ARG1_OUT_OF_RANGE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;

      return err;
    }

    static tParseError getCell(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 3)
      {
        argv[0].changeType(AnyType.tType.RT_INT64);
        int handle = (int)argv[0].getInt64();
        argv[1].changeType(AnyType.tType.RT_INT64);
        int row = (int)argv[1].getInt64();
        argv[2].changeType(AnyType.tType.RT_STR);
        string col = argv[2].getString();
        if ((handle >= 0) && (handle < _inst._listCsv.Count))
        {
          Csv csv = _inst._listCsv[handle];
          string ret = csv.getCell(row, col);
          result.assign(ret);
        }
        else
          err = tParseError.ARG1_OUT_OF_RANGE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }

    static tParseError setCell(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 4)
      {
        argv[0].changeType(AnyType.tType.RT_INT64);
        int handle = (int)argv[0].getInt64();
        argv[1].changeType(AnyType.tType.RT_INT64);
        int row = (int)argv[1].getInt64();
        argv[2].changeType(AnyType.tType.RT_STR);
        string col = argv[2].getString();
        argv[3].changeType(AnyType.tType.RT_STR);
        string value = argv[3].getString();
        if ((handle >= 0) && (handle < _inst._listCsv.Count))
        {
          Csv csv = _inst._listCsv[handle];
          csv.setCell(row, col, value);
        }
        else
          err = tParseError.ARG1_OUT_OF_RANGE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
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
