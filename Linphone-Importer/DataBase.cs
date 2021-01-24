using System.Collections.Generic;
using System.Data.SQLite;

namespace Contabo.Tools.Linphone.Importer
{
    public class DataBase
    {
        private SQLiteConnection connection;

        public bool IsDatabaseSelected { get; private set; }

        public bool IsDatabaseOpen { get; private set; }

        public void UpdateContact(VCard contact)
        {
            var command = new SQLiteCommand(connection);

            if (HasContact(contact))
            {
                command.CommandText = "UPDATE friends SET vCard = @vCard WHERE sip_uri = @sipUri";
                command.Parameters.AddWithValue("@vCard", contact.ToRawString);
                command.Parameters.AddWithValue("@sipUri", GetSipURI(contact.Phone));
            }
            else
            {
                command.CommandText = "INSERT INTO friends (friend_list_id, sip_uri, subscribe_policy, send_subscribe, vCard, presence_received) VALUES(@listID, @sipUri, @sp, @ss, @vCard, @pr)";
                command.Parameters.AddWithValue("@listID", 1);
                command.Parameters.AddWithValue("@sipUri", GetSipURI(contact.Phone));
                command.Parameters.AddWithValue("@sp", 1);
                command.Parameters.AddWithValue("@ss", 1);
                command.Parameters.AddWithValue("@vCard", contact.ToRawString);
                command.Parameters.AddWithValue("@pr", 0);
            }

            command.Prepare();
            command.ExecuteNonQuery();
        }

        public IList<string> GetAll()
        {
            using var command = new SQLiteCommand("SELECT vCard FROM friends", connection);
            using var reader = command.ExecuteReader();
            IList<string> contacts = new List<string>();

            while (reader.Read())
            {
                contacts.Add(reader.GetString(0));
            }

            return contacts;
        }

        public bool HasContact(VCard contact)
        {
            var command = new SQLiteCommand(connection);
            command.CommandText = "SELECT sip_uri FROM friends WHERE sip_uri = @sipUri";
            command.Parameters.AddWithValue("@sipUri", GetSipURI(contact.Phone));
            command.Prepare();

            return command.ExecuteScalar() != null;
        }

        public void Open(string dbFilePath)
        {
            if (IsDatabaseOpen) Close();

            connection = new SQLiteConnection(@$"URI=file:{dbFilePath}");
            connection.Open();
            IsDatabaseOpen = true;
        }

        public void Close()
        {
            if (IsDatabaseOpen)
            {
                connection.Close();
                IsDatabaseOpen = false;
            }
        }

        private string GetSipURI(string number)
        {
            return $"sip:{number}@fpbx.de";
        }
    }
}
