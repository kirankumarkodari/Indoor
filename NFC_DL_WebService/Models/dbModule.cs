using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cassandra;
using FirebirdSql.Data.FirebirdClient;
using System.IO;
using System.Data.OleDb;

namespace NFC_DL_WebService.Models
{
    public class dbModule
    {       
        //Added by D. Venkata Naresh on 24-11-2015 to include Cassandra DB Connection
        private static Cluster cluster;
        private static ISession session;
        private static string serverIpAddress;
        private static string keyspaceName;
        public dbModule()
        {            
          if ((cluster == null) || (session == null))
          {
              ReconnectServer();
          }
        }

        void ReconnectServer()
        {
            try
            {
                serverIpAddress = "192.168.0.166";
                keyspaceName = "effesensors";
                cluster = Cluster.Builder().AddContactPoint(serverIpAddress).WithCredentials("dba", "Efftronics@123").WithRetryPolicy(DowngradingConsistencyRetryPolicy.Instance).WithReconnectionPolicy(new ConstantReconnectionPolicy(100L)).Build();
                session = cluster.Connect(keyspaceName);
            }
            catch (Exception ex)
            {
                //do nothing
            }
        }
        public static void writeIntoFile(string msg)
        {
                // Write the string to a file.
                string dbPath = System.Web.HttpContext.Current.Request.PhysicalApplicationPath;
                System.IO.Directory.CreateDirectory(dbPath + "Errors");
                string FilePath = dbPath + "Errors//ErrorsOn" + DateTime.Now.ToString("dd") + DateTime.Now.ToString("MM") + DateTime.Now.ToString("yy") + ".txt";
                using (FileStream fs = new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    StreamWriter file = new StreamWriter(fs);
                    file.WriteLine(DateTime.Now.ToString() + ": " + msg);
                    file.WriteLine("*************************************************************");
                    file.Close();
                }
        }
        public List<Row> postQuery(List<string> queries)
        {                      
            try
            {
                List<Row> rows = new List<Row> { };
                foreach (string query in queries)
                    {
                        RowSet result = session.Execute(query);
                        List<Row> resultantRows = result.GetRows().ToList();
                        if (resultantRows.Count() > 0)
                        {
                            rows.AddRange(resultantRows);
                        }
                    }
                return rows;
            }
            catch (NoHostAvailableException ex)
            {
                ReconnectServer();
                throw ex;
            }
            catch (Exception ex)
            {
                ReconnectServer();
                writeIntoFile("In postQuery() while Executing Queries"+Environment.NewLine + ex.ToString());
                throw ex;
            }
        }

        /*public void postQueryIntoFB(string sqlQuery)
        {
            DateTime today = DateTime.Now;
            FbConnection fbconnection = new FbConnection();            
            
            try
            {
                string dbName = "192.168.0.160:C:\\NMRH\\BackUp\\" + today.ToString("yy") + today.ToString("MM") + today.ToString("dd") + "001.GDB";
                int retryCnt = 1;
                repeatConnection:
                try
                {
                    string connectionString = "Server=192.168.0.160;User ID=sysdba;Password=masterkey;" +
               "Database=" + dbName + "; " +
               "DataSource=localhost;Charset=NONE;Pooling=true;MinPoolSize=0;MaxPoolSize=50;";
                    fbconnection.ConnectionString = connectionString;
                    fbconnection.Open();
                }
                catch (Exception e)
                {
                    today = today.AddDays(-1);
                    dbName = "192.168.0.160:C:\\NMRH\\BackUp\\" + today.ToString("yy") + today.ToString("MM") + today.ToString("dd") + "001.GDB";
                    retryCnt++;
                    if (retryCnt < 5)
                    {
                        goto repeatConnection;
                    }
                }

                FbTransaction insert = fbconnection.BeginTransaction();
                FbCommand insertCommand = new FbCommand(sqlQuery, fbconnection, insert);
                //Execute insert Statement
                insertCommand.ExecuteNonQuery();
                // Commit changes
                insert.Commit();
                // Free command resources in Firebird Server
                insertCommand.Dispose();
            }
            catch (Exception ex)
            {
                writeIntoFile("Firebird Error:" + Environment.NewLine + ex.ToString());
            }
            finally
            {
                // Close connection
                fbconnection.Close();
            }
        }
         */
        public void postQueryIntoFB(string sqlQuery)
        {
            DateTime today = DateTime.Now;
            FbConnection fbconnection = new FbConnection();

            try
            {
                string dbName = "192.168.0.153:C:\\NMRH\\BackUp\\" + today.ToString("yy") + today.ToString("MM") + today.ToString("dd") + "001.GDB";
                string connectionString = "Server=192.168.0.153;User ID=sysdba;Password=masterkey;" +
               "Database=" + dbName + "; " +
               "DataSource=localhost;Charset=NONE;Pooling=true;MinPoolSize=0;MaxPoolSize=50;";
                    fbconnection.ConnectionString = connectionString;
                    fbconnection.Open();                

                FbTransaction insert = fbconnection.BeginTransaction();
                FbCommand insertCommand = new FbCommand(sqlQuery, fbconnection, insert);
                //Execute insert Statement
                insertCommand.ExecuteNonQuery();
                // Commit changes
                insert.Commit();
                // Free command resources in Firebird Server
                insertCommand.Dispose();
            }
            catch (Exception ex)
            {
                //writeIntoFile("Firebird Error:" + Environment.NewLine + ex.ToString());
            }
            finally
            {
                // Close connection
                fbconnection.Close();
            }
        }
    }
}
