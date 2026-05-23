using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using DistantLands.Zephyr;
using DistantLands.Zephyr.EditorScripts;
using UnityEditor.UIElements;
using System.Collections.Generic;

[Overlay(typeof(SceneView), "Zephyr Wind", true, defaultDockZone = DockZone.TopToolbar)]
[Icon("Packages/com.distantlands.zephyr/Editor/Resources/Zephyr Icon.png")]
public class ZephyrSceneOverlay : Overlay
{
    VisualElement root;
    private ZephyrWind windManager;
    private Button createWindZoneButton;

    public Slider BaseWindStrengthSlider => root.Q<Slider>("windSlider");
    public VisualElement StillButton => root.Q<VisualElement>("still");
    public VisualElement CalmButton => root.Q<VisualElement>("calm");
    public VisualElement BreezyButton => root.Q<VisualElement>("breezy");
    public VisualElement StrongButton => root.Q<VisualElement>("strong");
    public VisualElement ViolentButton => root.Q<VisualElement>("violent");
    public PropertyField DirectionSource => root.Q<PropertyField>("directionSource");
    public PropertyField TargetDirection => root.Q<PropertyField>("targetDirection");
    public VisualElement WindVelocityGraph => root.Q<VisualElement>("windVelocityGraph");

    private float startingAngle;
    private float startingWindAngle;
    private bool dragging;
    readonly float CONTROL_SIZE = 25;
    readonly static Color WHEEL_BG = new Color(0.35f, 0.35f, 0.35f);
    readonly static Color WHEEL_FG = new Color(1f, 1f, 1f);

    void OnEnable()
    {
        EditorApplication.update += UpdateWindVelocityGraph;
    }

    void UpdateWindVelocityGraph()
    {
        if (root != null && WindVelocityGraph != null)
            WindVelocityGraph.MarkDirtyRepaint();
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        EditorApplication.update -= UpdateWindVelocityGraph;
    }

    public override VisualElement CreatePanelContent()
    {
        windManager = ZephyrWind.Instance;

        root = new VisualElement();

        VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Packages/com.distantlands.zephyr/Editor/UI/Core/UXML/zephyr-overlay.uxml"
        );

        asset.CloneTree(root);

        if (windManager == null)
        {
            root.Add(new HelpBox("Zephyr is not currently set up in your scene. Setup now?", HelpBoxMessageType.Warning));
            Button button = new Button();
            button.text = "Setup Zephyr";
            button.RegisterCallback<ClickEvent>(evt =>
            {
                WindZoneCreation.CreateWindManager();
                Close();
            });
            root.Add(button);
            return root;
        }


        SerializedObject serializedObject = new SerializedObject(windManager);

        BaseWindStrengthSlider.BindProperty(serializedObject.FindProperty("targetWindStrength"));

        StillButton.RegisterCallback<ClickEvent>(evt =>
                    {
                        serializedObject.FindProperty("targetWindStrength").floatValue = 0;
                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                    });
        CalmButton.RegisterCallback<ClickEvent>(evt =>
        {
            serializedObject.FindProperty("targetWindStrength").floatValue = 1.25f;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        });
        BreezyButton.RegisterCallback<ClickEvent>(evt =>
        {
            serializedObject.FindProperty("targetWindStrength").floatValue = 2.5f;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        });
        StrongButton.RegisterCallback<ClickEvent>(evt =>
        {
            serializedObject.FindProperty("targetWindStrength").floatValue = 3.75f;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        });
        ViolentButton.RegisterCallback<ClickEvent>(evt =>
        {
            serializedObject.FindProperty("targetWindStrength").floatValue = 5;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        });

        WindVelocityGraph.RegisterCallback<MouseDownEvent>(evt =>
        {
            Vector2 offset = WindVelocityGraph.contentRect.center - evt.localMousePosition;
            startingAngle = Mathf.Atan2(-offset.y, offset.x) * Mathf.Rad2Deg;
            startingWindAngle = Mathf.Atan2(-windManager.Direction.z, windManager.Direction.x) * Mathf.Rad2Deg;
            dragging = true;
            MouseCaptureController.CaptureMouse(WindVelocityGraph);
        });

