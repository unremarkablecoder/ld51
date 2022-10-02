using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSound : MonoBehaviour {
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float offset = 0;
    [SerializeField] private float min = 0;
    [SerializeField] private float max = 1;
    private AudioSource source;
    

    void Awake() {
        source = GetComponent<AudioSource>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        source.volume = min + (max-min) * (Mathf.Sin((Time.time + offset) * speed) + 1) * 0.5f;
    }

    public float GetWind() {
        return source.volume / max;
    }
}
