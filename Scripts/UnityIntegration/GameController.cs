using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChessEngine; // Namespace for Board, Piece, Move, AIEngine etc.

namespace ChessInterface
{
    public class GameController : MonoBehaviour
    {
        public UIManager uiManager;
        public GameObject boardInteractionPlane;

        private Board gameBoard;
        private AIEngine aiEngine;
        private MoveGenerator moveGenerator;

        private PlayerColor humanPlayerColor = PlayerColor.White;
        private PlayerColor aiPlayerColor = PlayerColor.Black;
        private AIPlayerStyle currentAIStyle = AIPlayerStyle.Default;
        private int aiSearchDepth = 4;

        private Square selectedSquare = Square.Invalid;
        private List<Move> legalMovesForSelectedPiece = new List<Move>();

        private bool isPlayerTurn = true;
        private bool isProcessingMove = false;
        private bool isGameOver = false;

        void Start()
        {
            if (uiManager == null)
            {
                Debug.LogError("UIManager not assigned to GameController!");
                return;
            }

            InitializeGame();
            SetupUIInteractions();
        }

        void InitializeGame()
        {
            gameBoard = new Board();
            gameBoard.SetupStandardBoard();
            moveGenerator = new MoveGenerator(gameBoard);

            currentAIStyle = AIPlayerStyle.Kasparov;
            PlayerStyleProfile initialProfile = PlayerStyleProfile.GetProfile(currentAIStyle);
            aiEngine = new AIEngine(new Board(gameBoard.GetCurrentFEN()), initialProfile, aiSearchDepth);

            float squareSize = 1.0f;
            uiManager.Initialize(gameBoard, squareSize);

            isPlayerTurn = (humanPlayerColor == gameBoard.CurrentPlayer);
            isGameOver = false;
            isProcessingMove = false;
            selectedSquare = Square.Invalid;
            legalMovesForSelectedPiece.Clear();

            UpdateStatusMessage();
            CheckAndTriggerAIMove();
        }

        void SetupUIInteractions()
        {
            List<string> styleNames = new List<string>();
            foreach (AIPlayerStyle style in System.Enum.GetValues(typeof(AIPlayerStyle)))
            {
                styleNames.Add(style.ToString());
            }
            uiManager.SetupAIDropdown(styleNames, OnAIStyleSelected);
            if (uiManager.aiStyleDropdown != null)
            {
                int currentStyleIndex = styleNames.FindIndex(s => s == currentAIStyle.ToString());
                if (currentStyleIndex != -1) uiManager.aiStyleDropdown.value = currentStyleIndex;
            }

            if (uiManager.resetButton != null)
            {
                uiManager.resetButton.onClick.AddListener(InitializeGame);
            }
        }

        void Update()
        {
            if (isGameOver || isProcessingMove || !isPlayerTurn) return;

            if (Input.GetMouseButtonDown(0))
            {
                HandlePlayerInput();
            }
        }

