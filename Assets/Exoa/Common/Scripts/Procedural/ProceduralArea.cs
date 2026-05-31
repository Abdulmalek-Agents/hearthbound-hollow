
using ProceduralToolkit;
using ProceduralToolkit.ClipperLib;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// A class that represents a procedural area, which can be generated as either a cube or a rectangular shape.
    /// </summary>
    public class ProceduralArea : MonoBehaviour
    {
        public enum Type { Rectangle, Cube };
        public Type type;
        public float width = 50f;
        public float depth = 50f;
        public float height = 3f;
        public float thickness = .2f;
        public MeshFilter wallsMf;

        /// <summary>
        /// Called when the object is being destroyed.
        /// </summary>
        void OnDestroy()
        {

        }

        /// <summary>
        /// Called when the script instance is being loaded. Used to initialize the area.
        /// </summary>
        void Start()
        {

        }

        /// <summary>
        /// Generates the geometry for the procedural area based on the specified type (cube or rectangle).
        /// </summary>
        public void Generate()
        {
            if (type == Type.Cube)
            {
                MeshDraft cubeExt = MeshDraft.Cube(width + thickness, true);
                MeshDraft cubeInt = MeshDraft.Cube(width, true);
                cubeInt.FlipFaces();
                MeshDraft walls = new MeshDraft();
                walls.Add(cubeExt);
                walls.Add(cubeInt);
                wallsMf.mesh = walls.ToMesh();
            }
            else if (type == Type.Rectangle)
            {
                List<Vector3> points = new List<Vector3>();
                points.Add(new Vector3(-width * .5f, 0, -depth * .5f));
                points.Add(new Vector3(-width * .5f, 0, depth * .5f));
                points.Add(new Vector3(width * .5f, 0, depth * .5f));
                points.Add(new Vector3(width * .5f, 0, -depth * .5f));


                List<List<Vector2>> pointsInput = new List<List<Vector2>>();
                pointsInput.Add(MathUtils.ConvertVector3To2(points, MathUtils.PlanType.XZ));
                List<List<Vector2>> output1 = new List<List<Vector2>>();
                List<List<Vector2>> output2 = new List<List<Vector2>>();


                PathOffsetter offsetter = new PathOffsetter();
                offsetter.AddPaths(pointsInput);
                offsetter.Offset(ref output1, 0);
                offsetter.Offset(ref output2, thickness);

                List<Vector3> points1 = MathUtils.ConvertVector2To3(output1[0], MathUtils.PlanType.XZ);
                List<Vector3> points2 = MathUtils.ConvertVector2To3(output2[0], MathUtils.PlanType.XZ);

                MeshDraft walls = new MeshDraft();
                walls.Add(GenerateWalls(points1, false));
                walls.Add(GenerateWalls(points2, true));
                walls.Add(GenerateCeiling(output1[0], output2[0]));

                wallsMf.mesh = walls.ToMesh();
            }
        }

        /// <summary>
        /// Generates the ceiling of the procedural area based on the provided list of points.
        /// </summary>
        /// <param name="points1">The first list of points that define the ceiling.</param>
        /// <param name="points2">The second list of points used to subtract from the first.</param>
        /// <returns>A MeshDraft representing the ceiling.</returns>
        private MeshDraft GenerateCeiling(List<Vector2> points1, List<Vector2> points2)
        {
            List<List<Vector2>> output = new List<List<Vector2>>();

            var clipper = new PathClipper();
            clipper.AddPath(points2, PolyType.ptSubject);
            clipper.AddPath(points1, PolyType.ptClip);
            clipper.Clip(ClipType.ctDifference, ref output);


            Tessellator tessellator = new Tessellator();
            for (int i = 0; i < output.Count; i++)
                tessellator.AddContour(output[i]);
            tessellator.Tessellate(normal: Vector3.back);
            MeshDraft md = tessellator.ToMeshDraft();
            md.Rotate(Quaternion.Euler(90, 0, 0));
            md.Move(Vector3.up * height);
            return md;
        }

        /// <summary>
        /// Generates the walls of the procedural area based on the specified points.
        /// </summary>
        /// <param name="points">A list of points defining the wall shapes.</param>
        /// <param name="inward">A boolean indicating whether the walls should be generated inward.</param>
        /// <returns>A MeshDraft representing the walls.</returns>
        private MeshDraft GenerateWalls(List<Vector3> points, bool inward)
        {
            MeshDraft md = new MeshDraft { name = "Walls" };

            Vector3 p0, p1;
            for (int i = 0; i < points.Count; i++)
            {
                p0 = points[i];
                p1 = points[(i + 1) % points.Count];
                MeshDraft wall = ProceduralRoom.CreateWall(p0, p1, height, inward);
                md.Add(wall);
            }
            //md = md.FlipFaces();
            return md;

        }
    }
}
