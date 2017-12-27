using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SharePoint;
using System.Data.SqlTypes;

namespace GJ.RECEIVER.Utils
{
    public partial class ManagerSQL : IDisposable
    {
        private class LocalSettings
        {
            public LocalSettings(string name, string password, string serverName, string login)
            {
                Name = name;
                Password = password;
                ServerName = serverName;
                Login = login;
            }

            public string Login { get; }

            public string Password { get; }

            public string Name { get; }

            public string ServerName { get; }

            public bool IntegratedSequrity => (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password));
        }


        protected SqlConnectionStringBuilder ConnectionBuilder;
        protected SqlConnection ConnectionToDb;
        protected SqlTransaction Transaction;

        private static object _await = new object();
        private static Dictionary<Guid, LocalSettings> _dbSettings = new Dictionary<Guid, LocalSettings>();

        public string ConnectionString => ConnectionToDb.ConnectionString;

        public SqlConnection SqlConnection => ConnectionToDb;

        #region setting
        private static LocalSettings LoadSetting(SPWeb web)
        {
            var file = new XmlDocument();
            string connectionString;
            var localSettingFile = Function.GetFile(web, "/_catalogs/spellabs/spellabsWebPartLibrary.LocalSettings");
            using (var stream = localSettingFile.OpenBinaryStream())
            {
                file.Load(stream);
                var connectionStringNode = file.SelectSingleNode("//ConnectionString");
                connectionString = connectionStringNode?.InnerText;
            }
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string is empty");
            }
            var settingArray = connectionString.Split(';');
            var setting = new Dictionary<string, string>();
            foreach (var keyValue in settingArray)
            {
                var arr = keyValue.Split('=');
                if (arr.Count() == 2)
                {
                    var key = arr[0];
                    var value = arr[1];
                    setting[key.Trim().ToLower()] = value.Trim();
                }
            }
            string login = string.Empty;
            string password = string.Empty;
            string db = string.Empty;
            string server = string.Empty;

            if (setting.ContainsKey("user id"))
            {
                login = setting["user id"];
            }
            if (setting.ContainsKey("password"))
            {
                password = setting["password"];
            }
            if (setting.ContainsKey("initial catalog"))
            {
                db = setting["initial catalog"];
            }
            if (setting.ContainsKey("data source"))
            {
                server = setting["data source"];
            }

