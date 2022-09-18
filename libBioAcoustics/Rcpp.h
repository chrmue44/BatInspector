#pragma once
#define NOMINMAX
#include <vector>
#include <string>

#ifndef M_PI
#define M_PI (3.14159265358979323846)
#endif

namespace Rcpp
{
	class List
	{};

	class NumericVector
	{
	private:
		std::vector<double> _list;

	public:
		NumericVector()
		{
		}

		void push_back(double val)
		{
			_list.push_back(val);
		}

		void push_front(double val)
		{
			_list.push_back(0);
			for (int i = _list.size() - 1; i > 0 ; i--)
				_list[i] = _list[i - 1];
			_list[0] = val;
		}

		size_t size() { return _list.size(); }

		double& operator[](int i)
		{
			return _list[i];
		}
	}; 

	class StringVector
	{
	private:
		std::vector<std::string> _list;
	};
}