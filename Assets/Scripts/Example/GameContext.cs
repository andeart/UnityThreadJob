using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;



namespace Andeart.ThreadJobs.Examples
{

    /// <summary>
    /// Multi-threading example within Unity, using .NET 3.5.
    /// </summary>
    public class GameContext : MonoBehaviour
    {

        private const string _executionAlertFormat = "Adding ints via {0}";
        private const string _sumMessageFormat = "Result: <b>{1}</b> (via {0}).\nTime taken: <b>{2}</b>";

        private int[][] _myInts;

        [SerializeField] private int _arraysCount;
        [SerializeField] private int _arraySize;
        [SerializeField] private Vector2Int _arrayValueRange;

        private void Start ()
        {
            Debug.Log ("Running example...");

            GenerateNumbers ();

            AddNumbersOnMainThread();

            AddNumbersViaThreadJobs ();

            AddNumbersViaThreadPoolQueue ();
        }

        /// <summary>
        /// Generates a jagged array of ints.
        /// Currently unoptimised. Optimisation is irrelevant to the focus of this example.
        /// </summary>
        private void GenerateNumbers ()
        {
            Random randomSeed = new Random ();

            _myInts = new int[_arraysCount][];

            for (int i = 0; i < _myInts.Length; i++)
            {
                _myInts[i] = new int[_arraySize];
                for (int j = 0; j < _myInts[i].Length; j++)
                {
                    _myInts[i][j] = randomSeed.Next (_arrayValueRange.x, _arrayValueRange.y);
                }
            }
        }

        private void AddNumbersOnMainThread ()
        {
            const string alertSubject = "main thread";

            //Debug.LogFormat (_executionAlertFormat, alertSubject);

            Stopwatch stopwatch = new Stopwatch ();
            stopwatch.Reset ();
            stopwatch.Start ();

            int sum = 0;
            for (int i = 0; i < _myInts.Length; i++)
            {
                for (int j = 0; j < _myInts[i].Length; j++)
                {
                    sum += _myInts[i][j];
                }
            }

            stopwatch.Stop ();

            Debug.LogFormat (_sumMessageFormat, alertSubject, sum, stopwatch.Elapsed.TotalSeconds);
        }

        private void AddNumbersViaThreadPoolQueue ()
        {
            const string alertSubject = "automatic thread-pool queueing";

            //Debug.LogFormat (_executionAlertFormat, alertSubject);

            Stopwatch stopwatch = new Stopwatch ();
            stopwatch.Reset ();
            stopwatch.Start ();

            int sum = 0;
            int jobsRemaining = _myInts.Length + 1;
            ArraySumJob[] arraySumJobs = new ArraySumJob[_myInts.Length];
            ManualResetEvent allJobsFinished = new ManualResetEvent (false);
            for (int i = 0; i < _myInts.Length; i++)
            {
                arraySumJobs[i] = new ArraySumJob (_myInts[i]);
                arraySumJobs[i].Finished += delegate (object sender, ArraySumResultArgs args)
                                            {
                                                Interlocked.Add (ref sum, args.Sum);
                                                if (Interlocked.Decrement (ref jobsRemaining) == 0)
                                                {
                                                    allJobsFinished.Set ();
                                                }
                                            };

                ThreadPool.QueueUserWorkItem (arraySumJobs[i].ExecuteThreadQueueCallback);
            }

            if (Interlocked.Decrement (ref jobsRemaining) == 0)
            {
                allJobsFinished.Set ();
            }

            allJobsFinished.WaitOne ();


            stopwatch.Stop ();

            Debug.LogFormat (_sumMessageFormat, alertSubject, sum, stopwatch.Elapsed.TotalSeconds);
        }

        private void AddNumbersViaThreadJobs ()
        {
            const string alertSubject = "manual thread creation";

            //Debug.LogFormat (_executionAlertFormat, alertSubject);

            Stopwatch stopwatch = new Stopwatch ();
            stopwatch.Reset ();
            stopwatch.Start ();

            int sum = 0;
            int jobsRemaining = _myInts.Length + 1;
            ArraySumJob[] arraySumJobs = new ArraySumJob[_myInts.Length];
            ManualResetEvent allJobsFinished = new ManualResetEvent (false);
            for (int i = 0; i < _myInts.Length; i++)
            {
                arraySumJobs[i] = new ArraySumJob (_myInts[i]);
                arraySumJobs[i].Finished += delegate (object sender, ArraySumResultArgs args)
                                            {
                                                Interlocked.Add (ref sum, args.Sum);
                                                if (Interlocked.Decrement (ref jobsRemaining) == 0)
                                                {
                                                    allJobsFinished.Set ();
                                                }
                                            };

                arraySumJobs[i].ExecuteAsIntentionalThread ();
            }

            if (Interlocked.Decrement (ref jobsRemaining) == 0)
            {
                allJobsFinished.Set ();
            }

            allJobsFinished.WaitOne ();
            stopwatch.Stop ();

            Debug.LogFormat (_sumMessageFormat, alertSubject, sum, stopwatch.Elapsed.TotalSeconds);
        }

    }

}