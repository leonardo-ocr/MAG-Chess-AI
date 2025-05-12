using UnityEngine;
using System.Collections.Generic;

namespace ChessEngine
{
    public class MoveGenerator
    {
        private Board board;
        private List<Move> moves;

        private static readonly (int dr, int df)[] knightOffsets = {
            (-2, -1), (-2, 1), (-1, -2), (-1, 2),
            (1, -2), (1, 2), (2, -1), (2, 1)
        };
        private static readonly (int dr, int df)[] kingOffsets = {
            (-1, -1), (-1, 0), (-1, 1),
            (0, -1),           (0, 1),
            (1, -1), (1, 0), (1, 1)
        };
        private static readonly (int dr, int df)[] slidingPieceOffsets = {
            (-1, 0), (1, 0), (0, -1), (0, 1),
            (-1, -1), (-1, 1), (1, -1), (1, 1)
        };

        public MoveGenerator(Board board)
        {
            this.board = board;
            this.moves = new List<Move>(64);
        }

        public List<Move> GenerateLegalMoves()
        {
            moves.Clear();
            PlayerColor currentPlayer = board.CurrentPlayer;
            Square kingSquare = board.FindKingSquare(currentPlayer);

            List<Move> pseudoLegalMoves = GeneratePseudoLegalMoves(currentPlayer);

            foreach (Move pseudoMove in pseudoLegalMoves)
            {
                board.MakeMove(pseudoMove);
                if (!IsSquareAttacked(board.FindKingSquare(currentPlayer), Piece.GetOppositeColor(currentPlayer)))
                {
                    moves.Add(pseudoMove);
                }
                board.UndoMove();
            }
            return moves;
        }

        private List<Move> GeneratePseudoLegalMoves(PlayerColor playerColor)
        {
            List<Move> pseudoMoves = new List<Move>(64);
            for (int r = 0; r < 8; r++)
            {
                for (int f = 0; f < 8; f++)
                {
                    Square currentSquare = new Square(r, f);
                    Piece piece = board.GetPieceAt(currentSquare);

                    if (!piece.IsNone() && piece.color == playerColor)
                    {
                        switch (piece.type)
                        {
                            case PieceType.Pawn: GeneratePawnMoves(currentSquare, piece, pseudoMoves); break;
                            case PieceType.Knight: GenerateKnightMoves(currentSquare, piece, pseudoMoves); break;
                            case PieceType.Bishop: GenerateSlidingMoves(currentSquare, piece, 4, 8, pseudoMoves); break;
                            case PieceType.Rook: GenerateSlidingMoves(currentSquare, piece, 0, 4, pseudoMoves); break;
                            case PieceType.Queen: GenerateSlidingMoves(currentSquare, piece, 0, 8, pseudoMoves); break;
                            case PieceType.King: GenerateKingMoves(currentSquare, piece, pseudoMoves); break;
                        }
                    }
                }
            }
            return pseudoMoves;
        }

        private void GeneratePawnMoves(Square fromSquare, Piece pawn, List<Move> moveList)
        {
            int direction = (pawn.color == PlayerColor.White) ? 1 : -1;
            int startRank = (pawn.color == PlayerColor.White) ? 1 : 6;
            int promotionRank = (pawn.color == PlayerColor.White) ? 7 : 0;

            Square toSquare = new Square(fromSquare.rank + direction, fromSquare.file);
            if (toSquare.IsValid() && board.GetPieceAt(toSquare).IsNone())
            {
                if (toSquare.rank == promotionRank)
                {
                    AddPromotionMoves(fromSquare, toSquare, MoveFlags.None, moveList);
                }
                else
                {
                    moveList.Add(new Move(fromSquare, toSquare));
                }

                if (fromSquare.rank == startRank)
                {
                    Square twoForwardSquare = new Square(fromSquare.rank + 2 * direction, fromSquare.file);
                    if (twoForwardSquare.IsValid() && board.GetPieceAt(twoForwardSquare).IsNone())
                    {
                        moveList.Add(new Move(fromSquare, twoForwardSquare, MoveFlags.PawnTwoForward));
                    }
                }
            }

            int[] captureFiles = { fromSquare.file - 1, fromSquare.file + 1 };
            foreach (int targetFile in captureFiles)
            {
                Square captureToSquare = new Square(fromSquare.rank + direction, targetFile);
                if (captureToSquare.IsValid())
                {
                    Piece targetPiece = board.GetPieceAt(captureToSquare);
                    if (!targetPiece.IsNone() && targetPiece.color != pawn.color)
                    {
                        if (captureToSquare.rank == promotionRank)
                        {
                            AddPromotionMoves(fromSquare, captureToSquare, MoveFlags.Capture, moveList);
                        }
                        else
                        {
                            moveList.Add(new Move(fromSquare, captureToSquare, MoveFlags.Capture));
                        }
                    }
                    else if (board.EnPassantTargetSquare.HasValue && captureToSquare == board.EnPassantTargetSquare.Value)
                    {
                        moveList.Add(new Move(fromSquare, captureToSquare, MoveFlags.EnPassantCapture | MoveFlags.Capture));
                    }
                }
            }
        }

