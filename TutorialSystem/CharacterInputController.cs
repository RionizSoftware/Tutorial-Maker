using UnityEngine;

namespace TutorialSystem
{
    public class CharacterInputController : MonoBehaviour
    {
        [SerializeField] private TutorialSystem tutorialSystem;
        private CharacterController _characterController;

        // Start is called before the first frame update
        void Start()
        {
            _characterController = GetComponent<CharacterController>();
        }

        public void Update()
        {
            if (Input.GetKeyUp("space"))
            {
                if (tutorialSystem.GetCurrentStep() == 4)
                    tutorialSystem.IncreaseStepProgress(1f);
            }

            var movePosition = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            
            if (movePosition.magnitude > 0f)
            {
                if (tutorialSystem.GetCurrentStep() == 1)
                    tutorialSystem.IncreaseStepProgress(1f);
            }

            _characterController.Move(movePosition * Time.deltaTime * 10f);
        }
    }
}