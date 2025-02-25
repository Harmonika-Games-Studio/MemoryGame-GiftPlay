using UnityEngine;

public interface iGame
{
    public int Score { get; set; }

    public void StartGame();
    public void EndGame(bool win);
}
