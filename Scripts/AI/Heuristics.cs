//KNUTH, D. E.; MOORE, R. W. An Analysis of Alpha-Beta Pruning. Artificial Intelligence, v. 6, n. 4, p. 293-326, 1975.
//MARSLAND, T. A. Artificial Intelligence: An Introduction. 4. ed. Boca Raton: CRC Press, 2015.

using UnityEngine;
using System.Collections.Generic;

namespace ChessEngine
{
    public static class Heuristics
    {
        private static readonly int[] PawnPST = new int[] {
            0, 0, 0, 0, 0, 0, 0, 0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5, 5, 10, 25, 25, 10, 5, 5,
            0, 0, 0, 20, 20, 0, 0, 0,
            5, -5, -10, 0, 0, -10, -5, 5,
            5, 10, 10, -20, -20, 10, 10, 5,
            0, 0, 0, 0, 0, 0, 0, 0
        };

        private static readonly int[] KnightPST = new int[] {
            -50, -40, -30, -30, -30, -30, -40, -50,
            -40, -20, 0, 0, 0, 0, -20, -40,
            -30, 0, 10, 15, 15, 10, 0, -30,
            -30, 5, 15, 20, 20, 15, 5, -30,
            -30, 0, 15, 20, 20, 15, 0, -30,
            -30, 5, 10, 15, 15, 10, 5, -30,
            -40, -20, 0, 5, 5, 0, -20, -40,
            -50, -40, -30, -30, -30, -30, -40, -50
        };

        private static readonly int[] BishopPST = new int[] {
            -20, -10, -10, -10, -10, -10, -10, -20,
            -10, 0, 0, 0, 0, 0, 0, -10,
            -10, 0, 5, 10, 10, 5, 0, -10,
            -10, 5, 5, 10, 10, 5, 5, -10,
            -10, 0, 10, 10, 10, 10, 0, -10,
            -10, 10, 10, 10, 10, 10, 10, -10,
            -10, 5, 0, 0, 0, 0, 5, -10,
            -20, -10, -10, -10, -10, -10, -10, -20
        };

        private static readonly int[] RookPST = new int[] {
            0, 0, 0, 0, 0, 0, 0, 0,
            5, 10, 10, 10, 10, 10, 10, 5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            0, 0, 0, 5, 5, 0, 0, 0
        };

        private static readonly int[] QueenPST = new int[] {
            -20, -10, -10, -5, -5, -10, -10, -20,
            -10, 0, 0, 0, 0, 0, 0, -10,
            -10, 0, 5, 5, 5, 5, 0, -10,
            -5, 0, 5, 5, 5, 5, 0, -5,
            0, 0, 5, 5, 5, 5, 0, -5,
            -10, 5, 5, 5, 5, 5, 0, -10,
            -10, 0, 5, 0, 0, 0, 0, -10,
            -20, -10, -10, -5, -5, -10, -10, -20
        };

        private static readonly int[] KingPST_Midgame = new int[] {
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -20, -30, -30, -40, -40, -30, -30, -20,
            -10, -20, -20, -20, -20, -20, -20, -10,
            20, 20, 0, 0, 0, 0, 20, 20,
            20, 30, 10, 0, 0, 10, 30, 20
        };

        private static readonly int[] KingPST_Endgame = new int[] {
            -50, -40, -30, -20, -20, -30, -40, -50,
            -30, -20, -10, 0, 0, -10, -20, -30,
            -30, -10, 20, 30, 30, 20, -10, -30,
            -30, -10, 30, 40, 40, 30, -10, -30,
            -30, -10, 30, 40, 40, 30, -10, -30,
            -30, -10, 20, 30, 30, 20, -10, -30,
            -30, -30, 0, 0, 0, 0, -30, -30,
            -50, -30, -30, -30, -30, -30, -30, -50
        };

        private static int GetPSTValue(Piece piece, int rank, int file, bool isEndgame)
        {
            int squareIndex = piece.color == PlayerColor.White ? (rank * 8 + file) : ((7 - rank) * 8 + file);
            switch (piece.type)
            {
                case PieceType.Pawn: return PawnPST[squareIndex];
                case PieceType.Knight: return KnightPST[squareIndex];
                case PieceType.Bishop: return BishopPST[squareIndex];
                case PieceType.Rook: return RookPST[squareIndex];
                case PieceType.Queen: return QueenPST[squareIndex];
                case PieceType.King: return isEndgame ? KingPST_Endgame[squareIndex] : KingPST_Midgame[squareIndex];
                default: return 0;
            }
        }

        public static int EvaluatePosition(Board board, PlayerColor player)
        {
            int materialScore = 0;
            int positionalScore = 0;
            bool isEndgame = IsEndgame(board);

            foreach (var piece in board.GetAllPieces())
            {
                int value = board.GetPieceValue(piece.type);
                int pstValue = GetPSTValue(piece, piece.rank, piece.file, isEndgame);

                if (piece.color == player)
                {
                    materialScore += value;
                    positionalScore += pstValue;
                }
                else
                {
                    materialScore -= value;
                    positionalScore -= pstValue;
                }
            }

            int kingSafetyScore = EvaluateKingSafetyForPlayer(board, player) - EvaluateKingSafetyForPlayer(board, player == PlayerColor.White ? PlayerColor.Black : PlayerColor.White);

            return materialScore + positionalScore + kingSafetyScore;
        }

        private static int EvaluateKingSafetyForPlayer(Board board, PlayerColor player)
        {
            Piece king = board.GetKing(player);
            if (king == null) return -10000;

            int rank = king.rank;
            int file = king.file;

            int safetyScore = 0;
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int df = -1; df <= 1; df++)
                {
                    if (dr == 0 && df == 0) continue;
                    int r = rank + dr;
                    int f = file + df;
                    if (board.IsInsideBoard(r, f))
                    {
                        Piece p = board.GetPieceAt(r, f);
                        if (p != null && p.color == player && p.type == PieceType.Pawn)
                        {
                            safetyScore += 10;
                        }
                    }
                }
            }

            return safetyScore;
        }

        private static bool IsEndgame(Board board)
        {
            int totalQueens = 0;
            int totalMinorPieces = 0;

            foreach (var piece in board.GetAllPieces())
            {
                if (piece.type == PieceType.Queen)
                    totalQueens++;
                if (piece.type == PieceType.Bishop || piece.type == PieceType.Knight)
                    totalMinorPieces++;
            }

            return totalQueens == 0 || (totalQueens == 2 && totalMinorPieces <= 2);
        }
    }
}
