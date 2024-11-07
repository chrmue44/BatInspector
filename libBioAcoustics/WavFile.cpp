#include "WavFile.h"


void DataArray::copy(const char* pSrc, char* pDst, int nr)
{
  for (int i = 0; i < nr; i++)
    pDst[i] = pSrc[i];
}

int32_t DataArray::getInt32(const char* pSrc)
{
  uData d;
  copy(pSrc, &d.i8[0], 4);
  return d.i32[0];
}

uint16_t DataArray::getUint16(const char* pSrc)
{
  uData d;
  copy(pSrc, &d.i8[0], 2);
  return d.i16[0];
}

WaveHeader::WaveHeader()
{
  DataArray::copy(FILE_TYPE_ID, _data.FileTypeId, 4);
  DataArray::copy(MEDIA_TYPE_ID, _data.MediaTypeId, 4);
  // Minimum size is always 4 bytes
  _data.FileLength = 4;
}

void WaveHeader::set(const char* pData)
{
  DataArray::copy(&pData[0], _data.FileTypeId, 4);
  DataArray::copy(&pData[8], _data.MediaTypeId, 4);
  _data.FileLength = DataArray::getInt32(&pData[4]);
}

const char* WaveHeader::getBytes()
{
  return (const char*) & _data;
}



void FormatChunk::set(const char* data)
{
  DataArray::copy(data, &_data.fmt[0], 4);
  _data.Subchunk1Size = DataArray::getInt32(&data[4]);
  _data.AudioFormat = DataArray::getUint16(&data[8]);
  _data.Channels = DataArray::getUint16(&data[10]);
  _data.Frequency = DataArray::getInt32(&data[12]);
  _data.AverageBytesPerSec = DataArray::getInt32(&data[16]);
  _data.BlockAlign = DataArray::getUint16(&data[20]);
  _data.BitsPerSample = DataArray::getUint16(&data[22]);
}

const char* FormatChunk::getBytes()
{
  return (const char*)&_data;
}

void FormatChunk::RecalcBlockSizes()
{
  _data.BlockAlign = (uint16_t)(_data.Channels * _data.BitsPerSample / 8);
  _data.AverageBytesPerSec = _data.Frequency * _data.BlockAlign;
}


DataChunk::DataChunk()
{
  DataArray::copy(CHUNK_ID, _data.ChunkId,4);
  _data.ChunkSize = 0;  // Until we add some data
}

DataChunk::~DataChunk()
{

}

void DataChunk::set(const char* data)
{
  DataArray::copy(data, _data.ChunkId, 4);
  _data.ChunkSize =  DataArray::getInt32(&data[4]);
}

bool DataChunk::isData()
{
  return (_data.ChunkId[0] == 'd') && (_data.ChunkId[1] == 'a') &&
         (_data.ChunkId[2] == 't') && (_data.ChunkId[3] == 'a');

}

void DataChunk::addSampleData(const char* data, int offs, int len, int bitsPerSample, int chanCount)
{
  int size = len / bitsPerSample * 8 / chanCount;
  _samples.reserve(size);
  if (chanCount == 1)
  {
    if (bitsPerSample == 16)
    {
      for (int i = 0; i < size; i++)
      {
        int s = (data[offs] << 16) | (data[offs + 1] << 24);
        _samples.push_back((int16_t)(s >> 16));
        offs += 2;
      }
      _data.ChunkSize = (uint32_t)(size * 2);
    }
    else if (bitsPerSample == 24)
    {
      for (int i = 0; i < size; i++)
      {
        int s = (data[offs] << 8) | (data[offs + 1] << 16) | (data[offs + 2] << 24);
        _samples.push_back((int16_t)(s >> 16));
        offs += 3;
      }
      _data.ChunkSize = (uint32_t)(size * 3);
    }
    else if (bitsPerSample == 32)
    {
      for (int i = 0; i < size; i++)
      {
        int s = data[offs] | (data[offs + 1] << 8) | (data[offs + 2] << 16) | (data[offs + 3] << 24);
        _samples.push_back((int16_t)(s >> 16));
        offs += 4;
      }
      _data.ChunkSize = (uint32_t)(size * 4);
    }
  }
  else
  {
    if (bitsPerSample == 16)
    {
      for (int i = 0; i < size; i++)
      {
        int sl = (data[offs] << 16) | (data[offs + 1] << 24);
        int sr = (data[offs + 2] << 16) | (data[offs + 3] << 24);
        _samples.push_back((int16_t)((sl + sr) >> 17));
        offs += 4;
      }
      _data.ChunkSize = (uint32_t)(size * 2);
    }
    else if (bitsPerSample == 24)
    {
      for (int i = 0; i < size; i++)
      {
        int sl = (data[offs] << 8) | (data[offs + 1] << 16) | (data[offs + 2] << 24);
        int sr = (data[offs + 3] << 8) | (data[offs + 4] << 16) | (data[offs + 5] << 24);
        _samples.push_back((int16_t)((sl + sr) >> 17));
        offs += 6;
      }
      _data.ChunkSize = (uint32_t)(size * 3);
    }
    else if (bitsPerSample == 32)
    {
      for (int i = 0; i < size; i++)
      {
        long sl = data[offs] | (data[offs + 1] << 8) | (data[offs + 2] << 16) | (data[offs + 3] << 24);
        long sr = data[offs + 4] | (data[offs + 5] << 8) | (data[offs + 6] << 16) | (data[offs + 7] << 24);
        _samples.push_back((int16_t)((sl + sr) >> 17));
        offs += 8;
      }
      _data.ChunkSize = (uint32_t)(size * 4);
    }
  }
}



int WavFile::readFile(const char* name)
{
  int retVal = 0;
  char* data = nullptr;

  try
  {
    char rawdata[32];
    FILE* file = nullptr;
    errno_t err = fopen_s(&file, name, "rb");
    if (err != 0)
      return 1;

    size_t readBytes = fread(rawdata, 1, _header.getLength(), file);
    _header.set(rawdata);
    readBytes = fread(rawdata, 1, _formatChunk.getLength(), file);
    _formatChunk.set(rawdata);
    readBytes = fread(rawdata, 1, _dataChunk.getLength(), file);
    _dataChunk.set(rawdata);
    while(!_dataChunk.isData())
    int pos = 0;
    while (!_dataChunk.isData())
    {
      int cnt = _dataChunk.getData()->ChunkSize + 4 - _dataChunk.getLength();
      data = new char[cnt];
      readBytes = fread(data, cnt, 1, file);
      delete[] data;
      readBytes = fread(rawdata, 1, _dataChunk.getLength(),  file);
      _dataChunk.set(rawdata);
    }
    int cnt = _header.getData()->FileLength;
    data = new char[cnt];
    int rdIdx = 0;
    readBytes = fread(data + rdIdx, 1, cnt, file);
    
    _dataChunk.addSampleData(data, 0, readBytes, _formatChunk.getData()->BitsPerSample, _formatChunk.getData()->Channels);
    _fName = name;
    _isOpen = true;
  }
  catch(...)
  {
    retVal = 1;
  }

  if (data != nullptr)
    delete[] data;
  return retVal;
}

