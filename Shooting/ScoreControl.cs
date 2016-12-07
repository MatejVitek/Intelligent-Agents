using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreControl : MonoBehaviour {
	
	public int gameTime;
	
	private Text timerText, scoreText;
	private int score;
	private float timeLeft;

	// Use this for initialization
	void Awake() {
		score = 0;
		timeLeft = gameTime;
	}
	
	void Start() {
		scoreText = GameObject.Find ("ScoreUI").GetComponent<Text>();
		SetScoreText();
		timerText = GameObject.Find ("TimerUI").GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update() {
		timeLeft -= Time.deltaTime;
		if (timeLeft <= 0) {
			PlayerPrefs.SetInt ("Score", score);
			Application.LoadLevel ("Shooting End");
		}
		int min = Mathf.CeilToInt (timeLeft) / 60;
		int sec = Mathf.CeilToInt (timeLeft) % 60;
		timerText.text = min + ":" + sec.ToString ("D2");
	}

	public void IncreaseScore() {
		score++;
		SetScoreText();
	}

	private void SetScoreText() {
		scoreText.text = "Score: " + score;
	}
}
