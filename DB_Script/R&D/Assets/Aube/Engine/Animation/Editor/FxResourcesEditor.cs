using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Aube
{
    //! @class FxResourceEditor
    //!
    //! @brief Custom inspector for class FxResources.
    [CustomEditor(typeof(FxResources))]
    public class FxResourcesEditor : Editor
    {
    #region Attributes
    #region Private
        private const string RESOURCES_FIELD = "m_resources";
        private const string ROOT_FIELD = "m_root";
        private const string IDENT_FIELD = "m_ident";
        private const string PREFAB_FIELD = "m_prefab";
        private const string PARENT_FIELD = "m_parent";
        private const string FOLDOUT_FIELD = "m_foldout";
        private const string HAS_PARENT_FIELD = "m_hasParent";

        private Dictionary<int, Transform> m_parents = new Dictionary<int, Transform>();
        private List<int> m_removeList = new List<int>();
    #endregion
    #endregion
        
    #region Methods
    #region Public
        public override void OnInspectorGUI()
        {
            SerializedProperty resourcesProp = serializedObject.FindProperty(RESOURCES_FIELD);
            SerializedProperty rootProp = serializedObject.FindProperty(ROOT_FIELD);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(rootProp);
            bool rootChanged = EditorGUI.EndChangeCheck();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Fold"))
            {
                ApplyFold(true);
            }

            if (GUILayout.Button("Unfold"))
            {
                ApplyFold(false);
            }
            GUILayout.EndHorizontal();

            EditorCollection.Option options = EditorCollection.Option.ElementAdd | 
                EditorCollection.Option.ElementRemove | EditorCollection.Option.ElementLabel | EditorCollection.Option.BoxElement;
            EditorCollection.Show(resourcesProp, options, OnElementName, OnElementAdded, null, OnDisplayElement);

            if (rootChanged)
            {
                m_parents.Clear();
            }

            for (int i = 0, count = m_removeList.Count; i < count; ++i)
            {
                resourcesProp.DeleteArrayElementAtIndex(m_removeList[i]);
            }
            m_removeList.Clear();
            serializedObject.ApplyModifiedProperties();
        }
    #endregion
    #region Private
        private string OnElementName(int index)
        {
            SerializedProperty resourceProp = GetElementProp(index);
            SerializedProperty identProp = resourceProp.FindPropertyRelative(IDENT_FIELD);
            SerializedProperty prefabProp = resourceProp.FindPropertyRelative(PREFAB_FIELD);
            SerializedProperty foldoutProp = resourceProp.FindPropertyRelative(FOLDOUT_FIELD);
            SerializedProperty hasParentProp = resourceProp.FindPropertyRelative(HAS_PARENT_FIELD);

            string name = identProp.stringValue;
            string desc = "";

            if (!foldoutProp.boolValue)
            {
                desc += (prefabProp.objectReferenceValue != null)? prefabProp.objectReferenceValue.name : "None";

                if (hasParentProp.boolValue)
                {
                    Transform parent;
                    m_parents.TryGetValue(index, out parent);
                    desc += ((desc == "")? "" : " - ") + ((parent != null)? parent.name : "None");
                }
            }
            return name + ((desc == "")? desc : "   (" + desc + ")");
        }

        private void OnElementAdded(int index, SerializedProperty property)
        {
            property.FindPropertyRelative(IDENT_FIELD).stringValue = "FX_1";
            property.FindPropertyRelative(PREFAB_FIELD).objectReferenceValue = null;
            property.FindPropertyRelative(PARENT_FIELD).stringValue = null;
            CheckIdent(index);
        }

        private SerializedProperty GetElementProp(int index)
        {
            SerializedProperty resourcesProp = serializedObject.FindProperty(RESOURCES_FIELD);
            return resourcesProp.GetArrayElementAtIndex(index);
        }

        private void OnDisplayElement(int index, string label, SerializedProperty property)
        {
            SerializedProperty rootProp = serializedObject.FindProperty(ROOT_FIELD);
            SerializedProperty identProp = property.FindPropertyRelative(IDENT_FIELD);
            SerializedProperty prefabProp = property.FindPropertyRelative(PREFAB_FIELD);
            SerializedProperty parentProp = property.FindPropertyRelative(PARENT_FIELD);
            SerializedProperty foldoutProp = property.FindPropertyRelative(FOLDOUT_FIELD);
            SerializedProperty hasParentProp = property.FindPropertyRelative(HAS_PARENT_FIELD);

            if (!m_parents.ContainsKey(index))
            {
                if (rootProp.objectReferenceValue != null)
                {
                    MethodInfo method = target.GetType().GetMethod("GetParent", BindingFlags.Instance | BindingFlags.NonPublic);
                    Transform parent = method.Invoke(target as FxResources, new object[] { index }) as Transform;
                    m_parents.Add(index, parent);
                }
            }
            else if (rootProp.objectReferenceValue == null)
            {
                m_parents.Remove(index);
            }
            
            foldoutProp.boolValue = EditorGUILayout.Foldout(foldoutProp.boolValue, label);
            
            bool duplicatedIdent = HasDuplicatedIdent(index);

            if (duplicatedIdent)
            {
                foldoutProp.boolValue = duplicatedIdent;
            }

            if (foldoutProp.boolValue)
            {
                EditorGUI.indentLevel += 1;

                Color color = GUI.color;
                
                if (HasDuplicatedIdent(index))
                {
                    GUI.color = Color.red;
                    foldoutProp.boolValue = true;
                }

                GUI.SetNextControlName("Ident" + index);
                identProp.stringValue = EditorGUILayout.TextField("Ident", identProp.stringValue);

                if (HasDuplicatedIdent(index))
                {
                    GUI.color = color;
                }

                GameObject prefab = prefabProp.objectReferenceValue as GameObject;
                prefab = EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false) as GameObject;
                if (prefab != prefabProp.objectReferenceValue)
                {
                    prefabProp.objectReferenceValue = prefab;

                    if (prefab != null)
                    {
                        identProp.stringValue = prefab.name;
                        CheckIdent(index);
                    }                    
                }
                
                if (hasParentProp.boolValue)
                {
                    string parentField = "Parent";

                    if (rootProp.objectReferenceValue != null)
                    {
                        Transform parent;
                        m_parents.TryGetValue(index, out parent);
                        Transform guiParent = EditorGUILayout.ObjectField(parentField, parent, typeof(Transform), true) as Transform;
                        if (guiParent != parent)
                        {
                            m_parents.Remove(index);
                            parentProp.stringValue = GetChildPath(rootProp.objectReferenceValue as Transform, guiParent, false);
                        }
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(parentProp);
                    }
                }
                
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Toggle parent"))
                {
                    hasParentProp.boolValue = !hasParentProp.boolValue;
                    m_parents.Remove(index);
                    parentProp.stringValue = null;
                }
                GUILayout.EndHorizontal();
                
                EditorGUI.indentLevel -= 1;
            }
        }

        private string GetChildPath(Transform root, Transform child, bool includeRoot)
        {
            if (root == child)
            {
                return root.name;
            }

            for (int i = 0, count = root.childCount; i < count; ++i)
            {
                string path = GetChildPath(root.GetChild(i), child, true);

                if (path != null)
                {
                    return ((includeRoot)? root.name + "/" : "") + path;
                }
            }
            return null;
        }

        private void CheckIdent(int index)
        {
            SerializedProperty property = GetElementProp(index).FindPropertyRelative(IDENT_FIELD);
            property.stringValue.ToUpperInvariant();

            SerializedProperty resourcesProp = serializedObject.FindProperty(RESOURCES_FIELD);
            List<int> numberList = new List<int>();

            string name;
            int number;
            SplitIdentName(property.stringValue, out name, out number);

            for (int i = 0, count = resourcesProp.arraySize; i < count; ++i)
            {
                if (index != i)
                {
                    SerializedProperty resourceProp = resourcesProp.GetArrayElementAtIndex(i);
                    SerializedProperty identProp = resourceProp.FindPropertyRelative(IDENT_FIELD);

                    string otherName;
                    int otherNumber;
                    SplitIdentName(identProp.stringValue, out otherName, out otherNumber);

                    if (name == otherName)
                    {
                        numberList.Add(otherNumber);
                    }
                }
            }

            number = (numberList.Count > 0)? 1 : number;

            while (number >= 0 && numberList.Contains(number))
            {
                ++number; 
            }
            
            property.stringValue = name + ((number < 0)? "" : "_" + number);
        }

        private bool HasDuplicatedIdent(int index)
        {
            SerializedProperty property = GetElementProp(index).FindPropertyRelative(IDENT_FIELD);
            SerializedProperty resourcesProp = serializedObject.FindProperty(RESOURCES_FIELD);
            
            for (int i = 0, count = resourcesProp.arraySize; i < count; ++i)
            {
                if (index != i)
                {
                    SerializedProperty resourceProp = resourcesProp.GetArrayElementAtIndex(i);
                    SerializedProperty identProp = resourceProp.FindPropertyRelative(IDENT_FIELD);

                    if (property.stringValue == identProp.stringValue)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ApplyFold(bool fold)
        {
            SerializedProperty resourcesProp = serializedObject.FindProperty(RESOURCES_FIELD);

            for (int i = 0, count = resourcesProp.arraySize; i < count; ++i)
            {
                SerializedProperty resourceProp = resourcesProp.GetArrayElementAtIndex(i);
                resourceProp.FindPropertyRelative(FOLDOUT_FIELD).boolValue = !fold;
            }
        }

        private void SplitIdentName(string ident, out string baseName, out int number)
        {
            int index = ident.LastIndexOf("_");
            
            if (index >= 0 && int.TryParse(ident.Substring(index + 1), out number))
            {
                baseName = ident.Substring(0, index);
            }
            else
            {
                number = -1;
                baseName = ident;
            }
        }
    #endregion
    #endregion
    }
}
