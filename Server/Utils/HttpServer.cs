using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Server.MirEnvir;
using Newtonsoft.Json;

namespace Server
{
    class HttpServer : HttpService {

        

        Thread thread;

        public HttpServer() {
            host = Settings.HTTPIPAddress;
            port = Settings.HTTPIPPort;
        }

        public void Start() {
            thread = new Thread(new ThreadStart(Listen));
            thread.Start();
        }

        public new void Stop() {
            base.Stop();
            if (thread!=null)
            {
                thread.Abort();
            }
        }


        public override void OnGetRequest(HttpListenerRequest request, HttpListenerResponse response)
		{
            string url = request.Url.PathAndQuery;
            if (url.Contains("?"))
            {
                url = url.Substring(0,url.IndexOf("?"));
                url = url.ToLower();
            }
            try
            {
                switch (url)
                {
                    case "/":
                        WriteResponse(response, GameLanguage.GameName);
                        break;

                    case "/stats":
                        
                        string servername = GameLanguage.GameName;
                        string a = GameLanguage.OnlinePlayers.ToString();
                        //string version = Server.Settings.
                        string pcount = SMain.Envir.Players.Count.ToString();
                        string mcount = SMain.Envir.MonsterCount.ToString();
                        string acount = SMain.Envir.AccountList.Count().ToString();
                        string ccount = SMain.Envir.CharacterList.Count.ToString();
                        string connections = SMain.Envir.Connections.Count.ToString();

                        //string MirEnvir.

                        var stats = new
                        {
                            a = a,
                            name = servername,
                            //version = version,
                            accounts = acount,
                            characters = ccount,
                            players = pcount,
                            connections = connections,
                            mobs = mcount
                        };

                        var json = JsonConvert.SerializeObject(stats);
                        WriteResponse(response, json);
                        break;

                    case "/players":
                        var players = SMain.Envir.Players;
                        json = JsonConvert.SerializeObject(players, Formatting.Indented,
                        new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                            });
                        WriteResponse(response, json);
                        break;

                    case "/accounts":
                        var accounts = SMain.Envir.AccountList;
                        json = JsonConvert.SerializeObject(accounts, Formatting.Indented,
                        new JsonSerializerSettings()
                            {
                                   ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                            });
                        WriteResponse(response, json);
                        break;

                    case "/items":
                        var items = SMain.Envir.ItemInfoList;
                        json = JsonConvert.SerializeObject(items, Formatting.Indented,
                        new JsonSerializerSettings()
                            {
                                   ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                            });
                        WriteResponse(response, json);
                        break;

                    case "/maps":
                        var maps = SMain.Envir.MapList;
                        json = JsonConvert.SerializeObject(maps, Formatting.Indented,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                        });
                        WriteResponse(response, json);
                        break;

                    case "/magic":
                        var magic = SMain.Envir.MagicInfoList;
                        json = JsonConvert.SerializeObject(magic, Formatting.Indented,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                        });
                        WriteResponse(response, json);
                        break;

                    case "/register":
                        string id = request.QueryString["id"].ToString();
                        string psd = request.QueryString["psd"].ToString();
                        string email = request.QueryString["email"].ToString();
                        string name = request.QueryString["name"].ToString();
                        string question = request.QueryString["question"].ToString();
                        string answer = request.QueryString["answer"].ToString();
                        string ip = request.QueryString["ip"].ToString();


                        ClientPackets.NewAccount p = new ClientPackets.NewAccount();
                        p.AccountID = id;
                        p.Password = psd;
                        p.EMailAddress = email;
                        p.UserName = name;
                        p.SecretQuestion = question;
                        p.SecretAnswer = answer;
                        int result = SMain.Envir.HTTPNewAccount(p,ip);
                        WriteResponse(response, result.ToString());
                        break;

                    case "/login":
                        id = request.QueryString["id"].ToString();
                        psd = request.QueryString["psd"].ToString();
                        result = SMain.Envir.HTTPLogin(id, psd);
                        WriteResponse(response, result.ToString());                        
                        break;

                    case "/addnamelist":
                        id = request.QueryString["id"].ToString();
                        string fileName = request.QueryString["fileName"].ToString();                   
                        addNameList(id, fileName);
                        WriteResponse(response, "true");
                        break;

                    default:
                        WriteResponse(response, "error");
                        break;
                }
            }
            catch (Exception error)
            {
                WriteResponse(response, "request error: " + error);
            }
        }

        void addNameList(string playerName,string fileName) {
            fileName = Settings.NameListPath + fileName;
            string sDirectory = Path.GetDirectoryName(fileName);
            Directory.CreateDirectory(sDirectory);
            string tempString = fileName;
            if (File.ReadAllLines(tempString).All(t => playerName != t))
            {
                using (var line = File.AppendText(tempString))
                {
                    line.WriteLine(playerName);
                }
            }
        }


        public override void OnPostRequest(HttpListenerRequest request, HttpListenerResponse response) {
            Console.WriteLine("POST request: {0}", request.Url);
        }
    }

}
