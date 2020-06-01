using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace Countries
{
    using Modelos;
    using Servicos;
    using Svg;
    using System.IO;
    using System.Net;
    /// <summary>
    /// Interaction logic for LoadWindow.xaml
    /// </summary>
    public partial class LoadWindow : Window
    {
        private List<Country> Paises;
        private List<Rate> Rates;
        private readonly ApiService apiService;
        private readonly NetworkService networkService;
        private readonly DirectoryInfo Location;
        private readonly DataService dataService;

        public LoadWindow()
        {
            InitializeComponent();
            apiService = new ApiService();
            networkService = new NetworkService();
            Location = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent;
            dataService = new DataService();
            Load();
        }

        private async void Load()
        {
            bool load;

            var connection = networkService.CheckConnection();

            bool test = true;

            //if(!test)
            if (!connection.IsSuccess)
            {
                LoadLocalData();
                load = false;
            }
            else
            {
                List<Task> tasks = new List<Task>();             

                var watch = System.Diagnostics.Stopwatch.StartNew();

                if (!Directory.Exists(Location.FullName + "\\Images\\"))
                {
                    Directory.CreateDirectory(Location.FullName + "\\Images\\");
                }

                if (!Directory.Exists(Location.FullName + "\\Images\\Thumbnails\\"))
                {
                    Directory.CreateDirectory(Location.FullName + "\\Images\\Thumbnails\\");
                }

                //tblockStatus.Text = "Loading APIs";
                //tbTime.Text = "---------Time---------";
                tasks.Add(LoadApiRates());
                await LoadApiCountries();

                //tbTime.Text += Environment.NewLine + "Load Time: " + watch.ElapsedMilliseconds.ToString();
                //var time = watch.ElapsedMilliseconds;

                //tblockStatus.Text = "Updating DB";
                await Task.Run(() => dataService.DeleteData());
                tasks.Add(Task.Run(() => dataService.SaveData(Paises)));

                //tbTime.Text += Environment.NewLine + "Updating Time: " + watch.ElapsedMilliseconds.ToString();
                //time = watch.ElapsedMilliseconds;

                //tblockStatus.Text = "Fetching Flags";
                tasks.Add(RunDownloadParallelAsync());

                //tbTime.Text += Environment.NewLine + "Download Time: " + (watch.ElapsedMilliseconds - time).ToString();
                //time = watch.ElapsedMilliseconds;

                await Task.WhenAll(tasks);

                load = true;

                //tblockStatus.Text = "Done";
                tbTime.Text = Environment.NewLine + "TOTAL: " + watch.ElapsedMilliseconds.ToString()+ Environment.NewLine;
                watch.Stop();               
            }

            if (Paises.Count == 0)
            {
                tbTime.Text += "Não há ligação à Internet" + Environment.NewLine +
                    "e não foram previamente carregadas as taxas." + Environment.NewLine +
                    "Tente mais tarde!";

                tblockStatus.Text += "Primeira inicialização deverá ter ligação à Internet";

                return;
            }

            if (load)
            {
                tblockStatus.Text = $"Taxas carregadas da internet em {DateTime.Now}";
            }
            else
            {
                tblockStatus.Text = "Taxas carregadas da Base de Dados.";
            }

            btnLoad.Visibility = Visibility.Visible;

            //MainWindow mw = new MainWindow(Paises);
            //mw.Show();
            //Close();
        }

        private void LoadLocalData()
        {
            Paises = dataService.GetData();
        }

        private async Task LoadApiCountries()
        {
            progressbar.Value = 0;

            var response = await apiService.GetCountries("http://restcountries.eu", "/rest/v2/all");

            Paises = (List<Country>)response.Result;

            Country temp = new Country
            {
                name = "TESTE EMPTY FLAG",
                capital = "TESTE",
                currencies = new List<Currency>()
            };

            Paises.Add(temp);
        }
        private async Task LoadApiRates()
        {
            var response = await apiService.GetRates("https://cambiosrafa.azurewebsites.net", "/api/rates");

            Rates = (List<Rate>)response.Result;
        }

        private async Task RunDownloadParallelAsync()
        {
            List<Task> tasks = new List<Task>();

            foreach (Country pais in Paises)
            {
                if (!File.Exists($"{Location.FullName}\\Images\\{pais.name}.svg"))
                {
                    tasks.Add(Task.Run(() => DownloadSVG(Location, pais)));
                }
                else
                {
                    pais.caminhoImage = $"{Location.FullName}\\Images\\{pais.name}.svg";
                }

                if (!File.Exists($"{Location.FullName}\\Images\\Thumbnails\\{pais.name}.png"))
                {
                    tasks.Add(Task.Run(() => DownloadThumbnail(Location, pais)));
                }
                else
                {
                    pais.caminhoThumbnail = $"{Location.FullName}\\Images\\Thumbnails\\{pais.name}.png";
                }
            }

            await Task.WhenAll(tasks);
        }

        private void DownloadSVG(DirectoryInfo Location, Country pais)
        {
            string path = Location.FullName + "\\Images\\";

            using (WebClient webClient= new WebClient())
            {
                try
                {
                    webClient.DownloadFile(new Uri(pais.flag), $"{path}{pais.name}.svg");

                    pais.caminhoImage = path + pais.name + ".svg";
                }
                catch
                {
                    pais.caminhoImage = Location.FullName + "\\Resources\\notavailable.svg";
                }
            }
        }

        private void DownloadThumbnail(DirectoryInfo Location, Country pais)
        {
            string path = Location.FullName + "\\Images\\"+"\\Thumbnails\\";

            using (WebClient webClient = new WebClient())
            {
                try
                {
                    webClient.DownloadFile(new Uri("https://www.countryflags.io/" + $"{pais.alpha2Code}" + "/shiny/64.png"), $"{path}{pais.name}.png");

                    pais.caminhoThumbnail = path + pais.name + ".png";
                }
                catch
                {


                }
            }
        }

        private async Task ConvertAsync()
        {
            string path = Location.FullName + "\\Images\\";

            foreach (Country pais in Paises)
            {
                if (File.Exists($"{Location.FullName}\\Images\\{pais.name}.svg"))
                {
                    await Task.Run(() => ConvertSVG(Location, pais));
                }

                pais.caminhoImage = File.Exists($"{path}{pais.name}.bmp") ? path + pais.name + ".bmp" : Location.FullName + "\\Resources\\notavailable.png";
                pais.caminhoThumbnail = File.Exists($"{path}\\Thumbnails\\{pais.name}.png") ? path + "\\Thumbnails\\" + pais.name + ".png" : Location.FullName + "\\Resources\\notavailable.png";
            }
        }

        private void ConvertSVG(DirectoryInfo Location, Country pais)
        {
            //HtmlToImageConverter image;

            string path = Location.FullName + "\\Images\\";

            //var svgDoc = SvgDocument.Open<SvgDocument>($"{path}{pais.name}.svg");

            //image = new HtmlToImageConverter
            //{
            //    Height = Convert.ToInt32(svgDoc.Height),
            //    Width = Convert.ToInt32(svgDoc.Width)
            //};

            //image.GenerateImageFromFile($"{path}{pais.name}.svg", "bmp", $"{path}{pais.name}.bmp");

            //File.Delete($"{path}{pais.name}.svg");
            //pais.caminhoImage = File.Exists($"{path}{pais.name}.bmp") ? path + pais.name + ".bmp" : Location.FullName + "\\Resources\\notavailable.png";

            try
            {
                var svgDocument = SvgDocument.Open<SvgDocument>($"{path}{pais.name}.svg");
                svgDocument.ShapeRendering = SvgShapeRendering.Auto;
                //var bitmap = svgDocument.Draw();
                var bitmap = svgDocument.Draw(Convert.ToInt32(svgDocument.Width.Value * 0.3), Convert.ToInt32(svgDocument.Height.Value * 0.3)); //tamanho 100px por 100px
                bitmap.Save($"{path}{pais.name}" + ".bmp");
                bitmap.Dispose();

                File.Delete($"{path}{pais.name}.svg");
            }
            catch
            {

            }

        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var connection = networkService.CheckConnection();

            MainWindow mw = new MainWindow(Paises,Rates,connection.IsSuccess);
            mw.Show();
            Close();
        }
    }
}
