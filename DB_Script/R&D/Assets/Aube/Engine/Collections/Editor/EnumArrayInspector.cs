using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
    [CustomPropertyDrawer(typeof(EnumArrayBase), true)]
    public class EnumArrayInspector : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty a_property, GUIContent a_label)
        {
            SerializedProperty internalArrayProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".m_internalArray");
            if(internalArrayProperty.isExpanded)
            {
                float height = base.GetPropertyHeight(a_property, a_label);
                for(int elementIndex = 0; elementIndex < internalArrayProperty.arraySize; ++elementIndex)
                {
                    SerializedProperty elementProperty = internalArrayProperty.GetArrayElementAtIndex(elementIndex);
                    height += EditorGUI.GetPropertyHeight(elementProperty, new GUIContent(elementProperty.name), true);
                }
                return height;
            }
            else
            {
                return base.GetPropertyHeight(a_property, a_label);
            }
        }
        
        public override void OnGUI(Rect a_rect, SerializedProperty a_property, GUIContent a_label) 
        {
            SerializedProperty internalArrayProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".m_internalArray");
            SerializedProperty nameArrayProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".m_names");

            EditorCollection.DelegateElementName nameCallback = new EditorCollection.DelegateElementName(Curry.BindLast<int, SerializedProperty, string>(OnElementName, nameArrayProperty));

            Rect foldoutRect = new Rect(a_rect.x, a_rect.y, a_rect.width, base.GetPropertyHeight(a_property, a_label));
            EditorGUI.PropertyField(foldoutRect, a_property);
            internalArrayProperty.isExpanded = a_property.isExpanded;

            if (internalArrayProperty.isExpanded)
            {
                EditorGUI.indentLevel++;
                Rect collectionRect = new Rect(a_rect.x, a_rect.y + foldoutRect.height, a_rect.width, a_rect.height - foldoutRect.height);
                EditorCollection.Show(collectionRect, internalArrayProperty,
                                    EditorCollection.Option.CollectionFoldout | EditorCollection.Option.ElementLabel,
                                    nameCallback);
                EditorGUI.indentLevel--;
            }
        }

        private static string OnElementName(int a_index, SerializedProperty a_nameCollection)
        {
            SerializedProperty nameProperty = a_nameCollection.GetArrayElementAtIndex(a_index);
            return nameProperty.stringValue;
        }
    }
}