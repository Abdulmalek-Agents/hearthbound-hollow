
using Exoa.Designer;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Exoa.Designer.DataModel;

namespace Exoa.Designer
{
    /// <summary>
    /// Handles the application and management of materials for various surfaces in a space.
    /// This includes ceilings, floors, and walls, and allows for setting textures and tiling properties.
    /// </summary>
    public class SpaceMaterialController : MonoBehaviour
    {
        public List<MeshRenderer> separateWalls; // List of separate wall mesh renderers.
        public MeshRenderer walls; // Mesh renderer for the main wall.
        public MeshRenderer floor; // Mesh renderer for the floor.
        public MeshRenderer ceiling; // Mesh renderer for the ceiling.

        private TextureSetting floorMapSetting; // Texture settings for the floor.
        private TextureSetting ceilingMatSetting; // Texture settings for the ceiling.
        private List<TextureSetting> separateWallMatSettings; // List of texture settings for separate walls.

        /// <summary>
        /// Applies a specified material to the ceiling.
        /// </summary>
        /// <param name="m">The material to be applied.</param>
        public void ApplyCeilingMaterial(Material m)
        {
            if (m != null)
            {
                ceiling.material = m;
                ceilingMatSetting.materialName = m.name;
            }
        }

        /// <summary>
        /// Applies texture settings to the ceiling by loading the material from resources.
        /// </summary>
        /// <param name="ts">Texture settings containing material name and tiling.</param>
        public void ApplyCeilingMaterial(TextureSetting ts)
        {
            Material m = Resources.Load<Material>(HDSettings.CEILING_MATERIALS_FOLDER + ts.materialName);
            if (m != null)
            {
                ceiling.material = m;
            }
            SetScale(ceiling.material, ts.tiling);
            ceilingMatSetting = ts;
        }

        /// <summary>
        /// Sets the tiling for the ceiling material.
        /// </summary>
        /// <param name="tiling">The tiling scale to be set.</param>
        public void ApplyCeilingTiling(float tiling)
        {
            SetScale(ceiling.material, tiling);
            ceilingMatSetting.tiling = tiling;
        }

        /// <summary>
        /// Applies tiling for a specific interior wall.
        /// </summary>
        /// <param name="v">Index of the wall.</param>
        /// <param name="tiling">The tiling value to apply.</param>
        public void ApplyInteriorWallTiling(int v, float tiling)
        {
            SetScale(GetSeparateWall(v).sharedMaterial, tiling);
            SetWallMaterialTiling(v, tiling);
        }

        /// <summary>
        /// Applies tiling for the floor material.
        /// </summary>
        /// <param name="tiling">The tiling scale to be set.</param>
        public void ApplyFloorTiling(float tiling)
        {
            floorMapSetting.tiling = tiling;
            SetScale(floor.material, tiling);
        }

        /// <summary>
        /// Applies a specified material to the floor.
        /// </summary>
        /// <param name="m">The material to be applied.</param>
        public void ApplyFloorMaterial(Material m)
        {
            if (m != null)
            {
                floor.material = m;
                SetScale(floor.material, floorMapSetting.tiling);
                floorMapSetting.materialName = m.name;
            }
        }

        /// <summary>
        /// Sets the texture scale of a material based on the tiling value.
        /// </summary>
        /// <param name="material">The material to scale.</param>
        /// <param name="tiling">The tiling scale to be set.</param>
        private void SetScale(Material material, float tiling)
        {
            material.SetTextureScale("_MainTex", Vector2.one * tiling);
            material.SetTextureScale("_MetallicGlossMap", Vector2.one * tiling);
            material.SetTextureScale("_BumpMap", Vector2.one * tiling);
            material.SetTextureScale("_OcclusionMap", Vector2.one * tiling);
            material.SetTextureScale("_EmissionMap", Vector2.one * tiling);
            material.SetTextureScale("_DetailNormalMap", Vector2.one * tiling);
        }

        /// <summary>
        /// Applies texture settings to the floor by loading the material from resources.
        /// </summary>
        /// <param name="ts">Texture settings containing material name and tiling.</param>
        public void ApplyFloorMaterial(TextureSetting ts)
        {
            Material m = Resources.Load<Material>(HDSettings.FLOOR_MATERIALS_FOLDER + ts.materialName);
            if (m == null)
            {
                m = Resources.Load<Material>(HDSettings.OUTSIDE_MATERIALS_FOLDER + ts.materialName);
            }
            if (m != null)
            {
                floor.material = m;
            }
            SetScale(floor.material, ts.tiling);
            floorMapSetting = ts;
        }

