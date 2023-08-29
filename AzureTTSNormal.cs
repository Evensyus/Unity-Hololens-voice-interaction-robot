using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections;
using UnityEngine;

public class AzureTTSNormal : MonoSingleton<AzureTTSNormal>
{
    private AudioSource m_AudioSource;

    private string m_SubscriptionKey = "1c59566d77cd4462a7b3f73f66efd431";
    private string m_Region = "eastus";
    private string m_SpeechSynthesisLanguage = "zh-CN";
    private string m_SpeechSynthesisVoiceName = "zh-CN-XiaochenNeural";
    private Coroutine m_TTSCoroutine;

    /// <summary>
    /// 你的授权
    /// </summary>
    /// <param name="subscriptionKey">子脚本的Key</param>
    /// <param name="region">地区</param>
    public void SetAzureAuthorization(string subscriptionKey, string region)
    {
        m_SubscriptionKey = subscriptionKey;
        m_Region = region;
    }

    /// <summary>
    /// 设置语音和声音
    /// </summary>
    /// <param name="language">语言</param>
    /// <param name="voiceName">声音</param>
    public void SetLanguageVoiceName(SpeechSynthesisLanguage language, SpeechSynthesisVoiceName voiceName)
    {
        m_SpeechSynthesisLanguage = language.ToString().Replace('_', '-');
        m_SpeechSynthesisVoiceName = voiceName.ToString().Replace('_', '-');
    }

    /// <summary>
    /// 设置音源
    /// </summary>
    /// <param name="audioSource"></param>
    public void SetAudioSource(AudioSource audioSource)
    {
        m_AudioSource = audioSource;
    }

    /// <summary>
    /// 开始TTS
    /// </summary>
    /// <param name="spkMsg"></param>
    /// <param name="errorAction"></param>
    public void StartTTS(string spkMsg, Action<string> errorAction = null)
    {
        StopTTS();
        m_TTSCoroutine = StartCoroutine(SynthesizeAudioCoroutine(spkMsg, errorAction));
    }

    /// <summary>
    /// 开始TTS
    /// </summary>
    /// <param name="spkMsg"></param>
    /// <param name="audioSource"></param>
    /// <param name="errorAction"></param>
    public void StartTTS(string spkMsg, AudioSource audioSource, Action<string> errorAction = null)
    {
        SetAudioSource(audioSource);
        StartTTS(spkMsg, errorAction);
    }

    /// <summary>
    /// 暂停TTS
    /// </summary>
    public void StopTTS()
    {
        if (m_TTSCoroutine != null)
        {
            StopCoroutine(m_TTSCoroutine);
            m_TTSCoroutine = null;
        }
        if (m_AudioSource != null)
        {
            m_AudioSource.Stop();
            m_AudioSource.clip = null;
        }
    }

    public IEnumerator SynthesizeAudioCoroutine(string spkMsg, Action<string> errorAction)
    {
        yield return null;
        var config = SpeechConfig.FromSubscription(m_SubscriptionKey, m_Region);
        config.SpeechSynthesisLanguage = m_SpeechSynthesisLanguage;
        config.SpeechSynthesisVoiceName = m_SpeechSynthesisVoiceName;
        // Creates a speech synthesizer.
        // Make sure to dispose the synthesizer after use!
        using (var synthsizer = new SpeechSynthesizer(config, null))
        {

            // Starts speech synthesis, and returns after a single utterance is synthesized.
            var result = synthsizer.SpeakTextAsync(spkMsg).Result;
            //print("after   " + DateTime.Now);

            // Checks result.
            string newMessage = string.Empty;
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                // Since native playback is not yet supported on Unity yet (currently only supported on Windows/Linux Desktop),
                // use the Unity API to play audio here as a short term solution.
                // Native playback support will be added in the future release.
                var sampleCount = result.AudioData.Length / 2;
                var audioData = new float[sampleCount];
                for (var i = 0; i < sampleCount; ++i)
                {
                    audioData[i] = (short)(result.AudioData[i * 2 + 1] << 8 | result.AudioData[i * 2]) / 32768.0F;

                }
                // The default output audio format is 16K 16bit mono
                var audioClip = AudioClip.Create("SynthesizedAudio", sampleCount, 1, 16000, false);
                audioClip.SetData(audioData, 0);
                m_AudioSource.clip = audioClip;
                Debug.Log(" audioClip.length " + audioClip.length);
                m_AudioSource.Play();


            }

            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                newMessage = $"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?";
                Debug.Log(" newMessage " + newMessage);
                if (errorAction != null) { errorAction.Invoke(newMessage); }

            }


        }
    }
}