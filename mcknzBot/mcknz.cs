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
        private int _tieStreakCheckRandomMax;
        private int? _tieStreakCheck;
        private int _endGamePoints;
        private Move _lastEndGameThrow;

        // represents "ideal" upper bound random number for throwing Dynamite on ties, per 1000 points
        private double _tieStreakRandomMaxCoefficient  = 0.004;
        private int _tieStreakMin = Moves.GetRandomNumber(1) + 1;
        private int _endGameOffset = 50;
        private int _dynamiteRemainingOffset = Moves.GetRandomNumber(3);

        public Move MakeMove(IPlayer you, IPlayer opponent, GameRules rules)
        {
            if (you.NumberOfDecisions == 0)
            {
                you.Log.AppendLine("v9");
                _tieStreakCheckRandomMax = (int)Math.Round(rules.PointsToWin * _tieStreakRandomMaxCoefficient);
                _endGamePoints = rules.PointsToWin - _endGameOffset;
            }

            _hasDynamite = you.DynamiteRemaining > _dynamiteRemainingOffset;
            _opponentLastMove = opponent.LastMove;

            Move tieStreakMove = ProcessTieStreakLogic(you, opponent);
            Move moveStreakMove = ProcessMoveStreakLogic(you);
            Move endGameMove = ProcessEndGameLogic(opponent, rules);

            return endGameMove ?? tieStreakMove ?? moveStreakMove ?? GetMyMove();
        }
            
        private Move ProcessTieStreakLogic(IPlayer you, IPlayer opponent)
        {
            int points = you.Points + opponent.Points;

            if (points > 0)
            {
                if (points == _pointsPrev)
                {
                    _tieStreak++;
                    if (!_tieStreakCheck.HasValue)
                    {
                        _tieStreakCheck = Moves.GetRandomNumber(_tieStreakCheckRandomMax) + _tieStreakMin;
                        //you.Log.AppendLine(string.Format("_tieStreakCheck: {0}", _tieStreakCheck));
                    }
                }
                else
                {
                    _tieStreak = 0;
                    _tieStreakCheck = null;
                }
                if (_tieStreak == _tieStreakCheck && _hasDynamite)
                {
                    return Moves.Dynamite;
                }
                if (_tieStreak > _tieStreakCheck && _opponentLastMove == Moves.Dynamite || _opponentLastMove == Moves.WaterBalloon)
                {
                    switch (Moves.GetRandomNumber(2))
                    {
                        case 0: return Moves.WaterBalloon;
                        case 1: return _hasDynamite ? Moves.Dynamite: null;
                        case 2: return null;
                    }
                }
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
                
                if (streak > streakCheck)
                {
                    return GetMirrorMove();
                }
            }

            return null;
        }

        private Move ProcessEndGameLogic(IPlayer opponent, GameRules rules)
        {
            if (opponent.Points > _endGamePoints && _hasDynamite)
            {
                if(Moves.GetRandomNumber(2) == 0 && _lastEndGameThrow != Moves.Dynamite)
                {
                    _lastEndGameThrow = Moves.Dynamite;
                    return Moves.Dynamite;
                }
                else
                {
                    _lastEndGameThrow = null;
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
                return Moves.WaterBalloon;
            }
            return null;
        }

        private Move GetMyMove()
        {
            return Moves.GetRandomMove();
        }

        private bool IsRandom()
        {
            return Moves.GetRandomNumber(1) == 0;
        }
    }
}
