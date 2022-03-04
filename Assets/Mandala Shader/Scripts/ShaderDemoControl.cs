using UnityEngine;
using UnityEngine.UI;

public class ShaderDemoControl : MonoBehaviour
{
    [SerializeField] Slider LayersSlider = default;
    [SerializeField] Slider TilingXSlider = default;
    [SerializeField] Slider TilingYSlider = default;
    [SerializeField] Slider OffsetXSlider = default;
    [SerializeField] Slider OffsetYSlider = default;

    [SerializeField] Slider RotShiftSlider = default;
    [SerializeField] Slider ScaleShiftSlider = default;
    [SerializeField] Slider DarkenSlider = default;
    [SerializeField] Slider GammaSlider = default;
    [SerializeField] Slider LightenSlider = default;
    [SerializeField] Slider DesaturationSlider = default;

    [SerializeField] Text LayerText = default;
    [SerializeField] Text TilingXText = default;
    [SerializeField] Text TilingYText = default;
    [SerializeField] Text OffsetXText = default;
    [SerializeField] Text OffsetYText = default;

    [SerializeField] Text RotationText = default;
    [SerializeField] Text ScaleText = default;

    [SerializeField] Text DarkenText = default;
    [SerializeField] Text GammaText = default;
    [SerializeField] Text LightenText = default;

    [SerializeField] Text DesaturationText = default;

    [SerializeField] Renderer DemoMandala = default;

    Vector2 tiling = new Vector2(1, 1);
    Vector2 offset = new Vector2(0, 0);


    // Start is called before the first frame update
    void Start()
    {
        //Connecting the GUI Sliders to the shader Properties
        LayersSlider.onValueChanged.AddListener((x) => 
        {
            DemoMandala.material.SetFloat("_MandalaLayers", x);
            LayerText.text = x.ToString();
        });
        TilingXSlider.onValueChanged.AddListener((x) =>
        {
            tiling.x = x;
            DemoMandala.material.SetTextureScale("_MandalaTex", tiling);
            TilingXText.text = x.ToString();
        });
        TilingYSlider.onValueChanged.AddListener((x) => 
        {
            tiling.y = x;
            DemoMandala.material.SetTextureScale("_MandalaTex", tiling);
            TilingYText.text = x.ToString();
        });
        OffsetXSlider.onValueChanged.AddListener((x) =>
        {
            offset.x = x;
            DemoMandala.material.SetTextureOffset("_MandalaTex", offset);
            OffsetXText.text = x.ToString();
        });
        OffsetYSlider.onValueChanged.AddListener((x) =>
        {
            offset.y = x;
            DemoMandala.material.SetTextureOffset("_MandalaTex", offset);
            OffsetYText.text = x.ToString();
        });
        RotShiftSlider.onValueChanged.AddListener((x) => 
        {
            DemoMandala.material.SetFloat("_MandalaRotationShift", x);
            RotationText.text = x.ToString();
        });
        ScaleShiftSlider.onValueChanged.AddListener((x) => 
        {
            DemoMandala.material.SetFloat("_MandalaScaleShift", x);
            ScaleText.text = x.ToString();
        });
        DarkenSlider.onValueChanged.AddListener((x) =>
        {
            DemoMandala.material.SetFloat("_Darken", x);
            DarkenText.text = x.ToString();
        });
        GammaSlider.onValueChanged.AddListener((x) =>
        {
            DemoMandala.material.SetFloat("_Gamma", x);
            GammaText.text = x.ToString();
        });
        LightenSlider.onValueChanged.AddListener((x) =>
        {
            DemoMandala.material.SetFloat("_Lighten", x);
            LightenText.text = x.ToString();
        });
        DesaturationSlider.onValueChanged.AddListener((x) => 
        {
            DemoMandala.material.SetFloat("_Desaturate", x);
            DesaturationText.text = x.ToString();
        });

        //Setting the GUI Values to the shader property Value
        LayerText.text = LayersSlider.value.ToString();
        TilingXText.text = TilingXSlider.value.ToString();
        TilingYText.text = TilingYSlider.value.ToString();
        OffsetXText.text = OffsetXSlider.value.ToString();
        OffsetYText.text = OffsetYSlider.value.ToString();
        RotationText.text = RotShiftSlider.value.ToString();
        ScaleText.text = ScaleShiftSlider.value.ToString();
        DarkenText.text = DarkenSlider.value.ToString();
        GammaText.text = GammaSlider.value.ToString();
        LightenText.text = LightenSlider.value.ToString();
        DesaturationText.text = DesaturationSlider.value.ToString();

    }

    
}
