#include "cunittest.h"
#include "unittest.h"

CUnitTest::CUnitTest(clCondParser& parser) :
m_parser(parser){

}

CUnitTest::~CUnitTest() {
  for(size_t i = 0; i < m_list.size(); i++) {
    delete m_list[i];
  }
}

#define TEST(f,t,r)   m_list.push_back(new stTestCase((f),(t),(r)))
#define TESTC(f,t,re, im)   m_list.push_back(new stTestCase((f),(t),(re),(im)))

void CUnitTest::init() {
  TEST("1+2", RT_INT64, (uint64_t)3);
  TEST("1-2", RT_INT64, (int64_t)-1);
  TEST("3*2", RT_UINT64, (uint64_t)6);
  TEST("3/2.0", RT_FLOAT, 1.5);
  TEST("1+2.5", RT_FLOAT, 3.5);

  TEST("1.2e3", RT_FLOAT, 1200.0);
  TEST("1.2e-3", RT_FLOAT, 0.0012);

  TEST("sqrt(4)", RT_FLOAT, 2.0);
  TESTC("sqrt(-1)", RT_COMPLEX, 0, -1);
}

int32_t CUnitTest::check() {
  TS_INIT_TESTCASE();
  int32_t retVal = 0;
  for(size_t i = 0; i < m_list.size(); i++) {
     std::string str = m_list[i]->str;
     clAnyType result = m_parser.parse(&str);
     TS_ASSERT_INT_EQ_I(m_list[i]->type, result.getType(), "type of result", i);
     clAnyType r = result;
     r.changeType(RT_STR);
     std::cout << str << "  parser result: " << r.getString() << std::endl;

     switch(result.getType()) {
       case RT_INT64:
         TS_ASSERT_LL_EQ_I(m_list[i]->val.m_Int64, result.getInt64(), "result value int64_t", i);
         break;
       case RT_UINT64:
       case RT_HEXVAL:
         TS_ASSERT_LL_EQ_I(m_list[i]->val.m_Uint64, result.getUint64(), "result value uint64_t", i);
         break;
       case RT_BOOL:
       TS_ASSERT_TRUE_I(m_list[i]->val.m_Bool == result.getBool(), "result value bool", i);
       break;
       case RT_FLOAT:
         TS_ASSERT_FLOAT_EQ_I(m_list[i]->val.m_Double, result.getFloat(), "result value double", i);
         break;
       case RT_COMPLEX:
         TS_ASSERT_FLOAT_EQ_I(m_list[i]->val.m_complex.re, result.getComplexRe(), "result value complex Re", i);
         TS_ASSERT_FLOAT_EQ_I(m_list[i]->val.m_complex.im, result.getComplexIm(), "result value complex Im", i);
         break;
       case RT_COMMENT:
       case RT_STR:
       case RT_FORMULA:
         TS_ASSERT_FALSE_I(true, "test not implemented", i);
         break;

     }
  }
  std::cout << m_list.size() << " tests" << std::endl;
  TS_PRINT_RESULT();
  return retVal;
}
