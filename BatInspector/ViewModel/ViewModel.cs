using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace BatInspector
{
  public class ViewModel
  {

    string _selectedDir;
    Project _prj;

    Analysis _analysis;
    ProcessRunner _proc;
    ZoomView _zoom;

    Forms.MainWindow _mainWin;
    public string WavFilePath { get { return _selectedDir + "Records/"; } }
    public string PrjPath { get { return _selectedDir; } }

    public Analysis Analysis { get { return _analysis; } }

    public Project Prj { get { return _prj; } }

    public ZoomView ZoomView { get { return _zoom; } }
    public ViewModel(Forms.MainWindow mainWin)
    {
      _analysis = new Analysis();
      _proc = new ProcessRunner(DebugLog.log);
      _mainWin = mainWin;
      _prj = new Project();
      _zoom = new ZoomView();
    }



    public void initProject(DirectoryInfo dir)
    {
      if(Project.containsProject(dir))
      { 
        _selectedDir = dir.FullName + "/";
         if (File.Exists(_selectedDir + "report.csv"))
           _analysis.read(_selectedDir + "report.csv");
        string[] files = System.IO.Directory.GetFiles(dir.FullName, "*.bpr",
                         System.IO.SearchOption.TopDirectoryOnly);
        _prj.readPrjFile(files[0]);
      }
      else
        _prj = null;
    }

    public BitmapImage getFtImage(BatExplorerProjectFileRecordsRecord rec, out bool newImage)
    {
      string fullName = _selectedDir + "Records/" + rec.File;
      string pngName = fullName.Replace(".wav", ".png");
      Bitmap bmp = null;
      BitmapImage bImg = null;
      newImage = false;
      if (File.Exists(pngName))
      {
        bmp = new Bitmap(pngName);
      }
      else
      {
        Waterfall wf = new Waterfall(_selectedDir + "Records/" + rec.File, AppParams.FftWidth, AppParams.WaterfallWidth, AppParams.WaterfallHeight);
        if (wf.Ok)
        {
          wf.generateFtDiagram(0, (double)wf.Samples.Length / wf.SamplingRate, AppParams.FftWidth);
          bmp = wf.generateFtPicture(0, wf.SamplingRate/2000);
          bmp.Save(pngName);
          newImage = true;
        }
      }
      if(bmp != null)
        bImg = Convert(bmp);
      return bImg;
    }


    public void deleteFile(string wavName)
    {
      if (_prj != null)
      {
        string dirName = _selectedDir + "/Records";
        string delName = wavName.Replace(".wav", ".*");
        foreach (string f in Directory.EnumerateFiles(dirName, delName))
        {
          File.Delete(f);
        }

        _prj.removeFile(wavName);
        _prj.writePrjFile();
        _analysis.removeFile(_selectedDir, wavName);
      }
    }


    public void startEvaluation()
    {
      if(Prj != null)
      {
        string exe = "D:/bin/R-4.1.0/bin/Rscript.exe";
        string wrkDir = "D:/prj/bioacoustics";
        string args = "cm.R " + _selectedDir + "/Records " + _selectedDir + "/report.csv"; 
        _proc.LaunchCommandLineApp(exe, null, wrkDir, false, args, false, true);
      }
    }


    //http://www.shujaat.net/2010/08/wpf-images-from-project-resource.html
    static public BitmapImage Convert(Bitmap value)
    {
      MemoryStream ms = new MemoryStream();
      value.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
      BitmapImage image = new BitmapImage();
      image.BeginInit();
      ms.Seek(0, SeekOrigin.Begin);
      image.StreamSource = ms;
      image.EndInit();

      return image;
    }
  }

}
