using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(DigitalGlitchRenderer), PostProcessEvent.AfterStack, "Kino/DigitalGlitch")]
public sealed class DigitalGlitch : PostProcessEffectSettings
{
	[Range(0, 1)]
	public FloatParameter intensity = new FloatParameter { value = 0 };

	public Texture2D _noiseTexture;
	public RenderTexture _trashFrame1;
	public RenderTexture _trashFrame2;

	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		return enabled.value
			&& intensity.value > 0f;
	}
}

public sealed class DigitalGlitchRenderer : PostProcessEffectRenderer<DigitalGlitch>
{
	public override void Render(PostProcessRenderContext context) {
		var sheet = context.propertySheets.Get(Shader.Find("Hidden/Kino/Glitch/Digital"));

		SetUpResources();

        // Update trash frames on a constant interval.
        var fcount = Time.frameCount;
        if (fcount % 13 == 0) context.command.Blit(context.source, settings._trashFrame1);
        if (fcount % 73 == 0) context.command.Blit(context.source, settings._trashFrame2);

        sheet.properties.SetFloat("_Intensity", settings.intensity);
        sheet.properties.SetTexture("_NoiseTex", settings._noiseTexture);
        var trashFrame = UnityEngine.Random.value > 0.5f ? settings._trashFrame1 : settings._trashFrame2;
        sheet.properties.SetTexture("_TrashTex", trashFrame);

		context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
	}

	void SetUpResources()
    {
        settings._noiseTexture = new Texture2D(64, 32, TextureFormat.ARGB32, false);
        settings._noiseTexture.hideFlags = HideFlags.DontSave;
        settings._noiseTexture.wrapMode = TextureWrapMode.Clamp;
        settings._noiseTexture.filterMode = FilterMode.Point;

        settings._trashFrame1 = new RenderTexture(Screen.width, Screen.height, 0);
        settings._trashFrame2 = new RenderTexture(Screen.width, Screen.height, 0);
        settings._trashFrame1.hideFlags = HideFlags.DontSave;
        settings._trashFrame2.hideFlags = HideFlags.DontSave;

        UpdateNoiseTexture();
    }

	static Color RandomColor()
    {
        return new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
    }

	void UpdateNoiseTexture()
    {
        var color = RandomColor();

        for (var y = 0; y < settings._noiseTexture.height; y++)
        {
            for (var x = 0; x < settings._noiseTexture.width; x++)
            {
                if (UnityEngine.Random.value > 0.89f) color = RandomColor();
                settings._noiseTexture.SetPixel(x, y, color);
            }
        }

        settings._noiseTexture.Apply();
    }

	void Update()
    {
        if (UnityEngine.Random.value > Mathf.Lerp(0.9f, 0.5f, settings.intensity))
        {
            SetUpResources();
            UpdateNoiseTexture();
        }
    }
}