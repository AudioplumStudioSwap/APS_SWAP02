using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Aube
{
	//!	@class	EditorFuncs
	//!
	//!	@brief	list of useful methods to use for edition purposes.
	public static class EditorFuncs
	{
		//! @brief	Create a string to be displayed from the name of a variable
		//!			This method will remove the m_ preffix and use CamelCase parsing to add spaces between each word. 
		public static string ToDisplayableName(string variableName)
		{
			string str = variableName;

			// removes m_ prefix
			if(str.StartsWith("m_"))
			{
				str = str.Substring(2);
			}

			if(str.Length > 0  &&  str[0] >= 'a'  &&  str[0] <= 'z')
			{
				string firstChar = str.Substring(0, 1);
				firstChar = firstChar.ToUpperInvariant();
				str = firstChar + str.Substring(1, str.Length - 1);
			}

			// split from each word (using uppercase letter)
			int charIndex = 0;
			bool previousCharWasUppercased = true;
			while(charIndex < str.Length)
			{
				bool insertSpace = false;

				if(str[charIndex] >= 'A'  &&  str[charIndex] <= 'Z')
				{
					if(previousCharWasUppercased == false)
					{
						insertSpace = true;
					}
					else if(charIndex > 0  &&  charIndex + 1 < str.Length  &&  (str[charIndex + 1] < 'A'  ||  str[charIndex + 1] > 'Z'))
					{
						insertSpace = true;
					}

					previousCharWasUppercased = true;
				}
				else
				{
					previousCharWasUppercased = false;
				}

				if(insertSpace)
				{
					str = str.Insert(charIndex, " ");
					previousCharWasUppercased = true;
					charIndex += 2;
				}
				else
				{
					++charIndex;
				}
			}

			return str;
		}

		public static object GetObject(SerializedProperty a_property)
		{
			string path = a_property.propertyPath.Replace(".Array.data[", "[");
			object obj = a_property.serializedObject.targetObject;
			string[] elements = path.Split('.');
			foreach(string element in elements)
			{
				if(element.Contains("["))
				{
					string elementName = element.Substring(0, element.IndexOf("["));
					int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[","").Replace("]",""));
					obj = GetValue(obj, elementName, index);
				}
				else
				{
					obj = GetValue(obj, element);
				}
			}
			return obj;
		}

		private static object GetValue(object source, string name)
		{
			if(source == null)
				return null;
			System.Type type = source.GetType();
			System.Reflection.FieldInfo f = null;
			System.Reflection.PropertyInfo p = null;
			while(f == null  &&  p == null  &&  type != null)
			{
				f = type.GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy);
				if(f == null)
				{
					p = type.GetProperty(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.FlattenHierarchy);
				}

				type = type.BaseType;
			}

			if(f != null)
			{
				return f.GetValue(source);
			}
			if(p != null)
			{
				return p.GetValue(source, null);
			}
			return null;
		}
		private static object GetValue(object source, string name, int index)
		{
			System.Collections.IEnumerable enumerable = GetValue(source, name) as System.Collections.IEnumerable;
			System.Collections.IEnumerator enm = enumerable.GetEnumerator();
			while(index-- >= 0)
				enm.MoveNext();
			return enm.Current;
		}
	}
} // namespace Aube