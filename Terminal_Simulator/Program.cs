using System.Threading;

namespace Terminal_Simulator {
    public class Program {
        static Terminal terminal;

        static void Main(string[] args) {
            Thread thConn;

            terminal = new Terminal();
            terminal.GetDetails();

            thConn = new Thread(terminal.ConnectToServer);
            thConn.Start();

            terminal.ReadData();
        }
    }
}
