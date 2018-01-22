using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts.PowerUp
{
    public class PowerUpController : MonoBehaviour
    {
        public Color PowerUpColor = Color.red;
        public float TurnSpeed = 10f;
        public PowerUpManager PowerUpManagerInstance;
        [HideInInspector]
        public string ColoredText;
        [HideInInspector]
        public string PowerUpType;
        

        public void Setup()
        {
            ColoredText = "<color=#" + ColorUtility.ToHtmlStringRGB(PowerUpColor) + ">POWER UP</color>";

            Renderer renderer = GetComponent<Renderer>();
            renderer.sharedMaterial.color = PowerUpColor;
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, TurnSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            

            // Need to trigger an effect
            Rigidbody rigidBody = other.GetComponent<Rigidbody>();
            if (!rigidBody)
                return;

            //TankManager targetManager = rigidBody.GetComponent<TankManager>();
            //if (targetManager == null)
            //    return;

            PowerUpManagerInstance.PowerUpColliderTriggerHit(PowerUpType, ColoredText, other);


            // Then need to remove the power up
            Destroy(gameObject);
        }
    }
}