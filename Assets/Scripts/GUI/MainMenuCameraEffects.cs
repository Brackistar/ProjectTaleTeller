using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class MainMenuCameraEffects : MonoBehaviour
{
    [Range(5, 7)]
    public float lightingTimer;
    [Range(1, 2)]
    public float lightingDuration;
    [Range(1, 5)]
    public float lightingFirstDelay;
    [Range(0, 0.7f)]
    public float backGroundLightingAlpha;
    public Color LightingColor;
    public Camera MainCamera;
    public UnityEngine.UI.Image BackgroundImage;

    private AudioSource LightingSound;
    private Color OriginalColor;
    private Color OriginalBackgroundColor;
    private bool isChangingColor = false;
    // Start is called before the first frame update
    void Start()
    {
        OriginalColor = MainCamera.backgroundColor;
        OriginalBackgroundColor = BackgroundImage.color;
        LightingSound = GetComponent<AudioSource>();

        StartCoroutine("StrikeLighting");
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator StrikeLighting()
    {
        Debug.Log("Start Main menu Lighting Coroutine");

        yield return new WaitForSeconds(lightingFirstDelay);
        while (true)
        {
            Debug.Log("Lighting strike 1");
            LightingSound.time = 0.7f;
            LightingSound.Play();
            StartCoroutine(
                methodName: "ChangeColor",
                value: new ColorChange()
                {
                    time = lightingDuration / 2,
                    color = LightingColor,
                    color2 = new Color(
                        r: OriginalBackgroundColor.r,
                        g: OriginalBackgroundColor.g,
                        b: OriginalBackgroundColor.b,
                        a: backGroundLightingAlpha)
                });
            yield return new WaitForSeconds(lightingDuration);
            Debug.Log("Lighting strike 2");
            StartCoroutine(
                methodName: "ChangeColor",
                value: new ColorChange()
                {
                    time = lightingDuration / 2,
                    color = OriginalColor,
                    color2 = new Color(
                        r: OriginalBackgroundColor.r,
                        g: OriginalBackgroundColor.g,
                        b: OriginalBackgroundColor.b,
                        a: OriginalBackgroundColor.a)
                });
            yield return new WaitForSeconds(lightingDuration / 2);
            LightingSound.Stop();
            yield return new WaitForSeconds(lightingTimer);
        }
    }

    IEnumerator ChangeColor(ColorChange colorChange)
    {
        if (!isChangingColor)
        {
            Debug.Log("Start Main menu color change coroutine");
            float ElapsedTime = 0;
            float TotalTime = colorChange.time;
            //byte AlplaMultiplier = 1;
            Color StartColor = MainCamera.backgroundColor,
                StartBackgroundColor = BackgroundImage.color;


            isChangingColor = true;

            while (ElapsedTime < TotalTime)
            {
                ElapsedTime += UnityEngine.Time.deltaTime;
                MainCamera.backgroundColor = Color.Lerp(
                    a: StartColor,
                    b: colorChange.color,
                    t: (ElapsedTime / TotalTime));
                BackgroundImage.color = Color.Lerp(
                    a: StartBackgroundColor,
                    b: colorChange.color2,
                    t: (ElapsedTime / TotalTime));
                yield return null;
            }

            isChangingColor = false;
            Debug.Log("Stop Main menu color change coroutine");
        }

    }

    private class ColorChange
    {
        public float time;
        public Color color;
        public Color color2;
    }
}
