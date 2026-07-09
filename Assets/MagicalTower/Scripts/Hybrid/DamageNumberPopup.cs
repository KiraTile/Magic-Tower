using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public sealed class DamageNumberPopup : MonoBehaviour
{
    [SerializeField, Min(0f)] private float riseSpeed = 1.4f;
    [SerializeField, Min(0.01f)] private float lifetime = 0.8f;

    private Camera _camera;
    private float3 _worldPosition;
    private float _age;
    private Queue<DamageNumberPopup> _pool;
    private Text _text;

    private void Awake()
    {
        _text = GetComponent<Text>();
    }

    public void Initialize(Camera camera, float3 worldPosition, float amount, Queue<DamageNumberPopup> pool)
    {
        _camera = camera;
        _worldPosition = worldPosition;
        _age = 0f;
        _pool = pool;
        _text.text = Mathf.CeilToInt(amount).ToString();
        _text.color = Color.white;
        UpdatePosition();
    }

    private void Update()
    {
        _age += Time.deltaTime;
        _worldPosition.y += Time.deltaTime * riseSpeed;
        UpdatePosition();

        Color color = _text.color;
        color.a = Mathf.Clamp01(1f - _age / lifetime);
        _text.color = color;

        if (_age >= lifetime)
        {
            gameObject.SetActive(false);
            _pool.Enqueue(this);
        }
    }

    private void UpdatePosition()
    {
        transform.position = _camera.WorldToScreenPoint(_worldPosition);
    }
}
