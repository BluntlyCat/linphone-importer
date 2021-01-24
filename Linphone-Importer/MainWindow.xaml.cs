using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Contabo.Tools.Linphone.Importer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbFilePath = string.Format("{0}\\Linphone\\friends.db", localAppDataPath);

            if (File.Exists(dbFilePath) == true)
            {
                App.DbFilePath = dbFilePath;
                SetDbContactsList();
                SetFileText(txtDbFile, listBoxDBContacts.Items.Count, dbFilePath);
            }
        }

        private void SetFileText(TextBlock txtBlock, int count, string fileName)
        {
            var fileParts = fileName.Split("\\");
            txtBlock.Text = $"{count} Contacts loaded from {fileParts[fileParts.Length - 1]} file";
        }

        private void SetDbContactsList()
        {
            try
            {
                App.DB.Open(App.DbFilePath);
                var rows = App.DB.GetAll();
                App.DB.Close();

                var contacts = new List<VCard>();
                var failed = new List<string>();

                foreach (var row in rows)
                {
                    try
                    {
                        contacts.Add(VCardsReader.FromRawString(row));
                    }
                    catch
                    {
                        failed.Add(row);
                    }
                }

                if (failed.Count > 0)
                {
                    var msg = $"The following contacts could not be loaded from Linphone's DB:\r\n\r\n{string.Join("\r\n", failed)}";
                    var caption = "Some contacts could not be loaded";

                    if (failed.Count == 1)
                    {
                        msg = $"The following contact could not be loaded from Linphone's DB:\r\n\r\n{string.Join("\r\n", failed)}";
                        caption = "A contact could not be loaded";
                    }

                    ShowError(msg, caption);
                }

                listBoxDBContacts.ItemsSource = contacts.OrderBy(card => card.Nickname);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message, "Critical error");
            }
        }

        private void btnOpenDbClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "SQLite3 Files (*.db)|*.db";

            if (fileDialog.ShowDialog() == true)
            {
                App.DbFilePath = fileDialog.FileName;
                SetDbContactsList();
                SetFileText(txtDbFile, listBoxDBContacts.Items.Count, fileDialog.FileName);
            }
        }

        private void btnOpenVCardClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "vCard Files (*.vcf)|*.vcf";
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (fileDialog.ShowDialog() == true)
            {
                try
                {
                    var contacts = (new VCardsReader()).ReadContacts(fileDialog.FileName);

                    listBoxVCardContacts.ItemsSource = from vCards in contacts.Values.OrderBy(card => card.Nickname) select vCards;
                    listBoxVCardContacts.SelectAll();
                    SetFileText(txtVCardFile, listBoxVCardContacts.Items.Count, fileDialog.FileName);
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message, "Critical error");
                }
            }
        }

        private void btnImportClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(App.DbFilePath))
                {
                    if (listBoxVCardContacts.Items.Count > 0)
                    {
                        if(listBoxVCardContacts.SelectedItems.Count > 0)
                        {
                            App.DB.Open(App.DbFilePath);

                            foreach (var contact in listBoxVCardContacts.SelectedItems)
                            {
                                try
                                {
                                    App.DB.UpdateContact(contact as VCard);
                                }
                                catch (Exception ex)
                                {
                                    ShowError(ex.Message, "Critical error");
                                }
                            }

                            App.DB.Close();

                            var msg = $"{listBoxVCardContacts.SelectedItems.Count} contacts have been successfully imported.";
                            if(listBoxVCardContacts.SelectedItems.Count == 1)
                                msg = $"{listBoxVCardContacts.SelectedItems.Count} contact has been successfully imported.";

                            MessageBox.Show(msg, "Import successfull", MessageBoxButton.OK, MessageBoxImage.Information);
                            SetDbContactsList();
                            SetFileText(txtDbFile, listBoxDBContacts.Items.Count, App.DbFilePath);
                        }
                        else
                        {
                            ShowError("Please select at least one item which should be imported.", "No items selected");
                        }
                    }
                    else
                    {
                        ShowError("Please open a vcf file.", "No vCard file opened");
                    }
                }
                else
                {
                    ShowError("Please open the Lonphone's friends.db file.", "No database selected");
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message, "Critical error");
            }
        }

        private void ShowError(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
