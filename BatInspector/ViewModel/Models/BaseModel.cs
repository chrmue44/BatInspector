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
using System.IO;
using System.Web.UI.WebControls;
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

  public abstract class BaseModel
  {
    static protected readonly XmlSerializer ModParSerializer = new XmlSerializer(typeof(DefModelParamFile));

    int _index = 0;
    enModel _type;
    protected bool _isBusy = false;
    protected ViewModel _model;
    protected bool _cli = false;

    protected ProcessRunner _proc = new ProcessRunner();
    protected BaseModel(int index, enModel type, string name, ViewModel model)
    {
      _index = index;
      _type = type;
      Name = name;
      _model = model;
    }

    public string Name { get; }
    public int Index { get { return _index; } }
    public bool IsBusy { get { return _isBusy; } }
    public enModel Type { get { return _type; } }

    public abstract void train();
    public abstract int classify(Project prj, bool cli = false);
    public virtual int createReport(Project prj)
    {
      return 0;
    }

    public static BaseModel Create(int index, enModel type, ViewModel model)
    {
      switch (type) 
      {
        case enModel.rnn6aModel:
           return new ModelCmuTsa(index, model) as BaseModel;
        case enModel.BAT_DETECT2:
          return new ModelBatDetect2(index, model) as BaseModel;
        case enModel.BATTY_BIRD_NET:
          return new ModelBattyB(index, model) as BaseModel;

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


    public static ModelParams[] readDefaultModelParams()
    {
      string xml = File.ReadAllText(AppParams.Inst.ModelDefaultParamsFile);
      TextReader reader = new StringReader(xml);
      DefModelParamFile f = (DefModelParamFile)ModParSerializer.Deserialize(reader);
      ModelParams[] retVal = f.Models;
      return retVal;
    }

    public void stopClassification()
    {
      if (_proc.IsRunning && _isBusy)
      {
        _proc.Stop();
        _isBusy = false;
      }
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
  }
}
