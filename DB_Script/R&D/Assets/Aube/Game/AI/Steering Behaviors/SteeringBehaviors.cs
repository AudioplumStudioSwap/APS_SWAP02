using UnityEngine;

namespace Aube
{
	namespace AI
	{
		//! @class SteeringBehaviors
		//!
		//! @brief static class for main steering behaviors methods
		public static class SteeringBehaviors
		{
#region 2 Dimensions
			//! @brief generate a force that separates an element from other elments
			//!
			//! @param	a_me				position of the element
			//! @param	a_others			others elements to separate from
			//! @param	a_separationRadius	minimal distance to consider we need separation
			//!
			//! @return the result force to separate the element from the others
			public static Vector2 Separation(Vector2 a_me, Vector2[] a_others, float a_separationRadius)
			{
				Vector2 resultForce = Vector2.zero;
				int neighborCount = 0;

				foreach(Vector2 other in a_others)
				{
					if(Vector2.SqrMagnitude(other - a_me) <= a_separationRadius * a_separationRadius)
					{
						resultForce += a_me - other;
						++neighborCount;
					}
				}

				if(neighborCount > 0)
				{
					resultForce /= (float)neighborCount;
					resultForce.Normalize();
				}

				return resultForce;
			}

			//! @brief generate a force that aligns elements towards the same direction
			//!
			//! @param	a_me				position of the element
			//! @param	a_others			others elements to align to
			//! @param	a_othersForward		others elements forward direction
			//! @param	a_alignmentRadius	minimal distance to consider we need alignment
			//!
			//! @return the result force to separate the element from the others
			public static Vector2 Alignment(Vector2 a_me, Vector2[] a_others, Vector2[] a_othersForward, float a_alignmentRadius)
			{
				Assertion.Check(a_others.Length == a_othersForward.Length, "Invalid parameters for SteeringBehaviours.Alignment : the two arrays must have the same length.");
				Vector2 resultForce = Vector2.zero;
				int neighborCount = 0;

				for(int arrayIndex = 0; arrayIndex < a_others.Length; ++arrayIndex)
				{
					if(Vector2.SqrMagnitude(a_others[arrayIndex] - a_me) <= a_alignmentRadius * a_alignmentRadius)
					{
						resultForce += a_othersForward[arrayIndex];
						++neighborCount;
					}
				}
				
				if(neighborCount > 0)
				{
					resultForce /= (float)neighborCount;
					resultForce.Normalize();
				}
				
				return resultForce;
			}

			//! @brief generate a force that push an element to a point, slowing down when arriving at it
			//!
			//! @param	a_me				position of the element
			//! @param	a_target			position of the target
			//! @param	a_slowingRadius		maximal distance to start slowing down
			//!
			//! @return the result force to arrive at the target
			public static Vector2 Arrive(Vector2 a_me, Vector2 a_target, float a_slowingRadius)
			{
				Vector2 toTarget = (a_target - a_me).normalized;
				return toTarget * Mathf.Clamp(toTarget.magnitude / a_slowingRadius, 0.0f, 1.0f);
			}

			public static Vector2 Evade(Vector2 a_me, Vector2 a_target, Vector2 a_targetForward, float a_distance)
			{
				Vector2 resultForce = Vector2.zero;

				Vector2 targetToMe = (a_me - a_target);
				if(targetToMe.magnitude < a_distance)
				{
					Vector2 targetLeft = Quaternion.Euler(0f, 0f, 90f) * a_targetForward;

					if(targetToMe.magnitude == 0.0)
					{
						resultForce = targetLeft;
					}
					else
					{
						Vector2 targetToMeNormalized = targetToMe / targetToMe.magnitude;
						float dot = Vector2.Dot(targetToMeNormalized, a_targetForward);

						if(dot > 0.0f)
						{
							Vector2 meRight = Quaternion.Euler(0f, 0f, 90f) * (-targetToMeNormalized);

							float leftDot = Vector2.Dot(targetToMeNormalized, targetLeft);
							resultForce = (leftDot > 0)? -meRight : meRight;
							resultForce *= a_distance - targetToMe.magnitude;
						}
					}
				}

				return resultForce;
			}
#endregion
		}
	}
}