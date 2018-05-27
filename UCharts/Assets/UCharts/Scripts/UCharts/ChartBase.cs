using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UCharts 
{
	public class ChartBase : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter
	{

		protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs, Color32 color)
		{
			UIVertex[] vbo = new UIVertex[4];
			for (int i = 0; i < vertices.Length; i++)
			{
				var vert = UIVertex.simpleVert;
				vert.color = color;
				vert.position = vertices[i];
				vert.uv0 = uvs[i];
				vbo[i] = vert;
			}
			return vbo;
		}

		#region ILayoutElement Interface
		public virtual void CalculateLayoutInputHorizontal() { }
		public virtual void CalculateLayoutInputVertical() { }

		public virtual float minWidth { get { return 0; } }
		public virtual float preferredWidth { get { return 0; } }

		public virtual float flexibleWidth { get { return -1; } }
		public virtual float minHeight { get { return 0; } }

		public virtual float preferredHeight{ get { return 0; } }
		public virtual float flexibleHeight { get { return -1; } }

		public virtual int layoutPriority { get { return 0; } }
		#endregion

		#region ICanvasRaycastFilter
		public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
		{
			return true;
		}
		#endregion
	}
}
