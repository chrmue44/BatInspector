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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libParser
{
  public enum tParseError
  {
    SUCCESS = 0,
    FOPEN = -50100,  ///< error opening file
    NO_TEST_OPEN = -50200,  ///< no test file opened
    EOF = -50250,  ///< end of file reached
    MODULE = -50300,  ///< no test module loaded
    PARSESTRING = -50310, ///< no string to parse found
    PARSESTRING_END = -50311, ///< end of string reached
    BAD_TOKEN = -50320, ///< Bad token
    BRACECLOSE = -50350,  ///< expected closing brace
    SQR_BRACK_CLOSE = -50360,  ///< expected closing square brace
    PRIMARY = -50400,  ///< error in primary expression
    DIV = -50450,  ///< division by zero
    VARIABLE = -50460,  ///< variable not found
    ARRAY_INDEX = -50470,  ///< invalid array index
    STRING_DIV = -50500,  ///< division of strings not possible
    BOOL_SUB = -50510,  ///< substraction of bools not possible
    STRING_SUB = -50520,  ///< substraction of strings not possible
    STRING_MUL = -50530,  ///< multiplication of strings not possible
    STRING_OR = -50540,  ///< nor OR operand for strings
    STRING_AND = -50545,  ///< nor AND operand for strings
    STRING_NOT = -50550,  ///< nor NOT operand for strings
    FLOAT_OR = -50600,  ///< nor OR operand for floats
    FLOAT_AND = -50605,  ///< nor AND operand for floats
    FLOAT_NOT = -50610,  ///< nor NOT operand for floats
    PLUS_NOT_SUPPORTED = -50620,  ///< operator + not supported for this data type
    MINUS_NOT_SUPPORTED = -50622,  ///< operator - not supported for this data type
    MUL_NOT_SUPPORTED = -50624,  ///< operator * not supported for this data type
    DIV_NOT_SUPPORTED = -50626,  ///< operator / not supported for this data type
    AND_NOT_SUPPORTED = -50628,  ///< operator & not supported for this data type
    OR_NOT_SUPPORTED = -50628,  ///< operator | not supported for this data type
    NOT_NOT_SUPPORTED = -50628,  ///< operator ! not supported for this data type
    KOMMA = -50700,  ///< comma expected
    METHOD = -50800,  ///< function unknown
    NR_OF_ARGUMENTS = -50810,  ///< wrong number of arguments
    XML_DIFFERENT = -50815,  ///< difference in XML files
    ROW_INVALID = -50820,  ///< invalid row nuber in file
    COL_INVALID = -50825,  ///< invalid column nuber in file
    ARG1_OUT_OF_RANGE = - 50831, ///< argument 1 out of range
    ARG2_OUT_OF_RANGE = -50832, ///< argument 2 out of range
    ARG3_OUT_OF_RANGE = -50833, ///< argument 3 out of range
    ARG4_OUT_OF_RANGE = -50834, ///< argument 4 out of range
    ARG5_OUT_OF_RANGE = -50835, ///< argument 5 out of range
    HEXBUF = -50900,  ///< HEX buffer too small for serializing
    SERTYPE = -50910,  ///< Type not supported for serializing
    SERFMT = -50920,  ///< error format HEX-String for serializer
    HEXASC_HDR = -50930,  ///< error in HEX_ASC_Header (bytecount)
    RESULTSTRING = -50940,  ///< erroneous result string
    ASSIGN_CONST = -50950,  ///< assignment of consts not allowed
    UTIL_LANGUAGE = -51000,  ///< language not supported
    COMPLEX_COMMA = -51100,  ///< comma expected
    CRL_BRACE_CLOSE = -51110,  ///< closing curly brace expected
    ABS_NOT_SUPPORTED = -51120,  ///< function abs() not supported for data type og argument
    ARG_NOT_SUPPORTED = -51125,  ///< function arg() not supported for data type og argument
    UNEQ_NOT_SUPPORTED = -51130,  ///< operator unequal not supported for this data type
    LESS_NOT_SUPPORTED = -51140,  ///< operator less not supported for this data type
    LESSEQ_NOT_SUPPORTED = -51145,  ///< operator less_eq not supported for this data type
    GREATER_NOT_SUPPORTED = -51150,  ///< operator greater not supported for this data type
    GREATER_EQ_NOT_SUPPORTED = -51155, ///< operator greater_eq not supported for this data type
    TYPE_CONVERSION = -51160,  ///< type conversion not possible
    ARGUMENT_TYPE_ARG1 = -51171, ///< type of argument 1 wrong
    ARGUMENT_TYPE_ARG2 = -51172, ///< type of argument 2 wrong
    ARGUMENT_TYPE_ARG3 = -51173, ///< type of argument 3 wrong
    ARGUMENT_TYPE_ARG4 = -51174, ///< type of argument 4 wrong
    ARGUMENT_TYPE_ARG5 = -51175, ///< type of argument 5 wrong
    NOT_IMPLEMENTED = -52000,  ///< not implemented
  };


  public class Error
  {
    public Error()
    {

    }
    static public void report(tParseError error)
    {
      m_Last = error;
    }

    static public void report(int error)
    {
      m_Last = (tParseError)error;
    }
    public void report(tParseError Error, uint Line)
    {

    }
    public static void init()
    {
      m_Last = tParseError.SUCCESS;
    }

    public static tParseError get()
    {
      return m_Last;
    }

    // letzter Fehler
    private static tParseError m_Last;
  };
}
