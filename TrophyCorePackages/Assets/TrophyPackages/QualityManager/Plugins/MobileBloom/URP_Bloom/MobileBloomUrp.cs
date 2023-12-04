namespace UnityEngine.Rendering.Universal
{
    public class MobileBloomUrp : ScriptableRendererFeature
    {
        [System.Serializable]
        public class MobileBloomSettings
        {
            public RenderPassEvent Event = RenderPassEvent.AfterRenderingTransparents;

            public Material blitMaterial = null;

            [Range(1, 5)]
            public int NumberOfPasses = 3;

            [Range(0, 5)]
            public float BlurAmount = 1f;

            public Color BloomColor = Color.white;

            [Range(0, 5)]
            public float BloomAmount = 1f;

            [Range(0, 1)]
            public float BloomThreshold = 0.0f;
        }

        public MobileBloomSettings settings = new MobileBloomSettings();

        MobileBloomUrpPass mobilePostProcessLwrpPass;

        public override void Create()
        {
            mobilePostProcessLwrpPass = new MobileBloomUrpPass(settings.Event, settings.blitMaterial, settings.NumberOfPasses, settings.BlurAmount, settings.BloomColor, settings.BloomAmount, settings.BloomThreshold, this.name);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            mobilePostProcessLwrpPass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(mobilePostProcessLwrpPass);
        }
    }
}

