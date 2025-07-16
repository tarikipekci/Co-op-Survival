using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Manager
{
    public class DayNightManager : NetworkBehaviour
    {
        public Light2D globalLight;
        public float transitionDuration = 2f;

        private Coroutine currentTransition;

        private NetworkVariable<bool> isNight = new NetworkVariable<bool>();

        public override void OnNetworkSpawn()
        {
            isNight.OnValueChanged += OnNightChanged;

            if (IsServer)
            {
                OnNightChanged(false, isNight.Value);

                NetworkManager.OnClientConnectedCallback += HandleClientConnected;
            }
            else
            {
                OnNightChanged(!isNight.Value, isNight.Value);
            }
        }

        public void SetNight()
        {
            if (!IsServer) return;

            isNight.Value = true;
        }

        public void SetDay()
        {
            if (!IsServer) return;

            isNight.Value = false;
        }

        private void OnNightChanged(bool oldValue, bool newValue)
        {
            if (currentTransition != null) StopCoroutine(currentTransition);

            if (newValue)
            {
                currentTransition = StartCoroutine(TransitionLight(
                    globalLight.intensity,
                    0.05f,
                    globalLight.color,
                    new Color(0.1f, 0.1f, 0.2f)
                ));
                SpawnAllFlashlights();
            }
            else
            {
                currentTransition = StartCoroutine(TransitionLight(
                    globalLight.intensity,
                    1f,
                    globalLight.color,
                    Color.white
                ));
                RemoveAllFlashlights();
            }
        }

        private IEnumerator TransitionLight(float fromIntensity, float toIntensity, Color fromColor, Color toColor)
        {
            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionDuration;

                globalLight.intensity = Mathf.Lerp(fromIntensity, toIntensity, t);
                globalLight.color = Color.Lerp(fromColor, toColor, t);

                yield return null;
            }

            globalLight.intensity = toIntensity;
            globalLight.color = toColor;
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (!isNight.Value) return;

            StartCoroutine(SpawnFlashlightForClientDelayed(clientId));
        }

        private IEnumerator SpawnFlashlightForClientDelayed(ulong clientId)
        {
            yield return new WaitForSeconds(0.5f);

            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                var playerObj = client.PlayerObject;
                if (playerObj == null) yield break;

                var player = playerObj.GetComponent<Player.PlayerFlashlightController>();
                player?.SpawnFlashlight();
            }
        }

        private void SpawnAllFlashlights()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject == null) continue;

                var player = client.PlayerObject.GetComponent<Player.PlayerFlashlightController>();
                player?.SpawnFlashlight();
            }
        }

        private void RemoveAllFlashlights()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject == null) continue;

                var player = client.PlayerObject.GetComponent<Player.PlayerFlashlightController>();
                player?.RemoveFlashlight();
            }
        }

        public bool IsNight()
        {
            return isNight.Value;
        }

        public override void OnDestroy()
        {
            if (IsServer && NetworkManager)
            {
                NetworkManager.OnClientConnectedCallback -= HandleClientConnected;
            }
        }
    }
}