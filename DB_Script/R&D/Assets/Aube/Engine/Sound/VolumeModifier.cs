using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class VolumeModifier
	//!
	//! @brief Component that writes in the AudioSourceManager the value of a volume for a category.
	public class VolumeModifier : MonoBehaviour
	{
		[System.Serializable]
		class InitRequest
		{
			[SerializeField]
			public Object target;
			[SerializeField]
			public string propertyName;
		}

		[SerializeField]
		private AudioSourceCategory m_category = AudioSourceCategory.General;

		[SerializeField]
		private InitRequest[] m_initRequests;

		public static System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.Instance
																	| System.Reflection.BindingFlags.Public
																	| System.Reflection.BindingFlags.FlattenHierarchy;

		public void OnVolumeChange(float a_volume)
		{
			AudioSourceManager.SetVolume(m_category, a_volume);
		}

#region Unity Callbacks
		private void Start()
		{
			if(m_initRequests != null)
			{
				foreach(InitRequest request in m_initRequests)
				{
					if(request.target != null)
					{
						System.Reflection.PropertyInfo property = request.target.GetType().GetProperty(request.propertyName,
						                                                                            	bindingFlags,
						                                                                            	null,
						                                                                            	typeof(float),
						                                                                            	new System.Type[0],
						                                                                            	null);
						if(property != null)
						{
							property.SetValue(request.target, AudioSourceManager.GetVolume(m_category), new object[0]);
						}
					}
				}
			}
		}
#endregion
	}
}
