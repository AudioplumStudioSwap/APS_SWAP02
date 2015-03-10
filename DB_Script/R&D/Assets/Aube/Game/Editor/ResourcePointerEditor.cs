using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Aube
{
    //! @class PrefabPointerEditor
    //!
    //! @brief Custom Inspector for class Resource Pointers
	[CustomPropertyDrawer(typeof(IResourcePointer), true)]
	public class ResourcePointerEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
            SerializedProperty foldoutProp = property.FindPropertyRelative("m_foldout");
			return ((foldoutProp.boolValue)? 3 : 1) * base.GetPropertyHeight(property, label);
		}
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty loadKindProp = property.FindPropertyRelative("m_loadKind");
            SerializedProperty foldoutProp = property.FindPropertyRelative("m_foldout");
            
			IResourcePointer resourcePointer = EditorFuncs.GetObject(property) as IResourcePointer;
            if(!m_assets.ContainsKey(property))
            {
				m_assets.Add(property, resourcePointer.ResourceObject);
            }

            Rect labelRect = new Rect(position.x, position.y, position.width, ms_lineHeight);
            foldoutProp.boolValue = EditorGUI.Foldout(labelRect, foldoutProp.boolValue, label);             
                        
            if (foldoutProp.boolValue)
            {
                ++EditorGUI.indentLevel;

                Object resource = null;

                Rect prefabRect = labelRect;
                prefabRect.y += labelRect.height;
                m_assets.TryGetValue(property, out resource);
                resource = EditorGUI.ObjectField(prefabRect, (foldoutProp.boolValue) ? resourcePointer.ResourceLabel : null, resource, resourcePointer.ResourceType, false);

                Rect loadKindRect = prefabRect;
                loadKindRect.y += prefabRect.height;
                EditorGUI.PropertyField(loadKindRect, loadKindProp);

                --EditorGUI.indentLevel;

                if (GUI.changed)
                {
                    SynchronizeAttributes(property, resource, (PrefabPointer.LoadKind)loadKindProp.intValue);
                    m_assets.Remove(property);
                }
            }            
		}

		//! @brief synchronize a serialized property with a prefab (editor method)
		public static void SynchronizeAttributes(SerializedProperty a_property, Object a_resource, PrefabPointer.LoadKind a_loadKind)
		{
			SerializedProperty loadKindProp = a_property.FindPropertyRelative("m_loadKind");
			SerializedProperty assetProperty = a_property.FindPropertyRelative("m_resourceAsset");
			SerializedProperty pathProperty = a_property.FindPropertyRelative("m_resourcePath");
			
			loadKindProp.intValue = (int)a_loadKind;
			
			string path = (a_resource != null)? UnityEditor.AssetDatabase.GetAssetPath(a_resource) : null;
			if(path != null && PathAttribute.MakePathRelativeToResourcesFolder(ref path))
			{
				string filename = System.IO.Path.GetFileNameWithoutExtension(path);
				path = path.Substring(0, path.LastIndexOf(filename) + filename.Length);
			}
			
			Object loadedResource = Resources.Load(path);
			
			switch(a_loadKind)
			{
				case IResourcePointer.LoadKind.PreLoaded:
				{
					assetProperty.objectReferenceValue = loadedResource;
					pathProperty.stringValue = null;
					break;
				}
				case IResourcePointer.LoadKind.LoadOnDemand:
				{
					assetProperty.objectReferenceValue = null;
					pathProperty.stringValue = (loadedResource != null)? path : null;
					break;
				}
				default:
				{
					Assertion.UnreachableCode();
					break;
				}
			}
			
			if(a_resource != null && assetProperty.objectReferenceValue == null && string.IsNullOrEmpty(pathProperty.stringValue))
			{
				Debug.LogWarning(a_resource.name + ": invalid prefab (must be placed in a 'Resources' directory)");
			}
		}

#region Private
	#region Attributes
		private static float ms_lineHeight = 16.0f;
		private Dictionary<SerializedProperty, Object> m_assets = new Dictionary<SerializedProperty, Object>();
	#endregion
#endregion
    }
}
