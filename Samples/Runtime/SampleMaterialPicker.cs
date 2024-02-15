using UnityEngine;
using UnityEngine.Rendering;

namespace Flexalon.Samples
{
    [ExecuteAlways]
    public class SampleMaterialPicker : MonoBehaviour
    {
        public Material Standard;
        public Material URP;
        public Material HDRP;

        void OnEnable()
        {
            var renderer = GetComponent<MeshRenderer>();
            if (renderer)
            {
                if (renderer.sharedMaterial != null && renderer.sharedMaterial != Standard && renderer.sharedMaterial != URP && renderer.sharedMaterial != HDRP)
                {
                    return;
                }

                if (GraphicsSettings.renderPipelineAsset?.GetType().Name.Contains("HDRenderPipelineAsset") ?? false)
                {
                    renderer.sharedMaterial = HDRP;
                }
                else if (GraphicsSettings.renderPipelineAsset?.GetType().Name.Contains("UniversalRenderPipelineAsset") ?? false)
                {
                    renderer.sharedMaterial = URP;
                }
                else
                {
                    renderer.sharedMaterial = Standard;
                }
            }
        }
    }
}