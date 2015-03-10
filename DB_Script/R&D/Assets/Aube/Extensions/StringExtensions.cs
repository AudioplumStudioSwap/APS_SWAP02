using UnityEngine;

//! @class StringExtensions
//!
//! @brief Extension class methods for string
public static class StringExtensions
{
	//! @brief convert a string to a boolean
	//! extension method of class 'string'
	public static bool ToBoolean(this string str)
	{
		if(str == "true"  ||  str == "True")
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	//! @brief convert a string to an integer
	//! extension method of class 'string'
	public static int ToInteger(this string str)
	{
		return System.Convert.ToInt32(str);
	}

	//! @brief convert a string to a float
	//! extension method of class 'string'
	public static float ToFloat(this string str)
	{
		return System.Convert.ToSingle(str);
	}

	//! @brief convert a string to an enumeration value
	//! extension method of class 'string'
	public static T ToEnum<T>(this string str)
	{
		string[] enumNames = System.Enum.GetNames(typeof(T));
		int index = 0;
		while(index < enumNames.Length  &&  enumNames[index] != str)
		{
			++index;
		}

		Aube.Assertion.Check(index < enumNames.Length, "Invalid enumeration string '" + str + "' for type " + typeof(T).Name);
		return (T)System.Enum.GetValues(typeof(T)).GetValue(index);
	}
}