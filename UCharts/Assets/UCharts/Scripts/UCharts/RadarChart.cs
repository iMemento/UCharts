using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

namespace UCharts
{
	public class RadarChart : ChartBase, IPointerDownHandler, IDragHandler, IEndDragHandler
	{
        [SerializeField] private float m_Thickness = 5;
        
		[Range(3, 36)]
        [SerializeField] private int m_Sides = 3;
        
		[Range(0, 360)]
        private float m_Rotatioin = -90f;
        private float m_baseRotation = 0f;
		[SerializeField] private float m_Size = 0;

		[SerializeField] private Color32 m_Color0, m_Color1, m_BorderColor;

		[SerializeField] List<RadarChartIndicator> m_Indicators = new List<RadarChartIndicator>();
		[SerializeField] List<float> m_Data = new List<float>();
	    private GameObject m_TextProto;
		private bool m_PlayAnimation;
		private float m_PlayAnimationTimestamp;
		private List<float> m_DataDisplay = new List<float>();
		private Vector2 center;
		protected override void Awake()
		{
			base.Awake();
			
			m_Size = rectTransform.rect.width;
            if (rectTransform.rect.width > rectTransform.rect.height)
                m_Size = rectTransform.rect.height;
            else
                m_Size = rectTransform.rect.width;

		    m_TextProto = transform.Find("Text").gameObject;
			center.x = (-rectTransform.pivot.x + 0.5f) * m_Size;
			center.y = (-rectTransform.pivot.y + 0.5f) * m_Size;
			
			PlayAnimation();
            DrawIndicatorLabels();
        }

        void Update()
        {
            m_Size = rectTransform.rect.width;
            if (rectTransform.rect.width > rectTransform.rect.height)
                m_Size = rectTransform.rect.height;
            else
                m_Size = rectTransform.rect.width;
            m_Thickness = (float)Mathf.Clamp(m_Thickness, 0, m_Size / 2);

			if (m_PlayAnimation)
			{
				for (var i = 0; i < m_DataDisplay.Count; ++i)
				{
					m_DataDisplay[i] += Mathf.Lerp(0, m_Data[i], Time.deltaTime);
				}
				if (Time.time - m_PlayAnimationTimestamp > 1f) m_PlayAnimation = false;
				SetVerticesDirty();
			}
        }

  		protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
			DrawRadarView(vh);
			DrawDataPolygon(vh);
        }
		void PlayAnimation()
		{
			if (Application.isPlaying) 
			{
				if (m_DataDisplay.Count != m_Data.Count)
				{
					m_DataDisplay.Clear();
					for (var i = 0; i < m_Data.Count; ++i) m_DataDisplay.Add(0);
				}
				else
				{
					for (var i = 0; i < m_DataDisplay.Count; ++i) m_DataDisplay[i] = 0;
				}
				
				m_PlayAnimation = true;
				m_PlayAnimationTimestamp = Time.time;
			}
		}

	    void DrawIndicatorLabels()
	    {
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return;
			}
#endif
			if (m_TextProto == null)
			{
				Debug.LogWarning("Text prefab is needed!");
				return;
			}
            var degrees = 360f / m_Sides;

