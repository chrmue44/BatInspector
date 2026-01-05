using BatInspector.Forms;
using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Documents;

namespace BatInspector
{

  [DataContract]
  public class Bd2Annatation
  {
    [DataMember(Name = "class")]
    public string Class { get; set; } = "";
    [DataMember]
    public double end_time { get; set; } = 1;
    [DataMember(Name = "event")]
    public string Event { get; set; } = "";
    [DataMember]
    public double high_freq { get; set; } = 1;
    [DataMember]
    public string individual { get; set; } = "-1";
    [DataMember]
    public double low_freq { get; set; } = 0;
    [DataMember]
    public double start_time { get; set; } = 0;

    public Bd2Annatation() { }

    public Bd2Annatation(Bd2Annatation a)
    {
      Class = a.Class;
      end_time = a.end_time;
      Event = a.Event;
      high_freq = a.high_freq;
      individual = a.individual;
      low_freq = a.low_freq;
      start_time = a.start_time;
  }
}

  [DataContract]
  public class Bd2AnnFile
  {
    [DataMember]
    public bool annotated = true;

    [DataMember (Name = "annotation") ]
    public Bd2Annatation[] Annatations { get; set; }
    [DataMember]
    public string class_name { get; set; } = "";
    [DataMember]
    public double duration { get; set; } = 0;
    [DataMember]
    public string id { get; set; } = "";
    [DataMember]
    bool issues { get; set; } = false;
    [DataMember]
    public string notes { get; set; } = "";

    [DataMember]
    public int time_exp { get; set; } = 1;

    string _fileName;

    public void init()
    {
      Annatations = new Bd2Annatation[1]; 
    }

