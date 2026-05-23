// Distant Lands 2025
// Simple octree for finding closest wind zones quickly 
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;
using System.Collections.Generic;

namespace DistantLands.Zephyr
{
    public class PointOctree<T> where T : class
    {
        private OctreeNode<T> rootNode;
        private readonly float initialSize;
        private readonly Vector3 initialCenter;
        private List<T> lastResults;

        // A dictionary to quickly find the node an object belongs to, crucial for efficient removal.
        private readonly Dictionary<T, OctreeNode<T>> objectToNodeMap = new Dictionary<T, OctreeNode<T>>();

        public PointOctree(float size, Vector3 center)
        {
            this.initialSize = size;
            this.initialCenter = center;
            this.rootNode = new OctreeNode<T>(new Bounds(center, new Vector3(size, size, size)), 0);
        }

        public void Add(T item, Vector3 position)
        {
            if (objectToNodeMap.ContainsKey(item))
            {
                Remove(item);
            }
            rootNode.Add(item, position, objectToNodeMap);
        }

        public bool Remove(T item)
        {
            if (objectToNodeMap.TryGetValue(item, out OctreeNode<T> node))
            {
                bool removed = node.Remove(item);
                if (removed)
                {
                    objectToNodeMap.Remove(item);
                    node.Trim();
                }
                return removed;
            }
            return false;
        }

        public List<T> GetNearby(Vector3 point, float radius)
        {
            if (lastResults == null)
                lastResults = new List<T>();

            lastResults.Clear();

            rootNode.GetNearby(new Bounds(point, new Vector3(radius * 2, radius * 2, radius * 2)), lastResults);
            return lastResults;
        }

        public void Clear()
        {
            rootNode = new OctreeNode<T>(new Bounds(initialCenter, new Vector3(initialSize, initialSize, initialSize)), 0);
            objectToNodeMap.Clear();
        }

        public List<T> GetAllItems()
        {
            return new List<T>(objectToNodeMap.Keys);
        }
    }

    public class OctreeNode<T> where T : class
    {
        private Bounds nodeBounds;
        private int depth;
        private const int MAX_OBJECTS_PER_NODE = 8;
        private const int MAX_DEPTH = 10;

        private List<(T item, Vector3 position)> objects = new List<(T, Vector3)>();
        private OctreeNode<T>[] children = null;

        public OctreeNode(Bounds bounds, int depth)
        {
            this.nodeBounds = bounds;
            this.depth = depth;
        }

        public void Add(T item, Vector3 position, Dictionary<T, OctreeNode<T>> objectMap)
        {
            if (children != null)
            {
                int childIndex = GetChildIndex(position);
                children[childIndex].Add(item, position, objectMap);
                return;
            }

            objects.Add((item, position));
            objectMap[item] = this;

            if (objects.Count > MAX_OBJECTS_PER_NODE && depth < MAX_DEPTH)
            {
                Split(objectMap);
            }
        }

        public bool Remove(T item)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].item == item)
                {
                    objects.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public void Trim()
        {
            if (children == null) return;

            int emptyChildren = 0;
            foreach (var child in children)
            {
                child.Trim();
                if (child.IsEmpty())
                {
                    emptyChildren++;
                }
            }

            if (emptyChildren == 8)
            {
                children = null;
            }
        }

        private bool IsEmpty()
        {
            if (objects.Count > 0) return false;
            if (children != null)
            {
                foreach (var child in children)
                {
                    if (!child.IsEmpty()) return false;
                }
            }
            return true;
        }


        private void Split(Dictionary<T, OctreeNode<T>> objectMap)
        {
            children = new OctreeNode<T>[8];
            float childSize = nodeBounds.size.x / 2;
            Vector3 parentCenter = nodeBounds.center;

            for (int i = 0; i < 8; i++)
            {
                Vector3 childCenter = parentCenter;
                childCenter.x += (i & 1) == 0 ? -childSize / 2 : childSize / 2;
                childCenter.y += (i & 2) == 0 ? -childSize / 2 : childSize / 2;
                childCenter.z += (i & 4) == 0 ? -childSize / 2 : childSize / 2;
                children[i] = new OctreeNode<T>(new Bounds(childCenter, new Vector3(childSize, childSize, childSize)), depth + 1);
            }

            // Move objects to children
            List<(T item, Vector3 position)> oldObjects = objects;
            objects = new List<(T, Vector3)>();
            foreach (var obj in oldObjects)
            {
                objectMap.Remove(obj.item); // Remove old mapping
                Add(obj.item, obj.position, objectMap);
            }
        }

        private int GetChildIndex(Vector3 point)
        {
            int index = 0;
            if (point.x > nodeBounds.center.x) index |= 1;
            if (point.y > nodeBounds.center.y) index |= 2;
            if (point.z > nodeBounds.center.z) index |= 4;
            return index;
        }

        public void GetNearby(Bounds queryBounds, List<T> results)
        {
            if (!nodeBounds.Intersects(queryBounds))
            {
                return;
            }

            foreach (var obj in objects)
            {
                if (queryBounds.Contains(obj.position))
                {
                    results.Add(obj.item);
                }
            }

            if (children != null)
            {
                foreach (var child in children)
                {
                    child.GetNearby(queryBounds, results);
                }
            }
        }
    }
}