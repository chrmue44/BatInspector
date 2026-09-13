/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using libParser;

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
    public ScriptItem(int index, string name, string description, bool isTool, bool isInMenue, List<ParamItem> parameter)
    {
      Index = index;
      Name = name;
      Description = description;
      IsTool = isTool;
      IsInMenue = isInMenue;
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
    public bool IsInMenue { get; set; }

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


    public static ScriptInventory? loadFrom(string fPath, out bool firstLoadAfterInstall)
    {
      _scriptPath = fPath;
      string fileName = Path.Combine(fPath, FName);
      ScriptInventory? retVal = null;
      firstLoadAfterInstall = false;
      ScriptInventory? inventory = null;
      try
      {
        DebugLog.log("try to load:" + fileName, enLogType.DEBUG);
        bool inventoryExists = File.Exists(fileName);
        if (inventoryExists)
        {
          using (FileStream file = File.OpenRead(fileName))
          inventory = JsonSerializer.Deserialize<ScriptInventory>(file);
        }
        else
          DebugLog.log("load failed", enLogType.DEBUG);

        if(inventory == null)
        {
          DebugLog.log("ScriptInventory inventory file not well formed!", enLogType.ERROR);
          retVal = new ScriptInventory();
          retVal.initScripts();
          retVal.save();
        }
        else
        {
          retVal = inventory;
          if (retVal.Scripts == null)
            retVal.initScripts();
          DebugLog.log("Script inventory successfully loaded", enLogType.DEBUG);
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
          ScriptInventory.CopyScriptsFromInstaller();
          if (inventoryExists)
            retVal.updateInventory(fileName);
        }
        retVal.createParamNames();
      }
      catch (Exception e)
      {
        DebugLog.log($"failed to handle script inventory initialization: {e}", enLogType.ERROR);
      }  
      return retVal;
    }

    public void saveAs(string fName)
    {
      try
      {
        reIndex();
        using (StreamWriter file = new StreamWriter(fName))
        {
          using (MemoryStream stream = new MemoryStream())
          {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ScriptInventory));
            ser.WriteObject(stream, this);
            StreamReader sr = new StreamReader(stream);
            stream.Seek(0, SeekOrigin.Begin);
            string str = sr.ReadToEnd();
            file.Write(JsonHelper.FormatJson(str));
            file.Close();
            DebugLog.log("script inventory saved to '" + fName + "'", enLogType.INFO);
          }
        }
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

    public ScriptItem? getScriptInfo(string script)
    {
      ScriptItem? retVal = null;
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
        new ScriptItem( 0, "copyAutoToMan.scr", "Alle plausiblen KI-Bestimmungen übernehmen", false,false,new List<ParamItem>()),
        new ScriptItem( 1, "auto_to_man_german_bats_09.scr", "Alle plausiblen KI-Bestimmungen im Modell GermanBats 0.9 übernehmen", false, false, new List<ParamItem>()),
        new ScriptItem( 2, "auto_to_man_UK.scr", "Alle plausiblen KI-Bestimmungen im Modell UK übernehmen", false, false, new List<ParamItem>()),
        new ScriptItem( 3, "reset_man.scr", "Alle manuellen Species auf 'todo' setzen",false, true, new List<ParamItem>()),
        new ScriptItem( 4, "bandpass.scr", "Automatischen Bandpass auf alle selektierten Dateien", false,true, new List<ParamItem>()),
        new ScriptItem( 5, "resample.scr", "Resampling einer WAV-Datei",false, false, 
                           new List<ParamItem>(){new ParamItem("Name der WAV-Datei", enParamType.FILE, "PAR1"),
                                                 new ParamItem("neue Sampling-Rate [Hz]",enParamType.MICSCELLANOUS,"PAR2") }),
        new ScriptItem( 6, "rescale_dat.scr", "Samplingrate aller Dateien im Verzeichnis umskalieren",false, false,
                           new List<ParamItem>(){new ParamItem("Verzeichnis auswählen", enParamType.DIRECTORY,"PAR1"),
                           new ParamItem("Skalierfaktor", enParamType.MICSCELLANOUS,"PAR2")}),
        new ScriptItem( 7, "bulk_import_batspy.scr", "mehrere BatSpy-Projekte importieren und auswerten", false, true, new List<ParamItem>()
                                                                                        {new ParamItem("Quellverzeichnis", enParamType.DIRECTORY,"PAR1"),
                                                                                         new ParamItem("Zielverzeichnis", enParamType.DIRECTORY,"PAR2"),
                                                                                         new ParamItem("Aufnahmeort", enParamType.MICSCELLANOUS, "PAR3"),
                                                                                         new ParamItem("Ersteller des Projekts", enParamType.MICSCELLANOUS, "PAR4"),
                                                                                         new ParamItem("Bemerkungen", enParamType.MICSCELLANOUS,"PAR5")}),
        new ScriptItem( 8, "bulk_import_wav.scr", "mehrere Verz. mit WAV Dateien importieren und auswerten", false, true, new List<ParamItem>()
                                                                                        {new ParamItem("Quellverzeichnis", enParamType.DIRECTORY,"PAR1"),
                                                                                         new ParamItem("Zielverzeichnis", enParamType.DIRECTORY,"PAR2"),
                                                                                         new ParamItem("Quelldateien löschen", enParamType.BOOL,"PAR3") }),
        new ScriptItem( 9, "replace_plecotus.scr", "Alle selektierten PAUR, PAUS im Projekt mit Plecotus ersetzen", false, false, new List<ParamItem>()),
        new ScriptItem(10, "replace_spec.scr", "Für alle selektierten, SpeciesMan: ersetze eine Art durch eine andere", false, true, new List<ParamItem>()
                                                                                        {new ParamItem("Ersetze Species", enParamType.MICSCELLANOUS, "PAR1"),
                                                                                         new ParamItem("durch Species", enParamType.MICSCELLANOUS, "PAR2")}),
        new ScriptItem(11, "export_species.scr", "Alle gefundenen Arten in Unterprojekte speichern", false, true, new List<ParamItem>()
                                                                                        {new ParamItem("Zielverzeichnis für Unterprojekte", enParamType.DIRECTORY,"PAR1"),
                                                                                         new ParamItem("Name des Ziel-Projektes", enParamType.MICSCELLANOUS, "PAR2")}),
        new ScriptItem(12, "tool_all_todo.scr", "set all SpeciesMan to 'todo'", true, false, new List<ParamItem>()),
        new ScriptItem(13, "tool_replace_pipistrelle.scr", "Alle Pipistrelluns mit Gattung ersetzen", true, false, new List<ParamItem>()),
        new ScriptItem(14, "tool_replace_nyctalus.scr", "Alle Nyctalus mit Gattung ersetzen", true, false, new List<ParamItem>()),
        new ScriptItem(15, "tool_copy_spec_from_first.scr", "Alle Spezies mit Spezies des ersten Rufs ersetzen", true, false, new List<ParamItem>()),
        new ScriptItem(16, "tool_replace_PAUR.scr", "Alle PAUR, PAUS mit 'Social' ersetzen", true, false,new List<ParamItem>()),
        new ScriptItem(17, "tool_replace_plecotus.scr", "Alle PAUR, PAUS mit 'Plecotus' ersetzen", true, false, new List<ParamItem>())
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

    private static void CopyScriptsFromInstaller()
    {
      string srcDir = Path.Combine(AppParams.AppDataPath, "setup", AppParams.DIR_SCRIPT);
      DirectoryInfo dir = new DirectoryInfo(srcDir);
      FileInfo[] files = dir.GetFiles("*.scr");
      string [] fileNames = new string[files.Length];
      for (int i = 0; i < files.Length; i++)
      {
        fileNames[i] = files[i].FullName;
//        DebugLog.log($"copy script {fileNames[i]} to {_scriptPath}", enLogType.INFO);
      }

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
      int cntInMenue = 0;
      foreach (ScriptItem s in srcInventory.Scripts)
      {
        ScriptItem? i = getScriptInfo(s.Name);
        if (i == null)
        {
          Scripts.Add(s);
          save = true;
        }
        else
        {
          if (i.IsInMenue)
            cntInMenue++;
          if (i.Parameter.Count != s.Parameter.Count)
          {
            i.Parameter = s.Parameter;
            save = true;
            DebugLog.log($"updated parameter definition in script inventory for script {s.Name}", enLogType.INFO);
          }
        }
      }

      // if not a single script in menue, add them all to the menue
      if(cntInMenue == 0)
      {
        for (int i = 0; i < Scripts.Count; i++)
        {
          if (!Scripts[i].IsTool)
            Scripts[i].IsInMenue = true;
        }
      }

      if (save)
      {
        saveAs(fName);
      }
    }
  }
}
