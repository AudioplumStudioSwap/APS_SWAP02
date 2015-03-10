using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class Shake
	//!
	//! @brief Shake movement applied on game object
	[AddComponentMenu("Scripts/Movement/Random Shake")]
	public class RandomShake : ShakeComponent<ShakeParameters>
	{
#region Protected
	#region Shake Component Methods
		protected override void UpdateShake(ref Vector3 a_translation, ref Vector3 a_eulerRotation)
		{
			for(int degreeOfFreedom = 0; degreeOfFreedom < 6; ++degreeOfFreedom)
			{
				ShakeParameters parameters = GetParameters(degreeOfFreedom);
				if(parameters != null)
				{
					float currentIntensity = GetCurrentIntensity(degreeOfFreedom);
					float min = Mathf.Min(-currentIntensity, currentIntensity);
					float max = Mathf.Max(-currentIntensity, currentIntensity);

					switch(degreeOfFreedom)
					{
						// translations
						case 0: a_translation.x = Random.Range(min, max); break;
						case 1: a_translation.y = Random.Range(min, max); break;
						case 2: a_translation.z = Random.Range(min, max); break;

						// rotations
						case 3: a_eulerRotation.x = Random.Range(min, max); break;
						case 4: a_eulerRotation.y = Random.Range(min, max); break;
						case 5: a_eulerRotation.z = Random.Range(min, max); break;

						default: Assertion.UnreachableCode(); break;
					}
				}
			}
		}
	#endregion
#endregion
	}
}