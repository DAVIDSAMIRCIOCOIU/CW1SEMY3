﻿using NapierBankingApp.Commands;
using NapierBankingApp.Models;
using NapierBankingApp.Services;
using NapierBankingApp.Services.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NapierBankingApp.ViewModels
{
    class MainWindowViewModel : BaseViewModel
    {
        public ObservableCollection<Message> LoadedMessages { get; set; }
        public string MessageHeaderTextBlock { get; private set; }
        public string MessageBodyTextBlock { get; private set; }
        public string MessageSenderTextBlock { get; private set; }
        public string MessageIncidentTypeTextBlock { get; private set; }
        public string MessagesSortCodeTextBlock { get; private set; }
        public string MessageTextTextBlock { get; private set; }
        public string MessageSubjectTextBlock { get; private set; }
        public string MessageErrorTextBlock { get; private set; }
        public string SaveMessageErrorTextBlock { get; private set; }

        public string MessageHeaderTextBox { get; set; }
        public string MessageBodyTextBox { get; set; }
        public string ProcessedMessageSenderTextBox { get; set; }
        public string ProcessedMessageSortCodeTextBox { get; set; }
        public string ProcessedMessageIncidentTypeTextBox { get; set; }
        public string ProcessedMessageSubjectTextBox { get; set; }
        public string ProcessedMessageHeaderTextBox { get; set; }
        public string ProcessedMessageTextTextBox { get; set; }


        public ICommand ProcessMessageButtonCommand { get; private set; }
        public ICommand ClearMessageButton { get; private set; }
        public ICommand SaveMessageButtonCommand { get; private set; }
        public ICommand LoadMessageButtonCommand { get; private set; }

        public string ProcessMessageButtonText { get; private set; }
        public string ClearMessageButtonText { get; private set; }
        public string SaveMessageButtonText { get; private set; }
        public string LoadMessageButtonText { get; private set; }

        Processor processor;
        Validator validator;
        Database database;
        List<Message> currentMessages;
        Message currentMessage;

        public MainWindowViewModel()
        {
            LoadedMessages = new ObservableCollection<Message>();

            MessageHeaderTextBlock = "Header";
            MessageBodyTextBlock = "Body";
            MessageSenderTextBlock = "Sender";
            MessageIncidentTypeTextBlock = "Incident Type";
            MessagesSortCodeTextBlock = "Sort Code";
            MessageTextTextBlock = "Text";
            MessageSubjectTextBlock = "Subject";

            MessageErrorTextBlock = string.Empty;
            SaveMessageErrorTextBlock = string.Empty;


            ProcessMessageButtonText = "Process";
            ClearMessageButtonText = "Clear";
            SaveMessageButtonText = "Save";
            LoadMessageButtonText = "Load Messages";

            MessageHeaderTextBox = string.Empty;
            MessageBodyTextBox = string.Empty;
            ProcessedMessageSenderTextBox = string.Empty;
            ProcessedMessageSortCodeTextBox = string.Empty;
            ProcessedMessageIncidentTypeTextBox = string.Empty;
            ProcessedMessageSubjectTextBox = string.Empty;

            ProcessedMessageHeaderTextBox = string.Empty;
            ProcessedMessageTextTextBox = string.Empty;


            ClearMessageButton = new RelayCommand(ClearMessageButtonClick);
            ProcessMessageButtonCommand = new RelayCommand(ProcessMessageButtonClick);
            SaveMessageButtonCommand = new RelayCommand(SaveMessageButtonClick);
            LoadMessageButtonCommand = new RelayCommand(LoadMessagesFromFile);

            processor = new Processor();
            validator = new Validator();
            database = new Database("myMessage");
            currentMessages = new List<Message>();
        }

        /// <summary>
        /// Attempts so the save the message.
        /// Throws an error if the message is already in the db or if the fields are empty.
        /// </summary>
        private void SaveMessageButtonClick()
        {
            try
            {
                // Clean any leftover error
                SaveMessageErrorTextBlock = string.Empty;
                OnChanged(nameof(SaveMessageErrorTextBlock));

                // Save the message
                if (currentMessage != null)
                {
                    database.serializeToJSON(currentMessage);
                }
                else
                {
                    throw new Exception("No message to be saved.");
                }


                // Inform the user the message has been saved
                MessageBox.Show("Message Saved");
            }
            catch (Exception ex)
            {
                // Show the error to the user
                SaveMessageErrorTextBlock = ex.Message.ToString();
                OnChanged(nameof(SaveMessageErrorTextBlock));
            }

        }

        /// <summary>
        /// Attempts to process the message. Throws errors according to the invalid insertions.
        /// </summary>
        private void ProcessMessageButtonClick()
        {
            try
            {
                // Delete any leftover error from process messave
                MessageErrorTextBlock = string.Empty;
                OnChanged(nameof(MessageErrorTextBlock));
                // Clean any leftover error from saved messages
                SaveMessageErrorTextBlock = string.Empty;
                OnChanged(nameof(SaveMessageErrorTextBlock));

                // Process the message
                var message = processor.ProcessMessage(validator.ValidateMessage(MessageHeaderTextBox, MessageBodyTextBox));
                // Retain an instance of the last processed message so we can save it later
                currentMessage = message;

                // Display message in a separate component
                ProcessedMessageHeaderTextBox = message.Header;
                ProcessedMessageSenderTextBox = message.Sender;
                ProcessedMessageTextTextBox = message.Text;
                OnChanged(nameof(ProcessedMessageHeaderTextBox));
                OnChanged(nameof(ProcessedMessageTextTextBox));
                OnChanged(nameof(ProcessedMessageSenderTextBox));

                // If SIR display appropriate fields, else display "N/A"
                if (message.MessageType == "E")
                {
                    Email email = (Email)message;
                    // Display subject 
                    ProcessedMessageSubjectTextBox = email.Subject;
                    OnChanged(nameof(ProcessedMessageSubjectTextBox));

                    if (email.EmailType == "SIR")
                    {
                        SIR sir = (SIR)email;
                        ProcessedMessageSortCodeTextBox = sir.SortCode;
                        ProcessedMessageIncidentTypeTextBox = sir.IncidentType;
                        OnChanged(nameof(ProcessedMessageSortCodeTextBox));
                        OnChanged(nameof(ProcessedMessageIncidentTypeTextBox));
                    } else
                    {
                        // Assign Empty Fields
                        ProcessedMessageSortCodeTextBox = string.Empty;
                        ProcessedMessageIncidentTypeTextBox = string.Empty;
                        OnChanged(nameof(ProcessedMessageSortCodeTextBox));
                        OnChanged(nameof(ProcessedMessageIncidentTypeTextBox));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageErrorTextBlock = ex.Message.ToString();
                OnChanged(nameof(MessageErrorTextBlock));
            }
        }

        /// <summary>
        /// Clears the message section
        /// </summary>
        private void ClearMessageButtonClick()
        {
            MessageHeaderTextBox = string.Empty;
            MessageBodyTextBox = string.Empty;

            OnChanged(nameof(MessageHeaderTextBox));
            OnChanged(nameof(MessageBodyTextBox));
        }

        private void LoadMessagesFromFile()
        {
            var (messages, unloadedMessages) = validator.ValidateFile(browseFile());
            foreach (var message in messages)
            {
                try
                {
                    LoadedMessages.Add(processor.ProcessMessage(message));
                }
                catch (Exception ex)
                {
                    // Display Error For This Message
                }
            }

        }

        /// <summary>
        /// Opens a file dialog for selecting a File.
        /// </summary>
        /// <returns>A string representing the path of the file location.</returns>
        private string browseFile()
        {
            var FD = new System.Windows.Forms.OpenFileDialog();
            if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fileToOpen = FD.FileName;
                return fileToOpen;
            }
            return "";
        }
    }
}