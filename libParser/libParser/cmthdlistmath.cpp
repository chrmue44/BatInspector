#include "cmthdlistmath.h"
#include "dom_parser/clError.h"
#include "dom_parser/clVarList.h"
#include "dom_parser/cunittest.h"
#include <cmath>

CMthdListMath* CMthdListMath::m_Inst = NULL;

CMthdListMath::CMthdListMath():
m_pMethods(NULL)
{

}

CMthdListMath::~CMthdListMath() {
}

void CMthdListMath::initMthdTab()
{
  m_Inst = this;
  ADD_MTHD("sqrt", CMthdListMath::sqrt);
  ADD_MTHD("sin", CMthdListMath::sin);
  ADD_MTHD("asin", CMthdListMath::asin);
  ADD_MTHD("sinh", CMthdListMath::sinh);
  ADD_MTHD("cos", CMthdListMath::cos);
  ADD_MTHD("acos", CMthdListMath::acos);
  ADD_MTHD("cosh", CMthdListMath::cosh);
  ADD_MTHD("tan", CMthdListMath::tan);
  ADD_MTHD("atan", CMthdListMath::atan);
  ADD_MTHD("tanh", CMthdListMath::tanh);
  ADD_MTHD("pow", CMthdListMath::pow);
  ADD_MTHD("ln", CMthdListMath::ln);
  ADD_MTHD("exp", CMthdListMath::exp);
  ADD_MTHD("vars", CMthdListMath::vars);
  ADD_MTHD("consts", CMthdListMath::consts);
  ADD_MTHD("plot", CMthdListMath::plot);
  ADD_MTHD("abs", CMthdListMath::abs);
  ADD_MTHD("arg", CMthdListMath::arg);
  ADD_MTHD("savevars", CMthdListMath::savevars);
  ADD_MTHD("loadvars", CMthdListMath::loadvars);
}


const stHelpTabItem MathHelpTab[] =
{
  {"sqrt", "returns square root of a number",
    {
      "1: number", 0
    },
    {
      "1: square root of number",0
    }
  },
  {"sin", "returns sinus of a number",
    {
      "1: number", 0
    },
    {
      "1: sinus of number",0
    }
  },
  {"asin", "returns arcus sinus of a number",
    {
      "1: number", 0
    },
    {
      "1: arcus sinus of number",0
    }
  },
  {"sinh", "returns sinus hyperbolicus of a number",
    {
      "1: number", 0
    },
    {
      "1: sinus hyperbolicus of number",0
    }
  },
  {"cos", "returns cosinus of a number",
    {
      "1: number", 0
    },
    {
      "1: cosinus of number",0
    }
  },
  {"acos", "returns arcus cosinus of a number",
    {
      "1: number", 0
    },
    {
      "1: arcus cosinus of number",0
    }
  },
  {"cosh", "returns cosinus hyberbolicus of a number",
    {
      "1: number", 0
    },
    {
      "1: cosinus hyperbolicus of number",0
    }
  },
  {"tan", "returns tangens of a number",
    {
      "1: number", 0
    },
    {
      "1: tangens of number",0
    }
  },
  {"atan", "returns arcus tangens of a number",
    {
      "1: number", 0
    },
    {
      "1: arcus tangens of number",0
    }
  },
  {"tanh", "returns tangens hyperbolicus of a number",
    {
      "1: number", 0
    },
    {
      "1: tangens hyperolicus of number",0
    }
  },
  {"pow", "power",
    {
      "1: number",
      "2: exponent", 0
    },
    {
      "1: power of number",0
    }
  },
  {"ln", "logarithm",
    {
      "1: number", 0
    },
    {
      "1: logarithm of number",0
    }
  },
  {"abs", "absolut value",
    {
      "1: number", 0
    },
    {
      "1: absolut value of number(real or complex)",0
    }
  },
  {"arg", "argument of complex number",
    {
      "1: number", 0
    },
    {
      "1: argument of complex number",0
    }
  },
  {"exp", "exponential e^x",
    {
      "1: number", 0
    },
    {
      "1: e^x",0
    }
  },
  {"[vars", "dump list of variables",
    {
       0
    },
    {
      "1: lit of variables",0
    }
  },
  {"[consts", "dump list of constants",
    {
       0
    },
    {
      "1: list of constants",0
    }
  },
  { "cast","cast the type of an expression to the desired type",
    {
      "1 : Expr:   any valid expression",
      "2 : Type:   desired type\n"
      "            FLOAT = 1, INT32 = 2, UINT32 = 3, INT64 = 4,UINT64 = 5,\n"
      "            HEXVAL = 6,STR = 7, BOOL = 8",  0
    },
    {
      "1: result in desired type",
    }
  },
  {0,0,
    {0,0,0},
    {0,0,0},
  }
};


