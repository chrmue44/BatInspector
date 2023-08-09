/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2023-07-02                                       
 *   Copyright (C) 2023: christian Müller chrmue44(at)gmail(dot)com
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
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
    ViewModel _model = null;
    QueryFile _queryFile = null;
    List<BatExplorerProjectFileRecordsRecord> _records = null;
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

    public Analysis Analysis { get{ return _analysis; } }

    public BatExplorerProjectFileRecordsRecord[] Records { get { return _queryFile.Records; } }

    public Query(string name, string srcDir, string dstDir, string query, List<SpeciesInfos> speciesInfo, BatSpeciesRegions regions) :
    base(speciesInfo, regions)
    {
      _name = name;
      _srcDir = srcDir; 
      _destDir = dstDir;  
      _expression = query;
      _records = new List<BatExplorerProjectFileRecordsRecord>();
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
          _analysis = new Analysis(_model.SpeciesInfos);
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

    public override BatExplorerProjectFileRecordsRecord[] getRecords()
    {
      return _records.ToArray();
    }

    public override string getFullFilePath(string path)
    {
      return Path.Combine(_destDir, path);
    }

    private bool crawl(DirectoryInfo dir)
    {
      DirectoryInfo[] dirs = dir.GetDirectories();
      bool ok = false;
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
        foreach(BatExplorerProjectFileRecordsRecord rec in qFile.Records)
          retVal._records.Add(rec);
        retVal._queryFile = qFile;
        retVal._model = model;
        retVal._analysis = new Analysis(model.SpeciesInfos);
        string fullReportName = Path.Combine(dstDir, retVal._queryFile.ReportFile);
        retVal._analysis.read(fullReportName);
        retVal.initSpeciesList();

      }
      catch (Exception ex)
      {
        DebugLog.log("error reading query file: " + name + " :" + ex.ToString(), enLogType.ERROR);
        retVal = null;  
      }
      return retVal;
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
      DebugLog.log(_cntCall.ToString() + " calls in " + _cntFile.ToString() + " files found", enLogType.INFO);
    }


    private bool evaluatePrj(DirectoryInfo dir)
    {
      bool retVal = false;
      string[] files = System.IO.Directory.GetFiles(dir.FullName, "*" + AppParams.EXT_PRJ,
                   System.IO.SearchOption.TopDirectoryOnly);
      Project prj = new Project(_model.Regions, _model.SpeciesInfos);
      prj.readPrjFile(files[0]);
      Analysis analysis = new Analysis(_model.SpeciesInfos);
      analysis.read(prj.ReportName);

      FilterItem filter = new FilterItem();
      filter.Expression = _expression.Replace('\n', ' '); ;
      filter.IsForAllCalls = false;
      filter.Name = "query";

      string lastFileName = "";
      if (analysis.Files.Count == 0)
        MessageBox.Show(BatInspector.Properties.MyResources.QueryReportMissing, BatInspector.Properties.MyResources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
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
                BatExplorerProjectFileRecordsRecord rec = new BatExplorerProjectFileRecordsRecord();

                string absPath = Path.Combine(prj.PrjDir, prj.WavSubDir, file.Name);
                rec.File = Utils.relativePath( this._destDir, absPath);
                rec.Name = Path.GetFileNameWithoutExtension(file.Name);
                _records.Add(rec);
                file.Name = rec.File;
                _analysis.addFile(file);
                lastFileName = file.Name;
              }
              List<string> row = call.getReportRow();
              row[0] = file.Name;
              _analysis.addCsvReportRow(row);
              _analysis.addReportItem(file, call);
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
