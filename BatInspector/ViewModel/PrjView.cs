using BatInspector.Controls;
using BatInspector.Properties;
using libParser;
using libScripter;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;

namespace BatInspector
{


  public class ReportItem
  {
    string _remarks;
    bool _changed = false;

    public ReportItem()
    {

    }

    public ReportItem(AnalysisFile file, AnalysisCall call)
    {

      FileName = file.Name;
      int callNr = call.getInt(Cols.NR);
      CallNr = callNr.ToString();
      if (callNr < 2)
        Remarks = file.getString(Cols.REMARKS);
      _changed = false;
      Row = call.ReportRow;
      FreqMin = (call.getDouble(Cols.F_MIN) / 1000).ToString("0.#", CultureInfo.InvariantCulture);
      FreqMax = (call.getDouble(Cols.F_MAX) / 1000).ToString("0.#", CultureInfo.InvariantCulture);
      FreqMaxAmp = (call.getDouble(Cols.F_MAX_AMP) / 1000).ToString("0.#", CultureInfo.InvariantCulture);
      Duration = call.getDouble(Cols.DURATION).ToString("0.#", CultureInfo.InvariantCulture);
      StartTime = call.getString(Cols.START_TIME);
      SpeciesAuto = call.getString(Cols.SPECIES);
      Probability = call.getDouble(Cols.PROBABILITY).ToString("0.###", CultureInfo.InvariantCulture);
      Latitude = call.getDouble(Cols.LAT).ToString("0.#######", CultureInfo.InvariantCulture);
      Longitude = call.getDouble(Cols.LON).ToString("0.#######", CultureInfo.InvariantCulture);
      Temperature = call.getDouble(Cols.TEMPERATURE).ToString("0.#", CultureInfo.InvariantCulture);
      Humidity = call.getDouble(Cols.HUMIDITY).ToString("0.#", CultureInfo.InvariantCulture);
      //  Snr = call.getDouble(Cols.SNR).ToString();
      SpeciesMan = call.getString(Cols.SPECIES_MAN);
    }

    public bool Changed { get { return _changed; } }
    public int Row { get; set; }
    public string FileName { get; set; }
    public string CallNr { get; set; }
    public string StartTime { get; set; }
    public string Duration { get; set; }
    public string FreqMin { get; set; }
    public string FreqMax { get; set; }
    public string FreqMaxAmp { get; set; }

    public string SpeciesAuto { get; set; }
    public string SpeciesMan { get; set; }
    public string Probability { get; set; }
    //  public string Snr { get; set; }

    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public string Temperature { get; set; }
    public string Humidity { get; set; }
    public string Remarks
    {
      get { return _remarks; }
      set
      {
        _remarks = value;
        _changed = true;
      }
    }

    public void resetChanged()
    {
      _changed = false;
    }

    public static ReportItem find(string filename, List<ReportItem> list)
    {
      foreach (ReportItem r in list)
      {
        if (r.FileName == filename)
          return r;
      }
      return null;
    }
  }

  public class PrjView
  {
    List<ReportItem> _report = new List<ReportItem>();
    List<string> _showWavFiles = new List<string>();
    int _lastListStartIdx = -1;

    public List<ReportItem> ListView { get { return _report; } }
    public List<string> VisibleFiles { get { return _showWavFiles; } }

    public Project Prj { get; set; }

    public Query Query { get; set;}
    public int StartIdx { get; set; } = 0;
    public PrjView()
    {
    }



    public void buildListOfVisibles(bool selectedOnly)
    {
      _showWavFiles.Clear();
      PrjRecord[] list = getRecords();
      foreach (PrjRecord rec in list)
      {
        if (!selectedOnly || (selectedOnly && rec.Selected))
          _showWavFiles.Add(rec.File);
      }
    }

    public void addFile(AnalysisFile file)
    {
      foreach (AnalysisCall c in file.Calls)
      {
        addReportItem(file, c);
      }
    }

    public bool populateList(int startIdx, Filter filter, FilterItem filterItem)
    {
      if ((startIdx == _lastListStartIdx) && (_report.Count > 0))
        return false;
      
      StartIdx = startIdx;
      PrjRecord[] recList = getRecords();
      Analysis analysis = getAnalysis();
      if (recList != null)
      {
        _lastListStartIdx = startIdx;
        _report.Clear();
        int maxLines = 50;
        int fileIdx = startIdx;
        int line = 0;

        while ((line < maxLines) && (fileIdx < VisibleFiles.Count))
        {
          string wavFile = VisibleFiles[fileIdx];
          AnalysisFile f = analysis.find(wavFile);
          if (f != null)
          {
            for (int c = 0; c < f.Calls.Count; c++)
            {
              bool res = true;
              if(filter != null)
                res = filter.apply(filterItem, f.Calls[c]);
              if (res)
              {
                ReportItem it = new ReportItem(f, f.Calls[c]);
                _report.Add(it);
                line++;
              }
            }
          }
          fileIdx++;
        }
      }
      return true;
    }

    public void addReportItem(AnalysisFile file, AnalysisCall call)
    {
      if (_report != null)
      {
        ReportItem item = new ReportItem(file, call);
        _report.Add(item);
      }
    }

    public void updateReport(Csv csv)
    {
      foreach (ReportItem item in _report)
      {
        int row = item.Row;
        item.Longitude = csv.getCell(row, Cols.LON);
        item.Latitude = csv.getCell(row, Cols.LAT);
        item.FreqMax = csv.getCell(row, Cols.F_MAX);
        item.FreqMin = csv.getCell(row, Cols.F_MIN);
        item.Duration = csv.getCell(row, Cols.DURATION);
        item.CallNr = csv.getCell(row, Cols.NR);
        item.FileName = csv.getCell(row, Cols.NAME);
        item.FreqMaxAmp = csv.getCell(row, Cols.F_MAX_AMP);
        item.Probability = csv.getCell(row, Cols.PROBABILITY);
        item.Remarks = csv.getCell(row, Cols.REMARKS);
        item.StartTime = csv.getCell(row, Cols.START_TIME);
        item.SpeciesAuto = csv.getCell(row, Cols.SPECIES);
        item.SpeciesMan = csv.getCell(row, Cols.SPECIES_MAN);
      }
    }

    private PrjRecord[] getRecords()
    {
      PrjRecord[] retVal = new PrjRecord[0];
      if ((Prj != null) && (Prj.Ok))
      {
        retVal = Prj.Records;
      }
      else if (Query != null)
      {
        retVal = Query.Records;
      }
      return retVal;
    }

    private Analysis getAnalysis()
    {
      Analysis retVal = null;
      if ((Prj != null) && (Prj.Ok))
      {
        retVal = Prj.Analysis;
      }
      else if (Query != null)
      {
        retVal = Query.Analysis;
      }
      return retVal;
    }


  }
}
