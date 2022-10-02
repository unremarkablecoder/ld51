using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBar : MonoBehaviour {
    [SerializeField] private GameObject bar;
    // Start is called before the first frame update
    void Start()
    {
        SetProgress(1);
        
    }


    public void SetProgress(float t) {
        bar.transform.localScale = new Vector3(t, 1, 1);
        bar.transform.localPosition = new Vector3(t / 2, 0, 0);
    }
}
