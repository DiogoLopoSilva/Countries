using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Countries
{
    using Modelos;
    using NReco.ImageGenerator;
    using Servicos;
    using Svg;
    using System.Drawing;
    using System.Net;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Country> Paises;
        private readonly ApiService apiService;
        private readonly NetworkService networkService;
        Stream mediaStream;

        public MainWindow()
        {
            InitializeComponent();
            apiService = new ApiService();
            networkService = new NetworkService();
            LoadRates();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private async Task LoadApiRates()
        {
            var response = await apiService.GetRates("http://restcountries.eu", "/rest/v2/all");

            Paises = (List<Country>)response.Result;
        }

        private async void LoadRates()
        {
            await LoadApiRates();
            await SaveImages();

            //cbPaises.ItemsSource = Paises;
            //cbPaises.DisplayMemberPath = "name";

            listBoxPaises.ItemsSource = Paises;
        }

        private async Task SaveImages()
        {
            DirectoryInfo Location = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent;

            string path = Location.FullName + "\\Images\\";

            WebClient webClient = new WebClient();

            HtmlToImageConverter image;

            Country temp = new Country
            {
                name = "TESTE EMPTY FLAG"
            };

            Paises.Add(temp);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (var pais in Paises) //Isto nao devia estar aqui
            {
                if (!File.Exists($"{path}{pais.name}.bmp") && !String.IsNullOrEmpty(pais.flag))
                {

                    try
                    {
                        webClient.DownloadFile(new Uri(pais.flag), $"{path}{pais.name}.svg");

                        //var svgDocument = SvgDocument.Open<SvgDocument>($"{path}{pais.name}.svg");
                        //svgDocument.ShapeRendering = SvgShapeRendering.OptimizeSpeed;
                        //var bitmap = svgDocument.Draw(100, 100); //tamanho 100px por 100px
                        //bitmap.Save($"{path}{pais.name}" + ".bmp");
                        //bitmap.Dispose();

                        var svgDoc = SvgDocument.Open<SvgDocument>($"{path}{pais.name}.svg");

                        image = new HtmlToImageConverter
                        {
                            Height = Convert.ToInt32(svgDoc.Height),
                            Width = Convert.ToInt32(svgDoc.Width)
                        };

                        image.GenerateImageFromFile($"{path}{pais.name}.svg", "bmp", $"{path}{pais.name}.bmp");

                        //var imageByte = image.GenerateImageFromFile($"{path}{pais.name}.svg", ImageFormat.Png);

                        //using (var stream = new MemoryStream(imageByte, 0, imageByte.Length))
                        //{
                        //    Bitmap bm = new Bitmap(Image.FromStream(stream));
                        //    bm.Save($"{path}{pais.name}" + ".png", System.Drawing.Imaging.ImageFormat.Png);
                        //}
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }


                    //Image imagem = svgDoc.Draw();

                    //imagem.Save(Path.Combine(path, $"{pais.name}.png"));

                    //imagem.Dispose();


                    //using (var document = new SVGDocument(Path.Combine(path, $"{pais.name}.svg")))
                    //{
                    //    using (var device = new ImageDevice(new ImageRenderingOptions(Aspose.Svg.Rendering.Image.ImageFormat.Jpeg), path + $"{pais.name}.png"))
                    //    {
                    //        document.RenderTo(device);
                    //    }
                    //}

                    //var a = svgDocument.Content;

                    //image = new HtmlToImageConverter();
                    //var imageByte = image.GenerateImageFromFile($"{path}{pais.name}.svg", ImageFormat.Png);

                    //var byteArray = Encoding.ASCII.GetBytes(a);
                    //using (var stream = new MemoryStream(byteArray))
                    //{
                    //    svgDocument = SvgDocument.Open<SvgDocument>(stream);
                    //    var bitmap = svgDocument.Draw();
                    //    bitmap.Save($"{path}{pais.name}" + ".png");
                    //}   

                    //using (var stream = new MemoryStream(imageByte, 0, imageByte.Length))
                    //{
                    //    Bitmap bm = new Bitmap(Image.FromStream(stream));
                    //    bm.Save($"{path}{pais.name}" + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    //}
                    File.Delete($"{path}{pais.name}.svg");
                }

                pais.caminhoImage = File.Exists($"{path}{pais.name}.bmp") ? path + pais.name + ".bmp" : Location.FullName+"\\Resources\\notavailable.png";
            }

            webClient.Dispose();
        }

        //private void cbPaises_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    Country pais = (Country)cbPaises.SelectedItem;
        //    tb1.Text = pais.nativeName;
        //}
        void DisposeMediaStream()
        {
            if (mediaStream != null)
            {
                mediaStream.Close();
                mediaStream.Dispose();
                mediaStream = null;
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            }
        }

        void Update(string path)
        {
            DisposeMediaStream();

            var bitmap = new BitmapImage();
            mediaStream = new FileStream(path, FileMode.Open);

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = mediaStream;
            bitmap.EndInit();

            bitmap.Freeze();
            imagebox1.Source = bitmap;
        }

        private void listBoxPaises_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Country pais = (Country)listBoxPaises.SelectedItem;

            if (pais != null)
            {
                tb1.Text = pais.name;

                if (File.Exists(pais.caminhoImage))
                {
                    //Update(pais.caminhoImage);

                    /////////////////////////////////////////////////////////
                    var bitmap = new BitmapImage();
                    var stream = File.OpenRead(pais.caminhoImage);

                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    stream.Close();
                    stream.Dispose();

                    bitmap.Freeze();
                    imagebox1.Source = bitmap;
                    /////////////////////////////////////////////////////////
                    //BitmapImage image = new BitmapImage();
                    //image.BeginInit();
                    //Uri imageSource = new Uri(pais.caminhoImage);
                    //image.UriSource = imageSource;
                    //image.EndInit();
                    //imagebox1.Source = image;
                    /////////////////////////////////////////////////////////
                    //imagebox1.Source = new BitmapImage(new Uri(pais.caminhoImage));
                }
            }
        }

        private void searchbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Paises != null)
            {
                IEnumerable<Country> lista = Paises.FindAll(x => x.name.ToLower().Contains(searchbox.Text.ToLower()));

                listBoxPaises.ItemsSource = lista;
            }
        }
    }
}
