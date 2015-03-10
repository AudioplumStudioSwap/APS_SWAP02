using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Aube.Sandbox
{
	[AddComponentMenu("Sandbox/Main Menu GUI")]

	//! @class MainMenuGUI
	//!
	//! @brief list all examples and allow to start one or another
	public class MainMenuGUI : MonoBehaviour
	{
		[Header("Folder Gui Styles")]
		[SerializeField]
		private GUIStyle m_normalFolderStyle;
		[SerializeField]
		private GUIStyle m_selectedFolderStyle;
		[SerializeField]
		private GUIStyle m_activeSelectionFolderStyle;

		[Header("Item Gui Styles")]
		[SerializeField]
		private GUIStyle m_normalItemStyle;
		[SerializeField]
		private GUIStyle m_activeSelectionItemStyle;

#region Unity Callbacks
		private void Awake()
		{
			// get examples
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
			System.Type[] types = assembly.GetTypes().Where(type => string.Equals(type.Namespace, "Aube.Sandbox", System.StringComparison.Ordinal)
			                                                &&  typeof(MonoBehaviour).IsAssignableFrom(type)).ToArray();

			List<Example> examples = new List<Example>(types.Length);
			foreach(System.Type type in types)
			{
				Example[] exampleAttributes = System.Array.ConvertAll(type.GetCustomAttributes(typeof(Example), false), item => item as Example);
				foreach(Example example in exampleAttributes)
				{
					example.type = type;
				}
				examples.AddRange(exampleAttributes);
			}

			m_examples = examples.ToArray();

			// sort examples with path
			m_guiSortedElements = new List<GuiElement>();
			GuiFolder exampleFolder = GetOrAddFolder(ref m_guiSortedElements, "Examples");
			exampleFolder.children = new List<GuiElement>();
			GuiLeaf sandboxLeaf = GetOrAddLeaf(ref m_guiSortedElements, "Sandbox");
			sandboxLeaf.exampleIndex = -1;

			for(int exampleIndex = 0; exampleIndex < m_examples.Length; ++exampleIndex)
			{
				string[] pathElements = m_examples[exampleIndex].path.Split(ms_guiSeparators, System.StringSplitOptions.RemoveEmptyEntries);
				List<GuiElement> currentElements = exampleFolder.children;
				for(int elementIndex = 0; elementIndex < pathElements.Length - 1; ++elementIndex)
				{
					GuiFolder folder = GetOrAddFolder(ref currentElements, pathElements[elementIndex]);
					if(folder.children == null)
					{
						folder.children = new List<GuiElement>();
					}
					currentElements = folder.children;
				}

				GuiLeaf leaf = GetOrAddLeaf(ref currentElements, pathElements[pathElements.Length - 1]);
				leaf.exampleIndex = exampleIndex;
			}

			m_guiCurrentSelection = new List<int>();
			m_guiCurrentSelection.Add(0);

			useGUILayout = false;
		}

		private void Update()
		{
			UpdateInputs();
			ProcessAction();
		}

		private void OnGUI()
		{
			Vector2 nextOffsets = Vector2.zero;
			List<GuiElement> currentElements = null;
			for(int selectionIndex = 0; selectionIndex < m_guiCurrentSelection.Count; ++selectionIndex)
			{
				currentElements = (currentElements == null)? m_guiSortedElements : (currentElements[m_guiCurrentSelection[selectionIndex - 1]] as GuiFolder).children;

				int newSelection = OnGUI(nextOffsets, currentElements.ToArray(), m_guiCurrentSelection[selectionIndex], m_guiCurrentSelection.Count == selectionIndex + 1, out nextOffsets);
				HandleGuiSelection(newSelection, selectionIndex);
			}

			List<GuiElement> parentList;
			GuiFolder selectedFolder = GetSelection(out parentList) as GuiFolder;
			if(selectedFolder != null)
			{
				int newSelection = OnGUI(nextOffsets, selectedFolder.children.ToArray(), -1, false, out nextOffsets);
				HandleGuiSelection(newSelection, m_guiCurrentSelection.Count);
			}
		}

		private GuiElement GetSelection(out List<GuiElement> a_parentList)
		{
			a_parentList = null;
			GuiElement currentElement = null;
			for(int selectionIndex = 0; selectionIndex < m_guiCurrentSelection.Count; ++selectionIndex)
			{
				if(currentElement == null)
				{
					a_parentList = m_guiSortedElements;
				}
				else
				{
					a_parentList = (currentElement as GuiFolder).children;
				}
				currentElement = a_parentList[m_guiCurrentSelection[selectionIndex]];
			}

			return currentElement;
		}
#endregion

#region Private
		private class GuiElement
		{
			public string label;
		}

		private class GuiLeaf : GuiElement
		{
			public int exampleIndex;
		}

		private class GuiFolder : GuiElement
		{
			public List<GuiElement> children;
		}

		private static char[] ms_guiSeparators = new char[]{ '/', '\\', '|' };

		private static GuiFolder GetOrAddFolder(ref List<GuiElement> a_elements, string a_name)
		{
			int elementIndex = 0;
			while(elementIndex < a_elements.Count
			      &&  a_elements[elementIndex] is GuiFolder
			      &&  string.Compare(a_elements[elementIndex].label, a_name) < 0)
			{
				++elementIndex;
			}

			if(elementIndex == a_elements.Count  ||  a_elements[elementIndex] is GuiLeaf  ||  string.Compare(a_elements[elementIndex].label, a_name) > 0)
			{
				GuiFolder folder = new GuiFolder();
				folder.label = a_name;
				a_elements.Insert(elementIndex, folder);
				return folder;
			}
			else/* if(a_elements[elementIndex] is GuiFolder  &&  a_elements[elementIndex].label == a_name)*/
			{
				return a_elements[elementIndex] as GuiFolder;
			}
		}

		private static GuiLeaf GetOrAddLeaf(ref List<GuiElement> a_elements, string a_name)
		{
			int elementIndex = 0;
			while(elementIndex < a_elements.Count
			      &&  (a_elements[elementIndex] is GuiFolder  ||  string.Compare(a_elements[elementIndex].label, a_name) < 0))
			{
				++elementIndex;
			}

			if(elementIndex == a_elements.Count  ||  string.Compare(a_elements[elementIndex].label, a_name) > 0)
			{
				GuiLeaf leaf = new GuiLeaf();
				leaf.label = a_name;
				a_elements.Insert(elementIndex, leaf);
				return leaf;
			}
			else/* if(a_elements[elementIndex] is GuiFolder  &&  a_elements[elementIndex].label == a_name)*/
			{
				return a_elements[elementIndex] as GuiLeaf;
			}
		}

		private int OnGUI(Vector2 a_offsets, GuiElement[] a_elements, int a_selectionIndex, bool a_last, out Vector2 a_nextOffsets)
		{
			int newSelection = -1;

			GUIContent[] contents = new GUIContent[a_elements.Length];
			for(int contentIndex = 0; contentIndex < a_elements.Length; ++contentIndex)
			{
				contents[contentIndex] = new GUIContent(a_elements[contentIndex].label);
			}

			float maxWidth = 0.0f;
			float maxWidthWithMargins = 0.0f;
			float[] maxHeights = new float[contents.Length];
			for(int contentIndex = 0; contentIndex < contents.Length; ++contentIndex)
			{
				bool isFolder = a_elements[contentIndex] is GuiFolder;
				GUIStyle normalStyle = isFolder? m_normalFolderStyle : m_normalItemStyle;
				GUIStyle selectedStyle = isFolder? m_selectedFolderStyle : null;
				GUIStyle activeStyle = isFolder? m_activeSelectionFolderStyle : m_activeSelectionItemStyle;

				Vector2 normalSize = normalStyle.CalcSize(contents[contentIndex]);
				Vector2 selectedSize = selectedStyle == null? Vector2.zero : selectedStyle.CalcSize(contents[contentIndex]);
				Vector2 activeSize = activeStyle.CalcSize(contents[contentIndex]);

				int normalHMargins = normalStyle.margin.horizontal;
				int selectedHMargins = selectedStyle == null? 0 : selectedStyle.margin.horizontal;
				int activeHMargins = activeStyle.margin.horizontal;

				maxWidth = Mathf.Max(maxWidth, normalSize.x, selectedSize.x, activeSize.x);
				maxWidthWithMargins = Mathf.Max(maxWidthWithMargins, normalSize.x + normalHMargins, selectedSize.x + selectedHMargins, activeSize.x + activeHMargins);
				maxHeights[contentIndex] = Mathf.Max(normalSize.y, selectedSize.y, activeSize.y);
			}

			a_nextOffsets = a_offsets + Vector2.right * maxWidthWithMargins;

			float offsetY = 0.0f;
			for(int contentIndex = 0; contentIndex < contents.Length; ++contentIndex)
			{
				bool isFolder = a_elements[contentIndex] is GuiFolder;
				GUIStyle normalStyle = isFolder? m_normalFolderStyle : m_normalItemStyle;
				GUIStyle selectedStyle = isFolder? m_selectedFolderStyle : null;
				GUIStyle activeStyle = isFolder? m_activeSelectionFolderStyle : m_activeSelectionItemStyle;
				GUIStyle style = (contentIndex == a_selectionIndex)? (a_last? activeStyle : selectedStyle) : normalStyle;

				Rect contentRect = new Rect(a_offsets.x + style.margin.left, a_offsets.y + offsetY + style.margin.top, maxWidth, maxHeights[contentIndex]);
				if(contentIndex == a_selectionIndex)
				{
					a_nextOffsets = new Vector2(a_nextOffsets.x, a_offsets.y + offsetY);
				}

				if(GUI.Button(contentRect, contents[contentIndex], style))
				{
					newSelection = contentIndex;
				}
				offsetY += contentRect.height + style.margin.vertical;
			}

			return newSelection;
		}

		private void HandleGuiSelection(int a_newSelection, int a_selectionIndex)
		{
			if(a_newSelection >= 0  &&  m_action == null)
			{
				m_action = new Action();
				m_action.id = Action.ID.GuiSelect;
				m_action.guiSelectionParam = new List<int>(a_selectionIndex);

				for(int index = 0; index < a_selectionIndex; ++index)
				{
					m_action.guiSelectionParam.Add(m_guiCurrentSelection[index]);
				}
				m_action.guiSelectionParam.Add(a_newSelection);
			}
		}

		private void UpdateInputs()
		{
			if(m_action == null)
			{
				if(Input.GetKeyDown(KeyCode.Return))
				{
					m_action = new Action();
					m_action.id = Action.ID.Select;
				}
				else if(Input.GetKeyDown(KeyCode.LeftArrow))
				{
					m_action = new Action();
					m_action.id = Action.ID.ToParent;
				}
				else if(Input.GetKeyDown(KeyCode.RightArrow))
				{
					m_action = new Action();
					m_action.id = Action.ID.ToChild;
				}
				else if(Input.GetKeyDown(KeyCode.DownArrow))
				{
					m_action = new Action();
					m_action.id = Action.ID.ToNextSibling;
				}
				else if(Input.GetKeyDown(KeyCode.UpArrow))
				{
					m_action = new Action();
					m_action.id = Action.ID.ToPreviousSibling;
				}
			}
		}

		private void ProcessAction()
		{
			if(m_action != null)
			{
				List<GuiElement> parentList;
				GuiElement currentSelection = GetSelection(out parentList);

				switch(m_action.id)
				{
					case Action.ID.GuiSelect: m_guiCurrentSelection = m_action.guiSelectionParam; break;

					case Action.ID.Select:
					{
						if(currentSelection is GuiLeaf)
						{
							GuiLeaf leaf = currentSelection as GuiLeaf;
							if(leaf.exampleIndex == -1)
							{
								LoadingManager.LoadLevel("sandbox", null, "LoadingScreen");
							}
							else
							{
								ExampleLoader.currentExample = m_examples[leaf.exampleIndex];
								LoadingManager.LoadLevel("example", null, "LoadingScreen");
							}
						}
					}
					break;
					case Action.ID.ToParent:
					{
						if(m_guiCurrentSelection.Count > 1)
						{
							m_guiCurrentSelection.RemoveAt(m_guiCurrentSelection.Count - 1);
						}
					}
					break;
					case Action.ID.ToChild:
					{
						if(currentSelection is GuiFolder)
						{
							m_guiCurrentSelection.Add(0);
						}
					}
					break;
					case Action.ID.ToNextSibling:
					{
						m_guiCurrentSelection[m_guiCurrentSelection.Count - 1] = (m_guiCurrentSelection[m_guiCurrentSelection.Count - 1] + 1) % parentList.Count;
					}
					break;
					case Action.ID.ToPreviousSibling:
					{
						m_guiCurrentSelection[m_guiCurrentSelection.Count - 1] = (m_guiCurrentSelection[m_guiCurrentSelection.Count - 1] + parentList.Count - 1) % parentList.Count;
					}
					break;
				}

				m_action = null;			
			}
		}

		private Example[] m_examples;
		private List<GuiElement> m_guiSortedElements;
		private List<int> m_guiCurrentSelection;

		private class Action
		{
			public enum ID
			{
				GuiSelect,

				Select,
				ToParent,
				ToChild,
				ToNextSibling,
				ToPreviousSibling,
			}

			public ID id;
			public List<int> guiSelectionParam;
		}
		private Action m_action = null;
#endregion
	}
}