            for (var i = 0; i < m_Indicators.Count && i < m_Sides; i++)
	        {
                var rad = Mathf.Deg2Rad * (i * degrees + m_Rotatioin);
                var c = Mathf.Cos(rad);
                var s = Mathf.Sin(rad);

	            var trans = transform.Find("Text" + i);
                if (trans == null)
	            {
                    trans = Instantiate(m_TextProto).transform;
                    trans.SetParent(transform, false);
	                trans.name = "Text" + i;
	            }
                var text = trans.GetComponent<Text>();
                text.text = m_Indicators[i].Text;
				var outer = -0.5f * (m_Size + text.preferredWidth);
                var pos = new Vector2(outer * c, outer * s);

	            text.rectTransform.localPosition = pos + center;
	        }

        }

		private void DrawRadarView(VertexHelper vh)
		{
			var size = m_Size;
			center.x = (-rectTransform.pivot.x + 0.5f) * m_Size;
			center.y = (-rectTransform.pivot.y + 0.5f) * m_Size;

			for (var p = 0; p < 5; ++p) 
			{
				size = m_Size - p * m_Size / 5;
				Vector2 prevX = center;
				Vector2 prevY = center;
				Vector2 uv0 = new Vector2(0, 0);
				Vector2 uv1 = new Vector2(0, 1);
				Vector2 uv2 = new Vector2(1, 1);
				Vector2 uv3 = new Vector2(1, 0);
				Vector2 pos0;
				Vector2 pos1;
				Vector2 pos2;
				Vector2 pos3;
				float degrees = 360f / m_Sides;
				int vertices = m_Sides + 1;
				
				for (int i = 0; i < vertices; i++)
				{
					float outer = -0.5f * size;
					float inner = -0.5f * size  + m_Thickness;

					float rad = Mathf.Deg2Rad * (i * degrees + m_Rotatioin);
					float c = Mathf.Cos(rad);
					float s = Mathf.Sin(rad);
					uv0 = new Vector2(0, 1);
					uv1 = new Vector2(1, 1);
					uv2 = new Vector2(1, 0);
					uv3 = new Vector2(0, 0);
					pos0 = prevX;
					pos1 = new Vector2(outer * c, outer * s) + center;

					pos2 = new Vector2(inner * c, inner * s) + center;
					pos3 = prevY;
				
					prevX = pos1;
					prevY = pos2;

					// draw fill color
					vh.AddUIVertexQuad(
						SetVbo(new[] { pos0, pos1, center, center }, 
							   new[] { uv0, uv1, uv2, uv3 }, p % 2 == 0 ? m_Color0 : m_Color1)
					);

					// draw borders
					vh.AddUIVertexQuad(
						SetVbo(new[] { pos0, pos1, pos2, pos3 }, 
							   new[] { uv0, uv1, uv2, uv3 }, 
							   m_BorderColor)
					);

					// draw lines
					if (p == 4)
					{
						var pos = (pos1 - center) * m_Size / size;
						Vector2 vector = (Quaternion.Euler(0f, 0f, 90f) * pos).normalized;
						
						vh.AddUIVertexQuad(
						SetVbo(new[] { 
									  center - m_Thickness * 0.5f * vector, 
									  pos - m_Thickness * 0.5f * vector + center, 
									  pos + m_Thickness * 0.5f * vector + center, 
									  center + m_Thickness * 0.5f * vector },
							   new[] { uv0, uv1, uv2, uv3 }, 
							   m_BorderColor)
						);
					}
				}
			}
		}

		private void DrawDataPolygon(VertexHelper vh)
		{
			var vertices = m_Sides + 1;
			var size = m_Size;
			Vector2 prevX = center;
			Vector2 prevY = center;
			Vector2 uv0 = new Vector2(0, 0);
			Vector2 uv1 = new Vector2(0, 1);
			Vector2 uv2 = new Vector2(1, 1);
			Vector2 uv3 = new Vector2(1, 0);
			Vector2 pos0;
			Vector2 pos1, pos2, pos3;
			float degrees = 360f / m_Sides;

			for (int i = 0; i < vertices; i++)
			{
				float outer = -0.5f * size;
				float inner = -0.5f * size  + m_Thickness;

				float rad = Mathf.Deg2Rad * (i * degrees + m_Rotatioin);
				float c = Mathf.Cos(rad);
				float s = Mathf.Sin(rad);
				uv0 = new Vector2(0, 1);
				uv1 = new Vector2(1, 1);
				uv2 = new Vector2(1, 0);
				uv3 = new Vector2(0, 0);
				pos0 = prevX;
				var index = i % (vertices - 1);
				var value = index < m_Data.Count - 1 ? m_Data[index] : 0;

				if (m_PlayAnimation)
				{
					value = index < m_DataDisplay.Count - 1 ? m_DataDisplay[index] : 0;
				}

				pos1 = new Vector2(outer * c, outer * s) * value + center;
			
				pos2 = new Vector2(inner * c, inner * s) * value + center;
				pos3 = prevY;
				prevX = pos1;
				prevY = pos2;

				// draw fill color
				vh.AddUIVertexQuad(
					SetVbo(new[] { pos0, pos1, center, center }, 
							new[] { uv0, uv1, uv2, uv3 }, new Color32(244, 12, 12, 100))
				);

				// draw borders
				vh.AddUIVertexQuad(
					SetVbo(new[] { pos0, pos1, pos2, pos3 }, 
							new[] { uv0, uv1, uv2, uv3 }, 
							m_BorderColor)
				);
			}
		}

		#region TouchHanlder
		public void OnPointerDown(PointerEventData data)
        {
			var pos = transform.InverseTransformPoint(data.position);
			var dir = new Vector2(pos.x, pos.y) - center;
			m_baseRotation = Vector3.Angle(dir, Vector2.left) * -Mathf.Sign(dir.y) - m_Rotatioin;
        }

        public void OnDrag(PointerEventData data)
        {
			var pos = transform.InverseTransformPoint(data.position);
			var dir = new Vector2(pos.x, pos.y) - center;
			m_Rotatioin = Vector3.Angle(dir, Vector2.left) * -Mathf.Sign(dir.y) - m_baseRotation;
			SetVerticesDirty();
			DrawIndicatorLabels();
        }
		
		public void OnEndDrag(PointerEventData eventData)
		{
			m_baseRotation = m_Rotatioin;
		}
		#endregion
	}

	[Serializable]
	public struct RadarChartIndicator
	{
		public string Text;
		public float MaxValue;
	}

}

