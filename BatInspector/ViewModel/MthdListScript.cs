/********************************************************************************
*               Author: Christian Müller
*     Date of creation: 2021-08-10                                       
*   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
*
*              Licence:  CC BY-NC 4.0 
********************************************************************************/

using libParser;
using libScripter;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using NAudio.MediaFoundation;
using System.Globalization;

namespace BatInspector
{
  public class MthdListScript : MethodList
  {
    static MthdListScript _inst;
    List<Csv> _listCsv;
    List<HelpTabItem> _scriptHelpTab = new List<HelpTabItem>();
    ViewModel _model;
    string _wrkDir;
    List<string> _files = null;
    List<string> _subDirs = null;
    SoundEdit _audio = null;
    int _lastFileIdx = -1;

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
      addMethod(new FuncTabItem("bandPass", bandPass));
      _scriptHelpTab.Add(new HelpTabItem("bandPass", "apply a band pass to the specified file",
                      new List<string> { "1: fileName or file index", "2:min freq [Hz]", "3:nax freq [Hz]" }, new List<string> { "" }));
      addMethod(new FuncTabItem("setSampleRate", setSampleRate));
      _scriptHelpTab.Add(new HelpTabItem("setSampleRate", "sets the samplerate of a file",
                      new List<string> { "1: fileName","2:sample rate" }, new List<string> { "" }));
      addMethod(new FuncTabItem("getSampleRate", getSampleRate)); 
      _scriptHelpTab.Add(new HelpTabItem("getSampleRate", "returns the samplerate of a file",
                      new List<string> { "1: fileName" }, new List<string> { "1: sampling rate" }));
      addMethod(new FuncTabItem("reSampleWav", resampleWav));
      _scriptHelpTab.Add(new HelpTabItem("reSampleWav", "resamples a WAV file to a new sampleRate",
                      new List<string> { "1: fileName", "2:new sampling rate"}, new List<string> { "1: sampling rate" }));
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
      addMethod(new FuncTabItem("getFileIndex", getFileIndex));
      _scriptHelpTab.Add(new HelpTabItem("getFileIndex", "get file index in opended project",
                      new List<string> { "1: file name"}, new List<string> { "1: index (-1: not found)" }));
      addMethod(new FuncTabItem("occursAtLocation", occursAtLocation));
      _scriptHelpTab.Add(new HelpTabItem("occursAtLocation", "check wether speicies occurs at spcified loction",
                      new List<string> {"1: species abbrviation, 2: latitude, 3: longitude"},
                      new List<string> { "1: boolean if occurs or not"}));
      addMethod(new FuncTabItem("getRegion", getRegion));
      _scriptHelpTab.Add(new HelpTabItem("getRegion", "get name of region",
                      new List<string> { "1: latitude, 2: longitude" },
                      new List<string> { "1: name of detected region" }));
      addMethod(new FuncTabItem("createPrjFromFiles", createPrjFromFiles));
      _scriptHelpTab.Add(new HelpTabItem("createPrjFromFiles", "create a project from a list of WAV files",
                      new List<string> { "1: project name","2:source folder containing project","3:destination folder","4: max files per project (int)",
                                         "5: max length of WAV file [s] (float)" , "6: latitude (float)", "7: longitude (float)",
                                         "8: information about landscape", "9: information about weather"}, 
                      new List<string> { "1: list of opened projects" }));
      addMethod(new FuncTabItem("importPrj", importPrj));
      _scriptHelpTab.Add(new HelpTabItem("importPrj", "import an Elekon or BatSpy project",
                      new List<string> { "1:source folder containing project","2:destination folder","3: max files per project (int)",
                                         "4: max length of WAV file [s] (float)" },
                      new List<string> { "1: 0" }));
      addMethod(new FuncTabItem("openPrj", openPrj));
      _scriptHelpTab.Add(new HelpTabItem("openPrj", "open a project",
                      new List<string> { "1: project dir", "2: project name" }, new List<string> { "1: result" }));
      addMethod(new FuncTabItem("inspectPrj", inspectPrj));
      _scriptHelpTab.Add(new HelpTabItem("inspectPrj", "start inspecting currently opened project",
                      new List<string> {  }, new List<string> { "1: result" }));
      addMethod(new FuncTabItem("savePrj", savePrj));
      _scriptHelpTab.Add(new HelpTabItem("savePrj", "save the currently opened project",
                      new List<string> { }, new List<string> { "1: result" }));
      addMethod(new FuncTabItem("cleanup", cleanup));
      _scriptHelpTab.Add(new HelpTabItem("cleanup", "remove files not needed permanently from disk",
                      new List<string> { "1: root directory to start cleanup (string)", "2: delWavs (bool)", "3: delLogs (bool)", "4: delPngs (bool)" }, new List<string> { "1: result" }));
      addMethod(new FuncTabItem("reloadPrj", reloadPrj));
      _scriptHelpTab.Add(new HelpTabItem("reloaPrj", "reload project",
                      new List<string> { }, new List<string> { "1: result" }));
      addMethod(new FuncTabItem("getPrjInfo", getPrjInfo));
      _scriptHelpTab.Add(new HelpTabItem("getPrjInfo", "get project info",
                      new List<string> { "1: type of info"}, new List<string> { "1: result" }));
      addMethod(new FuncTabItem("countDirs", countDirs));
      _scriptHelpTab.Add(new HelpTabItem("countDirs", "count sub directories in a directory",
                      new List<string> { "1: path", "2: filter" }, new List<string> { "1: nr of sub directories" }));
      addMethod(new FuncTabItem("countFiles", countFiles));
      _scriptHelpTab.Add(new HelpTabItem("countFiles", "count files in a directory",
                      new List<string> { "1: path", "2: filter" }, new List<string> { "1: nr of files" }));
      addMethod(new FuncTabItem("getDir", getDir));
      _scriptHelpTab.Add(new HelpTabItem("getDir", "get file from previously read dir info",
                      new List<string> { "1: index", "2: 0=full path, 1:name without path" }, new List<string> { "1: full directory name" }));
      addMethod(new FuncTabItem("getFile", getFile));
      _scriptHelpTab.Add(new HelpTabItem("getFile", "get file from previously read dir info",
                      new List<string> { "1: index", "2: 0=full path, 1:name without path" }, new List<string> { "1: full file name" }));
      addMethod(new FuncTabItem("mkDir", mkDir));
      _scriptHelpTab.Add(new HelpTabItem("mkDir", "create directory",
                      new List<string> { "1: path" }, new List<string> { "1: error code" }));
      addMethod(new FuncTabItem("checkOverdrive", checkOverdrive));
      _scriptHelpTab.Add(new HelpTabItem("checkOverdrive", "check if a given call is overdriven",
                      new List<string> { "1: file index", "2:call index" }, new List<string> { "1: TRUE if call is overdriven" }));
      addMethod(new FuncTabItem("setTxtLocfilePars", setTxtLocfilePars));
      _scriptHelpTab.Add(new HelpTabItem("setTxtLocfilePars", "set parameters for location import from txt files",
                      new List<string> { "1: ColFilename", "2: ColLatitude","3: ColLongititude", "4: Mode (0=FileName, 1=TimeStamp)",
                                         "5: ColNS", "6: ColWE","7: ColDate", "8: ColTime", "9: Delimiter"},
                                         new List<string> { "1: error code" }));
      addMethod(new FuncTabItem("calcSNR", calcSNR));
      _scriptHelpTab.Add(new HelpTabItem("calcSNR", "calculate sound noise ratio for each call in file",
                      new List<string> { "1: file name"}, new List<string> { "1: error code" }));


    }

