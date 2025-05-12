using UnityEngine;

namespace ChessEngine
{
    public enum PieceType
    { 
        None = 0,
        Pawn = 1,
        Knight = 2,
        Bishop = 3,
        Rook = 4,
        Queen = 5,
        King = 6
    }

    public enum PlayerColor
    {
        None = 0,
        White = 1,
        Black = 2
    }

    [System.Serializable]
    public struct Piece
    {
        public PieceType type;
        public PlayerColor color;
        public bool hasMoved;

        public static readonly Piece None = new Piece(PieceType.None, PlayerColor.None);

        public Piece(PieceType type, PlayerColor color, bool hasMoved = false)
        {
            this.type = type;
            this.color = color;
            this.hasMoved = hasMoved;
        }

        public bool IsNone()
        {
            return type == PieceType.None;
        }

        public bool IsPawn() { return type == PieceType.Pawn; }
        public bool IsKnight() { return type == PieceType.Knight; }
        public bool IsBishop() { return type == PieceType.Bishop; }
        public bool IsRook() { return type == PieceType.Rook; }
        public bool IsQueen() { return type == PieceType.Queen; }
        public bool IsKing() { return type == PieceType.King; }

        public static PlayerColor GetOppositeColor(PlayerColor playerColor)
        {
            if (playerColor == PlayerColor.White) return PlayerColor.Black;
            if (playerColor == PlayerColor.Black) return PlayerColor.White;
            return PlayerColor.None;
        }

        public int GetBaseValue()
        {
            switch (type)
            {
                case PieceType.Pawn: return 100;
                case PieceType.Knight: return 320;
                case PieceType.Bishop: return 330;
                case PieceType.Rook: return 500;
                case PieceType.Queen: return 900;
                case PieceType.King: return 20000;
                default: return 0;
            }
        }

        public override string ToString()
        {
            return $"{color} {type}";
        }
    }

    [System.Serializable]
    public struct Square
    {
        public int rank;
        public int file;

        public Square(int rank, int file)
        {
            this.rank = rank;
            this.file = file;
        }

        public bool IsValid()
        {
            return rank >= 0 && rank < 8 && file >= 0 && file < 8;
        }

        public static bool operator ==(Square a, Square b)
        {
            return a.rank == b.rank && a.file == b.file;
        }

        public static bool operator !=(Square a, Square b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is Square other && this == other;
        }

        public override int GetHashCode()
        {
            return rank * 8 + file;
        }

        public override string ToString()
        {
            char fileChar = (char)('a' + file);
            return $"{fileChar}{rank + 1}";
        }

        public static Square FromString(string algebraicNotation)
        {
            if (string.IsNullOrEmpty(algebraicNotation) || algebraicNotation.Length != 2)
            {
                throw new System.ArgumentException("Invalid algebraic notation for square.");
            }
            char fileChar = algebraicNotation[0];
            char rankChar = algebraicNotation[1];

            int file = fileChar - 'a';
            int rank = rankChar - '1';

            if (file < 0 || file > 7 || rank < 0 || rank > 7)
            {
                 throw new System.ArgumentException($"Invalid square coordinates from notation: {algebraicNotation}");
            }
            return new Square(rank, file);
        }
    }
}
