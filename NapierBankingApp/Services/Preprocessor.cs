﻿using NapierBankingApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic.FileIO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Navigation;

namespace NapierBankingApp.Services
{
    public class Preprocessor
    {
        public Dictionary<string, int> TrendingList { get; private set; }
        public Dictionary<string, int> MentionsList { get; private set; }
        public List<string[]> SirList { get; private set; }
        public Dictionary<string, int> QuarantinedLinks { get; private set; }

        private Dictionary<string, string> abbreviations;

        

        public Preprocessor()
        {
            TrendingList = new Dictionary<string, int>();
            MentionsList = new Dictionary<string, int>();
            SirList = new List<string[]>();
            QuarantinedLinks = new Dictionary<string, int>();
            abbreviations = new Dictionary<string, string>();
            LoadAbbreviations("textwords.csv");
        }

        #region Preprocessors
        private Tweet PreprocessTweet(Message message)
        {
            message.Text = SobstituteAbbreviations(message.Text);
            AddToMentionList(message.Text);
            AddToTrendingList(message.Text);
            Tweet tweet = new Tweet(message.Header, message.Sender, message.Text);
            return tweet;
        }
        private SMS PreprocessSMS(SMS message)
        {
            message.Text = SobstituteAbbreviations(message.Text);
            return message;
        }
        private SEM PreprocessSEM(SEM message)
        {
            message.Text = SobstituteURL(message.Text);
            return message;
        }
        private SIR PreprocessSIR(SIR message)
        {
            message.Text = SobstituteURL(message.Text);
            //Add To SIR
            string[] sirObject = { message.SortCode, message.IncidentType };
            SirList.Add(sirObject);
            return message;
        }
        public Message PreprocessMessage(Message message)
        {
            switch (message.MessageType)
            {
                case "S":
                    SMS sms = new SMS(message.Header, message.Sender, message.Text);
                    sms = PreprocessSMS(sms);
                    return sms;
                case "E":
                    Email email = (Email)message;
                    if (email.EmailType == "SEM")
                    {
                        SEM sem = (SEM)email;
                        sem = PreprocessSEM(sem);
                        return sem;

                    }
                    else if (email.EmailType == "SIR")
                    {
                        SIR sir = (SIR)email;
                        sir = PreprocessSIR(sir);
                        return sir;
                    }
                    else
                    {
                        throw new Exception("Email type not recognized, please make sure you have a valid email type.");
                    }
                case "T":
                    Tweet tweet = new Tweet(message.Header, message.Sender, message.Text);
                    tweet = PreprocessTweet(tweet);
                    return tweet;
                default:
                    throw new Exception("Incorrect message type");
            }
            
        }
        #endregion

        #region Preprocessor private methods
        private void LoadAbbreviations(string filename)
        {
            var path = Path.Combine(Environment.CurrentDirectory, filename);
            var lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                var abbreviation = line.Split(',');
                abbreviations.Add(abbreviation[0], abbreviation[1]);
            }
        }
        private string SobstituteAbbreviations(string text)
        {
            foreach (var entry in abbreviations)
            {
                text = text.Replace(entry.Key, $"{entry.Key} <{entry.Value}>");
            }
            return text;
        }

        private string SobstituteURL(string text)
        {
            List<string> urls = new List<string>();
            var linkRegex = @"\b(?:http(s)?:\\\\)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+\b";
            foreach (Match match in Regex.Matches(text, linkRegex))
            {
                urls.Add(match.ToString());
                if (QuarantinedLinks.ContainsKey(match.ToString()))
                {
                    QuarantinedLinks[match.ToString()] += 1;
                }
                else
                {
                    QuarantinedLinks.Add(match.ToString(), 1);
                }
            }

            foreach (var url in urls)
            {
                text = text.Replace(url, $"<URL quarantined>");
            }
            return text;
        }

        /// <summary>
        /// Checks if the text has any occurencies of twitter Ids, is so updates the instance, otherwise it adds it to the mention list.
        /// </summary>
        /// <param name="text"></param>
        private void AddToMentionList(string text)
        {

            // Split the text in order to find each id
            string[] textSplit = text.Split('@');
            foreach (var chunk in textSplit)
            {
                var id = '@' + chunk;
                foreach (Match match in Regex.Matches(id, @"\B\@\w{1,15}\b"))
                {
                    if (MentionsList.ContainsKey(match.ToString()))
                    {
                        MentionsList[match.ToString()] += 1;
                    }
                    else
                    {
                        MentionsList.Add(match.ToString(), 1);
                    }
                }
            }

           
        }

        /// <summary>
        /// Checks if the list has any mentions of the trending list, if so updates the list, otherwise it adds the new instance to the trending list.
        /// </summary>
        /// <param name="text"></param>
        private void AddToTrendingList(string text)
        {
            // Split the text in order to find each hashtag
            string[] textSplit = text.Split('#');
            foreach (var chunk in textSplit)
            {
                var hashtag = '#' + chunk;
                //  Hashtags can only contain letters, numbers, and underscores
                foreach (Match match in Regex.Matches(hashtag, @"\B\#\w{1,15}\b"))
                {
                    if (TrendingList.ContainsKey(match.ToString()))
                    {
                        TrendingList[match.ToString()] += 1;
                    }
                    else
                    {
                        TrendingList.Add(match.ToString(), 1);
                    }
                }
            }

        }

        #endregion
    }
}