    static tParseError setTxtLocfilePars(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 9)
      {
        argv[0].changeType(AnyType.tType.RT_INT64);
        int colFile = (int)argv[0].getInt64();
        argv[1].changeType(AnyType.tType.RT_INT64);
        int colLat = (int)argv[1].getInt64();
        argv[2].changeType(AnyType.tType.RT_INT64);
        int colLon = (int)argv[2].getInt64();
        argv[3].changeType(AnyType.tType.RT_INT64);
        enLocFileMode mode = (enLocFileMode)argv[3].getInt64();
        argv[4].changeType(AnyType.tType.RT_INT64);
        int colNS = (int)argv[4].getInt64();
        argv[5].changeType(AnyType.tType.RT_INT64);
        int colWE = (int)argv[5].getInt64();
        argv[6].changeType(AnyType.tType.RT_INT64);
        int colDate = (int)argv[6].getInt64();
        argv[7].changeType(AnyType.tType.RT_INT64);
        int colTime = (int)argv[7].getInt64();
        argv[8].changeType(AnyType.tType.RT_STR);
        char delim = argv[8].getString()[0];
        LocFileSettings loc = new LocFileSettings(mode, colFile, colNS, colLat, colWE, colLon, colTime, colDate, delim);
        AppParams.Inst.LocFileSettings = loc;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      result.assignInt64((long)err);
      return err;
    }

