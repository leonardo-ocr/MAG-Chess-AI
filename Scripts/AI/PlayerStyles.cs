//KARPOV, A. Positional Play in Chess. London: Everyman Chess, 2000.
//TAL, M. The Life and Games of Mikhail Tal. New York: Hart Publishing, 1991
//KASPAROV, G. My Great Predecessors. v. 1–5. London: Everyman Chess, 2003.

using UnityEngine;
using System.Collections.Generic;

namespace ChessEngine
{
    public enum AIPlayerStyle
    {
        Default,
        Karpov,
        Tal,
        Kasparov
    }

    [System.Serializable]
    public class PlayerStyleProfile
    {
        public AIPlayerStyle styleName = AIPlayerStyle.Default;

        public float materialWeight = 1.0f;
        public float pawnValue = 100f;
        public float knightValue = 320f;
        public float bishopValue = 330f;
        public float rookValue = 500f;
        public float queenValue = 900f;

        public float pstWeight = 0.1f;

        public float mobilityWeight = 0.1f;

        public float kingSafetyWeight = 2.0f;
        public float kingPawnShieldBonus = 5f;
        public float kingOpenFilePenalty = -10f;
        public float kingAttackedByPiecePenalty = -15f;

        public float pawnStructureWeight = 0.5f;
        public float passedPawnBonus = 20f;
        public float doubledPawnPenalty = -10f;
        public float isolatedPawnPenalty = -10f;

        public float centerControlWeight = 0.2f;
        public float centerPawnBonus = 10f;
        public float centerMinorPieceBonus = 5f;

        public float attackWeight = 0.3f;
        public float hangingPiecePenalty = -50f;
        public float attackingKingBonus = 25f;

        public float initiativeWeight = 0.2f;
        public float developedPieceBonus = 5f;
        public float castlingBonus = 25f;

        public float aggressionFactor = 1.0f;
        public float positionalFactor = 1.0f;
        public float riskTolerance = 0.0f;

        public PlayerStyleProfile(AIPlayerStyle style = AIPlayerStyle.Default)
        {
            this.styleName = style;
            SetStyleParameters(style);
        }

        public void SetStyleParameters(AIPlayerStyle style)
        {
            this.styleName = style;
            materialWeight = 1.0f; pawnValue = 100f; knightValue = 320f; bishopValue = 330f; rookValue = 500f; queenValue = 900f;
            pstWeight = 0.1f;
            mobilityWeight = 0.1f;
            kingSafetyWeight = 2.0f; kingPawnShieldBonus = 5f; kingOpenFilePenalty = -10f; kingAttackedByPiecePenalty = -15f;
            pawnStructureWeight = 0.5f; passedPawnBonus = 20f; doubledPawnPenalty = -10f; isolatedPawnPenalty = -10f;
            centerControlWeight = 0.2f; centerPawnBonus = 10f; centerMinorPieceBonus = 5f;
            attackWeight = 0.3f; hangingPiecePenalty = -50f; attackingKingBonus = 25f;
            initiativeWeight = 0.2f; developedPieceBonus = 5f; castlingBonus = 25f;
            aggressionFactor = 1.0f; positionalFactor = 1.0f; riskTolerance = 0.0f;

            switch (style)
            {
                case AIPlayerStyle.Karpov:
                    positionalFactor = 1.5f;
                    kingSafetyWeight = 2.5f;
                    pawnStructureWeight = 0.7f;
                    isolatedPawnPenalty = -15f;
                    doubledPawnPenalty = -12f;
                    mobilityWeight = 0.05f;
                    attackWeight = 0.15f;
                    aggressionFactor = 0.7f;
                    riskTolerance = -0.2f;
                    break;

                case AIPlayerStyle.Tal:
                    aggressionFactor = 1.8f;
                    attackWeight = 0.6f;
                    attackingKingBonus = 50f;
                    initiativeWeight = 0.5f;
                    mobilityWeight = 0.2f;
                    materialWeight = 0.8f;
                    riskTolerance = 0.5f;
                    kingSafetyWeight = 1.0f;
                    positionalFactor = 0.5f;
                    break;

                case AIPlayerStyle.Kasparov:
                    initiativeWeight = 0.4f;
                    centerControlWeight = 0.4f;
                    mobilityWeight = 0.15f;
                    attackWeight = 0.4f;
                    aggressionFactor = 1.3f;
                    positionalFactor = 1.1f;
                    kingSafetyWeight = 1.8f;
                    pawnStructureWeight = 0.6f;
                    break;

                case AIPlayerStyle.Default:
                    break;
            }
        }

        public static PlayerStyleProfile GetProfile(AIPlayerStyle style)
        {
            return new PlayerStyleProfile(style);
        }
    }
}
