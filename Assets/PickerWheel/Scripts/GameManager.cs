using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PickerWheel.Scripts
{
   public class GameManager : MonoBehaviour {
      [SerializeField] private Button btnSpin;
      [SerializeField] private Text spinButtonText;
      [SerializeField] private Button btnBack;

      [Header ("References:")]
      [SerializeField] private GameObject linePrefab;
      [SerializeField] private Transform wheelCircle;
      [SerializeField] private GameObject wheelPiecePrefab;
      [SerializeField] private Text scoreText;
      [SerializeField] private Text lastScoreText;

      [Space]
      [Header ("Audio:")]
      [SerializeField] private AudioSource audioSource1;
      [SerializeField] private AudioSource audioSource2;
      [SerializeField] private AudioClip countAudioClip;
      [SerializeField] private AudioClip rewardAudioClip; 
      [SerializeField] [Range (0f, 1f)] private float volume = .5f;
      [SerializeField] [Range (-3f, 3f)] private float pitch = 1f;

      [Space]
      [Header ("Spin duration:")]
      [Range (1, 15)] public int spinDuration = 8;

      private WheelOfFortune _wheelOfFortune;


      private void Start()
      {
         //pattern Builder
         _wheelOfFortune = new WheelOfFortune(wheelCircle, wheelPiecePrefab, linePrefab, spinDuration, audioSource1, audioSource2, scoreText);
         _wheelOfFortune.OnInstantiate += InstantiateGameObject;
         _wheelOfFortune.OnDestroy += DestroyGameObject;
         _wheelOfFortune.OnSpinEndEvent += OnSpinEnd;
         _wheelOfFortune.Create();

         lastScoreText.text = DefaultVariables.LastScore + FormatValues.FormatScore(GameVariables.LastResult);
         SetupAudio();
      
         btnSpin.onClick.AddListener(() => {
            btnSpin.enabled = false;
            spinButtonText.text = "WAIT";
            _wheelOfFortune.Spin();
         });
         
         if (btnBack != null)
         {
            btnBack.onClick.AddListener(OnBackButtonClick);
         }
      }

      private void OnSpinEnd()
      {
         btnSpin.enabled = true;
         spinButtonText.text = "SPIN";
      }
      
      private void OnBackButtonClick()
      {
         _wheelOfFortune.SaveCurrentResult(); //pattern Memento
         _wheelOfFortune.CheckRecord();
         Clear();
         SceneManager.LoadScene("StartScene");
      }

      private void Clear()
      {
         _wheelOfFortune.Clear();
         _wheelOfFortune.OnInstantiate -= InstantiateGameObject;
         _wheelOfFortune.OnDestroy -= DestroyGameObject;
         _wheelOfFortune.OnSpinEndEvent -= OnSpinEnd;
         btnBack.onClick.RemoveAllListeners();
         btnSpin.onClick.RemoveAllListeners();
      }

      private void SetupAudio() {
         audioSource1.clip = countAudioClip;
         audioSource2.clip = rewardAudioClip;
         audioSource1.volume = audioSource2.volume = volume;
         audioSource1.pitch = audioSource2.pitch = pitch;
      }
   
      private void DestroyGameObject(GameObject gObject)
      {
         Destroy(gObject);
      }
   
      private GameObject InstantiateGameObject(GameObject prefab, Vector3 position, Quaternion identity, Transform parent) {
         return Instantiate(prefab, parent.position, Quaternion.identity, parent);
      }
   }
}
