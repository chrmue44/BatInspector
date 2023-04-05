using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatInspector
{
  

  public abstract class BaseModel
  {
    int _index = 0;
    enModel _type;
    protected ProcessRunner _proc = new ProcessRunner();
    protected BaseModel(int index, enModel type)
    {
      _index = index;
      _type = type;
      Name = _type.ToString();
    }

    public string Name { get; }

    public int Index { get { return _index; } }

    public abstract void train();
    public abstract int classify(Project prj);

    public static BaseModel Create(int index, enModel type)
    {
      switch (type) 
      {
        case enModel.rnn6aModel:
           return new ModelCmuTsa(index) as BaseModel;
        case enModel.BAT_DETECT2:
          return new ModelBatDetect2(index) as BaseModel;
        default:
          return null;  
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