        private void AddPromotionMoves(Square from, Square to, MoveFlags existingFlags, List<Move> moveList)
        {
            moveList.Add(new Move(from, to, existingFlags | MoveFlags.Promotion, PieceType.Queen));
            moveList.Add(new Move(from, to, existingFlags | MoveFlags.Promotion, PieceType.Rook));
            moveList.Add(new Move(from, to, existingFlags | MoveFlags.Promotion, PieceType.Bishop));
            moveList.Add(new Move(from, to, existingFlags | MoveFlags.Promotion, PieceType.Knight));
        }

        private void GenerateKnightMoves(Square fromSquare, Piece knight, List<Move> moveList)
        {
            foreach (var offset in knightOffsets)
            {
                Square toSquare = new Square(fromSquare.rank + offset.dr, fromSquare.file + offset.df);
                if (toSquare.IsValid())
                {
                    Piece targetPiece = board.GetPieceAt(toSquare);
                    if (targetPiece.IsNone())
                    {
                        moveList.Add(new Move(fromSquare, toSquare));
                    }
                    else if (targetPiece.color != knight.color)
                    {
                        moveList.Add(new Move(fromSquare, toSquare, MoveFlags.Capture));
                    }
                }
            }
        }

        private void GenerateSlidingMoves(Square fromSquare, Piece piece, int startOffsetIndex, int endOffsetIndex, List<Move> moveList)
        {
            for (int i = startOffsetIndex; i < endOffsetIndex; i++)
            {
                var offset = slidingPieceOffsets[i];
                for (int step = 1; step < 8; step++)
                {
                    Square toSquare = new Square(fromSquare.rank + offset.dr * step, fromSquare.file + offset.df * step);
                    if (!toSquare.IsValid()) break;

                    Piece targetPiece = board.GetPieceAt(toSquare);
                    if (targetPiece.IsNone())
                    {
                        moveList.Add(new Move(fromSquare, toSquare));
                    }
                    else
                    {
                        if (targetPiece.color != piece.color)
                        {
                            moveList.Add(new Move(fromSquare, toSquare, MoveFlags.Capture));
                        }
                        break;
                    }
                }
            }
        }

        private void GenerateKingMoves(Square fromSquare, Piece king, List<Move> moveList)
        {
            foreach (var offset in kingOffsets)
            {
                Square toSquare = new Square(fromSquare.rank + offset.dr, fromSquare.file + offset.df);
                if (toSquare.IsValid())
                {
                    Piece targetPiece = board.GetPieceAt(toSquare);
                    if (targetPiece.IsNone())
                    {
                        moveList.Add(new Move(fromSquare, toSquare));
                    }
                    else if (targetPiece.color != king.color)
                    {
                        moveList.Add(new Move(fromSquare, toSquare, MoveFlags.Capture));
                    }
                }
            }
            GenerateCastlingMoves(fromSquare, king, moveList);
        }

