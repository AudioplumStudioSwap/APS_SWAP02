using UnityEngine;
using System.Collections;

namespace Aube
{
	[System.Serializable]
	public class ShakeParameters
	{
		public enum CoordinateSystem
		{
			Local,
			World,
		}

		[SerializeField]
		CoordinateSystem m_coordinateSystem;

		[SerializeField]
		private float m_intensity;
		[SerializeField]
		private float m_duration;

		public CoordinateSystem coordinateSystem
		{
			get{ return m_coordinateSystem; }
		}

		public float intensity
		{
			get{ return m_intensity; }
		}

		public float duration
		{
			get{ return m_duration; }
		}
	}

	//! @class ShakeComponentBase
	//!
	//! @brief abstract class that defines basics for a shake component
	public abstract class ShakeComponentBase : MonoBehaviour
	{
		static Quaternion QuaternionFromMatrix(Matrix4x4 m)
		{
			// Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
			Quaternion q = new Quaternion();
			q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] + m[1,1] + m[2,2] ) ) / 2;
			q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] - m[1,1] - m[2,2] ) ) / 2;
			q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] + m[1,1] - m[2,2] ) ) / 2;
			q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] - m[1,1] + m[2,2] ) ) / 2;
			q.x *= Mathf.Sign( q.x * ( m[2,1] - m[1,2] ) );
			q.y *= Mathf.Sign( q.y * ( m[0,2] - m[2,0] ) );
			q.z *= Mathf.Sign( q.z * ( m[1,0] - m[0,1] ) );
			return q;
		}

		[System.Flags]
		public enum TranslationAxis
		{
			PositionX,
			PositionY,
			PositionZ,
		}
		
		[System.Flags]
		public enum RotationAxis
		{
			Pitch,
			Yaw,
			Roll,
		}

		[SerializeField]
		private bool m_playOnStart;
		
		[SerializeField][MaskField]
		private TranslationAxis m_translationAxis;
		[SerializeField][MaskField]
		public RotationAxis m_rotationAxis;

		public void DoShake()
		{
			// keep original transform
			m_originalPosition = transform.localPosition;
			m_originalRotation = transform.localRotation;

			// init values
			m_currentIntensity = new float[6];
			m_decaySpeed = new float[6];
			m_translationUnits = new Vector3[3];
			m_rotationUnits = new Quaternion[3,2];

			Quaternion worldToLocal = QuaternionFromMatrix(transform.worldToLocalMatrix);
			Quaternion localToWorld = QuaternionFromMatrix(transform.localToWorldMatrix);
			for(int degreeOfFreedom = 0; degreeOfFreedom < 6; ++degreeOfFreedom)
			{
				ShakeParameters parameters = GetBaseParameters(degreeOfFreedom);

				if(parameters == null)
				{
					m_currentIntensity[degreeOfFreedom] = 0.0f;
					m_decaySpeed[degreeOfFreedom] = 0.0f;
				}
				else
				{
					m_currentIntensity[degreeOfFreedom] = parameters.intensity;
					m_decaySpeed[degreeOfFreedom] = (parameters.duration <= 0.0f)? float.MaxValue : (parameters.intensity / parameters.duration);
				}

				switch(degreeOfFreedom)
				{
					// translation units
					case 0: m_translationUnits[0] = (parameters == null ||  parameters.coordinateSystem == ShakeParameters.CoordinateSystem.World)? Vector3.right : transform.right; break;
					case 1: m_translationUnits[1] = (parameters == null ||  parameters.coordinateSystem == ShakeParameters.CoordinateSystem.World)? Vector3.up : transform.up; break;
					case 2: m_translationUnits[2] = (parameters == null ||  parameters.coordinateSystem == ShakeParameters.CoordinateSystem.World)? Vector3.forward : transform.forward; break;

					// rotation units
					case 3:
					{
						m_rotationUnits[0,0] = (parameters == null ||  parameters.coordinateSystem == ShakeParameters.CoordinateSystem.World)? worldToLocal : Quaternion.identity;
						m_rotationUnits[0,1] = (parameters == null ||  parameters.coordinateSystem == ShakeParameters.CoordinateSystem.World)? localToWorld : Quaternion.identity;
					}
					break;
					case 4:
					{
						m_rotationUnits[1,0] = (parameters == null ||  parameters.coordinateSystem == ShakeParameters.CoordinateSystem.World)? worldToLocal : Quaternion.identity;
						m_rotationUnits[1,1] = (parameters == null ||  parameters.coordinateSystem == ShakeParameters.CoordinateSystem.World)? localToWorld : Quaternion.identity;
					}
					break;
					case 5:
					{
						m_rotationUnits[2,0] = (parameters == null ||  parameters.coordinateSystem == ShakeParameters.CoordinateSystem.World)? worldToLocal : Quaternion.identity;
						m_rotationUnits[2,1] = (parameters == null ||  parameters.coordinateSystem == ShakeParameters.CoordinateSystem.World)? localToWorld : Quaternion.identity;
					}
					break;

					default: Assertion.UnreachableCode(); break;
				}
			}	
			bool initializationOk = InitShake();

			enabled = initializationOk;
		}

