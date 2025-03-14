using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreText : MonoBehaviour
{
    public static ScoreText instance;
    public int playerScore;
    public Text scoreText;

    public void AddPoints(int pointsToAdd){
        playerScore+=pointsToAdd;
        UpdateScoreText();
    }
    private void UpdateScoreText(){
        if (scoreText != null)
        {
            scoreText.text = "Score: " + playerScore;
        }   
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
