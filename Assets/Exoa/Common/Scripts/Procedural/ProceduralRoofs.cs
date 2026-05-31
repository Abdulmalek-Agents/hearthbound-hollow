
using System.Collections.Generic;
using ProceduralToolkit;
using ProceduralToolkit.Skeleton;
using UnityEngine;
using static Exoa.Designer.ProceduralRoofTop;

namespace Exoa.Designer
{
    /// <summary>
    /// Interface for constructible elements that can produce a specific type of object.
    /// </summary>
    /// <typeparam name="T">The type of object that can be constructed.</typeparam>
    public interface IConstructible<out T>
    {
        /// <summary>
        /// Constructs an object of type T at the specified layout origin.
        /// </summary>
        /// <param name="parentLayoutOrigin">The origin point for the layout.</param>
        /// <returns>A constructed object of type T.</returns>
        T Construct(Vector2 parentLayoutOrigin);
    }

    /// <summary>
    /// Abstract class representing a procedural roof that can be constructed from a foundation polygon.
    /// </summary>
    public abstract class ProceduralRoof : IConstructible<MeshDraft>
    {
        protected readonly List<Vector2> foundationPolygon;
        protected readonly AppController.RoofConfig roofConfig;
        protected readonly Color roofColor;

        /// <summary>
        /// Initializes a new instance of the ProceduralRoof class.
        /// </summary>
        /// <param name="foundationPolygon">The polygon defining the foundation of the roof.</param>
        /// <param name="roofConfig">The configuration settings for the roof.</param>
        /// <param name="roofColor">The color of the roof.</param>
        protected ProceduralRoof(List<Vector2> foundationPolygon, AppController.RoofConfig roofConfig, Color roofColor)
        {
            this.foundationPolygon = foundationPolygon;
            this.roofConfig = roofConfig;
            this.roofColor = roofColor;
        }

        /// <summary>
        /// Constructs the roof at the specified layout origin.
        /// </summary>
        /// <param name="parentLayoutOrigin">The origin point for the roof layout.</param>
        /// <returns>A MeshDraft representing the constructed roof.</returns>
        public abstract MeshDraft Construct(Vector2 parentLayoutOrigin);

        /// <summary>
        /// Constructs the base of the roof including overhang and border if applicable.
        /// </summary>
        /// <param name="roofPolygon2">Output parameter for the 2D roof polygon.</param>
        /// <param name="roofPolygon3">Output parameter for the 3D roof polygon.</param>
        /// <returns>A MeshDraft representing the base of the roof.</returns>
        protected MeshDraft ConstructRoofBase(out List<Vector2> roofPolygon2, out List<Vector3> roofPolygon3)
        {
            roofPolygon2 = Geometry.OffsetPolygon(foundationPolygon, roofConfig.overhang);
            roofPolygon3 = roofPolygon2.ConvertAll(v => v.ToVector3XZ());

            var roofDraft = new MeshDraft();
            if (roofConfig.thickness > 0)
            {
                roofDraft.Add(ConstructBorder(roofPolygon2, roofPolygon3, roofConfig));
            }
            if (roofConfig.overhang > 0)
            {
                roofDraft.Add(ConstructOverhang(foundationPolygon, roofPolygon3));
            }
            return roofDraft;
        }

        /// <summary>
        /// Constructs the border of the roof based on the provided polygons and roof configuration.
        /// </summary>
        /// <param name="roofPolygon2">The 2D polygon of the roof.</param>
        /// <param name="roofPolygon3">The 3D polygon of the roof.</param>
        /// <param name="roofConfig">The roof configuration settings.</param>
        /// <returns>A MeshDraft representing the border of the roof.</returns>
        protected static MeshDraft ConstructBorder(List<Vector2> roofPolygon2, List<Vector3> roofPolygon3, AppController.RoofConfig roofConfig)
        {
            List<Vector3> upperRing = roofPolygon2.ConvertAll(v => v.ToVector3XZ() + Vector3.up * roofConfig.thickness);
            return new MeshDraft().AddFlatQuadBand(roofPolygon3, upperRing, true);
        }

        /// <summary>
        /// Constructs the overhang part of the roof.
        /// </summary>
        /// <param name="foundationPolygon">The polygon defining the foundation of the building.</param>
        /// <param name="roofPolygon3">The 3D polygon of the roof.</param>
        /// <returns>A MeshDraft representing the overhang of the roof.</returns>
        protected static MeshDraft ConstructOverhang(List<Vector2> foundationPolygon, List<Vector3> roofPolygon3)
        {
            List<Vector3> lowerRing = foundationPolygon.ConvertAll(v => v.ToVector3XZ());
            return new MeshDraft().AddFlatQuadBand(lowerRing, roofPolygon3, true);
        }

