/*********************************************************
 *         project: ProcessMon                           *
 *                                                       *
 *              (C) 2010 Christian Mueller               *
 *                       Odenwaldstrasse 134g            *
 *                       D-64372 Ober-Ramstadt           *
 *                                                       *
 *********************************************************/
#include "clHexSerializer.h"
#include "clError.h"

clHexSerializer::clHexSerializer()
{
}

clHexSerializer::~clHexSerializer()
{
}

int32_t clHexSerializer::StartSerializing(char *pBuf, uint32_t BufSize)
{
    int32_t RetVal = 0;

	m_pBuffer = pBuf;
	m_pBufStart= pBuf;
	m_Remaining = BufSize;
	m_BufSize = BufSize;
    if(m_Remaining > 2*sizeof(uint32_t) + 1)
	{
		*m_pBuffer++ = '#';
		m_Remaining--;
	Add((uint64_t)0);
		m_Count = 0;
	}
	else
		RetVal = ERR_HEXBUF;

	return RetVal;
}

int32_t clHexSerializer::StartDeserializing(const char *pBuf, uint32_t BufSize)
{
    int32_t RetVal = 0;
    uint32_t ByteCount;

	m_pBuffer = const_cast<tPM_Char*>(pBuf);
	m_pBufStart= const_cast<tPM_Char*>(pBuf);
	m_Remaining = BufSize - 2; // -1 für '#' und -1 für 0(Stringende)
	m_BufSize = BufSize;
	if(*m_pBuffer++ != '#')
		RetVal = ERR_SERFMT;
	else
	{
    Get(&ByteCount,RT_UINT64);
		if(ByteCount > m_Remaining)
			RetVal = ERR_HEXASC_HDR;
		m_Count = 0;
	}
	return RetVal;
}


int32_t clHexSerializer::FinalizeSerializing()
{
    int32_t RetVal = 0;
	if(m_BufSize > (2* sizeof(m_Count) + 1))
	{
	  *m_pBuffer = 0;
		m_Remaining = 2 * sizeof(m_Count);
    m_pBuffer = m_pBufStart + 1;
          Add((uint64_t)m_Count);
	}
	else
		RetVal = ERR_HEXBUF;
	return RetVal;
}

/*INT32 clHexSerializer::Add(bool Val)
{
	m_Value.ValBool = Val;
	return HexToAsc(sizeof(bool));
} */

int32_t clHexSerializer::Add(int16_t Val)
{
	m_Value.ValInt16 = Val;
    return HexToAsc(sizeof(int16_t));
}

int32_t clHexSerializer::Add(int64_t Val)
{
        m_Value.ValInt64 = Val;
    return HexToAsc(sizeof(int64_t));
}

int32_t clHexSerializer::Add(uint64_t Val)
{
        m_Value.ValUint64 = Val;
    return HexToAsc(sizeof(uint64_t));
}

int32_t clHexSerializer::Add(REAL Val)
{
	m_Value.ValReal= Val;
	return HexToAsc(sizeof(REAL));
}

int32_t clHexSerializer::Add(const tPM_Char* Val, uint32_t Len)
{
  int32_t RetVal = 0;
    for(uint32_t i = 0; i < Len; i++)
	{
		m_Value.Byte[0] = *Val++;
		RetVal = HexToAsc(1);
		if(RetVal != 0)
			break;
	}
	return RetVal;
}


int32_t clHexSerializer::HexToAsc(uint32_t Size)
{
    int32_t RetVal = 0;
	if(m_Remaining >= 2 * Size)
	{
        for(uint32_t i = 0; i < Size; i++)
		{
            uint8_t b = (m_Value.Byte[i] & 0xF0) >> 4;
			if(b > 9)
				b += 7;
			b += '0';
			*m_pBuffer++ = b;
			b = (m_Value.Byte[i] & 0xF);
			if(b > 9)
				b += 7;
			b += '0';
			*m_pBuffer++ = b;
		}
		m_Count += Size * 2;
		m_Remaining -= Size * 2;
	}
	else
		RetVal = ERR_HEXBUF;
	return RetVal;
}



int32_t clHexSerializer::AscToHex(uint32_t Size, tPM_Char* pDest)
{
    int32_t RetVal = 0;
	if(m_Remaining >= (Size *2))
	{
        for(uint32_t i = 0; i < Size; i++)
		{
            uint8_t bh = *m_pBuffer++ - '0';
			if (bh > 9)
				bh -= 7;
            uint8_t bl = *m_pBuffer++ - '0';
			if (bl > 9)
				bl -= 7;
			if(pDest == NULL)
			  m_Value.Byte[i] = (bh << 4) | bl;
			else
				*pDest++ = (bh << 4) | bl;
		}
		m_Remaining -= Size * 2;
	}
	else
		RetVal = ERR_HEXBUF;
	return RetVal;

}

int32_t clHexSerializer::GetStr(tPM_Char* Str, uint32_t Len)
{
   return AscToHex(Len,Str);
}

int32_t clHexSerializer::Get(void *pVal, tType Type)
{
    int32_t RetVal = 0;
	switch(Type)
	{
/*	  case RT_INT16:
      RetVal = AscToHex(sizeof(int16_t));
            *(reinterpret_cast<int16_t*>(pVal)) = m_Value.ValInt16;
                        break; */
          case RT_INT64:
      RetVal = AscToHex(sizeof(int64_t));
            *(reinterpret_cast<int64_t*>(pVal)) = m_Value.ValInt64;
			break;
	  case RT_UINT64:
      RetVal = AscToHex(sizeof(uint64_t));
            *(reinterpret_cast<uint64_t*>(pVal)) = m_Value.ValUint64;
			break;
		case RT_FLOAT:
      RetVal = AscToHex(sizeof(REAL));
			*(reinterpret_cast<REAL*>(pVal)) = m_Value.ValReal;
			break;
/*		case RT_BOOL:
      RetVal = AscToHex(sizeof(bool));
			*(reinterpret_cast<bool*>(pVal)) = m_Value.ValBool;
			break; */
		default:
			RetVal = ERR_SERTYPE;
			break;
	}
	return RetVal;
}
