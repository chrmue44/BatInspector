// die Texte muessen aufsteigend (nach Texthandles) sortiert sein, weil
// der Suchalgorithmus dann auf binaere Suche optimiert werden kann

#include "util/cltexthandler.h"
#include "dom_parser/clError.h"

const tTxtEntry EnglishTab[] =
{
  {ERR_FOPEN                 , "error opening file" },
  {ERR_NO_TEST_OPEN          , "no test file opened"},
  {ERR_EOF                   , "end of file reached" },
  {ERR_MODULE                , "no test module loaded" },
  {ERR_BRACECLOSE            , "expected closing brace" },
  {ERR_SQR_BRACK_CLOSE       , "expected closing square brace" },
  {ERR_PRIMARY               , "in primary expression" },
  {ERR_DIV                   , "division by zero" },
  {ERR_VARIABLE              , "variable not found" },
  {ERR_ARRAY_INDEX           , "invalid array index" },
  {ERR_STRING_DIV            , "division of strings not possible" },
  {ERR_BOOL_SUB              , "substraction of bools not possible" },
  {ERR_STRING_SUB            , "substraction of strings not possible" },
  {ERR_STRING_MUL            , "multiplication of strings not possible" },
  {ERR_STRING_OR             , "nor OR operand for strings" },
  {ERR_STRING_AND            , "nor AND operand for strings" },
  {ERR_STRING_NOT            , "nor NOT operand for strings" },
  {ERR_FLOAT_OR              , "nor OR operand for floats" },
  {ERR_FLOAT_AND             , "nor AND operand for floats" },
  {ERR_FLOAT_NOT             , "nor NOT operand for floats" },
  {ERR_PLUS_NOT_SUPPORTED    , "operator + not supported for this data type" },
  {ERR_MINUS_NOT_SUPPORTED   , "operator - not supported for this data type" },
  {ERR_MUL_NOT_SUPPORTED     , "operator * not supported for this data type" },
  {ERR_DIV_NOT_SUPPORTED     , "operator / not supported for this data type" },
  {ERR_AND_NOT_SUPPORTED     , "operator & not supported for this data type" },
  {ERR_OR_NOT_SUPPORTED      , "operator | not supported for this data type" },
  {ERR_NOT_NOT_SUPPORTED     , "operator ! not supported for this data type" },
  {ERR_KOMMA                 , "comma expected" },
  {ERR_METHOD                , "function unknown" },
  {ERR_NR_OF_ARGUMENTS       , "wrong number of arguments" },
  {ERR_XML_DIFFERENT         , "difference in XML files" },
  {ERR_ROW_INVALID           , "invalid row nuber in file" },
  {ERR_COL_INVALID           , "invalid column nuber in file" },
  {ERR_HEXBUF                , "HEX buffer too small for serializing" },
  {ERR_SERTYPE               , "Type not supported for serializing" },
  {ERR_SERFMT                , "error format HEX-String for serializer" },
  {ERR_HEXASC_HDR            , "error in HEX_ASC_Header (bytecount)" },
  {ERR_RESULTSTRING          , "erroneous result string" },
  {ERR_ASSIGN_CONST          , "assignment of consts not allowed" },
  {ERR_UTIL_LANGUAGE         , "language not supported" },
  {ERR_COMPLEX_COMMA         , "comma expected"},
  {ERR_CRL_BRACE_CLOSE       , "closing curly brace expected" },
  {ERR_ABS_NOT_SUPPORTED     , "function abs() not supported for data type og argument" },
  {ERR_ARG_NOT_SUPPORTED     , "function arg() not supported for data type og argument" },
  {ERR_UNEQ_NOT_SUPPORTED    , "operator unequal not supported for this data type" },
  {ERR_LESS_NOT_SUPPORTED    , "operator less not supported for this data type" },
  {ERR_LESSEQ_NOT_SUPPORTED  , "operator less_eq not supported for this data type" },
  {ERR_GREATER_NOT_SUPPORTED , "operator greater not supported for this data type" },
  {ERR_GREATER_EQ_NOT_SUPPORTED, "operator greater_eq not supported for this data type" },
  {ERR_TYPE_CONVERSION       , "type conversion not possible" },
  {ERR_NOT_IMPLEMENTED       , "not implemented" },

  {0,""},
};

const tTxtEntry* parseTextTabs[] =
{
  EnglishTab,
/*  GermanTab,
  FrenchTab,
  SpanishTab,
  ItalianTab, */
  NULL
};
