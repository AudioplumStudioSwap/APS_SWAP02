using UnityEngine;
using System.Collections.Generic;

namespace Aube
{
	//! @class Logger
	//!
	//! @brief	instance of logger
	public abstract class Logger
	{
#region Internal
		internal bool _Configure(Dictionary<string, string> parameters) { return Configure(parameters); }

		internal void _Init() { Init(); }

		internal void _Release() { Release(); }

		internal void _Append(Log.Verbosity verbosity, string message, uint blockCount, uint callStackIndex)
		{
			m_currentVerbosity = verbosity;

			string[] messageElements = message.Split('\n');

			for(int elementIndex = 0; elementIndex < messageElements.Length; ++elementIndex)
			{
				string formattedMessage = m_layout.ApplyLayout(verbosity, messageElements[elementIndex], blockCount, callStackIndex + 1);
				Append(formattedMessage);
			}
		}

		internal LoggerLayout Layout
		{
			set{ m_layout = value; }
		}
#endregion

#region Protected
		protected Log.Verbosity CurrentVerbosity
		{
			get{ return m_currentVerbosity; }
		}

		protected bool GetBooleanParameter(Dictionary<string, string> parameters, string identifier, bool defaultValue)
		{
			return GetStringParameter(parameters, identifier, defaultValue.ToString()).ToBoolean();
		}

		protected int GetIntegerParameter(Dictionary<string, string> parameters, string identifier, int defaultValue)
		{
			return GetStringParameter(parameters, identifier, defaultValue.ToString()).ToInteger();
		}

		protected string GetStringParameter(Dictionary<string, string> parameters, string identifier, string defaultValue)
		{
			string parameterString = "";
			parameters.TryGetValue(identifier, out parameterString);

			if(string.IsNullOrEmpty(parameterString))
			{
				parameterString = defaultValue;
			}

			return parameterString;
		}

		protected virtual bool Configure(Dictionary<string, string> parameters)
		{			
			return true;
		}

		protected abstract void Append(string message);
		protected virtual void Init()							{}
		protected virtual void Release()						{}
#endregion

#region Private
		//! layout
		private LoggerLayout m_layout;

		//! current verbosity
		private Log.Verbosity m_currentVerbosity;
#endregion
	}
} // namespace Aube