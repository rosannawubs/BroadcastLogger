using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BroadcastLoggerLib
{
    public enum Status { NEW, AUTHORIZED, ID_CREATED, REC_DONE, REC_SET_DONE, UPLOADED, SET_UPLOADED }
    /// <summary>
    /// This is a singleton class that handles persistent storage for Broadcast logger
    /// The data is stored inside of a SQLite database.
    /// </summary>
    public class Storage
    {
        /// <summary>
        /// Connection to the database
        /// </summary>
        private SQLiteConnection connection;
        /// <summary>
        /// Command object that will be executing our commands.
        /// </summary>
        /// <summary>
        /// Reader to read the output from executed commands. 
        /// </summary>
        /// <summary>
        /// Returns an instance of storage, as it is a singleton
        /// there will always only be one. 
        /// </summary>
        /// <summary>
        /// Number of elmeents in the queue
        /// </summary>
        public int Length {
            get
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                    "SELECT COUNT(*) FROM UploadQueue";
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        int output = int.Parse(reader.GetValue(0).ToString());
                        return output;
                    }
                }
            }
        }
        /// <summary>
        /// Returns the queue object of a given uploadID
        /// </summary>
        /// <param name="i">The id of the object you want. </param>
        /// <returns>StorageResult that holds all the information from the requested index</returns>
        public StorageResult this[int i]
        {
            get { return GetFromIndex(i); }
        }
        /// <summary>
        /// Private constructor to constructor a storage object when there isnt one.
        /// As this is a singleton class we don't want the public to generate the class.
        /// </summary>
        public Storage()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dataSource = Path.Combine(folder, @"BroadcastLogger/Storage.db");
            connection = new SQLiteConnection();
            connection.ConnectionString = "Data Source=" + dataSource + ";foreign keys=true;";
            connection.Open();
            InitalizeTables();
        }
        /// <summary>
        /// Kill the connection when this class is destoryed. 
        /// </summary>
        ~Storage()
        {
            connection.Dispose();
        }
        /// <summary>
        /// Recreated the tables if there are non. 
        /// </summary>
        public void InitalizeTables()
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                //Create the stations table
                command.CommandText =
                "CREATE TABLE IF NOT EXISTS Stations (" +
                    "StationID 	    VARCHAR(100) NOT NULL PRIMARY KEY," +
                    "StationName    VARCHAR(100) NULL," +
                    "AuthCode	    VARCHAR(100) NOT NULL)";
                command.ExecuteNonQuery();
                //Create the upload queue
                command.CommandText =
                "CREATE TABLE IF NOT EXISTS UploadQueue (" +
                    "UploadID  		INTEGER 	    PRIMARY KEY," +
                    "StationID 		VARCHAR(100) 	NOT NULL," +
                    "LocalFileName 	VARCHAR(250) 	NULL," +
                    "AWSFileName    VARCHAR(250)	NULL," +
                    "RecordID		VARCHAR(25)		NULL," +
                    "CreationTime   TIMESTAMP       DEFAULT CURRENT_TIMESTAMP," +
                    "Status         INT             NULL    DEFAULT 0," +
                    "FOREIGN KEY (StationID) REFERENCES Stations(StationID))";
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Add a station to the database, you must add the station first before
        /// you can add a object to the queue
        /// </summary>
        /// <param name="stationID">The ID of the station</param>
        /// <param name="authCode">The authorization code for the station</param>
        public void AddStation(string stationID, string authCode)
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =
                "INSERT OR REPLACE INTO Stations" +
                    "(StationID, 		    AuthCode)" +
                "VALUES" +
                    "(\"" + stationID + "\", 	\"" + authCode + "\") ";
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Change the station name for a given stationID
        /// </summary>
        /// <param name="stationID">The station ID for the station we want to edit</param>
        /// <param name="stationName">The name we want to change it to.</param>
        public void SetStationName(string stationID, string stationName)
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =
                "UPDATE Stations " +
                "SET " +
                    "StationName = '" + stationName + "'" +
                "WHERE " +
                "StationID = '" + stationID + "'";
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Add an object to the queue, the status of the object is set to -1
        /// </summary>
        /// <param name="stationID">The id of the station this object is related to.</param>
        /// <param name="localFileName">The location where the audio log exists locally</param>
        /// <param name="recordID">The ID of the recording. It defaults to an empty string if nothing
        /// is entered</param>
        /// <returns>The id of the entered queue object.</returns>
        public int AddToQueue(string stationID, string localFileName, string recordID = "")
        {
            using(SQLiteCommand command = new SQLiteCommand(connection)) {
                command.CommandText =
                "INSERT INTO UploadQueue " +
                    "(StationID,			LocalFileName,		    RecordID) " +
                "VALUES " +
                    "('" + stationID + "',  '" + localFileName + "','" + recordID + "')";
                command.ExecuteNonQuery();
                command.CommandText = "SELECT last_insert_rowid() UploadQueue";
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    int output = int.Parse(reader.GetValue(0).ToString());
                    return output;//Return row 0 because last_insert_row only returns one thing
                }
            }
        }
        /// <summary>
        /// Change the local file path of a given upload queue object
        /// </summary>
        /// <param name="uploadID">The upload ID for the object we want to modify</param>
        /// <param name="localFileName">The file name to change it to</param>
        /// <param name="status">The status we want to change the object to</param>
        public void UpdateLocalFileName(int uploadID, string localFileName, Status status)
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =
                "UPDATE UploadQueue " +
                "SET " +
                    "LocalFileName 	= '" + localFileName + "'," +
                    "Status			= " + (int)status +
                " WHERE " +
                "UploadID		= " + uploadID;
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Updates the local file path of a given upload id
        /// </summary>
        /// <param name="uploadID">The id of the object we want to modify.</param>
        /// <param name="localFileName">The file name to change to</param>
        public void UpdateLocalFileName(int uploadID, string localFileName)
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =
                "UPDATE UploadQueue " +
                "SET " +
                    "LocalFileName 	= \"" + localFileName + "\" " +
                "WHERE" +
                " UploadID		= " + uploadID;
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// The file name that needs to be used by amazon
        /// </summary>
        /// <param name="uploadID">The id of the object in the queue to be modified</param>
        /// <param name="AWSFileName">The file name to change to</param>
        /// <param name="status">use this to change the status of the object</param>
        public void UpdateAWSFileName(int uploadID, string AWSFileName, Status status)
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =
                            "UPDATE UploadQueue " +
                            "SET " +
                                "AWSFileName 	= \"" + AWSFileName + "\"," +
                                "Status         = \"" + (int)status + "\"" +
                            " WHERE" +
                            " UploadID		= " + uploadID;
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Change the satus of a given object using its id
        /// </summary>
        /// <param name="uploadID">The ID of the object that we want to change</param>
        /// <param name="status">The status to change it to</param>
        public void UpdateStatus(int uploadID, Status status)
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =
                    "Update UploadQueue " +
                    "SET " +
                        "Status = " + (int)status + " " +
                    "WHERE " +
                    "UploadID = " + uploadID;
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Change the file path that amazon wants
        /// </summary>
        /// <param name="uploadID">The id of the object to change</param>
        /// <param name="AWSFileName">The path to be changed to</param>
        public void UpdateAWSFileName(int uploadID, string AWSFileName)
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =
                            "UPDATE UploadQueue " +
                            "SET " +
                                "AWSFileName 	= '" + AWSFileName + "' " +
                            "WHERE " +
                            "UploadID		= " + uploadID;
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Update the recording id given a object ID
        /// </summary>
        /// <param name="uploadID">The id of the object we want to change</param>
        /// <param name="recordID">The ID we want to change it to.</param>
        public void UpdateRecordID(int uploadID, string recordID)
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =
                   "Update UploadQueue " +
                   "Set " +
                       "RecordId = '" + recordID + "' " +
                   "Where " +
                   "UploadID = '" + uploadID + "'";
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Returns a set of results in a list in a first in first out order.
        /// </summary>
        /// <param name="numberToGet">The number of results to get</param>
        /// <returns></returns>
        public List<StorageResult> GetSeveral(int numberToGet)
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =
                "SELECT " +
                    "datetime(CreationTime, 'localtime'), " +
                    "StationID," +
                    "StationName," +
                    "AuthCode," +
                    "LocalFileName," +
                    "AWSFileName," +
                    "RecordID," +
                    "Status," +
                    "UploadID " +
                "FROM Stations " +
                    "NATURAL JOIN UploadQueue " +
                "ORDER BY " +
                    "CreationTime ASC " +
                "LIMIT " + numberToGet;
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    List<StorageResult> output = new List<StorageResult>();
                    while (reader.Read())
                    {
                        output.Add(
                            new StorageResult(reader["StationID"].ToString()
                                , reader["StationName"].ToString()
                                , reader["AuthCode"].ToString()
                                , reader["LocalFileName"].ToString()
                                , reader["AWSFileName"].ToString()
                                , reader["RecordID"].ToString()
                                , reader.GetDateTime(0)
                                , int.Parse(reader["Status"].ToString())
                                , reader["UploadID"].ToString()));
                        Console.WriteLine("Inside of storage upload ID: " + reader["UploadID"]);
                    }
                    return output;
                }
            }
            
        }
        /// <summary>
        /// Removes everything in the queue
        /// </summary>
        public void ClearQueue()
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText = "DELETE FROM UploadQueue";
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Remove all remebered stations
        /// </summary>
        public void ClearStations()
        {
            using (SQLiteCommand command = new SQLiteCommand(connection)) 
            {
                command.CommandText = "DELETE FROM Stations";
                command.ExecuteNonQuery();
            }
            
        }
        /// <summary>
        /// Remove the stations table, call initalizeTables afterwards or the class will be unusable.
        /// </summary>
        public void DropStations()
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText = "DROP TABLE IF EXISTS Stations";
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Remove the upload queue table, call initalizeTables afterwards or the class will be unusable.
        /// </summary>
        public void DropUploadQueue()
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText = "DROP TABLE IF EXISTS UploadQueue";
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Remove the first thing in the list
        /// </summary>
        public void Pop()
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =
                "DELETE FROM UploadQueue " +
                "WHERE UploadID IN " +
                "(SELECT UploadID " +
                "FROM UploadQueue " +
                "ORDER BY " +
                    "CreationTime ASC " +
                "LIMIT 1)";
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Get an object with the given index.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public StorageResult GetFromIndex(int i)
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =
                "SELECT " +
                    "datetime(CreationTime, 'localtime'), " +
                    "StationID," +
                    "StationName," +
                    "AuthCode," +
                    "LocalFileName," +
                    "AWSFileName," +
                    "RecordID," +
                    "Status," +           
                    "UploadID " +
                "FROM Stations " +
                    "NATURAL JOIN UploadQueue " +
                "WHERE UploadID = '" + i + "'";

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    StorageResult output = null;
                    try
                    {
                        output = new StorageResult(reader["StationID"].ToString()
                               , reader["StationName"].ToString()
                               , reader["AuthCode"].ToString()
                               , reader["LocalFileName"].ToString()
                               , reader["AWSFileName"].ToString()
                               , reader["RecordID"].ToString()
                               , reader.GetDateTime(0)
                               , int.Parse(reader["Status"].ToString())
                               , reader["UploadID"].ToString());
                        reader.Dispose();
                    }
                    catch (InvalidOperationException ex)
                    {
                        throw (new InvalidOperationException("Could not get the row that was requested. Possibly out of range"));
                    }

                    return output;
                }
            }
        }
    }
}
/// <summary>
/// Data object used by the storage class to give users the required data. 
/// </summary>
public class StorageResult
{
    public string StationID { get; set; }
    public string StationName { get; set; }
    public string AuthCode { get; set; }
    public string LocalFileName { get; set; }
    public string AWSFileName { get; set; }
    public string RecordID { get; set; }
    public BroadcastLoggerLib.Status Status { get; set; }
    public string UploadID { get; set; }
    public DateTime CreationDate {get;set;}
    public StorageResult(string stationID,
            string stationName,
            string authCode,
            string localFileName,
            string awsFileName,
            string recordID,
            DateTime creationDate,
            int status,
            string uploadID)
    {
        this.StationID = stationID;
        this.StationName = stationName;
        this.AuthCode = authCode;
        this.LocalFileName = localFileName;
        this.AWSFileName = awsFileName;
        this.RecordID = recordID;
        this.CreationDate = creationDate;    
        this.Status = (BroadcastLoggerLib.Status)status;
        this.UploadID = uploadID;
    }
}
