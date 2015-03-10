using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	//! @class PrefabInstancierInspector
	//!
	//! @brief Custom Inspector for class PrefabInstancier
	[CustomEditor(typeof(PrefabInstancier))]
	public class PrefabInstancierInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			SerializedProperty elementsProperty = serializedObject.FindProperty("m_elements");
			EditorCollection.Show(elementsProperty,
			                      EditorCollection.Option.CollectionFoldout | EditorCollection.Option.CollectionSize | EditorCollection.Option.CollectionLabel | EditorCollection.Option.BoxElement);
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}