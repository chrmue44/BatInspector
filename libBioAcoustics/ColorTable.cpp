#include "ColorTable.h"

void ColorTable::set(int* colorTable)
{
  for (int i = 0; i < 5; i++)
  {
    _red[i].Color = *colorTable++;
    _red[i].Value = *colorTable++;
  }
  for (int i = 0; i < 5; i++)
  {
    _green[i].Color = *colorTable++;
    _green[i].Value = *colorTable++;
  }
  for (int i = 0; i < 5; i++)
  {
    _blue[i].Color = *colorTable++;
    _blue[i].Value = *colorTable++;
  }
  createColorLookupTable();
}


uint8_t ColorTable::getColorFromGradient(double val, stColorItem list[])
{
  double retVal = 100;
  for (int i = 0; i < 5; i++)
  {
    if ((val >= list[i].Value) && (val < list[i + 1].Value))
    {
      double m = (double)(list[i + 1].Color - list[i].Color) / (list[i + 1].Value - list[i].Value);
      double b = list[i].Color - m * list[i].Value;
      retVal = m * val + b;
    }
  }
  return (uint8_t)retVal;
}


void ColorTable::createColorLookupTable()
{
  for (int i = 0; i < CTABLE_SIZE; i++)
  {
    stColorChans c;
    c.red = getColorFromGradient((double)i, _red);
    c.green = getColorFromGradient((double)i, _green);
    c.blue = getColorFromGradient((double)i, _blue);
    _colorTable[i] = c;
  }
}


stColorChans ColorTable::getColor(double val, double min, double max)
{
  if (val < min)
    return _colorTable[0];
  else if (val > max)
    return _colorTable[CTABLE_SIZE - 1];
  else
  {
    int i = (int)((val - min) / (max - min) * CTABLE_SIZE);
    if (i < CTABLE_SIZE)
      return _colorTable[i];
    else
      return _colorTable[CTABLE_SIZE - 1];
  }
}
