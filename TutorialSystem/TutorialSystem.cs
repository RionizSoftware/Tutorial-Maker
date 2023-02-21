using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace TutorialSystem
{
    [Serializable]
    public class TutorialState
    {
        public string helpString;
        public bool deactivateOnFinish = true;
        public TutorialType tutorialType = TutorialType.ReachDestination;
        public UnityEvent onStepStarted;
        public UnityEvent onStepFinished;
        public Transform destination;
        public GameObject helpArrow; //Optional
        public List<GameObject> objectsToActivate = new List<GameObject>();
        [HideInInspector] public float currentProgress = 0f;
    }

    public enum TutorialType
    {
        Manual,
        ReachDestination,
    }

    public class TutorialSystem : MonoBehaviour
    {
        [SerializeField] private float reachDestinationThreshHold = 0.1f; //Max distance to reach destination

        [SerializeField] private float
            reachDestinationUpdateTime =
                0.1f; //By decreasing it, the help arrow will update faster but will have less performance

        [SerializeField] private string finishTutorialHelpString;
        [SerializeField] private UnityEvent onTutorialStarted;
        [SerializeField] private List<TutorialState> tutorialStates;
        [SerializeField] private UnityEvent onTutorialFinished;
        [SerializeField] private TextMeshProUGUI helpText;
        [SerializeField] private Transform character;
        [SerializeField] private Transform characterHelpArrow; //Optional
        [SerializeField] private bool rotateArrowOnlyInYDirection = false;

        private int _currentStateIndex = 0;
        private TutorialState _currentState;
        private bool _isTutorialFinished = false;

        private string _helpMessage = "";
        private Coroutine _setTutorialTextInstance = null;


        public void StartTheTutorial()
        {
            _currentStateIndex = 0;
            RefreshCurrentState();
            StartCoroutine(UpdateHelpArrow());
            onTutorialStarted?.Invoke();
        }

        //Set what percentage of tutorial finished ( in case of TutorialType.ReachDestination, this fill set automatically)
        public void IncreaseStepProgress(float increasePercent)
        {
            if (_isTutorialFinished) return;
            _currentState.currentProgress += increasePercent;
            if (_currentState.currentProgress >= 1f)
            {
                GoToNextState();
            }
        }

        public float GetDistanceFromDestination()
        {
            if (_currentState.destination == null) return -1f;
            var characterPosition = character.position;
            var destinationPosition = _currentState.destination.position;
            return Vector3.Distance(characterPosition, destinationPosition);
        }

        public int GetCurrentStep()
        {
            return _currentStateIndex + 1;
        }

        public int GetStepsCount()
        {
            return tutorialStates.Count;
        }

        /****
         *  Setter functions
         * Use these functions to set tutorial needed objects from code , when you can't do it from editor ( in case that something generated in middle of game )
         */
        public void AddObjToActivateInState(int stateIndex, GameObject obj)
        {
            tutorialStates[stateIndex].objectsToActivate.Add(obj);
        }

        public void SetCurrentStepDestination(Transform destination)
        {
            _currentState.destination = destination;
        }

        public void SetPlayerCharacter(Transform characterParam)
        {
            character = characterParam;
        }

        public void SetPlayerHelpArrow(Transform helpArrowParam)
        {
            characterHelpArrow = helpArrowParam;
        }

        ////////////////////// End of Setter functions

        private IEnumerator SetTutorialTextCoRoutine(string message)
        {
            if (helpText == null) yield break;
            var index = 0;
            while (index < message.Length)
            {
                if (index == message.Length - 1)
                {
                    helpText.text = message;
                }
                else
                {
                    helpText.text = message.Substring(0, index + 1) + "|";
                }

                yield return new WaitForSeconds(Random.Range(0.03f, 0.15f));
                index++;
            }
        }

        private void RefreshCurrentState()
        {
            if (_currentStateIndex < tutorialStates.Count)
            {
                if (_currentState == tutorialStates[_currentStateIndex]) return;
                _currentState = tutorialStates[_currentStateIndex];
            }

            if (helpText != null) helpText.text = "";
            for (var index = 0; index < tutorialStates.Count; index += 1)
            {
                var state = tutorialStates[index];
                var isCurrentState = index == _currentStateIndex;
                if (state.helpArrow != null) state.helpArrow.SetActive(isCurrentState);
                foreach (var objToActivate in state.objectsToActivate)
                {
                    objToActivate.SetActive(isCurrentState);
                }

                if (isCurrentState)
                {
                    if (helpText != null)
                    {
                        if (_setTutorialTextInstance != null) StopCoroutine(_setTutorialTextInstance);
                        _setTutorialTextInstance = StartCoroutine(SetTutorialTextCoRoutine(state.helpString));
                    }
                }
            }
        }

        private IEnumerator UpdateHelpArrow()
        {
            while (true)
            {
                if (_isTutorialFinished) yield break;
                if (_currentState.destination != null)
                {
                    characterHelpArrow.gameObject.SetActive(true);
                    if (characterHelpArrow != null)
                    {
                        var rotation =
                            Quaternion.LookRotation(_currentState.destination.position - characterHelpArrow.position);
                        if (rotateArrowOnlyInYDirection)
                        {
                            var arrowCurrentRotation = characterHelpArrow.transform.rotation;
                            rotation.x = arrowCurrentRotation.x;
                            rotation.z = arrowCurrentRotation.z;
                        }

                        characterHelpArrow.transform.rotation = rotation;
                    }
                }
                else
                {
                    characterHelpArrow.gameObject.SetActive(false);
                }


                if (_currentState.tutorialType == TutorialType.ReachDestination)
                {
                    if (GetDistanceFromDestination() < reachDestinationThreshHold)
                    {
                        IncreaseStepProgress(1f);
                    }
                }

                if (reachDestinationUpdateTime < 0f) yield return new WaitForEndOfFrame();
                else yield return new WaitForSeconds(reachDestinationUpdateTime);
            }
        }


        private void GoToNextState()
        {
            _currentStateIndex += 1;
            _currentState.onStepFinished?.Invoke();
            if (_currentState.deactivateOnFinish) _currentState.destination.gameObject.SetActive(false);
            RefreshCurrentState();
            if (_currentStateIndex >= tutorialStates.Count)
            {
                TutorialFinished();
            }
            else
            {
                _currentState?.onStepStarted.Invoke();
            }
        }

        private void TutorialFinished()
        {
            if (characterHelpArrow != null) characterHelpArrow.transform.gameObject.SetActive(false);
            _isTutorialFinished = true;
            onTutorialFinished?.Invoke();
            if (helpText != null)
            {
                if (_setTutorialTextInstance != null) StopCoroutine(_setTutorialTextInstance);
                _setTutorialTextInstance = StartCoroutine(SetTutorialTextCoRoutine(finishTutorialHelpString));
            }
        }
    }
}