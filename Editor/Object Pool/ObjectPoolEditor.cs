using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectPool))]
public class ObjectPoolEditor : HswlfBaseEditor<ObjectPool>
{
    public override void OnInspectorGUI()
    {
        if (BuildButton("Find Startup Pools"))
        {
            Dictionary<string, ObjectPool.StartupPool> startupPools = new Dictionary<string, ObjectPool.StartupPool>();
            var allObjectPoolItems = Resources.FindObjectsOfTypeAll<ObjectPoolItem>();
            foreach (var poolItem in allObjectPoolItems)
            {
                // Don't inventory items which are part of another prefab.
                if (poolItem.transform.parent != null)
                    continue;

                // Make sure we're only getting unique assets.
                string assetPath = AssetDatabase.GetAssetPath(poolItem);
                if (startupPools.ContainsKey(assetPath))
                    continue;

                var startupPoolItem = new ObjectPool.StartupPool()
                {
                    prefab = poolItem.gameObject,
                    size = poolItem.InitialPoolSize
                };

                startupPools.Add(assetPath, startupPoolItem);
            }

            _target.startupPools = startupPools.Values.ToArray();

            EditorUtility.SetDirty(_target);
        }

        GUILayout.Space(10f);

        base.OnInspectorGUI();

        Repaint();
    }
}
