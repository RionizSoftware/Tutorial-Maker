using UnityEngine;

namespace DemoScripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private TutorialSystem.TutorialSystem tutorialSystem;

        private static GameManager _instance;

        private void Awake()
        {
            _instance = this;
        }

        public static GameManager GetInstance()
        {
            return _instance;
        }

        
        private void Start()
        {
            tutorialSystem.StartTheTutorial();
        }
     
        public void OnTutorialFinished()
        {
            Debug.Log("Tutorial finished , let the game begin");
        }
      
    }
}