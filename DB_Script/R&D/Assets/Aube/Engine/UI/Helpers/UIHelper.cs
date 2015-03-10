#if !AUBE_NO_UI

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace Aube
{
    //*****************************************************************************
    //
    //*****************************************************************************
    
    public static class UIHelper
    {
        //*************************************************************************
        //
        //*************************************************************************
        
        public enum UPDATE_OPT { NONE, RESET };
        
        //*************************************************************************
        //
        //*************************************************************************
        
        private static GameObject m_rootObj    = null;
        
        private static GameObject m_camObj     = null;
        
        private static UIRoot     m_uiRoot     = null;
        
        private static UICamera   m_uiCam      = null;
        
        private static Camera     m_unityCam   = null;

        private static Vector2    m_canvasSize = Vector2.zero;
        
        //*************************************************************************
        //
        //*************************************************************************
        
        public static GameObject RootObj  { get { return m_rootObj;  } }
        
        public static GameObject CamObj   { get { return m_camObj;   } }
        
        public static UIRoot     Root     { get { return m_uiRoot;   } }
        
        public static UICamera   Cam      { get { return m_uiCam;    } }
        
        public static Camera     UnityCam { get { return m_unityCam; } }

        //*************************************************************************
        //
        //*************************************************************************
        
        public static void Reset()
        {
            m_uiRoot   = null;
            
            m_uiCam    = null;
            
            m_rootObj  = null;
            
            m_camObj   = null;
            
            m_unityCam = null;
        }

        //*************************************************************************
        //
        //*************************************************************************
        
        public static void UpdateDependencies( UPDATE_OPT mode, UIRoot uiRoot )
        {
            if( mode == UPDATE_OPT.RESET ) Reset();
            
            if( m_uiRoot   == null ) m_uiRoot   = uiRoot   == null ? GameObject.FindObjectOfType< UIRoot >()   : uiRoot;
            
            if( m_uiCam    == null ) m_uiCam    = uiRoot   == null ? GameObject.FindObjectOfType< UICamera >() : uiRoot.gameObject.GetComponentInChildren< UICamera >();
            
            if( m_rootObj  == null ) m_rootObj  = m_uiRoot != null ? m_uiRoot.gameObject : null;
            
            if( m_camObj   == null ) m_camObj   = m_uiCam  != null ? m_uiCam.gameObject  : null;
            
            if( m_unityCam == null ) m_unityCam = m_camObj != null ? m_camObj.GetComponent< Camera >() : null;

            GetCanvasSize( mode );
        }

        //*************************************************************************
        //
        //*************************************************************************

        public static bool AreDependenciesValid()
        {
            return ( m_rootObj != null ) && ( m_camObj != null ) && ( m_unityCam != null );
        }

        //*************************************************************************
        //
        //*************************************************************************
        
        public static Vector2 GetCanvasSize( UPDATE_OPT opt )
        {
            if( opt == UPDATE_OPT.RESET )
            {   
                if( ( m_uiRoot != null ) && ( m_unityCam != null ) )
                {
                    m_canvasSize.y = m_unityCam.isOrthoGraphic ? m_uiRoot.maximumHeight : m_unityCam.farClipPlane * Mathf.Tan( m_unityCam.fieldOfView );
                    
                    m_canvasSize.x = m_canvasSize.y * m_unityCam.aspect;
                }
                else
                {
                    m_canvasSize = Vector2.zero;
                }
            }
            
            return m_canvasSize;
        }

        //*************************************************************************
        //
        //*************************************************************************

        public static Vector3 GetScrPixelCoordinates( Vector3 worldPos, UIRoot uiRoot, Camera srcCam, Camera dstCam )
        {
            if( uiRoot   == null ) return Vector3.zero;

            if( srcCam   == null ) return Vector3.zero;
            
            if( dstCam   == null ) return Vector3.zero;
            
            Vector3 srcViewPos = srcCam.WorldToViewportPoint ( worldPos   );
            
            Vector3 dstScrPos  = dstCam.ViewportToScreenPoint( srcViewPos );
            
            dstScrPos.x -= dstCam.pixelRect.center.x; 
            
            dstScrPos.y -= dstCam.pixelRect.center.y;

            if( dstCam.pixelWidth  != 0.0f ) dstScrPos.x *= ( ( uiRoot.activeHeight * dstCam.aspect ) / dstCam.pixelWidth  );

            if( dstCam.pixelHeight != 0.0f ) dstScrPos.y *= ( ( uiRoot.activeHeight )                 / dstCam.pixelHeight );
            
            return dstScrPos;
        }

        //*************************************************************************
        //
        //*************************************************************************
        
        public static Vector3 GetScrPixelCoordinates( Vector3 worldPos, Camera srcCam )
        {
            return GetScrPixelCoordinates( worldPos, m_uiRoot, srcCam, m_unityCam );
        }

        //*************************************************************************
        //
        //*************************************************************************
        
        public static void AttachToUIRoot( GameObject gameObject )
        {
            if( gameObject == null ) return;
            
            if( m_camObj   == null ) return;

            Vector3 initialScale            = gameObject.transform.localScale;

            gameObject.transform.parent     = m_camObj.transform;
            
            gameObject.transform.localScale = initialScale;
        }
        
        //*************************************************************************
        //
        //*************************************************************************
        
        public static GameObject FindUIAnchor( GameObject gameObject )
        {
            int nbChilds = gameObject.transform.childCount;
            
            for( int child = 0; child < nbChilds; ++child )
            {
                Transform  transform = gameObject.transform.GetChild( child );
                
                GameObject childObj  = transform.gameObject;
                
                if( childObj.GetComponent< UIAnchor >() != null )
                {
                    return childObj;
                }
            }
            
            return null;
        }
        
        //*************************************************************************
        //
        //*************************************************************************
        
        public static void AddMissingUIAnchor( GameObject gameObject )
        {
            GameObject uiAnchorObj = FindUIAnchor( gameObject );
            
            if( uiAnchorObj == null )
            {
                uiAnchorObj = new GameObject( "UIAnchor", typeof( UIAnchor ) );
                
                if( uiAnchorObj != null ) uiAnchorObj.transform.parent = gameObject.transform;
            }
        }
    }
}

#endif // !AUBE_NO_UI