using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class AutoDestruction
	//!
	//! @brief Destroys the game object in which the behaviour is when the countdown reaches 0.
	//!			If the component is disabled, the countdown is paused.
	[AddComponentMenu("Scripts/Auto-Destruction")]
	public class AutoDestruction : MonoBehaviour
	{
		[SerializeField]
		private float m_duration;

#region Private
	#region Unity methods
		void Start()
		{
			m_timeLeft = m_duration;
		}

		void Update()
		{
			m_timeLeft -= Time.deltaTime;

			if(m_timeLeft <= 0.0f)
			{
				GameObject.Destroy(gameObject);
			}
		}
	#endregion

	#region Attributes
		//! countdown
		float m_timeLeft;
	#endregion
#endregion
	}
}
