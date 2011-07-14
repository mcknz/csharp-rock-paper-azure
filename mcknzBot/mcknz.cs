using RockPaperScissorsPro;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RockPaperAzure
{
    public sealed class mcknz : IRockPaperScissorsBot
    {
        private Dictionary<Move, int> _moveStreak = new Dictionary<Move, int>();
        private bool _hasDynamite;
        private Move _opponentLastMove;
        private int _pointsPrev;
        private int _tieStreak;

        public Move MakeMove(IPlayer you, IPlayer opponent, GameRules rules)
        {
            if (you.NumberOfDecisions == 0)
            {
                you.Log.AppendLine("v7");
            }

            _hasDynamite = you.DynamiteRemaining > 2;
            _opponentLastMove = opponent.LastMove;

            Move tieStreakMove = ProcessTieStreakLogic(you, opponent);
            Move moveStreakMove = ProcessMoveStreakLogic(you);
            Move endGameMove = ProcessEndGameLogic(opponent, rules);

            return tieStreakMove ?? moveStreakMove ?? endGameMove ?? GetMyMove();
        }
            
        private Move ProcessTieStreakLogic(IPlayer you, IPlayer opponent)
        {
            int points = you.Points + opponent.Points;

            if (points > 0)
            {
                int streakCheck = Moves.GetRandomNumber(7) + 1;

                int waterBalloonThrow = Moves.GetRandomNumber(2);

                if (points == _pointsPrev)
                {
                    _tieStreak++;
                }
                else
                {
                    _tieStreak = 0;
                }
                if (_tieStreak == streakCheck && _hasDynamite)
                {
                    return Moves.Dynamite;
                }
                if (_tieStreak > streakCheck && _opponentLastMove == Moves.Dynamite) //&& waterBalloonThrow != 0)
                {
                    return Moves.WaterBalloon;
                }
                if (_tieStreak > streakCheck + 1 && _opponentLastMove == Moves.WaterBalloon)
                {
                    return null;
                }
                if (_tieStreak > streakCheck && _hasDynamite)
                {
                    return Moves.Dynamite;
                }
                //you.Log.AppendLine(string.Format("{0}. points={1};_pointsPrev={2};TieStreak={3}", you.NumberOfDecisions, points, _pointsPrev, _tieStreak));

                _pointsPrev = points;
            }

            return null;
        }

        private Move ProcessMoveStreakLogic(IPlayer you)
        {
            int streak = 0;

            if (_opponentLastMove != null)
            {
                if (_moveStreak.ContainsKey(_opponentLastMove))
                {
                    streak++;
                }

                int streakCheck = Moves.GetRandomNumber(1);

                _moveStreak.Clear();
                _moveStreak.Add(_opponentLastMove, streak);
                //you.Log.AppendLine(string.Format("{0}. OpponentLastMove={1};streak={2}", you.NumberOfDecisions, _opponentLastMove, streak));

                if (streak > streakCheck)
                {
                    return GetMirrorMove();
                }
            }

            return null;
        }

        private Move ProcessEndGameLogic(IPlayer opponent, GameRules rules)
        {
            if (opponent.Points > rules.PointsToWin - 20 && _hasDynamite)
            {
                int dynaNumber = 4;

                if (!opponent.HasDynamite)
                {
                    dynaNumber = 2;
                }

                int randomNumber = Moves.GetRandomNumber(dynaNumber);
                switch(randomNumber)
                {
                    case 0: return Moves.Dynamite;
                }
            }
            return null;
        }

        private Move GetMirrorMove()
        {
            if (_opponentLastMove == Moves.Rock)
            {
                return Moves.Paper;
            }
            if (_opponentLastMove == Moves.Paper)
            {
                return Moves.Scissors;
            }
            if (_opponentLastMove == Moves.Scissors)
            {
                return Moves.Rock;
            }
            if (_opponentLastMove == Moves.Dynamite)
            {
                //int waterBalloonThrow = Moves.GetRandomNumber(2);
                //if (waterBalloonThrow != 0)
                //{
                    return Moves.WaterBalloon;
                //}
            }
            return null;
        }

        private Move GetMyMove()
        {
            return Moves.GetRandomMove();
        }
    }
}
