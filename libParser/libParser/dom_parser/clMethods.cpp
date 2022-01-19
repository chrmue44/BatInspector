/*********************************************************
 *         project: ProcessMon                           *
 *                                                       *
 *              (C) 2010 Christian Mueller               *
 *                       Odenwaldstrasse 134g            *
 *                       D-64372 Ober-Ramstadt           *
 *                                                       *
 *********************************************************/

#include "clMethods.h"
#include "clMthdList.h"
#include "../clMthdResult.h"
#include "clError.h"
#include <cstring>

clMethods::clMethods(clMthdResult* pResult)
:
m_pResult(pResult)
{

}

clMethods::~clMethods()
{

}


void clMethods::addMethodList(  clMthdList* pMthdList)
{
  m_List.push_back(pMthdList);
}



int32_t clMethods::executeFunction(tPM_string& FuncName,  uint32_t ParCnt,  clAnyType** argv, clAnyType& result)
{
  int32_t RetVal = 0;
  tMthdListListIter i;
  tFuncTabIter f;
  //bool Found = false;
  if(FuncName == "help")
  {
    help(ParCnt,argv);
    return RetVal;
  }
  else
  {
    for(i = m_List.begin(); i != m_List.end(); i++)
    {
      bool RetVal = (*i)->findMethod(f, FuncName);
      if(RetVal)
      {
        return f->pFunc(ParCnt, argv, result);
      }
    }
  }
  RetVal = ERR_METHOD;
  return RetVal;
}


void clMethods::help(uint32_t ParCnt,  clAnyType** argv)
{
  tMthdListListIter i;

  if (ParCnt == 0)
  {
    tPM_string Delimiter = "\r\n";
    m_pResult->setDelimiter(Delimiter);
    m_pResult->addValue("syntax: method(par1, par2, ...)");
    Delimiter = "\r\n  ";
    m_pResult->setDelimiter(Delimiter);

    for(i = m_List.begin(); i != m_List.end(); i++)
    {
      tFuncTab* pTab = (*i)->getMthdList();
      m_pResult->addValue((*i)->getMthdListHelp());
      tFuncTabIter j;
      for(j= pTab->begin(); j != pTab->end(); j++)
        m_pResult->addValue(j->pName);
    }
    m_pResult->addValue("q");
  }
  else
  {
    bool Found = false;
    tFuncTabIter f;
    
    for(i = m_List.begin(); i != m_List.end(); i++)
    {
      tPM_string FuncName = argv[0]->getString();
      bool RetVal = (*i)->findMethod(f, FuncName);
      if(RetVal)
      {
        Found = true;
        break;
      }
    }
    if(Found)
    {
      const stHelpTabItem *pItem = (*i)->getHelpTab();
      while(pItem->pMthdHelp != NULL)
      {
        if(strcmp(pItem->pMthdName,argv[0]->getString()) == 0)
        {
          tPM_string Delimiter = "\r\n";
          
          m_pResult->setDelimiter(Delimiter);
          m_pResult->addValue("");
          m_pResult->addValue(pItem->pMthdName);
          m_pResult->addValue(pItem->pMthdHelp);
          m_pResult->addValue("\nParameters:");
          Delimiter = "\n  ";
          m_pResult->setDelimiter(Delimiter);

          uint32_t i;
          for(i = 0; i < PAR_HELP_CNT; i++)
          {
            if(pItem->pParHelp[i] != NULL)
              m_pResult->addValue(pItem->pParHelp[i]);
            else
              break;
          }

          Delimiter = "\r\n";
          m_pResult->setDelimiter(Delimiter);
          m_pResult->addValue("\nReturn values:");
          Delimiter = "\r\n  ";
          m_pResult->setDelimiter(Delimiter);
          for(i = 0; i < RET_HELP_CNT; i++)
          {
            if(pItem->pRetHelp[i] != NULL)
              m_pResult->addValue(pItem->pRetHelp[i]);
            else
              break;
          }
          break;
        }
        pItem++;
      }
    }
  }
}
