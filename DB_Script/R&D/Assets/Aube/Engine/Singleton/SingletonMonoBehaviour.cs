#region Imports
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
#endregion

namespace Aube
{
	//!	@class	SingletonMonoBehaviour
	//!
	//!	@brief	singleton pattern class implementation for a mono behaviour.
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
		public static T Instance
		{
			get
			{
				if(null == ms_instance)
				{
					ms_instance = GameObject.FindObjectOfType<T>();
					if(ms_instance == null)
					{
						//Create object
						GameObject obj = new GameObject(typeof(T).Name, typeof(T));
						obj.name = typeof(T).Name + " (Singleton)";
						Assertion.Check(ms_instance != null, "Awake method should have been called at this point.");
					}
				}
				
				return (T)ms_instance;
			}
		}

#region Unity callbacks
		protected virtual void Awake()
		{
			if(ms_instance == null)
			{
				ms_instance = this as T;
				DontDestroyOnLoad(this);
			}
			else
			{
				if(this != ms_instance)
				{
					GameObject.Destroy(this.gameObject);
				}
			}
		}

		protected virtual void OnDestroy()
		{
			if(ms_instance == this)
			{
				ms_instance = null;
			}
		}
#endregion

#region Private
	#region Attributes
		//! instance reference
		private static T ms_instance;
	#endregion
#endregion
    }
} // namespace Aube
