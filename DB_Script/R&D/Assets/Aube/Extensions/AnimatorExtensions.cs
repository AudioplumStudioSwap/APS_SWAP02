using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

//! @class AnimatorExtensions
//!
//! @brief Extensions methods for UnityEngine.Animator
public static class AnimatorExtensions
{
#if UNITY_EDITOR
	//! @brief Check if a state exist in an animator
	//!
	//! @this	a_animator	animator
	//! @param	a_stateId	Id of the state
	//! @param	a_layerId	Id of the layer
	public static bool StateExists(this Animator a_animator, int a_stateId, int a_layerId)
	{
		UnityEditorInternal.AnimatorController ctrl = a_animator.runtimeAnimatorController as UnityEditorInternal.AnimatorController;
		UnityEditorInternal.AnimatorControllerLayer layerCtrl = (ctrl != null)? ctrl.GetLayer(a_layerId) : null;
		return StateExists(a_stateId, (layerCtrl != null)? layerCtrl.stateMachine : null);
	}
#endif // UNITY_EDITOR
    
    public static bool IsAnimPlaying(this Animator a_animator, int a_stateId, int a_layerId)
    {
        if (a_animator != null)
        {
            AnimatorStateInfo state = a_animator.GetCurrentAnimatorStateInfo(a_layerId);

            if (state.nameHash == a_stateId)
            {
                if (a_animator.IsInTransition(a_layerId))
                {
                    return true;
                }
                else
                {
                    return (state.loop)? true : state.normalizedTime <= 1.0f;
                }
            }
            else if (a_animator.IsInTransition(a_layerId))
            {
                AnimatorStateInfo nextState = a_animator.GetNextAnimatorStateInfo(a_layerId);
                return nextState.nameHash == a_stateId;
            }
        }

        return false;
    }

#region Private
#if UNITY_EDITOR
	private static bool StateExists(int stateId, UnityEditorInternal.StateMachine stateMachine)
	{
		if (stateMachine != null)
		{
			for (int i = 0; i < stateMachine.stateCount; ++i)
			{
				if (stateMachine.GetState(i).uniqueNameHash == stateId)
				{
					return true;
				}
			}

			for (int i = 0; i < stateMachine.stateMachineCount; ++i)
			{
				if (StateExists(stateId, stateMachine.GetStateMachine(i)))
				{
					return true;
				}
			}
		}
		return false;
	}
#endif // UNITY_EDITOR
#endregion
}