using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace Aube
{
	//! @class WizardCreateAsset
	//!
	//! @brief Editor wizard to create asset files
	public class WizardCreateAsset<T> : WizardCreateFile where T : ScriptableObject
	{		
	//*********************************************************************
	// Methods for Scriptable Wizards
	//*********************************************************************
		public void OnWizardCreate()
		{
            if(m_filePath.StartsWith(Application.dataPath))
            {
                m_filePath = m_filePath.Substring(Application.dataPath.Length - "Assets".Length);
            }

            string assetRelativePath = m_filePath + m_fileName + ".asset";
            string[] folders = assetRelativePath.Split(new char[]{ '/' }, System.StringSplitOptions.RemoveEmptyEntries);

            string currentPathToCreate = Application.dataPath;
            for(int folderIndex = 1; folderIndex < folders.Length - 1; ++folderIndex)
            {
                System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(currentPathToCreate + "/" + folders[folderIndex]);
                if(folder.Exists == false)
                {
                    AssetDatabase.CreateFolder(currentPathToCreate, folders[folderIndex]);
                    AssetDatabase.Refresh();
                }
            }

            ScriptableObject asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetRelativePath);        
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
		}
	}
} // namespace Aube