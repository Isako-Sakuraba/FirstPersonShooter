using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Weapons.Main
{
    public class CoinManager : MonoBehaviour
    {
        [SerializeField] private Coin _coinPrefab;
        [SerializeField] private Transform _coinContainer;
        [SerializeField] private int _defaultCapacity = 8;
        [SerializeField] private int _maxPoolSize = 64;
        [SerializeField] private AudioClip _coinAudio;
        [SerializeField] private AudioSource _coinAudioSource;

        private static CoinManager _instance;
        public static CoinManager Instance => _instance;

        private readonly List<Coin> _activeCoins = new List<Coin>(16);
        private readonly List<Vector3> _activeCoinPositions = new List<Vector3>(16);
        private readonly Dictionary<Coin, int> _coinIndices = new Dictionary<Coin, int>(16);

        private ObjectPool<Coin> _coinPool;

        public IReadOnlyList<Coin> ActiveCoins => _activeCoins;
        public IReadOnlyList<Vector3> ActiveCoinPositions => _activeCoinPositions;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            if (_coinPrefab == null)
            {
                Debug.LogError("CoinManager requires a coin prefab reference.", this);
                enabled = false;
                return;
            }

            _coinPool = new ObjectPool<Coin>(
                CreateCoin,
                OnCoinGet,
                OnCoinRelease,
                OnCoinDestroy,
                collectionCheck: true,
                defaultCapacity: _defaultCapacity,
                maxSize: _maxPoolSize);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void ThrowCoin(Vector3 position, Vector3 playerVelocity, Vector3 lookDirection)
        {
            Coin coin = _coinPool.Get();
            coin.PrepareForSpawn();
            coin.transform.position = position;
            coin.Launch(playerVelocity, lookDirection);
            UpdateCoinPosition(coin, position);
            coin.QueueTrailEnable();
            _coinAudioSource.PlayOneShot(_coinAudio);
        }

        public bool TryGetNearestAvailableCoin(Vector3 origin, float radius, Coin ignoredCoin, out Coin nearestCoin)
        {
            nearestCoin = null;

            float bestDistanceSqr = radius * radius;
            int count = _activeCoins.Count;

            for (int i = 0; i < count; i++)
            {
                Coin candidate = _activeCoins[i];
                if (candidate == null || candidate == ignoredCoin || candidate.HasPendingHit)
                {
                    continue;
                }

                float distanceSqr = (origin - _activeCoinPositions[i]).sqrMagnitude;
                if (distanceSqr < bestDistanceSqr)
                {
                    bestDistanceSqr = distanceSqr;
                    nearestCoin = candidate;
                }
            }

            return nearestCoin != null;
        }

        public void UpdateCoinPosition(Coin coin, Vector3 position)
        {
            if (coin == null || !_coinIndices.TryGetValue(coin, out int index))
            {
                return;
            }

            _activeCoinPositions[index] = position;
        }

        public void ReleaseCoin(Coin coin)
        {
            if (coin == null)
            {
                return;
            }

            _coinPool.Release(coin);
        }

        private Coin CreateCoin()
        {
            Transform parent = _coinContainer != null ? _coinContainer : transform;
            return Instantiate(_coinPrefab, parent);
        }

        private void OnCoinGet(Coin coin)
        {
            coin.gameObject.SetActive(true);

            int index = _activeCoins.Count;
            _activeCoins.Add(coin);
            _activeCoinPositions.Add(coin.transform.position);
            _coinIndices[coin] = index;

            coin.OnTakenFromPool();
        }

        private void OnCoinRelease(Coin coin)
        {
            if (_coinIndices.TryGetValue(coin, out int index))
            {
                int lastIndex = _activeCoins.Count - 1;

                if (index != lastIndex)
                {
                    Coin lastCoin = _activeCoins[lastIndex];
                    Vector3 lastPosition = _activeCoinPositions[lastIndex];

                    _activeCoins[index] = lastCoin;
                    _activeCoinPositions[index] = lastPosition;
                    _coinIndices[lastCoin] = index;
                }

                _activeCoins.RemoveAt(lastIndex);
                _activeCoinPositions.RemoveAt(lastIndex);
                _coinIndices.Remove(coin);
            }

            coin.OnReturnedToPool();
            coin.gameObject.SetActive(false);
        }

        private static void OnCoinDestroy(Coin coin)
        {
            if (coin != null)
            {
                Destroy(coin.gameObject);
            }
        }
    }
}
