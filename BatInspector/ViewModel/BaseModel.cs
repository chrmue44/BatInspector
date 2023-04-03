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
    protected ProcessRunner _proc = new ProcessRunner();
    protected BaseModel() { }
    public abstract string Name { get; }

    public abstract void train();
    public abstract void classify();
  }
}
