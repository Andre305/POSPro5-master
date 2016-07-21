using Microsoft.Win32;
using Samba.Infrastructure.Data.SQL;
using Samba.Infrastructure.Settings;
using Samba.Persistance.Data;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using Samba.Presentation.Controls.UIControls;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;

namespace Samba.Modules.SettingsModule
{
    class DatabaseSupportViewModel
    {
        private static string backup_path;

        public DatabaseSupportViewModel()
        {
            backup_path = get_scalar_value(get_query("backupPath", "")) + "\\";
        }

        public void do_db_backup()
        {
            string name = ask_file_name(".bak");
            if (name != "")
            {
                ProcessingBox.show_Processing("Creating backup...\nPlease Wait...");
                run_background_process("backup", name);
            }
        }

        public void do_db_restore()
        {
            string name = ask_select_file();
            if (name != "")
            {
                string newPath = backup_path + Path.GetFileName(name);
                ProcessingBox.show_Processing("Restoring Database...\nPlease Wait...");

                if(name != newPath)
                    File.Copy(name, backup_path + Path.GetFileName(name), true);

                run_background_process("restore", newPath);
            } 
        }

        public void clear_products()
        {
            ProcessingBox.show_Processing("Clearing Products...\nPlease Wait...");
            run_background_process("clearProducts", "", LocalSettings.AppName);
        }

        public void clear_transactions()
        {
            ProcessingBox.show_Processing("Clearing Transactions...\nPlease Wait...");
            run_background_process("clearTransactions", "", LocalSettings.AppName);
        }

        public void clear_database()
        {
            ProcessingBox.show_Processing("Clearing Database...\nPlease Wait...");
            run_background_process("clearDatabase", "", LocalSettings.AppName);
        }

        private string ask_file_name(string extenstion)
        {
            string filename = string.Format("POS_PRO_BACKUP-{0:yyyy-MM-dd_HH-mm-ss}" + extenstion, DateTime.Now);

            InputBox ib = new InputBox();
            if(ib.Show("Backup Filename", "Please enter a filename for the backup:", ref filename) == DialogResult.OK)
            {
                return filename;
            }

            return "";
        }

        private string ask_select_file()
        {
            string file = "";
            System.Windows.Forms.OpenFileDialog openFileDialog1;
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.InitialDirectory = backup_path;
            openFileDialog1.Filter = "BAK files (*.BAK)|*.bak";
            openFileDialog1.RestoreDirectory = true;
            DialogResult result = openFileDialog1.ShowDialog(); 
            if (result == DialogResult.OK) 
            {
                file = openFileDialog1.FileName;
            }
            return file;
        }

        private bool execute_non_query(string type, string filename, string databaseName)
        {
            bool success = true;
            
            SqlConnection dataConnection = new SqlConnection(
                LocalSettings.ConnectionString.EndsWith(";") ?
                LocalSettings.ConnectionString + "Database=" + databaseName + ";" :
                LocalSettings.ConnectionString + "; Database=" + databaseName + ";");
            SqlCommand cmd = new SqlCommand(get_query(type, filename));

            try
            {
                cmd.Connection = dataConnection;
                dataConnection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (SqlException sqlE)
            {
                System.Windows.MessageBox.Show(sqlE.Message);
                success = false;
            }
            finally
            {
                dataConnection.Close();
            }
            return success;
        }

        public string get_scalar_value(string query)
        {
            string variable = "";
            using (var connection = new SqlConnection(
                LocalSettings.ConnectionString.EndsWith(";") ? 
                LocalSettings.ConnectionString + "Database=master;" : 
                LocalSettings.ConnectionString + "; Database=master;")
            )
            {
                using (var command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        variable = (string)command.ExecuteScalar();
                    }
                    catch (SqlException sqlE)
                    {
                        System.Windows.MessageBox.Show(sqlE.Message);
                    }
                }
            }
            return variable;
        }

        private void run_background_process(string type, string name, string databaseName = "master")
        {
            QueryInfo qi = new QueryInfo();
            qi.type = type;
            qi.name = name;
            qi.dbName = databaseName;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync(qi);
        }
        
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            QueryInfo o = (QueryInfo)e.Argument;
            e.Result = execute_non_query(o.type, o.name, o.dbName);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProcessingBox.hide_Processing();

            if ((bool)e.Result)
            {
                System.Windows.MessageBox.Show("Operation Successful!\nPOS Pro 5 application will restart to apply database changes.", "Success");
                System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
                System.Windows.Application.Current.Shutdown();
            }
            else
                System.Windows.MessageBox.Show("An Unexpected Error Occured! Please try again.");
        }

