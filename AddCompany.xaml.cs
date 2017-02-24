using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Threading;
using System.Configuration;
using System.Windows.Data;

namespace ENTERNAMESPACE
{
    /// <summary>
    /// Interaction logic for AddCompany.xaml
    /// </summary>
    public partial class AddCompany : Window
    {
        public AddCompany()
        {
            InitializeComponent();
            populateSqlServers();
        }

        private async void populateSqlServers()
        {
            //Running the SQLServer instance scan stuff in the background so the application doesn't become unresponsive.
            //Also allows for the sql loading screen progress bar to be updated in real-time
            TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            SqlLoading loadScreen = new SqlLoading();
            var progress = new Progress<int>(value => loadScreen.loadingProgress.Value = value);

            loadScreen.Show();

            await Task.Run(() =>
            {
                //Grabs list of all available sql servers on the network.
                MySqlHelper sqlHelper = new MySqlHelper();
                DataTable dataSources = sqlHelper.getSQLServers();
                this.Dispatcher.Invoke(() =>
                {
                    loadScreen.loadingProgress.Maximum = dataSources.Rows.Count - 1;

                    for (int i = 0; i < dataSources.Rows.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(dataSources.Rows[i]["InstanceName"].ToString()))
                            cmbServer.Items.Add(dataSources.Rows[i]["ServerName"] + "\\" + dataSources.Rows[i]["InstanceName"]);
                        else
                            cmbServer.Items.Add(dataSources.Rows[i]["ServerName"]);

                        loadScreen.loadingProgress.Dispatcher.Invoke(() => loadScreen.loadingProgress.Value = i, DispatcherPriority.Background);
                        Thread.Sleep(200);
                    }
                });
            });
            loadScreen.Close();
        }

        private void populateDatabases()
        {
            //Grabs all databases on selected server\instance.
            //SQL Database MUST contain "redacted_" to be listed.
            MySqlHelper sqlHelper = new MySqlHelper();
            DataTable databases = sqlHelper.getDatabases(cmbServer.SelectedValue.ToString());

            if (databases != null)
            {
                foreach (DataRow row in databases.Rows)
                {
                    if (row["database_name"].ToString().Contains("redacted_"))
                    {
                        cmbDatabase.Items.Add(row["database_name"]);
                    }
                }
            }
        }
        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            populateDatabases();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            ConnectionStringSettings conStringSettings = new ConnectionStringSettings();
            string connectionStringName = txtCompanyName.Text.ToString();

            //Using a string builder for ease of reading what's going on.
            StringBuilder connectionString = new StringBuilder();
            connectionString.Append("Data Source=");
            connectionString.Append(cmbServer.SelectedValue.ToString());
            connectionString.Append(";Initial Catalog=");
            connectionString.Append(cmbDatabase.SelectedValue.ToString());
            connectionString.Append(";Integrated Security=SSPI;");

            conStringSettings.Name = connectionStringName;
            conStringSettings.ConnectionString = connectionString.ToString();
            conStringSettings.ProviderName = "System.Data.sqlclient";

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            try
            {
                config.ConnectionStrings.ConnectionStrings.Add(conStringSettings);
                config.ConnectionStrings.SectionInformation.ForceSave = true;
                config.Save(ConfigurationSaveMode.Modified);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
	    //Not needed if there is no DataGid on the main window.
            MainWindow main = (MainWindow)Application.Current.MainWindow;
            main.updateDataGrid();
            CollectionViewSource.GetDefaultView(main.dataGrid.ItemsSource).Refresh();
            Close();
        }
    }
}