#region Protected
	#region Abstract Methods
		protected virtual bool InitShake() { return true; }
		protected abstract void UpdateShake(ref Vector3 a_translation, ref Vector3 a_eulerRotation);
		protected virtual void PostUpdateShake() {}
		protected abstract ShakeParameters GetBaseParameters(int a_degreeOfFreedom);
	#endregion

	#region Getters
		protected float GetCurrentIntensity(int a_degreeOfFreedom)
		{
			return m_currentIntensity[a_degreeOfFreedom];
		}
	#endregion
		
	#region Properties
		protected TranslationAxis translationAxis
		{
			get{ return m_translationAxis; }
		}

		protected RotationAxis rotationAxis
		{
			get{ return m_rotationAxis; }
		}
	#endregion
#endregion

#region Private
	#region MonoBehaviour Methods
		void Start()
		{
			if(m_playOnStart)
			{
				DoShake();
			}
			else
			{
				enabled = false;
			}
		}
		
		void Update()
		{
			if(m_currentIntensity == null)
			{
				enabled = false;
				return;
			}
			
			int degreeOfFreedom = 0;
			while(degreeOfFreedom < 6  &&  m_currentIntensity[degreeOfFreedom] <= 0.0f)
			{
				++degreeOfFreedom;
			}
			
			if(degreeOfFreedom < 6)
			{
				// get absolute transformation
				Vector3 translation = Vector3.zero;
				Vector3 eulerRotation = Vector3.zero;
				UpdateShake(ref translation, ref eulerRotation);

				// set it to the proper coordinate system
				Vector3 shakeTranslation = Vector3.zero;
				Quaternion shakeRotation = Quaternion.identity;

				for(degreeOfFreedom = 0; degreeOfFreedom < 6; ++degreeOfFreedom)
				{
					ShakeParameters parameters = GetBaseParameters(degreeOfFreedom);
					if(parameters != null)
					{
						switch(degreeOfFreedom)
						{
							// translation
							case 0: shakeTranslation += translation.x * m_translationUnits[0]; break;
							case 1: shakeTranslation += translation.y * m_translationUnits[1]; break;
							case 2: shakeTranslation += translation.z * m_translationUnits[2]; break;

							// rotation
						case 3: shakeRotation *= m_rotationUnits[0,0] * Quaternion.Euler(eulerRotation.x, 0.0f, 0.0f) * m_rotationUnits[0,1]; break;
						case 4: shakeRotation *= m_rotationUnits[1,0] * Quaternion.Euler(0.0f, eulerRotation.y, 0.0f) * m_rotationUnits[1,1]; break;
						case 5: shakeRotation *= m_rotationUnits[2,0] * Quaternion.Euler(0.0f, 0.0f, eulerRotation.z) * m_rotationUnits[2,1]; break;

							default: Assertion.UnreachableCode(); break;
						}
					}
				}

				// apply transformation
				transform.localPosition = m_originalPosition + shakeTranslation;
				transform.localRotation = m_originalRotation * shakeRotation;

				// prepare next frame
				for(degreeOfFreedom = 0; degreeOfFreedom < 6; ++degreeOfFreedom)
				{
					m_currentIntensity[degreeOfFreedom] -= m_decaySpeed[degreeOfFreedom] * Time.deltaTime;
					m_currentIntensity[degreeOfFreedom] = Mathf.Max(0.0f, m_currentIntensity[degreeOfFreedom]);
				}
				
				PostUpdateShake();
			}
			else
			{
				transform.localPosition = m_originalPosition;
				transform.localRotation = m_originalRotation;
				enabled = false;
			}
		}
	#endregion

	#region Atrributes
		//! parameters
		float[] m_currentIntensity;
		float[] m_decaySpeed;
		Vector3[] m_translationUnits;
		Quaternion[,] m_rotationUnits;
		
		//! original position and rotation of the object
		Vector3 m_originalPosition;
		Quaternion m_originalRotation;
	#endregion
#endregion
	}

	//! @class ShakeComponent
	//!
	//! @brief template abstract class that defines basics for a shake component with specific parameter class
	public abstract class ShakeComponent<t_ParameterClass> : ShakeComponentBase where t_ParameterClass : ShakeParameters
	{
		[SerializeField]
		private t_ParameterClass[] m_parameters = new t_ParameterClass[6];

#region Protected
	#region Getters
		protected t_ParameterClass GetParameters(int a_degreeOfFreedom)
		{
			if((a_degreeOfFreedom < 3  &&  (((int)translationAxis) & (1 << a_degreeOfFreedom)) != 0)
			   ||  (a_degreeOfFreedom >= 3  &&  (((int)rotationAxis) & (1 << (a_degreeOfFreedom - 3))) != 0))
			{
				return m_parameters[a_degreeOfFreedom];
			}

			return null;
		}

		protected override ShakeParameters GetBaseParameters(int a_degreeOfFreedom)
		{
			return GetParameters(a_degreeOfFreedom);
		}
	#endregion
#endregion
	}
}