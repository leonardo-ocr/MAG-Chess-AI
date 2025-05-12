//KNUTH, D. E.; MOORE, R. W. An Analysis of Alpha-Beta Pruning. Artificial Intelligence, v. 6, n. 4, p. 293-326, 1975.
//KARPOV, A. My 300 best games. Moscou: Russian Chess House; Chess V.I.P.’s, 1997.
//TAL, M. The Life and Games of Mikhail Tal. New York: Hart Publishing, 1991
//KASPAROV, G. My Great Predecessors. v. 1–5. London: Everyman Chess, 2003.

using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessEngine
{
    public class AIEngine
    {
        private PlayerStyleProfile currentStyle;
        private MoveGenerator moveGenerator;
        private Board board;
        private int searchDepth = 4;

        private Move bestMoveFound;
        private bool cancelSearch = false;

        public AIEngine(Board board, PlayerStyleProfile style, int depth = 4)
        {
            this.board = board;
            this.currentStyle = style;
            this.searchDepth = depth;
            this.moveGenerator = new MoveGenerator(this.board);
        }

        public void SetStyle(PlayerStyleProfile style)
        {
            this.currentStyle = style;
        }

        public void SetSearchDepth(int depth)
        {
            this.searchDepth = Mathf.Max(1, depth);
        }

        public Move FindBestMoveSync(Board currentBoardState)
        {
            this.board.LoadPositionFromFEN(currentBoardState.GetCurrentFEN());
            this.moveGenerator = new MoveGenerator(this.board);
            this.cancelSearch = false;
            this.bestMoveFound = default(Move);

            AlphaBeta(searchDepth, float.NegativeInfinity, float.PositiveInfinity, board.CurrentPlayer == PlayerColor.White);
            
            if (bestMoveFound.Equals(default(Move))) {
                 List<Move> legalMoves = moveGenerator.GenerateLegalMoves();
                 if (legalMoves.Count > 0) return legalMoves[0];
            }
            return bestMoveFound;
        }

        public async Task<Move> FindBestMoveAsync(Board currentBoardState)
        {
            this.board.LoadPositionFromFEN(currentBoardState.GetCurrentFEN());
            this.moveGenerator = new MoveGenerator(this.board);
            this.cancelSearch = false;
            this.bestMoveFound = default(Move);

            await Task.Run(() => AlphaBeta(searchDepth, float.NegativeInfinity, float.PositiveInfinity, board.CurrentPlayer == PlayerColor.White));
            
            if (bestMoveFound.Equals(default(Move))) {
                 List<Move> legalMoves = moveGenerator.GenerateLegalMoves();
                 if (legalMoves.Count > 0) return legalMoves[0];
            }
            return bestMoveFound;
        }

        public void RequestSearchCancellation()
        {
            this.cancelSearch = true;
        }

        private float AlphaBeta(int depth, float alpha, float beta, bool maximizingPlayer)
        {
            if (cancelSearch) return 0;

            if (depth == 0)
            {
                return Heuristics.EvaluatePosition(board, currentStyle, this.moveGenerator);
            }

            List<Move> legalMoves = moveGenerator.GenerateLegalMoves();
            if (legalMoves.Count == 0) {
                if (moveGenerator.IsSquareAttacked(board.FindKingSquare(board.CurrentPlayer), Piece.GetOppositeColor(board.CurrentPlayer))) {
                    return maximizingPlayer ? float.NegativeInfinity + (searchDepth - depth) : float.PositiveInfinity - (searchDepth - depth);
                } else {
                    return 0;
                }
            }

            if (maximizingPlayer)
            {
                float maxEval = float.NegativeInfinity;
                foreach (Move move in legalMoves)
                {
                    board.MakeMove(move);
                    float eval = AlphaBeta(depth - 1, alpha, beta, false);
                    board.UndoMove();
                    if (cancelSearch) break;

                    if (eval > maxEval)
                    {
                        maxEval = eval;
                        if (depth == searchDepth)
                        {
                            bestMoveFound = move;
                        }
                    }
                    alpha = Mathf.Max(alpha, eval);
                    if (beta <= alpha) break;
                }
                return maxEval;
            }
            else
            {
                float minEval = float.PositiveInfinity;
                foreach (Move move in legalMoves)
                {
                    board.MakeMove(move);
                    float eval = AlphaBeta(depth - 1, alpha, beta, true);
                    board.UndoMove();
                    if (cancelSearch) break;

                    if (eval < minEval)
                    {
                        minEval = eval;
                        if (depth == searchDepth)
                        {
                            bestMoveFound = move; 
                        }
                    }
                    beta = Mathf.Min(beta, eval);
                    if (beta <= alpha) break;
                }
                return minEval;
            }
        }
    }
}
