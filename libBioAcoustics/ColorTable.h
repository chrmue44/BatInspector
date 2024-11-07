#pragma once
#include <cstdint>

#define CNT_ITEMS      5
#define CTABLE_SIZE  100

struct stColorItem
{
  int Color;
  int Value;
};

struct stColorChans
{
  uint8_t red;
  uint8_t green;
  uint8_t blue;
};

class ColorTable
{
public:
  void set(int* colorTable);
  stColorChans getColor(double val, double min, double max);

private:
  void createColorLookupTable();
  uint8_t getColorFromGradient(double val, stColorItem list[]);

  stColorItem _red[CNT_ITEMS];
  stColorItem _green[CNT_ITEMS];
  stColorItem _blue[CNT_ITEMS];

  stColorChans _colorTable[CTABLE_SIZE];
};

