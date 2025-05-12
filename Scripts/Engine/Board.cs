using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Board
{
    public Piece[,] pieces = new Piece[8, 8];
    public List<string> moveHistory = new List<string>();

    public bool whiteToMove = true;
    public bool whiteKingMoved = false;
    public bool blackKingMoved = false;
    public bool whiteKingsideRookMoved = false;
    public bool whiteQueensideRookMoved = false;
    public bool blackKingsideRookMoved = false;
    public bool blackQueensideRookMoved = false;
    public Vector2Int enPassantSquare = new Vector2Int(-1, -1);
    public int halfmoveClock = 0;
    public int fullmoveNumber = 1;

    public void LoadFEN(string fen)
    {
        string[] parts = fen.Split(' ');
        string[] ranks = parts[0].Split('/');
        for (int y = 0; y < 8; y++)
        {
            int x = 0;
            foreach (char c in ranks[y])
            {
                if (char.IsDigit(c))
                {
                    x += int.Parse(c.ToString());
                }
                else
                {
                    pieces[x, 7 - y] = new Piece(c);
                    x++;
                }
            }
        }

        whiteToMove = parts[1] == "w";
        SetCastlingRights(parts[2]);
        enPassantSquare = parts[3] != "-" ? ParseCoordinates(parts[3]) : new Vector2Int(-1, -1);
        halfmoveClock = int.Parse(parts[4]);
        fullmoveNumber = int.Parse(parts[5]);
    }

    void SetCastlingRights(string rights)
    {
        whiteKingsideRookMoved = !rights.Contains("K");
        whiteQueensideRookMoved = !rights.Contains("Q");
        blackKingsideRookMoved = !rights.Contains("k");
        blackQueensideRookMoved = !rights.Contains("q");
        whiteKingMoved = whiteKingsideRookMoved && whiteQueensideRookMoved;
        blackKingMoved = blackKingsideRookMoved && blackQueensideRookMoved;
    }

    public string GetFEN()
    {
        string fen = "";
        for (int y = 7; y >= 0; y--)
        {
            int empty = 0;
            for (int x = 0; x < 8; x++)
            {
                if (pieces[x, y] == null)
                {
                    empty++;
                }
                else
                {
                    if (empty > 0)
                    {
                        fen += empty;
                        empty = 0;
                    }
                    fen += pieces[x, y].ToString();
                }
            }
            if (empty > 0) fen += empty;
            if (y > 0) fen += "/";
        }

        fen += whiteToMove ? " w " : " b ";
        fen += (whiteKingsideRookMoved ? "" : "K") +
               (whiteQueensideRookMoved ? "" : "Q") +
               (blackKingsideRookMoved ? "" : "k") +
               (blackQueensideRookMoved ? "" : "q");
        if (fen.EndsWith(" ")) fen += "-";
        fen += " " + (enPassantSquare.x != -1 ? CoordinateToString(enPassantSquare) : "-");
        fen += " " + halfmoveClock + " " + fullmoveNumber;

        return fen;
    }

    public void MovePiece(Vector2Int from, Vector2Int to, string promotion = "")
    {
        Piece movingPiece = pieces[from.x, from.y];
        Piece targetPiece = pieces[to.x, to.y];
        string currentFen = GetFEN();
        moveHistory.Add(currentFen);

        bool pawnMove = movingPiece.type == 'P' || movingPiece.type == 'p';
        bool capture = targetPiece != null;

        if (movingPiece.IsWhite)
        {
            if (movingPiece.type == 'K') whiteKingMoved = true;
            if (from.x == 0 && from.y == 0) whiteQueensideRookMoved = true;
            if (from.x == 7 && from.y == 0) whiteKingsideRookMoved = true;
        }
        else
        {
            if (movingPiece.type == 'k') blackKingMoved = true;
            if (from.x == 0 && from.y == 7) blackQueensideRookMoved = true;
            if (from.x == 7 && from.y == 7) blackKingsideRookMoved = true;
        }

        if (movingPiece.IsWhite)
        {
            if (to.x == 0 && to.y == 7) blackQueensideRookMoved = true;
            if (to.x == 7 && to.y == 7) blackKingsideRookMoved = true;
        }
        else
        {
            if (to.x == 0 && to.y == 0) whiteQueensideRookMoved = true;
            if (to.x == 7 && to.y == 0) whiteKingsideRookMoved = true;
        }

        if (movingPiece.type == 'P' && from.y == 1 && to.y == 3) enPassantSquare = new Vector2Int(from.x, 2);
        else if (movingPiece.type == 'p' && from.y == 6 && to.y == 4) enPassantSquare = new Vector2Int(from.x, 5);
        else enPassantSquare = new Vector2Int(-1, -1);

        if (movingPiece.type == 'P' && to == enPassantSquare)
            pieces[to.x, to.y - 1] = null;
        if (movingPiece.type == 'p' && to == enPassantSquare)
            pieces[to.x, to.y + 1] = null;

        pieces[to.x, to.y] = promotion != "" ? new Piece(promotion[0]) : movingPiece;
        pieces[from.x, from.y] = null;

        if (movingPiece.type == 'K' && from.x == 4 && to.x == 6) // White kingside
        {
            pieces[5, 0] = pieces[7, 0];
            pieces[7, 0] = null;
        }
        else if (movingPiece.type == 'K' && from.x == 4 && to.x == 2) // White queenside
        {
            pieces[3, 0] = pieces[0, 0];
            pieces[0, 0] = null;
        }
        else if (movingPiece.type == 'k' && from.x == 4 && to.x == 6) // Black kingside
        {
            pieces[5, 7] = pieces[7, 7];
            pieces[7, 7] = null;
        }
        else if (movingPiece.type == 'k' && from.x == 4 && to.x == 2) // Black queenside
        {
            pieces[3, 7] = pieces[0, 7];
            pieces[0, 7] = null;
        }

        if (pawnMove || capture)
            halfmoveClock = 0;
        else
            halfmoveClock++;

        if (!whiteToMove) fullmoveNumber++;
        whiteToMove = !whiteToMove;
    }

    public void UndoMove()
    {
        if (moveHistory.Count == 0) return;
        string lastFen = moveHistory[moveHistory.Count - 1];
        LoadFEN(lastFen);
        moveHistory.RemoveAt(moveHistory.Count - 1);
    }

    Vector2Int ParseCoordinates(string coord)
    {
        return new Vector2Int(coord[0] - 'a', coord[1] - '1');
    }

    string CoordinateToString(Vector2Int coord)
    {
        return ((char)('a' + coord.x)).ToString() + (coord.y + 1);
    }
}
