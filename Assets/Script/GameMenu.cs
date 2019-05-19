using UnityEngine;
using UnityEngine.UI;

namespace Script
{
    public class GameMenu : MonoBehaviour
    {
        public Button buttonPanZoom;

        // Use this for initialization
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
        }

        public void BackButton()
        {
            Debug.Log("Game HUD Back button pressed");
            GameManager.Instance.RestartAll();
        }

        public void PanZoomButton()
        {
            Debug.Log("Game HUD Pan/Zoom button pressed");
            GameManager.Instance.PanZoomButton(buttonPanZoom);
        }
    }
}