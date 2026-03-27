using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Diagnostics;
using UnityEngine;

namespace EyeOfProvidence
{
    public class AssetHandler
    {
        public static AssetBundle bundle;
        public static bool bundleLoaded = false;
        public static Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        public static void LoadBundle(string name)
        {
            if (bundleLoaded)
            {
                Debug.Log("AssetBundle " + name + " already loaded");
                return;
            }
            bundle = AssetBundle.LoadFromFile(Path.Combine(Path.Combine(Utils.ModDir(), "Data"), name));
            if (bundle != null)
            {
                Debug.Log("AssetBundle " + name + " successfully loaded");
                bundleLoaded = true;
            }
            else
            {
                Debug.Log("AssetBundle " + name + " failed to loaded");
                bundleLoaded = false;
            }
        }

        public static T LoadAsset<T>(string name) where T : UnityEngine.Object
        {
            if (!bundleLoaded)
            {
                Debug.Log("Bundle is not loaded");
                return null;
            }

            T asset = bundle.LoadAsset<T>(name);
            if (asset == null)
            {
                Debug.Log(name + " didn't load");
            }
            return asset;
        }

        public static void UnloadBundle()
        {
            if (!bundleLoaded)
            {
                Debug.Log("Bundle already unloaded");
                return;
            }
            Debug.Log("Bundle succesfully unloaded");
            bundle.Unload(true);
            bundle = null;
        }
    }
}