        WindVelocityGraph.RegisterCallback<MouseMoveEvent>(evt =>
        {
            if (dragging)
            {
                Vector2 offset = WindVelocityGraph.contentRect.center - evt.localMousePosition;
                float newAngle = Mathf.Atan2(-offset.y, offset.x) * Mathf.Rad2Deg;
                float difference = NormalizeAngle(startingAngle - newAngle);
                float finalAngle = startingWindAngle + difference;

                if (evt.shiftKey)
                    finalAngle = Mathf.Round(finalAngle / 45f) * 45f;

                WindVelocityGraph.MarkDirtyRepaint();

                windManager.TargetDirection = new Vector3(Mathf.Cos(finalAngle * Mathf.Deg2Rad), 0, -Mathf.Sin(finalAngle * Mathf.Deg2Rad)).normalized;
            }
        });

        WindVelocityGraph.RegisterCallback<MouseUpEvent>(evt =>
        {
            if (dragging)
            {
                Vector2 offset = WindVelocityGraph.contentRect.center - evt.localMousePosition;
                float newAngle = Mathf.Atan2(-offset.y, offset.x) * Mathf.Rad2Deg;
                float difference = NormalizeAngle(startingAngle - newAngle);
                float finalAngle = startingWindAngle + difference;

                if (evt.shiftKey)
                    finalAngle = Mathf.Round(finalAngle / 45f) * 45f;

                Undo.RecordObject(windManager, "Change Wind Direction");
                windManager.TargetDirection = new Vector3(Mathf.Cos(finalAngle * Mathf.Deg2Rad), 0, -Mathf.Sin(finalAngle * Mathf.Deg2Rad)).normalized;

                EditorUtility.SetDirty(windManager);
                dragging = false;
                MouseCaptureController.ReleaseMouse(WindVelocityGraph);
            }

        });

