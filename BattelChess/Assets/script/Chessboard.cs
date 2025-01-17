using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chessboard : MonoBehaviour
{
    public static Chessboard Instance = null;
    
    [Header("Art stuff")] 
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float deathSize;
    [SerializeField] private float deathSpacing;
    [SerializeField] private float dragOffset;

    [Header("Prefabs & Materials")] 
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;  
    
    // LOGIC
    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<ChessPiece> whiteTeam = new List<ChessPiece>();
    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> blackTeam = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private bool isWhiteTurn = true;
    public bool isMove = true;


    public int killSet;
        
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        SpawnAllPieces();
        PositionALlPieces();
    }
    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            //Get the indexes of tile we hit
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            //If we are hovering any tile after not hovering any tile
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            //if we were already hovering a tile, change previous one
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[currentHover.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If we press down on the mouse
            if (Input.GetMouseButtonDown(0) && isMove)
            {
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    // Is it our turn;
                    if ((chessPieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn) ||
                        (chessPieces[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn))
                    {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];

                        // Get a list of where I can go, hightlight tiles as well
                        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);

                        HighlightTiles();
                    }
                }
            }

            // If we are releasing the mouse button
            if (currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);
                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                if (!validMove)
                {
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                }
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }
        else
        {
            if(currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }

            if (currentlyDragging&& Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }
        
        // If we're dragging a piece
        if (currentlyDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, 0);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
            {
                currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
            }
        }
    }

    // Generate the board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, 0, y * tileSize);
        vertices[1] = new Vector3(x * tileSize, 0, (y + 1) * tileSize);
        vertices[2] = new Vector3((x + 1) * tileSize, 0, y * tileSize);
        vertices[3] = new Vector3((x + 1) * tileSize, 0, (y + 1) * tileSize);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        tileObject.layer = LayerMask.NameToLayer("Tile");
        mesh.RecalculateNormals();
        
        tileObject.AddComponent<BoxCollider>();
        
        return tileObject;
    }
    
    // Spawning of the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        int whiteTime = 0, blackTeam = 1;
        
        // White team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTime);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTime);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTime);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTime);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTime);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTime);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTime);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTime);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTime);
        }
        
        // Black team
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        }
        
        
        
    }
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;

        if (cp.team == 0) {
            whiteTeam.Add(cp);
        } else {
            cp.GetComponent<Transform>().rotation = Quaternion.Euler(-90, 0, 90);
            blackTeam.Add(cp);
        }
        
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];
        
        return cp;
    }
    
    // Positioning
    private void PositionALlPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }
    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);// Operations// Operations
    }
    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, 0, y * tileSize) + new Vector3(tileSize / 2, 0, tileSize / 2);
    }
    
    // Highlight Tiles
    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++) {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++) {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }

        availableMoves.Clear();
    }
    
    // Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            if(moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }

        return false;
    }
    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        if (!ContainsValidMove(ref availableMoves, new Vector2(x, y)))
        {
            return false;
        }

        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        // Is there another piece on the target position?
        if (chessPieces[x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];

            if (cp.team == ocp.team)
                return false;

            // If it's the enemy team
            if (ocp.team == 0)
            {
                deadWhites.Add(ocp);
                if (ocp.type == ChessPieceType.Ghost)
                {
                    ocp.SetScale(Vector3.one * 0.5f);
                }
                else
                {
                    ocp.SetScale(Vector3.one * deathSize);
                }
                ocp.SetPosition(new Vector3((9 * tileSize + 1), 0, tileSize * deathSpacing * deadWhites.Count));
                if (deadWhites.Count == whiteTeam.Count)
                {
                    GameManager.Instance.GameEnd("Black");
                }
            }
            else
            {
                deadBlacks.Add(ocp);
                if (ocp.type == ChessPieceType.Ghost)
                {
                    ocp.SetScale(Vector3.one * 0.5f);
                }
                else
                {
                    ocp.SetScale(Vector3.one * deathSize);
                }
                ocp.SetPosition(new Vector3((-tileSize - 1), 0, -tileSize * deathSpacing * deadBlacks.Count + 8));
                if (deadBlacks.Count == blackTeam.Count)
                {
                    GameManager.Instance.GameEnd("White");
                }
            }

            if (cp.type == ChessPieceType.Pawn)
            {
                cp.GetComponent<Pawn>().killCount++;
            }
        }

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        // Pawn promotion to Queen
        if (cp.type == ChessPieceType.Pawn)
        {
            if ((cp.team == 0 && y == TILE_COUNT_Y - 1) || (cp.team == 1 && y == 0))
            {
                // Remove the pawn
                if (cp.team == 0)
                    whiteTeam.Remove(cp);
                else
                    blackTeam.Remove(cp);
            
                Destroy(cp.gameObject);

                // Spawn a new Queen
                ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, cp.team);
                newQueen.currentX = x;
                newQueen.currentY = y;
                newQueen.SetPosition(GetTileCenter(x, y), true);
                chessPieces[x, y] = newQueen;

                if (newQueen.team == 0)
                    whiteTeam.Add(newQueen);
                else
                    blackTeam.Add(newQueen);
            }
            
            if (cp.GetComponent<Pawn>().killCount >= killSet)
            {
                if (cp.team == 0)
                    whiteTeam.Remove(cp);
                else
                    blackTeam.Remove(cp);
            
                Destroy(cp.gameObject);

                // Spawn a new Queen
                ChessPiece newGhost = SpawnSinglePiece(ChessPieceType.Ghost, cp.team);
                newGhost.currentX = x;
                newGhost.currentY = y;
                newGhost.SetPosition(GetTileCenter(x, y), true);
                newGhost.SetScale(Vector3.one);
                chessPieces[x, y] = newGhost;

                if (newGhost.team == 0)
                {
                    newGhost.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                    whiteTeam.Add(newGhost);
                }
                else
                {
                    newGhost.gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                    blackTeam.Add(newGhost);
                }
            }
        }

        isWhiteTurn = !isWhiteTurn;
        CameraManager.Instance.StartCameraMove();

        return true;
    }

    
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);
            }
        }

        return -Vector2Int.one; // Invalid
    }
}
