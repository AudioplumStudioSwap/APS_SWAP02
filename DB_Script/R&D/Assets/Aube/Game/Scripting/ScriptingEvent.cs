using UnityEngine;
using System.Collections.Generic;

namespace Aube
{
	//! @class ScriptingEvent
	//!
	//! @brief Event of scripting that could be saved
	[System.Serializable]
	public class ScriptingEvent
	{
		//! targeted object
		[SerializeField]
		GameObject m_targetObject;
		//! target behaviour in object
		[SerializeField]
		Component m_targetComponent;
		//! target method name
		[SerializeField]
		string m_targetMethodName;
		//! parameters
		[SerializeField]
		List<Parameter> m_parameters;

		public static System.Reflection.BindingFlags BindingFlags = System.Reflection.BindingFlags.Instance
																	| System.Reflection.BindingFlags.Public
																	| System.Reflection.BindingFlags.FlattenHierarchy;

		//! @brief invoke the call to notifier
		public void Invoke()
		{
			if(m_targetObject == null)
			{
				Log.Error("No target object for scripting event.");
				return;
			}

			if(string.IsNullOrEmpty(m_targetMethodName))
			{
				Log.Error("No method for scripting event.");
				return;
			}

			Object instance = (m_targetComponent == null)? (Object)m_targetObject : (Object)m_targetComponent;
			System.Reflection.MethodInfo method = instance.GetType().GetMethod(m_targetMethodName,
			                                                                  BindingFlags,
			                                                                  null,
			                                                                  new System.Type[0],
			                                                                  null);
			if(method == null)
			{
				Log.Error("No method found '" + m_targetMethodName + "' for scripting event in object " + instance.name + ".");
				return;
			}

			method.Invoke(instance, new object[0]);
		}

		[System.Serializable]
		class Parameter
		{
			enum SetKind
			{
				Static,
				Dynamic,
			}

			//[SerializeField]
			//SetKind m_setKind = SetKind.Static;

			//! static parameters
			[SerializeField]
			bool m_boolValue;
			[SerializeField]
			int m_intValue;
			[SerializeField]
			float m_floatValue;
			[SerializeField]
			Object m_objectValue;

			//! dynamic parameters
			[SerializeField]
			Object m_objectRef;
			[SerializeField]
			string m_propertyName;
		}
	}
}