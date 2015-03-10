using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Aube
{
	//! @class FileLogger
	//!
	//! @brief logger that writes in 1+ files
	public class FileLogger : Logger
	{
#region Inherited Methods from Logger
		protected override bool Configure(Dictionary<string, string> parameters)
		{
			if(base.Configure(parameters) == false)
			{
				return false;
			}

			m_fileName = GetStringParameter(parameters, "file", "");
			if(string.IsNullOrEmpty(m_fileName))
			{
				return false;
			}

			m_rolling = GetBooleanParameter(parameters, "rolling", false);
			if(m_rolling)
			{
				m_rollingSizeLimit = (uint)GetIntegerParameter(parameters, "rollingSizeLimit", 100) * 1024; // expressed as kB in config file
			}

			// checks there is no invalid characters
			char[] invalidChars = Path.GetInvalidFileNameChars();
			bool invalidCharacterFound = false;
			int charIndex = 0;
			m_rollingIndexFormat = "";
			bool lastCharWasRollingIdentifier = false;
			int firstRollingIdentifier = -1;
			while(invalidCharacterFound == false  &&  charIndex < m_fileName.Length)
			{
				if(m_rolling  &&  m_fileName[charIndex] == '*'  &&  (lastCharWasRollingIdentifier  ||  m_rollingIndexFormat == ""))
				{
					m_rollingIndexFormat += "0";
					lastCharWasRollingIdentifier = true;

					if(firstRollingIdentifier == -1)
					{
						firstRollingIdentifier = charIndex;
					}
				}
				else
				{
					int invalidCharIndex = 0;
					while(invalidCharIndex < invalidChars.Length  &&  m_fileName[charIndex] != invalidChars[invalidCharIndex])
					{
						++invalidCharIndex; 
					}

					invalidCharacterFound = invalidCharIndex < invalidChars.Length;

					lastCharWasRollingIdentifier = false;
				}

				++charIndex;
			}

			if(invalidCharacterFound)
			{
				return false;
			}

			if(m_rolling  &&  (m_rollingIndexFormat == ""  ||  firstRollingIdentifier == -1))
			{
				return false;
			}

			if(m_rolling)
			{
				m_fileName.Remove(firstRollingIdentifier, m_rollingIndexFormat.Length);
				m_fileName.Insert(firstRollingIdentifier, "%s");
			}

			return true;
		}

		protected override void Init()
		{
			base.Init();

			string directoryPath = Application.persistentDataPath;
			string[] files = Directory.GetFiles(directoryPath);

			string regexString = (m_rolling)? m_fileName.Replace(@"%s", @"[0,9]{" + m_rollingIndexFormat.Length + "}") : m_fileName;
			regexString = regexString.Replace(".", @"\.");
			Regex regEx = new Regex(regexString);

			foreach(string filePath in files)
			{
				FileInfo info = new FileInfo(filePath);

				string filenameWithExtension = string.IsNullOrEmpty(info.Extension)? info.Name : info.Name + "." + info.Extension;
				if(regEx.IsMatch(filenameWithExtension))
				{
					File.Delete(filePath);
				}
			}

			m_fileIndex = 0;
			m_currentFileSize = 0;
			OpenFile();
		}

		protected override void Release()
		{
			CloseFile();

			base.Release();
		}

		protected override void Append(string message)
		{
			// The logger isn't opened anymore
			if (m_stream == null)
			{
				return;
			}

			byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message + '\n'); 
			m_stream.Write(messageBytes, 0, messageBytes.Length);

			if(m_rolling)
			{
				m_currentFileSize += (uint)messageBytes.Length;
				if(m_currentFileSize >= m_rollingSizeLimit)
				{
					CloseFile();
					++m_fileIndex;
					OpenFile();
				}
			}
		}
#endregion

#region Private
	#region Methods
		private void OpenFile()
		{
			Assertion.Check(m_stream == null, "Invalid call to FileLogger::OpenFile : there is a stream opened.");
			m_stream = File.Create(Application.persistentDataPath + "/" + GetFilename());
		}

		private void CloseFile()
		{
			Assertion.Check(m_stream != null, "Invalid call to FileLogger::CloseFile : there is no stream opened.");
			m_stream.Close();
			m_stream = null;
		}

		private string GetFilename()
		{
			if(m_rolling)
			{
				return string.Format(m_fileName, m_fileIndex.ToString(m_rollingIndexFormat));
			}
			else
			{
				return m_fileName;
			}
		}
	#endregion

	#region Data Attributes
		//! name of the file
		private string m_fileName;

		//! is rolling
		private bool m_rolling;
		private uint m_rollingSizeLimit;
		private string m_rollingIndexFormat;
	#endregion

	#region Runtime Attributes
		private FileStream m_stream;

		//! rolling data
		private uint m_fileIndex;
		private uint m_currentFileSize;
	#endregion
#endregion
	}
} // namespace Aube