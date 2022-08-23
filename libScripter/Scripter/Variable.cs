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
using libParser;
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

    VarList _list;

    public Variables()
    {
      _list = new VarList();
    }

    public VarList VarList { get { return _list; } }

/*    public void SetValue(string name, string value)
    {
      string varName = name.Replace("%", "");
      _list.set(name, value);
    }
*/
    public string GetValue(string name, int index = 0)
    {
      VarName v =_list.get(name);
      string retVal = "";
      if (v != null)
      {
        AnyType value = new AnyType();
        v.getValue(index, ref value);
        value.changeType(AnyType.tType.RT_STR);
        retVal = value.getString();
      }
      return retVal;
    } 

    public VariableItem Find(string name, int index = 0)
    {
      VariableItem retVal = null;
      VarName v = _list.get(name);
      if (v != null)
      {
        AnyType value = new AnyType();
        v.getValue(index, ref value);
        value.changeType(AnyType.tType.RT_STR);
        string valStr = value.getString();
        retVal = new VariableItem(v.getName(), valStr);
      }
      return retVal;
    }

    public void Remove(string name)
    {
      _list.remove(name);
    }
  }
}
