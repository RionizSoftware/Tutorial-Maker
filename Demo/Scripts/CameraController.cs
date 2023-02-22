using UnityEngine;

namespace DemoScripts
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private GameObject player;
        private Vector3 _offset;

        void Start()
        {
            _offset = transform.position - player.transform.position;
        }

        void LateUpdate()
        {
            transform.position = player.transform.position + _offset;
        }
    }
}