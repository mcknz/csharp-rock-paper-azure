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
        private bool _tieBreakAttempted; 
        private int _endGamePoints;
        private Move _lastEndGameThrow;
        private int _dynamiteUsed;
        private int _waterBalloonUsed;

        private Dictionary<int, int> _tieStreakOdds = new Dictionary<int, int>();
        private Dictionary<int,int> _tieStreakOddsOrig = new Dictionary<int,int>
        {
            {10,1},
            {9,1},
            {8,1},
            {7,1},
            {6,1},
            {5,1},
            {4,2},
            {3,3},
            {2,6},
            {1,8}
        };

        private int _endGameOffset = 50;
        private int _dynamiteRemainingOffset = Moves.GetRandomNumber(3);

        public Move MakeMove(IPlayer you, IPlayer opponent, GameRules rules)
        {
            if (you.NumberOfDecisions == 0)
            {
                int pointsToWin = rules.PointsToWin;
                you.Log.AppendLine("v10");
                _endGamePoints = pointsToWin - _endGameOffset;
                double calc;
                foreach (var odd in _tieStreakOddsOrig)
                {
                    calc = odd.Value > 2 ? (odd.Value * .001 * pointsToWin) / (0.01 * rules.StartingDynamite) : odd.Value;
                    _tieStreakOdds.Add(odd.Key, (int)Math.Round(calc, MidpointRounding.AwayFromZero));
                    you.Log.AppendLine(string.Format("{0}:{1}",odd.Key, _tieStreakOdds[odd.Key]));
                }
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
                    int odds;

                    _tieStreak++;

                    you.Log.AppendLine(string.Format("points: {0}", points));
                    you.Log.AppendLine(string.Format("pointsPrev: {0}", points));
                    you.Log.AppendLine(string.Format("_tieBreakAttempted: {0}", _tieBreakAttempted));
                    you.Log.AppendLine(string.Format("_tieStreak: {0}", _tieStreak));

                    if(_tieBreakAttempted)
                    {
                        int moveNum = Moves.GetRandomNumber(3);
                        you.Log.AppendLine(string.Format("moveNum: {0}", moveNum));
                        switch (moveNum)
                        {
                            case 0: return Moves.WaterBalloon;
                            case 1: return _hasDynamite ? ThrowDynamite() : null;
                            case 2: return null;
                        }
                    }
                    else
                    {
                        if(_tieStreak > _tieStreakOdds.Count)
                        {
                            odds = 0;
                        }
                        else
                        {
                            odds = Moves.GetRandomNumber(_tieStreakOdds[_tieStreak]);
                        }

                        you.Log.AppendLine(string.Format("odds: {0}", odds));
                    

                        if(odds == 0 && _hasDynamite)
                        {
                            _tieBreakAttempted = true;
                            return ThrowDynamite();
                        }
                    }
                }
                else
                {
                    _tieStreak = 0;
                    _tieBreakAttempted = false;
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

                int streakCheck = Moves.GetRandomNumber(2);

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
                    return ThrowDynamite();
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
                return ThrowWaterBalloon();
            }
            return null;
        }

        private Move GetMyMove()
        {
            return Moves.GetRandomMove();
        }

        private Move ThrowDynamite()
        {
            _dynamiteUsed++;
            return Moves.Dynamite;
        }

        private Move ThrowWaterBalloon()
        {
            _waterBalloonUsed++;
            return Moves.WaterBalloon;
        }

    }
}
