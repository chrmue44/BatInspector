/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-07-02                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/


using BatInspector.Controls;
using libParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace BatInspector
{
  /// <summary>
  /// a class to handle queries covering multiple projects 
  /// </summary>
  public class Query : PrjBase
  {
    string _name;
    string _srcDir;
    string _destDir;
    string _expression;
    string _reportName = "";
    ViewModel _model = null;
    QueryFile _queryFile = null;
    List<PrjRecord> _records = null;
    int _cntCall;
    int _cntFile;

    /// <summary>
    /// name of the query
    /// </summary>
    public string Name { get { return _name; } }
    /// <summary>
    /// source directory to start the search. All sub directories will be browsed during the search
    /// </summary>
    public string SrcDir { get { return _srcDir; } }
    /// <summary>
    /// directory to store the result of the query
    /// </summary>
    public string DestDir { get { return _destDir; } }
    /// <summary>
    /// logic expression to apply to the data
    /// </summary>
    public string Expression { get { return _expression; } }

    public string ReportName { get {  return _reportName; } }
    public PrjRecord[] Records { get { return _queryFile.Records; } }

    public Query(string name, string srcDir, string dstDir, string query, List<SpeciesInfos> speciesInfo, BatSpeciesRegions regions) :
    base(speciesInfo, regions, null)
    {
      _name = name;
      _srcDir = srcDir; 
      _destDir = dstDir;  
      _expression = query;
      _records = new List<PrjRecord>();
    }

    public void evaluate(ViewModel model)
    {
      if (!Directory.Exists(_srcDir))
        DebugLog.log("invalid directory: " + _srcDir, enLogType.ERROR);
      else if (!Directory.Exists(_destDir))
        DebugLog.log("invalid directory: " + _destDir, enLogType.ERROR);
      else
      {
        try
        {
          DirectoryInfo dir = new DirectoryInfo(_srcDir);
          _model = model;
          _analysis = new Analysis(_model.SpeciesInfos, null);
          _cntCall = 0;
          _cntFile = 0;
          createQueryFile();
          bool ok = crawl(dir);
          if (ok)
            writeQueryFile();
          else
            DebugLog.log("Query aborted", enLogType.ERROR);
        }
        catch ( Exception ex )
        {
          DebugLog.log("Query failed: " + ex.ToString(), enLogType.ERROR);
        }
      }
    }


    /// <summary>
    /// checks wether a directory contains query files
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static bool isQuery(FileInfo file)
    {
      bool retVal = false;
      if (file.Extension == AppParams.EXT_QUERY)
        retVal = true;
      return retVal;
    }

    public override PrjRecord[] getRecords()
    {
      return _records.ToArray();
    }

    public override string getFullFilePath(string path)
    {
      return Path.Combine(_destDir, path);
    }

    /// <summary>
    /// find a file in the project
    /// </summary>
    /// <param name="fileName">name of the file (full path or just file name)</param>
    /// <returns>record containing the file information</returns>
    public PrjRecord find(string fileName)
    {
      PrjRecord retVal = null;
      foreach (PrjRecord r in Records)
      {
        if (fileName.ToLower().Contains(r.File.ToLower()))
        {
          retVal = r;
          break;
        }
      }
      return retVal;
    }


    private bool crawl(DirectoryInfo dir)
    {
      DirectoryInfo[] dirs = dir.GetDirectories();
      bool ok = true;
      foreach (DirectoryInfo d in dirs)
      {
        if (Project.containsProject(d) != "")
          ok = evaluatePrj(d);
        else
          ok = crawl(d);
        if (!ok)
          break;
      }
      return ok;
    }


    private void createQueryFile()
    {
      _queryFile = new QueryFile
      {
        Name = _name,
        Created = DateTime.Now.ToString(),
        Expression = _expression,
        SrcDir = Utils.relativePath(_destDir, _srcDir),
        ReportFile = _name + "_" + AppParams.PRJ_REPORT
      };
    }

    public static Query readQueryFile(string name, ViewModel model)
    {
      Query retVal;
      try
      {
        string dstDir = Path.GetDirectoryName(name);
        string queryName = Path.GetFileName(name);
        string xml = File.ReadAllText(name);
        var serializer = new XmlSerializer(typeof(QueryFile));
        TextReader reader = new StringReader(xml);
        QueryFile qFile = (QueryFile)serializer.Deserialize(reader);
        retVal = new Query(qFile.Name, qFile.SrcDir, dstDir, qFile.Expression, model.SpeciesInfos, model.Regions);
        foreach(PrjRecord rec in qFile.Records)
          retVal._records.Add(rec);
        retVal._queryFile = qFile;
        retVal._model = model;
        retVal._analysis = new Analysis(model.SpeciesInfos, null);
        string fullReportName = Path.Combine(dstDir, retVal._queryFile.ReportFile);
        retVal._analysis.read(fullReportName);
        retVal._reportName = fullReportName;
        retVal.initSpeciesList();

      }
      catch (Exception ex)
      {
        DebugLog.log("error reading query file: " + name + " :" + ex.ToString(), enLogType.ERROR);
        retVal = null;  
      }
      return retVal;
    }


    public void exportFiles(string outputDir, bool withXml = true, bool withPng = true)
    {
      int countWav = 0;
      if (Directory.Exists(outputDir)) 
      {
        DebugLog.log("start files export from Query " + _name, enLogType.INFO);
        foreach (PrjRecord rec in _records) 
        {
          if(rec.Selected)
          {
            string srcWavName = Path.Combine(_destDir, rec.File);
            string srcXmlName = srcWavName.ToLower().Replace("wav", "xml");
            string srcPngName = srcWavName.ToLower().Replace("wav", "png");
            string dstWavName = Path.Combine(outputDir, Path.GetFileName(rec.File));
            string dstXmlName = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(rec.File)) + ".xml";
            string dstPngName = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(rec.File)) + ".png";
            if (File.Exists(srcWavName))
            {
              File.Copy(srcWavName, dstWavName, true);
              countWav++;
            }
            if (withXml && File.Exists(srcXmlName))
              File.Copy(srcXmlName, dstXmlName, true);
            if (withPng && File.Exists(srcPngName))
              File.Copy(srcPngName, dstPngName, true);
          }
        }
        DebugLog.log(countWav.ToString() + " files exported from Query " + _name, enLogType.INFO);
      }
      else
        DebugLog.log("could not find target directory " + outputDir, enLogType.ERROR);
    }


    private void writeQueryFile() 
    {
      if (_queryFile != null)
      {
        _queryFile.Records = _records.ToArray();
        var serializer = new XmlSerializer(typeof(QueryFile));
        string name = Path.Combine(_destDir, _name) + AppParams.EXT_QUERY;
        TextWriter writer = new StreamWriter(name);
        serializer.Serialize(writer, _queryFile);
        writer.Close();
        DebugLog.log("query '" + name + "' saved", enLogType.INFO);
      }
      string reportName = Path.Combine(_destDir, _queryFile.ReportFile);
      _analysis.save(reportName, "sum query\nsum query");
      _reportName = reportName;
      DebugLog.log(_cntCall.ToString() + " calls in " + _cntFile.ToString() + " files found", enLogType.INFO);
    }


    private bool evaluatePrj(DirectoryInfo dir)
    {
      bool retVal = false;
      Project prj = new Project(_model.Regions, _model.SpeciesInfos, null);
      prj.readPrjFile(dir.FullName, _model.getDefaultModelParams());
      Analysis analysis = new Analysis(_model.SpeciesInfos, null);
      analysis.read(prj.ReportName);

      FilterItem filter = new FilterItem(-1, "query",
       _expression.Replace('\n', ' '), false);

      string lastFileName = "";
      if (analysis.Files.Count == 0)
      {
        MessageBoxResult res = MessageBox.Show(BatInspector.Properties.MyResources.QueryReportMissing, BatInspector.Properties.MyResources.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        retVal = res == MessageBoxResult.Yes;
      }
      else
      {
        DebugLog.log("evaluating project " + prj.Name, enLogType.INFO);
        foreach (AnalysisFile file in analysis.Files)
        {
          foreach (AnalysisCall call in file.Calls)
          {
            bool match = _model.Filter.apply(filter, call,
                          file.getString(Cols.REMARKS),
                          AnyType.getTimeString(file.RecTime), out retVal);
            if (!retVal)
            {
              DebugLog.log("error parsing query expression: " + _expression, enLogType.ERROR);
              break;
            }
            if (match)
            {
              _cntCall++;
              if (lastFileName != file.Name)
              {
                _cntFile++;
                PrjRecord rec = new PrjRecord();

                string absPath = Path.Combine(prj.PrjDir, prj.WavSubDir, file.Name);
                rec.File = Utils.relativePath(this._destDir, absPath);
         //       rec.Name = Path.GetFileNameWithoutExtension(file.Name);
                _records.Add(rec);
                file.Name = rec.File;
                lastFileName = file.Name;
              }
              List<string> row = call.getReportRow();
              row[0] = file.Name;
              _analysis.addCsvReportRow(row);
            }
          }
          if (!retVal)
            break;
        }
      }
      return retVal;
    }
  }
}
