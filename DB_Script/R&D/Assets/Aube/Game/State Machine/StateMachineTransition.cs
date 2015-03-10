using UnityEngine;
using System.Collections.Generic;

namespace Aube
{
	//! @class StateMachineTransition
	//!
	//! @brief Transition of an editable state machine
	public class StateMachineTransition : ScriptableObject
	{
		[SerializeField][HideInInspector]
		private StateMachineState m_stateFrom;

		[SerializeField][HideInInspector]
		private StateMachineState m_stateTo;

		[SerializeField]
		private ConditionSet[] m_conditionSets;

		public StateMachineState stateTo
		{
			get{ return m_stateTo; }
		}

		[System.Serializable]
		private class Condition
		{
#if UNITY_EDITOR
			[SerializeField][HideInInspector]
			internal string m_parameterName;
#endif // UNITY_EDITOR
			[SerializeField][HideInInspector]
			internal int m_parameterHashedName;

			[SerializeField][HideInInspector]
			internal int m_parameterType;

			[SerializeField][HideInInspector]
			internal int m_comparisonValue;

			[SerializeField][HideInInspector]
			internal bool m_otherValueBool;

			[SerializeField][HideInInspector]
			internal int m_otherValueInt;

			[SerializeField][HideInInspector]
			internal float m_otherValueFloat;

			internal static bool Match(Condition a_condition,
			                           Dictionary<int, bool> a_booleans, Dictionary<int, int> a_integers, Dictionary<int, float> a_floats, Dictionary<int, bool> a_triggers)
			{
				switch(a_condition.m_parameterType)
				{
					case 0:
					{
						bool value;
						bool valueFound = a_booleans.TryGetValue(a_condition.m_parameterHashedName, out value);
						return valueFound  &&  value == a_condition.m_otherValueBool;
					}
					case 1:
					{
						int value;
						bool valueFound = a_integers.TryGetValue(a_condition.m_parameterHashedName, out value);

						switch(a_condition.m_comparisonValue)
						{
							case 0:	return valueFound  &&  value == a_condition.m_otherValueInt;
							case 1:	return valueFound  &&  value < a_condition.m_otherValueInt;
							case 2:	return valueFound  &&  value <= a_condition.m_otherValueInt;
							case 3:	return valueFound  &&  value > a_condition.m_otherValueInt;
							case 4:	return valueFound  &&  value >= a_condition.m_otherValueInt;
						}

						return false;
					}
					case 2:
					{
						float value;
						bool valueFound = a_floats.TryGetValue(a_condition.m_parameterHashedName, out value);
						
						switch(a_condition.m_comparisonValue)
						{
							case 0:	return valueFound  &&  value == a_condition.m_otherValueFloat;
							case 1:	return valueFound  &&  value < a_condition.m_otherValueFloat;
							case 2:	return valueFound  &&  value <= a_condition.m_otherValueFloat;
							case 3:	return valueFound  &&  value > a_condition.m_otherValueFloat;
							case 4:	return valueFound  &&  value >= a_condition.m_otherValueFloat;
						}
						
						return false;
					}
					case 3:
					{
						bool value;
						bool valueFound = a_triggers.TryGetValue(a_condition.m_parameterHashedName, out value);
						return valueFound  &&  value;
					}
					default: return false;
				}
			}
		}

		[System.Serializable]
		private class ConditionSet
		{
			[SerializeField]
			internal Condition[] m_conditions;
		}

		//!	@brief	Check if the transition should be used
		//!
		//!	@param	a_transition	transition to check
		//!	@param	a_booleans		collection of boolean parameters
		//!	@param	a_integers		collection of integer parameters
		//!	@param	a_floats		collection of float parameters
		//!	@param	a_triggers		collection of trigger parameters
		internal static bool Match(StateMachineTransition a_transition,
		                           Dictionary<int, bool> a_booleans, Dictionary<int, int> a_integers, Dictionary<int, float> a_floats, Dictionary<int, bool> a_triggers)
		{
			bool matchSet = false;
			int setIndex = 0;
			while(matchSet == false  &&  setIndex < a_transition.m_conditionSets.Length)
			{
				bool matchAllConditions = a_transition.m_conditionSets[setIndex].m_conditions.Length > 0;
				int conditionIndex = 0;
				while(matchAllConditions  &&  conditionIndex < a_transition.m_conditionSets[setIndex].m_conditions.Length)
				{
					matchAllConditions = Condition.Match(a_transition.m_conditionSets[setIndex].m_conditions[conditionIndex], a_booleans, a_integers, a_floats, a_triggers);
					++conditionIndex;
				}

				matchSet = matchAllConditions;
				++setIndex;
			}

			return matchSet;
		}
	}
}
