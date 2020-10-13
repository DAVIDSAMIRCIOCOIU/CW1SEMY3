﻿using NapierBankingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NapierBankingApp.Services.Validation
{
    class EmailValidator : MessageValidator
    {
        public static Email ValidateEmail(string header, string body)
        {
            var fields = Parser.ParseBody(body, ",", true);
            var sender = ValidateSender(fields, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z");
            var type = ValidateSubject(fields, 1, @"^SIR \d{1,2}/\d{1,2}/\d{4}$");

            if (type == "SIR")
            {
                return new SIR(header, fields[0], fields[1], ValidateSortCode(fields, 2, @"\b[0-9]{2}-?[0-9]{2}-?[0-9]{2}\b"), ValidateIncidentType(fields, 3), ValidateText(fields, 4, 1028));
            }
            else if (type == "SEM")
            {
                return new SEM(header, fields[0], fields[1], ValidateText(fields, 2, 1028));
            }
            else
            {
                throw new Exception("Can't validate the email, the sender has an invalid type.");
            }
        }

        /// <summary>
        /// Validates the subject of an email.
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="subjectRegex"></param>
        /// <returns>The type of the email.</returns>
        protected static string ValidateSubject(List<string> fields, int subjectIndex,string subjectRegex)
        {
            if ((fields.Count < (subjectIndex + 1)))
            {
                throw new Exception("The body must have a subject specified.");
            }
            if (string.IsNullOrWhiteSpace(fields[subjectIndex])) { throw new Exception("The subject can not be empty."); }
            if (fields[subjectIndex].Length > 20) { throw new Exception("The subject length must be less or equal to 20 characters."); }
            if (Regex.IsMatch(fields[subjectIndex], subjectRegex))
            {
                return "SIR";
            }
            else
            {
                return "SEM";
            }
        }

        protected static string ValidateSortCode(List<string> fields, int sortCodeIndex, string sortCodeRegex)
        {
            if ((fields.Count < (sortCodeIndex + 1)))
            {
                throw new Exception("The body must have a sort code specified.");
            }
            if (fields[sortCodeIndex] != Regex.Match(fields[sortCodeIndex], sortCodeRegex).Value)
            {
                throw new Exception("Incorrect sort code format.");
            }
            return fields[sortCodeIndex];
        }

        protected static string ValidateIncidentType(List<string> fields, int incidentIndex)
        {
            if ((fields.Count < (incidentIndex + 1)))
            {
                throw new Exception("The body must have an incident type specified.");
            }
            List<string> incidentTypes = new List<string>() { "Theft", "Staff Attack", "ATM Theft", "Raid", "Customer Attack", "Staff Abuse", "Bomb Threat", "Terrorism", "Suspicious Incident", "Intelligence", "Cash Loss" };
            if (!incidentTypes.Contains(fields[incidentIndex]))
            {
                throw new Exception("Invalid incident type.");
            }
            return fields[incidentIndex];
        }
    }
}