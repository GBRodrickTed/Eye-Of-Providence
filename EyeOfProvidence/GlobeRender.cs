using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

namespace EyeOfProvidence
{
    public class GlobeRender : MonoBehaviour
    {
        public CommandBuffer bloodOilCB;
        public Camera mainCam;

        /*BloodsplatterManager bsm;
        StainVoxelManager svm;
        PostProcessV2_Handler pph;*/
        Mesh mesh;
        public void Awake()
        {
            Init();
        }
        public void Init()
        {
            if (this.bloodOilCB == null)
            {
                this.bloodOilCB = new CommandBuffer();
                this.bloodOilCB.name = "Globe Blood and Oil";
                Plugin.cams[0].AddCommandBuffer(CameraEvent.AfterEverything, bloodOilCB);
                
                //CameraEvent.BeforeForwardAlph
            }
            Camera.onPostRender += OnPreRenderCallback;
        }
        public void Render(Camera cam)
        {
            BloodsplatterManager bsm = BloodsplatterManager.Instance;
            StainVoxelManager svm = StainVoxelManager.Instance;
            PostProcessV2_Handler pph = PostProcessV2_Handler.Instance;//

            mainCam = cam;
            if (bsm.usedComputeShadersAtStart)
            {
                bsm.stainMat.SetBuffer("instanceBuffer", bsm.instanceBuffer);
                bsm.stainMat.SetBuffer("parentBuffer", bsm.parentBuffer);
                svm.gasStainMat.SetBuffer("instanceBuffer", svm.propBuffer);
                svm.gasStainMat.SetBuffer("stainMatrices", svm.stainBuffer);
            }
            bloodOilCB.Clear();
            Matrix4x4 inverseVP_NonOblique = (GL.GetGPUProjectionMatrix(pph.mainCam.projectionMatrix, true) * pph.mainCam.worldToCameraMatrix).inverse;
            /*this.bloodOilCB.SetGlobalMatrix("_InverseVP", inverseVP_NonOblique);
            this.bloodOilCB.SetGlobalVector("_ProjectionParams_Oblique", PostProcessV2_Handler.GetProjectionParams(pph.mainCam));
            this.bloodOilCB.SetGlobalMatrix("_InvProjection_Oblique", pph.mainCam.projectionMatrix.inverse);
            this.bloodOilCB.SetGlobalMatrix("_InverseView", pph.mainCam.worldToCameraMatrix);
            this.bloodOilCB.SetGlobalTexture("_DepthBuffer", pph.depthBuffer);
            this.bloodOilCB.SetGlobalInteger("_FixOblique", 1);*/

            /*this.bloodOilCB.SetGlobalTexture("_WorldNormal", pph.viewNormal.colorBuffer);
            this.bloodOilCB.SetGlobalTexture("_BloodstainTex", pph.reusableBufferB.colorBuffer);
            this.bloodOilCB.SetGlobalTexture("_OilStainTex", pph.reusableBufferB.colorBuffer);
            this.bloodOilCB.SetGlobalVector("_ZBufferParams_NonOblique", PostProcessV2_Handler.ZBufferParams(this.mainCam));*/

            //this.bloodOilCB.SetRenderTarget(mainCam.targetTexture);
            //bloodOilCB.ClearRenderTarget(true, true, Color.clear);
            //bloodOilCB.Blit(pph.mainTex, mainCam.targetTexture);

            // Why isnt this working? Is there something im missing? must more blood be shed?

            this.bloodOilCB.SetRenderTarget(pph.reusableBufferB.colorBuffer);
            this.bloodOilCB.ClearRenderTarget(false, true, Color.clear);

            if (bsm.usedComputeShadersAtStart)
            {
                this.bloodOilCB.DrawMeshInstancedIndirect(bsm.optimizedBloodMesh, 0, bsm.stainMat, 0, bsm.argsBuffer, 0, null);
            }
            else
            {
                this.bloodOilCB.DrawMesh(bsm.totalStainMesh, Matrix4x4.identity, bsm.stainMat);
            }
            this.bloodOilCB.SetRenderTarget(mainCam.targetTexture.colorBuffer);
            this.bloodOilCB.DrawProcedural(PostProcessV2_Handler.identityMatrix, bsm.bloodCompositeMaterial, 1, MeshTopology.Triangles, 3, 1);
            //bloodOilCB.Blit(pph.mainTex, mainCam.targetTexture);
            //this.bloodOilCB.DrawProcedural(PostProcessV2_Handler.identityMatrix, bsm.bloodCompositeMaterial, 1, MeshTopology.Triangles, 3, 1);
            /*if (this.usedComputeShadersAtStart)
            {
                this.bloodOilCB.SetRenderTarget(this.reusableBufferB.colorBuffer);
                this.bloodOilCB.ClearRenderTarget(false, true, new Color(1f, 1f, 1f, 0f));
                this.bloodOilCB.DrawMeshInstancedIndirect(instance2.gasStainMesh, 0, instance2.gasStainMat, 0, instance2.argsBuffer, 0, null);
                this.bloodOilCB.SetRenderTarget(this.mainTex.colorBuffer);
                this.bloodOilCB.DrawProcedural(PostProcessV2_Handler.identityMatrix, instance2.gasolineCompositeMaterial, 1, MeshTopology.Triangles, 3, 1);
            }*/

            this.bloodOilCB.DrawMesh(Plugin.meshTest, Matrix4x4.identity, Plugin.meshMat);
            //this.bloodOilCB.ClearRenderTarget(false, true, Color.clear);
            /*this.bloodOilCB.SetViewProjectionMatrices(mainCam.worldToCameraMatrix, mainCam.projectionMatrix);
            if (bsm.usedComputeShadersAtStart)
            {
                this.bloodOilCB.DrawMeshInstancedIndirect(bsm.optimizedBloodMesh, 0, bsm.stainMat, 0, bsm.argsBuffer, 0, null);
            }
            else
            {
                this.bloodOilCB.DrawMesh(bsm.totalStainMesh, Matrix4x4.identity, bsm.stainMat);
            }*/
            /*if (bsm.usedComputeShadersAtStart)
            {
                this.bloodOilCB.DrawMeshInstancedIndirect(bsm.optimizedBloodMesh, 0, bsm.stainMat, 0, bsm.argsBuffer, 0, null);
            }
            else
            {
                this.bloodOilCB.DrawMesh(bsm.totalStainMesh, Matrix4x4.identity, bsm.stainMat);
            }
            bloodOilCB.SetRenderTarget(mainCam.targetTexture.colorBuffer);
            bloodOilCB.DrawProcedural(PostProcessV2_Handler.identityMatrix, bsm.bloodCompositeMaterial, 1, MeshTopology.Triangles, 3, 1);
            if (bsm.usedComputeShadersAtStart)
            {
                bloodOilCB.SetRenderTarget(pph.reusableBufferB.colorBuffer);
                bloodOilCB.ClearRenderTarget(false, true, new Color(1f, 1f, 1f, 0f));
                bloodOilCB.DrawMeshInstancedIndirect(svm.gasStainMesh, 0, svm.gasStainMat, 0, svm.argsBuffer, 0, null);
                bloodOilCB.SetRenderTarget(mainCam.targetTexture.colorBuffer);
                bloodOilCB.DrawProcedural(PostProcessV2_Handler.identityMatrix, svm.gasolineCompositeMaterial, 1, MeshTopology.Triangles, 3, 1);
            }*/
            //this.bloodOilCB.SetRenderTarget(mainCam.targetTexture.colorBuffer);
            //bloodOilCB.Blit(Plugin.texst, mainCam.targetTexture.colorBuffer);

            //bloodOilCB.SetRenderTarget(Plugin.tex.colorBuffer);


            /*bloodOilCB.DrawProcedural(PostProcessV2_Handler.identityMatrix, bsm.bloodCompositeMaterial, 1, MeshTopology.Triangles, 3, 1);
            if (bsm.usedComputeShadersAtStart)
            {
                bloodOilCB.SetRenderTarget(pph.reusableBufferB.colorBuffer);
                bloodOilCB.ClearRenderTarget(false, true, new Color(1f, 1f, 1f, 0f));
                bloodOilCB.DrawMeshInstancedIndirect(svm.gasStainMesh, 0, svm.gasStainMat, 0, svm.argsBuffer, 0, null);
                bloodOilCB.SetRenderTarget(this.mainTex.colorBuffer);
                bloodOilCB.DrawProcedural(PostProcessV2_Handler.identityMatrix, svm.gasolineCompositeMaterial, 1, MeshTopology.Triangles, 3, 1);
            }*/
            //Debug.LogError(cam.name);
        }

        private void OnPreRenderCallback(Camera cam)
        {
            if (cam.name == "Camera Front")
            {
                //Debug.LogError(cam.name);
                Render(cam);
            }
            
        }
    }
}
