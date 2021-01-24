using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Contabo.Tools.Linphone.Importer
{
    internal class VCardsReader
    {
        public static VCard FromRawString(string rawString)
        {
            var patterns = new string[] {
                @"\bsip:\d*@fpbx.de\b",
                @"\bFN:(\w*-\w*\s\w*-\w*|\w*\s\w*-\w*|\w*-\w*\s\w*|\w*\s\w*)\b",
                @"\bNICKNAME:\w*\b",
                @"\bEMAIL:(\w*|\w*-\w*).(\w*|\w*-\w*)@contabo.(de|com)\b"
            };

            string name = "";
            string nickname = "";
            string email = "";
            string phone = "";

            for (var i = 0; i < patterns.Length; i++)
            {
                var pattern = patterns[i];
                Match m = Regex.Match(rawString, pattern);

                if (m.Success && i == 0)
                    phone = m.Value.Replace("sip:", "").Replace("@fpbx.de", "");

                else if (m.Success && i == 1)
                    name = m.Value.Replace("FN:", "");

                else if (m.Success && i == 2)
                    nickname = m.Value.Replace("NICKNAME:", "");

                else if (m.Success && i == 3)
                    email = m.Value.Replace("EMAIL:", "");
            }

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(phone) && phone != "0")
            {
                return new VCard(name, email, phone, nickname);
            }

            throw new Exception("The raw string could not be converted into a vCard object.");
        }

        public IDictionary<string, VCard> ReadContacts(string vCardFilePath)
        {
            if (!File.Exists(vCardFilePath)) throw new FileNotFoundException("The file does not exist.", vCardFilePath);

            var contacts = new Dictionary<string, VCard>();
            var lines = from line in File.ReadLines(vCardFilePath) select line;
            var has_phone = false;
            string name = "";
            string nickname = "";
            string phone = "";
            string email = "";

            foreach (var line in lines)
            {
                if (line.Contains("BEGIN:VCARD"))
                {
                    has_phone = false;
                }
                else if (line.Contains("END:VCARD"))
                {
                    if (has_phone && !contacts.ContainsKey(name)) contacts.Add(name, new VCard(name, email, phone, nickname));
                }
                else if (line.Contains("FN:"))
                {
                    name = line.Replace("FN:", "");
                }
                else if (line.Contains("NICKNAME:"))
                {
                    nickname = line.Replace("NICKNAME:", "");
                }
                else if (line.Contains("EMAIL:"))
                {
                    email = line.Replace("EMAIL:", "");
                }
                else if (line.Contains("TEL;type=work:"))
                {
                    if (int.TryParse(line.Replace("TEL;type=work:", ""), out int number))
                    {
                        if (number > 0)
                        {
                            has_phone = true;
                            phone = number.ToString();
                        }
                    }
                }
            }

            return contacts;
        }
    }
}