        /// <summary>
        /// Gets the texture settings for a specific interior wall.
        /// </summary>
        /// <param name="v">Index of the wall.</param>
        /// <returns>The texture settings for the specified wall.</returns>
        internal TextureSetting GetInteriorWallTextureSettings(int v)
        {
            return GetWallTextureSetting(v);
        }

        /// <summary>
        /// Gets the texture settings for the floor.
        /// </summary>
        /// <returns>The texture settings for the floor.</returns>
        public TextureSetting GetFloorTextureSettings()
        {
            floorMapSetting.tiling = floorMapSetting.tiling == 0 ? 1f : floorMapSetting.tiling;
            return floorMapSetting;
        }

        /// <summary>
        /// Applies tiling to all interior walls.
        /// </summary>
        /// <param name="tiling">The tiling value to apply.</param>
        public void ApplyInteriorWallTiling(float tiling)
        {
            for (int i = 0; i < walls.transform.childCount; i++)
            {
                SetWallMaterialTiling(i, tiling);
            }
        }

        /// <summary>
        /// Applies a specified material to all interior walls.
        /// </summary>
        /// <param name="mat">The material to apply to all walls.</param>
        public void ApplyInteriorWallMaterial(Material mat)
        {
            for (int i = 0; i < walls.transform.childCount; i++)
            {
                GetSeparateWall(i).material = mat;
                SetWallMaterialName(i, mat.name);
            }
        }

        /// <summary>
        /// Gets the texture settings for the ceiling.
        /// </summary>
        /// <returns>The texture settings for the ceiling.</returns>
        internal TextureSetting GetCeilingTextureSettings()
        {
            ceilingMatSetting.tiling = ceilingMatSetting.tiling == 0 ? 1f : ceilingMatSetting.tiling;
            return ceilingMatSetting;
        }

        /// <summary>
        /// Applies a specified material to a specific interior wall.
        /// </summary>
        /// <param name="wallId">The ID of the wall.</param>
        /// <param name="mat">The material to apply.</param>
        public void ApplyInteriorWallMaterial(int wallId, Material mat)
        {
            GetSeparateWall(wallId).material = mat;
            SetWallMaterialName(wallId, mat.name);
        }

        /// <summary>
        /// Applies texture settings to a specific interior wall by loading the material from resources.
        /// </summary>
        /// <param name="wallId">The ID of the wall.</param>
        /// <param name="ts">Texture settings containing material name and tiling.</param>
        public void ApplyInteriorWallMaterial(int wallId, TextureSetting ts)
        {
            Material mat = Resources.Load<Material>(HDSettings.WALL_MATERIALS_FOLDER + ts.materialName);
            if (mat == null) return;

            MeshRenderer mr = GetSeparateWall(wallId);
            mr.material = mat;
            SetScale(mr.sharedMaterial, ts.tiling);
            SetWallTextureSetting(wallId, ts);
        }

        /// <summary>
        /// Sets the texture settings for a specific wall.
        /// </summary>
        /// <param name="wallId">The ID of the wall.</param>
        /// <param name="ts">The texture settings to set.</param>
        private void SetWallTextureSetting(int wallId, TextureSetting ts)
        {
            GetWallTextureSetting(wallId);
            separateWallMatSettings[wallId] = ts;
        }

        /// <summary>
        /// Sets the material name for a specific wall.
        /// </summary>
        /// <param name="wallId">The ID of the wall.</param>
        /// <param name="name">The name of the material to set.</param>
        private void SetWallMaterialName(int wallId, string name)
        {
            TextureSetting ts = GetWallTextureSetting(wallId);
            ts.materialName = name;
            separateWallMatSettings[wallId] = ts;
        }

