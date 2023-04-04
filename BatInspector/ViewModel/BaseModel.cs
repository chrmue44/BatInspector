using libScripter;
using System;
using System.Collections.Generic;
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
    public abstract int classify(int options, Project prj);

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
  }
}
