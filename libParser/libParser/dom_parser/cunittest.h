#ifndef CUNITTEST_H
#define CUNITTEST_H

#include "clAnyType.h"
#include "clCondParser.h"
#include <vector>

struct stTestCase {
  stTestCase(const char* s, tType t, double v) :
  str(s),
  type(t)
  {
    val.m_Double = v;
  }

  stTestCase(const char* s, tType t, double re, double im) :
  str(s),
  type(t)
  {
    val.m_complex.re = re;
    val.m_complex.im = im;
  }

  stTestCase(const char* s, tType t, int64_t v) :
  str(s),
  type(t)
  {
    val.m_Int64 = v;
  }

  stTestCase(const char* s, tType t, uint64_t v) :
  str(s),
  type(t)
  {
    val.m_Uint64 = v;
  }

  stTestCase(const char* s, tType t, bool v) :
  str(s),
  type(t)
  {
    val.m_Bool = v;
  }

  const char* str;
  tType type;
  tValue val;
};

class CUnitTest {
  public:
    CUnitTest(clCondParser& parser);
    virtual ~CUnitTest();
    void init();
    int32_t check();

  protected:
    CUnitTest();

  private:
    clCondParser& m_parser;
    std::vector<stTestCase*> m_list;
};

#endif // CUNITTEST_H

