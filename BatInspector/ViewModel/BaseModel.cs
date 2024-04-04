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
using System.IO;

namespace BatInspector
{
  

  public abstract class BaseModel
  {
    int _index = 0;
    enModel _type;
    protected bool _isBusy = false;
    protected ViewModel _model;
    protected bool _cli = false;

    protected ProcessRunner _proc = new ProcessRunner();
    protected BaseModel(int index, enModel type, ViewModel model)
    {
      _index = index;
      _type = type;
      Name = _type.ToString();
      _model = model;
    }

    public string Name { get; }

    public int Index { get { return _index; } }
    public bool IsBusy { get { return _isBusy; } }

    public abstract void train();
    public abstract int classify(Project prj, bool cli = false);

    public static BaseModel Create(int index, enModel type, ViewModel model)
    {
      switch (type) 
      {
        case enModel.rnn6aModel:
           return new ModelCmuTsa(index, model) as BaseModel;
        case enModel.BAT_DETECT2:
          return new ModelBatDetect2(index, model) as BaseModel;
        default:
          return null;  
      }    
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