    static tParseError openPrj(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 1)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        try
        {
          DirectoryInfo dir = new DirectoryInfo(argv[0].getString());
          _inst._model.initProject(dir, null);
        }
        catch
        {
          DebugLog.log("openPrj: Error opening project: " + argv[0], enLogType.ERROR);
          err = tParseError.RESSOURCE;
        }
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      result.assign((Int64)err);
      return err;
    }

    static tParseError createPrjFromFiles(List<AnyType> argv, out AnyType result)
    {
      tParseError err = tParseError.SUCCESS;
      result = new AnyType();
      PrjInfo info = null;
      if (argv.Count >= 9)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        argv[1].changeType(AnyType.tType.RT_STR);
        argv[2].changeType(AnyType.tType.RT_STR);
        argv[3].changeType(AnyType.tType.RT_INT64);
        argv[4].changeType(AnyType.tType.RT_FLOAT);
        argv[5].changeType(AnyType.tType.RT_FLOAT);
        argv[6].changeType(AnyType.tType.RT_FLOAT);
        argv[7].changeType(AnyType.tType.RT_BOOL);
        argv[8].changeType(AnyType.tType.RT_BOOL);
        info = new PrjInfo
        {
          Name = argv[0].getString(),
          SrcDir = argv[1].getString(),
          DstDir = argv[2].getString(),
          MaxFileCnt = (int)argv[3].getInt64(),
          MaxFileLenSec = (int)argv[4].getFloat(),
          Latitude = argv[5].getFloat(),
          Longitude = argv[6].getFloat(),
          GpxFile = "",
          Notes = "",
          StartTime = new DateTime(1, 1, 1),
          EndTime = new DateTime(2099, 12, 31),
          OverwriteLocation = true,
          WavSubDir = argv[8].getBool() ? AppParams.DIR_WAVS : "",
          
        };
      }
      else if (argv.Count == 8)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        argv[1].changeType(AnyType.tType.RT_STR);
        argv[2].changeType(AnyType.tType.RT_STR);
        argv[3].changeType(AnyType.tType.RT_INT64);
        argv[4].changeType(AnyType.tType.RT_FLOAT);
        argv[5].changeType(AnyType.tType.RT_STR);
        argv[6].changeType(AnyType.tType.RT_BOOL);
        argv[7].changeType(AnyType.tType.RT_BOOL);
        string locFile = argv[5].getString();
        info = new PrjInfo
        {
          Name = argv[0].getString(),
          SrcDir = argv[1].getString(),
          DstDir = argv[2].getString(),
          MaxFileCnt = (int)argv[3].getInt64(),
          MaxFileLenSec = (int)argv[4].getFloat(),
          Latitude = 0,
          Longitude = 0,
          GpxFile = locFile,
          Notes = "",
          RemoveSource = argv[6].getBool(),
          StartTime = new DateTime(1, 1, 1),
          EndTime = new DateTime(2099, 12, 31),
          OverwriteLocation = true,
          WavSubDir = argv[7].getBool() ? AppParams.DIR_WAVS : "",
        };
        if (locFile.IndexOf(AppParams.EXT_GPX) >= 0)
          info.LocSourceGpx = true;
        else if(locFile.IndexOf(AppParams.EXT_KML) >= 0)
          info.LocSourceKml = true;
        else if (locFile.IndexOf(AppParams.EXT_TXT) >= 0)
          info.LocSourceTxt = true;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      if (err == tParseError.SUCCESS)
      {
        ModelParams modelParams = _inst._model.DefaultModelParams[_inst._model.getModelIndex(AppParams.Inst.DefaultModel)];

        Project.createPrjFromWavs(info, _inst._model.Regions, _inst._model.SpeciesInfos,
                                  modelParams, _inst._model.DefaultModelParams);
      }
      result.assignInt64((long)err);
      return err;
    }

    static tParseError importPrj(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 4)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        argv[1].changeType(AnyType.tType.RT_STR);
        argv[2].changeType(AnyType.tType.RT_INT64);
        argv[3].changeType(AnyType.tType.RT_FLOAT);
        PrjInfo info = new PrjInfo
        {
          Name = Path.GetFileNameWithoutExtension(argv[0].getString()),
          SrcDir = argv[0].getString(),
          DstDir = argv[1].getString(),
          MaxFileCnt = (int)argv[2].getInt64(),
          MaxFileLenSec = (int)argv[3].getFloat(),
          Latitude = 0.0,
          Longitude = 0.0,
          GpxFile = "",
          Notes = "",
          IsProjectFolder = true,
          LocSourceGpx = false,
          LocSourceKml = false,
          LocSourceTxt = false,
          OverwriteLocation = false,
          RemoveSource = false,
        };
        ModelParams modPars = _inst._model.DefaultModelParams[_inst._model.getModelIndex(AppParams.Inst.DefaultModel)];
        if (argv.Count > 4)
        {
          modPars = _inst._model.DefaultModelParams[_inst._model.getModelIndex(argv[4].getString())];
          if (argv.Count > 5)
          {
            string dataSet = argv[5].getString();
            bool found = false;
            foreach (string s in modPars.AvailableDataSets)
            {
              if (s == dataSet)
                found = true;
            }
            if (found)
              modPars.DataSet = dataSet;
            else
              DebugLog.log($"Model '{dataSet}' not found!", enLogType.ERROR);
          }
        }
        else
          DebugLog.log("import project with default model: " + AppParams.Inst.DefaultModel, enLogType.INFO);
        if (File.Exists(Path.Combine(info.SrcDir, info.Name + AppParams.EXT_PRJ)) || 
            File.Exists(Path.Combine(info.SrcDir, info.Name + AppParams.EXT_BATSPY)))
        {
          _inst._model.createProject(info, true, true);
        }
        else
          err = tParseError.RESSOURCE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      result.assignInt64((Int64)err);
      return err;
    }

    static tParseError inspectPrj(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if(_inst._model.Prj.Name != "")
      {
        int e = _inst._model.evaluate(true);
        if (e == 0)
          _inst._model.Prj.writePrjFile();
        else
          err = tParseError.RESSOURCE;
      }
      result.assign((Int64)err);
      return err;
    }

    static tParseError savePrj(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (_inst._model.Prj.Name != "")
      {
        _inst._model.Prj.writePrjFile();
        _inst._model.Prj.Analysis.save(_inst._model.Prj.ReportName, _inst._model.Prj.Notes, _inst._model.Prj.SummaryName);
      }
      return err;
    }


    static tParseError getFileIndex(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 1)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        if (_inst._model.Prj.Analysis != null)
        {
          string name = argv[0].getString();
          int index = _inst._model.Prj.Analysis.getIndex(name);
          result.assignInt64(index);
        }
        else
          err = tParseError.ARG1_OUT_OF_RANGE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }

    static tParseError mkDir(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 1)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        string path = argv[0].getString();
        try
        {
          if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        }
        catch
        {
          err = tParseError.ARG1_OUT_OF_RANGE;
        }
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      result.assignInt64((Int64)err);
      return err;
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
      NAME,
      LATITUDE,
      LONGITUDE
    }

    enum enPrjInfo
    {
      ROOT,
      OK,
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
          int maxIdxF = _inst._model.Prj.Analysis.Files.Count;
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
                    _inst._model.Prj.Records[idxF].Selected = argv[2].getBool();
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
            result.assign(_inst._model.SelectedDir + "/" + _inst._model.Prj.WavSubDir + "/" +
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


    static tParseError getPrjInfo(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if ((_inst._model.Prj != null) && (_inst._model.Prj.Ok))
      {
        if (argv.Count >= 1)
        {
          argv[0].changeType(AnyType.tType.RT_STR);
          enPrjInfo prjInfo;
          bool ok = Enum.TryParse(argv[0].getString(), out prjInfo);
          if (ok)
          {
             switch(prjInfo)
             {
              case enPrjInfo.ROOT:
                result.assign(_inst._model.Prj.PrjDir);
                break;

              case enPrjInfo.OK:
                result.assignBool(_inst._model.Prj.Ok);
                break;
            }
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
        else
          err = tParseError.NR_OF_ARGUMENTS;
      }
      else
        err = tParseError.RESSOURCE;
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
          int maxIdxF = _inst._model.Prj.Analysis.Files.Count;
          if (idxF < maxIdxF)
          {
            enFileInfo fileInfo;
            bool ok = Enum.TryParse(argv[1].getString(), out fileInfo);
            if (ok)
            {
              switch (fileInfo)
              {
                case enFileInfo.NAME:
                  result.assign(_inst._model.Prj.Analysis.Files[idxF].Name);
                  break;
                case enFileInfo.SAMPLE_RATE:
                  result.assignInt64(_inst._model.Prj.Analysis.Files[idxF].getInt(Cols.SAMPLERATE));
                  break;
                case enFileInfo.DURATION:
                  result.assign(_inst._model.Prj.Analysis.Files[idxF].getDouble(Cols.DURATION));
                  break;
                case enFileInfo.SELECT:
                 result.assignBool(_inst._model.Prj.Records[idxF].Selected);
                  break;
                case enFileInfo.LATITUDE:
                  result.assign(_inst._model.Prj.Analysis.Files[idxF].getDouble(Cols.LAT));
                  break;
                case enFileInfo.LONGITUDE:
                  result.assign(_inst._model.Prj.Analysis.Files[idxF].getDouble(Cols.LON));
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

    static tParseError bandPass(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 3)
      {
        argv[1].changeType(AnyType.tType.RT_FLOAT);
        argv[2].changeType(AnyType.tType.RT_FLOAT);
        string fName = "";
        if ((_inst._model.Prj != null) && (_inst._model.Prj.Ok))
        {
          if (argv[0].getType() == AnyType.tType.RT_STR)
            fName = argv[0].getString();
          else
          {
            argv[0].changeType(AnyType.tType.RT_INT64);
            int idx = (int)argv[0].getInt64();
            if (_inst._model.Prj.Records.Length > idx)
              fName = Path.Combine(_inst._model.Prj.PrjDir, _inst._model.Prj.WavSubDir, _inst._model.Prj.Records[idx].File);
            else
              err = tParseError.ARG1_OUT_OF_RANGE;
          }
        }
        else
          err = tParseError.RESSOURCE;
        if (err == 0)
        {
          double fMin = argv[1].getFloat();
          double fMax = argv[2].getFloat();
          SoundEdit edit = new SoundEdit();
          edit.readWav(fName);
          edit.FftForward();
          edit.bandpass(fMin, fMax);
          edit.FftBackward();
          edit.saveAs(fName, _inst._model.Prj.WavSubDir);
        }
      }
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
          int maxIdx = _inst._model.Prj.Analysis.Files.Count;
          argv[0].changeType(AnyType.tType.RT_UINT64);
          int idx = (int)argv[0].getUint64();
          if (idx < maxIdx)
          {
            int nr = _inst._model.Prj.Analysis.Files[idx].Calls.Count;
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
          int maxIdx = _inst._model.Prj.Analysis.Files.Count;
          argv[0].changeType(AnyType.tType.RT_UINT64);
          argv[1].changeType(AnyType.tType.RT_UINT64);
          int idx = (int)argv[0].getUint64();
          int rank = (int)argv[1].getUint64();
          if (idx < maxIdx)
          {
            int cnt = _inst._model.Prj.Analysis.Files[idx].getNrOfAutoSpecies();
            if ((rank > 0) && (rank <= cnt))
            {
              KeyValuePair<string, int> spec = _inst._model.Prj.Analysis.Files[idx].getSpecies(rank);
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
          int maxIdx = _inst._model.Prj.Analysis.Files.Count;
          argv[0].changeType(AnyType.tType.RT_UINT64);
          argv[1].changeType(AnyType.tType.RT_UINT64);
          int idx = (int)argv[0].getUint64();
          int rank = (int)argv[1].getUint64();
          if (idx < maxIdx)
          {
            int cnt = _inst._model.Prj.Analysis.Files[idx].getNrOfAutoSpecies();
            if ((rank > 0) && (rank <= cnt))
            {
              KeyValuePair<string, int> spec = _inst._model.Prj.Analysis.Files[idx].getSpecies(rank);
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
          int maxIdx = _inst._model.Prj.Analysis.Files.Count;
          argv[0].changeType(AnyType.tType.RT_UINT64);
          int idx = (int)argv[0].getUint64();
          if (idx < maxIdx)
          {
            int nr = _inst._model.Prj.Analysis.Files[idx].getNrOfAutoSpecies();
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
      SPEC_MAN,
      PROB_RATIO,
      PROBABILITY,
      F_MIN,
      F_MAX,
      F_MAX_AMP,
      DURATION
    }

    static tParseError getCallInfo(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if ((_inst._model.Prj != null) && (_inst._model.Prj.Ok) && (_inst._model.Prj.Analysis.Files.Count > 0))
      {
        if (argv.Count >= 3)
        {
          argv[0].changeType(AnyType.tType.RT_UINT64);
          argv[1].changeType(AnyType.tType.RT_UINT64);
          argv[2].changeType(AnyType.tType.RT_STR);
          int idxF = (int)argv[0].getUint64();
          int idxC = (int)argv[1].getUint64();
          int maxIdxF = _inst._model.Prj.Analysis.Files.Count;
          if(idxF < maxIdxF)
          { 
            int maxIdxC = _inst._model.Prj.Analysis.Files[idxF].Calls.Count;
            if (idxC < maxIdxC)
            {
              enCallInfo callInfo;
              bool ok = Enum.TryParse(argv[2].getString(), out callInfo);
              if (ok)
              {
                switch (callInfo)
                {
                  case enCallInfo.SPEC_AUTO:
                    result.assign(_inst._model.Prj.Analysis.Files[idxF].Calls[idxC].getString(Cols.SPECIES));
                    break;
                  case enCallInfo.SPEC_MAN:
                    result.assign(_inst._model.Prj.Analysis.Files[idxF].Calls[idxC].getString(Cols.SPECIES_MAN));
                    break;
                  case enCallInfo.PROB_RATIO:
                    result.assign(_inst._model.Prj.Analysis.Files[idxF].Calls[idxC].FirstToSecond);
                    break;
                  case enCallInfo.PROBABILITY:
                    result.assign(_inst._model.Prj.Analysis.Files[idxF].Calls[idxC].getDouble(Cols.PROBABILITY));
                    break;
                  case enCallInfo.F_MIN:
                    result.assign(_inst._model.Prj.Analysis.Files[idxF].Calls[idxC].getDouble(Cols.F_MIN));
                    break;
                  case enCallInfo.F_MAX:
                    result.assign(_inst._model.Prj.Analysis.Files[idxF].Calls[idxC].getDouble(Cols.F_MAX));
                    break;
                  case enCallInfo.F_MAX_AMP:
                    result.assign(_inst._model.Prj.Analysis.Files[idxF].Calls[idxC].getDouble(Cols.F_MAX_AMP));
                    break;
                  case enCallInfo.DURATION:
                    result.assign(_inst._model.Prj.Analysis.Files[idxF].Calls[idxC].getDouble(Cols.DURATION));
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
          int maxIdxF = _inst._model.Prj.Analysis.Files.Count;
          if (idxF < maxIdxF)
          {
            int maxIdxC = _inst._model.Prj.Analysis.Files[idxF].Calls.Count;
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
                    _inst._model.Prj.Analysis.Files[idxF].Calls[idxC].setString(Cols.SPECIES, argv[3].getString());
                    break;
                  case enCallInfo.SPEC_MAN:
                    argv[3].changeType(AnyType.tType.RT_STR);
                    _inst._model.Prj.Analysis.Files[idxF].Calls[idxC].setString(Cols.SPECIES_MAN, argv[3].getString());
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
          wav.FormatChunk.Frequency= (uint)sampleRate;
          wav.saveFile();
        }
        else
          err = tParseError.ARG1_OUT_OF_RANGE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }

    static tParseError resampleWav(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 2)
      {
        bool withBackup = true;
        argv[0].changeType(AnyType.tType.RT_STR);
        string fName = argv[0].getString();
        argv[1].changeType(AnyType.tType.RT_INT64);
        int sampleRate = (int)argv[1].getInt64();
        if(argv.Count == 3)
        {
          argv[2].changeType(AnyType.tType.RT_BOOL);
          withBackup = argv[2].getBool();
        }
        if (File.Exists(fName))
        {
          if (Path.GetExtension(fName).ToLower() == AppParams.EXT_WAV)
          {
            string bakName = fName.ToLower().Replace(AppParams.EXT_WAV, "_bak.wav");
            if ((_inst._model.Prj != null) && (_inst._model.Prj.Ok))
            {
              string fileName = Path.GetFileName(fName);
              bakName = Path.Combine(_inst._model.Prj.PrjDir, AppParams.DIR_ORIG, fileName);
              ZoomView.saveWavBackup(fName, _inst._model.Prj.WavSubDir);
              File.Delete(fName);
            }
            else
            {
              if(!File.Exists(bakName))
                File.Move(fName, bakName);
            }
            using (AudioFileReader reader = new AudioFileReader(bakName))
            {
              var resampler = new WdlResamplingSampleProvider(reader, sampleRate);
              WaveFileWriter.CreateWaveFile16(fName, resampler);
            }
            if(!withBackup)
              File.Delete(fName);
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
        else
          err = tParseError.ARG1_OUT_OF_RANGE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }

    /*
    static tParseError calcProbabilityRatios(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if((_inst._model.Prj != null) && (_inst._model.Prj.Analysis != null))
      {
        string speciesFile = 
        _inst._model.Prj.Analysis.calcProbabilityRatios(AppParams.Inst.SpeciesFile);
      }
      else
      {
        err = tParseError.ARG1_OUT_OF_RANGE;
        result.assign(1);
      }
      return err;
    }*/

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
          result.assignInt64((long)wav.FormatChunk.Frequency);
        }
        else
          err = tParseError.ARG1_OUT_OF_RANGE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }

    static tParseError cleanup(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count == 6)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        argv[1].changeType(AnyType.tType.RT_BOOL);
        argv[2].changeType(AnyType.tType.RT_BOOL);
        argv[3].changeType(AnyType.tType.RT_BOOL);
        argv[4].changeType(AnyType.tType.RT_BOOL);
        string root = argv[0].getString();
        bool delWavs = argv[1].getBool();
        bool logs = argv[2].getBool();
        bool pngs = argv[3].getBool();
        bool origs = argv[4].getBool();
        bool annotations = argv[5].getBool();
        _inst._model.cleanup(root, delWavs, logs, pngs, origs, annotations);
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }

    static tParseError countFiles(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 2)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        argv[1].changeType(AnyType.tType.RT_STR);
        string root = argv[0].getString();
        string filter = argv[1].getString();
        try
        {
          DirectoryInfo dir = new DirectoryInfo(root);
          FileInfo[] files = dir.GetFiles(filter);
          _inst._files = new List<string>();
          foreach (FileInfo f in files)
            _inst._files.Add(f.FullName);
          result.assign(_inst._files.Count);
        }
        catch (Exception ex)
        {
          DebugLog.log("Script function 'readDir': " + ex.ToString(), enLogType.ERROR);
          err = tParseError.RESSOURCE;
        }
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }

    static tParseError countDirs(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 2)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        argv[1].changeType(AnyType.tType.RT_STR);
        string root = argv[0].getString();
        string filter = argv[1].getString();
        try
        {
          DirectoryInfo dir = new DirectoryInfo(root);
          DirectoryInfo[] subDirs = dir.GetDirectories(filter);
          _inst._subDirs = new List<string>();
          foreach (DirectoryInfo f in subDirs)
            _inst._subDirs.Add(f.FullName);
          result.assign(_inst._subDirs.Count);
        }
        catch (Exception ex)
        {
          DebugLog.log("Script function 'countDirs': " + ex.ToString(), enLogType.ERROR);
          err = tParseError.RESSOURCE;
        }
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }


    static tParseError getFile(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 1)
      {
        argv[0].changeType(AnyType.tType.RT_INT64);
        int index = (int)argv[0].getInt64();
        bool fullPath = true;
        if (argv.Count >= 2)
        {
          argv[1].changeType(AnyType.tType.RT_BOOL);
          fullPath = !argv[1].getBool();
        }
        if (_inst._files != null)
        {
          if ((index >= 0) && (index < _inst._files.Count))
          {
            if(fullPath)
              result.assign(_inst._files[index]);
            else
              result.assign(Path.GetFileName(_inst._files[index]));
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
        else
          err = tParseError.RESSOURCE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }

    static tParseError getDir(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      if (argv.Count >= 1)
      {
        argv[0].changeType(AnyType.tType.RT_INT64);
        bool fullPath = true;
        if (argv.Count >= 2)
        {
          argv[1].changeType(AnyType.tType.RT_BOOL);
          fullPath = !argv[1].getBool();
        }
        int index = (int)argv[0].getInt64();
        if (_inst._subDirs != null)
        {
          if ((index >= 0) && (index < _inst._subDirs.Count))
          {
            if (fullPath)
              result.assign(_inst._subDirs[index]);
            else
              result.assign(Path.GetFileName(_inst._subDirs[index]));
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
        else
          err = tParseError.RESSOURCE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }

    static tParseError getRegion(List<AnyType> argv, out AnyType result)
    {
      result = new AnyType();
      tParseError err = tParseError.SUCCESS;
      if (argv.Count >= 2)
      { 
        argv[0].changeType(AnyType.tType.RT_FLOAT);
        double lat = argv[0].getFloat();
        if ((lat >= -90) && (lat <= 90))
        {
          argv[1].changeType(AnyType.tType.RT_FLOAT);
          double lon = argv[1].getFloat();
          if ((lon >= -180) && (lon <= 180))
          {
            ParRegion region = _inst._model.Regions.findRegion(lat, lon);
            if (region != null)
              result.assign(region.Name);
            else
              result.assign("unspecific");
          }
        else
          err = tParseError.ARG2_OUT_OF_RANGE;
        }
        else
          err = tParseError.ARG1_OUT_OF_RANGE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }

  static tParseError occursAtLocation(List<AnyType> argv, out AnyType result)
    {
      result = new AnyType();
      tParseError err = tParseError.SUCCESS;
      if (argv.Count >= 2)
      {
        argv[0].changeType(AnyType.tType.RT_STR);
        string spec = argv[0].getString();
        argv[1].changeType(AnyType.tType.RT_FLOAT);
        double lat = argv[1].getFloat();
        if ((lat >= -90) && (lat <= 90))
        {
          argv[2].changeType(AnyType.tType.RT_FLOAT);
          double lon = argv[2].getFloat();
          if ((lon >= -180) && (lon <= 180))
          {
            bool occurs = _inst._model.Regions.occursAtLocation(spec, lat, lon);
            result.assignBool(occurs);
          }
          else
            err = tParseError.ARG3_OUT_OF_RANGE;
        }
        else
          err = tParseError.ARG2_OUT_OF_RANGE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }

    static tParseError calcSNR(List<AnyType> argv, out AnyType result)
    {
      result = new AnyType();
      tParseError err = 0;
      if (argv.Count >= 1)
      {
        string fileName = argv[0].getString();
        if(_inst._model.Prj?.Ok == true)
        {
          AnalysisFile f = _inst._model.Prj.Analysis.find(fileName);
          if (f != null)
          {
            WavFile wav = new WavFile();
            int res = wav.readFile(Path.Combine(_inst._model.Prj.PrjDir, _inst._model.Prj.WavSubDir, fileName));
            if (res == 0)
            {
              foreach (AnalysisCall c in f.Calls)
              {
                double tStart = c.getDouble(Cols.START_TIME);
                double tEnd = tStart + c.getDouble(Cols.DURATION) / 1000;
                double snr = wav.calcSnr(tStart, tEnd);
                _inst._model.Prj.Analysis.Csv.setCell(c.ReportRow, Cols.SNR, snr.ToString(CultureInfo.InvariantCulture));
              }
              _inst._model.Prj.Analysis.Csv.save(false);
            }
            else
              err = tParseError.ARG1_OUT_OF_RANGE;
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }


    static tParseError checkOverdrive(List<AnyType> argv, out AnyType result)
    {
      result = new AnyType();
      tParseError err = 0;
      if (argv.Count >= 2)
      {
        if ((_inst._model.Prj != null) && (_inst._model.Prj.Ok) && (_inst._model.Prj.Analysis.Files.Count > 0))
        {
          argv[0].changeType(AnyType.tType.RT_UINT64);
          argv[1].changeType(AnyType.tType.RT_UINT64);
          int idxF = (int)argv[0].getUint64();
          int idxC = (int)argv[1].getUint64();
          int maxIdxF = _inst._model.Prj.Analysis.Files.Count;
          if (idxF < maxIdxF)
          {
            if ((_inst._audio == null) || (_inst._lastFileIdx != idxF))
            {
              _inst._audio = new SoundEdit();
              string file = _inst._model.Prj.Analysis.Files[idxF].getString(Cols.NAME);
              string fullPath = _inst._model.Prj.getFullFilePath(file);
              int ret = _inst._audio.readWav(fullPath);
              if (ret == 0)
              {
                double maxT = (double)_inst._audio.Samples.Length / _inst._audio.SamplingRate;
                _inst._audio.findOverdrive(0, maxT);
                _inst._lastFileIdx = idxF;
              }
              else
                err = tParseError.ARG1_OUT_OF_RANGE;
            }
            int maxIdxC = _inst._model.Prj.Analysis.Files[idxF].Calls.Count;
            if (idxC < maxIdxC)
            {
              AnalysisCall call = _inst._model.Prj.Analysis.Files[idxF].Calls[idxC];
              double tStart = call.getDouble(Cols.START_TIME) - 0.02;
              double tEnd = tStart + call.getDouble(Cols.DURATION) / 1000 + 0.02;
              int idxS = (int)(tStart * _inst._audio.SamplingRate);
              int idxE = (int)(tEnd * _inst._audio.SamplingRate);
              bool ovr = _inst._audio.isOverdrive(idxS, idxE);
              result.assignBool(ovr);
            }
            else
              err = tParseError.ARG2_OUT_OF_RANGE;
          }
          else
            err = tParseError.ARG1_OUT_OF_RANGE;
        }
        else
          err = tParseError.RESSOURCE;
      }
      else
        err = tParseError.NR_OF_ARGUMENTS;
      return err;
    }


    static tParseError reloadPrj(List<AnyType> argv, out AnyType result)
    {
      tParseError err = 0;
      result = new AnyType();
      _inst._model.Prj.ReloadInGui = true;
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
      return "\nlist of project related commands";
    }
  }
}
