using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(AnalogGlitchRenderer), PostProcessEvent.AfterStack, "Kino/AnalogGlitch")]
public sealed class AnalogGlitch : PostProcessEffectSettings
{
	[Range(0, 1)]
	public FloatParameter scanLineJitter = new FloatParameter {value = 0};

	[Range(0, 1)]
	public FloatParameter verticalJump = new FloatParameter {value = 0};
	
	[Range(0, 1)]
	public FloatParameter horizontalShake = new FloatParameter {value = 0};
	
	[Range(0, 1)]
	public FloatParameter colorDrift = new FloatParameter {value = 0};

	public float _verticalJumpTime = 0f;
}

public sealed class AnalogGlitchRenderer : PostProcessEffectRenderer<AnalogGlitch>
{
	public override void Render(PostProcessRenderContext context) {
		var sheet = context.propertySheets.Get(Shader.Find("Hidden/Kino/Glitch/Analog"));;

		settings._verticalJumpTime += Time.deltaTime * settings.verticalJump * 11.3f;

        var sl_thresh = Mathf.Clamp01(1.0f -settings.scanLineJitter * 1.2f);
        var sl_disp = 0.002f + Mathf.Pow(settings.scanLineJitter, 3) * 0.05f;
		sheet.properties.SetVector("_ScanLineJitter", new Vector2(sl_disp, sl_thresh));

        var vj = new Vector2(settings.verticalJump, settings._verticalJumpTime);
		sheet.properties.SetVector("_VerticalJump", vj);

		sheet.properties.SetFloat("_HorizontalShake", settings.horizontalShake * 0.2f);

        var cd = new Vector2(settings.colorDrift * 0.04f, Time.time * 606.11f);
		sheet.properties.SetVector("_ColorDrift", cd);

		context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
	}
}