        private void GenerateCastlingMoves(Square kingFromSquare, Piece king, List<Move> moveList)
        {
            if (king.hasMoved) return;
            if (IsSquareAttacked(kingFromSquare, Piece.GetOppositeColor(king.color))) return;

            bool canKingSide = (king.color == PlayerColor.White) ? board.WhiteKingSideCastle : board.BlackKingSideCastle;
            if (canKingSide)
            {
                Square rookSquare = new Square(kingFromSquare.rank, 7);
                Piece rook = board.GetPieceAt(rookSquare);
                if (rook.type == PieceType.Rook && rook.color == king.color && !rook.hasMoved)
                {
                    Square pathSquare1 = new Square(kingFromSquare.rank, kingFromSquare.file + 1);
                    Square pathSquare2 = new Square(kingFromSquare.rank, kingFromSquare.file + 2);
                    if (board.GetPieceAt(pathSquare1).IsNone() && board.GetPieceAt(pathSquare2).IsNone())
                    {
                        if (!IsSquareAttacked(pathSquare1, Piece.GetOppositeColor(king.color)) && 
                            !IsSquareAttacked(pathSquare2, Piece.GetOppositeColor(king.color)))
                        {
                            moveList.Add(new Move(kingFromSquare, pathSquare2, MoveFlags.CastleKingSide));
                        }
                    }
                }
            }

            bool canQueenSide = (king.color == PlayerColor.White) ? board.WhiteQueenSideCastle : board.BlackQueenSideCastle;
            if (canQueenSide)
            {
                Square rookSquare = new Square(kingFromSquare.rank, 0);
                Piece rook = board.GetPieceAt(rookSquare);
                if (rook.type == PieceType.Rook && rook.color == king.color && !rook.hasMoved)
                {
                    Square pathSquare1 = new Square(kingFromSquare.rank, kingFromSquare.file - 1);
                    Square pathSquare2 = new Square(kingFromSquare.rank, kingFromSquare.file - 2);
                    Square pathSquare3 = new Square(kingFromSquare.rank, kingFromSquare.file - 3);
                    if (board.GetPieceAt(pathSquare1).IsNone() && 
                        board.GetPieceAt(pathSquare2).IsNone() && 
                        board.GetPieceAt(pathSquare3).IsNone())
                    {
                        if (!IsSquareAttacked(pathSquare1, Piece.GetOppositeColor(king.color)) && 
                            !IsSquareAttacked(pathSquare2, Piece.GetOppositeColor(king.color)))
                        {
                            moveList.Add(new Move(kingFromSquare, pathSquare2, MoveFlags.CastleQueenSide));
                        }
                    }
                }
            }
        }

        public bool IsSquareAttacked(Square targetSquare, PlayerColor attackerColor)
        {
            int pawnDir = (attackerColor == PlayerColor.White) ? 1 : -1;
            Square pawnAttackSq1 = new Square(targetSquare.rank - pawnDir, targetSquare.file - 1);
            Square pawnAttackSq2 = new Square(targetSquare.rank - pawnDir, targetSquare.file + 1);
            if (pawnAttackSq1.IsValid())
            {
                Piece p = board.GetPieceAt(pawnAttackSq1);
                if (p.type == PieceType.Pawn && p.color == attackerColor) return true;
            }
            if (pawnAttackSq2.IsValid())
            {
                Piece p = board.GetPieceAt(pawnAttackSq2);
                if (p.type == PieceType.Pawn && p.color == attackerColor) return true;
            }

            foreach (var offset in knightOffsets)
            {
                Square knightSq = new Square(targetSquare.rank + offset.dr, targetSquare.file + offset.df);
                if (knightSq.IsValid()) {
                    Piece p = board.GetPieceAt(knightSq);
                    if (p.type == PieceType.Knight && p.color == attackerColor) return true;
                }
            }

            for (int i = 0; i < slidingPieceOffsets.Length; i++)
            {
                var offset = slidingPieceOffsets[i];
                for (int step = 1; step < 8; step++)
                {
                    Square currentSq = new Square(targetSquare.rank + offset.dr * step, targetSquare.file + offset.df * step);
                    if (!currentSq.IsValid()) break;
                    Piece p = board.GetPieceAt(currentSq);
                    if (!p.IsNone()) {
                        if (p.color == attackerColor) {
                            bool isRookType = (p.type == PieceType.Rook || p.type == PieceType.Queen);
                            bool isBishopType = (p.type == PieceType.Bishop || p.type == PieceType.Queen);
                            bool isCorrectSlidingType = (i < 4 && isRookType) || (i >= 4 && isBishopType);
                            if (isCorrectSlidingType) return true;
                        }
                        break;
                    }
                }
            }

            foreach (var offset in kingOffsets)
            {
                Square kingSq = new Square(targetSquare.rank + offset.dr, targetSquare.file + offset.df);
                if (kingSq.IsValid()) {
                    Piece p = board.GetPieceAt(kingSq);
                    if (p.type == PieceType.King && p.color == attackerColor) return true;
                }
            }
            return false;
        }
    }
}
