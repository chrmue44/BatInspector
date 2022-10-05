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
      addMethod(new FuncTabItem("getPrjFileCount", getPrjFileCount));
      _scriptHelpTab.Add(new HelpTabItem("getPrjFileCount", "get nr of files of open project",
                      new List<string> { "" }, new List<string> { "1: nr of files in project" }));
      addMethod(new FuncTabItem("setFileInfo", setFileInfo));
      _scriptHelpTab.Add(new HelpTabItem("setFileInfo", "set specific data for specified file",
                      new List<string> { "1: file index (0..n)","2: file info type","3:data" }, new List<string> { "" }));
      addMethod(new FuncTabItem("getFileName", getFileName));
      _scriptHelpTab.Add(new HelpTabItem("getFileName", "get file name for specified file index",
                      new List<string> { "1: file index (0..n)", "2: file info type" }, new List<string> { "" }));
      addMethod(new FuncTabItem("getFileInfo", getFileInfo));
      _scriptHelpTab.Add(new HelpTabItem("getFileInfo", "get specific data for specified file",
                      new List<string> { "1: file index (0..n)", "2: file info type" }, new List<string> { "" }));
      addMethod(new FuncTabItem("getCallCount", getCallCount));
      _scriptHelpTab.Add(new HelpTabItem("getCallCount", "get number of calls for spec. file in open project",
                      new List<string> { "1: file index (0..n)" }, new List<string> { "1: nr of calls for selected file" }));
      addMethod(new FuncTabItem("getCallInfo", getCallInfo));
      _scriptHelpTab.Add(new HelpTabItem("getCallInfo", "get specific data for specified call",
                      new List<string> { "1: file index (0..n)", "2: call index (0..n)", "3: type of information" }, new List<string> { "1: call data" }));
      addMethod(new FuncTabItem("setCallInfo", setCallInfo));
      _scriptHelpTab.Add(new HelpTabItem("setCallInfo", "set specific data for specified call",
                      new List<string> { "1: file index (0..n)", "2: call index (0..n)", "3: type of information","4: data to set" }, new List<string> { "1: call data" }));
      addMethod(new FuncTabItem("getNrOfSpecies", getNrOfSpecies));
      _scriptHelpTab.Add(new HelpTabItem("getNrOfSpecies", "get number of auto detected species for spec. file in open project",
                      new List<string> { "1: file index (0..n)" }, new List<string> { "1: nr of species in recording" }));
      addMethod(new FuncTabItem("getRankSpecies", getRankSpecies));
      _scriptHelpTab.Add(new HelpTabItem("getRankSpecies", "get the species name for the specified rank in specified file in open project",
                      new List<string> { "1: file index (0..n)","2: rank (1..m)" }, new List<string> { "1: nr of species in recording" }));
      addMethod(new FuncTabItem("getRankCount", getRankCount));
      _scriptHelpTab.Add(new HelpTabItem("getRankCount", "get the number of calls of a species for the specified rank in specified file in open project",
                      new List<string> { "1: file index (0..n)", "2: rank (1..m)" }, new List<string> { "1: nr of species in recording" }));
    }

    static tParseError getPrjFileCount(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if ((_inst._model.Prj != null) && (_inst._model.Prj.Ok))
      {
        int nr = _inst._model.Prj.Records.Length;
        result.assignInt64(nr);
      }
      else
        err = tParseError.ARG1_OUT_OF_RANGE;
      return err;
    }

    enum enFileInfo
    {
      SAMPLE_RATE,
      DURATION,
      SELECT,
      NAME
    }

    static tParseError setFileInfo(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if ((_inst._model.Prj != null) && (_inst._model.Prj.Ok))
      {
        if (argv.Count >= 3)
        {
          argv[0].changeType(AnyType.tType.RT_UINT64);
          argv[1].changeType(AnyType.tType.RT_STR);
          int idxF = (int)argv[0].getUint64();
          int maxIdxF = _inst._model.Analysis.Files.Count;
          if (idxF < maxIdxF)
          {
            enFileInfo fileInfo;
            bool ok = Enum.TryParse(argv[1].getString(), out fileInfo);
            if (ok)
            {
              switch (fileInfo)
              {
                  case enFileInfo.SELECT:
                    argv[2].changeType(AnyType.tType.RT_BOOL);
                    _inst._model.Analysis.Files[idxF].Selected = argv[2].getBool();
                    break;
                  case enFileInfo.SAMPLE_RATE:
                    break;
              }
            }
            else
              err = tParseError.ARG2_OUT_OF_RANGE;
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
        else
          err = tParseError.NR_OF_ARGUMENTS;
      }
      else
        err = tParseError.ARG1_OUT_OF_RANGE;
      return err;
    }

    static tParseError getFileName(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if ((_inst._model.Prj != null) && (_inst._model.Prj.Ok))
      {
        if (argv.Count >= 1)
        {
          argv[0].changeType(AnyType.tType.RT_UINT64);
          int idxF = (int)argv[0].getUint64();
          int maxIdxF = _inst._model.Prj.Records.Length;
          if (idxF < maxIdxF)
          {
            result.assign(_inst._model.PrjPath + _inst._model.Prj.WavSubDir + 
                          _inst._model.Prj.Records[idxF].File);
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
        else
          err = tParseError.NR_OF_ARGUMENTS;
      }
      else
        err = tParseError.ARG1_OUT_OF_RANGE;
      return err;
    }


    static tParseError getFileInfo(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if ((_inst._model.Prj != null) && (_inst._model.Prj.Ok))
      {
        if (argv.Count >= 2)
        {
          argv[0].changeType(AnyType.tType.RT_UINT64);
          argv[1].changeType(AnyType.tType.RT_STR);
          int idxF = (int)argv[0].getUint64();
          int maxIdxF = _inst._model.Analysis.Files.Count;
          if (idxF < maxIdxF)
          {
            enFileInfo fileInfo;
            bool ok = Enum.TryParse(argv[1].getString(), out fileInfo);
            if (ok)
            {
              switch (fileInfo)
              {
                case enFileInfo.NAME:
                  result.assign(_inst._model.Analysis.Files[idxF].FileName);
                  break;
                case enFileInfo.SAMPLE_RATE:
                  result.assignInt64(_inst._model.Analysis.Files[idxF].SampleRate);
                  break;
                case enFileInfo.DURATION:
                  result.assign(_inst._model.Analysis.Files[idxF].Duration);
                  break;
                default:
                  result.assign("ERROR: supported data type: " + argv[1].getString());
                  break;
              }
            }
            else
              err = tParseError.ARG2_OUT_OF_RANGE;
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
        else
          err = tParseError.NR_OF_ARGUMENTS;
      }
      else
        err = tParseError.ARG1_OUT_OF_RANGE;
      return err;
    }


    static tParseError getCallCount(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if ((_inst._model.Prj != null) && (_inst._model.Prj.Ok))
      {
        if (argv.Count >= 1)
        {
          int maxIdx = _inst._model.Analysis.Files.Count;
          argv[0].changeType(AnyType.tType.RT_UINT64);
          int idx = (int)argv[0].getUint64();
          if (idx < maxIdx)
          {
            int nr = _inst._model.Analysis.Files[idx].Calls.Count;
            result.assignInt64(nr);
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
        else
          err = tParseError.NR_OF_ARGUMENTS;
      }
      else
        err = tParseError.ARG1_OUT_OF_RANGE;
      return err;
    }

    static tParseError getRankSpecies(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if ((_inst._model.Prj != null) && (_inst._model.Prj.Ok))
      {
        if (argv.Count >= 1)
        {
          int maxIdx = _inst._model.Analysis.Files.Count;
          argv[0].changeType(AnyType.tType.RT_UINT64);
          argv[1].changeType(AnyType.tType.RT_UINT64);
          int idx = (int)argv[0].getUint64();
          int rank = (int)argv[1].getUint64();
          if (idx < maxIdx)
          {
            int cnt = _inst._model.Analysis.Files[idx].getNrOfAutoSpecies();
            if ((rank > 0) && (rank <= cnt))
            {
              KeyValuePair<string, int> spec = _inst._model.Analysis.Files[idx].getSpecies(rank);
              result.assign(spec.Key.ToUpper());
            }
            else
              err = tParseError.ARG2_OUT_OF_RANGE;
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
        else
          err = tParseError.NR_OF_ARGUMENTS;
      }
      else
        err = tParseError.ARG1_OUT_OF_RANGE;
      return err;
    }

    static tParseError getRankCount(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if ((_inst._model.Prj != null) && (_inst._model.Prj.Ok))
      {
        if (argv.Count >= 1)
        {
          int maxIdx = _inst._model.Analysis.Files.Count;
          argv[0].changeType(AnyType.tType.RT_UINT64);
          argv[1].changeType(AnyType.tType.RT_UINT64);
          int idx = (int)argv[0].getUint64();
          int rank = (int)argv[1].getUint64();
          if (idx < maxIdx)
          {
            int cnt = _inst._model.Analysis.Files[idx].getNrOfAutoSpecies();
            if ((rank > 0) && (rank <= cnt))
            {
              KeyValuePair<string, int> spec = _inst._model.Analysis.Files[idx].getSpecies(rank);
              result.assignInt64(spec.Value);
            }
            else
              err = tParseError.ARG2_OUT_OF_RANGE;
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
        else
          err = tParseError.NR_OF_ARGUMENTS;
      }
      else
        err = tParseError.ARG1_OUT_OF_RANGE;
      return err;
    }

    static tParseError getNrOfSpecies(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if ((_inst._model.Prj != null) && (_inst._model.Prj.Ok))
      {
        if (argv.Count >= 1)
        {
          int maxIdx = _inst._model.Analysis.Files.Count;
          argv[0].changeType(AnyType.tType.RT_UINT64);
          int idx = (int)argv[0].getUint64();
          if (idx < maxIdx)
          {
            int nr = _inst._model.Analysis.Files[idx].getNrOfAutoSpecies();
            result.assignInt64(nr);
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
        else
          err = tParseError.NR_OF_ARGUMENTS;
      }
      else
        err = tParseError.ARG1_OUT_OF_RANGE;
      return err;
    }


    enum enCallInfo
    {
      SPEC_AUTO,
      SPEC_MAN
    }

    static tParseError getCallInfo(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if ((_inst._model.Prj != null) && (_inst._model.Prj.Ok))
      {
        if (argv.Count >= 3)
        {
          argv[0].changeType(AnyType.tType.RT_UINT64);
          argv[1].changeType(AnyType.tType.RT_UINT64);
          argv[2].changeType(AnyType.tType.RT_STR);
          int idxF = (int)argv[0].getUint64();
          int idxC = (int)argv[1].getUint64();
          int maxIdxF = _inst._model.Analysis.Files.Count;
          if(idxF < maxIdxF)
          { 
            int maxIdxC = _inst._model.Analysis.Files[idxF].Calls.Count;
            if (idxC < maxIdxC)
            {
              enCallInfo callInfo;
              bool ok = Enum.TryParse(argv[2].getString(), out callInfo);
              if (ok)
              {
                switch (callInfo)
                {
                  case enCallInfo.SPEC_AUTO:
                    result.assign(_inst._model.Analysis.Files[idxF].Calls[idxC].SpeciesAuto.ToUpper());
                    break;
                  case enCallInfo.SPEC_MAN:
                    result.assign(_inst._model.Analysis.Files[idxF].Calls[idxC].SpeciesMan.ToUpper());
                    break;
                }
              }
              else
                err = tParseError.ARG3_OUT_OF_RANGE;
            }
            else
              err = tParseError.ARG2_OUT_OF_RANGE;
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
        else
          err = tParseError.NR_OF_ARGUMENTS;
      }
      else
        err = tParseError.ARG1_OUT_OF_RANGE;
      return err;
    }

    static tParseError setCallInfo(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if ((_inst._model.Prj != null) && (_inst._model.Prj.Ok))
      {
        if (argv.Count >= 4)
        {
          argv[0].changeType(AnyType.tType.RT_UINT64);
          argv[1].changeType(AnyType.tType.RT_UINT64);
          argv[2].changeType(AnyType.tType.RT_STR);
          int idxF = (int)argv[0].getUint64();
          int idxC = (int)argv[1].getUint64();
          int maxIdxF = _inst._model.Analysis.Files.Count;
          if (idxF < maxIdxF)
          {
            int maxIdxC = _inst._model.Analysis.Files[idxF].Calls.Count;
            if (idxC < maxIdxC)
            {
              enCallInfo callInfo;
              bool ok = Enum.TryParse(argv[2].getString(), out callInfo);
              if (ok)
              {
                switch (callInfo)
                {
                  case enCallInfo.SPEC_AUTO:
                    argv[3].changeType(AnyType.tType.RT_STR);
                    _inst._model.Analysis.Files[idxF].Calls[idxC].SpeciesAuto = argv[3].getString();
                    break;
                  case enCallInfo.SPEC_MAN:
                    argv[3].changeType(AnyType.tType.RT_STR);
                    _inst._model.Analysis.Files[idxF].Calls[idxC].SpeciesMan = argv[3].getString();
                    break;
                }
              }
              else
                err = tParseError.ARG3_OUT_OF_RANGE;
            }
            else
              err = tParseError.ARG2_OUT_OF_RANGE;
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
        else
          err = tParseError.NR_OF_ARGUMENTS;
      }
      else
        err = tParseError.ARG1_OUT_OF_RANGE;
      return err;
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
