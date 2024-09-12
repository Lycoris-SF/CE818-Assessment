// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Author: Lycoris

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public enum EngineState
{
    LowSpeed,
    HighSpeed,
    Starting,
    Stopping
}

public class EngineSoundController : MonoBehaviour
{
    public AudioMixer EngineMixer;
    public AudioMixerSnapshot lowSpeedSnapshot;
    public AudioMixerSnapshot highSpeedSnapshot;
    public AudioMixerSnapshot startSnapshot;
    public AudioMixerSnapshot stopSnapshot;
    public AudioSource lowSpeedAudioSource;
    public AudioSource highSpeedAudioSource;
    public AudioSource StopAudioSource;

    public List<ParticleSystem> engineParticlesBack;
    public Material engineMat;
    public List<Light> engineLights;

    private float currentPower = 0f; 
    private float maxPower = 100f; 
    private bool isBurstAcceleration = false;
    public bool enableEngineStateUpdate = false;

    public float scaleFactorBurst = 1.875f;
    private float ParticleScaleOffset;
    private float targetEGLVolume = 0;
    private float EGLVolumeChangeRate = 0;
    private float targetEGHVolume = 0;
    private float EGHVolumeChangeRate = 0;

    public List<Text> Debug_text;

    void Update()
    {
        syncSound();
        if(enableEngineStateUpdate)
            UpdateEngineState();

        float currentVolume;
        EngineMixer.GetFloat("EGLVolume", out currentVolume);
        if (EGLVolumeChangeRate != 0)
        {
            float newVolume = Mathf.MoveTowards(currentVolume, targetEGLVolume, EGLVolumeChangeRate * Time.deltaTime);
            EngineMixer.SetFloat("EGLVolume", newVolume);

            if (Mathf.Approximately(newVolume, targetEGLVolume))
            {
                EGLVolumeChangeRate = 0;
            }
        }
        EngineMixer.GetFloat("EGHVolume", out currentVolume);
        if (EGHVolumeChangeRate != 0)
        {
            float newVolume = Mathf.MoveTowards(currentVolume, targetEGHVolume, EGHVolumeChangeRate * Time.deltaTime);
            EngineMixer.SetFloat("EGHVolume", newVolume);

            if (Mathf.Approximately(newVolume, targetEGHVolume))
            {
                EGHVolumeChangeRate = 0;
            }
        }
    }

    void syncSound()
    {
        float volume = PlayerPrefs.GetFloat("SEVolume",0.5f);

        float dB;
        if (volume <= 0)
            dB = -80f;
        else
            dB = Mathf.Lerp(-80f, 10f, volume);

        EngineMixer.SetFloat("SEVolume", dB);
    }
    void UpdateEngineState()
    {
        float powerFactor = currentPower / maxPower;

        if (powerFactor < 0.5f)
        {
            if (isBurstAcceleration)
            {
                TransitionToHighSpeed();
            }
            else
            {
                TransitionToLowSpeed();
            }
        }
        else
        {
            if (isBurstAcceleration)
            {
                TransitionToHighSpeed();
            }
            else
            {
                TransitionToLowSpeed();
            }
        }
    }
    public void TransitionToLowSpeed()
    {
        if (!lowSpeedAudioSource.isPlaying)
        {
            lowSpeedAudioSource.Play();
        }
        float howClose = SetTargetEGLVolume(0, 0.5f);
        SetTargetEGHVolume(-35, 3f);

        Color color;
        ColorUtility.TryParseHtmlString("#00FFE9", out color);
        //lerp engine color
        Color Lerped = Color.Lerp(color, Color.red, howClose);
        //lerp particle scale
        foreach (ParticleSystem Pi in engineParticlesBack)
        {
            float newZ = Mathf.Lerp(ParticleScaleOffset, ParticleScaleOffset * scaleFactorBurst, howClose);
            Vector3 targetScale = new Vector3(Pi.transform.localScale.x, Pi.transform.localScale.y, newZ);
            Pi.transform.localScale = targetScale;
        }
        engineMat.SetColor("_Color", Lerped);
        engineMat.SetColor("_EmissionColor", Lerped);
        foreach(Light engineLight in engineLights)
            engineLight.color = Lerped;
    }

    public void TransitionToHighSpeed()
    {
        if (!highSpeedAudioSource.isPlaying)
        {
            highSpeedAudioSource.Play();
        }
        SetTargetEGLVolume(-35, 3f);
        float howClose = SetTargetEGHVolume(0, 0.5f);

        Color color;
        ColorUtility.TryParseHtmlString("#00FFE9", out color);
        //lerp engine color
        Color Lerped = Color.Lerp(Color.red, color, howClose);
        //lerp particle scale
        foreach (ParticleSystem Pi in engineParticlesBack)
        {
            float newZ = Mathf.Lerp(ParticleScaleOffset * scaleFactorBurst, ParticleScaleOffset, howClose);
            Vector3 targetScale = new Vector3(Pi.transform.localScale.x, Pi.transform.localScale.y, newZ);
            Pi.transform.localScale = targetScale;
        }
        engineMat.SetColor("_Color", Lerped);
        engineMat.SetColor("_EmissionColor", Lerped);
        foreach (Light engineLight in engineLights)
            engineLight.color = Lerped;
    }
    public void shutEngine()
    {
        enableEngineStateUpdate = false;
        TransitionToSnapshot(stopSnapshot, 0.1f);
        lowSpeedAudioSource.Stop();
        highSpeedAudioSource.Stop();
        StopAudioSource.Play();
    }
    public void TransitionToSnapshot(AudioMixerSnapshot snapshot, float transitionTime)
    {
        snapshot.TransitionTo(transitionTime);
    }
    public void SetPower(float power)
    {
        currentPower = Mathf.Clamp(power, 0, maxPower);
    }

    public void SetBurst(bool isBurst)
    {
        isBurstAcceleration = isBurst;
    }
    private void Start()
    {
        ParticleScaleOffset = engineParticlesBack[0].transform.localScale.z;
        StartCoroutine(PlayLowSpeedSoundAfterStart());
    }
    private IEnumerator PlayLowSpeedSoundAfterStart()
    {
        yield return new WaitForSeconds(0.5f);
        enableEngineStateUpdate = true;
        TransitionToSnapshot(lowSpeedSnapshot, 0.1f);
    }
    public float SetTargetEGLVolume(float targetVolume, float timeToReachTarget)
    {
        targetEGLVolume = targetVolume;
        float currentVolume;
        EngineMixer.GetFloat("EGLVolume", out currentVolume);
        EGLVolumeChangeRate = Mathf.Abs(targetEGLVolume - currentVolume) / timeToReachTarget;
        return Mathf.Abs(currentVolume - targetVolume) / 35; 
    }
    public float SetTargetEGHVolume(float targetVolume, float timeToReachTarget)
    {
        targetEGHVolume = targetVolume;
        float currentVolume;
        EngineMixer.GetFloat("EGHVolume", out currentVolume);
        EGHVolumeChangeRate = Mathf.Abs(targetEGHVolume - currentVolume) / timeToReachTarget;
        return Mathf.Abs(currentVolume - targetVolume) / 35;
    }
}
