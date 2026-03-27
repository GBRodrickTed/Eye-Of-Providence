using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace EyeOfProvidence
{
    // Honestly amazying https://yal.cc/tools/pixel-font/
    public static class Utils
    {
        // Forgot to multiply acceleration by time :(
        // Lets me (kinda) not care about framerate dependance while also not having to use big scary numbers
        public const float TIMESTEP_BANDAGE = 60;
        public static string ModDir()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        public static string ModDataDir()
        {
            return Path.Combine(ModDir(), "Data");
        }
        public static Vector3 RandPos(float x, float y, float z)
        {
            return new Vector3(
                    UnityEngine.Random.Range(-x, x),
                    UnityEngine.Random.Range(-y, y),
                    UnityEngine.Random.Range(-z, z)
                );
        }
        public static Vector3 RandPos(float x, float y)
        {
            return new Vector3(
                    UnityEngine.Random.Range(-x, x),
                    UnityEngine.Random.Range(-y, y),
                    0
                );
        }

        public static Vector3 Uniform(float num)
        {
            return new Vector3(num, num, num);
        }
        public static Vector3 Scale(float num)
        {
            return new Vector3(num, num, num);
        }

        public static float Sin01(float x)
        {
            return (Mathf.Sin(x) + 1) * 0.5f;
        }

        public static float Normalize(float num, bool oneDefault = true)
        {
            if (num == 0)
            {
                if (oneDefault)
                {
                    return 1;
                }
                return 0;
            }
            return num / Mathf.Abs(num);
        }

        public static float DirToAngle(Vector3 dir)
        {
            return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

        public static float UpDirToAngle(Vector3 dir)
        {
            dir = Quaternion.AngleAxis(-90, Vector3.forward) * dir;
            return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

        public static float AngleRelToDir(Vector3 dir, Vector3 relDir)
        {
            dir = Quaternion.AngleAxis(-DirToAngle(relDir), Vector3.forward) * dir;
            return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

        public static float AngleRelToDir(Vector3 dir, float angle)
        {
            dir = Quaternion.AngleAxis(angle, Vector3.forward) * dir;
            return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

        public static Vector3 AngleToDir(float angle)
        {
            return Quaternion.Euler(0, 0, -angle) * Vector3.right;
        }

        public static Vector3 UpAngleToDir(float angle)
        {
            return Quaternion.Euler(0, 0, angle) * Vector3.up;
        }

        public static void SetTimelineBinding(PlayableDirector pd, int index, UnityEngine.Object obj)
        {

            pd.SetGenericBinding((pd.playableAsset as TimelineAsset).GetOutputTrack(index), obj);
            //Debug.Log(index + " " + (pd.playableAsset as TimelineAsset).GetOutputTrack(index).isEmpty);
        }
        public static string GetTransformPath(Transform transform)
        {
            List<string> names = new List<string>();
            Transform focusTrans = transform;
            while (focusTrans != null)
            {
                names.Add(focusTrans.name);
                focusTrans = focusTrans.parent;
            }
            string path = "";
            for (int i = names.Count() - 1; i >= 0; i--)
            {
                path += names[i]; if (i != 0) path += "/";
            }
            return path;
        }


        private static List<GameObject> descendants = new List<GameObject>();
        private static void GatherDescendants(this GameObject from)
        {
            int count = 0;
            while (count < from.transform.childCount)
            {
                GatherDescendants(from.transform.GetChild(count).gameObject);
                descendants.Add(from.transform.GetChild(count).gameObject);
                count++;
            }
        }
        public static List<GameObject> DescendantsList(this GameObject from)
        {
            if (descendants.Count > 0)
            {
                descendants.Clear();
            }
            GatherDescendants(from);
            return descendants;
        }

        private static int descendantDepth = 0;
        public static GameObject DescendantByName(this GameObject from, string name)
        {
            if (from.transform.childCount > 0)
            {
                for (int i = 0; i < from.transform.childCount; i++)
                {
                    GameObject child = from.transform.GetChild(i).gameObject;
                    if (child.name == name)
                    {
                        return child;
                    }
                    descendantDepth++;
                    GameObject grandChild = DescendantByName(child, name);
                    descendantDepth--;
                    if (grandChild != null)
                    {
                        return grandChild;
                    }
                }
            }
            if (descendantDepth == 0) // og object
            {
                Debug.Log("No child with name " + name + " found in " + from.name);
            }
            return null;
        }
    }
}
