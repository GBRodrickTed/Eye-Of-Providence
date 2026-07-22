using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EyeOfProvidence
{
    public enum GlobeSkyboxType
    {
        Limbo = 0,
        Space = 1,
        Theater = 2
    }
    public class GlobeSkybox : MonoBehaviour
    {
        public Camera[] skyboxCams = new Camera[6];
        public RenderTexture[] skyboxTexs = new RenderTexture[6];
        public bool skyboxSetup = false;

        public static GlobeSkybox CreateSkyboxGlobe(Transform target)
        {
            GlobeSkybox skyboxGlobe = GameObject.Instantiate<GameObject>(Plugin.globePref, target).AddComponent<GlobeSkybox>();

            skyboxGlobe.SetupSkyboxCams();

            return skyboxGlobe;
        }
        public void SetupSkyboxCams()
        {
            skyboxCams = GetComponentsInChildren<Camera>();
            skyboxTexs = new RenderTexture[skyboxCams.Length];
            for (int i = 0; i < skyboxCams.Length; i++)
            {
                if (skyboxCams[i])
                {
                    
                    skyboxCams[i].enabled = false;
                    skyboxTexs[i] = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
                    skyboxCams[i].targetTexture = skyboxTexs[i];
                }
            }
            UpdateSkyboxCams();
            RenderSkybox();
            skyboxSetup = true;
        }
        public void DestroySkyboxGlobe()
        {
            Destroy(gameObject);
        }
        public Texture GetSkyboxTexture(int index)
        {
            return skyboxTexs[index];
        }
        public void UpdateSkyboxCams()
        {
            transform.localRotation = Plugin.globeRotation;
            for (int i = 0; i < skyboxCams.Length; i++)
            {
                if (i < Plugin.cams.Length)
                {
                    if (skyboxCams[i] && Plugin.cams[i])
                    {
                        skyboxCams[i].nearClipPlane = Plugin.cams[i].nearClipPlane;
                        skyboxCams[i].farClipPlane = Plugin.cams[i].farClipPlane;
                        skyboxCams[i].cullingMask = Plugin.cams[i].cullingMask;

                        skyboxCams[i].aspect = Plugin.cams[i].aspect;
                        skyboxCams[i].fieldOfView = Plugin.cams[i].fieldOfView;

                        skyboxCams[i].transform.localRotation = Plugin.cams[i].transform.localRotation;
                    }
                }
                else
                {
                    break;
                }
            }
        }
        public void RenderSkybox()
        {
            /*if (skyboxGlobe)
            {
                skyboxGlobe.transform.rotation = CameraController.Instance.cam.transform.rotation;
            }*/
            for (int i = 0; i < skyboxCams.Length; i++)
            {
                if (i < Plugin.cams.Length)
                {
                    if (skyboxCams[i] && Plugin.cams[i])
                    {
                        if (Plugin.cams[i].enabled)
                        {
                            // This is the exact point where it crashes for theater skybox
                            skyboxCams[i].Render();
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}
