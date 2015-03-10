using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Aube
{
	//! @class SortedMultiList
	//!
	//! @brief collection of objects t_Value sorted by t_Key
	//! Keys and Values may have more than 1 occurrence.
	//!
	//! Performances : 
	//! - Inserters are in O(log(n) + m)
	//! - Removers and getters are in O(log(n) + 2m)
	public class SortedMultiList<t_Key, t_Value> : ICollection<KeyValuePair<t_Key, t_Value>>
	{
	//*************************************************************************
	// Public declaration
	//*************************************************************************
		public enum Policy
		{
			First,
			Last,
		}

	//*************************************************************************
	// Constructors
	//*************************************************************************
		//! @brief Construct a sorted multi list
		public SortedMultiList() :
			this(DefaultInsertPolicy, Comparer<t_Key>.Default, 0) {}

		//! @brief Construct a sorted multi list specifying the insertion policy
		//!
		//! @param policy		Insertion Policy to use
		public SortedMultiList(Policy policy) :
			this(policy, Comparer<t_Key>.Default, 0) {}

		//! brief Construct a sorted multi list specifying the comparer to use
		//!
		//! @param comparer		Comparer to use
		public SortedMultiList(IComparer<t_Key> comparer) :
			this(DefaultInsertPolicy, comparer, 0) {}

		//! brief Construct a sorted multi list specifying the insertion policy and the comparer to use
		//!
		//! @param policy		Insertion Policy to use
		//! @param comparer		Comparer to use
		public SortedMultiList(Policy policy, IComparer<t_Key> comparer) :
			this(DefaultInsertPolicy, comparer, 0) {}

		//! @brief Construct a sorted multi list
		//!
		//! @param capacity		The capacity of list
		public SortedMultiList(int capacity) :
			this(DefaultInsertPolicy, Comparer<t_Key>.Default, capacity) {}
		
		//! @brief Construct a sorted multi list specifying the insertion policy
		//!
		//! @param policy		Insertion Policy to use
		//! @param capacity		The capacity of list
		public SortedMultiList(Policy policy, int capacity) :
			this(DefaultInsertPolicy, Comparer<t_Key>.Default, capacity) {}
		
		//! brief Construct a sorted multi list specifying the comparer to use
		//!
		//! @param comparer		Comparer to use
		//! @param capacity		The capacity of list
		public SortedMultiList(IComparer<t_Key> comparer, int capacity) :
			this(DefaultInsertPolicy, comparer, capacity) {}
		
		//! brief Construct a sorted multi list specifying the insertion policy and the comparer to use
		//!
		//! @param policy		Insertion Policy to use
		//! @param comparer		Comparer to use
		//! @param capacity		The capacity of list
		public SortedMultiList(Policy policy, IComparer<t_Key> comparer, int capacity)
		{
			m_internalStructure = new List<KeyValuePair<t_Key, t_Value>>(capacity);
			m_insertPolicy = policy;
			m_comparer = comparer;
		}

	//*************************************************************************
	// Public Methods
	//*************************************************************************
		//! @brief Add an element to the collection
		//!
		//! @param kvp	pair of key and value to add
		public virtual void Add(KeyValuePair<t_Key, t_Value> kvp)
		{
			int index = FindIndex(kvp.Key);
			m_internalStructure.Insert(index, kvp);
		}

		//! @brief Insert an element into the list
		//!
		//! @param key		key
		//! @param value	value
		public void Insert(t_Key key, t_Value value)
		{
			Add(new KeyValuePair<t_Key, t_Value>(key, value));
		}


		//! @brief Remove the first occurrence of element from the collection
		//!
		//! @param kvp element to remove
		//!
		//! @return true if the element has been removed
		public virtual bool Remove(KeyValuePair<t_Key, t_Value> kvp)
		{
			return Remove(kvp.Key, kvp.Value);
		}

		//! @brief Remove the first occurrence of element from the collection
		//!
		//! @param key 		key of the element to remove
		//! @param value 	value of the element to remove
		//!
		//! @return true if the element has been removed
		public bool Remove(t_Key key, t_Value value)
		{
			int index = FindIndex(key, Policy.First);
			
			bool erased = false;
			while(erased == false  &&  index < m_internalStructure.Count  &&  m_comparer.Compare(key, m_internalStructure[index].Key) == 0)
			{
				if(Comparer<t_Value>.Default.Compare(value, m_internalStructure[index].Value) == 0)
				{
					m_internalStructure.RemoveAt(index);
					erased = true;
				}
				else
				{
					++index;
				}
			}
			
			return erased;
		}

		//! @brief Remove the element at the given index
		//!
		//! @param index 	index of the element to remove
		public void RemoveAt(int index)
		{
			m_internalStructure.RemoveAt(index);
		}

		//! @brief Clear the collection
		public void Clear()
		{
			m_internalStructure.Clear();
		}

		//! @brief Check if the collection contains the element
		//!
		//! @param kvp element to check
		public virtual bool Contains(KeyValuePair<t_Key, t_Value> kvp)
		{
			int index = FindIndex(kvp.Key, Policy.First);
			
			bool found = false;
			while(found == false  &&  index < m_internalStructure.Count  &&  m_comparer.Compare(kvp.Key, m_internalStructure[index].Key) == 0)
			{
				if(Comparer<t_Value>.Default.Compare(kvp.Value, m_internalStructure[index].Value) == 0)
				{
					found = true;
				}
				else
				{
					++index;
				}
			}
			
			return found;
		}

		//! @brief Returns a custom generic enumerator for the collection
		public virtual IEnumerator<KeyValuePair<t_Key, t_Value>> GetEnumerator()
		{
			return new SortedMultiListEnumerator<t_Key, t_Value>(this);
		}

		//! @brief Returns a non-generic enumerator for the collection
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new SortedMultiListEnumerator<t_Key, t_Value>(this);
		}

	//*************************************************************************
	// Public Properties
	//*************************************************************************
		//! @brief accessor
		public virtual KeyValuePair<t_Key, t_Value> this[int index]
		{
			get{ return m_internalStructure[index]; }
		}

		//! @brief returns the number of elements in the collection
		public int Count
		{
			get{ return m_internalStructure.Count; }
		}

		//! @brief returns true if the collection can not be modified
		public bool IsReadOnly
		{
			get{ return false; }
		}

	//*************************************************************************
	// Inherited Methods
	//*************************************************************************
		//! @brief TODO
		public virtual void CopyTo(KeyValuePair<t_Key, t_Value>[] array, int arrayIndex)
		{
			Assertion.UnreachableCode();
		}

	//*************************************************************************
	// Private Methods
	//*************************************************************************
		private int FindIndex(t_Key key)
		{
			return FindIndex(key, m_insertPolicy);
		}

		private int FindIndex(t_Key key, Policy policy)
		{
			int minIndex = 0;
			int maxIndex = Count;
			
			while(minIndex < maxIndex)
			{
				int index = (minIndex + maxIndex) / 2;
				
				int compareValue = m_comparer.Compare(key, m_internalStructure[index].Key);
				if(compareValue == 0)
				{
					switch(policy)
					{
						case Policy.First: maxIndex = index; break;
						case Policy.Last: minIndex = index + 1; break;
						default: Assertion.UnreachableCode(); return -1;
					}
				}
				else if(compareValue < 0)
				{
					maxIndex = index;
				}
				else
				{
					minIndex = index + 1;
				}
			}
			
			Assertion.Check(minIndex == maxIndex, "Error during assertion algorithm in SortedMultiList.");
			return minIndex;
		}

	//*************************************************************************
	// Private Attributes
	//*************************************************************************
		//! internal structure
		private List<KeyValuePair<t_Key, t_Value>> m_internalStructure;

		//! insertion policy
		private static Policy DefaultInsertPolicy = Policy.Last;
		private readonly Policy m_insertPolicy;

		//! comparer
		private readonly IComparer<t_Key> m_comparer;
	}

	//! @class SortedMultiListEnumerator
	//!
	//! @brief enumerator for SortedMultiList collection
	public class SortedMultiListEnumerator<t_Key, t_Value> : IEnumerator<KeyValuePair<t_Key, t_Value>>
	{
	//*************************************************************************
	// Constructors
	//*************************************************************************
		public SortedMultiListEnumerator()
		{
		}

		public SortedMultiListEnumerator(SortedMultiList<t_Key, t_Value> collection)
		{
			m_collection = collection;
			m_index = -1;
			m_current = default(KeyValuePair<t_Key, t_Value>);
		}

	//*************************************************************************
	// Public Properties
	//*************************************************************************
		public virtual KeyValuePair<t_Key, t_Value> Current
		{
			get{ return m_current; }
		}

		object IEnumerator.Current
		{
			get{ return m_current; }
		}

	//*************************************************************************
	// Inherited Public Methods
	//*************************************************************************
		public virtual void Dispose()
		{
			m_collection = null;
			m_index = -1;
			m_current = default(KeyValuePair<t_Key, t_Value>);
		}

		public virtual bool MoveNext()
		{
			if(++m_index >= m_collection.Count)
			{
				return false;
			}
			else
			{
				m_current = m_collection[m_index];
				return true;
			}
		}

		public virtual void Reset()
		{
			m_index = -1;
			m_current = default(KeyValuePair<t_Key, t_Value>);
		}

	//*************************************************************************
	// Private Attributes
	//*************************************************************************
		private SortedMultiList<t_Key, t_Value> m_collection;
		private int m_index;
		private KeyValuePair<t_Key, t_Value> m_current;
	}
} // namespace Aube