using UnityEngine;
using System.Collections.Generic;

namespace Aube
{
	//! @class	Log
	//!
	//! @brief		set of functions to log information
	//! @details	A configuration file has to be added in Assets/Resources/logConfig.xml
	//!				example :
	//!				<configuration>
	//!					<layout name=NAME>
	//!						<value>FORMAT-STRING-WITH-IDENTIFIERS</value>
	//!						<IDENTIFIER-WITHOUT-PERCENT min=MINIMAL-NUMBER-OF-CHARACTERS max=MAXIMAL-NUMBER-OF-CHARACTERS align={left, center, right} trim={left, center, right} />
	//!						<IDENTIFIER-WITHOUT-PERCENT min=MINIMAL-NUMBER-OF-CHARACTERS max=MAXIMAL-NUMBER-OF-CHARACTERS align={left, center, right} trim={left, center, right} />
	//!						...
	//!					</layout>
	//!					...
	//!					<logger type=NAME-OF-LOGGER-CLASS-WITH-NAMESPACE verbosity=VERBOSITY layout-ref=LAYOUT-NAME>
	//!						<LOGGER-PARAMETER-NAME>LOGGER-PARAMETER-VALUE</LOGGER-PARAMETER-NAME>
	//!						...
	//!					</logger>
	//!					...
	//!				</configuration>
	//!
	//!				To have a list of all layout identifiers, see the documentation of LoggerLayout.
	//!				To have a list of all logger parameters, see the documentation of all classes in the inheritance tree of the logger.
	public class Log : MonoBehaviour
	{
		//! verbosity of the log
		public enum Verbosity
		{
			Debug,
			Info,
			Warning,
			WarningPerf,
			Error,
			Fatal,

			BlockBegin,				// do not use : internal
			BlockEnd,				// do not use : internal
		}

		//! @brief log a debug message 
		//!
		//! @param	message			message to log
		[System.Diagnostics.ConditionalAttribute("_DEBUG")]
		public static void Debug(string message)							{ ms_instance.Message(Verbosity.Debug, message, 2); }
		//! @brief log an informative message 
		//!
		//! @param	message			message to log
		public static void Info(string message)								{ ms_instance.Message(Verbosity.Info, message, 2); }
		//! @brief log a warning message 
		//!
		//! @param	message			message to log
		public static void Warning(string message)							{ ms_instance.Message(Verbosity.Warning, message, 2); }
		//! @brief log a performance warning message 
		//!
		//! @param	message			message to log
		public static void WarningPerf(string message)						{ ms_instance.Message(Verbosity.WarningPerf, message, 2); }
		//! @brief log an error message 
		//!
		//! @param	message			message to log
		public static void Error(string message)							{ ms_instance.Message(Verbosity.Error, message, 2); }
		//! @brief log a fatal error message 
		//!
		//! @param	message			message to log
		public static void Fatal(string message)							{ ms_instance.Message(Verbosity.Fatal, message, 2); }
		//! @brief log a message with verbosity
		//!
		//! @param	verbosity		level of the log message
		//! @param	message			message to log
		public static void Message(Verbosity verbosity, string message)		{ ms_instance.Message(verbosity, message, 2); }

		public class BlockScope
		{
			internal string m_blockName;
			internal Log m_logInstance;
			internal bool m_blocklogged;

			public void Dispose()
			{
				m_logInstance.DisposeBlock(this);
			}
		}

		//! @brief Declare a log block
		public static BlockScope Block(string name)
		{
			return ms_instance.CreateBlock(name);
		}

		//! @Initialize the Log instance
		public static void Init()
		{
			CreateInstance();
		}

#region Private
	#region Configuration
		private void Configure()
		{
			m_layouts = new Dictionary<string, LoggerLayout>();
			m_defaultLayout = new LoggerLayout();

			m_loggers = new SortedMultiList<Verbosity, Logger>(Comparer<Verbosity>.Default.Reverse());
			m_blockStack = new Stack<BlockScope>();

			TextAsset configFile = Resources.Load<TextAsset>("logConfig");
			Assertion.Check(configFile != null, "The configuration file is missing. The file 'logConfig.xml' has not been found in a Resources folder.");
			System.IO.MemoryStream configStream = new System.IO.MemoryStream(configFile.bytes);
			System.Xml.XmlDocument configXmlDocument = new System.Xml.XmlDocument();
			configXmlDocument.Load(configStream);
			_Configure(configXmlDocument);
		}

		private void _Configure(System.Xml.XmlDocument configXmlDocument)
		{
			System.Xml.XmlElement rootElement = configXmlDocument.DocumentElement;

			System.Xml.XmlNodeList layouts = rootElement.GetElementsByTagName("layout");
			foreach(System.Xml.XmlElement layout in layouts)
			{
				_ConfigureLayout(layout);
			}

			System.Xml.XmlNodeList loggers = rootElement.GetElementsByTagName("logger");
			foreach(System.Xml.XmlElement logger in loggers)
			{
				_ConfigureLogger(logger);
			}
		}

		private void _ConfigureLayout(System.Xml.XmlElement configXmlLayout)
		{
			string name = configXmlLayout.GetAttribute("name");
			if(string.IsNullOrEmpty(name))
			{
				UnityEngine.Debug.LogError("A logger layout exists without a name.");
				return;
			}

			if(m_layouts.ContainsKey(name))
			{
				UnityEngine.Debug.LogError("2 logger layouts have the same name : " + name + ".");
				return;
			}

			LoggerLayout layout = new LoggerLayout();
			if(layout.Configure(configXmlLayout.ChildNodes))
			{
				m_layouts.Add(name, layout);
			}
			else
			{
				UnityEngine.Debug.LogError("The logger layout named " + name + " failed to properly configure itself.");
			}
		}

