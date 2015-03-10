using System.Collections.Generic;

namespace Aube
{
	//! @class ComparerReverser
	//!
	//! @brief Reversed comparer of the wrapped comparer
	public class ComparerReverser<T> : IComparer<T>
	{
	//*************************************************************************
	// Constructors
	//*************************************************************************
		public ComparerReverser(IComparer<T> comparer)
		{
			Assertion.Check(comparer != null, "Null parameter.");
			m_wrappedComparer = comparer;
		}

	//*************************************************************************
	// Inherited Methods
	//*************************************************************************
		//! @brief Compare two elements
		//! 
		//! @param operand1		first element
		//! @param operand2		second element
		//!
		//! @return a negative value if the first element is less than the second,
		//!			0 if the elements are equals
		//!			a positive value if the first element is greater than the second.
		public int Compare(T operand1, T operand2)
		{
			return m_wrappedComparer.Compare(operand2, operand1);
		}

	//*************************************************************************
	// Private Attributes
	//*************************************************************************
		//! wrapped comparer
		private readonly IComparer<T> m_wrappedComparer;
	}
} // namespace Aube