#if !AUBE_NO_UI
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Aube
{
    //! @class NGUIAssetEditor
    //!
    //! @brief Custom Inspector for class NGUIAsset
    [CustomEditor(typeof(NGUIAsset), true)]
    public class NGUIAssetEditor : Editor
    {
    #region Methods
    #region Public
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RootPrefab"));

            SerializedProperty baseLayer = serializedObject.FindProperty("m_baseLayer");
            baseLayer.intValue = EditorGUILayout.LayerField("Base Layer", baseLayer.intValue);

            SerializedProperty lockLayer = serializedObject.FindProperty("m_lockLayer");
            lockLayer.intValue = EditorGUILayout.LayerField("Lock Layer", lockLayer.intValue);

            serializedObject.ApplyModifiedProperties();
        }
    #endregion
    #endregion
    }
} // namespace Aube

#endif // !AUBE_NO_UI