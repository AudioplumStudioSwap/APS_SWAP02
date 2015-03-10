#if !AUBE_NO_UI

using UnityEngine;
using System.Collections;

namespace Aube
{
	public interface NGUIHudDesc
	{
	//*************************************************************************
	// Properties
	//*************************************************************************
		uint Count{ get; }

	//*************************************************************************
	// Methods
	//*************************************************************************
		GameObject GetHUD(uint hudIndex);

        void OnHudHidden(uint hudIndex);

        void OnLoadingScene();
	}
} // namespace Aube

#endif // !AUBE_NO_UI