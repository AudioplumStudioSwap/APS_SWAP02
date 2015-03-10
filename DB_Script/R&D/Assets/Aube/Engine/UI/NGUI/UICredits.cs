#if !AUBE_NO_UI

using UnityEngine;
using System.Collections.Generic;

namespace Aube
{
	//! @class UICredits
	//!
	//! @brief display the credits
	public class UICredits : MonoBehaviour
	{
		[SerializeField]
		private TextAsset m_creditsFile;

		[SerializeField]
		private GameObject m_beginPlaceholder;
		[SerializeField]
		private GameObject m_endPlaceHolder;
		
		[SerializeField]
		private bool m_enableSpeedChange;
		[SerializeField]
		private float m_speed;
		[SerializeField]
		private Vector2 m_speedBounds;
		[SerializeField]
		private KeyCode m_increaseSpeedKey;
		[SerializeField]
		private KeyCode m_decreaseSpeedKey;
		[SerializeField]
		private float m_acceleration;

		[SerializeField]
		private IdentifierParameters m_default;
		[SerializeField]
		private string[] m_identifiers;
		[SerializeField]
		private IdentifierParameters[] m_idParameters;

		[SerializeField]
		private KeyCode m_endCreditsKey;
		[SerializeField]
		private ScriptingEvent[] m_onEndCreditsEvents;

#region Private
	#region Methods
		void Awake()
		{
			if(m_creditsFile != null)
			{
				m_updatableElements = new List<KeyValuePair<int, GameObject>>();
				m_spaceAfterLastElement = 0.0f;
				m_nextElement = null;
				m_nextElementParameters = null;

				m_default.InitializePool(transform);
				foreach(IdentifierParameters parameter in m_idParameters)
				{
					parameter.InitializePool(transform);
				}

				if(m_enableSpeedChange)
				{
					m_speedBounds.y = Mathf.Max(m_speedBounds.x, m_speedBounds.y);
					m_speed = Mathf.Clamp(m_speed, m_speedBounds.x, m_speedBounds.y);
					m_acceleration = Mathf.Abs(m_acceleration);
				}
			}
			else
			{
				Log.Error("There is no credits file set in " + name);
				enabled = false;
			}
		}

		void OnEnable()
		{
			m_reader = new System.IO.StringReader(m_creditsFile.text);
			PrepareNextLabel();
		}

		void OnDisable()
		{
			// remove elements at the end
			while(m_updatableElements.Count > 0)
			{
				int identifierId = m_updatableElements[0].Key;
				GameObject elementToRemove = m_updatableElements[0].Value;
				m_updatableElements.RemoveAt(0);

				IdentifierParameters parameters = (identifierId >= 0)? m_idParameters[identifierId] : m_default;
				parameters.ReleaseInstance(elementToRemove);
			}

			m_nextElement = null;
			m_nextElementParameters = null;

			m_reader.Close();
		}

		void Update()
		{
			if(Input.GetKeyDown(m_endCreditsKey))
			{
				NotifyOnEndCredits();
				return;
			}

			if(m_enableSpeedChange)
			{
				if(Input.GetKey(m_increaseSpeedKey))
				{
					m_speed += m_acceleration * Time.deltaTime;
				}
				if(Input.GetKey(m_decreaseSpeedKey))
				{
					m_speed -= m_acceleration * Time.deltaTime;
				}
				m_speed = Mathf.Clamp(m_speed, m_speedBounds.x, m_speedBounds.y);
			}

			foreach(KeyValuePair<int, GameObject> element in m_updatableElements)
			{
				element.Value.transform.position = Vector3.MoveTowards(element.Value.transform.position, m_endPlaceHolder.transform.position, m_speed * Time.deltaTime);
			}

			// remove elements at the end
			while(m_updatableElements.Count > 0 &&  m_updatableElements[0].Value.transform.position == m_endPlaceHolder.transform.position)
			{
				int identifierId = m_updatableElements[0].Key;
				GameObject elementToRemove = m_updatableElements[0].Value;
				m_updatableElements.RemoveAt(0);
				
				IdentifierParameters parameters = (identifierId >= 0)? m_idParameters[identifierId] : m_default;
				parameters.ReleaseInstance(elementToRemove);
			}

			bool startUpdateNextLabel = m_updatableElements.Count == 0;
			if(m_updatableElements.Count > 0  &&  m_nextElement != null)
			{
				Vector3 lastLabelPosition = m_updatableElements[m_updatableElements.Count - 1].Value.transform.position;
				float distanceFromBeginning = Vector3.Magnitude(lastLabelPosition - m_beginPlaceholder.transform.position);

				if(distanceFromBeginning >= m_spaceAfterLastElement + m_nextElementParameters.spaceBefore)
				{
					startUpdateNextLabel = true;
				}
			}

			if(startUpdateNextLabel  &&  m_nextElement != null)
			{
				m_updatableElements.Add(new KeyValuePair<int, GameObject>(System.Array.IndexOf(m_idParameters, m_nextElementParameters), m_nextElement));
				m_spaceAfterLastElement = m_nextElementParameters.spaceAfter;
				m_nextElement = null;
				m_nextElementParameters = null;

				PrepareNextLabel();
			}
			else if(m_updatableElements.Count == 0  &&  m_nextElement == null)
			{
				NotifyOnEndCredits();
			}
		}

