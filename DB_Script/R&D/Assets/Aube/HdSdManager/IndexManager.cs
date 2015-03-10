using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//////////////////////////////////////////////////////////////////////////
// Copyright © 2010-2014 Artefacts Studio, all rights reserved.
//////////////////////////////////////////////////////////////////////////

namespace Aube
{

    [ExecuteInEditMode]
    public class IndexManager : MonoBehaviour
    {

        public List<ObjectIndexer> m_sceneObjects;

        public IndexManager()
        {
            m_sceneObjects = new List<ObjectIndexer>();
        }
        public ObjectIndexer FindByID(int id)
        {
            ObjectIndexer result = null;

            foreach (ObjectIndexer index in m_sceneObjects)
            {
                if (index.m_id == id)
                {
                    result = index;
                    break;
                }
            }
            return result;
        }
        public ObjectIndexer FindByGameObject(GameObject obj)
        {
            ObjectIndexer result = null;

            foreach (ObjectIndexer index in m_sceneObjects)
            {
                if (index.m_obj.GetInstanceID() == obj.GetInstanceID())
                {
                    result = index;
                    break;
                }
            }
            return result;
        }
        public ArrayList cachedObjects;


    }

    public class SelectionBakingParam
    {
        public UnityEngine.Object[] objs;

    }
}