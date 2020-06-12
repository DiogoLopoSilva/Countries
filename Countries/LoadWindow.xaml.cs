using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace Countries
{
    using Modelos;
    using Servicos;

    /// <summary>
    /// Interaction logic for LoadWindow.xaml
    /// </summary>
    public partial class LoadWindow : Window
    {
        public List<Country> Countries { get; set; }
        public List<Rate> Rates { get; set; }
        public CovidData _CovidData { get; set; }
        public string LocalCountry { get; set; }
        public NetworkService networkService { get; }

        private readonly ApiService apiService;
        private DataService dataService;
        private readonly ProgressReport report;

        public LoadWindow()
        {
            InitializeComponent();

            apiService = new ApiService();
            networkService = new NetworkService();

            dataService = new DataService();
            report = new ProgressReport();

            Load();
        }

        private async void Load()
        {
            progressbar.Visibility = Visibility.Visible;
            btnRetry.Visibility = Visibility.Hidden;

            bool load;

            var connection = networkService.CheckConnection();

            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;

            if (!connection.IsSuccess)
            {
                await Task.Run(() => LoadLocalData(progress));
                load = false;
            }
            else
            {
                List<Task> tasks = new List<Task>();

                var watch = System.Diagnostics.Stopwatch.StartNew();

                await GetCountryByIp(progress);
                await LoadCovidData(progress);
                await LoadApiRates(progress);
                await LoadApiCountries(progress);

                progressbar.Maximum = Countries.Count + Rates.Count;

                await Task.Run(() => dataService.DeleteData());
                await Task.Run(() => dataService.SaveData(Countries, Rates, progress));

                progressbar.Maximum = Countries.Count * 2;

                await dataService.RunDownloadParallelAsync(Countries, progress);

                await Task.WhenAll(tasks);

                load = true;

                tbTime.Text = Environment.NewLine + "TOTAL: " + watch.ElapsedMilliseconds.ToString() + Environment.NewLine;
                watch.Stop();
            }

            if (Countries.Count == 0)
            {
                tbStatus.Text = "You must have an Internet connection the first time you run this Application";
                progressbar.Visibility = Visibility.Hidden;
                btnRetry.Visibility = Visibility.Visible;
                return;
            }

            if (load)
            {
                tbStatus.Text = $"Data loaded from the Internet on {DateTime.Now}";
            }
            else
            {
                tbStatus.Text = "Data loaded from the Database";
            }

            MainWindow mw = new MainWindow(this);
            mw.Show();
            Close();
        }

        private void ReportProgress(object sender, ProgressReport e)
        {
            progressbar.Value = e.PercentageCompleted;
            tbStatus.Text = e.Description;
        }

        private void LoadLocalData(IProgress<ProgressReport> progress)
        {
            Countries = dataService.GetDataCountries();

            Rates = dataService.GetDataRates();

            report.PercentageCompleted = 50;
            report.Description = "Getting Data from the Database";
            progress.Report(report);

            dataService.CheckImages(Countries);

            report.PercentageCompleted += 50;
            progress.Report(report);
        }

        private async Task LoadApiCountries(IProgress<ProgressReport> progress)
        {
            var response = await apiService.GetCountries("http://restcountries.eu", "/rest/v2/all");

            Countries = (List<Country>)response.Result;

            Country temp = new Country
            {
                name = "TESTE EMPTY FLAG",
                capital = "TESTE",
                currencies = new List<Currency>()
            };

            Countries.Add(temp);

            report.PercentageCompleted += 25;
            report.Description = "Loading APIs";
            progress.Report(report);
        }

        private async Task LoadCovidData(IProgress<ProgressReport> progress)
        {
            var response = await apiService.GetCovidData("https://api.covid19api.com", "/summary");

            _CovidData = (CovidData)response.Result;

            report.PercentageCompleted += 25;
            report.Description = "Loading APIs";
            progress.Report(report);
        }

        private async Task LoadApiRates(IProgress<ProgressReport> progress)
        {
            var response = await apiService.GetRates("https://cambiosrafa.azurewebsites.net", "/api/rates");

            Rates = (List<Rate>)response.Result;

            report.PercentageCompleted += 25;
            report.Description = "Loading APIs";
            progress.Report(report);
        }

        private async Task GetCountryByIp(IProgress<ProgressReport> progress)
        {
            var response = await apiService.GetCountryByIp("http://ip-api.com", "/json");

            LocalCountry = response.Result.ToString();

            report.PercentageCompleted += 25;
            report.Description = "Loading APIs";
            progress.Report(report);
        }

        private void btnRetry_Click(object sender, RoutedEventArgs e)
        {
            dataService = new DataService();
            Load();
        }
    }
}
