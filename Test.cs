using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    //public InputField m_InputField;
    public Text OutputText;
    public Button m_StreamButton;
    public Button m_NormalButton;
    public AudioSource m_AudioSource;

    // Start is called before the first frame update
    void Start()
    {

        m_StreamButton.onClick.AddListener(() => {
            AzureTTSStream.Instance.StartTTS(OutputText.text, m_AudioSource);
        });
        m_NormalButton.onClick.AddListener(() => {
            AzureTTSNormal.Instance.StartTTS(OutputText.text, m_AudioSource);
        });
    }

}
