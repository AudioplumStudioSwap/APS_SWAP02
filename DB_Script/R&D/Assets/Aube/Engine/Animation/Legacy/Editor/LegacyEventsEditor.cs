using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Aube
{
    //! @class LegacyEventsEditor
    //!
    //! @brief Custom Inspector for class LegacyEvents
    [CustomEditor(typeof(LegacyEvents))]
    public class LegacyEventsEditor : Editor
    {
    #region Attributes
    #region Private
        private const string ANIMATION_FIELD = "m_animation";
        private const string CLIPS_FIELD = "m_clips";
        private const string CLIP_NAME_FIELD = "m_clipName";
        private const string EVENTS_FIELD = "m_events";
        private const string FOLDOUT_FIELD = "m_foldout";

        private List<int> m_removeList = new List<int>();
    #endregion
    #endregion
        
    #region Methods
    #region Public
        public override void OnInspectorGUI()
        {
            SerializedProperty animationProp = serializedObject.FindProperty(ANIMATION_FIELD);
            SerializedProperty clipsProp = serializedObject.FindProperty(CLIPS_FIELD);
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Fold"))
            {
                ApplyFold(true);
            }

            if (GUILayout.Button("Unfold"))
            {
                ApplyFold(false);
            }

            if (GUILayout.Button("Sort"))
            {
                Sort();
            }
            GUILayout.EndHorizontal();

            Object animation = EditorGUILayout.ObjectField("Animator", animationProp.objectReferenceValue as Animation, typeof(Animation), true);

            if (animation != animationProp.objectReferenceValue)
            {
                animationProp.objectReferenceValue = animation;
                SynchronizeAnimation();
            }

            EditorCollection.Option options = EditorCollection.Option.ElementLabel | EditorCollection.Option.BoxElement;
            EditorCollection.Show(clipsProp, options, OnElementName, null, null, OnDisplayElement);

            for (int i = 0, count = m_removeList.Count; i < count; ++i)
            {
                clipsProp.DeleteArrayElementAtIndex(m_removeList[i]);
            }
            m_removeList.Clear();            
            serializedObject.ApplyModifiedProperties();
        }
    #endregion
    #region Private
        private void OnEnable()
        {
            SynchronizeAnimation();
        }

        private void SynchronizeAnimation()
        {
            serializedObject.ApplyModifiedProperties();
            MethodInfo method = target.GetType().GetMethod("Synchronize", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(target as LegacyEvents, new object[] { });
            EditorUtility.SetDirty(target as LegacyEvents);
            serializedObject.Update();
        }

        private string OnElementName(int index)
        {
            SerializedProperty clipProp = GetClipProp(index);
            return clipProp.FindPropertyRelative(CLIP_NAME_FIELD).stringValue;
        }

        private SerializedProperty GetClipProp(int index)
        {
            SerializedProperty resourcesProp = serializedObject.FindProperty(CLIPS_FIELD);
            return resourcesProp.GetArrayElementAtIndex(index);
        }

        private void OnDisplayElement(int index, string label, SerializedProperty property)
        {
            SerializedProperty eventsProp = property.FindPropertyRelative(EVENTS_FIELD);
            SerializedProperty foldoutProp = property.FindPropertyRelative(FOLDOUT_FIELD);

            foldoutProp.boolValue = EditorGUILayout.Foldout(foldoutProp.boolValue, label);
                        
            if (foldoutProp.boolValue)
            {                
                EditorGUI.indentLevel += 1;
                
                EditorCollection.Option options = EditorCollection.Option.BoxElement | EditorCollection.Option.ElementAdd |
                    EditorCollection.Option.ElementRemove | EditorCollection.Option.ElementLabel;
                EditorCollection.Show(eventsProp, options, OnEventName);
                
                EditorGUI.indentLevel -= 1;
            }
        }

        private string OnEventName(int index)
        {
            return "Event " + index;
        }

        private void ApplyFold(bool fold)
        {
            SerializedProperty clipsProp = serializedObject.FindProperty(CLIPS_FIELD);

            for (int i = 0, count = clipsProp.arraySize; i < count; ++i)
            {
                SerializedProperty clipProp = clipsProp.GetArrayElementAtIndex(i);
                clipProp.FindPropertyRelative(FOLDOUT_FIELD).boolValue = !fold;
            }
        }
        
        private void Sort()
        {
            serializedObject.ApplyModifiedProperties();
            MethodInfo method = target.GetType().GetMethod("SortEvents", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(target as LegacyEvents, new object[] { });
            EditorUtility.SetDirty(target as LegacyEvents);
            serializedObject.Update();
        }
    #endregion
    #endregion
    }
}
