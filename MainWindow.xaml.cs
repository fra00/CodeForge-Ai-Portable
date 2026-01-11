using Newtonsoft.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CodeForgePortable
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }


        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await webView.EnsureCoreWebView2Async(null);
            // Ottieni il percorso del file HTML
            //string htmlFilePath = "http://localhost:5173/";
            string htmlFilePath = "https://llm-codeforge.netlify.app/";
            webView.Source = new Uri(htmlFilePath);
            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
        }

        private async void CoreWebView2_WebMessageReceived(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            // Il messaggio arriva come stringa JSON
            string jsonDati = e.WebMessageAsJson;

            WebMessagePayload? webMessagePayload = Newtonsoft.Json.JsonConvert.DeserializeObject<WebMessagePayload>(jsonDati);

            if(webMessagePayload!=null)
            {
                ProjectProcessor process = new();
                var r = await process.ProcessPayload(webMessagePayload);
                SendToJavaScript(r);
            }
            
        }

        // Metodo helper per inviare messaggi
        private void SendToJavaScript(object data)
        {
            var message = new
            {
                result = data,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            var json = JsonConvert.SerializeObject(message);
            webView.CoreWebView2.PostWebMessageAsJson(json);
        }
    }
}