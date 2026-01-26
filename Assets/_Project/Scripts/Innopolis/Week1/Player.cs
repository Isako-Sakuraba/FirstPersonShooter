using UnityEngine;

public class Player
{
    public Vector3 Position;
    private int _health;

    public Player(int health)
    {
        _health = health;
    }

    public void UpdateHealth(int delta)
    {
        _health += delta;
        _health = Mathf.Max(_health, 0);
        Debug.Log($"New health: {_health}");
        if (_health == 0)
            Debug.Log("Player died");
    }
}
