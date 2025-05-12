using UnityEngine;

namespace ChessEngine
{
    [System.Flags]
    public enum MoveFlags
    {
        None = 0,
        Capture = 1 << 0,
        PawnTwoForward = 1 << 1,
        EnPassantCapture = 1 << 2,
        Promotion = 1 << 3,
        CastleKingSide = 1 << 4,
        CastleQueenSide = 1 << 5
    }

    public readonly struct Move
    {
        public readonly Square FromSquare { get; }
        public readonly Square ToSquare { get; }
        public readonly PieceType PromotionPieceType { get; }
        public readonly MoveFlags Flags { get; }

        public Move(Square fromSquare, Square toSquare, MoveFlags flags = MoveFlags.None, PieceType promotionPieceType = PieceType.None)
        {
            FromSquare = fromSquare;
            ToSquare = toSquare;
            Flags = flags;
            PromotionPieceType = (flags & MoveFlags.Promotion) != 0 ? promotionPieceType : PieceType.None;
        }

        public bool IsCapture => (Flags & MoveFlags.Capture) != 0;
        public bool IsPawnTwoForward => (Flags & MoveFlags.PawnTwoForward) != 0;
        public bool IsEnPassant => (Flags & MoveFlags.EnPassantCapture) != 0;
        public bool IsPromotion => (Flags & MoveFlags.Promotion) != 0;
        public bool IsCastleKingSide => (Flags & MoveFlags.CastleKingSide) != 0;
        public bool IsCastleQueenSide => (Flags & MoveFlags.CastleQueenSide) != 0;
        public bool IsCastling => IsCastleKingSide || IsCastleQueenSide;

        public override string ToString()
        {
            string promotionSuffix = "";
            if (IsPromotion)
            {
                switch (PromotionPieceType)
                {
                    case PieceType.Queen: promotionSuffix = "=Q"; break;
                    case PieceType.Rook: promotionSuffix = "=R"; break;
                    case PieceType.Bishop: promotionSuffix = "=B"; break;
                    case PieceType.Knight: promotionSuffix = "=N"; break;
                }
            }
            return $"{FromSquare}{ToSquare}{promotionSuffix}";
        }

        public string ToUCINotation()
        {
            string promotionSuffix = "";
            if (IsPromotion)
            {
                switch (PromotionPieceType)
                {
                    case PieceType.Queen: promotionSuffix = "q"; break;
                    case PieceType.Rook: promotionSuffix = "r"; break;
                    case PieceType.Bishop: promotionSuffix = "b"; break;
                    case PieceType.Knight: promotionSuffix = "n"; break;
                }
            }
            return $"{FromSquare.ToString().ToLower()}{ToSquare.ToString().ToLower()}{promotionSuffix}";
        }

        public static Move FromUCINotation(string uciMove, Board board)
        {
            if (string.IsNullOrEmpty(uciMove) || uciMove.Length < 4)
                throw new System.ArgumentException("Invalid UCI move string length.");

            Square from = Square.FromString(uciMove.Substring(0, 2));
            Square to = Square.FromString(uciMove.Substring(2, 2));
            PieceType promotion = PieceType.None;
            MoveFlags flags = MoveFlags.None;

            if (uciMove.Length == 5)
            {
                flags |= MoveFlags.Promotion;
                switch (uciMove[4])
                {
                    case 'q': promotion = PieceType.Queen; break;
                    case 'r': promotion = PieceType.Rook; break;
                    case 'b': promotion = PieceType.Bishop; break;
                    case 'n': promotion = PieceType.Knight; break;
                    default: throw new System.ArgumentException("Invalid promotion piece in UCI move.");
                }
            }

            Piece movingPiece = board.GetPieceAt(from);
            Piece targetPiece = board.GetPieceAt(to);

            if (!targetPiece.IsNone()) flags |= MoveFlags.Capture;

            if (movingPiece.IsPawn())
            {
                if (Mathf.Abs(to.rank - from.rank) == 2) flags |= MoveFlags.PawnTwoForward;
                if (to == board.EnPassantTargetSquare && from.file != to.file)
                {
                    flags |= MoveFlags.EnPassantCapture;
                    flags |= MoveFlags.Capture;
                }
            }
            else if (movingPiece.IsKing())
            {
                if (Mathf.Abs(to.file - from.file) == 2)
                {
                    if (to.file > from.file) flags |= MoveFlags.CastleKingSide;
                    else flags |= MoveFlags.CastleQueenSide;
                }
            }

            return new Move(from, to, flags, promotion);
        }

        public override bool Equals(object obj)
        {
            if (obj is Move other)
            {
                return FromSquare == other.FromSquare &&
                       ToSquare == other.ToSquare &&
                       PromotionPieceType == other.PromotionPieceType &&
                       Flags == other.Flags;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return FromSquare.GetHashCode() ^
                   (ToSquare.GetHashCode() << 1) ^
                   ((int)PromotionPieceType << 2) ^
                   ((int)Flags << 3);
        }

        public static bool operator ==(Move a, Move b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Move a, Move b)
        {
            return !a.Equals(b);
        }
    }
}
