// ****************************************************************************
// *                                                                          *
// *   (C)  Hottinger Baldwin Messtechnik GmbH                                *
// *        Im Tiefen See 45,                                                 *
// *        D-64293 Darmstadt                                                 *
// *        www.PM.com                                                       *
// *                                                                          *
// *                                                                          *
// ******* MODULE HISTORY *****************************************************
// *                                                                          *
// *   Ver     Date     Author      Remarks                                   *
// *   ---------------------------------------------------------------------- *
// *   000     18.10.05 Schreyer    implemented                               *
// ****************************************************************************

#include "clMthdResult.h"
#include <stdio.h>

clMthdResult::clMthdResult()
:
m_SaveResult(false)
{

}

clMthdResult::~clMthdResult()
{
}

void clMthdResult::addValue(clAnyType Val)
{
  if(m_List.size() > 0)
    m_ResString += m_Delimiter;
  m_List.push_back(Val);
  tType Type = Val.getType();
  Val.changeType(RT_STR);
  if(Type == RT_STR)
  {
    m_ResString += "\"";
    m_ResString += Val.getString();
    m_ResString += "\"";
  }
  else
    m_ResString += Val.getString();
}

void clMthdResult::addValue(double Val)
{
  char Buf[100];
  clAnyType AVal;

  if(m_List.size() > 0)
    m_ResString += m_Delimiter;

  sprintf(Buf, /*sizeof(Buf),*/ DOUBLE_TOSTR_FMT, Val);
  m_ResString += Buf;
  AVal = Val; 
  m_List.push_back(AVal);
}

/*void clMthdResult::addValue(int32_t Val)
{
  clAnyType AVal;

  if(m_List.size() > 0)
    m_ResString += m_Delimiter;

  AVal = Val; 
  m_List.push_back(AVal);
  AVal.changeType(RT_STR);
  m_ResString += AVal.getString();
}

void clMthdResult::addValue(uint32_t Val)
{
  clAnyType AVal;

  if(m_List.size() > 0)
    m_ResString += m_Delimiter;

  AVal = static_cast<uint64_t>(Val);
  m_List.push_back(AVal);
  AVal.changeType(RT_STR);
  m_ResString += AVal.getString();
}
*/
void clMthdResult::addValue(uint64_t Val)
{
  clAnyType AVal;

  if(m_List.size() > 0)
    m_ResString += m_Delimiter;

  AVal = Val; 
  m_List.push_back(AVal);
  AVal.changeType(RT_STR);
  m_ResString += AVal.getString();
}

void clMthdResult::addValue(const char* Val)
{
  clAnyType AVal;

  if(m_List.size() > 0)
    m_ResString += m_Delimiter;

  AVal = Val; 
  m_List.push_back(AVal);
  AVal.changeType(RT_STR);
  m_ResString += "\"";
  m_ResString += AVal.getString();
  m_ResString += "\"";
}

void clMthdResult::addValue(const char* Val,uint32_t Len)
{
  clAnyType AVal;

  if(m_List.size() > 0)
    m_ResString += m_Delimiter;

  AVal.setString(Val,Len); 
  m_List.push_back(AVal);
  AVal.changeType(RT_STR);
  m_ResString += "\"";
  m_ResString += AVal.getString();
  m_ResString += "\"";
}

void clMthdResult::flush()
{
  m_ResString = "";
  m_Delimiter = ",";
  m_List.clear();
}

void clMthdResult::signalizeSaveResult()
{
  m_SaveResult = true;
}


void clMthdResult::storeResult()
{
  uint32_t i;
  if(!m_SaveResult)
  {
    m_LastResult.clear();
    for(i = 0; i < m_List.size(); i++)
    {
      m_LastResult.push_back(m_List[i]);
    }
  }
  else
    m_SaveResult = false;
}

const char* clMthdResult::getString()
{
  return m_ResString.c_str();
}

void clMthdResult::setDelimiter(tPM_string& Delimiter)
{
  m_Delimiter = Delimiter;
}

clAnyType clMthdResult::getPar(uint32_t Nr)
{
  clAnyType Val;

  if ((m_LastResult.size() >= Nr) && (Nr > 0))
    return m_LastResult[Nr-1];
  else
  {
    Val = 0.0;
    return Val;
  }
}

const char* clMthdResult::getParString(uint32_t Nr)
{
  if ((m_LastResult.size() >= Nr) && (Nr > 0))
		return m_LastResult[Nr-1].getString();
  else
  {
    return NULL;
  }
}

uint32_t clMthdResult::getParStringSize(uint32_t Nr)
{
  if ((m_LastResult.size() >= Nr) && (Nr > 0))
		return m_LastResult[Nr-1].getStrSize();
  else
  {
    return 0;
  }
}

uint32_t clMthdResult::getLastResultParCount()
{
  return static_cast<uint32_t>(m_LastResult.size());
}

uint32_t clMthdResult::getParCount()
{
  return static_cast<uint32_t>(m_List.size());
}
