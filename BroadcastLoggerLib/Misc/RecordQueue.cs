using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BroadcastLoggerLib.Misc
{
    using Pair = KeyValuePair<string, string>;
    public class RecordQueue
    { 
//--------------------------------------------------------------------------------------------
// MEMBERS 
//--------------------------------------------------------------------------------------------
#region members
        /// <summary>
        /// Instance of RecordQueue.
        /// </summary>
        private static RecordQueue instance;
        // Primary queue.
        private Queue<Pair> queue;
        // Number of records in queue.
        private int numRecords = 0;
        // Is first record.
        private bool firstRecord = false;
#endregion
//--------------------------------------------------------------------------------------------
// METHODS 
//--------------------------------------------------------------------------------------------
#region methods
        /// <summary>
        /// Number of records in queue.
        /// </summary>
        public int NumRecords {
            get {
                return numRecords;
            }
        }
//--------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        private RecordQueue()
        {
            queue = new Queue<Pair>();
        }
//--------------------------------------------------------------------------------------------
        /// <summary>
        /// Singleton call.
        /// </summary>
        public static RecordQueue Instance
        {
            get
            {
                if (instance == null)
                    instance = new RecordQueue();
                return instance;
            }
        }
//--------------------------------------------------------------------------------------------
        /// <summary>
        /// If queue has at least 1.
        /// </summary>
        /// <returns>Return false if not more than 1.</returns>
        public bool notEmpty() { return queue.Count > 0; }
//--------------------------------------------------------------------------------------------
        /// <summary>
        /// If queue has more than 1.
        /// </summary>
        /// <returns>Return false if not more than 1.</returns>
        public bool notLast() { return queue.Count > 1; }
//--------------------------------------------------------------------------------------------
        /// <summary>
        /// Push record and filename.
        /// </summary>
        /// <param name="recordId">Id of recording.</param>
        /// <param name="filename">File name of recording.</param>
        public void push(string recordId, string filename){
            Pair p = new Pair(recordId, filename);
            queue.Enqueue(p);
            numRecords = queue.Count();
        }
//--------------------------------------------------------------------------------------------
        /// <summary>
        /// Get the next set of information to store.
        /// </summary>
        /// <returns>returns id and filename of first recording, and id of second recording.</returns>
        public string[] getNextId() {
            string[] ids = new string[3];
            Array array = queue.ToArray();

            ids[0] = ((Pair)array.GetValue(0)).Key;
            ids[1] = ((Pair)array.GetValue(0)).Value;
            ids[2] = ((Pair)array.GetValue(1)).Key;

            // Necessary?
            queue.Dequeue();
            queue.TrimExcess();
            numRecords = queue.Count();
            return ids;
        }
#endregion
    }
}
