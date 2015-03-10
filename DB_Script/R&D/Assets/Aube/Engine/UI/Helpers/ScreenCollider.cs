#if !AUBE_NO_UI

using System;
using UnityEngine;

namespace Aube
{
    [ RequireComponent( typeof( UIWidget    ) ) ]

    [ RequireComponent( typeof( UIButton    ) ) ]

    [ RequireComponent( typeof( BoxCollider ) ) ]

    public class ScreenCollider : MonoBehaviour
    {
        private GameObject    m_object   = null;

        private UIWidget      m_widget   = null;

        private UIButton      m_button   = null;

        private BoxCollider   m_collider = null;

        public float ZOffset 
        { 
            get { return transform.localPosition.z; } 

            set { Vector3 pos = transform.localPosition; pos.z = value; transform.localPosition = pos; } 
        }

        private void Awake()
        {
            m_object   = gameObject;
            m_widget   = GetComponent< UIWidget >();
            m_button   = GetComponent< UIButton >();
            m_collider = GetComponent< BoxCollider >();

            UpdateCanvasSize();
        }

        private void UpdateCanvasSize()
        {
            Vector2 canvasSize = UIHelper.GetCanvasSize( UIHelper.UPDATE_OPT.NONE );
            
            if( m_widget != null )
            {
                m_widget.SetDimensions( ( int )canvasSize.x, ( int )canvasSize.y );
            }

            if( m_collider != null )
            {
                m_collider.size   = new Vector3( canvasSize.x, canvasSize.y, 1.0f );
                
                m_collider.center = Vector3.zero;
            }
        }

        private void Update()
        {
            UpdateCanvasSize();
        }

        private static int GetWidgetsMinDepth( GameObject obj )
        {
            UIWidget[] widgets   = obj     != null ? obj.GetComponentsInChildren< UIWidget >() : null;

            int        nbWidgets = widgets != null ? widgets.Length : 0;

            int        minDepth  = int.MaxValue;

            for( int widget = 0; widget < nbWidgets; ++widget )
            {
                if( minDepth > widgets[ widget ].depth )
                {
                    minDepth = widgets[ widget ].depth;
                } 
            }

            return minDepth;
        }

        private static void SetWidgetDepth( GameObject obj, int depth )
        {
            UIWidget widget = obj != null ? obj.GetComponent< UIWidget >() : null;
            
            if( widget != null ) widget.depth = depth;
        }

        private static ScreenCollider Get( GameObject root, bool add )
        {
            ScreenCollider instance = null;

            if( root != null )
            {
                instance = root.GetComponentInChildren< ScreenCollider >();

                if( ( instance == null ) && ( add ) )
                {
                    GameObject obj = new GameObject( "ScreenCollider", typeof( ScreenCollider ) );

                    if( obj != null )
                    {
						obj.layer = root.layer;
                        obj.transform.parent = root.transform;
                        obj.transform.localScale = Vector3.one;                        
                        obj.transform.rotation = Quaternion.identity;                       
                        instance = obj.GetComponent< ScreenCollider >();
                        SetWidgetDepth( instance.gameObject, GetWidgetsMinDepth( root ) - 1 );
                    }
                }
            }

            return instance;
        }

        public static void Enable( GameObject root, bool enable )
        {
            ScreenCollider instance = Get( root, enable );

            if( instance != null ) instance.enabled = enable;
        }
    }
}

#endif // !AUBE_NO_UI