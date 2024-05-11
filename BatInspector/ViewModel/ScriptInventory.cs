/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using libParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace BatInspector
{
  public enum enParamType
  {
    FILE,
    DIRECTORY,
    MICSCELLANOUS,
    BOOL
  }

  [DataContract]
  public class ParamItem
  {
    [DataMember]
    public string Name { get; set; } = "";
    public string VarName { get; set; } = "";
    [DataMember]
    public enParamType Type { get; set; } = enParamType.MICSCELLANOUS; 
    public ParamItem(string description, enParamType type, string varName)
    {
      Name = description;
      Type = type;
      VarName = varName;
    }

    public ParamItem()
    {

    }
  }

  [DataContract]
  public class ScriptItem
  {
    public ScriptItem(int index, string name, string description, bool isTool, List<ParamItem> parameter)
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
    public List<ParamItem> Parameter { get; set; }
  }


  [DataContract]
  public class ScriptInventory
  {

    public const string FName = "Scripts.json";
    public const string InstallFile = "scriptinst.txt";
    public const string INST_FLAG = "INSTALLED";

    static string _scriptPath = "";

    [DataMember]
    [LocalizedCategory("SetCatScripting"),
      DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public List<ScriptItem> Scripts { get; set; } = new List<ScriptItem>();


    public static ScriptInventory loadFrom(string fPath, out bool firstLoadAfterInstall)
    {
      _scriptPath = fPath;
      string fileName = Path.Combine(fPath, FName);
      ScriptInventory retVal = null;
      FileStream file = null;
      firstLoadAfterInstall = false;
      try
      {
        DebugLog.log("try to load:" + fileName, enLogType.DEBUG);
        bool inventoryExists = false;
        if (File.Exists(fileName))
        {
          inventoryExists = true;
          file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
          DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ScriptInventory));
          retVal = (ScriptInventory)ser.ReadObject(file);
          if (retVal == null)
            DebugLog.log("settings file not well formed!", enLogType.ERROR);
          else if (retVal.Scripts == null)
            retVal.initScripts();
          DebugLog.log("successfully loaded", enLogType.DEBUG);
          file.Close();
          file = null;
        }
        else
        {
          DebugLog.log("load failed", enLogType.DEBUG);
          retVal = new ScriptInventory();
          retVal.initScripts();
          retVal.save();
        }

        string instFile = Path.Combine(AppParams.AppDataPath, "setup",  InstallFile);
        bool copyScripts = false;
        if (File.Exists(instFile))
        {
          string str = File.ReadAllText(instFile);
          if (str != INST_FLAG)
          {
            firstLoadAfterInstall = true;
            File.WriteAllText(instFile, INST_FLAG);
            copyScripts = true;              
          }
        }
        else
        {
          File.WriteAllText(instFile, INST_FLAG);
          copyScripts = true;
        }

        if (copyScripts)
        {
          retVal.CopyScriptsFromInstaller();
          if (inventoryExists)
            retVal.updateInventory(fileName);
        }
        retVal.createParamNames();
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
        reIndex();
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
      if (!Directory.Exists(_scriptPath))
        Directory.CreateDirectory(_scriptPath);
      saveAs(Path.Combine(_scriptPath, FName));
    }

    public ScriptItem getScriptInfo(string script)
    {
      ScriptItem retVal = null;
      script = Path.GetFileName(script);
      foreach(ScriptItem s in Scripts)
      {
        if(s.Name == script)
        {
          retVal = s;
          break;
        }
      }
      return retVal;
    }

    private void initScripts()
    {
      Scripts = new List<ScriptItem>
      {
        new ScriptItem(0, "copyAutoToMan.scr", "Alle plausiblen KI-Bestimmungen übernehmen", false,new List<ParamItem>()),
        new ScriptItem(1, "reset_man.scr", "Alle manuellen Species auf 'todo' setzen",false,new List<ParamItem>()),
        new ScriptItem(2, "bandpass.scr", "Automatischen Bandpass auf alle selektierten Dateien", false,new List<ParamItem>()),
        new ScriptItem(3, "resample.scr", "Resampling einer WAV-Datei",false,
                           new List<ParamItem>(){new ParamItem("Name der WAV-Datei", enParamType.FILE, "PAR1"),
                                                 new ParamItem("neue Sampling-Rate [Hz]",enParamType.MICSCELLANOUS,"PAR2") }),
        new ScriptItem(4, "rescale_dat.scr", "Samplingrate aller Dateien im Verzeichnis umskalieren",false,
                           new List<ParamItem>(){new ParamItem("Verzeichnis auswählen", enParamType.DIRECTORY,"PAR1"),
                           new ParamItem("Skalierfaktor", enParamType.MICSCELLANOUS,"PAR2")}),
        new ScriptItem(5, "bulk_import_batspy.scr", "mehrere BatSpy-Projekte importieren und auswerten", false, new List<ParamItem>()
                                                                                        {new ParamItem("Quellverzeichnis", enParamType.DIRECTORY,"PAR1"),
                                                                                         new ParamItem("Zielverzeichnis", enParamType.DIRECTORY,"PAR2") }),
        new ScriptItem(6, "bulk_import_wav.scr", "mehrere Verz. mit WAV Dateien importieren und auswerten", false, new List<ParamItem>()
                                                                                        {new ParamItem("Quellverzeichnis", enParamType.DIRECTORY,"PAR1"),
                                                                                         new ParamItem("Zielverzeichnis", enParamType.DIRECTORY,"PAR2"),
                                                                                         new ParamItem("Quelldateien löschen", enParamType.BOOL,"PAR3") }),
        new ScriptItem(7, "tool_all_todo.scr", "set all SpeciesMan to 'todo'", true,new List<ParamItem>()),
        new ScriptItem(8, "tool_replace_pipistrelle.scr", "Alle Pipistrelluns mit Gattung ersetzen", true, new List<ParamItem>()),
        new ScriptItem(9, "tool_replace_nyctalus.scr", "Alle Nyctalus mit Gattung ersetzen", true, new List<ParamItem>()),
        new ScriptItem(10, "tool_copy_spec_from_first.scr", "Alle Spezies mit Spezies des ersten Rufs ersetzen", true, new List<ParamItem>()),
        new ScriptItem(11, "tool_replace_PAUR.scr", "Alle PAUR, PAUS mit 'Social' ersetzen", true, new List<ParamItem>())
      };
    }

    private void createParamNames()
    {
      foreach(ScriptItem s in Scripts)
      {
        for(int i = 0; i< s.Parameter.Count; i++)
        {
          s.Parameter[i].VarName = "PAR" + (i + 1).ToString();
        }
      }
    }

    private void CopyScriptsFromInstaller()
    {
      string srcDir = Path.Combine(AppParams.AppDataPath, "setup", AppParams.DIR_SCRIPT);
      DirectoryInfo dir = new DirectoryInfo(srcDir);
      FileInfo[] files = dir.GetFiles("*.scr");
      string [] fileNames = new string[files.Length];
      for (int i = 0; i < files.Length; i++)
        fileNames[i] = files[i].FullName;
        
      Utils.copyFiles(fileNames, _scriptPath, false, true);
        DebugLog.log("initialized script folder", enLogType.INFO);

    }

    private void reIndex()
    {
      List<ScriptItem> list = new List<ScriptItem>();
      foreach(ScriptItem s in Scripts)
      {
        if (!s.IsTool)
          list.Add(s);
      }
      foreach (ScriptItem s in Scripts)
      {
        if (s.IsTool)
          list.Add(s);
      }
      Scripts = list;
      for (int i = 0; i < Scripts.Count; i++)
        Scripts[i].Index = i;
    }

    private void updateInventory(string fName)
    {
      ScriptInventory srcInventory = new ScriptInventory();
      srcInventory.initScripts();
      bool save = false;
      foreach (ScriptItem s in srcInventory.Scripts)
      {

        ScriptItem i = getScriptInfo(s.Name);
        if (i == null)
        {
          Scripts.Add(s);
          save = true;
        }
      }
      if (save)
      {
        saveAs(fName);
      }
    }
  }
}
