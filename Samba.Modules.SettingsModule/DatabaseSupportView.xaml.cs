using Samba.Presentation.Controls.UIControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Samba.Modules.SettingsModule
{
    /// <summary>
    /// Interaction logic for DatabaseSupportView.xaml
    /// </summary>
    public partial class DatabaseSupportView
    {
        public DatabaseSupportView()
        {
            InitializeComponent();
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            DatabaseSupportViewModel model = new DatabaseSupportViewModel();
            model.do_db_backup();
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            DatabaseSupportViewModel model = new DatabaseSupportViewModel();
            model.do_db_restore();
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            string password = "";
            InputBox ib = new InputBox();
            if (ib.Show("Warning!", "This will completely clear all transactions and products in the database.\n" +
                "Database backup suggested before proceding.\n\nPease enter password to continue:", ref password, 1) == DialogResult.OK
                && password == "0824285054")
            {
                DatabaseSupportViewModel model = new DatabaseSupportViewModel();
                model.clear_database();
            }
            else
                System.Windows.MessageBox.Show("The password you have entered is incorrect.\nPlease try again.", "Important Note");
        }

        private void btnClearTranscations_Click(object sender, RoutedEventArgs e)
        {
            string password = "";
            InputBox ib = new InputBox();
            if (ib.Show("Warning!", "This will completely clear all transactions in the database.\n" + 
                "Database backup suggested before proceding.\n\nPease enter password to continue:", ref password, 1) == DialogResult.OK 
                && password == "0824285054")
            {
                DatabaseSupportViewModel model = new DatabaseSupportViewModel();
                model.clear_transactions();
            }
            else
                System.Windows.MessageBox.Show("The password you have entered is incorrect.\nPlease try again.", "Important Note");
        }

        private void btnClearProducts_Click(object sender, RoutedEventArgs e)
        {
            string password = "";
            InputBox ib = new InputBox();
            if (ib.Show("Warning!", "This will completely clear all products in the database.\n" +
                "Database backup suggested before proceding.\n\nPease enter password to continue:", ref password, 1) == DialogResult.OK
                && password == "0824285054")
            {
                DatabaseSupportViewModel model = new DatabaseSupportViewModel();
                model.clear_products();
            }
            else
                System.Windows.MessageBox.Show("The password you have entered is incorrect.\nPlease try again.", "Important Note");
        }

        private void btnBatchImport_Click(object sender, RoutedEventArgs e)
        {
            Batch_Import bi = new Batch_Import();
            bi.Show();
        }
    }
}
