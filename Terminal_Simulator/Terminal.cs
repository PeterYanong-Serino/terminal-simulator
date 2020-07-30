using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static Terminal_Simulator.Utilities;

namespace Terminal_Simulator {
    public class Terminal {
        private string _ipAddress;
        private int _port;
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private Thread thRetryConn;
        private byte[] _buffer;

        private TerminalData tData;

        public Terminal() {

            // Load data from text file.
            string file = "data.txt";
            tData = new TerminalData(file);

            // Initialize Client
            _tcpClient = new TcpClient();
            thRetryConn = new Thread(RetryConnectToServer);
        }

        public TcpClient client { get { return _tcpClient; } }

        public void GetDetails() {
            try {
                Console.Write("IP Address: ");
                _ipAddress = Console.ReadLine();
                Console.Write("Port: ");
                while (!int.TryParse(Console.ReadLine(), out _port)) {
                    Console.Clear();
                    log("INFO: Port should be number only.");
                    log();
                    Console.WriteLine("IP Address: " + _ipAddress);
                    Console.Write("Port: ");
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        #region Connection Region

        public void ConnectToServer() {

            // Established for Open
            // CloseWait for Close

            try {
                //** Here you can hard coded the connection you want if you're tired of setting it up. 
                //_ipAddress = "192.168.100.21";
                //_port = 18101;
                _tcpClient = new TcpClient();
                log("Connecting... ");
                _tcpClient.Connect(_ipAddress, _port);


                if (_tcpClient.GetState() == TcpState.Established) {
                    _stream = _tcpClient.GetStream();

                    var ipData = IPEndPoint.Parse(_tcpClient.Client.RemoteEndPoint.ToString());
                    log($"Connected { ipData.Address.ToString().Replace("::ffff:", "") }@{ipData.Port}");
                    if (!thRetryConn.IsAlive) {
                        thRetryConn.Start();
                    }
                } else {
                    throw new Exception("Unexpected error occured during terminal connection process.");
                }
            } catch (SocketException sEx) {
                if (sEx.ErrorCode == 10061) {
                    Thread.Sleep(2000);

                    _tcpClient.Close();
                    _tcpClient.Dispose();

                    if (_stream != null) {
                        _stream.Dispose();
                    }
                    _stream = null;
                    ConnectToServer();
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        private void RetryConnectToServer() {
            while (true) {
                Thread.Sleep(2000);
                if (_tcpClient.GetState() == TcpState.CloseWait || _tcpClient.GetState() == TcpState.Unknown) {

                    _tcpClient.Close();
                    _tcpClient.Dispose();

                    _stream.Dispose();
                    _stream = null;

                    ConnectToServer();
                }
            }
        }

        #endregion

        public void ReadData() {
            while (true) {
                if (_stream != null && _tcpClient != null && _tcpClient.GetState() == TcpState.Established) {
                    try {

                        // Reading header of Data that have been received
                        byte[] headBuffer = new byte[2];
                        _stream.ReadAsync(headBuffer, 0, 2).Wait();
                        var dataLength = Task.Run(() => HeaderLength(headBuffer)).Result;
                        _buffer = new byte[dataLength];

                        if (_tcpClient != null) {

                            // Check if connection is still established here.
                            if (dataLength > 0) {

                                // Read data request
                                _stream.ReadAsync(_buffer, 0, _buffer.Length).Wait();
                                var request = ByteArrayToString(_buffer);
                                log();
                                log("Received: " + request);


                                // Send broadcast data to server.
                                if (!request.Contains("CANCEL")) {
                                    SendBroadcasts();
                                }

                                // Delaying sending of data to server. In random seconds                    
                                Thread.Sleep(new Random().Next(3, 5) * 1000);

                                // Sending Response to server
                                var resData = tData.GetResponse(request);
                                resData = CalculateHeader(StringToByteArray(resData)) + resData;
                                byte[] response = StringToByteArray(resData);
                                log("Sent: " + resData.Substring(2));
                                _stream.WriteAsync(response, 0, response.Length);
                            }
                        }
                    } catch (Exception ex) {
                        log(ex.Message);
                    }
                }
            }
        }

        public void PayAtTable() {
            while (true) {
                if (_stream != null && _tcpClient != null && _tcpClient.GetState() == TcpState.Established) {
                    try {
                        var readFile = File.ReadAllLinesAsync("patt.txt");
                        readFile.Wait();

                        if (readFile.Status == TaskStatus.RanToCompletion) {
                            foreach (string item in readFile.Result) {
                                var sep = item.Split('|');
                                string request = sep[0];
                                string requestName = sep[1];

                                var buildRequest = CalculateHeader(StringToByteArray(request)) + request;
                                byte[] actualRequest = StringToByteArray(buildRequest);
                                log(string.Format("Sent: {0} | {1}", request, requestName));
                                _stream.WriteAsync(actualRequest, 0, actualRequest.Length);

                                byte[] headerBuffer = new byte[2];
                                int dataLength = Task.Run(() => HeaderLength(headerBuffer)).Result;

                                while (dataLength <= 0) {
                                    if (dataLength > 0) {
                                        byte[] buffer = new byte[dataLength];
                                        _stream.ReadAsync(buffer, 0, buffer.Length).Wait();
                                        Thread.Sleep(new Random().Next(3, 5) * 1000);

                                        string received = ByteArrayToString(buffer);
                                        log("Received: " + received);
                                    }
                                }
                            }
                            break;
                        }
                    } catch (Exception e) {
                        log(e.Message);
                    }
                }
            }
        }

        public void SendBroadcasts() {
            foreach (var broadcastData in tData.GetBroadcast()) {
                if (_stream != null) {
                    var broadcast = CalculateHeader(StringToByteArray(broadcastData)) + broadcastData;
                    byte[] broadcastDataToSend = StringToByteArray(broadcast);

                    log("Sent: " + broadcast.Substring(2));
                    _stream.WriteAsync(broadcastDataToSend, 0, broadcastDataToSend.Length);
                    Thread.Sleep(new Random().Next(1, 5) * 1000);
                }
            }
        }
    }
}
