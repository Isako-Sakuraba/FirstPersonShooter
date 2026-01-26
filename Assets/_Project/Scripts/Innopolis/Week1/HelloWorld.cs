using System;
using UnityEngine;

public class HelloWorld : MonoBehaviour
{
    [SerializeField] private Transform _visuals;

    private float _lastTime = 0f;
    private Player _player = new Player(50);

    private void Start()
    {
        Debug.Log("Hello world!");
    }

    private void Update()
    {
        if (Time.time - _lastTime >= 2f)
        {
            _lastTime = Time.time;
            UpdatePlayer();
        }
    }

    private void UpdatePlayer()
    {
        int randomValue1 = UnityEngine.Random.Range(-3, 4);
        int randomValue2 = UnityEngine.Random.Range(-3, 4);
        int randomValue3 = UnityEngine.Random.Range(-3, 4);
        Vector3 randomVector = new Vector3(randomValue1, randomValue2, randomValue3);

        int randomHealt = UnityEngine.Random.Range(0, 30);
        _player.UpdateHealth(-randomHealt);
        _player.Position = randomVector;
        _visuals.transform.position = _player.Position;
    }
}
