using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Strand {
    public GameObject obj;
    public float targetRot;
    public float rot;
}
public class GrassStrands : MonoBehaviour {
    [SerializeField] private GameObject[] strandPrefabs;

    [SerializeField] private Color[] colors;
    [SerializeField] private WindSound windSound;
    
    private List<Strand> strands = new List<Strand>();

    private float timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 30; ++i) {
            var obj = Instantiate(strandPrefabs[Random.Range(0, strandPrefabs.Length)], transform);
            obj.transform.localPosition = new Vector3(Random.Range(-20.0f, 20.0f), 0, 0);
            obj.GetComponentInChildren<SpriteRenderer>().color = colors[Random.Range(0, colors.Length)];
            float scale = Random.Range(0.8f, 1.2f);
            obj.transform.localScale = new Vector3(Random.Range(0,2) == 0 ? -scale : scale, scale, scale);
            strands.Add(new Strand{obj = obj, targetRot = 0, rot = 0});
        }
    }

    // Update is called once per frame
    void Update() {
        timer -= Time.deltaTime;
        if (timer < 0.0f) {
            timer = Random.Range(0.5f, 2.0f);
            GenWind();
        }
        foreach (var strand in strands) {
            float diff = strand.targetRot - strand.rot;
            strand.rot += diff  * Time.deltaTime;
            strand.obj.transform.localRotation = Quaternion.Euler(0, 0, strand.rot);
        }
    }

    void GenWind() {
        float wind = windSound.GetWind();
        foreach (var strand in strands) {
            strand.targetRot = Random.Range(0.2f, 1.0f) * wind * -45;
        }
    }
    
    
}
