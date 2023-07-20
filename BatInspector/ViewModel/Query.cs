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

using libParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace BatInspector
{
  /// <summary>
  /// a class to handle queries covering multiple projects 
  /// </summary>
  public class Query
  {
    string _name;
    string _srcDir;
    string _destDir;
    string _expression;
    ViewModel _model = null;
    QueryFile _queryFile = null;
    Analysis _analysis = null;
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

    public Query(string name, string srcDir, string dstDir, string query) 
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
        SrcDir = _srcDir,
        ReportFile = Path.Combine(_destDir, _name) + "_" + AppParams.PRJ_REPORT
      };
    }

    private void openQueryFile(string name)
    {

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
      _analysis.save(_queryFile.ReportFile, "sum query\nsum query");
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
              rec.File = Path.Combine(prj.PrjDir, prj.WavSubDir, file.Name);
              rec.Name = Path.GetFileNameWithoutExtension(file.Name);
              _records.Add(rec);
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
      return retVal;
    }
  }
}
