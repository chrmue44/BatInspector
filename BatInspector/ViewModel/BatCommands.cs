/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/
using libScripter;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BatInspector
{
  public class BatCommands : BaseCommands
  {
    public BatCommands(delegateUpdateProgress delUpd) : base(delUpd)
    {

      _features = new ReadOnlyCollection<OptItem>(new[]
      {
        new OptItem("Test", "test", 1, fctTest),
      });

      _options = new Options(_features, false);
    }

    int fctTest(List<string> pars, out string ErrText)
    {
      ErrText = "";
      return 0;
    }
  }
}
