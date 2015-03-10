using UnityEngine;
using System.Collections;

namespace Aube
{
    [AddComponentMenu("Scripts/Pool")]

    //! @class  Pool
    //!
    //! @brief	Behaviour that manage a collection of an object's copies
	//!			Pooled objects may receive OnPoolAcquire and OnPoolRelease with the pool if needed.
    public class Pool : MonoBehaviour
    {
		[SerializeField]
        private GameObject m_source;
        
        [SerializeField]
        private GameObject[] m_copies;

		[SerializeField][MaskField]
		private Option m_options;

		[System.Flags]
		enum Option
		{
			CallOnPoolAcquire		= 1 << 0,
			CallOnPoolRelease		= 1 << 1,

			//! warnings
			WarnOnLengthGrow		= 1 << 20,
			WarnOnLengthNotEnough	= 1 << 21,

		}
        
        public GameObject AcquireInstance(bool a_createIfNeeded)
        {
			if(m_source == null)
            {
                Aube.Log.Error("The pool '" + name + "' has no source set.");
                return null;
            }
            
			int firstAvailableInstance = 0;
			while(firstAvailableInstance < m_copyUsed.Length  &&  m_copyUsed[firstAvailableInstance])
			{
				++firstAvailableInstance;
			}
			
			if(firstAvailableInstance == m_copyUsed.Length)
			{
				if(a_createIfNeeded)
				{
					if((m_options & Option.WarnOnLengthGrow) != 0)
					{
						Aube.Log.WarningPerf("The pool '" + name + "' is growing to size '" + (m_copies.Length + 1) + "'.");
					}

					System.Array.Resize(ref m_copies, m_copies.Length + 1);
					System.Array.Resize(ref m_copyUsed, m_copyUsed.Length + 1);
					
					m_copies[firstAvailableInstance] = (m_source == null)? null : (GameObject.Instantiate(m_source) as GameObject);
					m_copies[firstAvailableInstance].hideFlags = HideFlags.NotEditable;
					m_copies[firstAvailableInstance].SetActive(false);
				}
				else if((m_options & Option.WarnOnLengthNotEnough) != 0)
				{
					Aube.Log.WarningPerf("The pool '" + name + "' has not a sufficient length.");
				}
			}

			if(firstAvailableInstance < m_copyUsed.Length)
			{
				m_copies[firstAvailableInstance].SetActive(true);
				m_copyUsed[firstAvailableInstance] = true;

				if((m_options & Option.CallOnPoolAcquire) != 0)
				{
					m_copies[firstAvailableInstance].BroadcastMessage("OnPoolAcquire", this, SendMessageOptions.DontRequireReceiver);
				}

				return m_copies[firstAvailableInstance];
			}

			return null;
		}
		
		public void ReleaseInstance(GameObject a_instance)
		{
            if(m_source == null)
            {
                Aube.Log.Error("The pool '" + name + "' has no source set.");
                return;
            }

			if(a_instance == null)
			{
				Aube.Log.Error("Pool : can not release a null instance.");
				return;
			}

			int index = System.Array.IndexOf(m_copies, a_instance);
			if(index < 0  ||  index > m_copies.Length)
			{
				Aube.Log.Error("The instance '" + a_instance.name + "' is not a part of the pool '" + name + "'.");
				return;
			}
            
			if((m_options & Option.CallOnPoolRelease) != 0)
			{
				m_copies[index].BroadcastMessage("OnPoolRelease", this, SendMessageOptions.DontRequireReceiver);
			}

			m_copies[index].SetActive(false);
			m_copyUsed[index] = false;
        }

#region Unity Callbacks
		private void Awake()
		{
			m_copyUsed = new bool[m_copies.Length];
			m_copyUsed.Populate(false);

			foreach(Option option in System.Enum.GetValues(typeof(Option)))
			{
				Debug.Log(option + " : " + (int)option);
			}
		}
#endregion
                
#region Private
    #region Methods
        private int FindFirstAvailableInstance()
        {
            int firstAvailableInstance = 0;
			while(firstAvailableInstance < m_copyUsed.Length  &&  m_copyUsed[firstAvailableInstance])
            {
                ++firstAvailableInstance;
            }
            
			if(firstAvailableInstance == m_copyUsed.Length)
            {
                Aube.Log.WarningPerf("The pool '" + name + "' length is not sufficient.");
                System.Array.Resize(ref m_copies, m_copies.Length + 1);
				System.Array.Resize(ref m_copyUsed, m_copyUsed.Length + 1);
                
				m_copies[firstAvailableInstance] = (m_source == null)? null : (GameObject.Instantiate(m_source) as GameObject);
				m_copyUsed[firstAvailableInstance] = false;
            }
            
            return firstAvailableInstance;
        }
	#endregion

	#region Attributes
		private bool[] m_copyUsed;
	#endregion
#endregion
    }
}