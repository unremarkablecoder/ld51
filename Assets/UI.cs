using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour {
    [SerializeField] private GameObject preStart;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private GameObject results;
    [SerializeField] private Root root;
    [SerializeField] private GameObject soundDisabledIcon;

    void Start() {
        preStart.SetActive(true);
        gameOver.SetActive(false);
        results.SetActive(false);
    }
    
    public void OnStart() {
        preStart.SetActive(false);
        root.StartGame();
    }

    public void ShowGameOver() {
        gameOver.SetActive(true);
    }

    public void OnResults() {
        gameOver.SetActive(false);
        results.SetActive(true);
    }

    public void OnRestart() {
        results.SetActive(false);
        root.RestartGame();
        root.StartGame();
    }

    public void ToggleSound() {
        AudioListener.pause = !AudioListener.pause;
        soundDisabledIcon.SetActive(AudioListener.pause);
        
    }
}
