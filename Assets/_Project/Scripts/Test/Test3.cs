using UnityEngine;

public class Player
{
    public Player(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public void Log()
    {
        Debug.Log(Name);
    }
}

public class Test3 : MonoBehaviour
{
    private Player player1 = new("player1");
    private Player player2 = new("player2");

    private void Start()
    {
        player1.Log();
        Utils.Swap(ref player1, ref player2);
        player1.Log();
    }
}