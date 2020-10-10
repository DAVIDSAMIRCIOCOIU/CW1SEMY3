using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NapierBankingApp.Models
{
    public class SIR : Email
    {
        private readonly string _messageType;
        private string _header;
        private string _sender;
        private string _text;
        private string _subject;
        private string _emailType;
        private string _sortCode;
        private string _incidentType;

        public SIR()
        {
            _messageType = "SIR";
        }

        public SIR(string header, string sender, string text, string subject, string emailType, string sortCode, string incidentType)
        {
            _messageType = "SIR";
            _header = header;
            _sender = sender;
            _text = text;
            _subject = subject;
            _emailType = emailType;
            _sortCode = sortCode;
            _incidentType = incidentType;
        }

        public override string MessageType { get { return _messageType; } }
        public override string Header { get { return _header; } set { _header = value; } }
        public override string Sender { get { return _sender; } set { _sender = value; } }
        public override string Text { get { return _text; } set { _text = value; } }
        public override string Subject { get { return _subject; } set { _subject = value; } }
        public override string EmailType { get { return _emailType; } set { _emailType = value; } }
        public string SortCode { get { return _sortCode; } set { _sortCode = value; } }
        public string IncidentType { get; set; }

        public override string ToString()
        {
            return "MessageType:" + MessageType + "\nEmail Type: " + EmailType + "\nHeader: " + Header + "\nSender: " + Sender  + "\nSubject: " + Subject + "\nSorCode: " + SortCode + "\nIncidentTypet: " + IncidentType + "\nText: " + Text;
        }
    }
}