        WindVelocityGraph.generateVisualContent += (MeshGenerationContext ctx) =>
        {
            var painter = ctx.painter2D;
            float width = WindVelocityGraph.contentRect.width;
            float height = WindVelocityGraph.contentRect.height;
            painter.lineCap = LineCap.Round;

            painter.strokeColor = WHEEL_BG;
            painter.lineWidth = 2f;

            painter.BeginPath();
            painter.MoveTo(new Vector2(0.2f * width, 0.5f * height));
            painter.LineTo(new Vector2(0.8f * width, 0.5f * height));
            painter.Stroke();

            painter.BeginPath();
            painter.MoveTo(new Vector2(0.5f * width, 0.2f * height));
            painter.LineTo(new Vector2(0.5f * width, 0.8f * height));
            painter.Stroke();

            painter.lineWidth = 15f;
            painter.BeginPath();
            painter.Arc(new Vector2(0.5f * width, 0.5f * height), width * 0.3f, 0, 360, ArcDirection.Clockwise);
            painter.Stroke();

            float remappedWindAngle = Mathf.Atan2(-windManager.TargetDirection.z, windManager.TargetDirection.x) * Mathf.Rad2Deg;
            float remappedCurrentWindAngle = Mathf.Atan2(-windManager.Direction.z, windManager.Direction.x) * Mathf.Rad2Deg;

            void DrawTriangle(float angle)
            {
                float ARROW_WIDTH = 0.10f;
                float ARROW_DEPTH = 0.15f;
                Vector2[] points = new Vector2[3] {
                        new Vector2(0.3f * width, -ARROW_WIDTH * height),
                        new Vector2(0.3f * width,  ARROW_WIDTH * height),
                        new Vector2((0.3f + ARROW_DEPTH) * width, 0f)
                };

                Vector2 center = WindVelocityGraph.contentRect.center;

                // rotate points
                Vector2[] rotated = new Vector2[3];
                for (int i = 0; i < 3; i++)
                    rotated[i] = RotatePoint(points[i] + center, center, angle);

                float[] vertexAngles = new float[3];
                for (int i = 0; i < 3; i++)
                    vertexAngles[i] = Mathf.Atan2(points[i].y, points[i].x) * Mathf.Rad2Deg + angle;

                bool[] outside = new bool[3];
                for (int i = 0; i < 3; i++)
                    outside[i] = AngleBetween(vertexAngles[i], remappedCurrentWindAngle) > CONTROL_SIZE;

                // If all outside, just cover whole triangle
                if (outside[0] && outside[1] && outside[2])
                {
                    painter.fillColor = WHEEL_BG;
                    painter.strokeColor = painter.fillColor;

                    painter.BeginPath();
                    painter.MoveTo(rotated[0]);
                    painter.LineTo(rotated[1]);
                    painter.LineTo(rotated[2]);
                    painter.ClosePath();
                    painter.Fill();
                    return;
                }

                // Draw full grey triangle first (backdrop)
                painter.fillColor = WHEEL_BG;
                painter.BeginPath();
                painter.MoveTo(rotated[0]);
                painter.LineTo(rotated[1]);
                painter.LineTo(rotated[2]);
                painter.ClosePath();
                painter.Fill();

                // Clip against wedge
                float minAngle = remappedCurrentWindAngle - CONTROL_SIZE;
                float maxAngle = remappedCurrentWindAngle + CONTROL_SIZE;

                List<Vector2> clipped = ClipPolygonToWedge(rotated, center, minAngle, maxAngle);

                if (clipped.Count >= 3)
                {
                    painter.fillColor = WHEEL_FG;
                    painter.BeginPath();
                    painter.MoveTo(clipped[0]);
                    for (int i = 1; i < clipped.Count; i++)
                        painter.LineTo(clipped[i]);
                    painter.ClosePath();
                    painter.Fill();
                }


            }

            DrawTriangle(0);
            DrawTriangle(90);
            DrawTriangle(180);
            DrawTriangle(-90);



            painter.lineCap = LineCap.Butt;
            painter.strokeColor = WHEEL_FG;
            painter.lineWidth = 15f;
            painter.BeginPath();
            painter.Arc(new Vector2(0.5f * width, 0.5f * height), width * 0.3f, remappedCurrentWindAngle - CONTROL_SIZE, remappedCurrentWindAngle + CONTROL_SIZE, ArcDirection.Clockwise);
            painter.Stroke();


            painter.lineWidth = 10f;
            painter.lineCap = LineCap.Butt;
            painter.strokeColor = WHEEL_FG * 0.9f;
            painter.BeginPath();
            painter.Arc(new Vector2(0.5f * width, 0.5f * height), width * 0.3f, remappedCurrentWindAngle - (CONTROL_SIZE - 2), remappedCurrentWindAngle - (CONTROL_SIZE - 3), ArcDirection.Clockwise);
            painter.Arc(new Vector2(0.5f * width, 0.5f * height), width * 0.3f, remappedCurrentWindAngle - (CONTROL_SIZE - 5), remappedCurrentWindAngle - (CONTROL_SIZE - 6), ArcDirection.Clockwise);
            painter.Stroke();
            painter.BeginPath();
            painter.Arc(new Vector2(0.5f * width, 0.5f * height), width * 0.3f, remappedCurrentWindAngle + (CONTROL_SIZE - 3), remappedCurrentWindAngle + (CONTROL_SIZE - 2), ArcDirection.Clockwise);
            painter.Arc(new Vector2(0.5f * width, 0.5f * height), width * 0.3f, remappedCurrentWindAngle + (CONTROL_SIZE - 6), remappedCurrentWindAngle + (CONTROL_SIZE - 5), ArcDirection.Clockwise);
            painter.Stroke();

            painter.strokeColor = WHEEL_FG;
            painter.lineWidth = 2f;
            painter.BeginPath();
            painter.Arc(new Vector2(0.5f * width, 0.5f * height), width * 0.26f, remappedWindAngle - CONTROL_SIZE, remappedWindAngle + CONTROL_SIZE, ArcDirection.Clockwise);
            painter.Stroke();


        };

