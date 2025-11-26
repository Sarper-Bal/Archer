using System;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Data.Symbols
{
	[Serializable]
	public struct SymbolicFloat : IComparable<SymbolicFloat>
	{
		[SerializeField, Range(0, 999)] float _number;
		[SerializeField] int _symbol;
		[SerializeField] int _digits;
		
		public SymbolicFloat(int symbol, float i, int digits)
		{
			_symbol = symbol;
			_number = i;
			_digits = digits;
		}
		
		public int Digits => _digits;

		public float Number
		{
			get => _number;
			set
			{
				_number = value;
				AutoTrimNumber(this);
			}
		}

		public int Symbol
		{
			get => _symbol;
			set => _symbol = value;
		}

		public static SymbolicFloat operator +(SymbolicFloat a, SymbolicFloat b)
		{
			a = AutoTrimNumber(a);
			b = AutoTrimNumber(b);

			if (a._symbol == b._symbol)
			{
				var result = new SymbolicFloat();
				result._number = a._number + b._number;
				result._symbol = a._symbol;
				result = AutoTrimNumber(result);
				return result;
			}

			int diff = Mathf.Abs(a._symbol - b._symbol);
			int decimalDiff = diff * a._digits;
			if (a._symbol > b._symbol)
			{
				float result = b._number / Mathf.Pow(10, decimalDiff);
				a._number += result;
				return a;
			}
			else
			{
				float result = a._number / Mathf.Pow(10, decimalDiff);
				b._number += result;
				return b;
			}
		}
		
		public static SymbolicFloat operator -(SymbolicFloat a, SymbolicFloat b)
		{
			a = AutoTrimNumber(a);
			b = AutoTrimNumber(b);

			if (a._symbol == b._symbol)
			{
				var result = new SymbolicFloat();
				result._number = Mathf.Clamp(a._number - b._number, 0, int.MaxValue);
				result._symbol = a._symbol;
				result = AutoTrimNumber(result);
				return result;
			}
			else
			{
				return a._symbol > b._symbol ? a : b;
			}
		}
		
		public static SymbolicFloat operator *(SymbolicFloat a, float b)
		{
			var result = new SymbolicFloat();
			result._symbol = a._symbol;
			result._number = a._number * b;
			result = AutoTrimNumber(result);
			return result;
		}

		public static bool operator >=(SymbolicFloat a, SymbolicFloat b)
		{
			a = AutoTrimNumber(a);
			b = AutoTrimNumber(b);

			if (a._symbol >= b._symbol)
			{
				return true;
			}

			if (a._symbol == b._symbol)
			{
				if (a._number >= b._number)
				{
					return true;
				}
			}

			return false;
		}
		
		public static bool operator <=(SymbolicFloat a, SymbolicFloat b)
		{
			a = AutoTrimNumber(a);
			b = AutoTrimNumber(b);

			if (a._symbol <= b._symbol)
			{
				return true;
			}

			if (a._symbol == b._symbol)
			{
				if (a._number <= b._number)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Calculate how many times this number is greater than b. For example:
		/// <code>
		///	new SymbolicFloat(6, 0).HowBig(new SymbolicFloat(2, 0));
		/// </code>
		/// results in 3 which means it is 3 times bigger.
		/// </summary>
		public float HowBig(SymbolicFloat b)
		{
			float numberDifference = _number / b._number;
			float result = numberDifference;
			
			if (_symbol == b._symbol)
			{
				return result;
			}
			else
			{
				int symbolDifference = _symbol - b._symbol;
				float decimalBasedSymbolDifference = Mathf.Pow(10, _digits * symbolDifference);
				result = decimalBasedSymbolDifference * numberDifference;
			}
			return result;
		}
		
		public static SymbolicFloat AutoTrimNumber(SymbolicFloat symbolicFloat)
		{
			double digit = Math.Floor(Math.Log10(symbolicFloat._number));
			int output = Mathf.RoundToInt((float)digit / symbolicFloat._digits);

			if (output > 0)
			{
				var result = new SymbolicFloat();
				result._number = symbolicFloat._number / (Mathf.Pow(10, output * symbolicFloat._digits));
				result._symbol = symbolicFloat._symbol + output;
				return result;
			}

			return symbolicFloat;
		}
		
		public int CompareTo(SymbolicFloat other)
		{
			if (_symbol > other._symbol)
			{
				return 1;
			}
			else if (_symbol < other._symbol)
			{
				return -1;
			}
			else if (_symbol == other._symbol)
			{
				if (_number > other._number)
				{
					return 1;
				}
				else if (_number < other._number)
				{
					return -1;
				}
				else
				{
					return 0;
				}
			}
			Debug.LogError("Comparison of two SymbolicFloats has been failed");
			return -99;
		}
	}
}
