using libParser;
using System;


namespace BatInspector
{
  public class ModelBattyB : BaseModel
  {
    
    public ModelBattyB(int index, ViewModel model) : base(index, enModel.BATTY_BIRD_NET, "BattyBirdNET", model)
    {
    }

    public override int classify(Project prj, bool cli = false)
    {
      DebugLog.log("BattyBirdNET not yet implemented", enLogType.INFO);
      return 0; //TODO
    }

    public override int createReport(Project prj)
    {
      return 0; // TODO
    }


    public override void train()
    {
      throw new NotImplementedException();
    }
  }
}
