using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Transactions;

namespace HW_30092023_WPFServerApp.Net
{
    public class NetworkConnection
    {
        private TcpListener tcpListener;
        private TcpClient tcpClient;
        clientLogs clientlog;
        private bool isWork = false;
        List<clientLogs> clientsLogs = new List<clientLogs>();

        private int activeConnectionsCount = 0;
        public class clientLogs
        {
            public string? WhoConnected { get; set; }
            public DateTime WhenConnected { get; set; }
            public string? Quotation { get; set; }
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

        public async Task StartServer()
        {
            if(!isWork)
            {
                tcpListener = new TcpListener(IPAddress.Any, 8080);

                try
                {
                    tcpListener.Start();
                    isWork = true;
                    while (true)
                    {
                        tcpClient = await tcpListener.AcceptTcpClientAsync();
                        activeConnectionsCount++;
                        Task.Run(async () => await ProcessClientAsync());

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
                isWork= false;
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
                clientlog = new clientLogs(); // Создаем новый объект для каждого клиента
                clientlog.WhenConnected = DateTime.Now;
                clientlog.WhoConnected = $"{tcpClient.Client.RemoteEndPoint}";

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

                            byte[] data = Encoding.UTF8.GetBytes(message);
                            await stream.WriteAsync(data);
                        }
                        response.Clear();
                    }
                }

                clientlog.WhenDisconnected = DateTime.Now;
                clientlog.Quotation = message;
                clientsLogs.Add(clientlog);
                activeConnectionsCount--;
                tcpClient.Close();
                return;
            }
        }


    }
}