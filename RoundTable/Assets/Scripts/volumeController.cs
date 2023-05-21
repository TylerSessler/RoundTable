using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class volumeController : MonoBehaviour
{
    public static volumeController instance;

    [SerializeField] public string _volumeParameter = "MasterVolume";
    [SerializeField] public AudioMixer _mixer;
    [SerializeField] public Slider _slider;
    [SerializeField] public float _multiplier = 30f;
    [SerializeField] Toggle _toggle;
    private bool _disableToggleEvent;

    private void Awake()
    {
        instance = this;
        _slider.onValueChanged.AddListener(HandleSliderValueChanged);
        _toggle.onValueChanged.AddListener(HandleToggleValueChanged);
    }

    private void HandleToggleValueChanged(bool enableSound)
    {
        if (_disableToggleEvent)
        {
            return;
        }

        if (enableSound)
        {
            _slider.value = _slider.maxValue;
            buttonFunctions.instance.buttonAudio();
        }
        else
        {
            _slider.value = _slider.minValue;
        }
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(_volumeParameter, _slider.value);
    }

    private void HandleSliderValueChanged(float value)
    {
        if (value == 0)
            value = 0.001f;
        _mixer.SetFloat(_volumeParameter, Mathf.Log10(value) * _multiplier);
        _disableToggleEvent = true;
        _toggle.isOn = _slider.value > _slider.minValue;
        _disableToggleEvent = false;
        buttonFunctions.instance.buttonAudio();
    }

    // Start is called before the first frame update
    void Start()
    {
        _slider.value = PlayerPrefs.GetFloat(_volumeParameter, _slider.value);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updateVolume()
    {
        _slider.value = PlayerPrefs.GetFloat(_volumeParameter, _slider.value);

    }
}
