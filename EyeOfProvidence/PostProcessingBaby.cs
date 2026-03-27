using Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Utilities;
using UnityEngine.UIElements;

namespace EyeOfProvidence
{
    public class PostProssesingBaby : MonoBehaviour
    {
        public static float PlayerFOV = 360;
        public static PerspectiveMode Perspective = PerspectiveMode.Equirectangular;
        public static float Quality = 9;
        
        public static bool Debug = false;
        public static bool Grid = false;
        public static float GridOpac = 0;
        public static bool Map = false;
        public static float MapOpac = 0;
        public static bool Stretch = true;

        public static float StereoFactor = 0;
        public static float FisheyeFit = 0;
        public static float PaniniFactor = 1;
        enum CameraFace : int
        {
            Front = 0,
            Back = 1,
            Left = 2,
            Right = 3,
            Up = 4,
            Down = 5
        };
        public Camera mainCam;
        Shader postShader;
        public Material postEffectMaterial;
        public Camera[] cams;
        List<RenderTexture> camTexs = new List<RenderTexture>();
        public float quality = 9;
        public bool debugMode = false;
        public bool mapMode = false;
        public float mapOpac = 0.5f;
        public bool gridMode = false;
        public float gridOpac = 0.5f;
        public bool stretch = true;
        public float fisheyeFit = 0;
        public float stereoFactor = 0;
        public float paniniFactor = 1;
        public float fov = 360;
        public int mode = 0;
        float prevQuality;
        int qualityPixel = 1;
        public float aspect = 1;
        public float prevAspect = 1;
        float width = 2;
        float height = 1;
        float prevWidth = 0;
        float prevHeight = 0;
        public FilterMode filterMode = FilterMode.Point;
        private void Start()
        {
            mainCam = GetComponent<Camera>();
            mainCam.depthTextureMode = mainCam.depthTextureMode | DepthTextureMode.DepthNormals;

            quality = Quality;
            qualityPixel = (int)Mathf.Pow(2, quality);
            prevQuality = quality;

            fov = PlayerFOV;
            mode = (int)Perspective;
            stretch = Stretch;

            debugMode = Debug;
            gridOpac = GridOpac;
            mapOpac = MapOpac;

            fisheyeFit = FisheyeFit;
            paniniFactor = PaniniFactor;

            postEffectMaterial.SetFloat("_FOV", fov);
            postEffectMaterial.SetFloat("_MODE", mode);
            postEffectMaterial.SetFloat("_STRETCH", stretch ? 1f : 0);
            postEffectMaterial.SetFloat("_ASPECT", !stretch ? ((float)Screen.width / (float)Screen.height) : 1f);
            postEffectMaterial.SetFloat("_FISHEYE_STEREO_FACTOR", stereoFactor);
            postEffectMaterial.SetFloat("_FISHEYE_FIT", fisheyeFit);
            postEffectMaterial.SetFloat("_PANINI_FACTOR", paniniFactor);

            postEffectMaterial.SetFloat("_DEBUG", debugMode ? 1 : 0);
            postEffectMaterial.SetFloat("_GRID", gridMode ? 1 : 0);
            postEffectMaterial.SetFloat("_MAP", mapMode ? 1 : 0);
            postEffectMaterial.SetFloat("_GRID_FACTOR", gridOpac);
            postEffectMaterial.SetFloat("_MAP_FACTOR", mapOpac);

            RefreshRenderTextures();
        }
        private void Update()
        {
            if (PlayerFOV != fov)
            {
                fov = PlayerFOV;
                postEffectMaterial.SetFloat("_FOV", fov);
            }
            quality = Quality;
            if (quality != prevQuality)
            {
                quality = Mathf.Clamp(quality, 0, 10);
                prevQuality = quality;
                qualityPixel = (int)Mathf.Pow(2, quality);
                //Debug.Log("fefef");
                RefreshRenderTextures();
            }
            
            if (debugMode != Debug)
            {
                debugMode = Debug;
                postEffectMaterial.SetFloat("_DEBUG", debugMode ? 1 : 0);
            }

            if (gridMode != Grid)
            {
                gridMode = Grid;
                postEffectMaterial.SetFloat("_GRID", gridMode ? 1 : 0);
            }

            if (gridOpac != GridOpac)
            {
                gridOpac = GridOpac;
                postEffectMaterial.SetFloat("_GRID_FACTOR", gridOpac);
            }

            if (mapMode != Map)
            {
                mapMode = Map;
                postEffectMaterial.SetFloat("_MAP", mapMode ? 1 : 0);
            }

            if (mapOpac != MapOpac)
            {
                mapOpac = MapOpac;
                postEffectMaterial.SetFloat("_MAP_FACTOR", mapOpac);
            }

            if (stereoFactor != StereoFactor)
            {
                stereoFactor = StereoFactor;
                postEffectMaterial.SetFloat("_FISHEYE_STEREO_FACTOR", stereoFactor);
            }

            if (FisheyeFit != fisheyeFit)
            {
                fisheyeFit = FisheyeFit;
                postEffectMaterial.SetFloat("_FISHEYE_FIT", fisheyeFit);
            }

            if (paniniFactor != PaniniFactor)
            {
                paniniFactor = PaniniFactor;
                postEffectMaterial.SetFloat("_PANINI_FACTOR", paniniFactor);
            }

            if (stretch != Stretch)
            {
                stretch = Stretch;
                width = Screen.width;
                height = Screen.height;
                aspect = stretch ? 1f : ((float)Screen.width / (float)Screen.height);
                postEffectMaterial.SetFloat("_STRETCH", stretch ? 1f : 0);
                postEffectMaterial.SetFloat("_ASPECT", aspect);
                prevWidth = width;
                prevHeight = height;
            }

            if (mode != (int)Perspective)
            {
                mode = (int)Perspective;
                postEffectMaterial.SetFloat("_MODE", mode);
                width = Screen.width;
                height = Screen.height;
                prevWidth = width;
                prevHeight = height;
                if (mode == (int)PerspectiveMode.Panini)
                {
                    if (cams[(int)CameraFace.Back] != null)
                    {
                        cams[(int)CameraFace.Back].gameObject.SetActive(false);
                    }
                } else
                {
                    if (cams[(int)CameraFace.Back] != null)
                    {
                        cams[(int)CameraFace.Back].gameObject.SetActive(true);
                    }
                }
                RefreshRenderTextures();
            }
            
        }
        public void RefreshFOV()
        {
            
        }
        public void RefreshRenderTextures()
        {
            for (int i = 0; i < cams.Length; i++)
            {
                if (cams[i])
                {
                    if (cams[i].targetTexture != null)
                    {
                        cams[i].targetTexture.Release();
                    }
                    RenderTexture rendTex = new RenderTexture(qualityPixel, qualityPixel, 24);
                    rendTex.filterMode = filterMode;
                    cams[i].targetTexture = rendTex;
                }
            }
        }
        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            /*if (postEffectMaterial == null)
            {
                postEffectMaterial = new Material(postShader);
            }*/
            //RenderTexture rendTex = RenderTexture.GetTemporary(
            /*src.width,
            src.height,
            src.depth,
            src.format);*/

            //Graphics.Blit(cam1.targetTexture, cam1Tex);
            Matrix4x4 viewToWorld = mainCam.cameraToWorldMatrix;

            postEffectMaterial.SetMatrix("_viewToWorld", viewToWorld);

            postEffectMaterial.SetTexture("_Cam_Front", cams[(int)CameraFace.Front].targetTexture);
            postEffectMaterial.SetTexture("_Cam_Back", cams[(int)CameraFace.Back].targetTexture);
            postEffectMaterial.SetTexture("_Cam_Left", cams[(int)CameraFace.Left].targetTexture);
            postEffectMaterial.SetTexture("_Cam_Right", cams[(int)CameraFace.Right].targetTexture);
            postEffectMaterial.SetTexture("_Cam_Up", cams[(int)CameraFace.Up].targetTexture);
            postEffectMaterial.SetTexture("_Cam_Down", cams[(int)CameraFace.Down].targetTexture);


            Graphics.Blit(src, dest, postEffectMaterial);
            //RenderTexture.ReleaseTemporary(rendTex);
        }

    }
}
