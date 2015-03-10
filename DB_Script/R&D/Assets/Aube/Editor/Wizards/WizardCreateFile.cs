using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace Aube
{
	//! @class WizardFolderSelection
	//!
	//! @brief Editor wizard to select a folder
	public abstract class WizardFolderSelection : ScriptableWizard
	{
		//*********************************************************************
		// Private constants
		//*********************************************************************
		private static Regex ms_pathRegex = new Regex(string.Format("[{0}]", Regex.Escape(new string(System.IO.Path.GetInvalidPathChars()))));
		
		//*********************************************************************
		// Methods for Scriptable Wizards
		//*********************************************************************
		protected virtual void OnWizardUpdate()
		{
			isValid = m_filePath.StartsWith(Application.dataPath + "/");
			isValid &= ms_pathRegex.IsMatch(m_filePath) == false;
		}
		
		//*********************************************************************
		// Public Attributes
		//*********************************************************************
		[FolderPath]
		public string m_filePath = Application.dataPath + "/";
	}

	//! @class WizardCreateFile
	//!
	//! @brief Editor wizard to create files
	public abstract class WizardCreateFile : WizardFolderSelection
	{
		//*********************************************************************
		// Private constants
		//*********************************************************************
		private static Regex ms_fileRegex = new Regex(string.Format("[{0}]", Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()))));
		
		//*********************************************************************
		// Methods for Scriptable Wizards
		//*********************************************************************
		protected override void OnWizardUpdate()
		{
			base.OnWizardUpdate();
			isValid &= m_fileName.Length > 0;
			isValid &= ms_fileRegex.IsMatch(m_fileName) == false;
		}
		
		//*********************************************************************
		// Public Attributes
		//*********************************************************************
		public string m_fileName = "default";
	}
} // namespace Aube