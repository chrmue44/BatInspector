using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BatInspector
{
  public class Project
  {
    BatExplorerProjectFile _batExplorerPrj;
    string _prjFileName;

    public BatExplorerProjectFileRecordsRecord[] Records { get { return _batExplorerPrj.Records; } }

    public static bool containsProject(DirectoryInfo dir)
    {
      bool retVal = false;
      try
      {
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*.bpr",
                         System.IO.SearchOption.TopDirectoryOnly);
        if (files.Length > 0)
          retVal = true;
      }
      catch { }
      return retVal;
    }

    public static bool evaluationDone(DirectoryInfo dir)
    {
      bool retVal = false;
      string fName = dir.FullName + "\\report.csv";
      if (File.Exists(fName))
      {
        Csv csv = new Csv();
        {
          csv.read(fName);
          int colSp = csv.findInRow(1, Analysis.COL_SPECIES_MAN);
          if (colSp > 0)
          {
            int row = csv.findInCol("todo", colSp);
            if (row == 0)
              retVal = true;
          }
        }
      }

      return retVal;
    }

    public void readPrjFile(string fName)
    {
      _prjFileName = fName;
      string xml = File.ReadAllText(fName);
      var serializer = new XmlSerializer(typeof(BatExplorerProjectFile));

      TextReader reader = new StringReader(xml);
      _batExplorerPrj = (BatExplorerProjectFile)serializer.Deserialize(reader);
    }

    public void writePrjFile()
    {
      if (_batExplorerPrj != null)
      {
        var serializer = new XmlSerializer(typeof(BatExplorerProjectFile));
        TextWriter writer = new StreamWriter(_prjFileName);
        serializer.Serialize(writer, _batExplorerPrj);
        writer.Close();
      }
    }

    public void removeFile(string wavName)
    {
      List<BatExplorerProjectFileRecordsRecord> list = _batExplorerPrj.Records.ToList();
      foreach (BatExplorerProjectFileRecordsRecord rec in list)
      {
        if (rec.File == wavName)
        {
          list.Remove(rec);
          break;
        }
      }
      _batExplorerPrj.Records = list.ToArray();

    }
  }
}