        /// <summary>
        /// Gets the texture settings for a specific wall.
        /// </summary>
        /// <param name="wallId">The ID of the wall.</param>
        /// <returns>The texture settings for the specified wall.</returns>
        public TextureSetting GetWallTextureSetting(int wallId)
        {
            if (separateWallMatSettings == null)
            {
                separateWallMatSettings = new List<TextureSetting>();
            }
            if (wallId >= separateWallMatSettings.Count)
            {
                int start = separateWallMatSettings.Count;
                for (int i = start; i <= wallId; i++)
                {
                    separateWallMatSettings.Add(new TextureSetting(null, 1f));
                }
            }
            return separateWallMatSettings[wallId];
        }

        /// <summary>
        /// Sets the tiling property for a specific wall's texture settings.
        /// </summary>
        /// <param name="wallId">The ID of the wall.</param>
        /// <param name="tiling">The tiling to set.</param>
        private void SetWallMaterialTiling(int wallId, float tiling)
        {
            TextureSetting ts = GetWallTextureSetting(wallId);
            ts.tiling = tiling;
            separateWallMatSettings[wallId] = ts;
        }

        /// <summary>
        /// Retrieves the MeshRenderer for a specific wall.
        /// </summary>
        /// <param name="id">The index of the wall.</param>
        /// <returns>The MeshRenderer for the wall.</returns>
        private MeshRenderer GetSeparateWall(int id)
        {
            if (separateWalls == null || separateWalls.Count == 0)
            {
                separateWalls = new List<MeshRenderer>();
                walls.gameObject.GetComponentsInChildren<MeshRenderer>(separateWalls, false, false);
            }
            if (separateWalls.Count <= id)
            {
                throw new Exception("Trying to allocate a material to wall id:" + id + " but this wall cannot be found");
            }
            return separateWalls[id];
        }

#if INTERIOR_MODULE
        /// <summary>
        /// Gets the room setting including all texture settings for walls, floor, and ceiling.
        /// </summary>
        /// <returns>A RoomSetting object containing the room's material settings.</returns>
        public RoomSetting GetRoomSetting()
        {
            RoomSetting rs = new RoomSetting();
            rs.separateWallTextureSettings = separateWallMatSettings;
            rs.floorTextureSetting = floorMapSetting;
            rs.ceilingTextureSetting = ceilingMatSetting;
            rs.floorMapItemId = transform.GetSiblingIndex();
            return rs;
        }

        /// <summary>
        /// Sets the room's material settings based on provided RoomSetting object.
        /// </summary>
        /// <param name="roomSetting">The RoomSetting object containing the texture and material settings.</param>
        public void SetRoomSetting(RoomSetting roomSetting)
        {
            HDLogger.Log("SetRoomSetting " + roomSetting.wall + " " + roomSetting.floor, HDLogger.LogCategory.Interior);

            if (!string.IsNullOrEmpty(roomSetting.floorTextureSetting.materialName))
            {
                ApplyFloorMaterial(roomSetting.floorTextureSetting);
            }
            else if (!string.IsNullOrEmpty(roomSetting.floor))
            {
                ApplyFloorMaterial(new TextureSetting(roomSetting.floor, 1));
            }

            if (roomSetting.separateWallTextureSettings != null && roomSetting.separateWallTextureSettings.Count > 0)
            {
                for (int i = 0; i < roomSetting.separateWallTextureSettings.Count; i++)
                {
                    ApplyInteriorWallMaterial(i, roomSetting.separateWallTextureSettings[i]);
                }
            }
            else if (!string.IsNullOrEmpty(roomSetting.wall))
            {
                Material m = Resources.Load<Material>(HDSettings.WALL_MATERIALS_FOLDER + roomSetting.wall);
                if (m != null) ApplyInteriorWallMaterial(m);
            }

            if (!string.IsNullOrEmpty(roomSetting.ceiling))
            {
                Material m = Resources.Load<Material>(HDSettings.CEILING_MATERIALS_FOLDER + roomSetting.ceiling);
                if (m != null) ApplyCeilingMaterial(m);
            }
        }
#else
        /// <summary>
        /// Gets room setting, irrelevant in non-interior module contexts.
        /// </summary>
        /// <returns>Always returns null.</returns>
        public object GetRoomSetting()
        {
            return null;
        }

        /// <summary>
        /// Sets room settings, irrelevant in non-interior module contexts.
        /// </summary>
        /// <param name="roomSetting">The setting to be applied.</param>
        public void SetRoomSetting(object roomSetting)
        {
        }
#endif
    }
}
