using System.Linq;

namespace Contabo.Tools.Linphone.Importer
{
    public class VCard
    {
        public string Name { get; private set; }

        public string EMail { get; private set; }

        public string Phone { get; private set; }

        public string Nickname
        {
            get
            {
                return string.IsNullOrEmpty(_nickname) ? string.Join("", from names in Name.Split(" ") select names[0].ToString().ToUpper()) : _nickname;
            }
            private set
            {
                _nickname = value;
            }
        }

        private string _nickname;

        public string ToRawString { get => $"BEGIN:VCARD\r\nVERSION:4.0\r\nIMPP:sip:{Phone}@fpbx.de\r\nFN:{Name}\r\nNICKNAME:{Nickname}\r\nROLE:Contabo GmbH\r\nADR:;;;;;;\r\nEMAIL:{EMail}\r\nEND:VCARD"; }

        public VCard(string name, string email, string phone, string nickname = "")
        {
            Name = name;
            EMail = email;
            Phone = phone;
            Nickname = nickname;
        }

        public override string ToString()
        {
            return $"Phone: {Phone}, Name: {Nickname} - {Name}, EMail: {EMail}";
        }
    }
}
