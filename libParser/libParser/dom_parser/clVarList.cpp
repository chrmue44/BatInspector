/*********************************************************
 *         project: ProcessMon                           *
 *                                                       *
 *              (C) 2010 Christian Mueller               *
 *                       Odenwaldstrasse 134g            *
 *                       D-64372 Ober-Ramstadt           *
 *                                                       *
 *********************************************************/

#include "clVarList.h"
#include "clError.h"
#include "os/objhndlglib.h"
#include "util/utils.h"

#include <cstring>
#include <fstream>
#include <cstdlib>

clName::clName(clVarList* pVarList, clMethods* pMethods, bool isConst) :
m_pVarList(pVarList),
m_pMethods(pMethods),
m_isConst(isConst)
{
  clAnyType p(m_pVarList, m_pMethods);
  m_Value.push_back(p);
}

clName::~clName()
{

}

int32_t clName::getValue(uint32_t Index, clAnyType& Val)
{
   int32_t RetVal = ERR_ARRAY_INDEX;
  if(Index < m_Value.size())
  {
    Val = m_Value[Index];
    RetVal = 0;
  }
  else
    Val = static_cast<double>(RetVal);
  return RetVal;
}


int32_t clName::setValue(uint32_t Index, clAnyType Val)
{
  int32_t RetVal = 0;
  clAnyType p(m_pVarList, m_pMethods);
  while(Index >= m_Value.size())
    m_Value.push_back(p);
  m_Value[Index] = Val;
  return RetVal;
}

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

clVarList::clVarList()
{
   uint32_t i;
  for(i = 0; i < TBLSZ; i++)
  {
    m_Table[i] = NULL;
  }
}

clVarList::~clVarList()
{
  uint32_t i;
  for(i = 0; i < TBLSZ; i++)
  {
    if(m_Table[i] != NULL)
    {
      clName* p = m_Table[i];
      clName* pn = m_Table[i]->Next;
      while(p != NULL)
      {
        pn = p->Next;
        char* pname = p->getName();
        DELETE_OBJECT_ARR(pn,"clName::char[]");
        p->setName(pname);
        DELETE_TYPED_OBJECT(clName, p, "clName");
        p = pn;
      }
    }
  }

}

std::string& clVarList::dumpVarList(bool isConst) {
  if(isConst)
    m_listStr  = "LIST OF CONSTANTS: \n";
  else
    m_listStr = "LIST OF VARIABLES: \n";
  for(int i = 0; i < TBLSZ; i++) {
    if(m_Table[i] != NULL) {
      if(m_Table[i]->isConst() != isConst)
        continue;
      clAnyType val(this, m_pMethods);
      /*int32_t errCode = */m_Table[i]->getValue(0, val);
      m_listStr += m_Table[i]->getName();
      m_listStr += " = ";
      std::string formulaString;
      bool isFormula= false;
      const char* type = val.getTypeString();
      if(val.getType() == RT_FORMULA) {
        formulaString = val.getString();
        isFormula = true;
      }
      val.changeType(RT_STR);
      m_listStr += val.getString();
      m_listStr += "  ";
      m_listStr += type;
      if(isFormula) {
        m_listStr += "  { ";
        m_listStr += formulaString;
        m_listStr += " }";
      }
      m_listStr += "\n";
    }
  }
  return m_listStr;
}

void clVarList::getVarList(bool isConst, std::vector<stVarListItem>& list) {
  list.clear();
  for(int i = 0; i < TBLSZ; i++) {
    stVarListItem item;
    if(m_Table[i] != NULL) {
      if(m_Table[i]->isConst() != isConst)
        continue;
      clAnyType val(this, m_pMethods);
      m_Table[i]->getValue(0, val);
      item.name = m_Table[i]->getName();
      item.value = val;
      if(val.getType() == RT_FORMULA)
        item.formulaString = val.getString();
      list.push_back(item);
    }
  }
}

clName* clVarList::look(const char* p, clMethods* pMethods, bool isConst, int32_t ins)
{
  int32_t ii = 0;

  // Hashcode fuer Variablennamen berechnen
  const char* pp = p;
  while(*pp)
    ii = ii << 1 ^ *pp++;
  if(ii < 0)
    ii = -ii;
  ii %= TBLSZ;

  // Variable in der Tabelle suchen
  for( clName* n = m_Table[ii]; n; n = n->Next)
  {
    if(strcmp(p, n->getName()) == 0)
      return n;
  }

  if(ins == 0)
    return NULL;

  clName* nn = CREATE_OBJECT_CON(clName, (this, pMethods, isConst), "clName");
  size_t Len = strlen(p);
  nn->setName(CREATE_OBJECT_ARR(char,Len + 1,"clName::char[]"));
  strncpy(nn->getName(), p, Len + 1);
  nn->m_Value[0] = static_cast<float>(1.0);
  nn->Next = m_Table[ii];
  m_Table[ii] = nn;
  return nn;
}

clName* clVarList::insert(const char* s, bool isConst, clMethods* pMethods)
{
  return look(s,pMethods,isConst,1);
}

int32_t clVarList::save(const char *fileName) {
  std::ofstream out(fileName);
  if(!out.good())
    return ERR_FOPEN;
  for(int i = 0; i < TBLSZ; i++) {
    if(m_Table[i] != NULL) {

      clAnyType val;
     /*int32_t errCode = */m_Table[i]->getValue(0, val);
      out << m_Table[i]->getName() << ";";
      tType type = val.getType();
      out << val.getTypeString() << ";";
      out << m_Table[i]->isConst() << ";";
      switch (type) {
        case RT_FORMULA:
          out << val.getString();
          break;
        case RT_COMPLEX:
          out << val.getComplexRe() << ";" << val.getComplexIm();
          break;
        default:
          val.changeType(RT_STR);
          out << val.getString();
          break;
      }
      out << std::endl;
    }
  }
  return 0;
}

int32_t clVarList::load(const char *fileName, clMethods* pMethods, bool varConst) {
  int32_t err = 0;
  std::ifstream in(fileName);
  m_pMethods = pMethods;
  while(in.good()) {
    std::string line;
    std::vector<std::string> tokens;
    std::getline(in, line);
    tokenize(tokens, line);
    if(tokens.size() > 1) {
      bool isConst = atoi(tokens[2].c_str());
      if(isConst != varConst)
        continue;
      clName* varName = insert(tokens[0].c_str(), isConst, pMethods);
      tType type = clAnyType::stringToType(tokens[1].c_str());
      clAnyType var(this, pMethods);
      switch(type) {
        case RT_FLOAT:
          var = atof(tokens[3].c_str());
          break;
        case RT_INT64:
        case RT_UINT64:
        case RT_HEXVAL:
        case RT_BOOL:
          var = (int64_t)atoi(tokens[3].c_str());
          break;
        case RT_STR:
        case RT_FORMULA:
        case RT_COMMENT:
          var = tokens[3].c_str();
          break;
        case RT_COMPLEX:
          var.setComplex(atof(tokens[3].c_str()),
                         atof(tokens[4].c_str()));
      }
      var.setType(type);
      varName->setValue(0, var);
    }
  }
  return err;
}
