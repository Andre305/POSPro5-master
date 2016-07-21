using Samba.Infrastructure.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Samba.Modules.SettingsModule
{
    public partial class Batch_Import : Form
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        bool InventoryTransDoc = true;
        string invTransDocID = "", invID = "";

        public Batch_Import()
        {
            InitializeComponent();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (lblFile.Text == "No File Selected...")
                return;

            btnImport.Enabled = false;
            setTextAndColor("Calculating number of records..." + Environment.NewLine + Environment.NewLine, Color.Black);
            pbProgress.Minimum = 0;
            pbProgress.Maximum = File.ReadLines(lblFile.Text).Count();
            pbProgress.Step = 1;

            using (StreamReader sr = new StreamReader(lblFile.Text))
            {
                execute_query(query_WorkPeriod());
                string currentLine;
                while ((currentLine = sr.ReadLine()) != null)
                {
                    set_line_data(currentLine);
                    insert_line_data();
                    pbProgress.Value += 1;
                }
                setTextAndColor(Environment.NewLine + "DONE!" + Environment.NewLine, Color.Green);
                data.Clear();
            }
        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            string filePath = ask_select_file();
            if (string.IsNullOrEmpty(filePath))
            {
                lblFile.Text = "No File Selected...";
                return;
            }

            lblFile.Text = filePath;
        }

        private string ask_select_file()
        {
            string file = "";
            OpenFileDialog openFileDialog1;
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); ;
            openFileDialog1.Filter = "CSV files (*.CSV)|*.csv";
            openFileDialog1.RestoreDirectory = true;
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                file = openFileDialog1.FileName;
            }
            return file;
        }

        private void set_line_data(string line)
        {
            try
            {
                string[] columns = line.Split(',');
                if (data.Count == 0)
                {
                    foreach (string column in columns)
                    {
                        data.Add(column, "");
                    }
                    return;
                }

                for (int i = 0; i < columns.Length; i++)
                {
                    string key = data.Keys.ElementAt(i);
                    data[key] = columns[i];
                }
            }
            catch
            {
                setTextAndColor("INVALID CSV LINE! Record not inserted!" + Environment.NewLine, Color.Red);
            }
        }

        private void insert_line_data()
        {
            if (pbProgress.Value == 0)
                return;

            if(data.ContainsKey("ProductName") && 
                data.ContainsKey("GroupCode") && 
                data.ContainsKey("Barcode"))
            {
                string response = execute_query(query_MenuItems()); 
                setTextAndColor("Menu Item " + data["ProductName"] + ": " + response
                    + Environment.NewLine, response == "Record created successfully." ? Color.Green : Color.Red);
            }

            string id = "";
            if (data.ContainsKey("PortionName") &&
                data.ContainsKey("PortionMultiplier"))
            {
                id = get_scalar_value(query_MenuItemPortions());
                setTextAndColor("Menu Portion " + data["ProductName"] + ": " + 
                    (id == "Error" ? "AN ERROR OCCURRED" : "Record inserted successfully.") + Environment.NewLine,
                        id != "Error" ? Color.Green : Color.Red);
            }

            if(!string.IsNullOrEmpty(id) && 
                !id.Equals("Error") && 
                data.ContainsKey("SellPrice"))
            {
                string response = execute_query(query_MenuItemPrices(id));
                setTextAndColor("Price for " + data["ProductName"] + ": " + response + Environment.NewLine,
                    response == "Record created successfully." ? Color.Green : Color.Red);
            }

            if(data.ContainsKey("CreateInventory") && data["CreateInventory"].Equals("Y"))
            {
                invID = get_scalar_value(query_InventoryItems());
                if (invID == "Error")
                {
                    setTextAndColor("Inventory Item, " + data["InventoryName"] + " failed to load." + Environment.NewLine, Color.Red);
                    return;
                }
                setTextAndColor("Inventory Item " + data["InventoryName"] + " created successfully." + Environment.NewLine, Color.Green);

                if(data.ContainsKey("InvQty") && data["InvQty"] != "0")
                {
                    if (InventoryTransDoc)
                    {
                        invTransDocID = get_scalar_value(query_InventoryTransactionDocuments());
                        InventoryTransDoc = false;
                    }

                    string query = query_InventoryTransactions();
                    if (query == "No Warehouse")
                        setTextAndColor("Warehouse does not exist: " + data["WarehouseName"] + ", Cannot insert into [InventoryTransactions] without a valid Warehouse." + Environment.NewLine, Color.Red);
                    else
                    {
                        string response = execute_query(query);
                        setTextAndColor("Inventory Transaction: " + response + Environment.NewLine, response == "Recored created successfully." ? Color.Green : Color.Red);
                    }
                }
            }
        }

        private string query_MenuItems()
        {
            return "  IF NOT EXISTS (SELECT TOP 1 1 FROM [MenuItems] WHERE [GroupCode] = '" + data["GroupCode"] + "'" +
                                            " AND [Barcode] = '" + data["Barcode"] + "'" +
                                            " AND [Name] = '" + data["ProductName"] + "') " +
                                "INSERT INTO [MenuItems] (" +
                                    "[GroupCode]" +
                                    ",[Barcode]" +
                                    ",[Name]" +
                                    ",[Tag]" +
                                    ") VALUES (" +
                                    "'" + data["GroupCode"] + "'," +
                                    "'" + data["Barcode"] + "'," +
                                    "'" + data["ProductName"] + "'," +
                                    "null); ";
        }

        private string query_MenuItemPortions()
        {
            string id = get_scalar_value("SELECT CONVERT(NVARCHAR(255), max([Id])) FROM [MenuItems] WHERE [GroupCode] = '" + data["GroupCode"] + "'" +
                                            " AND [Barcode] = '" + data["Barcode"] + "'" +
                                            " AND [Name] = '" + data["ProductName"] + "'");

            return "  INSERT INTO [MenuItemPortions] ( " +
                                        "[Name]" +
                                       ",[MenuItemId]" +
                                       ",[Multiplier]" +
                                       ") VALUES(  " +
                                       "'" + data["PortionName"] + "'" +
                                       "," + id +
                                       ",'" + data["PortionMultiplier"] + "'); SELECT COALESCE(CONVERT(NVARCHAR(255), SCOPE_IDENTITY()), 'Error');  ";
        }

        private string query_MenuItemPrices(string id)
        {
            return " INSERT INTO [MenuItemPrices] ( " +
                                        "[MenuItemPortionId]" +
                                       ",[Price]" +
                                       ",[PriceTag]" +
                                       ") VALUES(" +
                                       "'" + id + "'" +
                                       ",'" + data["SellPrice"] + "'" +
                                       ",null ); ";
        }

        private string query_InventoryItems()
        {
            return " INSERT INTO [InventoryItems] (" +
                         "[GroupCode]" +
                        ",[BaseUnit]" +
                        ",[TransactionUnit]" +
                        ",[TransactionUnitMultiplier]" +
                        ",[Name]" +
                        ",[Warehouse]" +
                        ") VALUES(" +
                        "'" + data["GroupCode"] + "'" +
                        ",'" + data["BaseUnit"] + "'" +
                        ",'" + data["TransactionUnit"] + "'" +
                        ",'" + data["TransactionMultiplier"] + "'" +
                        ",'" + data["InventoryName"] + "'" +
                        ",'" + data["WarehouseName"] + "'" +
                        "); SELECT COALESCE(CONVERT(NVARCHAR(255), SCOPE_IDENTITY()), 'Error'); ";
        }

        private string query_InventoryTransactionDocuments()
        {
            return " INSERT INTO [InventoryTransactionDocuments] ( " +
                                 "[Date]" +
                                ",[Name]" +
                                ") VALUES(" +
                                "GETDATE()" +
                                ",'Opening Stock'" +
                                "); SELECT COALESCE(CONVERT(NVARCHAR(255), SCOPE_IDENTITY()), 'Error'); ";
        }

        private string query_InventoryTransactions()
        {
            string warehouse_ID = get_scalar_value("SELECT [Id] FROM [Warehouses] where [Name] = '" + data["WarehouseName"] + "'");

            if (warehouse_ID == "Error")
                return "No Warehouse";

            return "INSERT INTO [InventoryTransactions] ( " +
                             "[InventoryTransactionDocumentId]" +
                            ",[InventoryTransactionTypeId]" +
                            ",[SourceWarehouseId]" +
                            ",[TargetWarehouseId]" +
                            ",[Date]" +
                            ",[Unit]" +
                            ",[Multiplier]" +
                            ",[Quantity]" +
                            ",[TotalPrice]" +
                            ",[InventoryItem_Id]" +
                            ") VALUES(" +
                            "'" + invTransDocID + "'" +
                            ",'1'" +
                            ",'0'" +
                            ",'" + warehouse_ID + "'" +
                            ",GETDATE()" +
                            ",'" + data["TransUnit"] + "'" +
                            ",'" + data["TransMulti"] + "'" +
                            ",'" + data["InvQty"] + "'" +
                            ",'" + data["CostPrice"] + "'" +
                            ",'" + invID + "'" +
                            ")";
        }

        private string query_WorkPeriod()
        {
            return "IF NOT EXISTS (SELECT [Id] FROM [WorkPeriods] WHERE [StartDate]=[EndDate]) " +
                "INSERT INTO [WorkPeriods] ([StartDate],[EndDate]) VALUES (GETDATE(),GETDATE())";
        }

        private string execute_query(string query)
        {
            string status = "Record created successfully.";
            using (var connection = new SqlConnection(
                LocalSettings.ConnectionString.EndsWith(";") ?
                LocalSettings.ConnectionString + "Database=" + LocalSettings.AppName + ";" :
                LocalSettings.ConnectionString + "; Database=" + LocalSettings.AppName + ";")
            )
            {
                using (var command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException sqlE)
                    {
                        status = sqlE.Message;
                    }
                }
            }
            return status;
        }

        public string get_scalar_value(string query)
        {
            string variable = "";
            using (var connection = new SqlConnection(
                LocalSettings.ConnectionString.EndsWith(";") ?
                LocalSettings.ConnectionString + "Database=" + LocalSettings.AppName + ";" :
                LocalSettings.ConnectionString + "; Database=" + LocalSettings.AppName + ";")
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
                        txtStatus.AppendText(sqlE.Message + Environment.NewLine);
                    }
                }
            }
            return variable;
        }

        private void setTextAndColor(string text, Color color)
        {
            int length = txtStatus.TextLength; 
            txtStatus.AppendText(text);
            txtStatus.SelectionStart = length;
            txtStatus.SelectionLength = text.Length;
            txtStatus.SelectionColor = color;
        }
    }
}
