using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProssesingBaby : MonoBehaviour
{
    enum CameraFace : int { 
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
    public int debugMode = 0;
    public int fisheyeMode = 0;
    float prevQuality;
    int qualityPixel = 1;
    public FilterMode filterMode = FilterMode.Point;
    private void Start()
    {
        mainCam = GetComponent<Camera>();
        mainCam.depthTextureMode = mainCam.depthTextureMode | DepthTextureMode.DepthNormals;

        qualityPixel = (int)Mathf.Pow(2, quality);
        prevQuality = quality;

        RefreshRenderTextures();
        UpdateCamInfo();
    }
    private void Update()
    {
        
        if (quality != prevQuality)
        {
            quality = Mathf.Clamp(quality, 0, 10);
            prevQuality = quality;
            qualityPixel = (int)Mathf.Pow(2, quality);
            Debug.Log("fefef");
            RefreshRenderTextures();
        }
        for (int i = 0; i < cams.Length; i++)
        {
            
            //cams[i].targetTexture.filterMode = filterMode;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (debugMode == 1)
            {
                debugMode = 0;
            } else
            {
                debugMode = 1;
            }
            postEffectMaterial.SetFloat("_DEBUG", debugMode);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (fisheyeMode == 1)
            {
                fisheyeMode = 0;
            } else
            {
                fisheyeMode = 1;
            }
            postEffectMaterial.SetFloat("_FISHEYE", fisheyeMode);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            UpdateCamInfo();
        }
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
            } else
            {
                Transform target = cams[i].transform;
                Transform parent = target.transform.parent;
                float factorDist = 1;
                float aspectt = 1;
                float scaleX = 1;
                float scaleY = 1f;
                
                // the shader transforms the cartesian cordinates [-1, 1]^2 to uv so the true initial size is technically 2 by 2 and not the expected 1 by 1
                // may change it to be so in the future
                float planeW = 2;
                float planeH = 2;

                //Used for different globes to make sure they are allined properly and maximize their fidelity 
                if (!cams[i].name.Contains("Up") && !cams[i].name.Contains("Down"))
                {
                    //factorDist = 1f/Mathf.Sqrt(3);
                } else
                {
                    //factorX = 1f/2f;
                    
                }
                //factorDist = (1f/(2f*Mathf.Sqrt(3)));

                if (cams[i].name.Contains("Down") || cams[i].name.Contains("Up"))
                {
                    //yt = 1f / 1.15f;
                    //yt = 2;
                }

                if (!cams[i].name.Contains("Back"))
                {
                    //scaleY = 0.5f;
                } else
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

                float fov = Mathf.Atan((planeW/2) / Vector3.Distance(Vector3.zero,pos)) * 2;

                cams[i].aspect = aspect;
                cams[i].fieldOfView = Camera.VerticalToHorizontalFieldOfView(fov * Mathf.Rad2Deg, 1f/cams[i].aspect);

                Debug.Log(cams[i].name + " " + cams[i].aspect + " " + Camera.VerticalToHorizontalFieldOfView(cams[i].fieldOfView, cams[i].aspect));

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
        for (int i = 0; i < 6; i++)
        {
            if (cams[i]) postEffectMaterial.SetTexture("_Cam" + (i + 1), cams[i].targetTexture);
        }

        /*postEffectMaterial.SetTexture("_Cam1", cams[(int)CameraFace.Front].targetTexture);
        postEffectMaterial.SetTexture("_Cam2", cams[(int)CameraFace.Back].targetTexture);
        postEffectMaterial.SetTexture("_Cam3", cams[(int)CameraFace.Left].targetTexture);
        postEffectMaterial.SetTexture("_Cam4", cams[(int)CameraFace.Right].targetTexture);
        postEffectMaterial.SetTexture("_Cam5", cams[(int)CameraFace.Up].targetTexture);
        postEffectMaterial.SetTexture("_Cam6", cams[(int)CameraFace.Down].targetTexture);*/

        
        //
        Graphics.Blit(src, dest, postEffectMaterial);
        //RenderTexture.ReleaseTemporary(rendTex);
    }
}
