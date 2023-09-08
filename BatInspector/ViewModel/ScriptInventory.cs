using libParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace BatInspector
{
  [DataContract]
  public class ScriptItem
  {
    public ScriptItem(int index, string name, string description, bool isTool, List<string> parameter)
    {
      Index = index;
      Name = name;
      Description = description;
      IsTool = isTool;
      Parameter = parameter;
    }

    [DataMember]
    public int Index { get; set; }

    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public string Description { get; set; }

    [DataMember]
    public bool IsTool { get; set; }
    [DataMember]
    public List<string> Parameter { get; set; }
  }


  [DataContract]
  public class ScriptInventory
  {

    public const string FName = "Scripts.json";
    public const string DIR_SCRIPTS = "scripts";          // sub directory to store scripts

    static string _scriptPath = "";

    [DataMember]
    [LocalizedCategory("SetCatScripting"),
      DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public List<ScriptItem> Scripts { get; set; } = new List<ScriptItem>();


    public static ScriptInventory loadFrom(string fPath)
    {
      _scriptPath = fPath;
      string fileName = Path.Combine(fPath, FName);
      ScriptInventory retVal = null;
      FileStream file = null;
      try
      {
        DebugLog.log("try to load:" + fileName, enLogType.DEBUG);
        if (File.Exists(fileName))
        {
          file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
          DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ScriptInventory));
          retVal = (ScriptInventory)ser.ReadObject(file);
          if (retVal == null)
            DebugLog.log("settings file not well formed!", enLogType.ERROR);
          else if (retVal.Scripts == null)
            retVal.initScripts();
          DebugLog.log("successfully loaded", enLogType.DEBUG);
        }
        else
        {
          DebugLog.log("load failed", enLogType.DEBUG);
          retVal = new ScriptInventory();
          retVal.initScripts();
          if (!Directory.Exists(_scriptPath))
          {
            Directory.CreateDirectory(_scriptPath);
            retVal.CopyScriptsFromInstaller();
          }
          retVal.save();
        }
      }
      catch (Exception e)
      {
        DebugLog.log("failed to read config file : " + fPath + ": " + e.ToString(), enLogType.ERROR);
        retVal = null;
      }
      finally
      {
        if (file != null)
          file.Close();
      }
      return retVal;
    }

    public void saveAs(string fName)
    {
      try
      {
        StreamWriter file = new StreamWriter(fName);
        MemoryStream stream = new MemoryStream();
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ScriptInventory));
        ser.WriteObject(stream, this);
        StreamReader sr = new StreamReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        string str = sr.ReadToEnd();
        file.Write(JsonHelper.FormatJson(str));
        file.Close();
        DebugLog.log("script inventory saved to '" + fName + "'", enLogType.INFO);
      }
      catch (Exception e)
      {
        DebugLog.log("failed to write script inventory file for BatInspector" + fName + ": " + e.ToString(), enLogType.ERROR);
      }
    }

    public void save()
    {
      saveAs(Path.Combine(_scriptPath, FName));
    }

    private void initScripts()
    {
      string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
      Scripts = new List<ScriptItem>
      {
        new ScriptItem(0, "copyAutoToMan.scr", "Alle plausiblen KI-Bestimmungen übernehmen", false,new List<string>()),
        new ScriptItem(1, "reset_man.scr", "Alle manuellen Species auf 'todo' setzen",false,new List<string>()),
        new ScriptItem(2, "bandpass.scr", "Automatischen Bandpass auf alle selektierten Dateie", false,new List<string>()),
        new ScriptItem(3, "resample.scr", "Resampling einer WAV-Datei",false,new List<string>(){"Name der WAV-Datei", "neue Sampling-Rate [Hz]"}),
        new ScriptItem(4, "tool_all_todo.scr", "set all SpeciesMan to 'todo'", true,new List<string>()),
        new ScriptItem(5, "tool_replace_pipistrelle.scr", "Alle Pipistrelluns mit Gattung ersetzen", true, new List<string>()),
        new ScriptItem(6, "tool_replace_nyctalus.scr", "Alle Nyctalus mit Gattung ersetzen", true, new List<string>())
      };
    }

    private void CopyScriptsFromInstaller()
    {
        string srcDir = Path.Combine(AppParams.AppDataPath, "setup", DIR_SCRIPTS);
        List<string> files = new List<string>();

        foreach (ScriptItem item in Scripts)
          files.Add(Path.Combine(srcDir, item.Name));
        Utils.copyFiles(files.ToArray(), _scriptPath);
        DebugLog.log("initialized script folder", enLogType.INFO);
    }
  }
}
