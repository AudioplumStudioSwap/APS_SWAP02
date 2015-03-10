using System.Collections.Generic;

//! @class ComparerExtensions
//!
//! @brief extension methods for IComparer interface
public static class ComparerExtensions
{
	//! @brief Reverse a comparer
	//! 
	//! @param comparer	comparer to reverse
	public static IComparer<T> Reverse<T>(this IComparer<T> comparer)
	{
		Aube.Assertion.Check(comparer != null, "Null parameter");
		return new Aube.ComparerReverser<T>(comparer);
	}
}