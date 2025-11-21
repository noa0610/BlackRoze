using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BlackRose.Core.UI
{
    public sealed class RespawnMenu : MonoBehaviour
    {
        [SerializeField] private Button _respawn;
        [SerializeField] private Button _goToTitle;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private string _respawnText = "Never Give up !!!";

        public void Show()
        {
            if (_respawn != null)
            {
                _respawn.gameObject.SetActive(true);
            }
            if (_goToTitle != null)
            {
                _goToTitle.gameObject.SetActive(true);
            }
            if (_text != null)
            {
                _text.text = _respawnText;
                _text.gameObject.SetActive(true);
            }
            Debug.Log("Respawn Menu is Showed");
        }

        private void Awake()
        {
            if (_respawn != null)
            {
                _respawn.onClick.AddListener(() =>
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                });
                _respawn.gameObject.SetActive(false);
                Debug.Log("Respawn is Awaked");
            }
            if (_goToTitle != null)
            {
                _goToTitle.onClick.AddListener(() =>
                {
                    Debug.Log("Dummy Go To Title");
                });
                _goToTitle.gameObject.SetActive(false);
                Debug.Log("GotoTitle is Awaked");
            }
        }
    }
}