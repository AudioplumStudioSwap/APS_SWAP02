using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

//////////////////////////////////////////////////////////////////////////
// Copyright © 2010-2014 Artefacts Studio, all rights reserved.
//////////////////////////////////////////////////////////////////////////

namespace Aube
{
#if UNITY_EDITOR
    [ExecuteInEditMode]  
    public class StaticEditorModeCoroutine
    {
        //Doesn't work with time, only with yield return null 
        	public static StaticEditorModeCoroutine StartCoroutine( IEnumerator _routine )
            {
                StaticEditorModeCoroutine coroutine = new StaticEditorModeCoroutine(_routine);
                coroutine.Start();
                return coroutine;
            }
 
            readonly IEnumerator my_routine;
            StaticEditorModeCoroutine(IEnumerator _routine)
            {
                my_routine = _routine;
            }

            void Start()
            {
                EditorApplication.update += Update;
            }
            public void Stop()
            {
                EditorApplication.update -= Update;
            }

            void Update()
            {
                if (!my_routine.MoveNext())
                {
                    Stop();
                }
            }

    }
#endif
}