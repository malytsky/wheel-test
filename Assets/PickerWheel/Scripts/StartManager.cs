using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PickerWheel.Scripts
{
    public class StartManager : MonoBehaviour
    {
        [SerializeField] private Button btnStart;
        [SerializeField] private Button btnQuit;
        [SerializeField] private Text scoreText;

        private void Start()
        {
            if (btnStart != null)
            {
                btnStart.onClick.AddListener(OnStartButtonClick);
            }
            if (btnQuit != null)
            {
                btnQuit.onClick.AddListener(OnExitButtonClick);
            }
            scoreText.text = DefaultVariables.TextRecord + FormatValues.FormatScore(PlayerPrefs.GetInt(DefaultVariables.TextToSaveRecord));
        }

        private void OnStartButtonClick()
        {
            btnStart.onClick.RemoveAllListeners();
            btnQuit.onClick.RemoveAllListeners();
            SceneManager.LoadScene("MainScene");
        }
    
        private static void OnExitButtonClick()
        {
            Application.Quit();
        }
    }
}

