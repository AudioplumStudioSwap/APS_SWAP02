using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	public static class EditorCollection
	{
		[System.Flags]
		public enum Option
		{
			CollectionFoldout	= 1 << 0,
			CollectionSize 		= 1 << 1,
			CollectionLabel 	= 1 << 2,
			ElementLabel 		= 1 << 3,
			ElementAdd			= 1 << 4,
			ElementRemove		= 1 << 5,
			BoxElement			= 1 << 6,

			None = 0,
			Default = CollectionFoldout | CollectionSize | CollectionLabel | ElementLabel,
			Alternative = CollectionFoldout | ElementAdd | ElementRemove | CollectionLabel,
		}

		public delegate string DelegateElementName(int elementIndex);
		public delegate void DelegateElementAdded(int elementIndex, SerializedProperty property);
		public delegate void DelegateElementRemoved(int elementIndex, SerializedProperty property);
		public delegate void DelegateDisplayElement(int elementIndex, string label, SerializedProperty property);   

		public static void Show(SerializedProperty collection) { Show(collection, Option.Default, null); }
		public static void Show(SerializedProperty collection, Option options) { Show(collection, options, null); }
		public static void Show(SerializedProperty collection, Option options, DelegateElementName delegateElementName) { Show(collection, options, delegateElementName, null, null, null); }
		public static void Show(SerializedProperty collection, Option options, DelegateElementName delegateElementName, DelegateElementAdded delegateElementAdded, DelegateElementRemoved delegateElementRemoved) { Show(collection, options, delegateElementName, delegateElementAdded, delegateElementRemoved, null); }
		public static void Show(SerializedProperty collection, Option options, DelegateElementName delegateElementName, DelegateElementAdded delegateElementAdded, DelegateElementRemoved delegateElementRemoved, DelegateDisplayElement delegateDisplayElement)
		{
			if (!collection.isArray)
			{
				EditorGUILayout.HelpBox(collection.name + " is neither an array nor a list!", MessageType.Error);
				return;
			}

			bool showCollectionFoldout = (options & Option.CollectionFoldout) != 0;
			bool showCollectionLabel = (options & Option.CollectionLabel) != 0;
			bool showCollectionSize = (options & Option.CollectionSize) != 0;
			bool showAddButton = (options & Option.ElementAdd) != 0;
			
			if(showCollectionLabel)
			{
				if(showCollectionFoldout)
				{
					EditorGUILayout.PropertyField(collection);
				}
				else
				{
					string displayName = EditorFuncs.ToDisplayableName(collection.name);
					EditorGUILayout.LabelField(displayName, EditorStyles.boldLabel);
				}

				++EditorGUI.indentLevel;
			}

			if(!showCollectionLabel  ||  !showCollectionFoldout || collection.isExpanded)
			{
				SerializedProperty size = collection.FindPropertyRelative("Array.size");
				if(showCollectionSize)
				{
					EditorGUILayout.PropertyField(size);
				}
				if (size.hasMultipleDifferentValues)
				{
					EditorGUILayout.HelpBox("Not showing collection with different sizes.", MessageType.Info);
				}
				else
				{
					ShowElements(collection, options, delegateElementName, delegateElementRemoved, delegateDisplayElement);

					if(showAddButton)
					{
						if(GUILayout.Button("+", GUILayout.ExpandWidth(false)))
						{
							++collection.arraySize;
							if(delegateElementAdded != null)
							{
								delegateElementAdded(collection.arraySize - 1, collection.GetArrayElementAtIndex(collection.arraySize - 1));
							}
						}
					}
				}
			}

			if(showCollectionLabel)
			{
				--EditorGUI.indentLevel;
			}
		}

		private static void ShowElements (SerializedProperty list, Option options, DelegateElementName delegateElementName, DelegateElementRemoved delegateElementRemoved, DelegateDisplayElement delegateDisplayElement)
		{
			bool showElementLabels = (options & Option.ElementLabel) != 0;
			bool showRemoveButton = (options & Option.ElementRemove) != 0;
			bool showBox = (options & Option.BoxElement) != 0;

			for(int i = 0; i < list.arraySize; i++)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));

				// display element BEGIN
				Rect verticalRect = new Rect(0, 0, 0, 0);
				if(showBox)
				{
					verticalRect = EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
				}
				else
				{
					verticalRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
				}
				{
					string label = "";
					if(showElementLabels)
					{
						if(delegateElementName != null)
						{
							label = delegateElementName(i);
						}
						else
						{
							label = list.GetArrayElementAtIndex(i).name;
						}
					}

					SerializedProperty elementProperty = list.GetArrayElementAtIndex(i);
					if(delegateDisplayElement == null)
					{
						EditorGUILayout.PropertyField(elementProperty,
						    	                          string.IsNullOrEmpty(label)? GUIContent.none : new GUIContent(label), true);
					}
					else
					{
						delegateDisplayElement(i, label, elementProperty);
					}
				}
				EditorGUILayout.EndVertical();
				// display element END

				if(showRemoveButton)
				{
					Vector2 buttonSize = GUI.skin.button.CalcSize(new GUIContent("-"));

					Rect buttonDeleteMaxRect = GUILayoutUtility.GetRect(buttonSize.x, verticalRect.height, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
					float yOffset = (verticalRect.height > buttonSize.y)? (verticalRect.height - buttonSize.y) * 0.5f : 0.0f;
					Rect buttonRect = new Rect(buttonDeleteMaxRect.x, buttonDeleteMaxRect.y + yOffset, buttonDeleteMaxRect.width, buttonSize.y);
					if(GUI.Button(buttonRect, "-"))
					{
						if(delegateElementRemoved != null)
						{
							delegateElementRemoved(i, list.GetArrayElementAtIndex(i));
						}

						list.DeleteArrayElementAtIndex(i);
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}

        public static void Show(Rect a_rect, SerializedProperty collection) { Show(a_rect, collection, Option.Default); }
        public static void Show(Rect a_rect, SerializedProperty collection, Option options) { Show(a_rect, collection, options, null); }
        public static void Show(Rect a_rect, SerializedProperty collection, Option options, DelegateElementName delegateElementName) { Show(a_rect, collection, options, delegateElementName, null, null); }
        public static void Show(Rect a_rect, SerializedProperty collection, Option options, DelegateElementName delegateElementName, DelegateElementAdded delegateElementAdded, DelegateElementRemoved delegateElementRemoved)
        {
            if (!collection.isArray)
            {
                EditorGUI.HelpBox(a_rect, collection.name + " is neither an array nor a list!", MessageType.Error);
                return;
            }
            
            bool showCollectionFoldout = (options & Option.CollectionFoldout) != 0;
            bool showCollectionLabel = (options & Option.CollectionLabel) != 0;
            bool showCollectionSize = (options & Option.CollectionSize) != 0;
            bool showAddButton = (options & Option.ElementAdd) != 0;

            float top = a_rect.y;

            if(showCollectionLabel)
            {
                if(showCollectionFoldout)
                {
                    Rect foldoutRect = new Rect(a_rect.x, top, a_rect.width, 16.0f);
                    EditorGUI.PropertyField(foldoutRect, collection);

                    top += foldoutRect.height;
                }
                else
                {
                    GUIContent displayName = new GUIContent(EditorFuncs.ToDisplayableName(collection.name));
                    float height = EditorStyles.boldLabel.CalcHeight(displayName, a_rect.width);
                    Rect collectionNameRect = new Rect(a_rect.x, top, a_rect.width, height);
                    EditorGUI.LabelField(collectionNameRect, displayName, EditorStyles.boldLabel);

                    top += collectionNameRect.height;
                }
                
                ++EditorGUI.indentLevel;
            }
            
            if(!showCollectionLabel  ||  !showCollectionFoldout || collection.isExpanded)
            {
                SerializedProperty size = collection.FindPropertyRelative("Array.size");
                if(showCollectionSize)
                {
                    Rect sizeRect = new Rect(a_rect.x, top, a_rect.width, 16.0f);
                    EditorGUI.PropertyField(sizeRect, size);
                }
                if (size.hasMultipleDifferentValues)
                {
                    // TODO EditorGUILayout.HelpBox("Not showing collection with different sizes.", MessageType.Info);
                }
                else
                {
                    ShowElements(a_rect, ref top, collection, options, delegateElementName, delegateElementRemoved);
                    
                    if(showAddButton)
                    {
                        GUIContent addButtonContent = new GUIContent("+");
                        Vector2 addButtonSize = GUI.skin.button.CalcSize(addButtonContent);
                        Rect addButtonRect = new Rect(a_rect.x, top, addButtonSize.x, addButtonSize.y);

                        if(GUI.Button(addButtonRect, "+"))
                        {
                            ++collection.arraySize;
                            if(delegateElementAdded != null)
                            {
                                delegateElementAdded(collection.arraySize - 1, collection.GetArrayElementAtIndex(collection.arraySize - 1));
                            }
                        }
                    }
                }
            }
            
            if(showCollectionLabel)
            {
                --EditorGUI.indentLevel;
            }
        }

        private static void ShowElements(Rect a_rect, ref float a_top, SerializedProperty list, Option options, DelegateElementName delegateElementName, DelegateElementRemoved delegateElementRemoved)
        {
            bool showElementLabels = (options & Option.ElementLabel) != 0;
            bool showRemoveButton = (options & Option.ElementRemove) != 0;
            bool showBox = (options & Option.BoxElement) != 0;
            
            for(int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty elementProperty = list.GetArrayElementAtIndex(i);

                // label
                string label = string.Empty;
                if(showElementLabels)
                {
                    if(delegateElementName != null)
                    {
                        label = delegateElementName(i);
                    }
                    else
                    {
                        label = elementProperty.name;
                    }
                }
                GUIContent labelContent = string.IsNullOrEmpty(label)? GUIContent.none : new GUIContent(label);
                float elementHeight = EditorGUI.GetPropertyHeight(elementProperty, labelContent, true);

                GUIContent removeButtonContent = new GUIContent("-");
                Vector2 removeButtonSize = (showRemoveButton)? GUI.skin.button.CalcSize(removeButtonContent) : Vector2.zero;

                Rect elementRect = new Rect(a_rect.x, a_top, a_rect.width - removeButtonSize.x, elementHeight);
                if(showBox)
                {
                    GUI.Box(elementRect, GUIContent.none);
                }
                EditorGUI.PropertyField(elementRect, elementProperty, labelContent, true);

                if(showRemoveButton)
                {
                    Rect removeButtonRect = new Rect(a_rect.x + elementRect.width, a_top + (elementHeight - removeButtonSize.y) * 0.5f, removeButtonSize.x, removeButtonSize.y);
                    if(GUI.Button(removeButtonRect, "-"))
                    {
                        if(delegateElementRemoved != null)
                        {
                            delegateElementRemoved(i, list.GetArrayElementAtIndex(i));
                        }
                        
                        list.DeleteArrayElementAtIndex(i);
                    }
                }

                a_top += elementHeight;
            }
        }
	}
} // namespace Aube