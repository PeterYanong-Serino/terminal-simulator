using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using static Terminal_Simulator.Utilities;

namespace Terminal_Simulator {
    public class TerminalData {
        private string _filePath;
        Dictionary<string, string> data = new Dictionary<string, string>();

        public TerminalData(string path) {
            if (string.IsNullOrEmpty(path)) {
                throw new Exception("File path is incorrect.");
            }
            _filePath = path;
            ReadFile();
        }

        private void ReadFile() {
            var readAsync = File.ReadAllLinesAsync(_filePath);
            readAsync.Wait();

            if (readAsync.Status == System.Threading.Tasks.TaskStatus.RanToCompletion) {

                foreach (var item in readAsync.Result) {
                    var line = item.Split('|');
                    string req = line[0];
                    string res = line[1];
                    data.Add(req, res);
                }

                log("Data Loaded Count: " +  data.Count);
                log();
            }
        }

        public string GetResponse(string request) {
            try {
                string returnData;
                if (data.TryGetValue(request, out returnData)) {
                    return returnData;
                }
                else {
                    throw new Exception($"There is not request for \"{request}\" found in data.txt file." +
                        $" Please add request and expected data response to test. Cick Cancel in Simulator Application");
                }
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        public string[] GetBroadcast() {
            List<string> broadcasts = new List<string>();
            broadcasts.Add("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><BROADCAST CODE=\"A9\"/>");
            broadcasts.Add("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><BROADCAST CODE=\"AD\"/>");
            broadcasts.Add("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><BROADCAST CODE=\"A0\"/>");
            broadcasts.Add("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><BROADCAST CODE=\"A1\"/>");
            broadcasts.Add("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><BROADCAST CODE=\"A2\"/>");
            broadcasts.Add("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><BROADCAST CODE=\"A7\"/>");

            return broadcasts.ToArray();
        }
    }
}
