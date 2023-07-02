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
using System.Runtime.InteropServices;

namespace BatInspector
{
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

    public string Name { get { return _name; } }
    public string SrcDir { get { return _srcDir; } }
    public string DestDir { get { return _destDir; } }
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
      DirectoryInfo dir = new DirectoryInfo(_srcDir);
      _model = model;
      createQueryFile();
      crawl(dir);
    }

    private void crawl(DirectoryInfo dir)
    {
      DirectoryInfo[] dirs = dir.GetDirectories();
      foreach (DirectoryInfo d in dirs)
      {
        if (Project.containsProject(d) != "")
          evaluatePrj(d);
        else
          crawl(d);
      }
    }


    private void createQueryFile()
    {
      _queryFile = new QueryFile();
      _queryFile.Name = _name;
      _queryFile.Created = DateTime.Now.ToString();
      _queryFile.Expression = _expression;
      _queryFile.SrcDir = _srcDir;
    }

    private void openQueryFile(string name)
    {

    }


    private void evaluatePrj(DirectoryInfo dir) 
    {
      string[] files = System.IO.Directory.GetFiles(dir.FullName, "*" + AppParams.EXT_PRJ,
                   System.IO.SearchOption.TopDirectoryOnly);
      Project prj = new Project(_model.Regions, _model.SpeciesInfos);
      prj.readPrjFile(files[0]);
      Analysis analysis = new Analysis(_model.SpeciesInfos, prj.Notes);
      analysis.read(prj.ReportName);
      foreach(AnalysisFile file in analysis.Files)
      {
        FilterItem filter = new FilterItem();
        filter.Expression = _expression;
        filter.IsForAllCalls = false;
        filter.Name = "query";
        bool match = _model.Filter.apply(filter, file);
        if(match) 
        {
          BatExplorerProjectFileRecordsRecord rec = new BatExplorerProjectFileRecordsRecord();
          rec.File = Path.Combine(prj.PrjDir, prj.WavSubDir, file.Name);
          _records.Add(rec);
          foreach (AnalysisCall call in file.Calls)
          {
            List<string> row = call.getReportRow();
            _analysis.addCsvReportRow(row);
            _analysis.addReportItem(file, call);
          }
        }
      }
    }
  }
}
