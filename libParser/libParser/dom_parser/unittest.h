/* *******************************************************************
 *
 *   minimalistic unit test framework in C
 *
 * ******************************************************************* */

#ifndef UNITTEST_H
#define UNITTEST_H
#include <string.h>
#include <stdio.h>
#include <float.h>



// *******************************************************************
// internal macros to build framework. For usage in unit tests use
// only macros of the second section in the header file

/// compare equality of two float values
#define _TS_EQUAL_FLOAT(X,Y)  ((X < (Y + _TS_float_eps)) && (X > (Y-_TS_float_eps)))

/// print error informations
#define _TS_BASE_PRT_S(S, SIN)  printf("### %s(), line %i %s: ERROR %s: " , __func__, __LINE__, (SIN), (S) ); \
						        _TS_actErrCnt_++
// print values in integer format
#define _TS_BASE_PRT(S, SIN, X, Y) _TS_BASE_PRT_S(S, SIN); printf("expected: %d, actual: %d\n", X, Y)

// print values in long long format
#define _TS_BASE_PRT_LL(S, SIN, X, Y) _TS_BASE_PRT_S(S, SIN); printf("expected: %ld, actual: %ld\n", X, Y)

// print values in integer format
#define _TS_BASE_PRT_LT(S, SIN, T, Y) _TS_BASE_PRT_S(S, SIN); printf("%i not lesser than %i\n", Y, T)

// print values in integer format
#define _TS_BASE_PRT_GT(S, SIN, T, Y) _TS_BASE_PRT_S(S, SIN); printf("%i not greater than %i\n", Y, T)

// print values in float format
#define _TS_BASE_PRT_F(S, SIN, X, Y) _TS_BASE_PRT_S(S, SIN); printf("expected: %f, actual: %f\n", X, Y)

// print values in float format
#define _TS_BASE_PRT_F_LT(S, SIN, T, Y) _TS_BASE_PRT_S(S, SIN); printf("%f not lesser than %f\n", Y, T)

// print values in float format
#define _TS_BASE_PRT_F_GT(S, SIN, T, Y) _TS_BASE_PRT_S(S, SIN); printf("%f not greater than %f\n", Y, T)

// print values in boolean format
#define _TS_BASE_PRT_TRUE(S, SIN) _TS_BASE_PRT_S(S, SIN); puts("expected:TRUE, actual: FALSE\n")

// print values in boolean format
#define _TS_BASE_PRT_FALSE(S, SIN) _TS_BASE_PRT_S(S, SIN); puts("expected:FALSE, actual: TRUE\n")

/// assert equality of two integers
#define _TS_ASSERT_BASE(X, Y, S, SIN)   if ((X) != (Y)) { _TS_BASE_PRT(S, SIN, X, Y); }

/// assert Y < T
#define _TS_ASSERT_BASE_LT(T, Y, S, SIN) if( T <= Y )  {_TS_BASE_PRT_LT(S, SIN, T, Y);}

/// assert Y > T
#define _TS_ASSERT_BASE_GT(T, Y, S, SIN) if( T >= Y )  {_TS_BASE_PRT_GT(S, SIN, T, Y);}

/// print index
#define _TS_PRT_IDX(I) char str[32]; \
	                   strcpy(str, "index "); \
                       sprintf(&str[strlen(str)], "%lu",(I));

/// assert equality of two integers with index information
#define _TS_ASSERT_BASE_I(X, Y, S, I) _TS_PRT_IDX(I); _TS_ASSERT_BASE(X, Y, S, str);

/// assert equality of two long long integers
#define _TS_ASSERT_BASE_LL(X, Y, S, SIN)   if ((X) != (Y)) { _TS_BASE_PRT_LL(S, SIN, X, Y); }

/// assert equality of two integers with index information
#define _TS_ASSERT_BASE_LL_I(X, Y, S, I) _TS_PRT_IDX(I); _TS_ASSERT_BASE_LL(X, Y, S, str);

/// assert equality of two floats
#define _TS_ASSERT_BASE_F(X, Y, S, SIN)  if (!_TS_EQUAL_FLOAT((X),(Y))) {_TS_BASE_PRT_F(S, SIN, X, Y);}

/// assert Y < T
#define _TS_ASSERT_BASE_F_LT(T, Y, S, SIN) if( T <= Y )  {_TS_BASE_PRT_F_LT(S, SIN, T, Y);}

/// assert equality of two floats with index information
#define _TS_ASSERT_BASE_F_I(X, Y, S, I) _TS_PRT_IDX(I); _TS_ASSERT_BASE_F(X, Y, S, str);

/// assert condition true
#define _TS_ASSERT_BASE_TRUE(X, S, SIN)  if (!(X)) {_TS_BASE_PRT_TRUE(S, SIN);}

/// assert condition false
#define _TS_ASSERT_BASE_FALSE(X, S, SIN)  if ((X)) {_TS_BASE_PRT_FALSE(S, SIN);}

/// print test result
#define _TS_PRT_NAME(S)  printf("### Test case: %s", (S)); \
					    if(_TS_actErrCnt_ != 0) \
					      printf(":%i ERROR(S)!!!\n", _TS_actErrCnt_); \
					    else \
					      printf(": OK\n\n");\

