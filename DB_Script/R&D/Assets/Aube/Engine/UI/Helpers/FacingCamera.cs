#if !AUBE_NO_UI

using System;
using UnityEngine;

namespace Aube
{
    public class FacingCamera : MonoBehaviour
    {
        GameObject m_object = null;
        
        private void Awake()  { m_object = gameObject; }
        
        private void Update() { m_object.transform.rotation = Camera.main.transform.rotation; }
    }
}

#endif // !AUBE_NO_UI