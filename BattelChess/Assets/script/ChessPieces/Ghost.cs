using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        
        // Kill move
        for (int i = 0; i < tileCountX; i++)
        {
            for (int j = 0; j < tileCountY; j++)
            {
                r.Add(new Vector2Int(i, j));
            }
        }

        r.Remove(new Vector2Int(currentX, currentY));

        return r;
    }
}
