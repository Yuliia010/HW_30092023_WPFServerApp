using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Transactions;
using System.IO;

namespace HW_30092023_WPFServerApp.Net
{
    public class NetworkConnection
    {
        private TcpListener tcpListener;
        private TcpClient tcpClient;
        private clientLogs clientlog;
        private bool isWork = false;
        private List<clientLogs> clientsLogs = new List<clientLogs>();

        private string _login = "admin";
        private string _password = "admin";

        private string login = "";
        private string password = "";

        private bool successLogin = false;

        private int activeConnectionsCount = 0;
        public class clientLogs
        {
            public string? WhoConnected { get; set; }
            public DateTime WhenConnected { get; set; }
            public List<string> Quotation { get; set; }
            public DateTime WhenDisconnected { get; set; }
        }

        private List<string> quotations = new List<string>
        {
            "Справжня краса в серці.",
            "Життя - це не очікування на бурю, а танець під дощем.",
            "Мрія - це перша сторінка книги, а реальність - це той же роман, який треба писати власноруч.",
            "Сьогодні - подарунок, тому відкривайте його з радістю.",
            "Життя надається раз, і ти повинен його прожити, як тобі подобається.",
            "Життя - це те, що з тобою відбувається, поки ти зайнятий іншими планами.",
            "Справжня велич - це величина душі.",
            "Думайте про завтра, але живіть сьогодні.",
            "Кращий спосіб впередитися - це рухатися.",
            "Найкращий час для початку - це зараз."
        };

       public bool isStart()
        {
            return isWork;
        }
       
        public int GetActiveConnectionsCount()
        {
            return activeConnectionsCount;
        }
        public async Task GetLogin()
        {
            var stream = tcpClient.GetStream();
            var responseData = new byte[8192];

            var response = new StringBuilder();
            int bytes;
            do
            {
                bytes = await stream.ReadAsync(responseData);
                response.Append(Encoding.UTF8.GetString(responseData, 0, bytes));
            }
            while (stream.DataAvailable);

            login = response.ToString();

        }
        public async Task GetPass()
        {
            var stream = tcpClient.GetStream();
            var responseData = new byte[8192];
            var response = new StringBuilder();
            int bytes;
            do
            {
                bytes = await stream.ReadAsync(responseData);
                response.Append(Encoding.UTF8.GetString(responseData, 0, bytes));
            }
            while (stream.DataAvailable);

            password = response.ToString();
        }
        public async Task StartServer()
        {
            if (!isWork)
            {
                tcpListener = new TcpListener(IPAddress.Any, 8080);

                try
                {
                    tcpListener.Start();
                    isWork = true;
                    while (true)
                    {
                        tcpClient = await tcpListener.AcceptTcpClientAsync();

                        await GetLogin();
                        if (_login == login)
                        {
                            string overloadMessage = "Ok";
                            byte[] overloadData = Encoding.UTF8.GetBytes(overloadMessage);
                            await tcpClient.GetStream().WriteAsync(overloadData);
                        }
                        else
                        {
                            string overloadMessage = "Reject";
                            byte[] overloadData = Encoding.UTF8.GetBytes(overloadMessage);
                            await tcpClient.GetStream().WriteAsync(overloadData);
                            tcpClient.Close();
                            continue; 
                        }

                        await GetPass();

                        if (_password == password)
                        {
                            string overloadMessage = "Ok";
                            byte[] overloadData = Encoding.UTF8.GetBytes(overloadMessage);
                            await tcpClient.GetStream().WriteAsync(overloadData);

                            if (tcpClient.Connected)
                            {
                                activeConnectionsCount++;
                                Task.Run(async () => await ProcessClientAsync());
                            }
                        }
                        else
                        {
                            string overloadMessage = "Reject";
                            byte[] overloadData = Encoding.UTF8.GetBytes(overloadMessage);
                            await tcpClient.GetStream().WriteAsync(overloadData);
                            tcpClient.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                tcpListener.Stop();
                isWork = false;
            }
        }

        public List<clientLogs> GetLogs()
        {
            return clientsLogs;
        }

        private async Task ProcessClientAsync()
        {
            var stream = tcpClient.GetStream();

            var response = new List<byte>();
            int bytesRead;
            string message = "";

            while (true)
            {
                clientlog = new clientLogs(); 
                clientlog.WhenConnected = DateTime.Now;
                clientlog.WhoConnected = $"{tcpClient.Client.RemoteEndPoint}";
                clientlog.Quotation = new List<string>();
                while ((bytesRead = stream.ReadByte()) != -1)
                {
                    response.Add((byte)bytesRead);
                    if (bytesRead == '\n')
                    {
                        var request = Encoding.UTF8.GetString(response.ToArray());
                        if (request.Trim() == "GET")
                        {
                            Random random = new Random();
                            int randomIndex = random.Next(quotations.Count);
                            message = quotations[randomIndex] + "\n";
                            clientlog.Quotation.Add(message);
                            byte[] data = Encoding.UTF8.GetBytes(message);
                            await stream.WriteAsync(data);
                        }
                        response.Clear();
                    }
                }

                clientlog.WhenDisconnected = DateTime.Now;
                clientsLogs.Add(clientlog);
                activeConnectionsCount--;
                tcpClient.Close();
                return;
            }
        }


    }
}