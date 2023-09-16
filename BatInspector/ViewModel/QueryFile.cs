/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-07-02                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Serialization;

namespace BatInspector
{

  // 
  // Dieser Quellcode wurde automatisch generiert von xsd, Version=4.8.3928.0.
  // 


  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public class QueryFile
  {
    private string _name;

    private string _srcDir;

    private string _created;

    private string _query;

    private string _reportFile;

    private BatExplorerProjectFileRecordsRecord[] _records;

    public QueryFile()
    {
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Name {  get { return _name; }  set { _name = value; } }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string SrcDir { get { return _srcDir;} set { _srcDir = value; } }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Created { get { return _created; } set { _created = value; } }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Expression { get { return _query; }  set {_query = value; } }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string ReportFile { get { return _reportFile; } set {_reportFile = value; } }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [System.Xml.Serialization.XmlArrayItemAttribute("Record", typeof(BatExplorerProjectFileRecordsRecord), Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
    public BatExplorerProjectFileRecordsRecord[] Records { get { return _records; } set { _records = value; } }
  }
}
