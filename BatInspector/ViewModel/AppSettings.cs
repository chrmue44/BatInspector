using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using gsiCtrls;
using System.Windows.Forms;

namespace BatInspector
{
  [DataContract]
  public class AppSettings
  {
    const string _fName = "BatInspectorSettings.json";

    public AppSettings()
    {
      init();
    }

    [DataMember]
    [Category("Application")]
    [Description("force usage of MS Excel for export functions")]
    public bool UseExcel { get; set; }

    [DataMember]
    [Category("Application")]
    [Description("generate copy of original file with prefix 'gen_' for material and address list")]
    public bool GenerateCopy { get; set; }

    [DataMember]
    [Category("Application")]
    [Description("Width of main window [pixel]")]
    public int MainWindowWidth { get; set; }

    [DataMember]
    [Category("Application")]
    [Description("Height of main window [pixel]")]
    public int MainWindowHeight { get; set; }

    [DataMember]
    [Category("Application")]
    [Description("Width of log window [pixel]")]
    public int LogWindowWidth { get; set; }

    [DataMember]
    [Category("Application")]
    [Description("Height of log window [pixel]")]
    public int LogWindowHeight { get; set; }

    [DataMember]
    [Category("Application")]
    [Description("Allow to open project file in Excel while opening it with RackPlan")]
    public bool AllowOpenExcel { get; set; }

    [DataMember]
    [Category("Material List")]
    [Description("create BOM in material list")]
    public bool CreateBOM { get; set; }

    [DataMember]
    [Category("EPLAN")]
    [Description("add prefix in PLC address replacement '=.' when hierarchy level eqauls '0' ==> e.g. result: '=.-AK11'")]
    public bool AddPlcAddrPrefix { get; set; }

    [DataMember]
    [Category("Wizard")]
    [Description("installation path of GSI import tool")]
    public string GsiInstPath { get; set; }

    [DataMember]
    [Category("EPLAN.ExportDeviceTag")]
    [Description("column on page 'EplSheet' for page name (containing device tag)")]
    public int ColbPageName { get; set; }

    [DataMember]
    [Category("EPLAN.ExportDeviceTag")]
    [Description("column on page 'EplSheet' for full device tag")]
    public int ColbFullDt { get; set; }

    [DataMember]
    [Category("EPLAN.ExportDeviceTag")]
    [Description("column on page 'EplSheet' for new device tag")]
    public int ColbDt { get; set; }

    [DataMember]
    [Category("EPLAN.ExportDeviceTag")]
    [Description("column on page 'EplSheet' for main function")]
    public int ColbMainFunc { get; set; }

    [DataMember]
    [Category("EPLAN.ExportDeviceTag")]
    [Description("column on page 'EplSheet' for main terminal")]
    public int ColbMainTerm { get; set; }

    [DataMember]
    [Category("EPLAN.ExportDeviceTag")]
    [Description("column on page 'EplSheet' for function text")]
    public int ColbFuncTextNew { get; set; }

    [DataMember]
    [Category("EPLAN.ExportDeviceTag")]
    [Description("column on page 'EplSheet' for pin name")]
    public int ColbPinName { get; set; }

    [DataMember]
    [Category("EPLAN.ExportDeviceTag")]
    [Description("column on page 'EplSheet' for X coordinate")]
    public int ColbXcoord { get; set; }


    [DataMember]
    [Category("EPLAN.ExportPlcAddress")]
    [Description("column on page 'EplSheet' for page name (containing device tag)")]
    public int ColePageName { get; set; }

    [DataMember]
    [Category("EPLAN.ExportPlcAddress")]
    [Description("column on page 'EplSheet' for new device tag")]
    public int ColeDt { get; set; }

    [DataMember]
    [Category("EPLAN.ExportPlcAddress")]
    [Description("column on page 'EplSheet' for PLC address")]
    public int ColePlcAddr { get; set; }

    [DataMember]
    [Category("EPLAN.ExportPlcAddress")]
    [Description("column on page 'EplSheet' for pin number")]
    public int ColePinNr { get; set; }

    [DataMember]
    [Category("EPLAN.ExportPlcAddress")]
    [Description("column on page 'EplSheet' for pin function")]
    public int ColePinFunc { get; set; }

    [DataMember]
    [Category("EPLAN.ExportPlcAddress")]
    [Description("column on page 'EplSheet' for new channel number (0..n)")]
    public int ColeChanNr { get; set; }

    [DataMember]
    [Category("EPLAN.ExportPlcAddress")]
    [Description("column on page 'EplSheet' for function text")]
    public int ColeFuncText { get; set; }

    [DataMember]
    [Category("EPLAN.ExportPlcAddress")]
    [Description("column on page 'EplSheet' for new function text")]
    public int ColeFuncTextNew { get; set; }

    [DataMember]
    [Category("EPLAN.ExportPlcAddress")]
    [Description("column on page 'EplSheet' for X coordinate")]
    public int ColeXccord { get; set; }

