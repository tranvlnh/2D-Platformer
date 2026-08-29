using UnityEngine;

public class Test : MonoBehaviour
{
    public Vector2 spacing = new(0.2f, 0.2f);
    public Vector2 size = new(1, 1);
    public Vector2Int grid = new(5, 5);


    private void OnDrawGizmos()
    {
        var step = size + spacing;
        var center = (grid - Vector2.one) * 0.5f;
        for (var i = 0; i < grid.x; i++)
        for (var j = 0; j < grid.y; j++)
        {
            var pos = (new Vector2(i, j) - center) * step;
            Gizmos.DrawWireCube((Vector2)transform.position + pos, size);
        }
    }
}