    public void saveAs(string fName)
    {
      try
      {
        using (StreamWriter file = new StreamWriter(fName))
        {
          using (MemoryStream stream = new MemoryStream())
          {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Bd2AnnFile));
            ser.WriteObject(stream, this);
            StreamReader sr = new StreamReader(stream);
            stream.Seek(0, SeekOrigin.Begin);
            string str = sr.ReadToEnd();
            file.Write(JsonHelper.FormatJson(str));
            file.Close();
            DebugLog.log("annotations saved to '" + fName + "'", enLogType.INFO);
          }
        }
      }
      catch (Exception e)
      {
        DebugLog.log("failed to write annotation file for BatInspector" + fName + ": " + e.ToString(), enLogType.ERROR);
      }
    }

    public void save()
    {
      saveAs(_fileName);
    }

    public static Bd2AnnFile loadFrom(string fPath, bool createIfMissing = true)
    {
      Bd2AnnFile retVal = null;
      FileStream file = null;
      try
      {
        DebugLog.log("try to load:" + fPath, enLogType.DEBUG);
        if (File.Exists(fPath))
        {
          using (file = new FileStream(fPath, FileMode.Open, FileAccess.Read))
          {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Bd2AnnFile));
            retVal = (Bd2AnnFile)ser.ReadObject(file);
            if (retVal == null)
              DebugLog.log("annotation file not well formed!", enLogType.ERROR);
            else
              retVal._fileName = fPath;
            DebugLog.log("successfully loaded", enLogType.DEBUG);
          }
        }
        else 
        {
          DebugLog.log("load failed", enLogType.DEBUG);
          retVal = new Bd2AnnFile();
          retVal._fileName = fPath;
          retVal.init();
          if(createIfMissing)
            retVal.save();
        }
      }
      catch (Exception e)
      {
        DebugLog.log($"failed to read config file: {fPath} : {e.ToString()}", enLogType.ERROR);
        retVal = null;
      }
      finally
      {
        if (file != null)
          file.Close();
      }
      
      return retVal;
    }


    public static void splitAnnotation(string name, double splitLength, bool removeOriginal)
    {
      Bd2AnnFile file = Bd2AnnFile.loadFrom(name);
      char ext = 'a';
      if(file != null)
      {
        double len = file.duration;
        Bd2Annatation[] annotations = file.Annatations;
        double dt = 0.000001;
        for (int i = 0; i <= (int)(len / splitLength); i++)
        {
          double startTime = i * splitLength;
          double endTime = (i + 1) * splitLength - dt;
          if (splitLength < len - splitLength * i)
            file.duration = splitLength;
          else
           file.duration = len - splitLength * i;
          
          List<Bd2Annatation> splitAnns = new List<Bd2Annatation>();
          foreach(Bd2Annatation a in annotations)
          {
            Bd2Annatation ann = new Bd2Annatation(a);
            if ((ann.start_time >= startTime) && (ann.end_time <= endTime))
            {
              ann.start_time = ann.start_time - startTime;
              ann.end_time = ann.end_time - startTime;
              splitAnns.Add(ann);
            }
            else if ((ann.start_time >= startTime) && (ann.start_time <= endTime) && (ann.end_time > endTime))
            {
              ann.start_time = ann.start_time - startTime;
              ann.end_time = splitLength - dt;
              if(ann.Event == "Echolocation")
                ann.Class = "Bat";
              splitAnns.Add(ann);
            }
            if ((ann.start_time < startTime) && (ann.end_time >= startTime) && (ann.end_time <= endTime))
            {
              ann.start_time = dt;
              ann.end_time = ann.end_time - startTime;
              if (ann.Event == "Echolocation")
                ann.Class = "Bat";
              splitAnns.Add(ann);
            }
          }
          file.Annatations = splitAnns.ToArray();
          int pos = name.ToLower().IndexOf(".wav.json");
          string fName = name.Substring(0, pos);
          fName += "_" + ext + ".wav.json";
          file.id = fName + ext + "wav";
          file.saveAs(fName);
          ext++;
        }
        if (File.Exists(name) && removeOriginal)
          File.Delete(name);
      }
    }


    public static void checkAndFixId(string name)
    {
      Bd2AnnFile file = Bd2AnnFile.loadFrom(name);
      if (file != null)
      {
        string fName = Path.GetFileNameWithoutExtension(name);
        if(file.id != fName)
        {
          file.id = fName;
          file.saveAs(name);
          DebugLog.log($"fixed id: {name}", enLogType.INFO);
        }
      }
    }
  }


  public class AnnReportItem
  {
    public string Name { get; set; } = "";
    public int Count { get; set; } = 0;

    public static AnnReportItem find(string name, List<AnnReportItem> list)
    {
      AnnReportItem retVal = null;
      foreach (AnnReportItem item in list)
      {
        if (name == item.Name)
        {
          retVal = item;
          break;
        }
      }
      return retVal;
    }
  }

  public class SumAnnotations
  {
    string _dir;
    string _report;
    List<AnnReportItem> _list; 

    public static int createReport(string rootDir, string report)
    {
      SumAnnotations sum = new SumAnnotations(rootDir, report);
      DirectoryInfo dir = new DirectoryInfo (rootDir);
      bool ok = sum.crawl(dir);
      if (ok)
        return sum.writeToCsv();
      else
        return 2;
    }

    private SumAnnotations(string rootDir, string report)
    {
      _dir = rootDir;
      _report = report;
      _list = new List<AnnReportItem>();
    }

    private bool evaluateAnns(FileInfo[] files)
    {
      bool retVal = true;
      foreach (FileInfo file in files)
      {
        Bd2AnnFile annFile = Bd2AnnFile.loadFrom(file.FullName);
        if (annFile != null)
        {
          foreach(Bd2Annatation ann in annFile.Annatations)
          {
            AnnReportItem rItem = AnnReportItem.find(ann.Class, _list);
            if (rItem != null)
              rItem.Count++;
            else
            {
              rItem = new AnnReportItem();
              rItem.Name = ann.Class;
              rItem.Count = 1;
              _list.Add(rItem);
            }
          }
        }
      }
      return retVal;
    }

    // CreateTrainingReport F:\bat\trainingBd2\ann F:\bat\trainingBd2\sumalls.csv
    private bool crawl(DirectoryInfo dir)
    {
      DirectoryInfo[] dirs = dir.GetDirectories();
      bool ok = true;
      foreach (DirectoryInfo d in dirs)
      {
          ok = crawl(d);
        if (!ok)
          break;
      }

      FileInfo[] files = dir.GetFiles("*.json");
      if ((files != null) && (files.Length > 0))
        ok = evaluateAnns(files);
      return ok;
    }

    private int writeToCsv()
    {
      Csv csv = new Csv();
      csv.addRow();
      csv.setCell(1, 1, "Species");
      csv.setCell(1, 2, "Count");
      foreach (AnnReportItem rItem in _list)
      {
        csv.addRow();
        csv.setCell(csv.RowCnt, 1, rItem.Name);
        csv.setCell(csv.RowCnt, 2, rItem.Count);
      }
      return csv.saveAs(_report);
    }
  }
}
