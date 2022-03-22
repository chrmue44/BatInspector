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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace libScripter
{

  /// <summary>
  /// helper class to display the content oa a variable in a property grid
  /// </summary>
  public class VarItemTypeConverter : ExpandableObjectConverter
  {
    public override bool CanConvertFrom(ITypeDescriptorContext context,
    Type sourceType)
    {

      if (sourceType == typeof(string))
      {
        return true;
      }
      return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context,
      CultureInfo culture, object value)
    {
      if (value is string)
      {
        string[] v = ((string)value).Split(new char[] { ',' });
        return new VariableItem(v[0], v[1]);
      }
      return base.ConvertFrom(context, culture, value);
    }

    // Overrides the ConvertTo method of TypeConverter.
    public override object ConvertTo(ITypeDescriptorContext context,
       CultureInfo culture, object value, Type destinationType)
    {
      if (destinationType == typeof(string))
      {
        return ((VariableItem)value).Name + "," + ((VariableItem)value).Value;
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }
  }

  

  /// <summary>
  /// representation of a variable including name and value
  /// </summary>
  [Browsable(true), TypeConverterAttribute(typeof(VarItemTypeConverter)), CategoryAttribute("Variables"), DescriptionAttribute("list of defined variables")]
  public class VariableItem
  {
    public VariableItem(string name, string value)
    {
      Name = name;
      Value = value;
    }

    [CategoryAttribute("Variables"), DescriptionAttribute("Variable value")]
    public string Value { get; set; }
    [CategoryAttribute("Variables"), DescriptionAttribute("Variable Name")]
    public string Name { get; }

    protected VariableItem()
    {

    }
  }

  public class Variables
  {

    List<VariableItem> _list;
    
    public Variables()
    {
      _list = new List<VariableItem>();
    }

    public List<VariableItem> VarList { get { return _list; } }

    public void SetValue(string name, string value)
    {
      string varName = name.Replace("%", "");
      VariableItem item = Find(name);
      if (item == null)
      {
        VariableItem var = new VariableItem(varName, value);
        _list.Add(var);
      }
      else
        item.Value = value;
    }

    public string GetValue(string name)
    {
      VariableItem v = Find(name);
      if (v != null)
        return v.Value;
      else
        return "";
    }

    public VariableItem Find(string name)
    {
      foreach(VariableItem v in _list)
      {
        string varName = name.Replace("%", "");
        if (v.Name == varName)
          return v;
      }
      return null;
    }

    public void Remove(string name)
    {
      VariableItem v = Find(name);
      if (v != null)
        _list.Remove(v);
    }
  }
}
