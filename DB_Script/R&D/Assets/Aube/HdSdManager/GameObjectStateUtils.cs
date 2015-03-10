using UnityEngine;
using System.Collections;

//////////////////////////////////////////////////////////////////////////
// Copyright © 2010-2014 Artefacts Studio, all rights reserved.
//////////////////////////////////////////////////////////////////////////

namespace Aube
{
    public static class GameObjectStateUtils
    {
        public static void DynamicToStatic(string rootName, string namePart, bool toStatic)
        {
            if (namePart != null && namePart != "")
            {
                GameObject geometry = GameObject.Find(rootName);
                if (geometry != null)
                {
                    Component[] Objects = geometry.GetComponentsInChildren(typeof(Transform));

                    foreach (Component obj in Objects)
                    {
                        if (obj.gameObject.name.Contains(namePart))
                        {
                            obj.gameObject.isStatic = toStatic;
                            ChildrenToStatic(obj.gameObject, toStatic);
                        }

                    }

                }
            }
        }

        static void ChildrenToStatic(GameObject Go, bool toStatic)
        {
            Component[] Objects = Go.GetComponentsInChildren(typeof(Transform));
            foreach (Component obj in Objects)
            {
                obj.gameObject.isStatic = toStatic;
            }

        }

    }
}