// ***** end of internal part ****************************************




// *******************************************************************
// All following macros are for usage in unit tests

/// initialize test suite
#define TS_INIT_TESTSUITE() int _TS_suiteErrCnt_ = 0

/// initialize local test
#define TS_INIT_TESTCASE() int _TS_actErrCnt_ = 0; float _TS_float_eps = 1e-8

#define TS_SET_FLOAT_EPS(e) _TS_float_eps = e

/// get total nr of errors since call of TS_INIT_TESTCASE()
#define TS_GET_ERRCNT() _TS_actErrCnt_

/// get total nr of errors since call of TS_INIT_TESTSUITE()
#define TS_GET_SUITE_ERRCNT() _TS_suiteErrCnt_

/// test two integers for equality with error informations in case of error
/// @param X expected value
/// @param Y actual value
/// @param S additional information string
/// @param I additional information value (eg. index)
#define TS_ASSERT_INT_EQ_I(X, Y, S, I)   do {_TS_ASSERT_BASE_I(X, Y, S, I);} while(0)

/// test two integers for equality with error informations in case of error
/// @param X expected value
/// @param Y actual value
/// @param S additional information string
#define TS_ASSERT_INT_EQ(X, Y, S)   do {_TS_ASSERT_BASE(X, Y, S,"");} while(0)

/// test if integer is lesser than a threshold value with error informations in case of error
/// @param T threshold value
/// @param Y actual value
/// @param S additional information string
#define TS_ASSERT_INT_LT(T, Y, S)   do {_TS_ASSERT_BASE_LT(T, Y, S,"");} while(0)

/// test if integer is greater than a threshold value with error informations in case of error
/// @param T threshold value
/// @param Y actual value
/// @param S additional information string
#define TS_ASSERT_INT_GT(T, Y, S)   do {_TS_ASSERT_BASE_GT(T, Y, S,"");} while(0)

/// test two long longs for equality with error informations in case of error
/// @param X expected value
/// @param Y actual value
/// @param S additional information string
/// @param I additional information value (eg. index)
#define TS_ASSERT_LL_EQ_I(X, Y, S, I)   do {_TS_ASSERT_BASE_LL_I(X, Y, S, I);} while(0)

/// test two long longs for equality with error informations in case of error
/// @param X expected value
/// @param Y actual value
/// @param S additional information string
#define TS_ASSERT_LL_EQ(X, Y, S)   do {_TS_ASSERT_BASE_LL(X, Y, S,"");} while(0)

/// test two floats for equality with error informations in case of error
/// @param X expected value
/// @param Y actual value
/// @param S additional information string
#define TS_ASSERT_FLOAT_EQ(X, Y, S) do {_TS_ASSERT_BASE_F(X, Y, S,"");} while(0)

/// test if float is lesser than a threshold value with error informations in case of error
/// @param X threshold value
/// @param Y actual value
/// @param S additional information string
#define TS_ASSERT_FLOAT_LT(T, Y, S) do {_TS_ASSERT_BASE_F_LT(T, Y, S,"");} while(0)

/// test two floats for equality with error informations in case of error
/// @param X expected value
/// @param Y actual value
/// @param S additional information string
/// @param I additional information value (eg. index)
#define TS_ASSERT_FLOAT_EQ_I(X, Y, S, I) do {_TS_ASSERT_BASE_F_I(X, Y, S, I);} while(0)

/// test condition true with informations in case of error
/// @param X condition
/// @param S additional information string
#define TS_ASSERT_TRUE(X, S) do { _TS_ASSERT_BASE_TRUE(X, S,"");} while(0)

/// test condition true with informations in case of error
/// @param X condition
/// @param S additional information string
/// @param I additional information value (eg. index)
#define TS_ASSERT_TRUE_I(X, S, I) do { _TS_PRT_IDX(I); _TS_ASSERT_BASE_TRUE(X, S, str);} while(0)

/// test condition false with informations in case of error
/// @param X condition
/// @param S additional information string
#define TS_ASSERT_FALSE(X, S) do { _TS_ASSERT_BASE_FALSE(X, S,"");} while(0)

/// test condition false with informations in case of error
/// @param X condition
/// @param S additional information string
/// @param I additional information value (eg. index)
#define TS_ASSERT_FALSE_I(X, S, I) do {_TS_PRT_IDX(I); _TS_ASSERT_BASE_FALSE(X, S, str);} while(0)

/// print result of test case
/// @param T name of test case
#define TS_PRINT_RESULT_N(S) do {_TS_PRT_NAME(S)} while(0)

/// print result of test case, name is function name
#define TS_PRINT_RESULT() do {_TS_PRT_NAME(__func__)} while(0)

/// execute test
/// @param f function name
/// @param t type of the argument to function f
/// @param d argument for funtion f
#define EXEC_TEST(f, t, d) int f(t d); do {setup(d); _TS_suiteErrCnt_ += f(d); teardown(d);} while(0)



#endif //#ifndef UNITTEST_H
