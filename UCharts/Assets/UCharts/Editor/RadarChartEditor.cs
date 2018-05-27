using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UI;
using UnityEditor;
using UnityEditorInternal;

namespace UCharts
{
	[CustomEditor(typeof(RadarChart), true)]
    [CanEditMultipleObjects]
	public class RadarChartEditor : GraphicEditor
    {

		SerializedProperty  m_Sides, m_Color0, m_Color1, m_BorderColor;
		
		ReorderableList m_Indicators, m_Data;
		protected override void OnEnable()
		{
			base.OnEnable();
			m_Sides            = serializedObject.FindProperty("m_Sides");
			m_Color0           = serializedObject.FindProperty("m_Color0");
			m_Color1           = serializedObject.FindProperty("m_Color1");
			m_BorderColor      = serializedObject.FindProperty("m_BorderColor");

			m_Indicators       = new ReorderableList(serializedObject, 
									serializedObject.FindProperty("m_Indicators"),
									true,
									true,
									true,
									true);

			m_Data       = new ReorderableList(serializedObject, 
									serializedObject.FindProperty("m_Data"),
									true,
									true,
									true,
									true);


			m_Indicators.drawElementCallback =  (Rect rect, int index, bool isActive, bool isFocused) => {
				var element = m_Indicators.serializedProperty.GetArrayElementAtIndex(index);
				rect.y += 2;
				EditorGUI.LabelField(new Rect(rect.x, rect.y, 30, EditorGUIUtility.singleLineHeight),"Text");
				EditorGUI.PropertyField(
					new Rect(rect.x + 30, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight),
					element.FindPropertyRelative("Text"), GUIContent.none);

				EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.6f, rect.y, 30, EditorGUIUtility.singleLineHeight),"Max");
				EditorGUI.PropertyField(
					new Rect(rect.x + rect.width * 0.6f + 30, rect.y, rect.width*0.4f - 30, EditorGUIUtility.singleLineHeight),
					element.FindPropertyRelative("MaxValue"), GUIContent.none);
			};

			m_Data.drawElementCallback =  (Rect rect, int index, bool isActive, bool isFocused) => {
				var element = m_Data.serializedProperty.GetArrayElementAtIndex(index);
				rect.y += 2;

				EditorGUI.PropertyField(
					new Rect(rect.x, rect.y, rect.width*0.4f - 30, EditorGUIUtility.singleLineHeight),
					element, GUIContent.none);
			};

			m_Indicators.drawHeaderCallback = (Rect rect) => {  
				EditorGUI.LabelField(rect, "Indicators");
			};

			m_Data.drawHeaderCallback = (Rect rect) => {  
				EditorGUI.LabelField(rect, "Data");
			};
		}

		public override void OnInspectorGUI()
        {
            serializedObject.Update();

			EditorGUILayout.PropertyField(m_Sides);
			m_Indicators.DoLayoutList();
			m_Data.DoLayoutList();
			
			EditorGUILayout.PropertyField(m_Color0);
			EditorGUILayout.PropertyField(m_Color1);
			EditorGUILayout.PropertyField(m_BorderColor);

			var sides = m_Sides.intValue;

            RaycastControlsGUI();
            NativeSizeButtonGUI();

            serializedObject.ApplyModifiedProperties();
        }
	}
}

