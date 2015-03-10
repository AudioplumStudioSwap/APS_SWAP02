using UnityEngine;
using System.Collections.Generic;

namespace Aube
{
	//! @class PrefabInstancier
	//!
	//! @brief behaviour that instantiate child objects from prefab
	[AddComponentMenu("Scripts/Prefab Instancier")]
	public class PrefabInstancier : MonoBehaviour
	{
		[System.Serializable]
		class Element
		{
			[SerializeField]
			GameObject m_prefab;

			[SerializeField]
			int m_count;

			public GameObject Prefab
			{
				get{ return m_prefab; }
			}

			public int Count
			{
				get{ return m_count; }
			}
		}

		[SerializeField]
		Element[] m_elements;

#region Private
	#region Methods
		void OnEnable()
		{
			m_instances = new List<GameObject>[m_elements.Length];

			for(int elementIndex = 0; elementIndex < m_elements.Length; ++elementIndex)
			{
				if(m_elements[elementIndex].Prefab != null)
				{
					for(int index = 0; index < m_elements[elementIndex].Count; ++index)
					{
						if(m_instances[elementIndex] == null)
						{
							m_instances[elementIndex] = new List<GameObject>();
						}

						GameObject newInstance = GameObject.Instantiate(m_elements[elementIndex].Prefab) as GameObject;

						// keep prefab local transform
						Vector3 localPosition = newInstance.transform.localPosition;
						Quaternion localRotation = newInstance.transform.localRotation;
						Vector3 localScale = newInstance.transform.localScale;

						// set parent transform
						newInstance.transform.parent = transform;

						// set prefab local transform
						newInstance.transform.localPosition = localPosition;
						newInstance.transform.localRotation = localRotation;
						newInstance.transform.localScale = localScale;

						m_instances[elementIndex].Add(newInstance);
					}
				}
			}
		}

		void OnDisable()
		{
			foreach(List<GameObject> list in m_instances)
			{
				if(list != null)
				{
					foreach(GameObject instance in list)
					{
						if(instance != null)
						{
							GameObject.Destroy(instance);
						}
					}
				}
			}
		}
	#endregion

	#region Attributes
		List<GameObject>[] m_instances;
	#endregion
#endregion
	}
}