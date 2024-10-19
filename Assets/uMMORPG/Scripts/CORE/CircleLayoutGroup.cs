using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI
{
	/// <summary>
	/// A UI Component that automatically positions your objects in beautiful circular arrangements.
	/// </summary>
	[AddComponentMenu ("Layout/Circle Layout Group", 152)]
	public class CircleLayoutGroup : LayoutGroup
	{
		[RangeAttribute(-1f, 1f)]
		[SerializeField] protected float m_Offset = 0f;
		public float offset
		{ 
			get { return m_Offset; } 
			set { SetProperty (ref m_Offset, Mathf.Clamp (value, -1f, 1f)); } 
		}

		[RangeAttribute(-1f, 1f)]
		[SerializeField] protected float m_CircleScale = 1f;
		public float circleScale
		{ 
			get { return m_CircleScale; } 
			set { SetProperty (ref m_CircleScale, Mathf.Clamp (value, -1f, 1f)); } 
		}

		[SerializeField] protected Vector2 m_Scaling = Vector2.one;
		public Vector2 scaling
		{ 
			get { return m_Scaling; } 
			set { SetProperty (ref m_Scaling, new Vector2(Mathf.Clamp(value.x, 0, 10f), Mathf.Clamp(value.y, 0, 10f))); } 
		}

		[SerializeField] protected Vector2 m_PositionOffset = Vector2.zero;
		public Vector2 positionOffset
		{ 
			get { return m_PositionOffset; } 
			set {
				SetProperty (ref m_PositionOffset, value); } 
		}

		[SerializeField] protected bool m_OffsetStartAxis = false;
		public bool offsetStartAxis { get { return m_OffsetStartAxis; } set { SetProperty (ref m_OffsetStartAxis, value); } }

		public float startOffset
		{ get { return offsetStartAxis ? 90f : 0f; } }

		[SerializeField] protected bool m_Clockwise = false;
		public bool clockwise { get { return m_Clockwise; } set { SetProperty (ref m_Clockwise, value); } }

		[SerializeField] protected bool m_SkipFirst = false;
		public bool skipFirst { get { return m_SkipFirst; } set { SetProperty (ref m_SkipFirst, value); } }

		[SerializeField] protected bool m_DynamicCenter = false;
		public bool dynamicCenter { get { return m_DynamicCenter; } set { SetProperty (ref m_DynamicCenter, value); } }

		[SerializeField] protected float m_Radius = 100f;
		public float radius { get { return m_Radius; } set { SetProperty (ref m_Radius, value); } }

		[SerializeField] protected int m_MaxCount = 10000;
		public int maxCount 
		{ 
			get { return m_MaxCount; } 
			set { SetProperty (ref m_MaxCount, Mathf.Clamp(value, 0, 10000)); } 
		}

		public List<Vector2> PositionsList
		{
			get
			{
				var list = new List<Vector2>();

				for (int i = 0; i < maxCount; i++)
				{
					Vector2 pos = GetPosition(i);

					if (!RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, transform.TransformPoint(pos)))
						continue;

					list.Add(pos);
				}

				return list;
			}
		}

		public enum LoopControlType
		{
			Infinite,
			Expand,
		}

		public enum SizeControlType
		{
			None,
			Fixed,
			Dynamic
		}

		public enum PositionControlType
		{
			Dynamic, // Divides by count
			Sides,
			Angle,
			Distance,
		}

		public enum OrientationType
		{
			None,
			Dynamic,
//			Fixed,
//			Incremental,
		}

		public LoopControlType LoopControl;

		public SizeControlType SizeControl;

		public OrientationType m_OrientationType;

//		[RangeAttribute(-1f, 1f)]
//		public float Orientation;

		[RangeAttribute(-3f, 3f)]
		public float AngleScale = 1f;

		[RangeAttribute(-1f, 1f)]
		public float AngleOffset = 0f;

		public Vector2 FixedSize = new Vector2(100, 100);

		public PositionControlType PositionControl;

		public float FixedDistance;

		public float GetDynamicAngle (int index)
		{
			if (PositionControl == PositionControlType.Dynamic)
				return 1f / ChildCount;
			else if (PositionControl == PositionControlType.Angle)
				return FixedDistance;
			else if (PositionControl == PositionControlType.Distance)
				return (FixedDistance / Radius * (LoopControl == LoopControlType.Infinite ? GetRingCount (index, (FixedDistance / Radius)) : 1));
			else if (PositionControl == PositionControlType.Sides)
				return  1f / FixedDistance;

			return  1f / 6f;
		}

		public float DynamicAngle
		{
			get
			{
				if (PositionControl == PositionControlType.Dynamic)
					return 1f / (ChildCount - (skipFirst ? 0 : 1));
				else if (PositionControl == PositionControlType.Angle)
					return FixedDistance;
				else if (PositionControl == PositionControlType.Distance)
					return (FixedDistance / Radius);
				else if (PositionControl == PositionControlType.Sides)
					return  1f / FixedDistance;

				return  1f / 6f;
			}
		}

		public float CircleCircumference
		{ get { return 2f * Mathf.PI * radius; } }

		public float Radius
		{
			get
			{
				if (SizeControl == SizeControlType.Dynamic)
					return DynamicRadius;
				else
					return radius;
			}
		}

		public float DynamicRadius
		{
			get
			{
				// First, get the length of the shortest side.
				var s = Mathf.Min ( this.rectTransform.sizeDelta.x, this.rectTransform.sizeDelta.y );

				// Divide this by the number of rings.
				return (s/2f) / (float)RingCount;
			}
		}

		public int RingCount
		{
			get
			{
				var count = ChildCount;
				float theta = this.DynamicAngle;

				if (!skipFirst)
					count--;

				return GetRingCount (count, theta);
			}
		}

		public static int GetRingCount (int count, float theta)
		{
			int finalRingCount = 1;
			float sides = (1f / theta);

			float cc = sides * finalRingCount;

			while (count > cc)
			{
				finalRingCount++;

				cc += sides * finalRingCount; 
			}

			return (int) finalRingCount;
		}

		public int ChildCount
		{ get { return rectChildren.Count; } }

		public Vector2 DynamicSize
		{
			get
			{
				return new Vector2 (DynamicRadius, DynamicRadius * 0.8667f);
			}
		}

		#if UNITY_EDITOR
		protected override void OnValidate ()
		{
			base.OnValidate ();
			offset = offset;
			maxCount = maxCount;
			scaling = scaling;
		}

		void OnDrawGizmosSelected() 
		{
			// Draw all potential positions within the box.


			// Display the explosion radius when selected

			var list = PositionsList;
			for (int i = 0; i < list.Count; i++)
			{
				Vector2 pos = list[i];

				// Draw center
				if (Event.current.shift)
				{
					Gizmos.color = new Color(1, 1, 0, 0.75F);
					if (i == 0 && !skipFirst)
						Gizmos.color = new Color(1, 0, 0, 0.75F);
					Gizmos.DrawSphere (transform.TransformPoint(pos), Radius * 0.1f);
				}

				// Draw circle
				if (Event.current.control)
				{
//					Gizmos.color = new Color(1, 1, 0, 0.75F);
//					Gizmos.DrawWireSphere (transform.TransformPoint(pos), Radius);


					for (int j = 1; j <= this.RingCount; j++) {
						Gizmos.color = new Color(1, 1, 0, 0.75F);
						Gizmos.DrawWireSphere (transform.TransformPoint(Vector3.zero), Radius * j);
					}
				}
			}
		}
		#endif

		protected CircleLayoutGroup()
		{ }

		#if UNITY_EDITOR
		protected override void Reset ()
		{
			this.childAlignment = TextAnchor.MiddleCenter;

			base.Reset ();
		}
		#endif

		protected Vector2 GetPositionMiddleCenter (int i)
		{
			if (!skipFirst)
			{
				if (i == 0)
					return new Vector2 (0, 0);
				else
					i = i - 1;
			}

			var dynamicAngle = this.DynamicAngle;
			float dynamicOffs = 0f;
			if (dynamicCenter)
			{
				dynamicOffs = dynamicAngle * (this.ChildCount - 1) / 2.0f;
				//				Vector2 center = (rectChildren [0].localPosition) / 2.0f;
				//				finalPos -= center;
			}

			float angle = (dynamicAngle) * i - dynamicOffs;

			angle = angle * circleScale;

			angle = angle + 0.25f + offset;

			if (offsetStartAxis)
				angle += dynamicAngle / 2.0f;

			float radians = angle * (2f * Mathf.PI);

			float ring = 1f;

			if (LoopControl == LoopControlType.Expand)
				ring = GetRingCount (i, dynamicAngle);

			float x = this.Radius * ring * (Mathf.Cos (radians / ring));
			float y = this.Radius * ring * (Mathf.Sin (radians / ring));

			Vector2 finalPos = new Vector2 (x * (clockwise ? -1f : 1f) * scaling.x, y * scaling.y) + positionOffset;

			return finalPos;
		}

		protected Vector2 GetPosition (int i)
		{
			return GetPositionMiddleCenter(i);

			// TODO: More implementations need to be considered for different alignments.
//			if (childAlignment == TextAnchor.MiddleCenter)
//				return GetPositionMiddleCenter(i);
//			else
//				return Vector2.zero;
		}

		protected void SetPositions ()
		{
//			int inc = clockwise ? -1 : 1;
			for (int i = 0; i < rectChildren.Count; i++)
			{
				RectTransform rect = rectChildren[Mathf.Abs(i)];

				m_Tracker.Add(this, rect,
					DrivenTransformProperties.Anchors |
					DrivenTransformProperties.AnchoredPosition);// |
//					DrivenTransformProperties.SizeDelta);

				if (m_OrientationType != OrientationType.None)
					m_Tracker.Add (this, rect, DrivenTransformProperties.Rotation);
						
//				rect.sizeDelta = cellSize;
				rect.localPosition = GetPosition(i);

				if (m_OrientationType != OrientationType.None)
					rect.localEulerAngles = GetRotation (i);
			}
		}

		public Vector3 GetRotation (int index)
		{
			if (index == 0 && !skipFirst)
				return new Vector3 (0, 0, 0);
			
			Vector3 rot = new Vector3 ();

			var angle = GetNormalizedPosition (index);

			rot = new Vector3 (0, 0, ((0.25f + AngleOffset) * 360f + angle * -360f) * AngleScale);

			return rot;
		}

		public float GetNormalizedPosition (int i)
		{
			if (!skipFirst)
			{
				if (i == 0)
					return 0;
				else
					i = i - 1;
			}

			var dynamicAngle = this.DynamicAngle;
			float dynamicOffs = 0f;
			if (dynamicCenter)
			{
				dynamicOffs = dynamicAngle * (this.ChildCount - 1) / 2.0f;
				//				Vector2 center = (rectChildren [0].localPosition) / 2.0f;
				//				finalPos -= center;
			}

			float angle = (dynamicAngle) * i - dynamicOffs;

			angle = angle * circleScale;

			angle = angle + 0.25f + offset;

			if (offsetStartAxis)
				angle += dynamicAngle / 2.0f;

			return angle;
		}

		protected void SetSize ()
		{
			if (SizeControl == SizeControlType.None)
				return;
			
			for (int i = 0; i < rectChildren.Count; i++)
			{
				var rect = rectChildren [i];

				if (SizeControl == SizeControlType.Fixed)
					rect.sizeDelta = FixedSize;
				else
					rect.sizeDelta = DynamicSize;

				m_Tracker.Add(this, rect, DrivenTransformProperties.SizeDelta);
			}
		}

		public override void SetLayoutVertical ()
		{
			SetPositions ();
			SetSize ();
		}

		public override void SetLayoutHorizontal ()
		{
			SetPositions ();
			SetSize ();
		}

		public override void CalculateLayoutInputVertical ()
		{
			// ...
		}

		public override void CalculateLayoutInputHorizontal ()
		{
			base.CalculateLayoutInputHorizontal ();
		}

		#if UNITY_EDITOR
		[ContextMenu("Generate Children Rings")]
		public void GenerateChildrenRingsFromContextMenu ()
		{
			if (rectChildren == null || rectChildren.Count == 0)
				return;

			int rings = maxCount;

			// 1: 6 
			// 2: 6x1 + 6x2 + 6x3
			// 3: 6 + 12 + 18
			// 4: 6 + 12 + 18 + 24
			int count = 0;

			for (int i = 1; i <= rings; i++)
			{
				count += 6 * i;
			}

//			int count = maxCount;

			if (count < 2)
				return;

			DestroyAllChildrenImmediate (this.transform, rectChildren[0]);

			GenerateChildren(count);
		}

		public static void DestroyAllChildrenImmediate ( Transform target, Transform immune )
		{
			foreach (Transform t in target)
				if (t != immune)
					DestroyImmediate (t.gameObject);
		}

		[ContextMenu("Generate Children")]
		public void GenerateChildrenFromContextMenu ()
		{
			if (rectChildren == null || rectChildren.Count == 0)
				return;

			int count = maxCount;

			if (count < 2)
				return;

			DestroyAllChildrenImmediate (this.transform, rectChildren[0]);

			GenerateChildren(count);
		}

		private void GenerateChildren (int count)
		{
			for (int i = 0; i < count; i++) 
			{
				var g = Instantiate (rectChildren[0]);
				g.transform.SetParent (transform,false);
				g.name = rectChildren[0].name + string.Format(" ({0})", i);
			}
		}
		#endif

	}
}