#pragma warning disable 28 

using System.Collections.Generic;
using System.Threading;

namespace Compete.Site.Refereeing
{
    public interface IRefereeThread
    {
        bool IsRunning { get; }
        bool Start(RoundParameters parameters);
    }

    public class RefereeThread : IRefereeThread
    {
        readonly Queue<RoundParameters> _queue = new Queue<RoundParameters>();
        Thread _thread;
        Referee _currentlyRunning;

        public bool IsRunning
        {
            get { return _currentlyRunning != null; }
        }

        public bool Start(RoundParameters parameters)
        {
            if (_currentlyRunning != null)
            {
                _queue.Enqueue(parameters);
                return false;
            }
            _thread = new Thread(Main);
            _currentlyRunning = new Referee(parameters);
            _thread.Start(this);
            return true;
        }

        protected virtual void Run()
        {
            try
            {
                _currentlyRunning.StartRound();
            }
            finally
            {
                _currentlyRunning = null;
            }
        }

        private static void Main(object parameter)
        {
            ((RefereeThread)parameter).Run();
        }
    }
}