            return new LocalSettings(db, password, server, login);
        }



        #endregion setting

        public void BeginTransaction(string name)
        {
            Transaction = ConnectionToDb.BeginTransaction(name);
        }

        public void CommitTransaction()
        {
            Transaction.Commit();
        }

        public void RollbackTransaction()
        {
            Transaction.Rollback();
        }

        public ManagerSQL(SPWeb contextWeb) : this(contextWeb.Site.ID, contextWeb.ID)
        {
        }

        public ManagerSQL(Guid siteId, Guid webId)
        {
            lock (_await)
            {
                if (!_dbSettings.ContainsKey(webId))
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate ()
                    {
                        using (var site = new SPSite(siteId))
                        {
                            using (var web = site.OpenWeb(webId))
                            {
                                _dbSettings.Add(webId, LoadSetting(web));
                            }
                        }
                    });
                }
            }
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = _dbSettings[webId].ServerName,
                IntegratedSecurity = _dbSettings[webId].IntegratedSequrity,
                InitialCatalog = _dbSettings[webId].Name,
                AsynchronousProcessing = true
            };
            if (_dbSettings[webId].IntegratedSequrity == false)
            {
                builder.UserID = _dbSettings[webId].Login;
                builder.Password = _dbSettings[webId].Password;
            }
            ConnectionToDb = new SqlConnection(builder.ToString());
            ConnectionBuilder = builder;
            if (ConnectionToDb.State != ConnectionState.Open)
            {
                ConnectionToDb.Open();
            }
        }

        public void Dispose()
        {
            if (ConnectionToDb.State != ConnectionState.Closed)
            {
                ConnectionToDb.Close();
            }
        }

        public Dictionary<string, string> Select(string nameStoredProcedure, params SqlParameter[] parameter)
        {
            SqlCommand selectCommand = new SqlCommand(nameStoredProcedure, ConnectionToDb);
            selectCommand.CommandType = CommandType.StoredProcedure;
            selectCommand.Parameters.AddRange(parameter);
            SqlDataReader reader = selectCommand.ExecuteReader();
            if (reader.Read())
            {
                Dictionary<string, string> result = GetFromReader(reader);
                if (!result.ContainsKey("Error"))
                {
                    reader.Close();
                    return result;
                }
            }
            reader.Close();
            return null;
        }

        public List<Dictionary<string, string>> SelectAll(string nameStoredProcedure, params SqlParameter[] parameter)
        {
            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
            SqlCommand selectCommand = new SqlCommand(nameStoredProcedure, ConnectionToDb);
            selectCommand.CommandType = CommandType.StoredProcedure;
            selectCommand.Parameters.AddRange(parameter);
            SqlDataReader reader = selectCommand.ExecuteReader();

            while (reader.Read())
            {
                result.Add(GetFromReader(reader));
            }
            reader.Close();
            return result;
        }

        public Dictionary<string, string> Select(string cmdText)
        {
            SqlCommand selectCommand = new SqlCommand(cmdText, ConnectionToDb);
            SqlDataReader reader = selectCommand.ExecuteReader();
            if (reader.Read())
            {
                Dictionary<string, string> result = GetFromReader(reader);
                if (reader.Read())
                {
                    reader.Close();
                    return null;
                }
                if (!result.ContainsKey("Error"))
                {
                    reader.Close();
                    return result;
                }
            }
            reader.Close();
            return null;
        }

        public List<Dictionary<string, string>> SelectAll(string cmdText)
        {
            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
            SqlCommand selectCommand = new SqlCommand(cmdText, ConnectionToDb);
            SqlDataReader reader = selectCommand.ExecuteReader();
            while (reader.Read())
            {
                result.Add(GetFromReader(reader));
            }
            reader.Close();
            return result;
        }

        public int Command(string nameStoredProcedure, bool withTransaction, params SqlParameter[] parameter)
        {
            SqlCommand command = new SqlCommand(nameStoredProcedure, ConnectionToDb);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddRange(parameter);
            command.UpdatedRowSource = UpdateRowSource.OutputParameters;
            if (withTransaction)
            {
                command.Transaction = Transaction;
            }
            int count = command.ExecuteNonQuery();
            return count;
        }

        public void CommandAsync(string query)//выполнение запроса без результата - для создания-удаления таблиц
        {
            SqlCommand command = new SqlCommand(query, ConnectionToDb);
            command.BeginExecuteNonQuery();
        }

        public int Command(string cmdText)
        {
            SqlCommand command = new SqlCommand(cmdText, ConnectionToDb);
            command.CommandType = CommandType.Text;
            int count = command.ExecuteNonQuery();
            return count;
        }

        private Dictionary<string, string> GetFromReader(SqlDataReader reader)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                result[reader.GetName(i)] = reader.GetValue(i).ToString();
            }
            return result;
        }

        public string LogCommand(string nameStoredProcedure, bool WithTransaction, params SqlParameter[] parameter)
        {
            var result = String.Format("Command[WithTransaction: {1}] {0} : ", nameStoredProcedure, WithTransaction);
            return parameter.Aggregate(result, (current, p) => current + String.Format(", {0} = {1}", p.ParameterName, p.Value));
        }

        public string LogSelect(string nameStoredProcedure, params SqlParameter[] parameter)
        {
            var result = String.Format("Select {0} : ", nameStoredProcedure);
            return parameter.Aggregate(result, (current, p) => current + String.Format(", {0} = {1}", p.ParameterName, p.Value));
        }

    }
}