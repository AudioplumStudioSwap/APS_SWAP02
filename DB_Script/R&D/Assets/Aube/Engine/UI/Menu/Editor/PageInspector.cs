#if !AUBE_NO_UI

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	namespace UI
	{
		//! @class PageInspector
		//!
		//! @brief Inspector class for Page
		[CustomEditor(typeof(Page))]
		public class PageInspector : Editor
		{
			public override void OnInspectorGUI()
			{
                SerializedProperty pagesProperty = serializedObject.FindProperty("m_prefabPages");
                SerializedProperty optionsProperty = serializedObject.FindProperty("m_options");

                System.Array values = System.Enum.GetValues(typeof(Page.Option));
                string[] names = System.Enum.GetNames(typeof(Page.Option));

                for (int i = 0, count = values.Length; i < count; ++i)
                {
                    int flag = (int)values.GetValue(i);

                    if (EditorGUILayout.Toggle(FormatEditorField(names[i]), (optionsProperty.intValue & flag) != 0))
                    {
                        optionsProperty.intValue = optionsProperty.intValue | flag;
                    }
                    else
                    {
                        optionsProperty.intValue = optionsProperty.intValue & ~flag;
                    }
                    
                }

				EditorCollection.Show(pagesProperty,
				                      EditorCollection.Option.CollectionFoldout | EditorCollection.Option.CollectionSize | EditorCollection.Option.CollectionLabel | EditorCollection.Option.BoxElement);

				serializedObject.ApplyModifiedProperties();
			}

            private string FormatEditorField(string name)
            {
                string result = "";
                string[] split = name.Split('_');

                for (int i = 0; i < split.Length; ++i)
                {
                    if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(split[i]))
                    {
                        result += " ";
                    }

                    string start = (split[i].Length > 0)? split[i].Substring(0, 1) : "";
                    string next = (split[i].Length > 1)? split[i].Substring(1, split[i].Length - 1) : "";
                    result += start.ToUpperInvariant() + next.ToLowerInvariant();
                }
                return result;
            }
		}
	}
}

#endif // !AUBE_NO_UI