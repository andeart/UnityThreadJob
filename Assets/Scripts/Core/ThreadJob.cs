using System;
using System.Threading;



namespace Andeart.ThreadJobs.Core
{

    public abstract class ThreadJob<T> where T : EventArgs
    {

        private Thread _thread;

        public EventHandler<T> Finished { get; set; }

        protected ThreadJob ()
        {
            Finished = delegate { };
        }

        public void ExecuteAsIntentionalThread ()
        {
            _thread = new Thread (ExecuteJob);
            _thread.Start ();
        }

        public void ExecuteThreadQueueCallback (object stateInfo)
        {
            ExecuteJob ();
        }

        private void ExecuteJob ()
        {
            Execute ();
            Finished.Invoke (this, GetResultArgs ());
        }

        protected abstract void Execute ();

        protected abstract T GetResultArgs ();

    }

}