        private string get_query(string type, string filename)
        {
            StringBuilder sb = new StringBuilder();

            if(type == "clearDatabase")
            {
                sb.Append(get_query("clearProducts", ""));
                sb.Append(get_query("clearTransactions", ""));
            }
            else if(type == "backup")
            {
                sb.Append("DECLARE @fileName VARCHAR(max);");
                sb.Append("DECLARE @db_name VARCHAR(max);");

                sb.Append("SET @fileName = '" + backup_path + filename + "'; ");
                sb.Append("SET @db_name = '" + LocalSettings.AppName + "';");

                sb.Append("BACKUP DATABASE @db_name TO DISK = @fileName; ");
            }
            else if(type == "restore")
            {
                sb.Append("ALTER DATABASE [" + LocalSettings.AppName + "]");
                sb.Append("SET OFFLINE WITH ROLLBACK IMMEDIATE;");

                sb.Append("DECLARE @MDF_File_Location NVARCHAR(MAX),");
                sb.Append("@Log_File_Location NVARCHAR(MAX);");

                sb.Append("SELECT @MDF_File_Location = physical_name FROM sys.master_files WHERE name = '" + LocalSettings.AppName + "';");
                sb.Append("SELECT @Log_File_Location = physical_name FROM sys.master_files WHERE name = '" + LocalSettings.AppName + "_log';");

                sb.Append("RESTORE DATABASE [" + LocalSettings.AppName + "] FROM DISK = '" + filename + "'");
                sb.Append("WITH MOVE '" + LocalSettings.AppName + "' TO @MDF_File_Location,");
                sb.Append("MOVE '" + LocalSettings.AppName + "_log' TO @Log_File_Location,");
                sb.Append("REPLACE,");
                sb.Append("STATS = 10;");

                sb.Append("ALTER DATABASE [" + LocalSettings.AppName + "]");
                sb.Append("SET ONLINE WITH ROLLBACK IMMEDIATE;");
            }
            else if (type == "backupPath")
            {
                sb.Append("DECLARE @path NVARCHAR(4000);");
                sb.Append("EXEC master.dbo.xp_instance_regread ");
                sb.Append("N'HKEY_LOCAL_MACHINE', ");
                sb.Append("N'Software\\Microsoft\\MSSQLServer\\MSSQLServer',N'BackupDirectory', ");
                sb.Append("@path OUTPUT, ");
                sb.Append("'no_output'; ");
                sb.Append("SELECT @path; ");
            }
            else if (type == "clearProducts")
            {
                sb.Append("DELETE FROM [InventoryTransactionDocuments]; ");
                sb.Append("DELETE FROM [InventoryTransactions]; ");
                sb.Append("DELETE FROM [WarehouseConsumptions]; ");
                sb.Append("DELETE FROM [PeriodicConsumptionItems]; ");
                sb.Append("DELETE FROM [PeriodicConsumptions]; ");
                sb.Append("DELETE FROM [RecipeItems]; ");
                sb.Append("DELETE FROM [Recipes]; ");
                sb.Append("DELETE FROM [InventoryItems]; ");
                sb.Append("DELETE FROM [MenuItems]; ");
                sb.Append("DELETE FROM [MenuItemPrices]; ");
                sb.Append("DELETE FROM [MenuItemPriceDefinitions]; ");
                sb.Append("DELETE FROM [MenuItemPortions]; ");
                sb.Append("DELETE FROM [ScreenMenuItems]; ");
                sb.Append("DELETE FROM [ScreenMenuCategories]; ");
            }
            else if (type == "clearTransactions")
            {
                sb.Append("DELETE FROM [TicketEntities]; ");
                sb.Append("DELETE FROM [Tickets]; ");
                sb.Append("DELETE FROM [AccountTransactionDocuments]; ");
                sb.Append("DELETE FROM [AccountTransactions]; ");
                sb.Append("DELETE FROM [AccountTransactionValues]; ");
                sb.Append("DELETE FROM [Calculations]; ");
                sb.Append("DELETE FROM [CostItems]; ");
                sb.Append("DELETE FROM [InventoryTransactionDocuments]; ");
                sb.Append("DELETE FROM [InventoryTransactions]; ");
                sb.Append("DELETE FROM [Orders]; ");
                sb.Append("DELETE FROM [PaidItems]; ");
                sb.Append("DELETE FROM [PeriodicConsumptionItems]; ");
                sb.Append("DELETE FROM [PeriodicConsumptions]; ");
                sb.Append("DELETE FROM [ProductTimerValues]; ");
                sb.Append("DELETE FROM [Payments]; ");
                sb.Append("DELETE FROM [WarehouseConsumptions]; ");
                sb.Append("DELETE FROM [WorkPeriods]; ");

                sb.Append("UPDATE [Numerators] SET [Number] = 0; ");

                sb.Append("UPDATE [EntityStateValues] SET ");
                sb.Append("[EntityStates] = '[{\"S\":\"Available\",\"SN\":\"Status\"}]' ");
                sb.Append("WHERE[EntityStates] like '%Status%' ; ");
            }

            return sb.ToString();
        }
    }

    class QueryInfo
    {
        public string name { get; set; }
        public string type { get; set; }
        public string dbName { get; set; }
    }
}
