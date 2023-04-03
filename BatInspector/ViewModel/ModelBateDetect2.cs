using libParser;
using libScripter;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Media.Animation;

namespace BatInspector
{
  public class ModelBateDetect2 : BaseModel
  {
    string _wavDir;
    string _annDir;
    string _reportPath;
    double _minProb = 0.5;
    List<SpeciesInfos> _speciesInfos;
    ViewModel _model;

    public override string Name { get; }
    public string WavDir { get { return _wavDir; } set { _wavDir = value; } }
    public string AnnotationDir { get { return _annDir; } set{ _annDir = value; } }
    public string ReportPath { get { return _reportPath; } set { _reportPath = value; } } 
    public double MinProb {  get { return _minProb; } set { _minProb = value; } }

    public ModelBateDetect2(List<SpeciesInfos> speciesInfos, ViewModel model) 
    {
      Name = enModel.BAT_DETECT2.ToString();
      _speciesInfos = speciesInfos;
      _model = model;
    }

    public override void classify()
    {
      string args = _wavDir + " " + _annDir + " " + _minProb.ToString(CultureInfo.InvariantCulture );
      int retVal = _proc.LaunchCommandLineApp(_model.Settings.PythonBin, null,  _model.Prj.PrjDir, true, args, true, true);
    }

    public override void train()
    {
      throw new NotImplementedException();
    }

    public void createReportFromAnnotations(double minProb)
    {
      Csv report = new Csv();
      string[] header = { "name", "recTime", "nr", "Species", "sampleRate", "FileLen", "freq_min", "freq_max", "duration","start","SpeciesMan","prob","remarks"};
    
      report.initColNames(header, true);

      string[] files = Directory.GetFiles(_annDir, "*.csv", SearchOption.AllDirectories);
      foreach (string file in files) 
      {
        // read infoFile
        string wavName = _wavDir + "/" + Path.GetFileName(file).Replace(".csv", "");
        string infoName = _wavDir + "/" + Path.GetFileName(file).Replace(".wav.csv", ".xml");
        string sampleRate = "?";
        string fileLen = "?";
        string recTime = "?";
        BatRecord info = ElekonInfoFile.read(infoName);
        if(info != null)
        {
          sampleRate = info.Samplerate.Replace(" Hz", ""); ;
          fileLen = info.Duration.Replace(" Sec","");
          recTime = info.DateTime;
        }

        // read annontation for one wav file
        Csv csv = new Csv();
        csv.read(file, ",", true);
        int rowcnt = csv.RowCnt;
        for(int row = 2; row <= rowcnt; row++) 
        {
          report.addRow();
          int repRow = report.RowCnt;
          report.setCell(repRow, "sampleRate", sampleRate);
          report.setCell(repRow, "FileLen", fileLen);
          report.setCell(repRow, "recTime", recTime);
          report.setCell(repRow, "name", wavName);
          int id = csv.getCellAsInt(row, "id");
          report.setCell(repRow, "nr", id + 1);
          double startTime = csv.getCellAsDouble(row, "start_time");
          report.setCell(repRow, "start", startTime, 3);
          double endTime = csv.getCellAsDouble(row, "end_time");
          double duration = (endTime - startTime)*1000;
          report.setCell(repRow, "duration", duration, 1);
          double fMin = csv.getCellAsDouble(row, "low_freq");
          report.setCell(repRow, "freq_min", fMin, 1);
          double fMax = csv.getCellAsDouble(row, "high_freq");
          report.setCell(repRow, "freq_max", fMax, 1);
          string latin = csv.getCell(row, "class");
          double prob = csv.getCellAsDouble(row, "class_prob");
          report.setCell(repRow, "prob", prob);
          string abbr = "";
          if (prob < minProb)
            abbr = "??PRO[";
          SpeciesInfos specInfo = SpeciesInfos.findLatin(latin, _speciesInfos);
          if(info != null) 
            abbr += specInfo.Abbreviation;
          if (prob < minProb)
            abbr += "]";
          report.setCell(repRow, "Species", abbr);
        }
      }

      report.saveAs(ReportPath);
    }
  }
}
