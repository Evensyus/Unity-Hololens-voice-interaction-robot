using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using System.IO;
using System;
using System.Collections;

public class AzureTTSStream : MonoSingleton<AzureTTSStream>
{
    private AudioSource m_AudioSource;

    private string m_SubscriptionKey = "1c59566d77cd4462a7b3f73f66efd431";

    private string m_Region = "eastus";

    private string m_SpeechSynthesisLanguage = "zh-CN";

    private string m_SpeechSynthesisVoiceName = "zh-CN-XiaochenNeural";

    public const int m_SampleRate = 16000;

    public const int m_BufferSize = m_SampleRate * 60; //���֧��60s��Ƶ������Ҳ���Ե�����ʽ������ν

    public const int m_UpdateSize = m_SampleRate / 10; //����������Խ��Խ��

    private Coroutine m_TTSCoroutine;
    private int m_DataIndex = 0;

    private AudioDataStream m_AudioDataStream;

    private void OnEnable()
    {
        StopTTS();
    }

    private void OnDisable()
    {
        StopTTS();
    }

    /// <summary>
    /// �����Ȩ
    /// </summary>
    /// <param name="subscriptionKey">�ӽű���Key</param>
    /// <param name="region">����</param>
    public void SetAzureAuthorization(string subscriptionKey, string region)
    {
        m_SubscriptionKey = subscriptionKey;
        m_Region = region;
    }

    /// <summary>
    /// ��������������
    /// </summary>
    /// <param name="language">����</param>
    /// <param name="voiceName">����</param>
    public void SetLanguageVoiceName(SpeechSynthesisLanguage language, SpeechSynthesisVoiceName voiceName)
    {
        m_SpeechSynthesisLanguage = language.ToString().Replace('_', '-');
        m_SpeechSynthesisVoiceName = voiceName.ToString().Replace('_', '-');
    }

    /// <summary>
    /// ������Դ
    /// </summary>
    /// <param name="audioSource"></param>
    public void SetAudioSource(AudioSource audioSource)
    {
        m_AudioSource = audioSource;
    }

    /// <summary>
    /// ��ʼTTS
    /// </summary>
    /// <param name="spkMsg"></param>
    /// <param name="errorAction"></param>
    public void StartTTS(string spkMsg, Action<string> errorAction = null)
    {
        StopTTS();
        m_TTSCoroutine = StartCoroutine(SynthesizeAudioCoroutine(spkMsg, errorAction));
    }

    /// <summary>
    /// ��ʼTTS
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
    /// ��ͣTTS
    /// </summary>
    public void StopTTS()
    {
        // �ͷ���
        if (m_AudioDataStream != null)
        {
            m_AudioDataStream.Dispose();
            m_AudioDataStream = null;
        }

        if (m_TTSCoroutine != null)
        {
            StopCoroutine(m_TTSCoroutine);
            m_TTSCoroutine = null;

        }
        if (m_AudioSource != null)
        {
            m_AudioSource.Stop();
            m_AudioSource.clip = null;
            m_DataIndex = 0;
        }
    }

    /// <summary>
    /// ����TTS
    /// </summary>
    /// <param name="speakMsg">TTS���ı�</param>
    /// <param name="errorAction">�����¼���Ŀǰû�кõ��жϷ�����</param>
    /// <returns></returns>
    private IEnumerator SynthesizeAudioCoroutine(string speakMsg, Action<string> errorAction)
    {
        var config = SpeechConfig.FromSubscription(m_SubscriptionKey, m_Region);
        config.SpeechSynthesisLanguage = m_SpeechSynthesisLanguage;
        config.SpeechSynthesisVoiceName = m_SpeechSynthesisVoiceName;
        var audioClip = AudioClip.Create("SynthesizedAudio", m_BufferSize, 1, m_SampleRate, false);
        m_AudioSource.clip = audioClip;
        using (var synthesizer = new SpeechSynthesizer(config, null))
        {

            var result = synthesizer.StartSpeakingTextAsync(speakMsg);
            yield return new WaitUntil(() => result.IsCompleted);
            m_AudioSource.Play();
            using (m_AudioDataStream = AudioDataStream.FromResult(result.Result))
            {
                MemoryStream memStream = new MemoryStream();
                byte[] buffer = new byte[m_UpdateSize * 2];
                uint bytesRead;
                do
                {
                    bytesRead = m_AudioDataStream.ReadData(buffer);
                    memStream.Write(buffer, 0, (int)bytesRead);
                    if (memStream.Length >= m_UpdateSize * 2)
                    {
                        var tempData = memStream.ToArray();
                        var audioData = new float[m_UpdateSize];
                        for (int i = 0; i < m_UpdateSize; ++i)
                        {
                            audioData[i] = (short)(tempData[i * 2 + 1] << 8 | tempData[i * 2]) / 32768.0F;
                        }

                        audioClip.SetData(audioData, m_DataIndex);
                        m_DataIndex = (m_DataIndex + m_UpdateSize) % m_BufferSize;

                        memStream = new MemoryStream();
                        yield return null;
                    }
                } while (bytesRead > 0);

            }
        }

        if (m_DataIndex == 0)
        {
            if (errorAction != null)
            {
                errorAction.Invoke(" AudioData error");
            }
        }
    }
}

/// <summary>
/// ��Ӹ������������
/// ��ʽ����Ϊ Zh_CN ��Ӧ "zh-CN";
/// </summary>
public enum SpeechSynthesisLanguage
{
    Zh_CN,
}

/// <summary>
/// ��Ӹ������������
/// ��ʽ����Ϊ Zh_CN_XiaochenNeural ��Ӧ "zh-CN-XiaochenNeural";
/// </summary>
public enum SpeechSynthesisVoiceName
{
    Zh_CN_XiaochenNeural,

}