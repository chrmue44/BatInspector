#include "clResultParser.h"

clResultParser::clResultParser()
:
m_Parser(NULL,NULL),
m_Result()//,
//m_Ser()
{
}

int32_t clResultParser::Parse(char *pBuf, uint32_t BufLen)
{
    int32_t RetVal;

	m_Result.flush();
  RetVal = m_Parser.parseResultStr(pBuf,BufLen, m_Result);
	m_Result.storeResult();
	return RetVal;
}

int64_t clResultParser::GetInt64Par(uint32_t Nr)
{
	clAnyType Val;

	Val = m_Result.getPar(Nr);
    Val.changeType(RT_INT64);
        return Val.getInt64();
}

uint64_t clResultParser::GetUint64Par(uint32_t Nr)
{
	clAnyType Val;

	Val = m_Result.getPar(Nr);
    Val.changeType(RT_UINT64);
        return Val.getUint64();
}

REAL clResultParser::GetRealPar(uint32_t Nr)
{
	clAnyType Val;

	Val = m_Result.getPar(Nr);
	Val.changeType(RT_FLOAT);
	return static_cast<REAL>(Val.getFloat());
}

const tPM_Char* clResultParser::GetStringPar(uint32_t Nr)
{
	return m_Result.getParString(Nr);
}

 uint32_t clResultParser::GetStringParSize(uint32_t Nr)
 {
	 return m_Result.getParStringSize(Nr);
 }


uint32_t clResultParser::GetParCount()
{
	return m_Result.getParCount();
}
