﻿/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System.Runtime.InteropServices;
using System;
using System.Windows;
using BatInspector.Controls;
using Microsoft.Win32;
using System.Windows.Interop;
using libParser;
using System.IO;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für frmWavFile.xaml
  /// </summary>
  public partial class frmWavFile : Window
  {
    ViewModel _model;
    MainWindow _parent;

    public frmWavFile(ViewModel model, MainWindow parent)
    {
      InitializeComponent();
      _model = model;
      _parent = parent;
      int wLbl = 110;
      _ctlFileName.setup("File name", Controls.enDataType.STRING, 0, wLbl);

      _ctlFileType.setup("File Type ID", enDataType.STRING, 0, wLbl);
      _ctlFileLength.setup("File length", enDataType.UINT, 0, wLbl);
      _ctlMediaTypeId.setup("Media Type ID", enDataType.STRING, 0, wLbl);

      _ctlChunkId.setup("Chunk ID", enDataType.STRING, 0, wLbl);
      _ctlChunkSize.setup("Chunk Size", enDataType.UINT, 0, wLbl);
      _ctlFormatTag.setup("Format Tag", enDataType.UINT, 0, wLbl);
      _ctlChannels.setup("Channel count.", Controls.enDataType.UINT, 0, wLbl);
      _ctlSamplingRate.setup("Sampling rate", Controls.enDataType.UINT, 0, wLbl, true, samplingRateChanged);
      _ctlAvgBytesPerSec.setup("Avg Bytes/sec", enDataType.UINT, 0, wLbl);
      _ctlBlockAlign.setup("Block Align", enDataType.UINT, 0, wLbl);
      _ctlBitsPerSample.setup("Bits per Chan", Controls.enDataType.UINT, 0, wLbl);

      _ctlSamples.setup("Nr of Samples", enDataType.INT, 0, wLbl);
    }


    private void _btnOpen_Click(object sender, RoutedEventArgs e)
    {
      string wavFile = "";
      try
      {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "WAV files (*.wav)|*.wav|All files (*.*)|*.*";
        if (openFileDialog.ShowDialog() == true)
        {
          wavFile = openFileDialog.FileName;
          _model.WavFile.readFile(wavFile);
          _ctlFileName.setValue(openFileDialog.FileName);

          _ctlFileType.setValue(_model.WavFile.WavHeader.FileTypeId);
          _ctlFileLength.setValue(_model.WavFile.WavHeader.FileLength);
          _ctlMediaTypeId.setValue(_model.WavFile.WavHeader.MediaTypeId);

          _ctlChunkId.setValue(_model.WavFile.FormatChunk.ChunkId);
          _ctlChunkSize.setValue(_model.WavFile.FormatChunk.ChunkSize);
          _ctlFormatTag.setValue((uint)_model.WavFile.FormatChunk.FormatTag);
          _ctlChannels.setValue((uint)_model.WavFile.FormatChunk.Channels);
          _ctlSamplingRate.setValue(_model.WavFile.FormatChunk.Frequency);
          _ctlAvgBytesPerSec.setValue(_model.WavFile.FormatChunk.AverageBytesPerSec);
          _ctlBlockAlign.setValue((uint)_model.WavFile.FormatChunk.BlockAlign);
          _ctlBitsPerSample.setValue((uint)_model.WavFile.FormatChunk.BitsPerSample);

          _ctlSamples.setValue(_model.WavFile.AudioSamples.Length);

          double duration = (double)_model.WavFile.AudioSamples.Length / _model.WavFile.FormatChunk.Frequency;
          AnalysisFile ana = new AnalysisFile(openFileDialog.FileName, (int)_model.WavFile.FormatChunk.Frequency, duration);
          _parent.setZoom(Path.GetFileName(openFileDialog.FileName), ana, Path.GetDirectoryName(openFileDialog.FileName), null, enModel.BAT_DETECT2);
        }
      }
      catch (Exception ex)
      {
        DebugLog.log($"error opening WAV file {wavFile}: {ex.ToString()}", enLogType.ERROR);
      }
    }

    private void _btnPlay_Click(object sender, RoutedEventArgs e)
    {
      _model.WavFile.play();
    }

    private void _btnSave_Click(object sender, RoutedEventArgs e)
    {
      _model.WavFile.saveFile();
    }

    private void samplingRateChanged(enDataType type, object val)
    {
      int sr = _ctlSamplingRate.getIntValue();
  //    uint oldSr = _model.WavFile.FormatChunk.Frequency;
      _model.WavFile.FormatChunk.Frequency = (uint)sr;
  //    double fact = (double)sr / oldSr;
  //    _model.WavFile.FormatChunk.AverageBytesPerSec = (uint)(fact * _model.WavFile.FormatChunk.AverageBytesPerSec);
    }

    private void windowLoaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }

    private void _btnClose_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
    }
  }
}
