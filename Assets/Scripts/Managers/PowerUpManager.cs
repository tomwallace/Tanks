using Assets.Scripts.PowerUp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class PowerUpManager : MonoBehaviour
    {
        public const string HEALTH = "HEALTH";
        public const string SHIELD = "SHIELD";
        public const string MEGASHELL = "MEGASHELL";

        public float StartDelay = 30f;
        public float MinTimeBeforeAppear = 30f;
        public float MaxTimeBeforeAppear = 45f;
        public float LifeTime = 10f;
        public float PowerUpLasts = 10f;
        public Rigidbody MegaShell;
        public GameObject PowerUpPrefab;
        public GameManager GameManagerInstance;
        public SpawnPointManager SpawnPointManager;

        private WaitForSeconds _startWait;
        private WaitForSeconds _nextPowerUpAppear;
        private List<string> _powerUpTypes;

        public void PowerUpColliderTriggerHit(string powerUpType, string coloredText, Collider colliderHit)
        {
            // Ensure it is a tank first
            TankMovement tankMovement = colliderHit.GetComponent<TankMovement>();

            if (tankMovement != null)
            {
                Debug.Log("Player picked up power up of type: " + powerUpType.ToString());

                string message = string.Format("Player {0} got a {1} {2}", tankMovement.m_PlayerNumber, powerUpType, coloredText);
                StartCoroutine(GameManagerInstance.SetMessageText(message));

                switch (powerUpType)
                {
                    case HEALTH:
                        TankHealth tankHealth = colliderHit.GetComponent<TankHealth>();
                        // Heal them by taking negative damage
                        tankHealth.TakeDamage(-50f);
                        break;

                    case SHIELD:
                        float playerShieldEnds = Time.time + PowerUpLasts;
                        StartCoroutine(MakeShield(colliderHit.gameObject, playerShieldEnds));
                        break;

                    case MEGASHELL:
                        StartCoroutine(SetMegaShell(colliderHit.gameObject, PowerUpLasts));
                        break;
                    
                    default:
                        throw new MissingComponentException("PowerUp name not recognized");
                }
            }
        }

        private void Start()
        {
            _powerUpTypes = new List<string>()
            {
                HEALTH, SHIELD, MEGASHELL
            };
            _startWait = new WaitForSeconds(StartDelay);

            StartCoroutine(PowerUpLoop());
        }

        private IEnumerator RoundStarting()
        {
            yield return _startWait;
        }

        private IEnumerator RoundPlaying()
        {
            SpawnPowerUp();
            SetNextPowerUpAppear();

            yield return _nextPowerUpAppear;
        }

        private IEnumerator PowerUpLoop()
        {
            yield return StartCoroutine(RoundStarting());
            yield return StartCoroutine(RoundPlaying());

            StartCoroutine(PowerUpLoop());
        }

        private void SetNextPowerUpAppear()
        {
            float time = Random.Range(MinTimeBeforeAppear, MaxTimeBeforeAppear);
            _nextPowerUpAppear = new WaitForSeconds(time);
        }

        private void SpawnPowerUp()
        {
            string type = _powerUpTypes[Random.Range(0, _powerUpTypes.Count - 1)];

            Color color = Color.red;

            switch (type)
            {
                case HEALTH:
                    color = Color.green;
                    break;

                case SHIELD:
                    color = Color.blue;
                    break;

                case MEGASHELL:
                    color = Color.red;
                    break;

                default:
                    throw new MissingComponentException("PowerUp name not recognized");
            }

            Transform spawnPoint = SpawnPointManager.GetRandomSpawnPoint();
            // Adjust height so powerUp not in ground
            GameObject powerUpInstance = Instantiate(PowerUpPrefab, new Vector3(spawnPoint.position.x, 0.75f, spawnPoint.position.z), spawnPoint.rotation) as GameObject;
            PowerUpController controller = powerUpInstance.GetComponent<PowerUpController>();
            controller.PowerUpColor = color;
            controller.PowerUpType = type;
            controller.PowerUpManagerInstance = this;
            controller.Setup();

            // Make sure it is destroyed in max lifetime
            Destroy(powerUpInstance, LifeTime);
        }

        private IEnumerator MakeShield(GameObject gameObject, float duration)
        {
            var endTime = Time.time + duration;
            TankHealth tankHealth = gameObject.GetComponent<TankHealth>();
            tankHealth.SetIsShielded(true);
            while (Time.time < endTime)
            {
                MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();

                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = false;
                }
                yield return new WaitForSeconds(0.1f);

                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = true;
                }
                yield return new WaitForSeconds(0.4f);
            }
            tankHealth.SetIsShielded(false);
        }

        private IEnumerator SetMegaShell(GameObject gameObject, float duration)
        {
            TankShooting tankShooting = gameObject.GetComponent<TankShooting>();
            tankShooting.SetShellRigidBody(MegaShell);

            yield return new WaitForSeconds(duration);

            tankShooting.EnableDefaultShellRigidBody();
        }
    }
}