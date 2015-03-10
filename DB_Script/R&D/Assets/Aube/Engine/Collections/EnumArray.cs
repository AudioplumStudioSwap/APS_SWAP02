using UnityEngine;
using System.Collections;

namespace Aube
{
	[System.Serializable]
	//!	@class	EnumArray
	//!
	//!	@brief	Collection that have as many elements as there is in the enumeration given
	//!	
	//!	@tparam	t_Enum	enumeration
	//!	@tparam	t_Class	element of the collection
	public class EnumArray<t_Enum, t_Class> : EnumArrayBase
#if UNITY_EDITOR
		, ISerializationCallbackReceiver 
#endif // UNITY_EDITOR
			where t_Enum : struct, System.IConvertible
	{
		[SerializeField]
		private t_Class[] m_internalArray;

#if UNITY_EDITOR
		[SerializeField]
		private string[] m_names;
#endif // UNITY_EDITOR
		
		public EnumArray()
		{
			m_internalArray = new t_Class[System.Enum.GetValues(typeof(t_Enum)).Length];
#if UNITY_EDITOR
			m_names = new string[m_internalArray.Length];
			for(int valueIndex = 0; valueIndex < m_names.Length; ++valueIndex)
			{
				m_names[valueIndex] = System.Enum.GetName(typeof(t_Enum), valueIndex);
			}
#endif // UNITY_EDITOR
		}

        public t_Class this[int a_index]
        {
            set
            {
                if(a_index < 0  &&  a_index > Length)
                {
                    throw new System.ArgumentOutOfRangeException("a_index", "Invalid existing index " + a_index + " for source length " + Length);
                }
                m_internalArray[a_index] = value;
            }
            get
            {
                if(a_index < 0  &&  a_index > Length)
                {
                    throw new System.ArgumentOutOfRangeException("a_index", "Invalid existing index " + a_index + " for source length " + Length);
                }
                return m_internalArray[a_index];
            }
        }

        public t_Class this[uint a_index]
        {
            set { this[(int)a_index] = value; }
            get { return this[(int)a_index]; }
        }

        public t_Class this[t_Enum a_enum]
        {
            set
            {
                int index = System.Array.IndexOf(System.Enum.GetValues(typeof(t_Enum)), a_enum);
                if(index < 0  &&  index > Length)
                {
                    throw new System.ArgumentOutOfRangeException("a_enum", "Invalid enumeration index " + a_enum.ToString() + " for enumeration " + typeof(t_Enum).Name);
                }
                m_internalArray[index] = value;
            }
            get
            {
                int index = System.Array.IndexOf(System.Enum.GetValues(typeof(t_Enum)), a_enum);
                if(index < 0  &&  index > Length)
                {
                    throw new System.ArgumentOutOfRangeException("a_enum", "Invalid enumeration index " + a_enum.ToString() + " for enumeration " + typeof(t_Enum).Name);
                }
                return m_internalArray[index];
            }
        }

        public int Length
        {
            get{ return m_internalArray.Length; }
        }

#region Serialization
#if UNITY_EDITOR
		public void OnBeforeSerialize()
		{
		}
		
		public void OnAfterDeserialize()
		{
            t_Enum[] enumArray = (t_Enum[])System.Enum.GetValues(typeof(t_Enum));

            t_Class[] serializedCollection = new t_Class[m_internalArray.Length];
			string[] serializedValues = new string[m_names.Length];
			System.Array.Copy(m_internalArray, serializedCollection, m_internalArray.Length);
			System.Array.Copy(m_names, serializedValues, m_names.Length);

			// synchronize size
            System.Array.Resize(ref m_internalArray, enumArray.Length);
            System.Array.Resize(ref m_names, enumArray.Length);

			// synchronize names
			for(int valueIndex = 0; valueIndex < m_names.Length; ++valueIndex)
			{
                m_names[valueIndex] = System.Enum.GetName(typeof(t_Enum), enumArray[valueIndex]);
			}

			// synchronize values
            for(int valueIndex = 0; valueIndex < enumArray.Length; ++valueIndex)
			{
                string name = System.Enum.GetName(typeof(t_Enum), enumArray[valueIndex]);
				int oldIndex = System.Array.IndexOf(serializedValues, name);

				if(oldIndex != valueIndex)
				{
					if(oldIndex < 0  || oldIndex >= serializedValues.Length)
					{
						m_internalArray[valueIndex] = default(t_Class);
					}
					else
					{
						m_internalArray[valueIndex] = serializedCollection[oldIndex];
					}
				}
			}
		}
#endif // UNITY_EDITOR
#endregion
	}
}