		private void _ConfigureLogger(System.Xml.XmlElement configXmlLogger)
		{
		#if !UNITY_EDITOR  &&  (UNITY_WEBPLAYER || UNITY_SAMSUNGTV)
			return;
		#else
			string loggerType = configXmlLogger.GetAttribute("type");

			string assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
			System.Runtime.Remoting.ObjectHandle handle = System.Activator.CreateInstance(assemblyName, loggerType);
			Logger logger = (handle == null? null : handle.Unwrap()) as Logger;
			if(handle == null  ||  logger == null)
			{
				UnityEngine.Debug.LogError("The logger named " + loggerType + " has not been found.");
				return;
			}

			Dictionary<string, string> parameters = new Dictionary<string, string>(configXmlLogger.ChildNodes.Count);
			foreach(System.Xml.XmlElement parameter in configXmlLogger.ChildNodes)
			{
				parameters.Add(parameter.Name, parameter.InnerText);
			}

			if(logger._Configure(parameters))
			{
				// get layout
				string layout = configXmlLogger.GetAttribute("layout-ref");
				if(string.IsNullOrEmpty(layout) == false  &&  m_layouts.ContainsKey(layout) == false)
				{
					UnityEngine.Debug.LogError("The logger named " + loggerType + " has an invalid layout reference.");
					return;
				}
				if(string.IsNullOrEmpty(layout))
				{
					logger.Layout = m_defaultLayout;
				}
				else
				{
					logger.Layout = m_layouts[layout];
				}

				// reference it
				string verbosityString = configXmlLogger.GetAttribute("verbosity");
				Verbosity verbosity = string.IsNullOrEmpty(verbosityString)? Verbosity.Debug : verbosityString.ToEnum<Verbosity>();
				m_loggers.Insert(verbosity, logger);
			}
			else
			{
				UnityEngine.Debug.LogError("The logger named " + loggerType + " failed to properly configure itself.");
			}
		#endif
		}
	#endregion

	#region Message
		//! @brief log a message with verbosity
		//!
		//! @param	verbosity		level of the log message
		//! @param	message			message to log
		private void Message(Verbosity verbosity, string message, uint callStackIndex)
		{
			int loggerIndex = 0;
			while(loggerIndex < m_loggers.Count  &&  verbosity >= m_loggers[loggerIndex].Key)
			{
				m_loggers[loggerIndex].Value._Append(verbosity, message, (uint)m_blockStack.Count, callStackIndex + 1);

				++loggerIndex;
			}
		}
	#endregion

	#region Block Management
		private BlockScope CreateBlock(string name)
		{
			BlockScope block = new BlockScope();
			block.m_logInstance = this;
			block.m_blockName = name;

			Message(Verbosity.BlockBegin, block.m_blockName, 3);

			m_blockStack.Push(block);

			return block;
		}

		private void DisposeBlock(BlockScope block)
		{
			Assertion.Check(block.m_logInstance == this, "This block is not registered with this log instance.");
			Assertion.Check (block == m_blockStack.Peek(), "A log block has not been released after use : " + m_blockStack.Peek ().m_blockName);

			m_blockStack.Pop();

			Message(Verbosity.BlockEnd, block.m_blockName, 3);
		}
	#endregion

	#region Enable/Disable
		public void OnEnable()
		{
			Configure();

			// initialize loggers
			foreach(KeyValuePair<Verbosity, Logger> kvp in m_loggers)
			{
				kvp.Value._Init();
			}

			// config log
			BlockScope scope = Block("System Data");
			{
				Info("Operating System : " 	+ SystemInfo.operatingSystem);
				Info("    System specs : " 	+ SystemInfo.processorType + " - #" + SystemInfo.processorCount + " (" 
				     						+ SystemInfo.systemMemorySize + ")");
				Info("    Device specs : " 	+ SystemInfo.deviceName + " " + SystemInfo.deviceModel + " ["
				     						+ SystemInfo.deviceUniqueIdentifier + "]");
				Info("   Graphic specs : " 	+ SystemInfo.graphicsDeviceID + ":" + SystemInfo.graphicsDeviceName + " - " 
				     						+ SystemInfo.graphicsDeviceVendorID + ":" + SystemInfo.graphicsDeviceVendor
											+ " (" + SystemInfo.graphicsMemorySize + "MB, Shader Level " + SystemInfo.graphicsShaderLevel + ")");
			}
			scope.Dispose();
		}

		private void OnDisable()
		{
			// initialize loggers
			foreach(KeyValuePair<Verbosity, Logger> kvp in m_loggers)
			{
				kvp.Value._Release();
			}

			Assertion.Check(m_blockStack.Count == 0, m_blockStack.Count + " log blocks are still referenced.");
		}
	#endregion
	
	#region Attributes
		//! layouts
		Dictionary<string, LoggerLayout> m_layouts;
		LoggerLayout m_defaultLayout;

		//! loggers
		SortedMultiList<Verbosity, Logger> m_loggers;

		//! block stack
		Stack<BlockScope> m_blockStack;
	#endregion

	#region Singleton
		static void CreateInstance()
		{
			GameObject logManagerObject = new GameObject("LogManager");
			logManagerObject.SetActive(false);
			UnityEngine.Object.DontDestroyOnLoad(logManagerObject);
			ms_instance = logManagerObject.AddComponent<Log>();
			logManagerObject.SetActive(true);
		}

		//! singleton instance
		static Log ms_instance;
	#endregion
#endregion
	}
} // namespace Aube