const stHelpTabItem* CMthdListMath::getHelpTab() {
  return MathHelpTab;
}


tPM_Char* CMthdListMath::getMthdListHelp() {
  return "\nlist of math commands";
}


int32_t CMthdListMath::sqrt(int32_t argcnt, clAnyType **argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 1) {
    if(argv[0]->getType() & TYPEFLAG_INT)
      argv[0]->changeType(RT_FLOAT);
    if((argv[0]->getType() == RT_FLOAT) && (argv[0]->getFloat() < 0))
      argv[0]->changeType(RT_COMPLEX);
    if(argv[0]->getType() == RT_FLOAT)
      result = std::sqrt(argv[0]->getFloat());
    else if(argv[0]->getType() == RT_COMPLEX) {
      double abs = argv[0]->getComplexAbs();
      double arg = argv[0]->getComplexArg();
      arg = arg/2.0 + M_PI;
      abs = std::sqrt(abs);
      result.setType(RT_COMPLEX);
      result.setComplexPolar(abs, arg);
    }
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;
}

int32_t CMthdListMath::sin(int32_t argcnt, clAnyType **argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 1) {
    argv[0]->changeType(RT_FLOAT);
    result = std::sin(argv[0]->getFloat());
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;
}

int32_t CMthdListMath::sinh(int32_t argcnt, clAnyType **argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 1) {
    argv[0]->changeType(RT_FLOAT);
    result = std::sinh(argv[0]->getFloat());
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;
}

int32_t CMthdListMath::asin(int32_t argcnt, clAnyType **argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 1) {
    argv[0]->changeType(RT_FLOAT);
    result = std::asin(argv[0]->getFloat());
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;
}

int32_t CMthdListMath::cos(int32_t argcnt, clAnyType **argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 1) {
    argv[0]->changeType(RT_FLOAT);
    result = std::cos(argv[0]->getFloat());
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;
}

int32_t CMthdListMath::cosh(int32_t argcnt, clAnyType **argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 1) {
    argv[0]->changeType(RT_FLOAT);
    result = std::cosh(argv[0]->getFloat());
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;
}

int32_t CMthdListMath::acos(int32_t argcnt, clAnyType **argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 1) {
    argv[0]->changeType(RT_FLOAT);
    result = std::acos(argv[0]->getFloat());
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;
}

int32_t CMthdListMath::tan(int32_t argcnt, clAnyType **argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 1) {
    argv[0]->changeType(RT_FLOAT);
    result = std::tan(argv[0]->getFloat());
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;
}

int32_t CMthdListMath::tanh(int32_t argcnt, clAnyType **argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 1) {
    argv[0]->changeType(RT_FLOAT);
    result = std::tanh(argv[0]->getFloat());
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;
}

int32_t CMthdListMath::atan(int32_t argcnt, clAnyType **argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 1) {
    argv[0]->changeType(RT_FLOAT);
    result = std::atan(argv[0]->getFloat());
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;
}

int32_t CMthdListMath::pow(int32_t argcnt, clAnyType **argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 2) {
    argv[0]->changeType(RT_FLOAT);
    argv[1]->changeType(RT_FLOAT);
    result = std::pow(argv[0]->getFloat(), argv[1]->getFloat());
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;
}

int32_t CMthdListMath::vars(int32_t argcnt, clAnyType **argv, clAnyType& result) {
  int32_t err = 0;
  std::string& str = m_Inst->m_pVarList->dumpVarList(false);
  result = str.c_str();
  return err;
}

int32_t CMthdListMath::consts(int32_t argcnt, clAnyType **argv, clAnyType& result) {
  int32_t err = 0;
  std::string& str = m_Inst->m_pVarList->dumpVarList(true);
  result = str.c_str();
  return err;
}

int32_t CMthdListMath::ln(int32_t argcnt, clAnyType** argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 1) {
    argv[0]->changeType(RT_FLOAT);
    result = std::log(argv[0]->getFloat());
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;
}

