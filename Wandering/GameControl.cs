using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameControl : MonoBehaviour {
	public float gameTime;
	
	private Text timerText, scoreText;
	private int score, maxScore;
	private float timeLeft;
	
	void Awake() {
		score = 0;
		timeLeft = gameTime * 60;
	}
	
	void Start() {
		scoreText = GameObject.Find ("ScoreUI").GetComponent<Text>();
		SetScoreText();
		timerText = GameObject.Find ("TimerUI").GetComponent<Text>();

		maxScore = 0;
		foreach (GameObject rack in GameObject.FindGameObjectsWithTag (Tags.rack))
			maxScore += rack.GetComponent<WeaponRack>().nTools;
	}
	
	void Update() {
		timeLeft -= Time.deltaTime;
		int currentTime = Mathf.FloorToInt (gameTime * 60 - timeLeft);
		int min = currentTime / 60;
		int sec = currentTime % 60;
		timerText.text = min + ":" + sec.ToString ("D2");

		if (score >= maxScore || timeLeft <= 0)
			EndGame();
	}
	
	public void IncreaseScore (int n) {
		score += n;
		SetScoreText();
	}

	public void EndGame() {
		PlayerPrefs.SetInt ("Score", score);
		PlayerPrefs.SetString ("Time", timerText.text);
		Application.LoadLevel ("Wandering End");
	}

	
	private void SetScoreText() {
		scoreText.text = "Score: " + score;
	}
}