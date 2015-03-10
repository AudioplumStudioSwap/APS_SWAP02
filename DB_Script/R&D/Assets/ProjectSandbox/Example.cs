using UnityEngine;
using System.Collections;

namespace Aube.Sandbox
{
	[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]

	//! @class Example
	//!
	//! @brief Register an example in the sand box
	public class Example : System.Attribute
	{
		public Example(string a_examplePath)
		{
			m_path = a_examplePath;
		}

		public string path
		{
			get{ return m_path; }
		}

		public System.Type type
		{
			get{ return m_type; }
			set{ m_type = value; }
		}

#region Private
		private readonly string m_path;
		private System.Type m_type;
#endregion
	}
}