    [DataMember]
    [Category("EPLAN.ExportShield")]
    [Description("column on page 'EplSheet' for page name (containing device tag)")]
    public int ColsPageName { get; set; }

    [DataMember]
    [Category("EPLAN.ExportShield")]
    [Description("column on page 'EplSheet' for device tag")]
    public int ColsDt { get; set; }

    [DataMember]
    [Category("EPLAN.ExportShield")]
    [Description("column on page 'EplSheet' for function text")]
    public int ColsFunction { get; set; }

    [DataMember]
    [Category("EPLAN.ExportShield")]
    [Description("column on page 'EplSheet' for Xcoord")]
    public int ColsXcoord { get; set; }


    [DataMember]
    [Category("EPLAN.ExportFunctionText")]
    [Description("column on page 'EplSheet' for page name")]
    public int ColfPage { get; set; }

    [DataMember]
    [Category("EPLAN.ExportFunctionText")]
    [Description("column on page 'EplSheet' for function text")]
    public int ColfText { get; set; }

    [DataMember]
    [Category("EPLAN.ExportFunctionText")]
    [Description("column on page 'EplSheet' for X coordinate")]
    public int ColfXcoord { get; set; }

    [DataMember]
    [Category("EPLAN.ExportStructureBox")]
    [Description("column on page 'EplSheet' for page name (containing device tag)")]
    public int ColoPageName { get; set; }

    [DataMember]
    [Category("EPLAN.ExportStructureBox")]
    [Description("column on page 'EplSheet' for device tag identifying)")]
    public int ColoDtIdent { get; set; }

    [DataMember]
    [Category("EPLAN.ExportStructureBox")]
    [Description("column on page 'EplSheet' for new device tag")]
    public int ColoDtNew { get; set; }

    [DataMember]
    [Category("EPLAN.ExportStructureBox")]
    [Description("column on page 'EplSheet' for function text")]
    public int ColoFuncText { get; set; }

    [DataMember]
    [Category("EPLAN.ExportStructureBox")]
    [Description("column on page 'EplSheet' for X coordinate")]
    public int ColoXcoord { get; set; }


    [DataMember]
    [Category("EPLAN.ExportConnections")]
    [Description("column on page 'EplSheet' for device tag")]
    public int ColcDt { get; set; }

    [DataMember]
    [Category("EPLAN.ExportConnections")]
    [Description("column on page 'EplSheet' for source (containing device tag)")]
    public int ColcSrc { get; set; }

    [DataMember]
    [Category("EPLAN.ExportConnections")]
    [Description("column on page 'EplSheet' for destination (containing device tag")]
    public int ColcDest { get; set; }

    [DataMember]
    [Category("EPLAN.ExportConnections")]
    [Description("column on page 'EplSheet' for wire color")]
    public int ColcColor { get; set; }

    [DataMember]
    [Category("EPLAN.ExportConnections")]
    [Description("column on page 'EplSheet' for pair index")]
    public int ColcPairIndex { get; set; }

    [DataMember]
    [Category("EPLAN.ExportConnections")]
    [Description("column on page 'EplSheet' for marker cable connection")]
    public int ColcIsCable { get; set; }


    [DataMember]
    [Category("EPLAN.ExportTerminals")]
    [Description("column on page 'EplSheet' for page name (containing device tag)")]
    public int ColxPageName { get; set; }

    [DataMember]
    [Category("EPLAN.ExportTerminals")]
    [Description("column on page 'EplSheet' for identifying device tag")]
    public int ColxDtIdent { get; set; }

    [DataMember]
    [Category("EPLAN.ExportTerminals")]
    [Description("column on page 'EplSheet' for visible device tag")]
    public int ColxDtVis { get; set; }

    [DataMember]
    [Category("EPLAN.ExportTerminals")]
    [Description("column on page 'EplSheet' for terminal position")]
    public int ColxPosition { get; set; }

    [DataMember]
    [Category("EPLAN.ExportTerminals")]
    [Description("column on page 'EplSheet' for terminal level")]
    public int ColxTermLevel { get; set; }

    [DataMember]
    [Category("EPLAN.ExportTerminals")]
    [Description("column on page 'EplSheet' for terminal main function")]
    public int ColxTermMain { get; set; }

    [DataMember]
    [Category("EPLAN.ExportTerminals")]
    [Description("column on page 'EplSheet' for pin number")]
    public int ColxPin { get; set; }

    [DataMember]
    [Category("EPLAN.ExportTerminals")]
    [Description("column on page 'EplSheet' for function text")]
    public int ColxFunction { get; set; }

    [DataMember]
    [Category("EPLAN.ExportTerminals")]
    [Description("column on page 'EplSheet' for X coordinate")]
    public int ColxXcoord { get; set; }

