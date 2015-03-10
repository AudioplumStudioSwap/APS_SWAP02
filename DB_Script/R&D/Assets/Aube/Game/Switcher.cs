using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class Switcher
	//!
	//! @brief Component that activates or deactivates sub objects depending on an integer property in another component of the object
	[AddComponentMenu("Scripts/Switcher")]
	public class Switcher : MonoBehaviour
	{
		[System.Serializable]
		public class Domain
		{
			[SerializeField]
			public GameObject target;
			[SerializeField]
			public bool activeInside;
			[SerializeField]
			public float minValue;
			[SerializeField]
			public float maxValue;
		}

		public enum PropertyType
		{
			Integer,
			Float,
		}

		[SerializeField]
		private Domain[] m_domains;

		[SerializeField]
		private Object m_target;

		[SerializeField]
		private string m_propertyName;

		[SerializeField]
		private PropertyType m_propertyType;

		public static System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.Instance
																	| System.Reflection.BindingFlags.Public
																	| System.Reflection.BindingFlags.FlattenHierarchy;


		public void Set(Object a_target, string a_propertyName, PropertyType a_propertyType)
		{
			m_target = a_target;
			m_propertyName = a_propertyName;
			m_propertyType = a_propertyType;

			CacheProperty();
		}

		public void AddDomain(GameObject a_gameObject, bool a_activeInside, float a_minValue, float a_maxValue)
		{
			if(m_domains == null)
			{
				m_domains = new Domain[0];
			}
			System.Array.Resize(ref m_domains, m_domains.Length + 1);

			m_domains[m_domains.Length - 1] = new Domain();
			m_domains[m_domains.Length - 1].target = a_gameObject;
			m_domains[m_domains.Length - 1].activeInside = a_activeInside;
			m_domains[m_domains.Length - 1].minValue = a_minValue;
			m_domains[m_domains.Length - 1].maxValue = a_maxValue;
		}

#region Private
		#region Methods
		private void Awake()
		{
			CacheProperty();
		}

		private void Start()
		{
			Check ();
		}

		private void Update()
		{
			Check();
		}

		private void CacheProperty()
		{
			if(m_target == null)
			{
				m_property = null;
			}
			else
			{
				switch(m_propertyType)
				{
					case PropertyType.Integer: 	m_property = m_target.GetType().GetProperty(m_propertyName, bindingFlags, null, typeof(int), new System.Type[0], null); break;
					case PropertyType.Float:	m_property = m_target.GetType().GetProperty(m_propertyName, bindingFlags, null, typeof(float), new System.Type[0], null); break;
					default: Assertion.UnreachableCode(); break;
				}
			}
		}

		private void Check()
		{
			if(m_property == null)
			{
				enabled = false;
				Aube.Log.Error(name + " : the property has not been set in component Switcher.");
				return;
			}

			object result = m_property.GetValue(m_target, new object[0]);

			float floatResult = result.GetType() == typeof(int)? (float)(int)result : (float)result;
			foreach(Domain domain in m_domains)
			{
				bool isInRange = floatResult >= domain.minValue  &&  floatResult <= domain.maxValue;
				domain.target.SetActive(isInRange  ==  domain.activeInside);
			}
		}
		#endregion

		#region Attributes
		private System.Reflection.PropertyInfo m_property;
		#endregion
#endregion
	}
}