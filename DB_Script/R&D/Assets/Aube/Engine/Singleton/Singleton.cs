using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Aube
{
	//!	@class	ISingleton
	//!
	//!	@brief	base class for a singleton
	//!			@aube_discussion : obsolete?
	public abstract class ISingleton
	{
		static List<ISingleton> ms_SingletonList = new List<ISingleton>();

		protected static T CreateInstance<T>() where T : ISingleton
		{
			T instance = Activator.CreateInstance(typeof(T)) as T;
			ms_SingletonList.Add(instance);
			return instance;
		}

		protected static void DestroyInstance<T>(T pInstance) where T : ISingleton
		{
			ms_SingletonList.Remove(pInstance);
		}

		public static void ProcessMessageAll(string a_message)
		{
			int messageId = a_message.GetHashCode();
			for(int i = 0; i < ms_SingletonList.Count; ++i)
			{
				ms_SingletonList[i].ProcessMessage(messageId);
			}
		}

		protected virtual void ProcessMessage(int a_messageId) {}
	}

	//!	@class	Singleton
	//!
	//!	@brief	singleton pattern class implementation
	//!			@aube_discussion : obsolete in unity?
	public abstract class Singleton<T> : ISingleton where T : ISingleton
	{
	    #region Attributs

	    /// <summary>
	    /// The index of the singleton instance
	    /// </summary>
	    private static T _sInstance = null;

	    public static T Instance
	    {
	        get {
				
				if(null==_sInstance)
				{
					_sInstance = CreateInstance<T>();
					(_sInstance as Singleton<T>).Awake();
				}
				
				return (T)_sInstance; 
			}
	    }

        public static bool IsInstanced
        {
            get { return _sInstance != null; }
        }

        #endregion Attributs

	    #region Constructors

	    /// <summary>
	    /// Default constructor
	    /// </summary>
	    protected Singleton()
	    {
	        if (_sInstance != null)
	        {
	            //todo : create specific exception
	            throw new Exception("instance already exist");
	        }

	        _sInstance = this as T;

	        if (_sInstance == null)
	        {
	            throw new Exception("instance creation failed");
	        }
	    }

        public virtual void Awake()     {}
        public virtual void OnDestroy() {}

	    public static void DestroyInstance()
	    {
            if(_sInstance != null) (_sInstance as Singleton<T>).OnDestroy();
			DestroyInstance<T>(_sInstance);
			_sInstance = null;			
			GC.Collect();
	    }	
			
		
	    #endregion Constructors
	}
} // namespace Aube