int32_t CMthdListMath::exp(int32_t argcnt, clAnyType** argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 1) {
    if(argv[0]->getType() == RT_COMPLEX) {
      double re = std::exp(argv[0]->getComplexRe()) * std::cos(argv[0]->getComplexIm());
      double im = std::exp(argv[0]->getComplexRe()) * std::sin(argv[0]->getComplexIm());
      result.setType(RT_COMPLEX);
      result.setComplex(re, im);
    }
    else {
      argv[0]->changeType(RT_FLOAT);
      result = std::exp(argv[0]->getFloat());
    }
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;
}

int32_t CMthdListMath::arg(int32_t argcnt, clAnyType** argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 1) {
    switch(argv[0]->getType()) {
      case RT_COMPLEX:
        result = argv[0]->getComplexArg();
        break;
      case RT_FLOAT:
      case RT_INT64:
      case RT_UINT64:
        result = 0.0;
        break;
    default:
      err = ERR_ARG_NOT_SUPPORTED;
    }
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;
}

int32_t CMthdListMath::cast(int32_t argcnt, clAnyType** argv, clAnyType& result)
{
  int32_t err = 0;

  if(argcnt == 2)
  {
    result = *argv[0];
    tType Type = static_cast<tType>(argv[1]->getUint64());
    result.changeType(Type);
  }
  else
  {
    err = ERR_NR_OF_ARGUMENTS;
  }
  return err;
}

int32_t CMthdListMath::abs(int32_t argcnt, clAnyType** argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 1) {
    switch(argv[0]->getType()) {
      case RT_FLOAT:
        result = std::fabs(argv[0]->getFloat());
        break;

      case RT_INT64:
        result = std::abs(argv[0]->getInt64());
        break;

      case RT_UINT64:
        result = argv[0]->getUint64();
        break;

      case RT_HEXVAL:
        result = argv[0]->getUint64();
        result.changeType(RT_HEXVAL);
        break;

      case RT_COMPLEX:
        result = argv[0]->getComplexAbs();
        break;

      default:
        err = ERR_ABS_NOT_SUPPORTED;
    }
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;
}

int32_t CMthdListMath::plot(int32_t argcnt, clAnyType** argv, clAnyType& result) {
  int32_t err = 0;
  if(argcnt == 5) {
    argv[0]->changeType(RT_STR);      ///< name of variable with formula
    argv[1]->changeType(RT_STR);      ///< name of variable with argument
    argv[2]->changeType(RT_FLOAT);    ///< min value for argument
    argv[3]->changeType(RT_FLOAT);    ///< max value for argument
    argv[4]->changeType(RT_UINT64);   ///< nr of steps
    do{
      clAnyType func;
      clAnyType funcArg;
      std::string resultString;
      clName* nameFunc = m_Inst->m_pVarList->look(argv[0]->getString());
      if(nameFunc)
        err = nameFunc->getValue(0, func);
      else
        err = ERR_VARIABLE;
      if(err)
        break;

      clName* nameFuncArg = m_Inst->m_pVarList->look(argv[1]->getString());
      if(nameFuncArg)
        err = nameFuncArg->getValue(0, funcArg);
      else
        err = ERR_VARIABLE;
      if(err)
        break;
      resultString = "";
      clAnyType step;
      step = (argv[3]->getFloat() - argv[2]->getFloat()) / (argv[4]->getUint64());
      clAnyType arg;
      arg = argv[2]->getFloat();
      for( ; arg.getFloat() <= argv[3]->getFloat(); arg += step) {
        nameFuncArg->setValue(0, arg);
        nameFunc->getValue(0, func);
        func.changeType(RT_STR);
        resultString += "\n";
        resultString += nameFunc->getName();
        resultString +="(";
        nameFuncArg->getValue(0, funcArg);
        funcArg.changeType(RT_STR);
        resultString += funcArg.getString();
        resultString +=")= ";
        resultString += func.getString();
      }
      result = resultString.c_str();
    } while(0);
  }
  else
    err = ERR_NR_OF_ARGUMENTS;
  return err;

}

int32_t CMthdListMath::savevars(int32_t argcnt, clAnyType** argv, clAnyType& result) {
  int32_t err = 0;
  m_Inst->m_pVarList->save(FILENAME_VARS);

  return err;
}

int32_t CMthdListMath::loadvars(int32_t argcnt, clAnyType** argv, clAnyType& result) {
  int32_t err = 0;
  m_Inst->m_pVarList->load(FILENAME_VARS, m_Inst->m_pMethods, false);

  return err;
}
