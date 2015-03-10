#if !AUBE_NO_UI

using UnityEngine;
using System.Collections;

namespace Aube
{
	public interface NGUIMenuDesc
	{
	//*************************************************************************
	// Properties
	//*************************************************************************
		uint Count{ get; }
		
	//*************************************************************************
	// Methods
	//*************************************************************************
        GameObject GetMenu(uint menuIndex);

        IEnumerator Load(uint menuIndex);

        void Unload(uint menuIndex);

        void LoadAll();

        void UnloadAll();

        void OnMenuHidden(uint menuIndex);  
	}
} // namespace Aube

#endif // !AUBE_NO_UI