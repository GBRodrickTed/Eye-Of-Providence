using System;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System.Security.Cryptography;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Collections;
using System.Net;
using UnityEngine.Audio;
using BepInEx.Bootstrap;
using PluginConfig;
using UnityEngine.Rendering;
using ULTRAKILL.Portal;
using System.Threading;
using UnityEngine.UIElements;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

// What needs to be done:
// Blood/Gasoline Rendering - This is currently the most doable objective as it is not tangled within the lovecraftian terrors that are portals
// Portal Rendering - Second most doable objective just by virtue of it's logic being exposed.
// Fix Portals Clipping Cameras - For reasons beyond reason, if the main camera is looking in the direction of a portal, all other cameras in
// the observable universe are bound to only look in that general direction. Practically this means the max fov is arbitrarally locked to 180
// and any geometry outside it gets clipped. I have yet to find where this takes place so is not fixable for the time being.

// What can be done:
// Optimizations - There's room in the shader to optimize. The cameras cause the main preformance hit. Can give option to disable certain
// cameras. Could also create globes using less cameras, though this would require a rewrite of the globe logic

namespace EyeOfProvidence
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency(PluginConfiguratorController.PLUGIN_GUID)]
    public class Plugin : BaseUnityPlugin
    {

        /*public static TrashCan<GameObject> spawner { get; private set; } = new TrashCan<GameObject>("block_main");
        public static UKAsset<GameObject> UK_Boom { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Attacks and Projectiles/Coin.prefab");
        public static AssetBundle UK_Prefabs;*/
        Harmony harmony = new Harmony(PluginInfo.GUID);
        public static Plugin instance;
        AssetBundle bundle;
        GameObject globePref;
        public static Camera[] cams = new Camera[6]; // 1-Front 2-Back 3-Left 4-Right 5-Up 6-Down
        static GameObject globe;
        public static float GlobeRotX = 0;
        public static float GlobeRotY = 0;
        public static float GlobeRotZ = 0;
        public static PostProssesingBaby post;
        public static Quaternion globeRotation = Quaternion.identity;
        public static int GlobeMask = int.MaxValue;
        public int globeMask = 0;
        Material mat;
        RenderBuffer[] chumBuckets = new RenderBuffer[3];
        public static RenderTexture tex;
        static RenderTexture tempTex;
        static RenderTexture tex2;
        public static Texture texst;
        public static Mesh meshTest;
        public static Material meshMat;
        public static bool UltraFOV = true;

        public static bool test = false;
        
        public CommandBuffer bloodOilCB;
        public void Awake()
        {

        }

        public void Start()
        {
            Debug.Log("Reddit, I think I know what we're gonna do today!");

            AssetHandler.LoadBundle("ultra_nato");

            meshTest = AssetHandler.LoadAsset<GameObject>("default").GetComponent<MeshFilter>().mesh;
            meshMat = AssetHandler.LoadAsset<GameObject>("default").GetComponent<MeshRenderer>().material;

            globePref = AssetHandler.LoadAsset<GameObject>("GlobeCam");
            mat = AssetHandler.LoadAsset<Material>("coolMat");
            texst = AssetHandler.LoadAsset<Texture>("Gun Color Small");
            harmony.PatchAll(typeof(XboxGamePass));
            ConfigManager.Setup();

            instance = this;

            globeMask = GlobeMask;
        }

        public void Update()
        {
            if (UltraFOV)
            {
                //CameraController.Instance.cam.enabled = false;
                if (CameraController.Instance)
                {
                    //Debug.LogError(CameraController.Instance.cam.targetTexture.name);
                    if (CameraController.Instance.cam)
                    {
                        if (!globe)
                        {
                            SetupGlobe();
                        }
                    }
                }
            }
            else
            {
                if (globe)
                {
                    if (CameraController.Instance.cam.targetTexture)
                    {
                        //Debug.LogError(CameraController.Instance.cam.targetTexture.name);
                    }
                    ClearGlobe();
                }
            }


            UpdateGlobe();

            /*if (Input.GetKeyUp(KeyCode.M))
            {
                test = !test;
            }*/
            ConfigManager.Update();
        }
        public void GlobeMaskCheck()
        {
            globeMask = GlobeMask;
            int mask = globeMask;
            for (int i = 0; i < cams.Length; i++)
            {
                cams[i].enabled = (mask % 2 == 1);
                mask = mask >> 1;
            }
        }
        public void UpdateGlobe()
        {
            if (globe)
            {
                if (globeMask != GlobeMask)
                {
                    GlobeMaskCheck();
                    ClearGlobe();
                    SetupGlobe();
                }

                /*globe.transform.position = CameraController.Instance.cam.transform.position;
                globe.transform.rotation = CameraController.Instance.cam.transform.rotation;
                globe.transform.rotation *= Quaternion.Inverse(PostProssesingBaby.globeRotation);*/

                if (globeRotation != PostProssesingBaby.globeRotation)
                {
                    globe.transform.localRotation = Quaternion.Euler(0, 0, 1) * Quaternion.Inverse(PostProssesingBaby.globeRotation);
                    //globeRotation = PostProssesingBaby.globeRotation;
                    //globe.transform.rotation = Quaternion.Euler(0, 0, 1) * Quaternion.Inverse(PostProssesingBaby.globeRotation);
                }
                
                //globe.transform.rotation *= Quaternion.Euler(GlobeRotY * -180, GlobeRotX * -180, GlobeRotZ * -180);
                //Vector3 rot = CameraController.Instance.gameObject.transform.rotation.eulerAngles - new Vector3(GlobeRotY * 90, GlobeRotX * 180, 0);// + new Vector3(GlobeRotX, GlobeRotY, 0)
                //globe.transform.rotation = Quaternion.Euler(rot);
                if (tex)
                {
                    if (post.mainCam.targetTexture != tex)
                    {
                        post.mainCam.targetTexture = tex;
                    }
                }
            }
        }
        public void SetupGlobe()
        {
            //Debug.LogError("Initializing Globe");
            XboxGamePass.reinitTex = true;
            PostProcessV2_Handler.Instance.reinitializeTextures = true;
            PostProcessV2_Handler.Instance.SetupRTs();

            globe = Instantiate<GameObject>(globePref, CameraController.Instance.cam.transform);
            //globe = Instantiate<GameObject>(globePref, CameraController.Instance.cam.transform);

            for (int i = 0; i < cams.Length; i++)
            {
                cams[i] = globe.transform.GetChild(i).GetComponent<Camera>();
                cams[i].nearClipPlane = CameraController.Instance.cam.nearClipPlane;
                cams[i].farClipPlane = CameraController.Instance.cam.farClipPlane;
                cams[i].cullingMask = CameraController.Instance.cam.cullingMask;
            }
            GlobeMaskCheck();
            //post = CameraController.Instance.gameObject.AddComponent<PostProssesingBaby>();
            //post.mainCam = cams[0];//CameraController.Instance.cam.transform.Find("HUD Camera").GetComponent<Camera>();
            post = CameraController.Instance.cam.transform.gameObject.AddComponent<PostProssesingBaby>();
            //post = CameraController.Instance.hudCamera.gameObject.AddComponent<PostProssesingBaby>();
            post.mainCam = post.GetComponent<Camera>();
            //tempTex = post.mainCam.targetTexture;
            post.mainCam.targetTexture = tex;
            //post.mainCam.targetTexture = tex;
            //CameraController.Instance.hudCamera.targetTexture = tex;

            post.postEffectMaterial = mat;
            post.cams = cams;

            globe.transform.localPosition = Vector3.zero;
            globe.transform.localRotation = Quaternion.Euler(0, 0, 1);
        }

        public void ClearGlobe()
        {
            //Debug.LogError("Clearing Globe");
            if (globe)
            {
                GameObject.Destroy(globe);
            }
            globe = null;


            for (int i = 0; i < cams.Length; i++)
            {
                cams[i] = null;
            }

            post.mainCam.targetTexture = PostProcessV2_Handler.Instance.mainTex;

            //post.mainCam.targetTexture = null;
            XboxGamePass.reinitTex = false;
            PostProcessV2_Handler.Instance.reinitializeTextures = true;
            PostProcessV2_Handler.Instance.SetupRTs();

            if (CameraController.Instance.gameObject.TryGetComponent<PostProssesingBaby>(out PostProssesingBaby bab))
            {
                GameObject.Destroy(bab);
            }
        }


        [HarmonyPatch]
        public static class XboxGamePass
        {
            public static bool reinitTex = UltraFOV;
            public static bool renderCam = false;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(PostProcessV2_Handler), nameof(PostProcessV2_Handler.OnPreRenderCallback))]
            public static void FUUUUUUUU(PostProcessV2_Handler __instance)
            {

            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(PostProcessV2_Handler), nameof(PostProcessV2_Handler.HeatWaves))]
            public static void skinitsundermyskinoffpeelitoffskinoffpeeloffskin(PostProcessV2_Handler __instance)
            {
                // I know what needs to be done. Materials like heatwave, blood, etc. use the player camera matrix as a global parameter
                // So if I want to properly render the scene, I would need to constantly cycle the global matrix between the six cameras and cordinate that every frame
                // It seems doable* with command buffers, but I'll leave it for future me to solve.
                // Also performance will most likely be abysmal but that's just the life of a family guy


            }
            [HarmonyPostfix]
            [HarmonyPatch(typeof(PostProcessV2_Handler), nameof(PostProcessV2_Handler.ReleaseTextures))]
            public static void DisbandTrollLegion(PostProcessV2_Handler __instance)
            {
                if (tex)
                {
                    tex.Release();
                }
                if (tex)
                {
                    UnityEngine.Object.Destroy(tex);
                }
                tex = null;
            }
            [HarmonyPostfix]
            [HarmonyPatch(typeof(PostProcessV2_Handler), nameof(PostProcessV2_Handler.ChangeCamera))]
            public static void remember(PostProcessV2_Handler __instance)
            {
                if (UltraFOV)
                {
                    //Debug.LogError("*burps covertly*");
                    reinitTex = true;
                }
            }
            /*[HarmonyPrefix]
            [HarmonyPatch(typeof(PortalRenderV2), nameof(PortalRenderV2.Render))]
            public static bool lal(PortalRenderV2 __instance)
            {
                //return !test;
                if (UltraFOV)
                {
                    if (post)
                    {
                        if (__instance.mainCam == post.mainCam)
                        {
                            for (int i = 0; i < cams.Length; i++)
                            {
                                if (cams[i] != null)
                                {
                                    __instance.Render(cams[i]);
                                }
                            }
                        }
                    }
                }
                return true;
            }*/

            [HarmonyPostfix]
            [HarmonyPatch(typeof(PostProcessV2_Handler), nameof(PostProcessV2_Handler.SetupRTs))]
            public static void rage(PostProcessV2_Handler __instance)
            {
                //return;
                /*if (Plugin.tex)
                {
                    __instance.hudCam.SetTargetBuffers(Plugin.tex.colorBuffer, Plugin.tex.depthBuffer);
                    __instance.postProcessV2_VSRM.SetTexture("_MainTex", texst);
                }*/
                //tex = (RenderTexture)texst;
                if (!tex)
                {
                    /*tex = new RenderTexture(__instance.width, __instance.height, 0, RenderTextureFormat.ARGB32)
                    {
                        name = "Main",
                        antiAliasing = 1,
                        filterMode = FilterMode.Point
                    };*/
                    //tex = (RenderTexture)texst;
                    /*tex = (RenderTexture)texst;
                    tex.depth = 0;
                    tex.format = RenderTextureFormat.ARGB32;
                    tex.filterMode = FilterMode.Point;
                    tex.antiAliasing = 1;*/

                }
                if (tex)
                {
                    //__instance.mainCam.targetTexture = tex;
                    //Graphics.Blit(texst, tex);
                }

                //Debug.LogError("fef");
                /*__instance.buffers[0] = __instance.mainTex.colorBuffer;
                __instance.buffers[1] = __instance.reusableBufferA.colorBuffer;
                __instance.buffers[2] = __instance.viewNormal.colorBuffer;
                __instance.mainCam.SetTargetBuffers(__instance.buffers, __instance.depthBuffer.depthBuffer);
                __instance.mainCam.RemoveCommandBuffers(CameraEvent.AfterForwardAlpha);
                __instance.SetupOutlines(false);
                __instance.hudCam.SetTargetBuffers(__instance.mainTex.colorBuffer, __instance.depthBuffer.depthBuffer);*/

                //Graphics.Blit(texst, tex);
                //Graphics.Blit(texst, tex);
                bool flag = __instance.width != __instance.lastWidth || __instance.height != __instance.lastHeight;
                if (reinitTex || flag)
                {
                    //Debug.LogError("Ohio perplexed");
                    tex = new RenderTexture(__instance.width, __instance.height, 0, RenderTextureFormat.ARGB32)
                    {
                        name = "Xx_TrollLegion_xX",
                        antiAliasing = 1,
                        filterMode = FilterMode.Point
                    };

                    Graphics.Blit(texst, tex);


                    __instance.mainTex = tex;
                    __instance.buffers[0] = __instance.mainTex.colorBuffer;
                    __instance.buffers[1] = __instance.reusableBufferA.colorBuffer;
                    __instance.buffers[2] = __instance.viewNormal.colorBuffer;
                    //__instance.mainCam.SetTargetBuffers(__instance.buffers, __instance.depthBuffer.depthBuffer);
                    //__instance.mainCam.RemoveCommandBuffers(CameraEvent.AfterForwardAlpha);
                    __instance.SetupOutlines(false);

                    __instance.hudCam.SetTargetBuffers(__instance.mainTex.colorBuffer, __instance.depthBuffer.depthBuffer);

                    //Graphics.Blit(texst, tex);
                    //Graphics.Blit(__instance.mainTex, tex);
                    __instance.postProcessV2_VSRM.SetTexture("_MainTex", __instance.mainTex);

                    reinitTex = false;
                }

                /*__instance.mainCam.SetTargetBuffers(Plugin.tex.colorBuffer, Plugin.tex.depthBuffer);
                //__instance.mainCam.RemoveCommandBuffers(CameraEvent.AfterForwardAlpha);
                __instance.hudCam.SetTargetBuffers(Plugin.tex.colorBuffer, Plugin.tex.depthBuffer);
                __instance.postProcessV2_VSRM.SetTexture("_MainTex", tex);*/

            }
        }
    }
}
