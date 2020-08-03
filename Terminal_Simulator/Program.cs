using System;
using System.Threading;

namespace Terminal_Simulator {
    public class Program {
        static Terminal terminal;

        static void Main(string[] args) {
            Thread thConn;
            bool payAtTable = false;

            terminal = new Terminal();
            terminal.GetDetails();

            Utilities.log("Enable Pay@Table feature? 1 = Yes, 0 = No");

            int isEnablePATT = Console.Read();
            if (isEnablePATT > 0) {
                payAtTable = true;
            }

            thConn = new Thread(terminal.ConnectToServer);
            thConn.Start();

            if (!payAtTable) {
                terminal.ReadData();
            } else {
                terminal.PayAtTable();
            }
        }
    }
}
