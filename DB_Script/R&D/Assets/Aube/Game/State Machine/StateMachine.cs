using UnityEngine;
using System.Collections;

namespace Aube
{
	//!	@class	StateMachineController
	//!
	//!	@brief	Controller of a state machine
	public class StateMachine : StateMachineBaseClass
	{
#if UNITY_EDITOR
		[SerializeField]
		private string[] m_booleanNames;
#endif // UNITY_EDITOR
		[SerializeField]
		private int[] m_hashedBooleanNames;
		[SerializeField]
		private bool[] m_booleanValues;

#if UNITY_EDITOR
		[SerializeField]
		private string[] m_integerNames;
#endif // UNITY_EDITOR
		[SerializeField]
		private int[] m_hashedIntegerNames;
		[SerializeField]
		private int[] m_integerValues;

#if UNITY_EDITOR
		[SerializeField]
		private string[] m_floatNames;
#endif // UNITY_EDITOR
		[SerializeField]
		private int[] m_hashedFloatNames;
		[SerializeField]
		private float[] m_floatValues;

#if UNITY_EDITOR
		[SerializeField]
		private string[] m_triggerNames;
#endif // UNITY_EDITOR
		[SerializeField]
		private int[] m_hashedTriggerNames;

		internal int[] booleanParameters
		{
			get{ return m_hashedBooleanNames; }
		}

		internal bool[] booleanValues
		{
			get{ return m_booleanValues; }
		}
		
		internal int[] integerParameters
		{
			get{ return m_hashedIntegerNames; }
		}

		internal int[] integerValues
		{
			get{ return m_integerValues; }
		}
		
		internal int[] floatParameters
		{
			get{ return m_hashedFloatNames; }
		}

		internal float[] floatValues
		{
			get{ return m_floatValues; }
		}
		
		internal int[] triggerParameters
		{
			get{ return m_hashedTriggerNames; }
		}

		public static int StringToHash(string a_source)
		{
			return Animator.StringToHash(a_source);
		}
	}
}