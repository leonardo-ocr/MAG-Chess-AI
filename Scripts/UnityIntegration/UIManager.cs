using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ChessInterface
{
    public class UIManager : MonoBehaviour
    {
        public GameObject boardSquarePrefab;
        public Transform boardContainer;
        public GameObject piecePrefab;
        public Transform piecesContainer;

        public Sprite boardLightSquareSprite;
        public Sprite boardDarkSquareSprite;

        public Sprite whitePawnSprite;
        public Sprite whiteRookSprite;
        public Sprite whiteKnightSprite;
        public Sprite whiteBishopSprite;
        public Sprite whiteQueenSprite;
        public Sprite whiteKingSprite;
        public Sprite blackPawnSprite;
        public Sprite blackRookSprite;
        public Sprite blackKnightSprite;
        public Sprite blackBishopSprite;
        public Sprite blackQueenSprite;
        public Sprite blackKingSprite;

        public Text statusText;
        public Button resetButton;
        public Dropdown aiStyleDropdown;

        private GameObject[,] boardSquareObjects;
        private Dictionary<ChessEngine.Piece, GameObject> pieceGameObjects;

        private ChessEngine.Board internalBoardState;
        private float squareSize = 1.0f;

        void Start()
        {
        }

        public void Initialize(ChessEngine.Board boardLogic, float boardSquareSize)
        {
            this.internalBoardState = boardLogic;
            this.squareSize = boardSquareSize;
            this.pieceGameObjects = new Dictionary<ChessEngine.Piece, GameObject>();
            CreateBoardUI();
            UpdateBoardPiecesUI(boardLogic);
        }

        void CreateBoardUI()
        {
            if (boardContainer == null || boardSquarePrefab == null) {
                Debug.LogError("Board container or square prefab not assigned in UIManager!");
                return;
            }

            boardSquareObjects = new GameObject[8, 8];
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    GameObject squareGO = Instantiate(boardSquarePrefab, boardContainer);
                    squareGO.name = $"Square_{rank}_{file}";
                    squareGO.transform.localPosition = new Vector3(file * squareSize, rank * squareSize, 0);
                    
                    SpriteRenderer sr = squareGO.GetComponent<SpriteRenderer>();
                    if (sr == null) sr = squareGO.AddComponent<SpriteRenderer>();

                    if ((rank + file) % 2 == 0)
                        sr.sprite = boardDarkSquareSprite;
                    else
                        sr.sprite = boardLightSquareSprite;
                    
                    boardSquareObjects[rank, file] = squareGO;

                    if (squareGO.GetComponent<BoxCollider2D>() == null)
                    {
                        BoxCollider2D col = squareGO.AddComponent<BoxCollider2D>();
                        col.size = new Vector2(squareSize, squareSize);
                    }
                }
            }
        }

        public void UpdateBoardPiecesUI(ChessEngine.Board currentBoard)
        {
            if (piecesContainer == null || piecePrefab == null) {
                Debug.LogError("Pieces container or piece prefab not assigned!");
                return;
            }

            foreach (var kvp in pieceGameObjects)
            {
                Destroy(kvp.Value);
            }
            pieceGameObjects.Clear();

            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    ChessEngine.Piece piece = currentBoard.GetPieceAt(rank, file);
                    if (!piece.IsNone())
                    {
                        GameObject pieceGO = Instantiate(piecePrefab, piecesContainer);
                        pieceGO.name = $"{piece.color}_{piece.type}_{rank}_{file}";
                        pieceGO.transform.localPosition = new Vector3(file * squareSize, rank * squareSize, -0.1f);
                        
                        SpriteRenderer sr = pieceGO.GetComponent<SpriteRenderer>();
                        if (sr == null) sr = pieceGO.AddComponent<SpriteRenderer>();
                        sr.sprite = GetSpriteForPiece(piece);
                        
                        pieceGameObjects[piece] = pieceGO;
                    }
                }
            }
        }

        public Sprite GetSpriteForPiece(ChessEngine.Piece piece)
        {
            if (piece.IsNone()) return null;
            switch (piece.color)
            {
                case ChessEngine.PlayerColor.White:
                    switch (piece.type)
                    {
                        case ChessEngine.PieceType.Pawn: return whitePawnSprite;
                        case ChessEngine.PieceType.Rook: return whiteRookSprite;
                        case ChessEngine.PieceType.Knight: return whiteKnightSprite;
                        case ChessEngine.PieceType.Bishop: return whiteBishopSprite;
                        case ChessEngine.PieceType.Queen: return whiteQueenSprite;
                        case ChessEngine.PieceType.King: return whiteKingSprite;
                    }
                    break;
                case ChessEngine.PlayerColor.Black:
                    switch (piece.type)
                    {
                        case ChessEngine.PieceType.Pawn: return blackPawnSprite;
                        case ChessEngine.PieceType.Rook: return blackRookSprite;
                        case ChessEngine.PieceType.Knight: return blackKnightSprite;
                        case ChessEngine.PieceType.Bishop: return blackBishopSprite;
                        case ChessEngine.PieceType.Queen: return blackQueenSprite;
                        case ChessEngine.PieceType.King: return blackKingSprite;
                    }
                    break;
            }
            return null;
        }

        public void HighlightSquare(int rank, int file, Color color)
        {
            if (boardSquareObjects[rank, file] != null)
            {
                SpriteRenderer sr = boardSquareObjects[rank, file].GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = color;
            }
        }

        public void ClearAllHighlights()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int f = 0; f < 8; f++)
                {
                    if (boardSquareObjects[r, f] != null)
                    {
                        SpriteRenderer sr = boardSquareObjects[r, f].GetComponent<SpriteRenderer>();
                        if (sr != null) sr.color = Color.white;
                    }
                }
            }
        }

        public void ShowMessage(string message)
        {
            if (statusText != null) statusText.text = message;
            else Debug.Log("Status: " + message);
        }

        public void SetupAIDropdown(List<string> styleNames, System.Action<int> onSelectionChanged)
        {
            if (aiStyleDropdown != null)
            {
                aiStyleDropdown.ClearOptions();
                aiStyleDropdown.AddOptions(styleNames);
                aiStyleDropdown.onValueChanged.AddListener((index) => onSelectionChanged(index));
            }
        }

        public ChessEngine.PieceType GetPromotionChoiceFromUI() {
            Debug.LogWarning("Pawn promotion UI not implemented. Auto-promoting to Queen.");
            return ChessEngine.PieceType.Queen;
        }
    }
}
