namespace UnityEngine.Rendering.Universal
{
    internal class MobileBloomUrpPass : ScriptableRenderPass
    {
        public Material material;

        private RenderTargetIdentifier source;
        private RenderTargetIdentifier bloomTemp = new RenderTargetIdentifier(blurTempString);
        private RenderTargetIdentifier bloomTemp1 = new RenderTargetIdentifier(blurTemp1String);
        private RenderTargetIdentifier bloomTex = new RenderTargetIdentifier(blurTexString);
        private RenderTargetIdentifier tempCopy = new RenderTargetIdentifier(tempCopyString);

        private readonly string tag;
        private readonly int numberOfPasses;
        private readonly float blurAmount;
        private readonly Color bloomColor;
        private readonly float bloomAmount;
        private readonly float bloomThreshold;

        static readonly int blurAmountString = Shader.PropertyToID("_BlurAmount");
        static readonly int bloomColorString = Shader.PropertyToID("_BloomColor");
        static readonly int blAmountString = Shader.PropertyToID("_BloomAmount");
        static readonly int thresholdString = Shader.PropertyToID("_Threshold");

        static readonly int blurTempString = Shader.PropertyToID("_BlurTemp");
        static readonly int blurTemp1String = Shader.PropertyToID("_BlurTemp2");
        static readonly int blurTexString = Shader.PropertyToID("_BlurTex");
        static readonly int tempCopyString = Shader.PropertyToID("_TempCopy");

        public MobileBloomUrpPass(RenderPassEvent renderPassEvent, Material material, int numberOfPasses,
            float blurAmount, Color bloomColor, float bloomAmount, float bloomThreshold, string tag)
        {
            this.renderPassEvent = renderPassEvent;
            this.tag = tag;
            this.material = material;

            this.numberOfPasses = numberOfPasses;
            this.blurAmount = blurAmount;
            this.bloomColor = bloomColor;
            this.bloomAmount = bloomAmount;
            this.bloomThreshold = bloomThreshold;
        }

        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            CommandBuffer cmd = CommandBufferPool.Get(tag);
            cmd.GetTemporaryRT(tempCopyString, opaqueDesc, FilterMode.Bilinear);
            cmd.CopyTexture(source, tempCopy);

            material.SetFloat(blurAmountString, blurAmount);
            material.SetColor(bloomColorString, bloomColor);
            material.SetFloat(blAmountString, bloomAmount);
            material.SetFloat(thresholdString, bloomThreshold);

            if (numberOfPasses == 1 || blurAmount == 0)
            {
                cmd.GetTemporaryRT(blurTexString, Screen.width / 2, Screen.height / 2, 0, FilterMode.Bilinear);
                cmd.Blit(tempCopy, bloomTex, material, 0);
            }
            else if (numberOfPasses == 2)
            {
                cmd.GetTemporaryRT(blurTexString, Screen.width / 2, Screen.height / 2, 0, FilterMode.Bilinear);
                cmd.GetTemporaryRT(blurTempString, Screen.width / 4, Screen.height / 4, 0, FilterMode.Bilinear);
                cmd.Blit(tempCopy, bloomTemp, material, 0);
                cmd.Blit(bloomTemp, bloomTex, material, 1);
            }
            else if (numberOfPasses == 3)
            {
                cmd.GetTemporaryRT(blurTexString, Screen.width / 4, Screen.height / 4, 0, FilterMode.Bilinear);
                cmd.GetTemporaryRT(blurTempString, Screen.width / 8, Screen.height / 8, 0, FilterMode.Bilinear);
                cmd.Blit(tempCopy, bloomTex, material, 0);
                cmd.Blit(bloomTex, bloomTemp, material, 1);
                cmd.Blit(bloomTemp, bloomTex, material, 1);
            }
            else if (numberOfPasses == 4)
            {
                cmd.GetTemporaryRT(blurTexString, Screen.width / 4, Screen.height / 4, 0, FilterMode.Bilinear);
                cmd.GetTemporaryRT(blurTempString, Screen.width / 8, Screen.height / 8, 0, FilterMode.Bilinear);
                cmd.GetTemporaryRT(blurTemp1String, Screen.width / 16, Screen.height / 16, 0, FilterMode.Bilinear);
                cmd.Blit(tempCopy, bloomTemp, material, 0);
                cmd.Blit(bloomTemp, bloomTemp1, material, 1);
                cmd.Blit(bloomTemp1, bloomTemp, material, 1);
                cmd.Blit(bloomTemp, bloomTex, material, 1);
            }
            else if (numberOfPasses == 5)
            {
                cmd.GetTemporaryRT(blurTexString, Screen.width / 4, Screen.height / 4, 0, FilterMode.Bilinear);
                cmd.GetTemporaryRT(blurTempString, Screen.width / 8, Screen.height / 8, 0, FilterMode.Bilinear);
                cmd.GetTemporaryRT(blurTemp1String, Screen.width / 16, Screen.height / 16, 0, FilterMode.Bilinear);
                cmd.Blit(tempCopy, bloomTex, material, 0);
                cmd.Blit(bloomTex, bloomTemp, material, 1);
                cmd.Blit(bloomTemp, bloomTemp1, material, 1);
                cmd.Blit(bloomTemp1, bloomTemp, material, 1);
                cmd.Blit(bloomTemp, bloomTex, material, 1);
            }

            cmd.SetGlobalTexture(blurTexString, bloomTex);
            cmd.Blit(tempCopy, source, material, 2);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempCopyString);
            cmd.ReleaseTemporaryRT(blurTempString);
            cmd.ReleaseTemporaryRT(blurTemp1String);
            cmd.ReleaseTemporaryRT(blurTexString);
        }
    }
}