        /// <summary>
        /// Constructs the contour draft of the roof using the provided polygon and pitch.
        /// </summary>
        /// <param name="skeletonPolygon2">The 2D polygon representing the skeleton of the roof.</param>
        /// <param name="roofPitch">The pitch of the roof, expressed in degrees.</param>
        /// <returns>A MeshDraft representing the contour of the roof.</returns>
        protected static MeshDraft ConstructContourDraft(List<Vector2> skeletonPolygon2, float roofPitch)
        {
            Vector2 edgeA = skeletonPolygon2[0];
            Vector2 edgeB = skeletonPolygon2[1];
            Vector2 edgeDirection2 = (edgeB - edgeA).normalized;
            Vector3 roofNormal = CalculateRoofNormal(edgeDirection2, roofPitch);

            var skeletonPolygon3 = skeletonPolygon2.ConvertAll(v => v.ToVector3XZ());

            var tessellator = new Tessellator();
            tessellator.AddContour(skeletonPolygon3);
            tessellator.Tessellate(normal: Vector3.up);
            var contourDraft = tessellator.ToMeshDraft(false);

            for (var i = 0; i < contourDraft.vertexCount; i++)
            {
                Vector2 vertex = contourDraft.vertices[i].ToVector2XZ();
                float height = CalculateVertexHeight(vertex, edgeA, edgeDirection2, roofPitch);
                contourDraft.vertices[i] = new Vector3(vertex.x, height, vertex.y);
                contourDraft.normals.Add(roofNormal);
            }
            contourDraft.uv = MathUtils.GenerateUVs(contourDraft.vertices, AppController.Instance.wallsHeight, AppController.Instance.wallsHeight, MathUtils.PlanType.XY);

            return contourDraft;
        }

        /// <summary>
        /// Constructs a gable draft from the provided vertices of the skeleton polygon.
        /// </summary>
        /// <param name="skeletonPolygon2">The 2D polygon representing the skeleton of the gable roof.</param>
        /// <param name="roofPitch">The pitch of the roof, expressed in degrees.</param>
        /// <returns>A MeshDraft representing the gable roof.</returns>
        protected static MeshDraft ConstructGableDraft(List<Vector2> skeletonPolygon2, float roofPitch)
        {
            Vector2 edgeA2 = skeletonPolygon2[0];
            Vector2 edgeB2 = skeletonPolygon2[1];
            Vector2 peak2 = skeletonPolygon2[2];
            Vector2 edgeDirection2 = (edgeB2 - edgeA2).normalized;

            float peakHeight = CalculateVertexHeight(peak2, edgeA2, edgeDirection2, roofPitch);
            Vector3 edgeA3 = edgeA2.ToVector3XZ();
            Vector3 edgeB3 = edgeB2.ToVector3XZ();
            Vector3 peak3 = new Vector3(peak2.x, peakHeight, peak2.y);
            Vector2 gableTop2 = Closest.PointSegment(peak2, edgeA2, edgeB2);
            Vector3 gableTop3 = new Vector3(gableTop2.x, peakHeight, gableTop2.y);

            MeshDraft md = new MeshDraft().AddTriangle(edgeA3, edgeB3, gableTop3, true)
                .AddTriangle(edgeA3, gableTop3, peak3, true)
                .AddTriangle(edgeB3, peak3, gableTop3, true);
            return md;
        }

        /// <summary>
        /// Calculates the height of a vertex on the roof based on its position relative to the edge and roof pitch.
        /// </summary>
        /// <param name="vertex">The vertex position as a 2D point.</param>
        /// <param name="edgeA">One endpoint of the edge line.</param>
        /// <param name="edgeDirection">The normalized direction of the edge line.</param>
        /// <param name="roofPitch">The pitch of the roof in degrees.</param>
        /// <returns>The calculated height of the vertex.</returns>
        protected static float CalculateVertexHeight(Vector2 vertex, Vector2 edgeA, Vector2 edgeDirection, float roofPitch)
        {
            float distance = Distance.PointLine(vertex, edgeA, edgeDirection);
            return Mathf.Tan(roofPitch * Mathf.Deg2Rad) * distance;
        }

        /// <summary>
        /// Calculates the normal vector of the roof based on the edge direction and pitch.
        /// </summary>
        /// <param name="edgeDirection2">The direction of the edge as a 2D vector.</param>
        /// <param name="roofPitch">The pitch of the roof in degrees.</param>
        /// <returns>The calculated normal vector for the roof.</returns>
        protected static Vector3 CalculateRoofNormal(Vector2 edgeDirection2, float roofPitch)
        {
            return Quaternion.AngleAxis(roofPitch, edgeDirection2.ToVector3XZ()) * Vector3.up;
        }
    }

    /// <summary>
    /// Procedural class that defines a flat roof.
    /// </summary>
    public class ProceduralFlatRoof : ProceduralRoof
    {
        /// <summary>
        /// Initializes a new instance of the ProceduralFlatRoof class.
        /// </summary>
        /// <param name="foundationPolygon">The polygon defining the foundation of the flat roof.</param>
        /// <param name="roofConfig">The configuration settings for the roof.</param>
        /// <param name="roofColor">The color of the roof.</param>
        public ProceduralFlatRoof(List<Vector2> foundationPolygon, AppController.RoofConfig roofConfig, Color roofColor)
            : base(foundationPolygon, roofConfig, roofColor)
        {
        }