        return root;
    }

    private static Vector2 RotatePoint(Vector2 point, Vector2 pivot, float angleDegrees)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;

        float cos = Mathf.Cos(angleRadians);
        float sin = Mathf.Sin(angleRadians);

        // Translate point back to origin
        float dx = point.x - pivot.x;
        float dy = point.y - pivot.y;

        // Rotate
        float xNew = dx * cos - dy * sin;
        float yNew = dx * sin + dy * cos;

        // Translate back
        return new Vector2(xNew + pivot.x, yNew + pivot.y);
    }

    // Helper to normalize angle to [-180, 180]
    private float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }

    float AngleBetween(float startAngle, float remappedWindAngle)
    {
        // Normalize to [0, 360)
        startAngle = (startAngle % 360 + 360) % 360;
        remappedWindAngle = (remappedWindAngle % 360 + 360) % 360;

        // Find shortest angular difference (-180, 180]
        float diff = Mathf.DeltaAngle(startAngle, remappedWindAngle);

        // Check if within tolerance
        return Mathf.Abs(diff);
    }

    // Clip polygon (triangle here) against angular wedge
    List<Vector2> ClipPolygonToWedge(Vector2[] poly, Vector2 center, float minDeg, float maxDeg)
    {
        List<Vector2> input = new List<Vector2>(poly);
        // Clip against min boundary
        input = ClipAgainstRay(input, center, minDeg, true);
        // Clip against max boundary
        input = ClipAgainstRay(input, center, maxDeg, false);
        return input;
    }

    // Clip polygon against a single ray boundary
    // isMin = true → keep inside region >= minDeg
    // isMin = false → keep inside region <= maxDeg
    List<Vector2> ClipAgainstRay(List<Vector2> poly, Vector2 center, float boundaryDeg, bool isMin)
    {
        List<Vector2> output = new List<Vector2>();
        if (poly.Count == 0) return output;

        Vector2 rayDir = new Vector2(Mathf.Cos(boundaryDeg * Mathf.Deg2Rad), Mathf.Sin(boundaryDeg * Mathf.Deg2Rad));

        Vector2 prev = poly[poly.Count - 1];
        bool prevInside = IsInside(prev, center, rayDir, isMin);

        foreach (Vector2 curr in poly)
        {
            bool currInside = IsInside(curr, center, rayDir, isMin);

            if (currInside)
            {
                if (!prevInside)
                {
                    // Edge enters region → add intersection
                    Vector2 inter = IntersectRayEdge(center, rayDir, prev, curr);
                    output.Add(inter);
                }
                output.Add(curr);
            }
            else if (prevInside)
            {
                // Edge leaves region → add intersection
                Vector2 inter = IntersectRayEdge(center, rayDir, prev, curr);
                output.Add(inter);
            }

            prev = curr;
            prevInside = currInside;
        }

        return output;
    }

    // Inside test: which side of ray are we on?
    bool IsInside(Vector2 point, Vector2 center, Vector2 rayDir, bool isMin)
    {
        Vector2 rel = point - center;
        float cross = rayDir.x * rel.y - rayDir.y * rel.x;
        return isMin ? (cross >= 0f) : (cross <= 0f);
    }

    // Intersection between edge and boundary ray
    Vector2 IntersectRayEdge(Vector2 center, Vector2 rayDir, Vector2 a, Vector2 b)
    {
        Vector2 e = b - a;
        float denom = e.x * rayDir.y - e.y * rayDir.x;
        if (Mathf.Abs(denom) < 1e-6f) return a; // parallel safeguard

        Vector2 ac = center - a;
        float t = (ac.x * rayDir.y - ac.y * rayDir.x) / denom;
        return a + t * e;
    }
}