using UnityEngine;
using System.Collections;

namespace Aube.Sandbox
{
	[Example("Game/Switcher/Default")]
	[AddComponentMenu("")]

	//! @class SwitcherComponentExample
	//!
	//! @brief Example of a Switcher component
	class SwitcherComponentExample : MonoBehaviour
	{
		public float value
		{
			get{ return m_value; }
		}

#region Unity Callbacks
		private void OnGUI()
		{
			GUILayout.BeginHorizontal();
			{
				m_value = GUILayout.HorizontalSlider(m_value, -100.0f, 100.0f, GUILayout.Width(200.0f));
				GUILayout.Label(m_value.ToString());
			}
			GUILayout.EndHorizontal();
		}

		private void Start()
		{
			Font font = Resources.Load<Font>("font");

			// desc
			GameObject[] objects = new GameObject[3];
			PrimitiveType[] primitives = new PrimitiveType[objects.Length];
			Vector3[] positions = new Vector3[objects.Length];
			float[] minValues = new float[objects.Length];
			float[] maxValues = new float[objects.Length];

			primitives[0] = PrimitiveType.Cube; 		positions[0] = new Vector3(-5.0f, 0.0f, 0.0f);	minValues[0] = -100.0f; maxValues[0] = 10.0f;
			primitives[1] = PrimitiveType.Sphere; 		positions[1] = new Vector3(0.0f, 0.0f, 0.0f); 	minValues[1] = -55.0f; 	maxValues[1] = 55.0f;
			primitives[2] = PrimitiveType.Cylinder; 	positions[2] = new Vector3(5.0f, 0.0f, 0.0f); 	minValues[2] = -10.0f; 	maxValues[2] = 100.0f;

			for(int objectIndex = 0; objectIndex < objects.Length; ++objectIndex)
			{
				objects[objectIndex] = GameObject.CreatePrimitive(primitives[objectIndex]);
				objects[objectIndex].transform.position = positions[objectIndex];
				objects[objectIndex].transform.parent = transform;

				GameObject cubeTextObject = new GameObject("label");
				cubeTextObject.transform.parent = objects[objectIndex].transform;
				cubeTextObject.transform.localPosition = Vector3.up * 2;

				TextMesh textMesh = cubeTextObject.AddComponent<TextMesh>();
				textMesh.font = font;
				textMesh.renderer.sharedMaterial = font.material;
				textMesh.fontSize = 16;
				textMesh.characterSize = 0.5f;
				textMesh.richText = false;
				textMesh.anchor = TextAnchor.UpperCenter;
				textMesh.text = "[" + minValues[objectIndex] + " ; " + maxValues[objectIndex] + "]";
			}
			
			GameObject switcher = new GameObject("Switcher");
			switcher.transform.parent = transform;
			Aube.Switcher switcherComponent = switcher.AddComponent<Aube.Switcher>();
			for(int objectIndex = 0; objectIndex < objects.Length; ++objectIndex)
			{
				switcherComponent.AddDomain(objects[objectIndex], true, minValues[objectIndex], maxValues[objectIndex]);
			}
			
			switcherComponent.Set(this, "value", Switcher.PropertyType.Float);
		}

		private void Update()
		{
			if(Input.GetKey(KeyCode.LeftArrow))
			{
				m_value -= 10.0f * Time.deltaTime;
			}
			if(Input.GetKey(KeyCode.RightArrow))
			{
				m_value += 10.0f * Time.deltaTime;
			}

			m_value = Mathf.Clamp(m_value, -100.0f, 100.0f);
		}
#endregion

#region Private
		private float m_value;
#endregion
	}
}
