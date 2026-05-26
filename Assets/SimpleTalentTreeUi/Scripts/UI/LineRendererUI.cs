using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Types of Bezier / curve smoothing used by LineRendererUI.
    /// </summary>
    public enum BezierType
    {
        None,
        Quick,
        Basic,
        Improved,
        Catenary
    }

    /// <summary>
    /// UI line renderer that draws 2D lines inside a Canvas using vertices,
    /// with optional Bezier/catenary smoothing and line caps.
    /// </summary>
    public class LineRendererUI : MaskableGraphic
    {
        /// <summary>
        /// Points defining the line path. If relativeSize = true, values are in [0,1]
        /// relative to the RectTransform size. Otherwise, values are in local space units.
        /// </summary>
        public Vector2[] Points;

        [Tooltip("Line thickness in local units.")]
        public float lineThickness = 2f;

        [Tooltip("If true, points are treated as normalized (0-1) relative to the RectTransform size.")]
        public bool relativeSize = true;

        [Tooltip("If true, treats points as independent segments (pairs). If false, draws a continuous polyline.")]
        public bool lineList;

        [Tooltip("If true, draws line caps at the start and end segments.")]
        public bool lineCaps;

        [Tooltip("Bezier smoothing mode used to generate intermediate points.")]
        public BezierType bezierMode = BezierType.None;

        [Tooltip("Number of segments generated per Bezier curve or catenary segment.")]
        public int bezierSegmentsPerCurve = 10;

        [Tooltip("Slack parameter for catenary (CableCurve) mode.")]
        public float bezierSlack = 0.5f;

        // Default UVs for the line texture.
        private static readonly Vector2 uvTopLeft = new Vector2(0f, 0f);
        private static readonly Vector2 uvBottomLeft = new Vector2(0f, 1f);
        private static readonly Vector2 uvTopRight = new Vector2(1f, 0f);
        private static readonly Vector2 uvBottomRight = new Vector2(1f, 1f);

        /// <summary>
        /// Returns the main texture used by this graphic.
        /// Falls back to Unity's white texture if no material texture is assigned.
        /// </summary>
        public override Texture mainTexture
        {
            get
            {
                if (material != null && material.mainTexture != null)
                    return material.mainTexture;

                return s_WhiteTexture;
            }
        }

        /// <summary>
        /// Populates the VertexHelper with vertices representing the line.
        /// This is where the 2D mesh for the line is built.
        /// </summary>
        /// <param name="vh">VertexHelper to write vertices into.</param>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (Points == null || Points.Length < 2)
                return;

            // Local copy of points so we can replace them with Bezier / catenary results.
            Vector2[] pts = Points;

            // Ensure valid segment count
            if (bezierSegmentsPerCurve < 1)
                bezierSegmentsPerCurve = 1;

            // Handle Bezier smoothing (except catenary mode).
            if (bezierMode != BezierType.None &&
                bezierMode != BezierType.Catenary &&
                pts.Length > 3)
            {
                var bezierPath = new BezierPath
                {
                    SegmentsPerCurve = bezierSegmentsPerCurve
                };

                bezierPath.SetControlPoints(pts);

                List<Vector2> drawingPoints;
                switch (bezierMode)
                {
                    case BezierType.Basic:
                        drawingPoints = bezierPath.GetDrawingPoints0();
                        break;
                    case BezierType.Improved:
                        drawingPoints = bezierPath.GetDrawingPoints1();
                        break;
                    default: // Quick or any other -> more adaptive method
                        drawingPoints = bezierPath.GetDrawingPoints2();
                        break;
                }

                pts = drawingPoints.ToArray();
            }

            // Handle catenary (CableCurve) when exactly 2 points are provided.
            if (bezierMode == BezierType.Catenary && pts.Length == 2)
            {
                var cable = new CableCurve(pts)
                {
                    slack = bezierSlack,
                    steps = bezierSegmentsPerCurve
                };

                pts = cable.Points();
            }

            float sizeX = relativeSize ? rectTransform.rect.width : 1f;
            float sizeY = relativeSize ? rectTransform.rect.height : 1f;
            var size = new Vector2(sizeX, sizeY);

            Vector2 offset = new Vector2(
                -rectTransform.pivot.x * sizeX,
                -rectTransform.pivot.y * sizeY);

            var segments = new List<UIVertex[]>(pts.Length * 2);

            if (lineList)
            {
                // Treat every pair of points as an independent segment.
                for (int i = 1; i < pts.Length; i += 2)
                {
                    Vector2 start = pts[i - 1] * size + offset;
                    Vector2 end = pts[i] * size + offset;

                    if (lineCaps)
                        segments.Add(CreateLineCap(start, end, true));

                    segments.Add(CreateLineSegment(start, end));

                    if (lineCaps)
                        segments.Add(CreateLineCap(start, end, false));
                }
            }
            else
            {
                // Draw a continuous polyline.
                for (int i = 1; i < pts.Length; i++)
                {
                    Vector2 start = pts[i - 1] * size + offset;
                    Vector2 end = pts[i] * size + offset;

                    if (lineCaps && i == 1)
                        segments.Add(CreateLineCap(start, end, true));

                    segments.Add(CreateLineSegment(start, end));

                    if (lineCaps && i == pts.Length - 1)
                        segments.Add(CreateLineCap(start, end, false));
                }
            }

            // Send all quads to the VertexHelper.
            for (int i = 0; i < segments.Count; i++)
            {
                vh.AddUIVertexQuad(segments[i]);
            }

            // Unity's UIVertex limit is ~65k. Guard against overflow.
            if (vh.currentVertCount > 64000)
            {
                Debug.LogError($"[SimpleTalentTreeUI] Vertex count exceeds UI limit: {vh.currentVertCount}. Clearing mesh to avoid issues.");
                vh.Clear();
            }
        }

        /// <summary>
        /// Creates a quad representing a line segment between two points.
        /// </summary>
        private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end)
        {
            Vector2 direction = end - start;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                direction = Vector2.right;

            Vector2 perpendicular = new Vector2(direction.y, -direction.x).normalized * (lineThickness * 0.5f);

            Vector2 v1 = start - perpendicular;
            Vector2 v2 = start + perpendicular;
            Vector2 v3 = end + perpendicular;
            Vector2 v4 = end - perpendicular;

            return new[]
            {
                CreateVertex(v1, uvTopLeft),
                CreateVertex(v2, uvBottomLeft),
                CreateVertex(v3, uvBottomRight),
                CreateVertex(v4, uvTopRight)
            };
        }

        /// <summary>
        /// Creates a small cap at one end of a line segment to close the geometry.
        /// </summary>
        private UIVertex[] CreateLineCap(Vector2 start, Vector2 end, bool isStart)
        {
            Vector2 dir = (end - start).normalized;
            if (dir.sqrMagnitude <= Mathf.Epsilon)
                dir = Vector2.right;

            float halfThickness = lineThickness * 0.5f;
            Vector2 capPoint = isStart
                ? start - dir * halfThickness
                : end + dir * halfThickness;

            return isStart
                ? CreateLineSegment(capPoint, start)
                : CreateLineSegment(end, capPoint);
        }

        /// <summary>
        /// Creates a single vertex with the given position, UV and the current graphic color.
        /// </summary>
        private UIVertex CreateVertex(Vector2 pos, Vector2 uv)
        {
            var vert = UIVertex.simpleVert;
            vert.position = pos;
            vert.uv0 = uv;
            vert.color = color;
            return vert;
        }
    }

    // ─────────────────────────────────────────────
    // CableCurve (Catenary curve approximation)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Approximates a catenary curve (a hanging cable) between two points in 2D.
    /// Used by LineRendererUI when bezierMode is Catenary.
    /// </summary>
    [Serializable]
    public class CableCurve
    {
        [SerializeField] private Vector2 m_start;
        [SerializeField] private Vector2 m_end;
        [SerializeField] private float m_slack;
        [SerializeField] private int m_steps;
        [SerializeField] private bool m_regen;

        private static readonly Vector2[] emptyCurve =
        {
            new Vector2(0f, 0f),
            new Vector2(0f, 0f)
        };

        [SerializeField] private Vector2[] points;

        /// <summary>
        /// True if the curve points should be regenerated on next access.
        /// </summary>
        public bool regenPoints
        {
            get => m_regen;
            set => m_regen = value;
        }

        /// <summary>
        /// Start point of the cable.
        /// </summary>
        public Vector2 start
        {
            get => m_start;
            set { if (value != m_start) m_regen = true; m_start = value; }
        }

        /// <summary>
        /// End point of the cable.
        /// </summary>
        public Vector2 end
        {
            get => m_end;
            set { if (value != m_end) m_regen = true; m_end = value; }
        }

        /// <summary>
        /// Slack factor for the cable (0 = straight line, larger values = more sag).
        /// </summary>
        public float slack
        {
            get => m_slack;
            set { if (!Mathf.Approximately(value, m_slack)) m_regen = true; m_slack = Mathf.Max(0f, value); }
        }

        /// <summary>
        /// Number of segments used to approximate the curve. Minimum is 2.
        /// </summary>
        public int steps
        {
            get => m_steps;
            set { if (value != m_steps) m_regen = true; m_steps = Mathf.Max(2, value); }
        }

        public CableCurve()
        {
            points = emptyCurve;
            m_start = Vector2.up;
            m_end = Vector2.up + Vector2.right;
            m_slack = 0.5f;
            m_steps = 20;
            m_regen = true;
        }

        public CableCurve(Vector2[] inputPoints)
        {
            points = inputPoints;
            m_start = inputPoints[0];
            m_end = inputPoints[1];
            m_slack = 0.5f;
            m_steps = 20;
            m_regen = true;
        }

        public CableCurve(List<Vector2> inputPoints)
        {
            points = inputPoints.ToArray();
            m_start = inputPoints[0];
            m_end = inputPoints[1];
            m_slack = 0.5f;
            m_steps = 20;
            m_regen = true;
        }

        public CableCurve(CableCurve other)
        {
            points = other.Points();
            m_start = other.start;
            m_end = other.end;
            m_slack = other.slack;
            m_steps = other.steps;
            m_regen = other.regenPoints;
        }

        /// <summary>
        /// Returns the current curve points, regenerating them if needed.
        /// </summary>
        public Vector2[] Points()
        {
            if (!m_regen)
                return points;

            if (m_steps < 2)
                return emptyCurve;

            float lineDist = Vector2.Distance(m_end, m_start);
            float lineDistH = Vector2.Distance(new Vector2(m_end.x, m_start.y), m_start);
            float l = lineDist + Mathf.Max(0.0001f, m_slack);
            float r = 0f;
            float s = m_start.y;
            float u = lineDistH;
            float v = m_end.y;

            if (Mathf.Approximately(u - r, 0f))
                return emptyCurve;

            float ztarget = Mathf.Sqrt(l * l - (v - s) * (v - s)) / (u - r);

            int loops = 30;
            int iterationCount = 0;
            int maxIterations = loops * 10;
            bool found = false;
            float z = 0f;
            float zstep = 100f;

            for (int i = 0; i < loops; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    iterationCount++;
                    float ztest = z + zstep;
                    float ztesttarget = (float)Math.Sinh(ztest) / ztest;

                    if (float.IsInfinity(ztesttarget))
                        continue;

                    if (Mathf.Approximately(ztesttarget, ztarget))
                    {
                        found = true;
                        z = ztest;
                        break;
                    }

                    if (ztesttarget > ztarget)
                    {
                        break;
                    }

                    z = ztest;

                    if (iterationCount > maxIterations)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                    break;

                zstep *= 0.1f;
            }

            float a = (u - r) / (2f * z);
            float p = (r + u - a * Mathf.Log((l + v - s) / (l - v + s))) * 0.5f;
            float q = (v + s - l * (float)Math.Cosh(z) / (float)Math.Sinh(z)) * 0.5f;

            points = new Vector2[m_steps];
            float stepsf = m_steps - 1;

            for (int i = 0; i < m_steps; i++)
            {
                float t = i / stepsf;

                Vector2 pos;
                pos.x = Mathf.Lerp(m_start.x, m_end.x, t);
                pos.y = a * (float)Math.Cosh(((t * lineDistH) - p) / a) + q;

                points[i] = pos;
            }

            m_regen = false;
            return points;
        }
    }

    // ─────────────────────────────────────────────
    // BezierPath (multi-curve Bezier helper)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Helper class for Bezier curve generation and sampling.
    /// Used by LineRendererUI for Bezier smoothing modes.
    /// </summary>
    public class BezierPath
    {
        /// <summary>
        /// Number of segments generated per Bezier curve when sampling.
        /// </summary>
        public int SegmentsPerCurve = 10;

        /// <summary>
        /// Minimum squared distance between sampled points when adaptively subdividing.
        /// </summary>
        public float MINIMUM_SQR_DISTANCE = 0.01f;

        /// <summary>
        /// Threshold for angle-based subdivision when adaptively subdividing.
        /// </summary>
        public float DIVISION_THRESHOLD = -0.99f;

        private readonly List<Vector2> controlPoints;
        private int curveCount;

        public BezierPath()
        {
            controlPoints = new List<Vector2>();
        }

        /// <summary>
        /// Sets the control points for this BezierPath using a list.
        /// </summary>
        public void SetControlPoints(List<Vector2> newControlPoints)
        {
            controlPoints.Clear();
            controlPoints.AddRange(newControlPoints);
            curveCount = (controlPoints.Count - 1) / 3;
        }

        /// <summary>
        /// Sets the control points for this BezierPath using an array.
        /// </summary>
        public void SetControlPoints(Vector2[] newControlPoints)
        {
            controlPoints.Clear();
            controlPoints.AddRange(newControlPoints);
            curveCount = (controlPoints.Count - 1) / 3;
        }

        /// <summary>
        /// Returns the underlying control points list (modifiable).
        /// </summary>
        public List<Vector2> GetControlPoints()
        {
            return controlPoints;
        }

        /// <summary>
        /// Builds control points by interpolating given segment points using a scale
        /// factor to generate smooth Bezier control handles.
        /// </summary>
        public void Interpolate(List<Vector2> segmentPoints, float scale)
        {
            controlPoints.Clear();

            if (segmentPoints.Count < 2)
                return;

            for (int i = 0; i < segmentPoints.Count; i++)
            {
                if (i == 0)
                {
                    Vector2 p1 = segmentPoints[i];
                    Vector2 p2 = segmentPoints[i + 1];
                    Vector2 tangent = (p2 - p1);
                    Vector2 q1 = p1 + scale * tangent;

                    controlPoints.Add(p1);
                    controlPoints.Add(q1);
                }
                else if (i == segmentPoints.Count - 1)
                {
                    Vector2 p0 = segmentPoints[i - 1];
                    Vector2 p1 = segmentPoints[i];
                    Vector2 tangent = (p1 - p0);
                    Vector2 q0 = p1 - scale * tangent;

                    controlPoints.Add(q0);
                    controlPoints.Add(p1);
                }
                else
                {
                    Vector2 p0 = segmentPoints[i - 1];
                    Vector2 p1 = segmentPoints[i];
                    Vector2 p2 = segmentPoints[i + 1];
                    Vector2 tangent = (p2 - p0).normalized;

                    Vector2 q0 = p1 - scale * tangent * (p1 - p0).magnitude;
                    Vector2 q1 = p1 + scale * tangent * (p2 - p1).magnitude;

                    controlPoints.Add(q0);
                    controlPoints.Add(p1);
                    controlPoints.Add(q1);
                }
            }

            curveCount = (controlPoints.Count - 1) / 3;
        }

        /// <summary>
        /// Pre-samples a set of points and builds control points based on them.
        /// </summary>
        public void SamplePoints(List<Vector2> sourcePoints, float minSqrDistance, float maxSqrDistance, float scale)
        {
            if (sourcePoints.Count < 2)
                return;

            var samplePoints = new Stack<Vector2>();
            samplePoints.Push(sourcePoints[0]);

            Vector2 potentialSamplePoint = sourcePoints[1];

            for (int i = 2; i < sourcePoints.Count; i++)
            {
                if ((potentialSamplePoint - sourcePoints[i]).sqrMagnitude > minSqrDistance &&
                    (samplePoints.Peek() - sourcePoints[i]).sqrMagnitude > maxSqrDistance)
                {
                    samplePoints.Push(potentialSamplePoint);
                }

                potentialSamplePoint = sourcePoints[i];
            }

            Vector2 p1 = samplePoints.Pop();
            Vector2 p0 = samplePoints.Peek();
            Vector2 tangent = (p0 - potentialSamplePoint).normalized;

            float d2 = (potentialSamplePoint - p1).magnitude;
            float d1 = (p1 - p0).magnitude;

            p1 = p1 + tangent * ((d1 - d2) * 0.5f);

            samplePoints.Push(p1);
            samplePoints.Push(potentialSamplePoint);

            Interpolate(new List<Vector2>(samplePoints), scale);
        }

        /// <summary>
        /// Calculates a single Bezier point on the curve at the given curve index and t parameter [0..1].
        /// </summary>
        public Vector2 CalculateBezierPoint(int curveIndex, float t)
        {
            int nodeIndex = curveIndex * 3;

            Vector2 p0 = controlPoints[nodeIndex];
            Vector2 p1 = controlPoints[nodeIndex + 1];
            Vector2 p2 = controlPoints[nodeIndex + 2];
            Vector2 p3 = controlPoints[nodeIndex + 3];

            return CalculateBezierPoint(t, p0, p1, p2, p3);
        }

        /// <summary>
        /// Simple uniform sampling. Returns equally spaced points on each curve.
        /// </summary>
        public List<Vector2> GetDrawingPoints0()
        {
            var drawingPoints = new List<Vector2>();

            for (int curveIndex = 0; curveIndex < curveCount; curveIndex++)
            {
                if (curveIndex == 0)
                    drawingPoints.Add(CalculateBezierPoint(curveIndex, 0f));

                for (int j = 1; j <= SegmentsPerCurve; j++)
                {
                    float t = j / (float)SegmentsPerCurve;
                    drawingPoints.Add(CalculateBezierPoint(curveIndex, t));
                }
            }

            return drawingPoints;
        }

        /// <summary>
        /// Uniform sampling using direct control points for each 4-point segment.
        /// </summary>
        public List<Vector2> GetDrawingPoints1()
        {
            var drawingPoints = new List<Vector2>();

            for (int i = 0; i < controlPoints.Count - 3; i += 3)
            {
                Vector2 p0 = controlPoints[i];
                Vector2 p1 = controlPoints[i + 1];
                Vector2 p2 = controlPoints[i + 2];
                Vector2 p3 = controlPoints[i + 3];

                if (i == 0)
                    drawingPoints.Add(CalculateBezierPoint(0f, p0, p1, p2, p3));

                for (int j = 1; j <= SegmentsPerCurve; j++)
                {
                    float t = j / (float)SegmentsPerCurve;
                    drawingPoints.Add(CalculateBezierPoint(t, p0, p1, p2, p3));
                }
            }

            return drawingPoints;
        }

        /// <summary>
        /// Adaptive subdivision sampling. This tends to generate more points where the curve bends.
        /// </summary>
        public List<Vector2> GetDrawingPoints2()
        {
            var drawingPoints = new List<Vector2>();

            for (int curveIndex = 0; curveIndex < curveCount; curveIndex++)
            {
                List<Vector2> bezierCurveDrawingPoints = FindDrawingPoints(curveIndex);

                if (curveIndex != 0 && bezierCurveDrawingPoints.Count > 0)
                    bezierCurveDrawingPoints.RemoveAt(0);

                drawingPoints.AddRange(bezierCurveDrawingPoints);
            }

            return drawingPoints;
        }

        private List<Vector2> FindDrawingPoints(int curveIndex)
        {
            var pointList = new List<Vector2>();

            Vector2 left = CalculateBezierPoint(curveIndex, 0f);
            Vector2 right = CalculateBezierPoint(curveIndex, 1f);

            pointList.Add(left);
            pointList.Add(right);

            FindDrawingPoints(curveIndex, 0f, 1f, pointList, 1);

            return pointList;
        }

        private int FindDrawingPoints(int curveIndex, float t0, float t1, List<Vector2> pointList, int insertionIndex)
        {
            Vector2 left = CalculateBezierPoint(curveIndex, t0);
            Vector2 right = CalculateBezierPoint(curveIndex, t1);

            if ((left - right).sqrMagnitude < MINIMUM_SQR_DISTANCE)
                return 0;

            float tMid = (t0 + t1) * 0.5f;
            Vector2 mid = CalculateBezierPoint(curveIndex, tMid);

            Vector2 leftDirection = (left - mid).normalized;
            Vector2 rightDirection = (right - mid).normalized;

            if (Vector2.Dot(leftDirection, rightDirection) > DIVISION_THRESHOLD ||
                Mathf.Abs(tMid - 0.5f) < 0.0001f)
            {
                int pointsAddedCount = 0;

                pointsAddedCount += FindDrawingPoints(curveIndex, t0, tMid, pointList, insertionIndex);

                pointList.Insert(insertionIndex + pointsAddedCount, mid);
                pointsAddedCount++;

                pointsAddedCount += FindDrawingPoints(curveIndex, tMid, t1, pointList, insertionIndex + pointsAddedCount);

                return pointsAddedCount;
            }

            return 0;
        }

        private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 p = uuu * p0;
            p += 3f * uu * t * p1;
            p += 3f * u * tt * p2;
            p += ttt * p3;

            return p;
        }
    }
}
