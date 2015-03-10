using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Aube
{
	[DebuggerDisplay("Count = {Count}, Capacity = {Capacity}")]
	[DebuggerTypeProxy(typeof(Deque<>.DebugView))]
	//! @class Deque
	//!
	//! @brief	A double ended queue (deque), which provides 0(1) indexed access, O(1) removals from the front and back, amortized O(1) insertions to the front and back, and O(N) insertions and removals anywhere else (with the operations getting slower as the index approaches the middle).
	//!
	//!	@tparam		t_Class		The type of elements contained in this deque.
	public sealed class Deque<T> : IList<T>, System.Collections.IList
	{
		//! @brief	Create a new instance of the collection using default parameters
		public Deque()
			: this(ms_defaultCapacity)
		{
		}

		//! @brief	Create a new instance of the collection with a capacity.
		//!
		//! @param	a_capacity	the number of elements this deque would contain. The parameter must be greater than 0.
		public Deque(int a_capacity)
		{
			if(a_capacity < 1)
				throw new ArgumentOutOfRangeException("a_capacity", "Capacity must be greater than 0.");
			m_buffer = new T[a_capacity];
		}
		
		//! @brief	Create a new instance of the collection from an other collection.
		//!
		//! @param	a_collection	the source collection
		public Deque(IEnumerable<T> a_collection)
		{
			int count = a_collection.Count();
			if(count > 0)
			{
				m_buffer = new T[count];
				DoInsertRange(0, a_collection, count);
			}
			else
			{
				m_buffer = new T[ms_defaultCapacity];
			}
		}

		//! Gets a value indicating whether this instance is empty.
		public bool IsEmpty
		{
			get{ return Count == 0; }
		}
		
		//! Gets a value indicating whether this instance is at full capacity.
		public bool IsFull
		{
			get{ return Count == Capacity; }
		}

		//!	@brief	Gets the number of elements contained in this deque.
		public int Count{ get; private set; }

		//!	@brief	Gets or sets the capacity for this deque. This value must always be greater than zero, and this property cannot be set to a value less than <the number of elements.
		public int Capacity
		{
			get
			{
				return m_buffer.Length;
			}
			
			set
			{
				if(value < 1)
					throw new ArgumentOutOfRangeException("value", "Capacity must be greater than 0.");
				
				if(value < Count)
					throw new InvalidOperationException("Capacity cannot be set to a value less than Count");
				
				if(value == m_buffer.Length)
					return;
				
				// Create the new buffer and copy our existing range.
				T[] newBuffer = new T[value];
				if(IsSplit)
				{
					// The existing buffer is split, so we have to copy it in parts
					int length = Capacity - m_offset;
					Array.Copy(m_buffer, m_offset, newBuffer, 0, length);
					Array.Copy(m_buffer, 0, newBuffer, length, Count - length);
				}
				else
				{
					// The existing buffer is whole
					Array.Copy(m_buffer, m_offset, newBuffer, 0, Count);
				}
				
				// Set up to use the new buffer.
				m_buffer = newBuffer;
				m_offset = 0;
			}
		}
		
#region GenericListImplementations		
		//! @brief	Gets a value indicating whether this list is read-only. This implementation always returns false.
		bool ICollection<T>.IsReadOnly
		{
			get { return false; }
		}
		
		//! @brief	Gets or sets the item at the specified index.
		//!
		//!	@param	a_index		the index of the item you want to get/set.
		public T this[int a_index]
		{
			get
			{
				CheckExistingIndexArgument(this.Count, a_index);
				return DoGetItem(a_index);
			}
			
			set
			{
				CheckExistingIndexArgument(this.Count, a_index);
				DoSetItem(a_index, value);
			}
		}
		
		//!	@brief	Inserts an item to this list at the specified index.
		//!
		//!	@param	a_index		The zero-based index at which the a_item should be inserted.
		//!	@param	a_item		The object to insert into this list.
		public void Insert(int a_index, T item)
		{
			CheckNewIndexArgument(Count, a_index);
			DoInsert(a_index, item);
		}
		
		//!	@brief	Removes the item at the specified index.
		//!
		//! @param	a_index		The zero-based index of the item to remove.
		public void RemoveAt(int a_index)
		{
			CheckExistingIndexArgument(Count, a_index);
			DoRemoveAt(a_index);
		}
		
		//!	@brief	Determines the index of a specific item in this list.
		//!
		//!	@param	a_item		The object to locate in this list.
		//!
		//!	@return	The index of a_item if found in this list; otherwise, -1.
		public int IndexOf(T item)
		{
			var comparer = EqualityComparer<T>.Default;
			int ret = 0;
			foreach (var sourceItem in this)
			{
				if (comparer.Equals(item, sourceItem))
					return ret;
				++ret;
			}
			
			return -1;
		}
		
		//!	@brief	Adds an item to the end of this list.
		//!
		//!	@param	a_item	The object to add to this list.
		void ICollection<T>.Add(T a_item)
		{
			DoInsert(Count, a_item);
		}
		
		//!	@brief	Determines whether this list contains a specific value.
		//!
		//!	@param	a_item	The object to locate in this list.
		//!
		//!	@return	true if a_item is found in this list; otherwise, false.
		bool ICollection<T>.Contains(T a_item)
		{
			return this.Contains(a_item, null);
		}
		
		//!	@brief	Copies the elements of this list to an array, starting at a particular index.
		//!
		//!	@param	a_array			The one-dimensional array that is the destination of the elements copied from this slice. The array must have zero-based indexing.
		//! @param	a_arrayIndex	The zero-based index in the array at which copying begins.
		void ICollection<T>.CopyTo(T[] a_array, int a_arrayIndex)
		{
			if (a_array == null)
				throw new ArgumentNullException("array", "Array is null");
			
			int count = this.Count;
			CheckRangeArguments(a_array.Length, a_arrayIndex, count);
			for (int i = 0; i != count; ++i)
			{
				a_array[a_arrayIndex + i] = this[i];
			}
		}
		
		//!	@brief	Removes the first occurrence of a specific object from this list.
		//!
		//!	@param	a_item	The object to remove from this list.
		//!
		//!	@return	true if a_item was successfully removed from this list; otherwise, false. This method also returns false if a_item is not found in this list.
		public bool Remove(T a_item)
		{
			int index = IndexOf(a_item);
			if (index == -1)
				return false;
			
			DoRemoveAt(index);
			return true;
		}
		
		//!	@brief	Returns an enumerator that iterates through the collection.
		//!
		//!	@return	A IEnumerator that can be used to iterate through the collection.
		public IEnumerator<T> GetEnumerator()
		{
			int count = this.Count;
			for(int i = 0; i != count; ++i)
			{
				yield return DoGetItem(i);
			}
		}
		
		//!	@brief	Returns an enumerator that iterates through a collection.
		//!
		//!	@return	An IEnumerator object that can be used to iterate through the collection.
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
#endregion

		//!	@brief	Inserts a single element at the back of this deque.
		//!
		//!	@param	a_value	The element to insert.
		public void AddToBack(T a_value)
		{
			EnsureCapacityForOneElement();
			DoAddToBack(a_value);
		}
		
		//!	@brief	Inserts a single element at the front of this deque.
		//!
		//!	@param	a_value	The element to insert.
		public void AddToFront(T a_value)
		{
			EnsureCapacityForOneElement();
			DoAddToFront(a_value);
		}
		
		//!	@brief	Inserts a collection of elements into this deque.
		//!
		//!	@param	a_index			The index at which the collection is inserted.
		//!	@param	a_collection	The collection of elements to insert.
		public void InsertRange(int a_index, IEnumerable<T> a_collection)
		{
			int collectionCount = a_collection.Count();
			CheckNewIndexArgument(Count, a_index);
			
			// Overflow-safe check for "this.Count + collectionCount > this.Capacity"
			if(collectionCount > Capacity - Count)
			{
				this.Capacity = checked(Count + collectionCount);
			}
			
			if(collectionCount == 0)
			{
				return;
			}
			
			this.DoInsertRange(a_index, a_collection, collectionCount);
		}
		
		//!	@brief	Removes a range of elements from this deque.
		//!
		//!	@param	a_offset	The index into the deque at which the range begins.</param>
		//!	@param	a_count		The number of elements to remove.</param>
		public void RemoveRange(int a_offset, int a_count)
		{
			CheckRangeArguments(a_count, a_offset, a_count);
			
			if(a_count == 0)
			{
				return;
			}
			
			this.DoRemoveRange(a_offset, a_count);
		}
		
		//!	@brief	Removes and returns the last element of this deque.
		//!
		//!	@return	The former last element.
		public T RemoveFromBack()
		{
			if (this.IsEmpty)
				throw new InvalidOperationException("The deque is empty.");
			
			return this.DoRemoveFromBack();
		}
		
		//!	@brief	Removes and returns the first element of this deque.
		//!
		//!	@return	The former first element.
		public T RemoveFromFront()
		{
			if (this.IsEmpty)
				throw new InvalidOperationException("The deque is empty.");
			
			return this.DoRemoveFromFront();
		}
		
		//!	@brief	Removes all items from this deque.
		public void Clear()
		{
			this.m_offset = 0;
			this.Count = 0;
		}

#region Private
	#region ObjectListImplementations		
		private bool ObjectIsT(object a_item)
		{
			if (a_item is T)
			{
				return true;
			}
			
			if (a_item == null)
			{
				var type = typeof(T);
				if (type.IsClass && !type.IsPointer)
					return true; // classes, arrays, and delegates
				if (type.IsInterface)
					return true; // interfaces
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
					return true; // nullable value types
			}
			
			return false;
		}
		
		int System.Collections.IList.Add(object a_value)
		{
			if (!ObjectIsT(a_value))
				throw new ArgumentException("Item is not of the correct type.", "value");
			AddToBack((T)a_value);
			return Count - 1;
		}
		
		bool System.Collections.IList.Contains(object a_value)
		{
			if (!ObjectIsT(a_value))
				throw new ArgumentException("Item is not of the correct type.", "value");
			return this.Contains((T)a_value);
		}
		
		int System.Collections.IList.IndexOf(object a_value)
		{
			if (!ObjectIsT(a_value))
				throw new ArgumentException("Item is not of the correct type.", "value");
			return IndexOf((T)a_value);
		}
		
		void System.Collections.IList.Insert(int a_index, object a_value)
		{
			if (!ObjectIsT(a_value))
				throw new ArgumentException("Item is not of the correct type.", "value");
			Insert(a_index, (T)a_value);
		}
		
		bool System.Collections.IList.IsFixedSize
		{
			get{ return false; }
		}
		
		bool System.Collections.IList.IsReadOnly
		{
			get{ return false; }
		}
		
		void System.Collections.IList.Remove(object a_value)
		{
			if (!ObjectIsT(a_value))
				throw new ArgumentException("Item is not of the correct type.", "value");
			Remove((T)a_value);
		}
		
		object System.Collections.IList.this[int a_index]
		{
			get
			{
				return this[a_index];
			}
			
			set
			{
				if (!ObjectIsT(value))
					throw new ArgumentException("Item is not of the correct type.", "value");
				this[a_index] = (T)value;
			}
		}
		
		void System.Collections.ICollection.CopyTo(Array a_array, int a_index)
		{
			if (a_array == null)
				throw new ArgumentNullException("array", "Destination array cannot be null.");
			CheckRangeArguments(a_array.Length, a_index, Count);
			
			for (int i = 0; i != Count; ++i)
			{
				try
				{
					a_array.SetValue(this[i], a_index + i);
				}
				catch (InvalidCastException ex)
				{
					throw new ArgumentException("Destination array is of incorrect type.", ex);
				}
			}
		}
		
		bool System.Collections.ICollection.IsSynchronized
		{
			get{ return false; }
		}
		
		object System.Collections.ICollection.SyncRoot
		{
			get{ return this; }
		}
	#endregion

	#region GenericListHelpers
		private static void CheckNewIndexArgument(int a_sourceLength, int a_index)
		{
			if(a_index < 0 || a_index > a_sourceLength)
			{
				throw new ArgumentOutOfRangeException("index", "Invalid new index " + a_index + " for source length " + a_sourceLength);
			}
		}
		
		private static void CheckExistingIndexArgument(int a_sourceLength, int a_index)
		{
			if(a_index < 0 || a_index >= a_sourceLength)
			{
				throw new ArgumentOutOfRangeException("index", "Invalid existing index " + a_index + " for source length " + a_sourceLength);
			}
		}
		
		private static void CheckRangeArguments(int a_sourceLength, int a_offset, int a_count)
		{
			if(a_offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "Invalid offset " + a_offset);
			}
			
			if(a_count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "Invalid count " + a_count);
			}
			
			if(a_sourceLength - a_offset < a_count)
			{
				throw new ArgumentException("Invalid offset (" + a_offset + ") or count + (" + a_count + ") for source length " + a_sourceLength);
			}
		}		
	#endregion

		//!	@brief	Gets a value indicating whether the buffer is "split" (meaning the beginning of the view is at a later index in buffer than the end).
		private bool IsSplit
		{
			get
			{
				// Overflow-safe version of "(offset + Count) > Capacity"
				return m_offset > (Capacity - Count);
			}
		}
		
		private int DequeIndexToBufferIndex(int a_index)
		{
			return(a_index + m_offset) % Capacity;
		}
		
		private T DoGetItem(int a_index)
		{
			return m_buffer[DequeIndexToBufferIndex(a_index)];
		}
		
		private void DoSetItem(int a_index, T a_item)
		{
			m_buffer[DequeIndexToBufferIndex(a_index)] = a_item;
		}
		
		private void DoInsert(int a_index, T a_item)
		{
			EnsureCapacityForOneElement();
			
			if(a_index == 0)
			{
				DoAddToFront(a_item);
				return;
			}
			else if(a_index == Count)
			{
				DoAddToBack(a_item);
				return;
			}
			
			DoInsertRange(a_index, new[] { a_item }, 1);
		}
		
		private void DoRemoveAt(int a_index)
		{
			if(a_index == 0)
			{
				DoRemoveFromFront();
				return;
			}
			else if(a_index == Count - 1)
			{
				DoRemoveFromBack();
				return;
			}
			
			DoRemoveRange(a_index, 1);
		}
		
		private int PostIncrement(int a_value)
		{
			int ret = m_offset;
			m_offset += a_value;
			m_offset %= Capacity;
			return ret;
		}
		
		private int PreDecrement(int a_value)
		{
			m_offset -= a_value;
			if (m_offset < 0)
				m_offset += Capacity;
			return m_offset;
		}
		
		private void DoAddToBack(T a_value)
		{
			m_buffer[DequeIndexToBufferIndex(Count)] = a_value;
			++Count;
		}
		
		private void DoAddToFront(T a_value)
		{
			m_buffer[PreDecrement(1)] = a_value;
			++Count;
		}
		
		private T DoRemoveFromBack()
		{
			T ret = m_buffer[DequeIndexToBufferIndex(Count - 1)];
			--Count;
			return ret;
		}
		
		private T DoRemoveFromFront()
		{
			--Count;
			return m_buffer[PostIncrement(1)];
		}
		
		private void DoInsertRange(int a_index, IEnumerable<T> a_collection, int a_collectionCount)
		{
			// Make room in the existing list
			if(a_index < Count / 2)
			{
				// Inserting into the first half of the list
				
				// Move lower items down: [0, index) -> [Capacity - collectionCount, Capacity - collectionCount + index)
				// This clears out the low "index" number of items, moving them "collectionCount" places down;
				//   after rotation, there will be a "collectionCount"-sized hole at "index".
				int copyCount = a_index;
				int writeIndex = Capacity - a_collectionCount;
				for (int j = 0; j != copyCount; ++j)
					m_buffer[DequeIndexToBufferIndex(writeIndex + j)] = m_buffer[DequeIndexToBufferIndex(j)];
				
				// Rotate to the new view
				this.PreDecrement(a_collectionCount);
			}
			else
			{
				// Inserting into the second half of the list
				
				// Move higher items up: [index, count) -> [index + collectionCount, collectionCount + count)
				int copyCount = Count - a_index;
				int writeIndex = a_index + a_collectionCount;
				for (int j = copyCount - 1; j != -1; --j)
					m_buffer[DequeIndexToBufferIndex(writeIndex + j)] = m_buffer[DequeIndexToBufferIndex(a_index + j)];
			}
			
			// Copy new items into place
			int i = a_index;
			foreach (T item in a_collection)
			{
				m_buffer[DequeIndexToBufferIndex(i)] = item;
				++i;
			}
			
			// Adjust valid count
			Count += a_collectionCount;
		}
		
		private void DoRemoveRange(int a_index, int a_collectionCount)
		{
			if(a_index == 0)
			{
				// Removing from the beginning: rotate to the new view
				this.PostIncrement(a_collectionCount);
				Count -= a_collectionCount;
				return;
			}
			else if(a_index == Count - a_collectionCount)
			{
				// Removing from the ending: trim the existing view
				Count -= a_collectionCount;
				return;
			}
			
			if((a_index + (a_collectionCount / 2)) < Count / 2)
			{
				// Removing from first half of list
				
				// Move lower items up: [0, index) -> [collectionCount, collectionCount + index)
				int copyCount = a_index;
				int writeIndex = a_collectionCount;
				for (int j = copyCount - 1; j != -1; --j)
					m_buffer[DequeIndexToBufferIndex(writeIndex + j)] = m_buffer[DequeIndexToBufferIndex(j)];
				
				// Rotate to new view
				this.PostIncrement(a_collectionCount);
			}
			else
			{
				// Removing from second half of list
				
				// Move higher items down: [index + collectionCount, count) -> [index, count - collectionCount)
				int copyCount = Count - a_collectionCount - a_index;
				int readIndex = a_index + a_collectionCount;
				for (int j = 0; j != copyCount; ++j)
					m_buffer[DequeIndexToBufferIndex(a_index + j)] = m_buffer[DequeIndexToBufferIndex(readIndex + j)];
			}
			
			// Adjust valid count
			Count -= a_collectionCount;
		}
		
		private void EnsureCapacityForOneElement()
		{
			if (this.IsFull)
			{
				this.Capacity = this.Capacity * 2;
			}
		}

		[DebuggerNonUserCode]
		private sealed class DebugView
		{
			private readonly Deque<T> deque;
			
			public DebugView(Deque<T> deque)
			{
				this.deque = deque;
			}
			
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public T[] Items
			{
				get
				{
					var array = new T[deque.Count];
					((ICollection<T>)deque).CopyTo(array, 0);
					return array;
				}
			}
		}

	#region Attributes
		//! default capacity of the collection
		private const int ms_defaultCapacity = 8;
		
		//! the circular buffer that holds the view
		private T[] m_buffer;
		
		//! The offset into <see cref="buffer"/> where the view begins.
		private int m_offset;
	#endregion
#endregion
	}
}