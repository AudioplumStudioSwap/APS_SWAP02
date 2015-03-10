using UnityEngine;
using System.Collections;

namespace Aube
{
	[System.Serializable]
	public class FrequencyShakeParameters : ShakeParameters
	{
		[SerializeField]
		private float m_period;

		public float period
		{
			get{ return m_period; }
		}
	}

	//! @class Shake
	//!
	//! @brief Shake movement applied on game object
	//! @details The movement is a sinusoid with an intensity, a frequency and a decay speed.
	[AddComponentMenu("Scripts/Movement/Frequency Shake")]
	public class FrequencyShake : ShakeComponent<FrequencyShakeParameters>
	{
#region Protected
	#region Shake Component Methods
		protected override bool InitShake()
		{
			m_shakeTimer = 0.0f;

			bool periodCheck = true;
			int degreeOfFreedom = 0;
			while(periodCheck  &&  degreeOfFreedom < 6)
			{
				FrequencyShakeParameters parameter = GetParameters(degreeOfFreedom);
				periodCheck = parameter == null  ||  parameter.period > 0.0f;

				++degreeOfFreedom;
			}

			return periodCheck;
		}

		protected override void UpdateShake(ref Vector3 a_translation, ref Vector3 a_eulerRotation)
		{
			for(int degreeOfFreedom = 0; degreeOfFreedom < 6; ++degreeOfFreedom)
			{
				FrequencyShakeParameters parameter = GetParameters(degreeOfFreedom);

				if(parameter != null)
				{
					float sinus = Mathf.Sin(m_shakeTimer * Mathf.PI * 2.0f / parameter.period) * GetCurrentIntensity(degreeOfFreedom);

					switch(degreeOfFreedom)
					{
						// translations
						case 0: a_translation.x = sinus; break;
						case 1: a_translation.y = sinus; break;
						case 2: a_translation.z = sinus; break;

						// rotations
						case 3: a_eulerRotation.x = sinus; break;
						case 4: a_eulerRotation.y = sinus; break;
						case 5: a_eulerRotation.z = sinus; break;

						default: Assertion.UnreachableCode(); break;
					}
				}
			}
		}

		protected override void PostUpdateShake()
		{
			m_shakeTimer += Time.deltaTime;
		}
	#endregion
		
	#region Attributes
		float m_shakeTimer;
	#endregion
#endregion
	}
}