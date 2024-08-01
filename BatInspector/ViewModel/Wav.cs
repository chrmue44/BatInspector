/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using libParser;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace BatInspector
{
  //https://www.codeguru.com/dotnet/making-sounds-with-waves-using-c/
  //https://isip.piconepress.com/projects/speech/software/tutorials/production/fundamentals/v1.0/section_02/s02_01_p05.html

  public class WaveHeader
  {
    private const string FILE_TYPE_ID = "RIFF";
    private const string MEDIA_TYPE_ID = "WAVE";

    public string FileTypeId { get; private set; }
    public UInt32 FileLength { get; set; }
    public string MediaTypeId { get; private set; }
    public WaveHeader()
    {
      FileTypeId = FILE_TYPE_ID;
      MediaTypeId = MEDIA_TYPE_ID;
      // Minimum size is always 4 bytes
      FileLength = 4;
    }

    public WaveHeader(byte[] data)
    {
      FileTypeId = System.Text.Encoding.ASCII.GetString(data, 0, 4);
      MediaTypeId = System.Text.Encoding.ASCII.GetString(data, 8, 4);
      // Minimum size is always 4 bytes
      FileLength = BitConverter.ToUInt32(data, 4);
    }

    public byte[] GetBytes()
    {
      List<Byte> chunkData = new List<byte>();
      chunkData.AddRange(Encoding.ASCII.GetBytes(FileTypeId));
      chunkData.AddRange(BitConverter.GetBytes(FileLength));
      chunkData.AddRange(Encoding.ASCII.GetBytes(MediaTypeId));
      return chunkData.ToArray();
    }
  }

  public class FormatChunk
  {
    private ushort _bitsPerSample;
    private ushort _channels;
    private uint _frequency;
    private const string CHUNK_ID = "fmt ";

    public string ChunkId { get; private set; }
    public UInt32 ChunkSize { get; private set; }
    public UInt16 FormatTag { get; private set; }

    public UInt16 Channels
    {
      get { return _channels; }
      set { _channels = value; RecalcBlockSizes(); }
    }

    public UInt32 Frequency
    {
      get { return _frequency; }
      set { _frequency = value; RecalcBlockSizes(); }
    }

    public UInt32 AverageBytesPerSec { get;  private set; }

    public UInt16 BlockAlign { get; private set; }

    public UInt16 BitsPerSample
    {
      get { return _bitsPerSample; }
      set { _bitsPerSample = value; RecalcBlockSizes(); }
    }

    public FormatChunk(byte[] data)
    {
      ChunkId = System.Text.Encoding.ASCII.GetString(data, 0, 4);
      ChunkSize = BitConverter.ToUInt32(data, 4);
      FormatTag = BitConverter.ToUInt16(data, 8);
      Channels = BitConverter.ToUInt16(data, 10);
      Frequency = BitConverter.ToUInt32(data, 12);
      AverageBytesPerSec = BitConverter.ToUInt32(data, 16);
      BlockAlign = BitConverter.ToUInt16(data, 20);
      BitsPerSample = BitConverter.ToUInt16(data, 22);
    }

    public FormatChunk(ushort chans, uint freq)
    {
      ChunkId = CHUNK_ID;
      ChunkSize = 16;
      FormatTag = 1;       // MS PCM (Uncompressed wave file)
      Channels = chans;        // Default to stereo
      Frequency = freq;   // Default to 44100hz
      BitsPerSample = 16;  // Default to 16bits
      RecalcBlockSizes();
    }

    private void RecalcBlockSizes()
    {
      BlockAlign = (UInt16)(_channels * (_bitsPerSample / 8));
      AverageBytesPerSec = _frequency * BlockAlign;
    }

    public byte[] GetBytes()
    {
      List<Byte> chunkBytes = new List<byte>();

      chunkBytes.AddRange(Encoding.ASCII.GetBytes(ChunkId));
      chunkBytes.AddRange(BitConverter.GetBytes(ChunkSize));
      chunkBytes.AddRange(BitConverter.GetBytes(FormatTag));
      chunkBytes.AddRange(BitConverter.GetBytes(Channels));
      chunkBytes.AddRange(BitConverter.GetBytes(Frequency));
      chunkBytes.AddRange(BitConverter.GetBytes(AverageBytesPerSec));
      chunkBytes.AddRange(BitConverter.GetBytes(BlockAlign));
      chunkBytes.AddRange(BitConverter.GetBytes(BitsPerSample));

      return chunkBytes.ToArray();
    }

    public UInt32 Length()
    {
      return (UInt32)GetBytes().Length;
    }

  }

  public class DataChunk
  {
    private const string CHUNK_ID = "data";

    public string ChunkId { get; private set; }
    public UInt32 ChunkSize { get; set; }
    public short[] WaveData { get; private set; }

    public DataChunk()
    {
      ChunkId = CHUNK_ID;
      ChunkSize = 0;  // Until we add some data
    }

    public DataChunk(byte[] data)
    {
      ChunkId = System.Text.Encoding.ASCII.GetString(data, 0, 4);
      ChunkSize = BitConverter.ToUInt32(data, 4);
    }

    public UInt32 Length()
    {
      return (UInt32)GetBytes().Length;
    }

    public byte[] GetBytes()
    {
      List<Byte> chunkBytes = new List<Byte>();

      chunkBytes.AddRange(Encoding.ASCII.GetBytes(ChunkId));
      chunkBytes.AddRange(BitConverter.GetBytes(ChunkSize));
      byte[] bufferBytes = new byte[WaveData.Length * 2];
      Buffer.BlockCopy(WaveData, 0, bufferBytes, 0,
         bufferBytes.Length);
      chunkBytes.AddRange(bufferBytes.ToList());

      return chunkBytes.ToArray();
    }

    public void AddSampleData(short[] leftBuffer,
     short[] rightBuffer, int idxStart, int idxEnd)
    {
      int len = leftBuffer.Length;
      if (rightBuffer != null)
        len += rightBuffer.Length;
      WaveData = new short[len];
      int bufferOffset = 0;
      for (int index = 0; index < WaveData.Length; index += 2)
      {
        WaveData[index] = leftBuffer[bufferOffset];
        WaveData[index + 1] = rightBuffer[bufferOffset];
        bufferOffset++;
      }
      ChunkSize = (UInt32)WaveData.Length * 2;
    }


    public void AddSampleData(byte[] data, int offs, int len, int bitsPerSample, int chanCount, ref double[] samples)
    {
      AddSampleData(data, offs, len, bitsPerSample, chanCount);
      samples = new double[WaveData.Length];
      for(int i = 0; i <  samples.Length; i++)
        samples[i] = (double)WaveData[i]/32768.0;
    }

    public void AddSampleData(byte[] data, int offs, int len, int bitsPerSample, int chanCount)
    {
      int size = len * 8 / bitsPerSample/chanCount;
      WaveData = new short[size];
      if (chanCount == 1)
      {
        if (bitsPerSample == 16)
        {
          for (int i = 0; i < size; i++)
          {
            int s = (data[offs] << 16) | (data[offs + 1] << 24);
            WaveData[i] = (short)(s >> 16);
            offs += 2;
          }
          ChunkSize = (UInt32)(WaveData.Length * 2);
        }
        else if (bitsPerSample == 24)
        {
          for (int i = 0; i < size; i++)
          {
            int s = (data[offs] << 8) | (data[offs + 1] << 16) | (data[offs + 2] << 24);
            WaveData[i] = (short)(s >> 16);
            offs += 3;
          }
          ChunkSize = (UInt32)(WaveData.Length * 3);
        }
        else if (bitsPerSample == 32)
        {
          for (int i = 0; i < size; i++)
          {
            int s = data[offs] | (data[offs + 1] << 8) | (data[offs + 2] << 16) | (data[offs + 3] << 24);
            WaveData[i] = (short)(s >> 16);
            offs += 4;
          }
          ChunkSize = (UInt32)(WaveData.Length * 4);
        }
      }
      else
      { 
        if (bitsPerSample == 16)
      {
        for (int i = 0; i < size; i++)
        {
          int sl = (data[offs] << 16) | (data[offs + 1] << 24);
          int sr = (data[offs+2] << 16) | (data[offs + 3] << 24);
          WaveData[i] = (short)((sl + sr) >> 17);
          offs += 4;
        }
        ChunkSize = (UInt32)(WaveData.Length * 2);
      }
      else if (bitsPerSample == 24)
      {
        for (int i = 0; i < size; i++)
        {
          int sl = (data[offs] << 8) | (data[offs + 1] << 16) | (data[offs + 2] << 24);
          int sr = (data[offs+3] << 8) | (data[offs + 4] << 16) | (data[offs + 5] << 24);
            WaveData[i] = (short)((sl+sr) >> 17);
          offs += 6;
        }
        ChunkSize = (UInt32)(WaveData.Length * 3);
      }
      else if (bitsPerSample == 32)
      {
        for (int i = 0; i < size; i++)
        {
          long sl = data[offs] | (data[offs + 1] << 8) | (data[offs + 2] << 16) | (data[offs + 3] << 24);
          long sr = data[offs+4] | (data[offs + 5] << 8) | (data[offs + 6] << 16) | (data[offs + 7] << 24);
            WaveData[i] = (short)((sl + sr) >> 17);
          offs += 8;
        }
        ChunkSize = (UInt32)(WaveData.Length * 4);
      }
    }

  }

  public void AddSampleData(byte[] data, int offs, int len, ref double[] samples, int bitsPerSample)
    {
      int size = len * 8 / bitsPerSample;
      WaveData = new short[size];
      samples = new double[size];
      if (bitsPerSample == 16)
      {
        for (int i = 0; i < size; i++)
        {
          int s = (data[offs] << 16) | (data[offs + 1] << 24);
          WaveData[i] = (short)(s >> 16);
          samples[i] = (double)s / 2147483648f;
          offs += 2;
        }
        ChunkSize = (UInt32)(WaveData.Length * 2);
      }
      else if(bitsPerSample == 24)
      {
        for (int i = 0; i < size; i++)
        {
          int s = (data[offs] << 8) | (data[offs + 1] << 16) | (data[offs + 2] << 24);
          WaveData[i] = (short)(s >> 16);
          samples[i] = (double)s / 2147483648f;
          offs += 3;
        }
        ChunkSize = (UInt32)(WaveData.Length * 3);
      }
      else if (bitsPerSample == 32)
      {
        for (int i = 0; i < size; i++)
        {
          int s = data[offs] | (data[offs+1] << 8) | (data[offs + 2] << 16) | (data[offs + 3] << 24);
          WaveData[i] = (short)(s >> 16);
          samples[i] = (double)s / 2147483648f;
          offs += 4;
        }
        ChunkSize = (UInt32)(WaveData.Length * 3);
      }
    }

    public void AddSampleData(double[] leftBuffer, int idxStart, int idxEnd)
    {
      WaveData = new short[idxEnd - idxStart + 2];
      if (idxEnd >= leftBuffer.Length)
        idxEnd = leftBuffer.Length - 1;

      int bufferOffset = 0;
      for (int index = idxStart; index < idxEnd; index++)
      {
        WaveData[bufferOffset] = (short)(leftBuffer[index] * 32767.0);
        bufferOffset++;
      }
      ChunkSize = (UInt32)WaveData.Length * 2;
    }

    public void AddSampleData(short[] leftBuffer, int idxStart, int idxEnd)
    {
      WaveData = new short[idxEnd - idxStart + 2];
      if (idxEnd >= leftBuffer.Length)
        idxEnd = leftBuffer.Length - 1;

      int bufferOffset = 0;
      for (int index = idxStart; index < idxEnd; index++)
      {
        WaveData[bufferOffset] = (leftBuffer[index]);
        bufferOffset++;
      }
      ChunkSize = (UInt32)WaveData.Length * 2;
    }
  }

  public class WavFile
  {
    private byte[] _rawData;
    WaveHeader _header;
    FormatChunk _format;
    DataChunk _data;
    string _fName;
    bool _isOpen;
    WaveOutEvent _outputDevice = null;
    MemoryStream _memStream = null;
    RawSourceWaveStream _rawStream = null;
    Pcm16BitToSampleProvider _sampleProvider = null;
    PlaybackState _playbackState = PlaybackState.Stopped;

    public WaveHeader WavHeader { get { return _header;} }

    public FormatChunk FormatChunk {  get {  return _format;} }

    public short[] AudioSamples {  get { return _data.WaveData; } }

    public PlaybackState PlaybackState
    {
      get
      {
        if (_outputDevice == null)
          return PlaybackState.Stopped;
        else
          return _outputDevice.PlaybackState;
      }
    }
    
    
    public double PlayPosition
    {
      get { return (_rawStream!= null) ? (double)_rawStream.Position / _rawStream.Length * _data.WaveData.Length/_format.Frequency  : 0.0; }
    }


    public WavFile()
    {
      _isOpen = false;
      _fName = "";
    }

    private byte[] partArray(byte[]list, int startIdx, int len)
    {
      byte[] retVal = new byte[len];
      for (int i = 0; i < len; i++)
        retVal[i] = list[startIdx + i];
      return retVal;
    }

    public int readFile(string name)
    {
      int retVal = 0;
      try
      {
        _rawData = File.ReadAllBytes(name);
        byte[] hdr = partArray(_rawData, 0, 12);
        _header = new WaveHeader(hdr);
        byte[] format = partArray(_rawData, 12, 24);
        _format = new FormatChunk(format);

        int pos = 12;
        while (!(_rawData[pos] == 'd' && _rawData[pos + 1] == 'a' && _rawData[pos + 2] == 't' && _rawData[pos + 3] == 'a'))
        {
          pos += 4;
          int chunkSize = _rawData[pos] + _rawData[pos + 1] * 256 + _rawData[pos + 2] * 65536 + _rawData[pos + 3] * 16777216;
          pos += 4 + chunkSize;
        }
        byte[] data = partArray(_rawData, pos, 8);
        _data = new DataChunk(data);
        pos += 8;
        _data.AddSampleData(_rawData, pos, _rawData.Length - pos, _format.BitsPerSample, _format.Channels);
        _fName = name;
        _isOpen = true;
      }
      catch 
      {
        retVal = 1;
      }
      return retVal;
    }

    public int readFile(string name, ref double[] samples)
    {
      int retVal = 0;
      try
      {
        _rawData = File.ReadAllBytes(name);
        byte[] hdr = partArray(_rawData, 0, 12);
        _header = new WaveHeader(hdr);
        byte[] format = partArray(_rawData, 12, 24);
        _format = new FormatChunk(format);

        int pos = 12;
        while (!(_rawData[pos] == 'd' && _rawData[pos + 1] == 'a' && _rawData[pos + 2] == 't' && _rawData[pos + 3] == 'a'))
        {
          pos += 4;
          int chunkSize = _rawData[pos] + _rawData[pos + 1] * 256 + _rawData[pos + 2] * 65536 + _rawData[pos + 3] * 16777216;
          pos += 4 + chunkSize;
        }
        byte[] data = partArray(_rawData, pos, 8);
        _data = new DataChunk(data);
        pos += 8;
        _data.AddSampleData(_rawData, pos, _rawData.Length - pos, _format.BitsPerSample, _format.Channels, ref samples);
        _fName = name;
        _isOpen = true;
      }
      catch (Exception ex)
      {
        DebugLog.log("error reading WAV file: " + ex.ToString(), enLogType.ERROR);
        retVal = 1;
      }
      return retVal;
    }


    public int saveFileAs(string name)
    {
      _fName = name;
      return saveFile();
    }

    public int saveFile()
    {
      int retVal = 0;
      try
      {
        if (_isOpen)
        {
          if(File.Exists(_fName))
            File.Delete(_fName);
          _format.BitsPerSample = 16;
          FileStream f = File.OpenWrite(_fName);
          List<Byte> tempBytes = new List<byte>();
          _header.FileLength = 4 + _format.Length() + _data.Length();
          tempBytes.AddRange(_header.GetBytes());
          tempBytes.AddRange(_format.GetBytes());
          if(_format.ChunkSize > 16)
          {
            byte[] dummy = new byte[_format.ChunkSize - 16];
            tempBytes.AddRange(dummy);
          }
          tempBytes.AddRange(_data.GetBytes());
          _rawData = tempBytes.ToArray();
          foreach (byte b in _rawData)
            f.WriteByte(b);
          f.Close();
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("error writing WAV file: " + _fName + " " + ex.ToString(), enLogType.ERROR);
        retVal = 1;
      }
      return retVal;
    }


    //
    // converts WAV samples to a raw byte array that can be played with NAUDIO
    //
    private void convertToRawData()
    {
      try
      {
        List<Byte> tempBytes = new List<byte>();
        tempBytes.AddRange(_data.GetBytes());
        _rawData = tempBytes.ToArray();
      }
      catch (Exception ex)
      {
        DebugLog.log("error writing WAV file to memory stream: " + ex.ToString(), enLogType.ERROR);
      }
    }

    public static void splitWav(string name, double splitLength, bool removeOriginal)
    {
      WavFile wav = new WavFile();
      wav.readFile(name);
      int sampleRate = (int)wav.FormatChunk.Frequency;
      double len = (double)wav.AudioSamples.Length / sampleRate;
      char ext = 'a';
      for(int i = 0; i <= len / splitLength; i++)
      {
        int idxStart = (int)(splitLength * i * sampleRate);
        int idxEnd = (int)(splitLength * (i + 1) * sampleRate) - 1;
        if (idxEnd > wav.AudioSamples.Length)
          idxEnd = wav.AudioSamples.Length - 1;
        WavFile splitWav = new WavFile();
        splitWav.createFile(sampleRate, idxStart, idxEnd, wav.AudioSamples);
        string fName = Path.Combine(Path.GetDirectoryName(name), Path.GetFileNameWithoutExtension(name) + "_" + ext + AppParams.EXT_WAV);
        splitWav.saveFileAs(fName);
        ext++;
      }
      if (removeOriginal && File.Exists(name))
        File.Delete(name);
    }

    public void createFile(ushort chanCount, int sampleRate, int idxStart, int idxEnd, double[] left, double[] right = null)
    {
      _header = new WaveHeader();
      _format = new FormatChunk(chanCount, (uint)sampleRate);
      _data = new DataChunk();
      List<Byte> tempBytes = new List<byte>();

      _data.AddSampleData(left, idxStart, idxEnd);

      tempBytes.AddRange(_header.GetBytes());
      tempBytes.AddRange(_format.GetBytes());
      tempBytes.AddRange(_data.GetBytes());
      _rawData = tempBytes.ToArray();
      _isOpen = true;
    }

    public void createFile(int sampleRate, int idxStart, int idxEnd, short[] left)
    {
      _header = new WaveHeader();
      _format = new FormatChunk(1, (uint)sampleRate);
      _data = new DataChunk();
      List<Byte> tempBytes = new List<byte>();

      _data.AddSampleData(left, idxStart, idxEnd);

      tempBytes.AddRange(_header.GetBytes());
      tempBytes.AddRange(_format.GetBytes());
      tempBytes.AddRange(_data.GetBytes());
      _rawData = tempBytes.ToArray();
      _isOpen = true;
    }

    public void createSineWave(double freqHz, int sampleRate, double amplitude, double offs)
    {
      double[] vals = new double[sampleRate];
      for(int i = 0; i < sampleRate; i++)
        vals[i] = amplitude * Math.Sin(i * freqHz * 2 * Math.PI/ sampleRate) + offs;
      createFile(1, sampleRate, 0, vals.Length - 1, vals);
    }

    public void pause()
    {
      if ((_rawData != null) && (_outputDevice != null))
      {
        if (_outputDevice.PlaybackState == PlaybackState.Playing)
        {
          _outputDevice.Pause();
          _playbackState = PlaybackState.Paused;
        }
      }
    }

    public void play_HET(ushort chanCount, int sampleRate, double f_HET, int idxStart, int idxEnd, double[] left, double[] right = null, double playPosition = 0.0)
    {
      if (_playbackState != PlaybackState.Paused)
      {
        createFile(chanCount, sampleRate, idxStart, idxEnd, left, right);
        applyHetFrequency(f_HET);
        convertToRawData();
      }
      else
      {
        if ((_outputDevice != null) && (_rawStream != null))
        {
          if (_outputDevice.PlaybackState == PlaybackState.Paused)
          {
            double length = (double)_data.WaveData.Length / _format.Frequency;
            int pos = (int)(playPosition / length * _rawStream.Length);
            _rawStream.Position = pos;
          }
        }
      }
      play();
    }

    public void play(ushort chanCount, int sampleRate, int idxStart, int idxEnd, double[] left, double[] right = null, double playPosition = 0.0)
    {
      if (_playbackState != PlaybackState.Paused)
      {
        createFile(chanCount, sampleRate, idxStart, idxEnd, left, right);
        convertToRawData();
      }
      else
      {
        if ((_outputDevice != null) && (_rawStream != null))
        {
          if (_outputDevice.PlaybackState == PlaybackState.Paused)
          {
            double length = (double)_data.WaveData.Length / _format.Frequency;
            int pos = (int)(playPosition / length * _rawStream.Length);
            _rawStream.Position = pos;
          }
        }
      }
      play();
    }

    public void play()
    {
      try
      {

        if (_rawData != null)
        {
          if (_outputDevice == null)
          {
            _outputDevice = new WaveOutEvent();
            _outputDevice.PlaybackStopped += OnPlaybackStopped;
          }
          if(_rawStream == null)
          {
            WaveFormat fmt = new WaveFormat((int)_format.Frequency, _format.BitsPerSample, _format.Channels);
            _memStream = new MemoryStream(_rawData);
            _rawStream = new RawSourceWaveStream(_memStream, fmt);
            _sampleProvider = new Pcm16BitToSampleProvider(_rawStream);
            _outputDevice.Init(_sampleProvider);
          }
          _playbackState = PlaybackState.Playing;
          _outputDevice.Play();
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("could not play wav:" + ex.ToString(), enLogType.ERROR);
      }
    }


    public double calcEffVoltage(double tStart = 0, double tEnd = 20, bool ac = true)
    {
      double retVal = 0;
      int iStart = timeToIndex(tStart);
      int iEnd = timeToIndex(tEnd);
      short mean = ac ? calcMean(iStart, iEnd) : (short)0;

      double sum = 0;
      double dt = 1.0 / (double)_format.Frequency;
      double t = _data.WaveData.Length / _format.Frequency;
      for(int i = iStart; i < iEnd; i++)
        sum += (long)(_data.WaveData[i] - mean) * (long)(_data.WaveData[i] - mean) * dt;

      retVal =   Math.Sqrt(t * (double)(sum) ) / 32768;

      return retVal;
    }
    public void stop()
    {
      _outputDevice?.Stop();
      _playbackState = PlaybackState.Stopped;
      Thread.Sleep(20);
    }

    public short calcMean(int iStart, int iEnd)
    {
      short retVal = 0;
      long sum = 0;
      for (int i = iStart; i < iEnd; i++)
        sum += _data.WaveData[i];

      retVal = (short)(sum / (iEnd - iStart));
      return retVal;
    }

    private int timeToIndex(double t)
    {
      int retVal = (int)(t * _format.Frequency);
      if(retVal < 0)
        retVal = 0;
      if(retVal >= _data.WaveData.Length)
        retVal = _data.WaveData.Length - 1;
      return retVal;
    }

    private void OnPlaybackStopped(object sender, StoppedEventArgs args)
    {
      try
      {
        _outputDevice.Dispose();
        _outputDevice = null;
        _rawStream.Dispose();
        _rawStream = null;
        _memStream.Dispose();
        _memStream = null;
        _sampleProvider = null;
        stop();
      }
      catch (Exception ex)
      {
        DebugLog.log("error stopping WAV play: " + ex.ToString(), enLogType.ERROR);
      }
    }


    private void applyHetFrequency(double f_HET)
    {
      for (int i = 0; i < AudioSamples.Length; i++)
      {
        double s = Math.Sin(f_HET / _format.Frequency * i * 2 * Math.PI);
        AudioSamples[i] = (short)(s * AudioSamples[i]);
      }
    }
  }
}