		void PrepareNextLabel()
		{
			Assertion.Check(m_nextElement == null, "Invalid call to PrepareNextLine");
			Assertion.Check(m_nextElementParameters == null, "Invalid call to PrepareNextLine");

			string nextLine = m_reader.ReadLine();
			if(nextLine == null)
			{
				return;
			}
			else
			{
				int idIndex = 0;
				bool found = false;
				while(found == false   &&  idIndex < m_identifiers.Length)
				{
					if(nextLine.StartsWith(m_identifiers[idIndex]))
					{
						found = true;
					}
					else
					{
						++idIndex;
					}
				}

				IdentifierParameters parameters = (found)? m_idParameters[idIndex] : m_default;
				string identifier = (found)? m_identifiers[idIndex] : "";

				string text = nextLine.Substring(identifier.Length);

				GameObject element = parameters.AcquireInstance();
				element.transform.parent = m_beginPlaceholder.transform.parent;
				element.transform.localPosition = m_beginPlaceholder.transform.localPosition;
				element.transform.localRotation = m_beginPlaceholder.transform.localRotation;
				element.transform.localScale = m_beginPlaceholder.transform.localScale;

				UILabel label = element.GetComponent<UILabel>();
				if(label)
				{
					label.text = text;
				}

				m_nextElement = element;
				m_nextElementParameters = parameters;
			}
		}

		void NotifyOnEndCredits()
		{
			foreach(ScriptingEvent scriptEvent in m_onEndCreditsEvents)
			{
				scriptEvent.Invoke();
			}
		}
	#endregion

	#region Attributes
		//! asset reader
		System.IO.StringReader m_reader;

		//! list of label to update
		List<KeyValuePair<int, GameObject>> m_updatableElements;

		//! distance before next elements
		float m_spaceAfterLastElement;

		//! next label to update
		GameObject m_nextElement;
		IdentifierParameters m_nextElementParameters;
	#endregion

	#region Identifier Parameters
		[System.Serializable]
		private class IdentifierParameters
		{
			[SerializeField]
			private GameObject m_prefab;
			[SerializeField]
			private float m_spaceBefore;
			[SerializeField]
			private float m_spaceAfter;

			[SerializeField]
			private int m_poolLength;

			public float spaceBefore
			{
				get{ return m_spaceBefore; }
			}

			public float spaceAfter
			{
				get{ return m_spaceAfter; }
			}

			public void InitializePool(Transform a_parent)
			{
				m_instances = new GameObject[Mathf.Max(0, m_poolLength)];
				for(int instanceIndex = 0; instanceIndex < m_instances.Length; ++instanceIndex)
				{
					m_instances[instanceIndex] = GameObject.Instantiate(m_prefab) as GameObject;
					m_instances[instanceIndex].transform.parent = a_parent;
#if UNITY_SAMSUNGTV
					// if we activate the object when we want to use it, it lags a lot
					m_instances[instanceIndex].transform.position = new Vector2(5000.0f, 5000.0f);
#else
					m_instances[instanceIndex].SetActive(false);
#endif // UNITY_SAMSUNGTV
				}

				m_instanceUsed = new bool[m_instances.Length];
				m_instanceUsed.Populate(false);
			}

			public GameObject AcquireInstance()
			{
				GameObject instance = null;

				int instanceIndex = 0;
				while(instanceIndex < m_instances.Length  &&  instance == null)
				{
					if(m_instanceUsed[instanceIndex] == false)
					{
						instance = m_instances[instanceIndex];
						m_instanceUsed[instanceIndex] = true;
					}
					else
					{
						++instanceIndex;
					}
				}

				if(instance == null)
				{
					if(m_instances.Length > 0)
					{
						Log.WarningPerf("Credits : a pool of " + m_instances.Length + " is not enough.");
					}
					instance = GameObject.Instantiate(m_prefab) as GameObject;
				}

#if !UNITY_SAMSUNGTV
				instance.SetActive(true);
#endif // !UNITY_SAMSUNGTV

				return instance;
			}

			public void ReleaseInstance(GameObject a_instance)
			{
				int instanceIndex = System.Array.IndexOf(m_instances, a_instance);
				if(instanceIndex >= 0  &&  instanceIndex < m_instances.Length)
				{
					m_instanceUsed[instanceIndex] = false;
#if UNITY_SAMSUNGTV
					m_instances[instanceIndex].transform.position = new Vector2(5000.0f, 5000.0f);
#else
					m_instances[instanceIndex].SetActive(false);
#endif // UNITY_SAMSUNGTV
				}
				else
				{
					GameObject.Destroy(a_instance);
				}
			}

			#region Private
			private GameObject[] m_instances;
			private bool[] m_instanceUsed;
			#endregion
		}
	#endregion
#endregion
	}
}

#endif // !AUBE_NO_UI