        void HandlePlayerInput()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null)
            {
                Vector3 localHitPoint = uiManager.boardContainer.InverseTransformPoint(hit.point);
                float squareSize = 1.0f;
                int file = Mathf.FloorToInt(localHitPoint.x / squareSize);
                int rank = Mathf.FloorToInt(localHitPoint.y / squareSize);
                Square clickedSquare = new Square(rank, file);

                if (!clickedSquare.IsValid()) return;

                Piece pieceOnClickedSquare = gameBoard.GetPieceAt(clickedSquare);

                if (selectedSquare.IsValid())
                {
                    Move attemptedMove = new Move(selectedSquare, clickedSquare);
                    bool isValidAttempt = false;
                    foreach (Move legalMove in legalMovesForSelectedPiece)
                    {
                        if (legalMove.toSquare.Equals(clickedSquare))
                        {
                            attemptedMove = legalMove;
                            isValidAttempt = true;
                            break;
                        }
                    }

                    if (isValidAttempt)
                    {
                        if (attemptedMove.IsPromotion())
                        {
                            attemptedMove.promotionPieceType = uiManager.GetPromotionChoiceFromUI();
                        }
                        MakePlayerMove(attemptedMove);
                    }
                    else
                    {
                        selectedSquare = Square.Invalid;
                        legalMovesForSelectedPiece.Clear();
                        uiManager.ClearAllHighlights();
                        if (!pieceOnClickedSquare.IsNone() && pieceOnClickedSquare.color == humanPlayerColor)
                        {
                            SelectPiece(clickedSquare);
                        }
                    }
                }
                else
                {
                    if (!pieceOnClickedSquare.IsNone() && pieceOnClickedSquare.color == humanPlayerColor)
                    {
                        SelectPiece(clickedSquare);
                    }
                }
            }
        }

        void SelectPiece(Square square)
        {
            selectedSquare = square;
            legalMovesForSelectedPiece = moveGenerator.GenerateLegalMovesForPiece(square);

            uiManager.ClearAllHighlights();
            uiManager.HighlightSquare(square.rank, square.file, Color.yellow);
            foreach (Move move in legalMovesForSelectedPiece)
            {
                uiManager.HighlightSquare(move.toSquare.rank, move.toSquare.file, Color.green);
            }
        }

        async void MakePlayerMove(Move move)
        {
            isProcessingMove = true;
            gameBoard.MakeMove(move);
            uiManager.UpdateBoardPiecesUI(gameBoard);
            uiManager.ClearAllHighlights();
            selectedSquare = Square.Invalid;
            legalMovesForSelectedPiece.Clear();

            if (CheckGameOver()) return;

            isPlayerTurn = false;
            UpdateStatusMessage();
            await Task.Delay(100);
            TriggerAIMove();
            isProcessingMove = false;
        }

        async void TriggerAIMove()
        {
            if (isGameOver || gameBoard.CurrentPlayer != aiPlayerColor) return;

            isProcessingMove = true;
            uiManager.ShowMessage($"{aiPlayerColor} (AI - {currentAIStyle}) is thinking...");

            Board boardCopyForAI = new Board(gameBoard.GetCurrentFEN());
            Move aiMove = await aiEngine.FindBestMoveAsync(boardCopyForAI);

            if (isGameOver) return;

            if (!aiMove.Equals(default(Move)) && gameBoard.IsMoveLegal(aiMove, moveGenerator.GenerateLegalMoves()))
            {
                gameBoard.MakeMove(aiMove);
                uiManager.UpdateBoardPiecesUI(gameBoard);
                uiManager.HighlightSquare(aiMove.fromSquare.rank, aiMove.fromSquare.file, Color.blue);
                uiManager.HighlightSquare(aiMove.toSquare.rank, aiMove.toSquare.file, Color.cyan);
            }
            else
            {
                Debug.LogError("AI proposed an invalid or null move!");
                List<Move> anyLegalMoves = new MoveGenerator(gameBoard).GenerateLegalMoves();
                if (anyLegalMoves.Count > 0)
                {
                    gameBoard.MakeMove(anyLegalMoves[0]);
                    uiManager.UpdateBoardPiecesUI(gameBoard);
                    uiManager.ShowMessage("AI fallback move made.");
                }
            }

            if (CheckGameOver()) return;

            isPlayerTurn = true;
            UpdateStatusMessage();
            isProcessingMove = false;
        }

        bool CheckGameOver()
        {
            GameState gameState = gameBoard.GetGameState(moveGenerator);
            if (gameState != GameState.Ongoing)
            {
                isGameOver = true;
                isProcessingMove = false;
                string message = gameState switch
                {
                    GameState.Checkmate => $"Checkmate! {Piece.GetOppositeColor(gameBoard.CurrentPlayer)} wins.",
                    GameState.Stalemate => "Stalemate! It's a draw.",
                    GameState.DrawByRepetition => "Draw by threefold repetition.",
                    GameState.DrawByFiftyMoveRule => "Draw by fifty-move rule.",
                    GameState.DrawByInsufficientMaterial => "Draw by insufficient material.",
                    _ => ""
                };
                uiManager.ShowMessage(message);
                return true;
            }
            return false;
        }

        void UpdateStatusMessage()
        {
            if (isGameOver) return;
            uiManager.ShowMessage($"{gameBoard.CurrentPlayer}'s turn. Style: {(gameBoard.CurrentPlayer == aiPlayerColor ? currentAIStyle.ToString() : "Human")}");
        }

        void CheckAndTriggerAIMove()
        {
            if (!isPlayerTurn && !isGameOver && gameBoard.CurrentPlayer == aiPlayerColor)
            {
                TriggerAIMove();
            }
        }

        public void OnAIStyleSelected(int styleIndex)
        {
            currentAIStyle = (AIPlayerStyle)styleIndex;
            PlayerStyleProfile newProfile = PlayerStyleProfile.GetProfile(currentAIStyle);
            aiEngine.SetStyle(newProfile);
            Debug.Log($"AI Style changed to: {currentAIStyle}");
            UpdateStatusMessage();
        }

        public void SetAISearchDepth(int depth)
        {
            aiSearchDepth = depth;
            if (aiEngine != null)
            {
                aiEngine.SetSearchDepth(aiSearchDepth);
            }
        }

        public void SetPlayerColor(PlayerColor color)
        {
            humanPlayerColor = color;
            aiPlayerColor = Piece.GetOppositeColor(color);
            InitializeGame();
        }
    }
}