        /// <summary>
        /// Constructs the flat roof at the specified layout origin.
        /// </summary>
        /// <param name="parentLayoutOrigin">The origin point for the roof layout.</param>
        /// <returns>A MeshDraft representing the constructed flat roof.</returns>
        public override MeshDraft Construct(Vector2 parentLayoutOrigin)
        {
            var roofDraft = ConstructRoofBase(out List<Vector2> roofPolygon2, out List<Vector3> roofPolygon3);

            var tessellator = new Tessellator();
            tessellator.AddContour(roofPolygon3);
            tessellator.Tessellate(normal: Vector3.up);
            var roofTop = tessellator.ToMeshDraft(false)
                .Move(Vector3.up * roofConfig.thickness);
            for (var i = 0; i < roofTop.vertexCount; i++)
            {
                roofTop.normals.Add(Vector3.up);
            }
            roofTop.uv = MathUtils.GenerateUVs(roofTop.vertices, AppController.Instance.wallsHeight, AppController.Instance.wallsHeight, MathUtils.PlanType.XZ);

            return roofDraft.Add(roofTop)
                .Paint(roofColor);
        }
    }

    /// <summary>
    /// Procedural class that defines a hipped roof.
    /// </summary>
    public class ProceduralHippedRoof : ProceduralRoof
    {
        private const float RoofPitch = 25; // The pitch of the hipped roof.

        /// <summary>
        /// Initializes a new instance of the ProceduralHippedRoof class.
        /// </summary>
        /// <param name="foundationPolygon">The polygon defining the foundation of the hipped roof.</param>
        /// <param name="roofConfig">The configuration settings for the roof.</param>
        /// <param name="roofColor">The color of the roof.</param>
        public ProceduralHippedRoof(List<Vector2> foundationPolygon, AppController.RoofConfig roofConfig, Color roofColor)
            : base(foundationPolygon, roofConfig, roofColor)
        {
        }

        /// <summary>
        /// Constructs the hipped roof at the specified layout origin.
        /// </summary>
        /// <param name="parentLayoutOrigin">The origin point for the roof layout.</param>
        /// <returns>A MeshDraft representing the constructed hipped roof.</returns>
        public override MeshDraft Construct(Vector2 parentLayoutOrigin)
        {
            var roofDraft = ConstructRoofBase(out List<Vector2> roofPolygon2, out List<Vector3> roofPolygon3);

            var skeletonGenerator = new StraightSkeletonGenerator();
            var skeleton = skeletonGenerator.Generate(roofPolygon2);

            var roofTop = new MeshDraft();
            foreach (var skeletonPolygon2 in skeleton.polygons)
            {
                roofTop.Add(ConstructContourDraft(skeletonPolygon2, RoofPitch));
            }
            roofTop.Move(Vector3.up * roofConfig.thickness);

            roofDraft.Add(roofTop)
                .Paint(roofColor);
            return roofDraft;
        }
    }

    /// <summary>
    /// Procedural class that defines a gabled roof.
    /// </summary>
    public class ProceduralGabledRoof : ProceduralRoof
    {
        private const float RoofPitch = 25; // The pitch of the gabled roof.

        /// <summary>
        /// Initializes a new instance of the ProceduralGabledRoof class.
        /// </summary>
        /// <param name="foundationPolygon">The polygon defining the foundation of the gabled roof.</param>
        /// <param name="roofConfig">The configuration settings for the roof.</param>
        /// <param name="roofColor">The color of the roof.</param>
        public ProceduralGabledRoof(List<Vector2> foundationPolygon, AppController.RoofConfig roofConfig, Color roofColor)
            : base(foundationPolygon, roofConfig, roofColor)
        {
        }

        /// <summary>
        /// Constructs the gabled roof at the specified layout origin.
        /// </summary>
        /// <param name="parentLayoutOrigin">The origin point for the roof layout.</param>
        /// <returns>A MeshDraft representing the constructed gabled roof.</returns>
        public override MeshDraft Construct(Vector2 parentLayoutOrigin)
        {
            var roofDraft = ConstructRoofBase(out List<Vector2> roofPolygon2, out List<Vector3> roofPolygon3);

            var skeletonGenerator = new StraightSkeletonGenerator();
            var skeleton = skeletonGenerator.Generate(roofPolygon2);

            var roofTop = new MeshDraft();
            foreach (var skeletonPolygon2 in skeleton.polygons)
            {
                if (skeletonPolygon2.Count == 3)
                {
                    roofTop.Add(ConstructGableDraft(skeletonPolygon2, RoofPitch));
                }
                else
                {
                    roofTop.Add(ConstructContourDraft(skeletonPolygon2, RoofPitch));
                }
            }
            roofTop.Move(Vector3.up * roofConfig.thickness);

            roofDraft.Add(roofTop)
                .Paint(roofColor);
            return roofDraft;
        }
    }
}
