using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Collections;
using System.Data;

namespace TheGameServer
{
    class Database
    {
        private static SqlConnection _sqlConnection;

        public static bool Initialize()
        {
            try
            {
                var connectionString = string.Format(
                    "User ID='{0}';Password='{1}';Server={2};Database={3};Trusted_Connection={4};Connection Timeout = 1;Pooling=True;MultipleActiveResultSets=True",
                    Globals.Config.Database.User, Globals.Config.Database.Pass, Globals.Config.Database.Host,
                    Globals.Config.Database.DatabaseName, Globals.Config.Database.WindowsAuth
                    );

                _sqlConnection = new SqlConnection(connectionString);
                _sqlConnection.Open();
                return true;
            }
            catch (Exception e)
            {
                Log.Write("Error Initializing DB: {0}", e.Message);
                return false;
            }
        }

        private static void Execute(string szQuery)
        {
            lock (_sqlConnection)
            {
                using (var command = new SqlCommand(szQuery, _sqlConnection))
                    command.ExecuteNonQuery();
            }
        }
        private static void Execute(string szQuery, ArrayList pArray)
        {
            lock (_sqlConnection)
            {
                using (var command = new SqlCommand(szQuery, _sqlConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader == null)
                            return;

                        while (reader.Read())
                            for (int i = 0; i < reader.FieldCount; ++i)
                                pArray.Add(reader.IsDBNull(i) ? 0 : reader[i]);
                    }
                }
            }
        }
        private static int GetQuery(string szQuery)
        {
            lock (_sqlConnection)
            {
                using (var command = new SqlCommand(szQuery, _sqlConnection))
                    return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public static int GetMaxPlayers(short id)
        {
            return (int)GetQueryScalar("SELECT Maxplayers FROM ServerStatus WHERE ID = " + id);
        }

        public static int Login(string user, string pass, string ip)
        {
            int Aid = GetQuery("SELECT AID FROM Account WHERE UserID = '"+user+"' AND Password = '"+pass+"' AND Loggedin = 0");
            if (Aid > 0)
            {
                Execute("UPDATE Account SET Loggedin = 1 WHERE AID = " + Aid);
                Execute("INSERT INTO Login (AID, UserID, LastIP, LastConnDate) VALUES ("+Aid+", '" +user+"', '"+ip+"', '"+DateTime.Now+"')");
            }
            return Aid;
        }

        public static void Logout(int AID)
        {
            Execute("UPDATE Account SET Loggedin = 0 WHERE AID = "+AID);
            Execute("DELETE FROM Login WHERE AID = " + AID);
        }

        private static object GetQueryScalar(string szQuery)
        {
            lock (_sqlConnection)
            {
                using (var command = new SqlCommand(szQuery, _sqlConnection))
                    return command.ExecuteScalar();
            }
        }

        #region Modules
        public static bool AccountExists(string user)
        {
            return GetQuery("SELECT COUNT(AID) FROM Account WHERE UserID='" + user + "'") == 1;
        }

        public static bool CreateCharacter(Int32 aid, byte nCharNumber, string szName, Int32 nSex, Int32 nHair, Int32 nFace, Int32 nCostume)
        {
            lock (_sqlConnection)
            {
                using (var command = new SqlCommand("dbo.spInsertChar", _sqlConnection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@AID", aid);
                    command.Parameters.AddWithValue("@CharNum", nCharNumber);
                    command.Parameters.AddWithValue("@Name", szName);
                    command.Parameters.AddWithValue("@Sex", nSex);
                    command.Parameters.AddWithValue("@Hair", nHair);
                    command.Parameters.AddWithValue("@Face", nFace);
                    command.Parameters.AddWithValue("@Costume", nCostume);

                    var returnValue = new SqlParameter("@Return_Value", DbType.Int32);
                    returnValue.Direction = ParameterDirection.ReturnValue;

                    command.Parameters.Add(returnValue);
                    command.ExecuteNonQuery();
                    return Int32.Parse(command.Parameters["@Return_Value"].Value.ToString()) != -1;
                }
            }
        }

        public static void UpdateBp(int cid, int newBounty)
        {
            Execute(string.Format("UPDATE Character SET BP={0} WHERE CID={1}", newBounty, cid));
        }

        public static bool CharacterExists(string name)
        {
            return GetQuery("SELECT COUNT(Name) FROM character WHERE Name='" + name + "'") > 0;
        }

        public static bool DeleteCharacter(Int32 aid, Int32 cid, Int32 nCharNum, string sCharName)
        {
            Execute(string.Format("UPDATE Character SET CharNum=-1, DeleteFlag=1, Name='', DeleteName='{0}' WHERE CID={1}", sCharName, cid));
            UpdateIndexes(aid);

            return true;
        }
        public static void UpdateIndexes(Int32 aid)
        {
            lock (_sqlConnection)
            {
                using (var command = new SqlCommand("SELECT TOP 4 CID FROM character WHERE AID=@aid AND DeleteFlag !=1", _sqlConnection))
                {
                    command.Parameters.AddWithValue("@aid", aid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader == null)
                            return;

                        var ii = new List<int>();
                        for (var i = 0; reader.Read(); ++i)
                            ii.Add(Convert.ToInt32(reader[0]));

                        reader.Close();

                        for (var i = 0; i < ii.Count; ++i)
                            Execute(string.Format("update character set CharNum={0} where cid={1}", i, ii[i]));
                    }
                }
            }
        }


        public static int GetCharacterCount(Int32 aid)
        {
            return GetQuery(string.Format("SELECT COUNT(AID) FROM character WHERE AID={0} AND DeleteFlag !=1", aid));
        }

        public static void JoinClan(Int32 cid, Int32 clid)
        {
            Execute(string.Format("INSERT INTO ClanMember (CLID,CID,Grade,RegDate) VALUES ({0},{1}, 9, GetDate())", clid, cid));
        }
        public static void ExpelMember(Int32 cid)
        {
            Execute("DELETE FROM ClanMember WHERE CID=" + cid);
        }
        public static void UpdateMember(Int32 cid, Int32 rank)
        {
            Execute("UPDATE ClanMember SET Grade=" + rank + " WHERE CID=" + cid);
        }

        public static void AddPlayerCount()
        {
            int nCurrPlayer = GetQuery("SELECT CurrPlayer FROM ServerStatus WHERE ServerID=" + Globals.Config.Server.Id);

            nCurrPlayer += 1;

            Execute(string.Format("UPDATE ServerStatus SET CurrPlayer={0} WHERE ServerID={1}", nCurrPlayer, Globals.Config.Server.Id));
        }
        public static void SubtractPlayerCount()
        {
            int nCurrPlayer = GetQuery("SELECT CurrPlayer FROM ServerStatus WHERE ServerID=" + Globals.Config.Server.Id);

            nCurrPlayer += 1;

            Execute(string.Format("UPDATE ServerStatus SET CurrPlayer={0} WHERE ServerID={1}", nCurrPlayer, Globals.Config.Server.Id));
        }

        public static Int32 GetClanId(string clanName)
        {
            lock (_sqlConnection)
            {
                using (var command = new SqlCommand("SELECT CLID FROM Clan where Name=@name", _sqlConnection))
                {
                    command.Parameters.AddWithValue("@name", clanName);

                    return (Int32)command.ExecuteScalar();
                }
            }
        }

        public static string GetCharacterName(Int32 cid)
        {
            lock (_sqlConnection)
            {
                using (var command = new SqlCommand("SELECT name FROM character where cid=@cid", _sqlConnection))
                {
                    command.Parameters.AddWithValue("@cid", cid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return string.Empty;

                        return Convert.ToString(reader["name"]);
                    }
                }
            }
        }
        /*public static void GetClanInfo(Int32 clanId, ref ClanInfo clanInfo)
        {
            lock (_sqlConnection)
            {
                using (
                    var command = new SqlCommand("SELECT * FROM clan WHERE CLID=@clid",
                                                   _sqlConnection))
                {
                    command.Parameters.AddWithValue("@clid", clanId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader == null || !reader.Read())
                        {
                            clanInfo = null;
                            return;
                        }

                        clanInfo.ClanId = clanId;
                        clanInfo.Name = Convert.ToString(reader["name"]);
                        clanInfo.Points = (Int32)reader["exp"];
                        clanInfo.Level = Convert.ToInt16(reader["level"]);
                        clanInfo.TotalPoints = (Int32)reader["point"];
                        clanInfo.Wins = Convert.ToInt16(reader["wins"]);
                        clanInfo.Losses = Convert.ToInt16(reader["losses"]);
                        clanInfo.Ranking = (Int32)reader["ranking"];
                        clanInfo.EmblemChecksum = reader["emblemurl"] == null ? 0 : 1;
                        var cid = (Int32)reader["mastercid"];

                        reader.Close();

                        clanInfo.Master = GetCharacterName(cid);
                        clanInfo.MemberCount = Convert.ToInt16(GetQuery("SELECT COUNT(CID) FROM ClanMember WHERE CLID=" + clanId));
                    }

                }
            }*/
        }
        #endregion
    }
