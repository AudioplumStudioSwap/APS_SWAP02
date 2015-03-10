using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class AnimatorParamModifier
	//!
	//! @brief Change the value of parameter in the animator
	public class AnimatorParamModifier : MonoBehaviour
	{
		public enum ParameterType
		{
			Int,
			Float,
			Bool,
			Trigger,
		}

		[SerializeField]
		private Animator m_animator;
		[SerializeField]
		private string m_animatorParameterName;

		[SerializeField]
		private ParameterType m_animatorParameterType;

		[SerializeField]
		private int m_animatorParameterDoInt;
		[SerializeField]
		private float m_animatorParameterDoFloat;
		[SerializeField]
		private bool m_animatorParameterDoBool;

		[SerializeField]
		private int m_animatorParameterUndoInt;
		[SerializeField]
		private float m_animatorParameterUndoFloat;
		[SerializeField]
		private bool m_animatorParameterUndoBool;

		public void Do()
		{
			if(m_animator == null  ||  string.IsNullOrEmpty(m_animatorParameterName))
			{
				return;
			}

			switch(m_animatorParameterType)
			{
				case ParameterType.Int: m_animator.SetInteger(m_animatorParameterName, m_animatorParameterDoInt); break;
				case ParameterType.Float: m_animator.SetFloat(m_animatorParameterName, m_animatorParameterDoFloat); break;
				case ParameterType.Bool: m_animator.SetBool(m_animatorParameterName, m_animatorParameterDoBool); break;
				case ParameterType.Trigger: m_animator.SetTrigger(m_animatorParameterName); break;
			}
		}

		public void Undo()
		{
			if(m_animator == null  ||  string.IsNullOrEmpty(m_animatorParameterName))
			{
				return;
			}
			
			switch(m_animatorParameterType)
			{
				case ParameterType.Int: m_animator.SetInteger(m_animatorParameterName, m_animatorParameterUndoInt); break;
				case ParameterType.Float: m_animator.SetFloat(m_animatorParameterName, m_animatorParameterUndoFloat); break;
				case ParameterType.Bool: m_animator.SetBool(m_animatorParameterName, m_animatorParameterUndoBool); break;
				case ParameterType.Trigger: Log.Error("A trigger has no Undo."); break;
			}
		}
	}
}