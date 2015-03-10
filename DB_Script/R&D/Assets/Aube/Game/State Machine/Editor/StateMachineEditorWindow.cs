using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Aube
{
	//! @class StateMachineEditorWindow
	//!
	//!	@brief Editor window to edit a finite state machine
	public class StateMachineEditorWindow : EditorWindow
	{
#region Private
	#region Menu Items
		[MenuItem ("Assets/Create/Aube/new State Machine")]
		//! @brief Menu item to create the asset
		private static void CreateAsset()
		{
			StateMachine stateMachine = ScriptableObject.CreateInstance<StateMachine>();
			UnityEditor.ProjectWindowUtil.CreateAsset(stateMachine, "new State Machine.asset");
		}

		[MenuItem("Window/Aube/State Machine")]
		//! @brief Menu item to create the window
		private static void CreateWindow()
		{
			/*StateMachineEditorWindow window = */EditorWindow.GetWindow<StateMachineEditorWindow>("State Machine", true);
		}
	#endregion

	#region Unity Callbacks
		private void OnGUI()
		{
			GUI.skin = AubeEditor.skin;

			if(stateMachineObject == null  ||  stateMachineObject.targetObject == null)
			{
				EditorGUILayout.HelpBox("There is no state machine selected.", MessageType.Info);
				return;
			}

			stateMachineObject.Update();

			OnStateGUI();
			OnConnectionGUI();

			OnToolbarGUI();
			OnStatePathGUI();

			switch(Event.current.type)
			{
				case EventType.ContextClick:
				{
					if(selectedStateIndex == -1)
					{
						GenericMenu menu = new GenericMenu();					
						menu.AddItem(new GUIContent("Add State"), false, AddState, Event.current.mousePosition);
						
						menu.AddSeparator("");

						if(m_editingPath.Count > 0)
						{
							menu.AddItem(new GUIContent("Exit State"), false, ExitFromState);
						}
						else
						{
							menu.AddDisabledItem(new GUIContent("Exit State"));
						}

						menu.ShowAsContext ();
							
						Event.current.Use();
					}
				}
				break;

				case EventType.MouseDown:
				{
					selectedStateIndex = -1;
					selectedTransitionIndex = -1;

					if(Event.current.button == 2)
					{
						m_isDraggingView = true;
					}

					Event.current.Use();
				}
				break;

				case EventType.MouseDrag:
				{
					if(m_isDraggingView)
					{
						m_scroll += Event.current.delta;
						Repaint();
					}
				}
				break;

				case EventType.MouseUp:
				{
					if(m_isDraggingView)
					{
						m_isDraggingView = false;
					}
				}
				break;
			}
		}

		private void OnSelectionChange()
		{
			if(Selection.activeObject != null  &&  Selection.activeObject is StateMachine
			   &&  (m_rootStateMachineObject == null  ||  m_rootStateMachineObject.targetObject != Selection.activeObject))
			{
				m_editingPath = new List<StateMachineState>();

				m_rootStateMachineObject = new SerializedObject(Selection.activeObject);
				stateMachineObject = m_rootStateMachineObject;

				selectedStateIndex = -1;
				selectedTransitionIndex = -1;

				m_isDraggingView = false;
				m_scroll = Vector2.zero;
				m_isDraggingState = false;
				m_transitionFromStateIndex = -1;
				m_transitionToStateIndex = -1;

				Repaint();
			}
		}

		private void OnFocus()
		{
			if(Selection.activeObject != null  &&  Selection.activeObject is StateMachine
			   &&  (m_rootStateMachineObject == null  ||  m_rootStateMachineObject.targetObject != Selection.activeObject))
			{
				m_editingPath = new List<StateMachineState>();
				
				m_rootStateMachineObject = new SerializedObject(Selection.activeObject);
				stateMachineObject = m_rootStateMachineObject;
				
				selectedStateIndex = -1;
				selectedTransitionIndex = -1;
				
				m_isDraggingView = false;
				m_scroll = Vector2.zero;
				m_isDraggingState = false;
				m_transitionFromStateIndex = -1;
				m_transitionToStateIndex = -1;
				
				Repaint();
			}
		}

		private void OnEnable()
		{
			m_rootStateMachineObject = null;
			OnSelectionChange();
		}
	#endregion

	#region GUI calls
		private void OnStateGUI()
		{
			for(int stateIndex = 0; stateIndex < m_stateArrayProperty.arraySize; ++stateIndex)
			{
				DrawStateContent(stateIndex);
			}
		}

		private void DrawStateContent(int a_stateIndex)
		{
			SerializedProperty stateProperty = m_stateArrayProperty.GetArrayElementAtIndex(a_stateIndex);
			SerializedObject stateObject = new SerializedObject(stateProperty.objectReferenceValue);
			stateObject.Update();

			SerializedProperty positionProperty = stateObject.FindProperty(ms_statePositionAttributeName);
            Vector2 stateSize = CalculateStateSize(stateObject.targetObject.name);
            /*SerializedProperty componentHolderProperty = stateObject.FindProperty(ms_stateComponentHolderAttributeName);

            SerializedObject componentHolderObject = new SerializedObject(componentHolderProperty.objectReferenceValue);
            componentHolderObject.Update();

            Vector2 stateSize = CalculateStateSize(stateName);*/

			Rect backgroundRect = new Rect(positionProperty.vector2Value.x + m_scroll.x, positionProperty.vector2Value.y + m_scroll.y, stateSize.x, stateSize.y);
			Rect newPosition = StateEvents(a_stateIndex, backgroundRect);
			if(newPosition.position - m_scroll != backgroundRect.position)
			{
				positionProperty.vector2Value = newPosition.position - m_scroll;
			}

			// draw background
			if(Event.current.type == EventType.Repaint)
			{
				GUI.skin.GetStyle("FSM State Background").Draw(backgroundRect, GUIContent.none, GUIUtility.GetControlID(FocusType.Passive), m_selectedStateIndex == a_stateIndex);
			}
			GUI.BeginGroup(backgroundRect);

			Rect innerRect = new Rect(ms_stateWindowInnerMargins.x, ms_stateWindowInnerMargins.y, stateSize.x - ms_stateWindowInnerMargins.x * 2, stateSize.y - ms_stateWindowInnerMargins.y * 2);
			if(Event.current.type == EventType.Repaint)
			{
				GUI.skin.GetStyle("FSM State").Draw(innerRect, GUIContent.none, GUIUtility.GetControlID(FocusType.Passive), m_defaultStateProperty.intValue == a_stateIndex);
			}

			GUIStyle style = GUI.skin.GetStyle("FSM State Name");
            Vector2 nameSize = style.CalcSize(new GUIContent(stateObject.targetObject.name));
			Rect nameRect = new Rect((innerRect.width - nameSize.x) * 0.5f + innerRect.x, (innerRect.height - nameSize.y) * 0.5f + innerRect.y, nameSize.x, nameSize.y);
			if(selectedStateIndex == a_stateIndex)
			{
                stateObject.targetObject.name = GUI.TextField(nameRect, stateObject.targetObject.name, style);
                /*componentHolderObject.targetObject.name = GUI.TextField(nameRect, componentHolderObject.targetObject.name, style);
                stateObject.targetObject.name = componentHolderObject.targetObject.name + " (State)";*/
			}
			else
			{
                GUI.Label(nameRect, stateObject.targetObject.name, style);
			}

			GUI.EndGroup();
            //componentHolderObject.ApplyModifiedProperties();
			stateObject.ApplyModifiedProperties();
		}

		void OnConnectionGUI()
		{
			for(int transitionIndex = 0; transitionIndex < m_transitionArrayProperty.arraySize; ++transitionIndex)
			{
				SerializedProperty transitionProperty = m_transitionArrayProperty.GetArrayElementAtIndex(transitionIndex);
				SerializedObject transitionObject = new SerializedObject(transitionProperty.objectReferenceValue);

				SerializedProperty stateFromProperty = transitionObject.FindProperty(ms_transitionStateFromAttributeName);
				SerializedProperty stateToProperty = transitionObject.FindProperty(ms_transitionStateToAttributeName);

				SerializedObject stateFromObject = new SerializedObject(stateFromProperty.objectReferenceValue);
				SerializedObject stateToObject = new SerializedObject(stateToProperty.objectReferenceValue);

				SerializedProperty positionFromProperty = stateFromObject.FindProperty(ms_statePositionAttributeName);
				SerializedProperty positionToProperty = stateToObject.FindProperty(ms_statePositionAttributeName);

				Vector2 stateFromSize = CalculateStateSize(stateFromObject.targetObject.name);
				Vector2 stateToSize = CalculateStateSize(stateToObject.targetObject.name);

				Vector2 positionFrom = positionFromProperty.vector2Value + stateFromSize * 0.5f;
				Vector2 positionTo = positionToProperty.vector2Value + stateToSize * 0.5f;

				Vector2 offset = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0.0f, 0.0f, -90.0f), Vector3.one) * (positionTo - positionFrom).normalized;
				offset *= ms_transitionArrowHeadSize.y * 0.25f;

				Vector2 advancedPositionFrom = GetIntersectionPoint(positionFrom, positionTo, offset, stateFromSize - ms_stateWindowInnerMargins);
				Vector2 advancedPositionTo = GetIntersectionPoint(positionTo, positionFrom, offset, stateToSize - ms_stateWindowInnerMargins);

				DrawTransition(advancedPositionFrom, advancedPositionTo, transitionIndex);
			}

			if(m_transitionFromStateIndex != -1  ||  m_transitionToStateIndex != -1)
			{
				bool from = m_transitionFromStateIndex != -1;

				int stateIndex = from? m_transitionFromStateIndex : m_transitionToStateIndex;
				SerializedProperty stateProperty = m_stateArrayProperty.GetArrayElementAtIndex(stateIndex);

				SerializedObject stateObject = new SerializedObject(stateProperty.objectReferenceValue);
				SerializedProperty statePositionProperty = stateObject.FindProperty(ms_statePositionAttributeName);

				Vector2 stateSize = CalculateStateSize(stateObject.targetObject.name);

				Vector2 statePosition = statePositionProperty.vector2Value + ms_stateWindowSize * 0.5f;

				Vector2 advancedPosition = GetIntersectionPoint(statePosition, Event.current.mousePosition, Vector2.zero, stateSize - ms_stateWindowInnerMargins);

				Vector2 advancedPositionFrom = from? advancedPosition : Event.current.mousePosition;
				Vector2 advancedPositionTo = from? Event.current.mousePosition : advancedPosition;
				DrawTransition(advancedPositionFrom, advancedPositionTo, -1);

				Repaint();
			}
		}

		private Vector2 GetIntersectionPoint(Vector2 a_lineBeginPoint, Vector2 a_lineEndPoint, Vector2 a_offset, Vector2 a_rectSize)
		{
			float leftSideX = a_lineBeginPoint.x - a_rectSize.x * 0.5f;
			float rightSideX = a_lineBeginPoint.x + a_rectSize.x * 0.5f;
			float botSideY = a_lineBeginPoint.y + a_rectSize.y * 0.5f;
			float topSideY = a_lineBeginPoint.y - a_rectSize.y * 0.5f;

			if(a_lineEndPoint == a_lineBeginPoint)
			{
				return a_lineBeginPoint;
			}
			else if(a_lineEndPoint.x == a_lineBeginPoint.x)
			{
				return new Vector2(a_lineBeginPoint.x, (a_lineEndPoint.y > a_lineBeginPoint.y)? botSideY : topSideY) + a_offset;
			}
			else if(a_lineEndPoint.y == a_lineBeginPoint.y)
			{
				return new Vector2((a_lineEndPoint.x > a_lineBeginPoint.x)? rightSideX : leftSideX, a_lineBeginPoint.y) + a_offset;
			}
			else
			{
				float coeffLine = (a_lineEndPoint.y - a_lineBeginPoint.y) / (a_lineEndPoint.x - a_lineBeginPoint.x);
				float coeffRect = a_rectSize.y / a_rectSize.x;

				float constantLine = a_lineBeginPoint.y + a_offset.y - (a_lineBeginPoint.x + a_offset.x) * coeffLine;

				Vector2 rectCorner = new Vector2(a_lineEndPoint.x > a_lineBeginPoint.x? rightSideX : leftSideX, a_lineEndPoint.y > a_lineBeginPoint.y? botSideY : topSideY);
				float hypothenusys = (rectCorner - a_lineBeginPoint).magnitude;
				float adjacent = Vector2.Dot(rectCorner - a_lineBeginPoint, (a_lineEndPoint - a_lineBeginPoint).normalized);

				float opposite = Mathf.Sqrt(hypothenusys * hypothenusys - adjacent * adjacent);

				bool isOnRight = Vector2.Dot(a_offset, rectCorner - a_lineBeginPoint) >= 0.0f;

				// intersection on left or right side of the rectangle
				if((Mathf.Abs(coeffLine) < Mathf.Abs(coeffRect)  &&  opposite >= a_offset.magnitude)
				   ||  (Mathf.Abs(coeffLine) > Mathf.Abs(coeffRect)  &&  isOnRight  &&  opposite < a_offset.magnitude)
				   ||  (Mathf.Abs(coeffLine) < Mathf.Abs(coeffRect)  &&  isOnRight == false))
				{
					float x = a_lineEndPoint.x > a_lineBeginPoint.x? rightSideX : leftSideX;
					float y = coeffLine * x + constantLine;
					return new Vector2(x, y);
				}
				// intersection on top or bottom side of the rectangle
				else
				{
					float y = (a_lineEndPoint.y < a_lineBeginPoint.y)? topSideY : botSideY;
					float x = (y - constantLine) / coeffLine;
					return new Vector2(x, y);
				}
			}
		}

		private void DrawTransition(Vector2 a_positionFrom, Vector2 a_positionTo, int a_index)
		{
			Matrix4x4 oldMatrix = GUI.matrix;
			float angle = Vector2.Angle(Vector2.right, (a_positionTo - a_positionFrom).normalized);

			if(Vector2.Dot((a_positionTo - a_positionFrom), Vector2.up) < 0.0f)
			{
				angle = -angle;
			}

			Vector2 positionFrom = a_positionFrom + m_scroll;
			Vector2 positionTo = a_positionTo + m_scroll;

			float transitionLength = (positionTo - positionFrom).magnitude;
			Vector2 rotationPivot = (positionFrom + positionTo) * 0.5f;

			//Debug.Log("length : " + transitionLength + " | from : " + positionFrom + " | to : " + positionTo);
			//GUI.Box(new Rect(50.0f, 50.0f, transitionLength, 20.0f), "");

			// Rotate around pivot
			GUIUtility.RotateAroundPivot(angle, rotationPivot);

			// transition arrow body
			Rect transitionRect = new Rect();
			transitionRect.position = rotationPivot - Vector2.right * transitionLength * 0.5f;
			transitionRect.position += new Vector2(0.0f, -ms_transitionArrowSize * 0.5f);
			transitionRect.size = new Vector2(transitionLength - ms_transitionArrowHeadSize.x * 0.5f, ms_transitionArrowSize);

			// transition arrow head
			Rect transitionHeadRect = new Rect();
			transitionHeadRect.position = transitionRect.position;
			transitionHeadRect.position += Vector2.right * (transitionRect.size.x - ms_transitionArrowHeadSize.x * 0.3f);
			transitionHeadRect.position += Vector2.up * (ms_transitionArrowSize -ms_transitionArrowHeadSize.y) * 0.5f;
			transitionHeadRect.size = ms_transitionArrowHeadSize;

			if(a_index >= 0)
			{
				switch(Event.current.type)
				{
				case EventType.MouseUp:
				{
					if(Event.current.button == 1  &&  transitionRect.Contains(Event.current.mousePosition)  ||  transitionHeadRect.Contains(Event.current.mousePosition))
					{
						GenericMenu menu = new GenericMenu();					
						menu.AddItem(new GUIContent("Delete"), false, DeleteTransition, a_index);
						
						menu.ShowAsContext ();
						
						Event.current.Use();
					}
				}
					break;
				}
			}

			bool wasSelected = selectedTransitionIndex == a_index;

			bool selectionArrow = GUI.Toggle(transitionRect, wasSelected, GUIContent.none, "FSM Transition");
			bool selectionArrowHead = GUI.Toggle(transitionHeadRect, wasSelected, GUIContent.none, "FSM Transition Head");
			if(wasSelected == false  &&  (selectionArrow  ||  selectionArrowHead))
			{
				selectedTransitionIndex = a_index;
			}

			GUI.matrix = oldMatrix;
			Repaint();
		}

		private void OnToolbarGUI()
		{
			Rect toolbarRect = new Rect(0.0f, 0.0f, position.width, ms_toolbarHeight);
			GUILayout.BeginArea(toolbarRect, EditorStyles.toolbar);
			{
				GUILayout.BeginHorizontal();
				{
					if(GUILayout.Button("Center View", EditorStyles.toolbarButton))
					{
						m_scroll = Vector2.zero;
					}
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndArea();
		}

		private void OnStatePathGUI()
		{
			GUIContent rootContent = new GUIContent("Root");

			GUIStyle elementStyle = GUI.skin.GetStyle("FSM Pathbar Element");
			Vector2 elementSize = elementStyle.CalcSize(rootContent)
								+ new Vector2(4.0f, elementStyle.margin.vertical);

			Rect backgroundRect = new Rect(0.0f, ms_toolbarHeight, position.width, elementSize.y);
			GUI.BeginGroup(backgroundRect, EditorStyles.toolbar);
			{
				int selection = int.MinValue;

				Rect elementRect = new Rect(elementStyle.padding.left, 0.0f, elementSize.x, backgroundRect.height);
				bool wasSelected = m_editingPath.Count == 0;
				if(GUI.Toggle(elementRect, wasSelected, "Root", elementStyle) != wasSelected)
				{
					selection = -1;
				}

				for(int pathElementIndex = 0; pathElementIndex < m_editingPath.Count; ++pathElementIndex)
				{
					elementRect.x += elementRect.width;

                    /*SerializedObject stateObject = new SerializedObject(m_editingPath[pathElementIndex]);
                    stateObject.Update();
                    SerializedProperty componentHolderProperty = stateObject.FindProperty(ms_stateComponentHolderAttributeName);*/

					// element
                    //GUIContent elementContent = new GUIContent(componentHolderProperty.objectReferenceValue.name);
                    GUIContent elementContent = new GUIContent(m_editingPath[pathElementIndex].name);
					elementSize = elementStyle.CalcSize(elementContent);
					
					elementRect.width = elementSize.x + elementStyle.margin.left;
					wasSelected = pathElementIndex == m_editingPath.Count - 1;
					if(GUI.Toggle(elementRect, pathElementIndex == m_editingPath.Count - 1, elementContent, elementStyle) != wasSelected)
					{
						selection = pathElementIndex;
					}

					// separator
					Rect separatorRect = new Rect(elementRect);
					separatorRect.width = 10.0f;
					if(GUI.Toggle(separatorRect, false, GUIContent.none, "FSM Pathbar Separator"))
					{
						selection = pathElementIndex - 1;
					}
				}

				// separator
				elementRect.x += elementRect.width;
				Rect lastSeparatorRect = new Rect(elementRect);
				lastSeparatorRect.width = 10.0f;
				if(GUI.Toggle(lastSeparatorRect, true, GUIContent.none, "FSM Pathbar Separator") == false)
				{
					selection = m_editingPath.Count - 1;
				}

				if(selection >= -1)
				{
					while(m_editingPath.Count > selection + 1)
					{
						ExitFromState();
					}
				}
			}
			GUI.EndGroup();
		}
	#endregion

	#region Events
		private Rect StateEvents(int a_stateIndex, Rect a_rect)
		{
			switch(Event.current.type)
			{
				case EventType.MouseUp:
				{
					if(a_rect.Contains(Event.current.mousePosition))
					{
						if(m_transitionFromStateIndex != -1  &&  Event.current.button == 0  &&  m_transitionFromStateIndex != a_stateIndex)
						{
							AddTransition(m_transitionFromStateIndex, a_stateIndex);
							Event.current.Use();
						}
						else if(m_transitionToStateIndex != -1  &&  Event.current.button == 0  &&  m_transitionToStateIndex != a_stateIndex)
						{
							AddTransition(a_stateIndex, m_transitionToStateIndex);
							Event.current.Use();
						}
						
						if(m_isDraggingState == false  &&  m_isDraggingView == false  &&  Event.current.button == 1)
						{
							GenericMenu menu = new GenericMenu();					
							if(m_defaultStateProperty.intValue == a_stateIndex)
							{
								menu.AddDisabledItem(new GUIContent("Set as Default"));
							}
							else
							{
								menu.AddItem(new GUIContent("Set as Default"), false, SetStateAsDefault, a_stateIndex);
							}
							menu.AddItem(new GUIContent("Create transition from"), false, BeginCreateTransitionFrom, a_stateIndex);
							menu.AddItem(new GUIContent("Create transition to"), false, BeginCreateTransitionTo, a_stateIndex);
							
							menu.AddSeparator("");
							
							menu.AddItem(new GUIContent("Enter State"), false, EnterInState, a_stateIndex);
							
							menu.AddSeparator("");
							
							menu.AddItem(new GUIContent("Delete"), false, DeleteState, a_stateIndex);
							
							menu.ShowAsContext();

							Event.current.Use();
						}
						
						m_isDraggingState = false;
						m_transitionFromStateIndex = -1;
						m_transitionToStateIndex = -1;
					}
				}
				break;
				
				case EventType.MouseDown:
				{
					if(a_rect.Contains(Event.current.mousePosition))
					{
						bool use = false;
						if(selectedStateIndex != a_stateIndex)
						{
							selectedStateIndex = a_stateIndex;
							use = true;
						}
						
						if(Event.current.button == 0  &&  Event.current.clickCount >= 2)
						{
							EnterInState(a_stateIndex);
							use = true;
						}

						if(use)
						{
							Event.current.Use();
						}
					}
				}
				break;
				
				case EventType.MouseDrag:
				{
					if(Event.current.button == 0  &&  m_selectedStateIndex == a_stateIndex)
					{
						m_isDraggingState = true;

						a_rect.position += Event.current.delta;

						Event.current.Use();
					}
				}
				break;
			}

			return a_rect;
		}
	#endregion

		private int selectedStateIndex
		{
			get{ return m_selectedStateIndex; }
			set
			{
				if(m_selectedStateIndex != value)
				{
					m_selectedTransitionIndex = -1;
					m_selectedStateIndex = value;

					if(m_selectedStateIndex == -1)
					{
						Selection.activeObject = m_stateMachineObject.targetObject;
					}
					else
					{
                        Selection.activeObject = m_stateArrayProperty.GetArrayElementAtIndex(m_selectedStateIndex).objectReferenceValue;
                        /*SerializedObject stateObject = new SerializedObject(m_stateArrayProperty.GetArrayElementAtIndex(m_selectedStateIndex).objectReferenceValue);
                        stateObject.Update();
                        SerializedProperty componentHolderProperty = stateObject.FindProperty(ms_stateComponentHolderAttributeName);
                        Selection.activeObject = componentHolderProperty.objectReferenceValue;*/
					}
				}
			}
		}

		private int selectedTransitionIndex
		{
			get{ return m_selectedTransitionIndex; }
			set
			{
				if(m_selectedTransitionIndex != value)
				{
					m_selectedStateIndex = -1;
					m_selectedTransitionIndex = value;

					if(m_selectedTransitionIndex == -1)
					{
						Selection.activeObject = m_stateMachineObject.targetObject;
					}
					else
					{
						Selection.activeObject = m_transitionArrayProperty.GetArrayElementAtIndex(m_selectedTransitionIndex).objectReferenceValue;
					}
				}
			}
		}

		private SerializedObject stateMachineObject
		{
			get{ return m_stateMachineObject; }
			set
			{
				if(m_stateMachineObject != value)
				{
					m_stateMachineObject = value;


					if(m_stateMachineObject != null)
					{
						m_stateArrayProperty = m_stateMachineObject.FindProperty("m_states");
						m_transitionArrayProperty = m_stateMachineObject.FindProperty("m_transitions");
						m_defaultStateProperty = m_stateMachineObject.FindProperty("m_defaultStateIndex");

						Selection.activeObject = m_stateMachineObject.targetObject;
					}
					else
					{
						m_stateArrayProperty = null;
						m_transitionArrayProperty = null;
						m_defaultStateProperty = null;
					}
				}
			}
		}

		private void BeginCreateTransitionFrom(object a_stateIndex)
		{
			m_transitionFromStateIndex = (int)a_stateIndex;
			m_transitionToStateIndex = -1;
		}

		private void BeginCreateTransitionTo(object a_stateIndex)
		{
			m_transitionFromStateIndex = -1;
			m_transitionToStateIndex = (int)a_stateIndex;
		}

		private Vector2 CalculateStateSize(string a_stateName)
		{
			GUIContent stateNameContent = new GUIContent(a_stateName);
			GUIStyle nameStyle = GUI.skin.GetStyle("FSM State Name");
			Vector2 nameSize = nameStyle.CalcSize(stateNameContent);

			float width = Mathf.Max(ms_stateWindowInnerMargins.x + nameSize.x + nameStyle.padding.horizontal + nameStyle.margin.horizontal, ms_stateWindowSize.x);
			float height = Mathf.Max(ms_stateWindowInnerMargins.y + nameSize.y + nameStyle.padding.vertical + nameStyle.margin.vertical, ms_stateWindowSize.y);

			return new Vector2(width, height);
		}

	#region State Machine Modification Methods
		private void AddTransition(int a_stateFromIndex, int a_stateToIndex)
		{
			SerializedProperty stateFromProperty = m_stateArrayProperty.GetArrayElementAtIndex(a_stateFromIndex);
			SerializedProperty stateToProperty = m_stateArrayProperty.GetArrayElementAtIndex(a_stateToIndex);

			bool doesTransitionExist = false;
			int transitionCheckIndex = 0;
			while(doesTransitionExist == false  &&  transitionCheckIndex < m_transitionArrayProperty.arraySize)
			{
				SerializedProperty transitionProperty = m_transitionArrayProperty.GetArrayElementAtIndex(transitionCheckIndex);

				SerializedObject transitionObject = new SerializedObject(transitionProperty.objectReferenceValue);
				SerializedProperty checkStateFromProperty = transitionObject.FindProperty(ms_transitionStateFromAttributeName);
				SerializedProperty checkStateToProperty = transitionObject.FindProperty(ms_transitionStateToAttributeName);

				doesTransitionExist = checkStateFromProperty.objectReferenceValue == stateFromProperty.objectReferenceValue  &&  
										checkStateToProperty.objectReferenceValue == stateToProperty.objectReferenceValue;
				++transitionCheckIndex;
			}

			if(doesTransitionExist == false)
			{
				++m_transitionArrayProperty.arraySize;
				SerializedProperty newTransitionProperty = m_transitionArrayProperty.GetArrayElementAtIndex(m_transitionArrayProperty.arraySize - 1);

				// create the transition
				StateMachineTransition newTransition = ScriptableObject.CreateInstance<StateMachineTransition>();
				newTransition.hideFlags = HideFlags.HideInHierarchy;
				AssetDatabase.AddObjectToAsset(newTransition, m_stateMachineObject.targetObject);
				newTransitionProperty.objectReferenceValue = newTransition;

				// fill it
				SerializedObject newTransitionObject = new SerializedObject(newTransition);
				SerializedProperty transitionFromProperty = newTransitionObject.FindProperty(ms_transitionStateFromAttributeName);
				SerializedProperty transitionToProperty = newTransitionObject.FindProperty(ms_transitionStateToAttributeName);
				transitionFromProperty.objectReferenceValue = stateFromProperty.objectReferenceValue;
				transitionToProperty.objectReferenceValue = stateToProperty.objectReferenceValue;
				newTransitionObject.ApplyModifiedProperties();

				// set it into the state from
				SerializedObject stateFromObject = new SerializedObject(stateFromProperty.objectReferenceValue);
				SerializedProperty stateFromNextTransitionArrayProperty = stateFromObject.FindProperty(ms_stateNextTransitionsAttributeName);
				++stateFromNextTransitionArrayProperty.arraySize;
				SerializedProperty stateFromNewNextTransition = stateFromNextTransitionArrayProperty.GetArrayElementAtIndex(stateFromNextTransitionArrayProperty.arraySize - 1);
				stateFromNewNextTransition.objectReferenceValue = newTransition;
				stateFromObject.ApplyModifiedProperties();

				// apply modification on parent asset
				m_stateMachineObject.ApplyModifiedProperties();
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_stateMachineObject.targetObject));
			}
		}

		private void DeleteTransition(object a_transitionIndex)
		{
			if(selectedTransitionIndex == (int)a_transitionIndex)
			{
				selectedTransitionIndex = -1;
			}

			SerializedProperty oldTransitionProperty = m_transitionArrayProperty.GetArrayElementAtIndex((int)a_transitionIndex);

			SerializedObject oldTransitionObject = new SerializedObject(oldTransitionProperty.objectReferenceValue);
			SerializedProperty stateFromProperty = oldTransitionObject.FindProperty(ms_transitionStateFromAttributeName);

			// remove the property from the state from
			SerializedObject stateFromObject = new SerializedObject(stateFromProperty.objectReferenceValue);
			SerializedProperty stateNextTransitionArrayProperty = stateFromObject.FindProperty(ms_stateNextTransitionsAttributeName);
			int stateNextTransitionIndex = 0;
			while(stateNextTransitionIndex < stateNextTransitionArrayProperty.arraySize  &&  stateNextTransitionArrayProperty.GetArrayElementAtIndex(stateNextTransitionIndex).objectReferenceValue != oldTransitionProperty.objectReferenceValue)
			{
				++stateNextTransitionIndex;
			}

			if(stateNextTransitionIndex == stateNextTransitionArrayProperty.arraySize)
			{
				Debug.LogError("Error deleting the transition from state '" + stateFromProperty.objectReferenceValue.name + "'.");
				return;
			}
			stateNextTransitionArrayProperty.GetArrayElementAtIndex(stateNextTransitionIndex).objectReferenceValue = null;
			stateNextTransitionArrayProperty.DeleteArrayElementAtIndex(stateNextTransitionIndex);
			stateFromObject.ApplyModifiedProperties();

			StateMachineTransition oldTransition = oldTransitionProperty.objectReferenceValue as StateMachineTransition;
			oldTransitionProperty.objectReferenceValue = null;

			m_transitionArrayProperty.DeleteArrayElementAtIndex((int)a_transitionIndex);
			StateMachineState.DestroyImmediate(oldTransition, true);

			m_stateMachineObject.ApplyModifiedProperties();
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_stateMachineObject.targetObject));
		}

		private void AddState(object a_position)
		{
			++m_stateArrayProperty.arraySize;			
			SerializedProperty newStateProperty = m_stateArrayProperty.GetArrayElementAtIndex(m_stateArrayProperty.arraySize - 1);

			// create the state
			StateMachineState newState = ScriptableObject.CreateInstance<StateMachineState>();
			newState.hideFlags = HideFlags.HideInHierarchy;
			AssetDatabase.AddObjectToAsset(newState, m_stateMachineObject.targetObject);
			newStateProperty.objectReferenceValue = newState;

			/*// create the prefab object
			GameObject componentHolderPrefab = new GameObject(newState.name);
            GameObject prefab = PrefabUtility.CreatePrefab("Assets/temp.prefab", componentHolderPrefab);
            prefab.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(prefab, newState);*/

			// fill the state fields
			SerializedObject newStateObject = new SerializedObject(newState);
			SerializedProperty positionProperty = newStateObject.FindProperty(ms_statePositionAttributeName);
			positionProperty.vector2Value = (Vector2)a_position;
			//SerializedProperty componentHolderProperty = newStateObject.FindProperty(ms_stateComponentHolderAttributeName);
			//componentHolderProperty.objectReferenceValue = componentHolderPrefab;
			newStateObject.ApplyModifiedProperties();
			
			m_stateMachineObject.ApplyModifiedProperties();
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_stateMachineObject.targetObject));
		}

		private void DeleteState(object a_stateIndex)
		{
			if(selectedStateIndex == (int)a_stateIndex)
			{
				selectedStateIndex = -1;
			}

			if(m_defaultStateProperty.intValue == (int)a_stateIndex)
			{
				m_defaultStateProperty.intValue = 0;
			}

			SerializedProperty oldStateProperty = m_stateArrayProperty.GetArrayElementAtIndex((int)a_stateIndex);
			StateMachineState oldState = oldStateProperty.objectReferenceValue as StateMachineState;
			oldStateProperty.objectReferenceValue = null;

			// delete all transition from and to this state
			int transitionIndex = 0;
			while(transitionIndex < m_transitionArrayProperty.arraySize)
			{
				SerializedProperty transitionProperty = m_transitionArrayProperty.GetArrayElementAtIndex(transitionIndex);

				SerializedObject transitionObject = new SerializedObject(transitionProperty.objectReferenceValue);
				SerializedProperty stateFromProperty = transitionObject.FindProperty(ms_transitionStateFromAttributeName);
				SerializedProperty stateToProperty = transitionObject.FindProperty(ms_transitionStateToAttributeName);

				if(stateFromProperty.objectReferenceValue == oldState  ||  stateToProperty.objectReferenceValue == oldState)
				{
					DeleteTransition(transitionIndex);
				}
				else
				{
					++transitionIndex;
				}
			}

			m_stateArrayProperty.DeleteArrayElementAtIndex((int)a_stateIndex);
			StateMachineState.DestroyImmediate(oldState, true);

			m_stateMachineObject.ApplyModifiedProperties();
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_stateMachineObject.targetObject));
		}

		private void SetStateAsDefault(object a_stateIndex)
		{
			m_defaultStateProperty.intValue = (int)a_stateIndex;
			m_stateMachineObject.ApplyModifiedProperties();
		}
	#endregion

		private void EnterInState(object a_stateIndex)
		{
			SerializedProperty stateProperty = m_stateArrayProperty.GetArrayElementAtIndex((int)a_stateIndex);
			stateMachineObject = new SerializedObject(stateProperty.objectReferenceValue);

            StateMachineState test = stateProperty.objectReferenceValue as StateMachineState;
			m_editingPath.Add(test);

			// reset values
			selectedStateIndex = -1;
			selectedTransitionIndex = -1;
			
			m_isDraggingView = false;
			m_scroll = Vector2.zero;
			
			m_isDraggingState = false;
			m_transitionFromStateIndex = -1;
			m_transitionToStateIndex = -1;

            m_scroll = Vector3.zero;

			Repaint();
		}

		private void ExitFromState()
		{
			if(m_editingPath.Count > 0)
			{
				m_editingPath.RemoveAt(m_editingPath.Count - 1);

				if(m_editingPath.Count > 0)
				{
					stateMachineObject = new SerializedObject(m_editingPath[m_editingPath.Count - 1]);
				}
				else
				{
					stateMachineObject = m_rootStateMachineObject;
				}

				// reset values
				selectedStateIndex = -1;
				selectedTransitionIndex = -1;
				
				m_isDraggingView = false;
				m_scroll = Vector2.zero;
				
				m_isDraggingState = false;
				m_transitionFromStateIndex = -1;
				m_transitionToStateIndex = -1;

                m_scroll = Vector3.zero;

				Repaint();
			}
		}

	#region Attributes
		//! serialized object and properties
		private List<StateMachineState> m_editingPath;

		private SerializedObject m_rootStateMachineObject;

		private SerializedObject m_stateMachineObject;
		private SerializedProperty m_stateArrayProperty;
		private SerializedProperty m_transitionArrayProperty;
		private SerializedProperty m_defaultStateProperty;

		//! selection
		private int m_selectedStateIndex;
		private int m_selectedTransitionIndex;

		//! gui variables
		private bool m_isDraggingView;
		private Vector2 m_scroll;

		private bool m_isDraggingState;
		private int m_transitionFromStateIndex;
		private int m_transitionToStateIndex;

		#region constants
		private static float ms_toolbarHeight = 15.0f;

		private static Vector2 ms_stateWindowSize = new Vector2(80.0f, 40.0f);
		private static Vector2 ms_stateWindowInnerMargins = new Vector2(3.0f, 3.0f);

		private static Vector2 ms_transitionArrowHeadSize = new Vector2(38.0f, 32.0f);
		private static float ms_transitionArrowSize = 32.0f;

		private static string ms_statePositionAttributeName = "m_position";
		private static string ms_stateNextTransitionsAttributeName = "m_nextTransitions";
		//private static string ms_stateComponentHolderAttributeName = "m_componentHolder";

		private static string ms_transitionStateFromAttributeName = "m_stateFrom";
		private static string ms_transitionStateToAttributeName = "m_stateTo";
		#endregion
	#endregion
#endregion
	}
}