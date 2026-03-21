/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace BatInspector
{
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public class DefModelParamFile
  {
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public ModelParams[] Models { get; set; } = new ModelParams[5];

  }

  class ModelScriptItem
  {
    public enModel Classifier { get; set; }
    public string Model { get; set; }
    public string Script { get; set; }

    ModelScriptItem(enModel classifier, string model, string script)
    {
      Classifier = classifier;
      Model = model;
      Script = script;
    }

    static ModelScriptItem[] _list = new ModelScriptItem[]
    {
      new ModelScriptItem(enModel.BAT_DETECT2, BaseModel.BD2_MODEL_GERMAN, "auto_to_man_german_bats_09.scr"),
      new ModelScriptItem(enModel.BAT_DETECT2, BaseModel.BD2_MODEL_UK, "auto_to_man_UK.scr"),
    };

    public static string getScriptName(enModel classifier, string modelName)
    {
      string retVal = AppParams.Inst.ScriptCopyAutoToMan;
      foreach (ModelScriptItem it in _list)
      {
        if ((it.Classifier == classifier) && (modelName == it.Model))
        {
          retVal = it.Script;
          break;
        }
      }
      return retVal;
    }
  }

  public abstract class BaseModel
  {

    public const string BD2_MODEL_GERMAN = "GermanBats_0.93.pth.tar";
    public const string BD2_MODEL_UK = "Net2DFast_UK_same.pth.tar";

    static protected readonly XmlSerializer ModParSerializer = new XmlSerializer(typeof(DefModelParamFile));

    int _index = 0;
    enModel _type;
    protected bool _isBusy = false;
    protected bool _cli = false;

    protected ProcessRunner _proc = new ProcessRunner();
    protected BaseModel(int index, enModel type, string name)
    {
      _index = index;
      _type = type;
      Name = name;
    }

    public string Name { get; }
    public int Index { get { return _index; } }
    public bool IsBusy { get { return _isBusy; } }
    public enModel Type { get { return _type; } }

    public abstract void train();
    public abstract int classify(Project prj, bool removeEmptyFiles, bool cli = false);
    public virtual int createReport(Project prj)
    {
      return 0;
    }

    public static void addReportRow(enModel classifier, Csv csv, sqlRow sqlRow)
    {
      switch (classifier)
      {
        case enModel.BATTY_BIRD_NET:
          ModelBattyB.addReportRow(csv, sqlRow);
          break;
        case enModel.BIRDNET:
          ModelBirdnet.addReportRow(csv, sqlRow);
          break;
        default:
        case enModel.BAT_DETECT2:
          ModelBatDetect2.addReportRow(csv, sqlRow);
          break;
      }
    }

    protected static void addFieldToRow(Csv csv, sqlRow sql, string fieldNameRep, string fieldNameDb)
    {
      int c = sql.findField(fieldNameDb);
      if (c >= 0)
        csv.setCell(csv.RowCnt, fieldNameRep, sql.Fields[c].ToString());
    }

    public static BaseModel Create(int index, enModel type)
    {
      switch (type)
      {
        case enModel.BAT_DETECT2:
          return new ModelBatDetect2(index) as BaseModel;
        case enModel.BATTY_BIRD_NET:
          return new ModelBattyB(index) as BaseModel;
        case enModel.BIRDNET:
          return new ModelBirdnet(index) as BaseModel;

        default:
          return null;
      }
    }

    public static void writeModelParams(ModelParams[] modelParams, string fileName)
    {
      TextWriter writer = new StreamWriter(fileName);
      DefModelParamFile dmf = new DefModelParamFile();
      dmf.Models = modelParams;
      ModParSerializer.Serialize(writer, dmf);
      writer.Close();
    }

    public static string getScriptName(enModel classifier, string modelName)
    {
      return ModelScriptItem.getScriptName(classifier, modelName);
    }

    public static ModelParams[] readDefaultModelParams()
    {
      ModelParams[] retVal = null;
      if (!File.Exists(AppParams.Inst.ModelDefaultParamsFile))
        DebugLog.log("Default model params not found: {AppParams.Inst.ModelDefaultParamsFile}", enLogType.ERROR);
      string xml = File.ReadAllText(AppParams.Inst.ModelDefaultParamsFile);
      TextReader reader = new StringReader(xml);
      DefModelParamFile f = (DefModelParamFile)ModParSerializer.Deserialize(reader);
      if (f != null && (f.Models != null))
      {
        List<ModelParams> l = new List<ModelParams>();
        foreach (ModelParams p in f.Models)
        {
          string path = Path.Combine(AppParams.Inst.ModelRootPath, p.SubDir);
          l.Add(p);
        }
        retVal = l.ToArray();
      }
      if (retVal == null)
        DebugLog.log("Serialization failed: {AppParams.Inst.ModelDefaultParamsFile}", enLogType.ERROR);
      return retVal;
    }

    /// <summary>
    /// check the performance of the classification result of a model
    /// </summary>
    /// <param name="prj">project with evaluation results</param>
    /// <param name="annotationDir">directory containing manually generated annotations related to prj</param>
    /// <param name="output">output file in csv format containing the evaluation results</param>
    /// <param name="thresh">threshhold value for probability that counts as false positive</param>
    public static void checkModelPerformance(Project prj, string annotationDir, string output, double thresh)
    {
      if (!Directory.Exists(annotationDir))
      {
        DebugLog.log($"checkModelPerformance: directory {annotationDir} does not exist", enLogType.ERROR);
        return;
      }
      string[] files = Directory.GetFiles(annotationDir, "*.json");
      if (files.Length == 0)
      {
        DebugLog.log($"checkModelPerformance: directory {annotationDir} does not contain annotation files", enLogType.ERROR);
        return;
      }
      Csv perfResult = createPerfResult(output);

      foreach (string file in files)
        evaluateAnnFile(prj, file, ref perfResult);
      int lastRow = perfResult.RowCnt;

      string modName = prj.AvailableModelParams[prj.SelectedModelIndex].Name + "-" +
                       prj.AvailableModelParams[prj.SelectedModelIndex].DataSet;

      List<SumSpec> li = null;
      for (thresh = 0.7; thresh > 0.2; thresh -= 0.1)
      {
        li = summarizePerformance(perfResult, thresh, lastRow);
        createConfusionMatrix(perfResult, thresh, li, modName);
      }

      createConfusionMatrix(perfResult, 0, li, modName);

      perfResult.insertRow(1);
      perfResult.insertRow(1);
      perfResult.insertRow(1);
      perfResult.setCell(1, 1, "Model Performance");
      perfResult.setCell(1, 4, "Reference Project:");
      perfResult.setCell(1, 5, prj.PrjDir);
      perfResult.setCell(2, 1, "Classifier:");
      perfResult.setCell(2, 2, prj.AvailableModelParams[prj.SelectedModelIndex].Name);
      perfResult.setCell(2, 3, "Model");
      perfResult.setCell(2, 4, prj.AvailableModelParams[prj.SelectedModelIndex].DataSet);

      perfResult.save();
    }

    /// <summary>
    /// check if annotation files match with wav files
    /// </summary>
    public static void findMissingFilesInTrainingData(string pathWav, string pathAnn)
    {
      string[] wavFiles = Directory.GetFiles(pathWav, "*.wav");
      string[] annFiles = Directory.GetFiles(pathAnn, "*.json");

      foreach (string wav in wavFiles)
      {
        string wavFile = Path.GetFileName(wav);
        bool found = false;
        foreach (string ann in annFiles)
        {
          string annFile = Path.GetFileName(ann);
          if (annFile.Contains(wavFile))
          {
            found = true;
            break;
          }
        }
        if (!found)
          DebugLog.log($"missing annotation: {wav}", enLogType.ERROR);
      }

      foreach (string ann in annFiles)
      {
        bool found = false;
        string annFile = Path.GetFileName(ann);
        foreach (string wav in wavFiles)
        {
          string wavFile = Path.GetFileName(wav);
          if (annFile.Contains(wavFile))
          {
            found = true;
            break;
          }
        }
        if (!found)
          DebugLog.log($"missing wav: {ann}", enLogType.ERROR);
      }
      DebugLog.log("check of training data complete", enLogType.INFO);
    }

    public static void countCallsInTrainingData(string pathAnn, string csvName)
    {
      Csv csv = new Csv();
      csv.read(csvName, ";", true);
      int maxRow = csv.RowCnt;
      for (int row = 2; row <= maxRow; row++)
      {
        string annFileName = csv.getCell(row, "FileName") + ".json";
        string spec = csv.getCell(row, "Species");
        annFileName = Path.Combine(pathAnn, annFileName);
        Bd2AnnFile annFile = Bd2AnnFile.loadFrom(annFileName, false);
        if (annFile != null)
        {
          int count = 0;
          foreach (Bd2Annatation ann in annFile.Annatations)
          {
            if ((ann != null) && (ann.Class != "Bat") && (ann.Class != "Not Bat") && (ann.Class != "Unknown"))
              count++;
          }
          csv.setCell(row, spec, count);
        }
        else
          csv.setCell(row, spec, 1);
      }
      csv.save();
    }

    public void stopClassification()
    {
      if (_proc.IsRunning && _isBusy)
      {
        _proc.Stop();
        _isBusy = false;
      }
    }

    public static string getDefaultModel(enModel model, double lat, double lon)
    {
      string retVal = "";
      switch (model)
      {
        case enModel.BAT_DETECT2:
          if (App.Model.Regions.IsInRegion(lat, lon, "Deutschland"))
            retVal = BaseModel.BD2_MODEL_GERMAN;
          else if (App.Model.Regions.IsInRegion(lat, lon, "United Kingdom"))
            retVal = BaseModel.BD2_MODEL_UK;
          else
            retVal = BaseModel.BD2_MODEL_GERMAN;
          break;

        case enModel.BIRDNET:
          retVal = "WorldWide";
          break;

        case enModel.BATTY_BIRD_NET:
          if (App.Model.Regions.IsInRegion(lat, lon, "Deutschland"))
            retVal = "BattyBirdNET-EU-256kHz";
          else if (App.Model.Regions.IsInRegion(lat, lon, "United Kingdom"))
            retVal = "BattyBirdNET-UK-256kHz";
          else
            retVal = "BattyBirdNET-EU-256kHz";
          break;
      }
      return retVal;
    }

    public void cleanUpAnnotations(Project prj)
    {
      string annDir = prj.getAnnotationDir();
      DirectoryInfo dir = new DirectoryInfo(annDir);
      if (dir.Exists)
        dir.Delete(true);
    }

    protected void createDir(string rootDir, string subDir, bool delete)
    {
      string dir = rootDir + "/" + subDir;
      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);
      if (delete)
      {
        System.IO.DirectoryInfo di = new DirectoryInfo(dir);

        foreach (FileInfo file in di.GetFiles())
        {
          file.Delete();
        }
        foreach (DirectoryInfo d in di.GetDirectories())
        {
          d.Delete(true);
        }
      }
    }

    protected void removeDir(string rootDir, string subDir)
    {
      string dir = rootDir + "/" + subDir;
      try
      {
        if (Directory.Exists(dir))
          Directory.Delete(dir, true);
      }
      catch (Exception ex)
      {
        DebugLog.log("problems deleting dir: " + dir + ", " + ex.ToString(), enLogType.ERROR);
      }
    }

    private static Csv createPerfResult(string name)
    {
      string header = Cols.PERF_ANN_FILE + ";" + Cols.NR + ";" + Cols.START_TIME + ";" + Cols.SPECIES_MAN + ";"
                     + Cols.SPECIES + ";" + Cols.PROBABILITY + ";" + Cols.PERF_DETECTED + ";" + Cols.PERF_CORRECT;
      Csv retVal = new Csv(header);
      retVal.saveAs(name);
      return retVal;
    }

    private static void evaluateAnnFile(Project prj, string file, ref Csv perfResult)
    {
      Bd2AnnFile annFile = Bd2AnnFile.loadFrom(file);
      int callNr = 0;
      foreach (Bd2Annatation ann in annFile.Annatations)
      {
        callNr++;
        perfResult.addRow();
        int row = perfResult.RowCnt;
        perfResult.setCell(row, Cols.PERF_ANN_FILE, file);
        perfResult.setCell(row, Cols.NR, callNr);
        perfResult.setCell(row, Cols.START_TIME, ann.start_time);
        perfResult.setCell(row, Cols.SPECIES_MAN, ann.Class);
        string wavFile = Path.GetFileName(file.Replace(".json", ""));
        AnalysisFile analysis = prj.Analysis.find(wavFile);
        bool found = false;

        if ((analysis != null) && (ann.Class != "Bat"))
        {
          foreach (AnalysisCall call in analysis.Calls)
          {
            double ts = call.getDouble(Cols.START_TIME);
            if (Utils.overLap(ts, ts + call.getDouble(Cols.DURATION) / 1000, ann.start_time, ann.end_time))
            {
              perfResult.setCell(row, Cols.PERF_DETECTED, 1);
              perfResult.setCell(row, Cols.PROBABILITY, call.getDouble(Cols.PROBABILITY));
              found = true;
              string spec = extractSpecies(call.getString(Cols.SPECIES));
              perfResult.setCell(row, Cols.SPECIES, spec);
              string latinName = getLatinName(spec);
              if (
                 (latinName == ann.Class) ||
                 ((latinName == "Mbart") && ((ann.Class == "Myotis brandtii") || ((ann.Class == "Myotis mystacinus")))) ||
                 ((latinName == "Pipistrellus nathusii") && (ann.Class == "Pipistrellus kuhlii")) ||
                 ((latinName == "Plecotus") && ((ann.Class == "Plecotus auritus") || (ann.Class == "Plecotus austricus")))
                )
                perfResult.setCell(row, Cols.PERF_CORRECT, 1);
              else
                perfResult.setCell(row, Cols.PERF_CORRECT, 0);
            }
          }
        }
        if (!found)
        {
          perfResult.setCell(row, Cols.PERF_DETECTED, 0);
          perfResult.setCell(row, Cols.PERF_CORRECT, 0);
        }
      }
    }

    private static string getLatinName(string abbr)
    {
      string retVal;
      SpeciesInfos specInfo = SpeciesInfos.findAbbreviation(abbr, App.Model.SpeciesInfos);
      if (specInfo != null)
        retVal = specInfo.Latin;
      else
        retVal = abbr;
      return retVal;
    }

    private static string getAbbreviation(string latin)
    {
      string retVal;
      SpeciesInfos specInfo = SpeciesInfos.findLatin(latin, App.Model.SpeciesInfos);
      if (specInfo != null)
        retVal = specInfo.Abbreviation;
      else
        retVal = latin;
      return retVal;
    }

    private static string extractSpecies(string spec)
    {
      string retVal = spec;
      int pos = spec.IndexOf('[');
      if ((pos >= 0) && (pos < spec.Length))
      {
        int pos2 = spec.IndexOf(']');
        retVal = spec.Substring(pos + 1, pos2 - pos - 1);
      }
      return retVal;
    }

    private static int getIndex(List<string> list, string str)
    {
      int retVal = -1;
      for (int i = 0; i < list.Count; i++)
      {
        if (list[i] == str)
        {
          retVal = i;
          break;
        }
      }
      return retVal;
    }

    private static void createConfusionMatrix(Csv perfResult, double thresh, List<SumSpec> li, string modelName)
    {
      perfResult.addRow();
      perfResult.addRow();
      perfResult.setCell(perfResult.RowCnt, 1, $"Confusion Matrix {modelName} threshold: {thresh.ToString(CultureInfo.InvariantCulture)}");
      perfResult.addRow();
      int row = perfResult.RowCnt;

      List<string> hdr = new List<string>();
      foreach (SumSpec s in li)
      {
        if (s.Name != "Bat")
        {
          string abbr = getAbbreviation(s.Name);
          hdr.Add(abbr);
        }
      }
      hdr.Add("Plecotus");
      hdr.Add("Mbart");
      hdr.Sort();
      hdr.Insert(0, "Species");
      hdr.Insert(1, "Count");
      perfResult.addRow(hdr);

      foreach (SumSpec s in li)
      {
        if (s.Name != "Bat")
        {
          perfResult.addRow();
          string abbr = getAbbreviation(s.Name);
          perfResult.setCell(perfResult.RowCnt, 1, abbr);
          perfResult.setCell(perfResult.RowCnt, 2, s.Total);
          int col = getIndex(hdr, abbr) + 1;
          if (thresh > 0.01)
          {
  //          perfResult.setCell(perfResult.RowCnt, col, s.CorrectThresh);
            foreach (ConfusionItem ci in s.ConfusionThresh)
            {
              col = getIndex(hdr, ci.Name) + 1;
              if (col >= 1)
                perfResult.setCell(perfResult.RowCnt, col, ci.Count);
            }
          }
          else
          {
    //        perfResult.setCell(perfResult.RowCnt, col, s.Correct);
            foreach (ConfusionItem ci in s.Confusion)
            {
              col = getIndex(hdr, ci.Name) + 1;
              if (col >= 1)
                perfResult.setCell(perfResult.RowCnt, col, ci.Count);
            }
          }
        }
      }
    }

    private static List<SumSpec> summarizePerformance(Csv perfResult, double thresh, int lastRow)
    {
      // build sums
      List<SumSpec> list = new List<SumSpec>();
      int row = 2;
      for (; row <= lastRow; row++)
      {
        string name = perfResult.getCell(row, Cols.SPECIES_MAN);
        SumSpec spec = SumSpec.find(name, list);
        if (spec == null)
        {
          spec = new SumSpec(name);
          list.Add(spec);
        }
        spec.Total++;
        int corr = perfResult.getCellAsInt(row, Cols.PERF_CORRECT);
        int det = perfResult.getCellAsInt(row, Cols.PERF_DETECTED);
        double prob = perfResult.getCellAsDouble(row, Cols.PROBABILITY);
        if (prob >= thresh)
          spec.CorrectThresh += corr;
        spec.Correct += corr;
        spec.Detected += det;
        if (det == 1)
        {
          string aIspec = perfResult.getCell(row, Cols.SPECIES);
          spec.addConfusion(aIspec);
          if (prob > thresh)
            spec.addConfusionThresh(aIspec);

          if (corr == 1)
          {
            spec.StatisticGood.add(prob);
          }
          if (corr == 0)
          {
            spec.FalsePositive++;
            if (prob > thresh)
              spec.FalsePositiveThresh++;
          }
        }
      }
      perfResult.addRow();
      perfResult.addRow();
      perfResult.setCell(perfResult.RowCnt, 1, $"Summarized Performance - Threshold:{thresh.ToString(CultureInfo.InvariantCulture)}");
      perfResult.addRow();

      //print sums
      List<string> h = new List<string>() { "Species", "Total", "Detected", "Correct", "CorrectThresh", "FalsePositive", "FalsePositiveThresh", "Recall", "RecallThresh", "Precision", "PrecisionThresh", "confusion", "ConfusionThresh", "mean_prob_good", "min_prob_good", "max_prob_good", "mean_prob_false_pos", "min_prob_false_pos", "max_prob_false_pos" };
      perfResult.addRow(h);
      row = perfResult.RowCnt;

      list.Sort((p1, p2) => p1.Name.CompareTo(p2.Name));
      foreach (SumSpec s in list)
      {
        row++;
        perfResult.addRow();
        perfResult.setCell(row, 1, s.Name);
        perfResult.setCell(row, 2, s.Total);
        perfResult.setCell(row, 3, s.Detected);
        perfResult.setCell(row, 4, s.Correct);
        perfResult.setCell(row, 5, s.CorrectThresh);
        perfResult.setCell(row, 6, s.FalsePositive);
        perfResult.setCell(row, 7, s.FalsePositiveThresh);
        double recall = s.Total > 0 ? (double)s.Correct / (double)s.Total : 0.0;
        double recallThresh = s.Total > 0 ? (double)s.CorrectThresh / (double)s.Total : 0.0;
        double precision = (s.Correct + s.FalsePositive) > 0 ? (double)s.Correct / (double)(s.Correct + s.FalsePositive) : 0.0;
        double precisionThresh = (s.Correct + s.FalsePositiveThresh) > 0 ? (double)s.Correct / (double)(s.Correct + s.FalsePositiveThresh) : 0.0;
        perfResult.setCell(row, 8, recall);
        perfResult.setCell(row, 9, recallThresh);
        perfResult.setCell(row, 10, precision);
        perfResult.setCell(row, 11, precisionThresh);
        perfResult.setCell(row, 12, s.getConfusion());
        perfResult.setCell(row, 13, s.getConfusionThresh());
        perfResult.setCell(row, 14, s.StatisticGood.Mean);
        perfResult.setCell(row, 15, s.StatisticGood.MinValue);
        perfResult.setCell(row, 16, s.StatisticGood.MaxValue);
        perfResult.setCell(row, 17, s.StatisticFalsePos.Mean);
        perfResult.setCell(row, 18, s.StatisticFalsePos.MinValue);
        perfResult.setCell(row, 19, s.StatisticFalsePos.MaxValue);
      }
      return list;
    }
  }

  class ConfusionItem
  {
    public string Name { get; set; } = "";
    public int Count { get; set; } = 0;

    static public string getListAsString(List<ConfusionItem> li)
    {
      string retVal = "";
      foreach (ConfusionItem s in li)
      {
        if (!string.IsNullOrEmpty(retVal))
          retVal += "|";
        retVal += $"{s.Name}[{s.Count}]";
      }
      return retVal;
    }

    static public void addItem(List<ConfusionItem> li, string name)
    {
      bool found = false;
      foreach (ConfusionItem s1 in li)
      {
        if (name == s1.Name)
        {
          found = true;
          s1.Count++;
          break;
        }
      }
      if (!found)
      {
        ConfusionItem it = new ConfusionItem();
        it.Name = name;
        it.Count = 1;
        li.Add(it);
      }
    }
  }

  class SumSpec
  {
    public string Name;
    public int Total;
    public int Detected;
    public int Correct;
    public int CorrectThresh;
    public int FalsePositive;
    public int FalsePositiveThresh;
    public List<ConfusionItem> Confusion;
    public List<ConfusionItem> ConfusionThresh;
    public Histogram StatisticGood;
    public Histogram StatisticFalsePos;

    public SumSpec(string name)
    {
      Name = name;
      Total = 0;
      Detected = 0;
      Correct = 0;
      FalsePositive = 0;
      FalsePositiveThresh = 0;
      ConfusionThresh = new List<ConfusionItem>();
      Confusion = new List<ConfusionItem>();
      StatisticGood = new Histogram(9);
      StatisticGood.init(0.0, 1.0);
      StatisticFalsePos = new Histogram(9);
      StatisticFalsePos.init(0.0, 1.0);
    }

    public static SumSpec find(string name, List<SumSpec> list)
    {
      SumSpec retVal = null;
      foreach (SumSpec s in list)
      {
        if (s.Name == name)
        {
          retVal = s;
          break;
        }
      }
      return retVal;
    }

    public void addConfusion(string s)
    {
      ConfusionItem.addItem(Confusion, s);
    }

    public string getConfusion()
    {
      return ConfusionItem.getListAsString(Confusion);
    }

    public void addConfusionThresh(string s)
    {
      ConfusionItem.addItem(ConfusionThresh, s);
    }

    public string getConfusionThresh()
    {
      return ConfusionItem.getListAsString(ConfusionThresh);
    }
  }
}
