using JetBrains.Annotations;
using Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static PostProssesingBaby instance;
        public static ProjectionMode Projection = ProjectionMode.Equirectangular;
        public static float Quality = 9;
        public static Quaternion globeRotation = Quaternion.identity;
        
        public static bool Debug = false;
        public static bool Grid = false;
        public static float GridOpac = 0;
        public static bool Map = false;
        public static float MapOpac = 0;
        public static bool Stretch = true;

        public static float GlobeRotX = 0;
        public static float GlobeRotY = 0;
        public static float GlobeRotZ = 0;

        public static float FisheyeFit = 0;
        public static float PaniniFactor = 1;
        public static float WinkelFit = 0;
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
        public float fov = 360;
        public float quality = 9;
        float prevQuality;

        public bool debugMode = false;
        public bool mapMode = false;
        public float mapOpac = 0.5f;
        public bool gridMode = false;
        public float gridOpac = 0.5f;

        public float globeRotX = 0;
        public float globeRotY = 0;
        public float globeRotZ = 0;

        public bool stretch = true;
        public float fisheyeFit = 0;
        public float paniniFactor = 1;
        public float winkelFit = 0;
        
        public int projectionMode = 0;
        
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
            projectionMode = (int)Projection;
            stretch = Stretch;

            debugMode = Debug;
            gridOpac = GridOpac;
            mapOpac = MapOpac;

            globeRotX = GlobeRotX;
            globeRotY = GlobeRotY;
            globeRotZ = GlobeRotZ;

            fisheyeFit = FisheyeFit;
            paniniFactor = PaniniFactor;
            winkelFit = WinkelFit;

            postEffectMaterial.SetFloat("_FOV", fov);
            postEffectMaterial.SetFloat("_MODE", projectionMode);
            postEffectMaterial.SetFloat("_STRETCH", stretch ? 1f : 0);
            postEffectMaterial.SetFloat("_ASPECT", !stretch ? ((float)Screen.width / (float)Screen.height) : 1f);
            postEffectMaterial.SetFloat("_FISHEYE_EQUIDIST_FIT", fisheyeFit);
            postEffectMaterial.SetFloat("_PANINI_FACTOR", paniniFactor);
            postEffectMaterial.SetFloat("_WINKEL_FIT", winkelFit);

            postEffectMaterial.SetFloat("_DEBUG", debugMode ? 1 : 0);
            postEffectMaterial.SetFloat("_GRID", gridMode ? 1 : 0);
            postEffectMaterial.SetFloat("_MAP", mapMode ? 1 : 0);
            postEffectMaterial.SetFloat("_GRID_FACTOR", gridOpac);
            postEffectMaterial.SetFloat("_MAP_FACTOR", mapOpac);

            //UpdateCamInfo();
            UpdateGlobeRotations();

            instance = this;

            RefreshRenderTextures();
        }

        private void SetGlobeRotations(float yaw, float pitch, float roll)
        {
            globeRotation = Quaternion.Euler(roll * 180, pitch * 180, yaw * 180);
            Matrix4x4 rot = Matrix4x4.Rotate(globeRotation);
            Plugin.instance.UpdateGlobe();
            //rot.SetRow(0, new Vector4(Mathf.Cos(yaw), -Mathf.Sin(yaw), 0));
            //rot.SetRow(1, new Vector4(Mathf.Sin(yaw), Mathf.Cos(yaw), 0));
            //rot.SetRow(2, new Vector4(0, 0, 1));
            //postEffectMaterial.SetMatrix("_yaw", rot);

            //rot.SetRow(0, new Vector4(Mathf.Cos(pitch), 0, Mathf.Sin(pitch)));
            //rot.SetRow(0, new Vector4(0, 1, 0));
            //rot.SetRow(0, new Vector4(-Mathf.Sin(pitch), 0, Mathf.Cos(pitch)));
            //postEffectMaterial.SetMatrix("_pitch", rot);

            //rot.SetRow(0, new Vector4(1, 0, 0));
            //rot.SetRow(0, new Vector4(0, Mathf.Cos(roll), -Mathf.Sin(roll)));
            //rot.SetRow(0, new Vector4(0, Mathf.Sin(roll), Mathf.Cos(roll)));
            //postEffectMaterial.SetMatrix("_roll", rot);

            postEffectMaterial.SetMatrix("_rotMatrix", rot);
            postEffectMaterial.SetFloat("_GLOBE_ROTATE", 1);
        }
        public void UpdateGlobeRotations()
        {
            SetGlobeRotations(globeRotZ, globeRotX, globeRotY);
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

            if (globeRotX != GlobeRotX)
            {
                globeRotX = GlobeRotX;
                SetGlobeRotations(globeRotZ, globeRotX, globeRotY);
            }

            if (globeRotY != GlobeRotY)
            {
                globeRotY = GlobeRotY;
                SetGlobeRotations(globeRotZ, globeRotX, globeRotY);
            }

            if (globeRotZ != GlobeRotZ)
            {
                globeRotZ = GlobeRotZ;
                SetGlobeRotations(globeRotZ, globeRotX, globeRotY);
            }

            if (FisheyeFit != fisheyeFit)
            {
                fisheyeFit = FisheyeFit;
                postEffectMaterial.SetFloat("_FISHEYE_EQUIDIST_FIT", fisheyeFit);
            }

            if (paniniFactor != PaniniFactor)
            {
                paniniFactor = PaniniFactor;
                postEffectMaterial.SetFloat("_PANINI_FACTOR", paniniFactor);
            }

            if (winkelFit != WinkelFit)
            {
                winkelFit = WinkelFit;
                postEffectMaterial.SetFloat("_WINKEL_FIT", winkelFit);
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

            if (projectionMode != (int)Projection)
            {
                projectionMode = (int)Projection;
                postEffectMaterial.SetFloat("_MODE", projectionMode);
                width = Screen.width;
                height = Screen.height;
                prevWidth = width;
                prevHeight = height;
                /*if (mode == (int)PerspectiveMode.Panini)
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
                }*/
                RefreshRenderTextures();
            }
            
        }
        public void RefreshFOV()
        {
            
        }
        public void UpdateCamInfo()
        {
            // self note: in the shader, planes are 2 by 2
            for (int i = 0; i < 6; i++)
            {
                if (!cams[i])
                {
                    Vector3 pos = new Vector3(30000000, 3000000, 300000);
                    Vector3 forward = pos;
                    Vector3 up = pos;
                    Vector3 right = pos;

                    List<Vector4> info = new List<Vector4>{
                    pos,
                    forward,
                    up,
                    right
                };

                    postEffectMaterial.SetVectorArray("_Cam" + (i + 1) + "_Info", info);
                }
                else
                {
                    Transform target = cams[i].transform;
                    Transform parent = target.transform.parent;
                    float factorDist = 1;
                    float aspectt = 1;
                    float scaleX = 1;
                    float scaleY = 1f;

                    float planeW = 2;
                    float planeH = 2;

                    if (!cams[i].name.Contains("Up") && !cams[i].name.Contains("Down"))
                    {
                        factorDist = 1f / Mathf.Sqrt(3);
                    }
                    else
                    {
                        //factorX = 1f/2f;

                    }
                    //factorDist = (1f / (2f * Mathf.Sqrt(3)));
                    factorDist = 1;

                    if (cams[i].name.Contains("Down") || cams[i].name.Contains("Up"))
                    {
                        //yt = 1f / 1.15f;
                        //yt = 2;
                    }

                    if (!cams[i].name.Contains("Back"))
                    {
                        //scaleY = 0.5f;
                    }
                    else
                    {
                    }
                    //yt = 0.5f;
                    //xt = 0.5f;

                    float yt = 1f / scaleY;
                    float xt = 1f / scaleX;

                    planeW *= (1f / xt);
                    planeH *= (1f / yt);
                    aspectt = planeW / planeH;

                    Vector3 pos = parent.InverseTransformDirection(target.transform.forward) * factorDist;
                    Vector3 forward = parent.InverseTransformDirection(target.transform.forward);
                    Vector3 up = parent.InverseTransformDirection(target.transform.up) * yt;
                    Vector3 right = parent.InverseTransformDirection(target.transform.right) * xt;

                    float aspect = aspectt;

                    float fov = Mathf.Atan((planeW / 2) / Vector3.Distance(Vector3.zero, pos)) * 2;

                    cams[i].aspect = aspect;
                    cams[i].fieldOfView = Camera.VerticalToHorizontalFieldOfView(fov * Mathf.Rad2Deg, 1f / cams[i].aspect);

                    //Debug.Log(cams[i].name + " " + cams[i].aspect + " " + Camera.VerticalToHorizontalFieldOfView(cams[i].fieldOfView, cams[i].aspect));

                    List<Vector4> info = new List<Vector4>{
                    pos,
                    forward,
                    up,
                    right
                };

                    postEffectMaterial.SetVectorArray("_Cam" + (i + 1) + "_Info", info);
                }

            }
        }
        public void UpdateCamInfo(CamInfo[] infos)
        {
            Vector3 shutUp = new Vector3(1000000, 1000000, 1000000);
            for (int i = 0; i < cams.Length; i++)
            {
                if (i >= infos.Length)
                {
                    postEffectMaterial.SetVectorArray("_Cam" + (i + 1) + "_Info", new List<Vector4> { shutUp , shutUp , shutUp , shutUp });
                    continue;
                }
                CamInfo info = infos[i];
                Vector2 scale = info.scale;
                Vector2 invScale = new Vector2(1f / scale.x, 1f / scale.y);

                Vector3 forward = info.camRot * Vector3.forward;
                Vector3 pos = forward * info.posDist;
                Vector3 right = info.camRot * Vector3.right * invScale.x;
                Vector3 up = info.camRot * Vector3.up * invScale.y;

                float planeW = 2;
                float planeH = 2;

                planeW *= (1f / invScale.x);
                planeH *= (1f / invScale.y);
                float aspect = planeW / planeH;

                float fov = Mathf.Atan((planeW / 2) / Vector3.Distance(Vector3.zero, pos)) * 2;

                cams[i].transform.localRotation = info.camRot;
                cams[i].aspect = aspect;
                cams[i].fieldOfView = Camera.VerticalToHorizontalFieldOfView(fov * Mathf.Rad2Deg, 1f / aspect);

                List<Vector4> gpuInfo = new List<Vector4>{
                    pos,
                    forward,
                    up,
                    right
                };

                postEffectMaterial.SetVectorArray("_Cam" + (i + 1) + "_Info", gpuInfo);
            }
        }

        public void SetRayMethod(int rayMethod)
        {
            postEffectMaterial.SetInt("_RAY_METHOD", rayMethod);
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

            //Graphics.Blit(cams[(int)CameraFace.Front].targetTexture, dest);
            //Matrix4x4 viewToWorld = mainCam.cameraToWorldMatrix;

            //postEffectMaterial.SetMatrix("_viewToWorld", viewToWorld);

            for (int i = 0; i < 6; i++)
            {
                if (cams[i]) postEffectMaterial.SetTexture("_Cam" + (i + 1), cams[i].targetTexture);
            }

            Graphics.Blit(src, dest, postEffectMaterial);
            //RenderTexture.ReleaseTemporary(rendTex);
        }

    }
}
