#if !AUBE_NO_UI
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

namespace Aube
{
    //! @class UIResourceDescBaseEditor
    //!
    //! @brief Custom Inspector for class UIResourceDescBase
    [CustomEditor(typeof(UIResourceDescBase), true)]
    public class UIResourceDescBaseEditor : Editor
    {
    #region Attributes
    #region Private
        private int m_loadKind = 0;
        private Dictionary<SerializedProperty, GameObject> m_prefabs = new Dictionary<SerializedProperty, GameObject>();
    #endregion
    #endregion

    #region Methods
    #region Public
        public override void OnInspectorGUI()
        {
            SerializedProperty resourcesProp = serializedObject.FindProperty("m_resources.m_internalArray");

            EditorGUILayout.BeginHorizontal();
            m_loadKind = EditorGUILayout.Popup("Loading behaviour", m_loadKind, System.Enum.GetNames(typeof(PrefabPointer.LoadKind)));

            if (GUILayout.Button("Apply") && resourcesProp != null)
            {
                SetBehaviour(resourcesProp, (PrefabPointer.LoadKind)m_loadKind);
            }
            EditorGUILayout.EndHorizontal();

            if (resourcesProp != null)
            {
                EditorCollection.Show(resourcesProp, EditorCollection.Option.ElementLabel | EditorCollection.Option.BoxElement, OnElementName, null, null, OnDisplayElement);
            }
            serializedObject.ApplyModifiedProperties();
        }
    #endregion
    #region Private
        private void OnEnable()
        {
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }

        private string OnElementName(int index)
        {
            MethodInfo method = target.GetType().GetMethod("GetResourceName", BindingFlags.Instance | BindingFlags.NonPublic);
            return FormatEditorField(method.Invoke(target, new object[] { (uint)index }) as string);
        }

        private void OnDisplayElement(int index, string label, SerializedProperty property)
        {
            SerializedProperty prefabProp = property.FindPropertyRelative("m_prefab");
            SerializedProperty foldoutProp = property.FindPropertyRelative("m_foldout");
            
            GameObject node = GetPrefab(prefabProp, false);
            UI.Page page = (node != null)? node.GetComponent<UI.Page>() : null;

            if (node == null || page == null)
            {
                foldoutProp.boolValue = true;
            }

            foldoutProp.boolValue = EditorGUILayout.Foldout(foldoutProp.boolValue, label);

            if (foldoutProp.boolValue)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(prefabProp, new GUIContent("Page"), true);

                if (GUI.changed)
                {
                    node = GetPrefab(prefabProp, true);
                }
                
                if (node != null && page == null)
                {
                    EditorGUILayout.BeginVertical("Button");
                    Color backup = GUI.contentColor;
                    GUI.contentColor = Color.yellow;
                    EditorGUILayout.LabelField("No Page found");
                    GUI.contentColor = backup;
                    EditorGUILayout.EndVertical();
                }

                if (page != null)
                {
                    System.Array values = System.Enum.GetValues(typeof(UI.Page.Option));
                    string[] names = System.Enum.GetNames(typeof(UI.Page.Option));

                    MethodInfo method = typeof(UI.Page).GetMethod("SetOption", BindingFlags.Instance | BindingFlags.NonPublic);

                    for (int i = 0, count = values.Length; i < count; ++i)
                    {
                        UI.Page.Option flag = (UI.Page.Option)values.GetValue(i);
                        bool option = EditorGUILayout.Toggle(FormatEditorField(names[i]), page.HasOption(flag));
                        if (option != page.HasOption(flag))
                        {
                            method.Invoke(page, new object[] { flag, option });
                            EditorUtility.SetDirty(page);
                        }
                    }
                }
                EditorGUI.indentLevel -= 1;
            }
        }
        
        private GameObject GetPrefab(SerializedProperty prefabProp, bool clear)
        {
            if (clear)
            {
                m_prefabs.Remove(prefabProp);
            }

            if (!m_prefabs.ContainsKey(prefabProp))
            {
				SerializedProperty loadKindProp = prefabProp.FindPropertyRelative("m_loadKind");
				SerializedProperty assetProp = prefabProp.FindPropertyRelative("m_resourceAsset");
				SerializedProperty pathProp = prefabProp.FindPropertyRelative("m_resourcePath");

				m_prefabs.Add(prefabProp, PrefabPointer.GetResource((PrefabPointer.LoadKind)loadKindProp.intValue, assetProp.objectReferenceValue as GameObject, pathProp.stringValue));
            }

            GameObject node = null;
            m_prefabs.TryGetValue(prefabProp, out node);
            return node;
        }

        private void SetBehaviour(SerializedProperty array, PrefabPointer.LoadKind behaviour)
        {
            for (int i = 0, count = array.arraySize; i < count; ++i)
            {
                SerializedProperty item = array.GetArrayElementAtIndex(i);
                SerializedProperty pointer = item.FindPropertyRelative("m_prefab");
				ResourcePointerEditor.SynchronizeAttributes(pointer, GetPrefab(pointer, false), behaviour);
            }
        }

        private string FormatEditorField(string name)
        {
            string result = "";
            string[] split = name.Split('_');

            for (int i = 0; i < split.Length; ++i)
            {
                if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(split[i]))
                {
                    result += " ";
                }

                string start = (split[i].Length > 0) ? split[i].Substring(0, 1) : "";
                string next = (split[i].Length > 1) ? split[i].Substring(1, split[i].Length - 1) : "";
                result += start.ToUpperInvariant() + next.ToLowerInvariant();
            }
            return result;
        }
    #endregion
    #endregion
    }
}
#endif // !AUBE_NO_UI