    public void init()
    {
      UseExcel = false;
      GenerateCopy = true;

      CreateBOM = true;
      AllowOpenExcel = false;

      MainWindowWidth = 980;
      MainWindowHeight = 666;
      LogWindowWidth = 980;
      LogWindowHeight = 340;
      AddPlcAddrPrefix = false;
      GsiInstPath = "C:\\Program Files (x86)\\GSI\\Spec File Creator";

      ColbPageName = 5;           //column on page "EplSheet" for page name (containing device tag)
      ColbFullDt = 6;             //column on page "EplSheet" for full device tag
      ColbDt = 7;                 //column on page "EplSheet" for new device tag
      ColbMainFunc = 10;          //column on page "EplSheet" for main function
      ColbMainTerm = 11;          //column on page "EplSheet" for main terminal
      ColbFuncTextNew = 13;       //column on page "EplSheet" for function text
      ColbPinName = 15;
      ColbXcoord = 16;            //column on page "EplSheet" for X coordinate

      ColePageName = 5;           //column on page "EplSheet" for page name (containing device tag)
      ColeDt = 7;                 //column on page "EplSheet" for new device tag
      ColePlcAddr = 8;            //column on page "EplSheet" for PLC address
      ColePinNr = 9;              //column on page "EplSheet" for pin number
      ColePinFunc = 10;           //column on page "EplSheet" for pin function
      ColeChanNr = 12;            //column on page "EplSheet" for new channel number (0..n)
      ColeFuncText = 13;          //column on page "EplSheet" for function text
      ColeFuncTextNew = 14;       //column on page "EplSheet" for function text
      ColeXccord = 15;            //column on page "EplSheet" for X coordinate

      ColsPageName = 5;           //column on page "EplSheet" for page name (containing device tag)
      ColsDt = 7;                 //column on page "EplSheet" for device tag
      ColsFunction = 9;           //column on page "EplSheet" for function text
      ColsXcoord = 13;            //column on page "EplSheet" for shield

      ColfPage = 2;
      ColfText = 4;
      ColfXcoord = 5;

      ColoPageName = 2;           //column on page "EplSheet" for page name (containing device tag)
      ColoDtIdent = 3;            //column on page "EplSheet" for device tag identifying
      ColoDtNew = 4;              //column on page "EplSheet" for new device tag
      ColoFuncText = 5;           //column on page "EplSheet" for function text
      ColoXcoord = 7;             //column on page "EplSheet" for X coordinate

      ColcDt = 6;                 //column on page "EplSheet" for device tag
      ColcSrc = 8;                //column on page "EplSheet" for source (containing device tag)
      ColcDest = 7;               //column on page "EplSheet" for destination (containing device tag)
      ColcColor = 10;
      ColcPairIndex = 11;         //column on page "EplSheet" for pair index
      ColcIsCable = 14;           //column on page "EplSheet" for marker cable connection

      ColxPageName = 5;           //column on page "EplSheet" for page name (containing device tag)
      ColxDtIdent = 6;            //column on page "EplSheet" for identifying device tag
      ColxDtVis = 7;              //column on page "EplSheet" for visible device tag
      ColxPosition = 8;           //column on page "EplSheet" for terminal position
      ColxTermLevel = 9;          //column on page "EplSheet" for terminal level
      ColxTermMain = 10;          //column on page "EplSheet" for terminal main function
      ColxPin = 12;               //column on page "EplSheet" for pin number
      ColxFunction = 14;          //column on page "EplSheet" for function text
      ColxXcoord = 15;            //column on page "EplSheet" for X coordinate

    }

    public void save()
    {
      string fPath = Application.LocalUserAppDataPath + "\\" + _fName;
      try
      {
        StreamWriter file = new StreamWriter(fPath);
        MemoryStream stream = new MemoryStream();
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AppSettings));
        ser.WriteObject(stream, this);
        StreamReader sr = new StreamReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        string str = sr.ReadToEnd();
        file.Write(JsonHelper.FormatJson(str));
        file.Close();
      }
      catch (Exception e)
      {
        Glob.log("failed to write config file for BatInspector" + fPath + ": " + e.ToString(), enLogType.RESULT_NOK);
      }
    }

    public static AppSettings load()
    {
      AppSettings retVal = null;
      FileStream file = null;
      string fPath = Application.LocalUserAppDataPath + "\\" + _fName;
      try
      {
        if (File.Exists(fPath))
        {
          file = new FileStream(fPath, FileMode.Open);
          DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AppSettings));
          retVal = (AppSettings)ser.ReadObject(file);
        }
        else
        {
          retVal = new AppSettings();
          retVal.init();
          retVal.save();
        }
      }
      catch (Exception e)
      {
        Glob.log("failed to read config file : " + fPath + ": " + e.ToString(), enLogType.RESULT_NOK);
        retVal = null;
      }
      finally
      {
        if (file != null)
          file.Close();
      }

      return retVal;
    }
  }
}
