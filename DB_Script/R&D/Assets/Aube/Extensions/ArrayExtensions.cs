using System;

//! @class ArrayExtensions
//!
//! @brief Extension class methods for System.Array
public static class ArrayExtensions
{
	//! @brief Populate an existing array with the value given
	//!
	//! @this	a_array		array to populate
	//! @param	a_value		value to use
	public static void Populate<T>(this T[] a_array, T a_value)
	{
		for(int index = 0; index < a_array.Length; ++index)
		{
			a_array[index] = a_value;
		}
	}

	//! @brief Check if a value is present in the array
	//!
	//! @this	a_array		array
	//! @param	a_value		value to check
	public static bool Contains<T>(this T[] a_array, T a_value)
	{
		int index = 0;
		while(index < a_array.Length  &&  Equals(a_array[index], a_value) == false)
		{
			++index;
		}

		return index < a_array.Length;
	}
}