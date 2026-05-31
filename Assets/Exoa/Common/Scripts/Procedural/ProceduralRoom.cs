
using ProceduralToolkit;
using ProceduralToolkit.ClipperLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// Represents a procedural room that can generate walls, floor, ceiling, and other components.
    /// </summary>
    public class ProceduralRoom : ProceduralSpace
    {
        private List<MeshFilter> separateWallsList; // List of separate wall mesh filters
        public MeshFilter walls; // Mesh filter for walls
        public MeshFilter floor; // Mesh filter for floor
        public MeshFilter ceiling; // Mesh filter for ceiling
        public MeshFilter roomBox; // Mesh filter for the room box
        public ReflectionProbe reflectionProbe; // Reflection probe for reflections
        public bool roomBoxFlipCollider; // Indicates if the room box collider should be flipped

        /// <summary>
        /// Generates an empty room by clearing all meshes.
        /// </summary>
        override public void GenerateEmpty()
        {
            walls.mesh = null;
            floor.mesh = null;
            ceiling.mesh = null;
            if (roomBox != null) roomBox.mesh = null;
            if (separateWallsList != null && separateWallsList.Count > 0)
            {
                foreach (MeshFilter filter in separateWallsList)
                {
                    if (filter != null)
                    {
                        filter.mesh = null;
                    }
                }
            }
        }

        /// <summary>
        /// Generates the room based on the provided world position list.
        /// </summary>
        /// <param name="worldPosList">List of world positions defining the room.</param>
        override public void Generate(List<Vector3> worldPosList)
        {
            normalRoomPoints = MathUtils.GetClockwise(worldPosList, MathUtils.PlanType.XZ);
            GenerateWalls();
            GenerateFloor();
            GenerateCeiling();
            GenerateRoomBox();
            SetupReflection();
            AddMeshColliders();
        }

        /// <summary>
        /// Sets up the reflection probe based on the walls mesh bounds.
        /// </summary>
        private void SetupReflection()
        {
            if (reflectionProbe != null)
            {
                Bounds b = walls.GetComponent<MeshRenderer>().bounds;
                reflectionProbe.center = b.center;
                reflectionProbe.size = b.size;
            }
        }

        /// <summary>
        /// Adds mesh colliders to the walls, floor, ceiling, and separate wall meshes if necessary.
        /// </summary>
        private void AddMeshColliders()
        {
            if (addMeshColliders)
            {
                if (walls != null && walls.GetComponent<MeshCollider>() == null)
                    walls.gameObject.AddComponent<MeshCollider>();
                if (floor != null && floor.GetComponent<MeshCollider>() == null)
                    floor.gameObject.AddComponent<MeshCollider>();
                if (ceiling != null && ceiling.GetComponent<MeshCollider>() == null)
                    ceiling.gameObject.AddComponent<MeshCollider>();
                if (separateWallsList != null && separateWallsList.Count > 0)
                {
                    foreach (MeshFilter filter in separateWallsList)
                    {
                        if (filter != null && filter.GetComponent<MeshCollider>() == null)
                        {
                            filter.gameObject.AddComponent<MeshCollider>();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generates the room box mesh and updates its collider.
        /// </summary>
        private void GenerateRoomBox()
        {
            if (roomBox == null)
                return;

            MeshDraft md = new MeshDraft() { name = "RoomBox" };
            md.Add(new MeshDraft(walls.sharedMesh));
            md.Add(new MeshDraft(floor.sharedMesh));
            md.Add(new MeshDraft(ceiling.sharedMesh));
            if (roomBoxFlipCollider)
                md.FlipFaces();
            md.Paint(SpaceVertexColor);

            roomBox.mesh = md.ToMesh();
            roomBox.GetComponent<MeshCollider>().sharedMesh = roomBox.sharedMesh;
        }

        /// <summary>
        /// Generates the floor mesh based on the defined room points.
        /// </summary>
        private void GenerateFloor()
        {
            Tessellator tessellator = new Tessellator();
            tessellator.AddContour(normalRoomPoints);
            tessellator.Tessellate(normal: Vector3.up);
            MeshDraft md = tessellator.ToMeshDraft();
            md.uv = MathUtils.GenerateUVs(md.vertices, AppController.Instance.wallsHeight, AppController.Instance.wallsHeight, MathUtils.PlanType.XZ);
            md.Paint(SpaceVertexColor);
            floor.mesh = md.ToMesh();
        }

        /// <summary>
        /// Generates the ceiling mesh by cloning the floor mesh and adjusting its position.
        /// </summary>
        private void GenerateCeiling()
        {
            MeshDraft md = new MeshDraft(floor.sharedMesh) { name = "Ceiling" };
            md.Move(Vector3.up * AppController.Instance.wallsHeight);
            md.Paint(SpaceVertexColor);

            MeshCollider mc = ceiling.GetComponent<MeshCollider>();
            if (mc != null)
            {
                Mesh colliderMesh = md.ToMesh();
                mc.sharedMesh = colliderMesh;
            }

            Mesh renderMesh = md.FlipFaces().ToMesh();
            ceiling.mesh = renderMesh;
        }


        /// <summary>
        /// Generates walls for the room based on the calculated inner and normal room points.
        /// </summary>
        public void GenerateWalls()
        {
            //generate inner points;
            List<List<Vector2>> pointsInput = new List<List<Vector2>>();
            pointsInput.Add(MathUtils.ConvertVector3To2(normalRoomPoints, MathUtils.PlanType.XZ));
            List<List<Vector2>> output2 = new List<List<Vector2>>();
            List<List<Vector2>> output3 = new List<List<Vector2>>();
            PathOffsetter offsetter = new PathOffsetter();
            offsetter.AddPaths(pointsInput);
            offsetter.Offset(ref output3, 0);
            offsetter.Offset(ref output2, -AppController.Instance.interiorWallThickness);

            if (output2 == null || output2.Count == 0)
                return;

            innerRoomPoints = MathUtils.ConvertVector2To3(output2[0], MathUtils.PlanType.XZ);
            normalRoomPoints = MathUtils.ConvertVector2To3(output3[0], MathUtils.PlanType.XZ);

            while (innerRoomPoints.Count != normalRoomPoints.Count)
            {
                innerRoomPoints = FixInnerPoints(innerRoomPoints, normalRoomPoints.Count);
            }

            MeshDraft md = new MeshDraft { name = "Walls" };
            Vector3 extrusionNormal = Vector3.up;
            float extrusionSize = AppController.Instance.wallsHeight;

            Vector3 p0Outer, p1Outer, p0, p1;

            InitSeparateWallsList(innerRoomPoints.Count);
            //Debug.Log("separateWallsList:" + separateWallsList.Count);

            for (int i = 0; i < innerRoomPoints.Count; i++)
            {
                if (normalRoomPoints.Count <= (i + 1) % innerRoomPoints.Count)
                    break;

                p0Outer = innerRoomPoints[i];
                p1Outer = innerRoomPoints[(i + 1) % innerRoomPoints.Count];
                p0 = normalRoomPoints[i];
                p1 = normalRoomPoints[(i + 1) % innerRoomPoints.Count];
                List<GenericOpening> selectedOpenings = GetOpeningsBetweenPoints(openings, p0, p1, p0Outer, p1Outer);
                MeshDraft wall = CreateWallWithOpening(p0Outer, p1Outer, AppController.Instance.wallsHeight, selectedOpenings, true, AppController.Instance.interiorWallThickness);

                md.Paint(SpaceVertexColor);

                // Add the wall to a separate mesh filter
                GetOrCreateSeparateWallMeshFilter(i).mesh = wall.ToMesh();

                // Add the same wall to the merged room walls
                md.Add(wall);
            }
            //md = md.FlipFaces();
            md.Paint(SpaceVertexColor);

            // Merged wall has been replaced by separate walls
            //walls.mesh = md.ToMesh();

            // Deactivating the merged wall rendering, only separate walls are displayed
            //MeshRenderer wallsMr = walls.GetComponent<MeshRenderer>();
            //wallsMr.enabled = false;
        }

        /// <summary>
        /// Fixes the inner points of the room to have the correct number of points.
        /// </summary>
        /// <param name="innerRoomPoints">List of inner room points to fix.</param>
        /// <param name="count">The desired count of points.</param>
        /// <returns>The fixed list of inner room points.</returns>
        private List<Vector3> FixInnerPoints(List<Vector3> innerRoomPoints, int count)
        {
            float bestDistance = float.MaxValue;
            int selectedP0 = 0;
            int selectedP1 = 1;
            for (int i = 0; i < innerRoomPoints.Count; i++)
            {
                int nextIndex = (i + 1) % innerRoomPoints.Count;
                Vector3 p0 = innerRoomPoints[i];
                Vector3 p1 = innerRoomPoints[nextIndex];
                float distance = Vector3.Distance(p0, p1);
                if (distance < bestDistance)
                {
                    selectedP0 = i;
                    selectedP1 = nextIndex;
                    bestDistance = distance;
                }
            }
            innerRoomPoints[selectedP0] = (innerRoomPoints[selectedP1] + innerRoomPoints[selectedP0]) * .5f;
            innerRoomPoints.RemoveAt(selectedP1);
            return innerRoomPoints;
        }

        /// <summary>
        /// Initializes the separate walls list to the specified count.
        /// </summary>
        /// <param name="count">The count to initialize the separate wall list to.</param>
        public void InitSeparateWallsList(int count)
        {
            if (separateWallsList == null)
            {
                separateWallsList = new List<MeshFilter>();
            }
            if (separateWallsList.Count > count)
            {
                for (int i = count; i < separateWallsList.Count; i++)
                {
                    separateWallsList[i].mesh = null;
                }
            }
        }

        /// <summary>
        /// Gets or creates a separate wall mesh filter based on the provided ID.
        /// </summary>
        /// <param name="id">The ID of the wall mesh filter.</param>
        /// <returns>The corresponding mesh filter.</returns>
        public MeshFilter GetOrCreateSeparateWallMeshFilter(int id)
        {
            if (separateWallsList == null)
            {
                separateWallsList = new List<MeshFilter>();
            }
            //Debug.Log("GetOrCreateSeparateWallMeshFilter id:" + id + " separateWallsList:" + separateWallsList.Count);


            if (separateWallsList.Count < id)
            {
                throw new System.Exception("You must first create wall id:" + separateWallsList.Count + " before trying to get/create id:" + id);
            }
            if (separateWallsList.Count == id)
            {
                GameObject wall = Instantiate(walls.gameObject);
                wall.transform.ClearChildren();
                wall.name = "Wall" + id;
                wall.transform.parent = walls.transform;
                MeshRenderer wallMr = wall.GetComponent<MeshRenderer>();
                wallMr.enabled = true;
                separateWallsList.Add(wall.GetComponent<MeshFilter>());
            }
            return separateWallsList[id];
        }

        /// <summary>
        /// Creates a wall mesh with openings based on the provided parameters.
        /// </summary>
        /// <param name="p0">The first point of the wall.</param>
        /// <param name="p1">The second point of the wall.</param>
        /// <param name="height">The height of the wall.</param>
        /// <param name="openings">List of openings for the wall.</param>
        /// <param name="inward">Indicates if the wall is inward-facing.</param>
        /// <param name="thickness">The thickness of the wall.</param>
        /// <returns>A MeshDraft representing the wall.</returns>
        public static MeshDraft CreateWallWithOpening(Vector3 p0, Vector3 p1, float height, List<GenericOpening> openings, bool inward, float thickness = .1f)
        {
            MeshDraft md = new MeshDraft() { name = "WallFull" };
            Vector3 vec = p1 - p0;
            float magnitude = vec.magnitude;

            List<Vector2> subject = new List<Vector2>();
            subject.Add(new Vector2(0, 0));
            subject.Add(new Vector2(0, height));
            subject.Add(new Vector2(magnitude, height));
            subject.Add(new Vector2(magnitude, 0));

            List<List<Vector2>> output = new List<List<Vector2>>();

            var clipper = new PathClipper();
            clipper.AddPath(subject, PolyType.ptSubject);

            for (int i = 0; i < openings.Count; i++)
            {
                float xPos = openings[i].xPos;
                float yPos = openings[i].yPos + height * .5f;
                //float yPos = height * .5f;
                //print("middle:" + middle);
                float holeMidSizeX = openings[i].width * .5f;
                float holeMidSizeY = openings[i].height * .5f;
                float holeLeft = Mathf.Clamp(xPos - holeMidSizeX, 0, magnitude);
                float holeRight = Mathf.Clamp(xPos + holeMidSizeX, 0, magnitude);
                float holeTop = Mathf.Clamp(yPos + holeMidSizeY, 0, height);
                float holeBottom = Mathf.Clamp(yPos - holeMidSizeY, 0, height);

                List<Vector2> clip = new List<Vector2>();

                if (openings[i].type == GenericOpening.OpeningType.Door)
                {
                    clip.Add(new Vector2(holeLeft, -0.001f));
                    clip.Add(new Vector2(holeLeft, AppController.Instance.doorsHeight));
                    clip.Add(new Vector2(holeRight, AppController.Instance.doorsHeight));
                    clip.Add(new Vector2(holeRight, -0.001f));
                }
                else if (openings[i].type == GenericOpening.OpeningType.Window || openings[i].type == GenericOpening.OpeningType.Opening)
                {
                    clip.Add(new Vector2(holeLeft, holeTop));
                    clip.Add(new Vector2(holeRight, holeTop));
                    clip.Add(new Vector2(holeRight, holeBottom));
                    clip.Add(new Vector2(holeLeft, holeBottom));
                }

                clipper.AddPath(clip, PolyType.ptClip);

                MeshDraft windowFrame = MathUtils.Extrude(clip, inward ? Vector3.back : Vector3.forward, thickness, inward);
                md.Add(windowFrame);
            }


            clipper.Clip(ClipType.ctDifference, ref output);

            if (output.Count == 0)
                return md;

            Tessellator tessellator = new Tessellator();
            for (int i = 0; i < output.Count; i++)
                tessellator.AddContour(output[i]);
            tessellator.Tessellate(normal: inward ? Vector3.forward : Vector3.back);

            MeshDraft wall = tessellator.ToMeshDraft();
            Mesh wallMesh = wall.ToMesh();

            md.Add(wall);
            md.uv = MathUtils.GenerateUVs(md.vertices, magnitude, height, MathUtils.PlanType.XY);
            md.Rotate(Quaternion.FromToRotation(Vector3.right, vec.normalized));
            md.Move(p0);

            return md;
        }


        /// <summary>
        /// Creates a simple wall mesh without openings based on the provided parameters.
        /// </summary>
        /// <param name="p0">The first point of the wall.</param>
        /// <param name="p1">The second point of the wall.</param>
        /// <param name="height">The height of the wall.</param>
        /// <param name="inward">Indicates if the wall is inward-facing.</param>
        /// <returns>A MeshDraft representing the wall.</returns>
        public static MeshDraft CreateWall(Vector3 p0, Vector3 p1, float height, bool inward)
        {
            MeshDraft md = new MeshDraft() { name = "WallFull" };
            Vector3 vec = p1 - p0;
            float magnitude = vec.magnitude;

            List<Vector2> uvs = new List<Vector2>();
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 0));

            List<Vector3> points = new List<Vector3>();
            points.Add(new Vector3(0, 0, 0));
            points.Add(new Vector3(0, height, 0));
            points.Add(new Vector3(magnitude, height, 0));
            points.Add(new Vector3(magnitude, 0, 0));

            if (!inward)
            {
                points.Reverse();
                uvs.Reverse();
            }

            md.AddQuad(points[0], points[1], points[2], points[3], true, uvs[0], uvs[1], uvs[2], uvs[3]);
            md.Rotate(Quaternion.FromToRotation(Vector3.right, vec.normalized));
            md.Move(p0);

            return md;
        }
    }
}
