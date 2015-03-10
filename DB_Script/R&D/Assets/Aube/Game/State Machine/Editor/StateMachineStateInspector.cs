using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	[CustomEditor(typeof(StateMachineState))]

	//!	@class	StateMachineStateInspector
	//!
	//!	@brief	Custom inspector for class StateMachineState
	public class StateMachineStateInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(m_componentHolderProperty, true);

			serializedObject.ApplyModifiedProperties();
		}

#region Unity Callbacks
		private void OnEnable()
		{
			m_componentHolderProperty = serializedObject.FindProperty("m_componentHolder");
		}
#endregion

#region Private
	#region Attributes
		private SerializedProperty m_componentHolderProperty;
	#endregion
#endregion
	}
}