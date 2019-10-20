using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Server.MirDatabase;
using Server.MirObjects;
using Server.MirEnvir;
using Newtonsoft.Json;
using System.Collections.Generic;

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
                        string pcount = SMain.Envir.Players.Count.ToString();
                        string mcount = SMain.Envir.MonsterCount.ToString();
                        string acount = SMain.Envir.AccountList.Count().ToString();
                        string ccount = SMain.Envir.CharacterList.Count.ToString();
                        string connections = SMain.Envir.Connections.Count.ToString();

                        var stats = new
                        {
                            name = servername,
                            accounts = acount,
                            characters = ccount,
                            players = pcount,
                            connections = connections,
                            mobs = mcount
                        };

                        var json = JsonConvert.SerializeObject(stats);
                        try
                        {
                            json = JsonConvert.SerializeObject(stats, Formatting.Indented,
                            new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                            });
                            WriteResponse(response, json);
                        }
                        catch (Exception e)
                        {
                            Console.Write(e);
                        }
                        break;

                    case "/players":

                        try
                        {
                            int playerscount = SMain.Envir.Players.Count;

                            var players = new List<string>();
                            players.Add("{ OnlinePlayers: " + playerscount + " }");

                            if (playerscount > 0)
                            {
                                
                                foreach (var player in SMain.Envir.Players)
                                {

                                    players.Add("{ name:" + player.Name + ", level:" + player.Level + ", class:" + player.Class + " }");
                                   
                                }

                            }


                            json = JsonConvert.SerializeObject(players, Formatting.Indented,
                            new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                            });
                            WriteResponse(response, json);

                        } catch (Exception e)
                        {
                            Console.Write(e);
                        }
                       

                        break;

                    case "/accounts":
                        var accounts = SMain.Envir.AccountList;
                        try
                        {
                            json = JsonConvert.SerializeObject(accounts, Formatting.Indented,
                            new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                            });
                            WriteResponse(response, json);
                        } catch (Exception e)
                        {
                            Console.Write(e);
                        }
                        break;

                    case "/items":
                        var items = SMain.Envir.ItemInfoList;
                        try
                        {
                            json = JsonConvert.SerializeObject(items, Formatting.Indented,
                            new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                            });
                            WriteResponse(response, json);
                        }
                        catch (Exception e)
                        {
                            Console.Write(e);
                        }
                        break;

                    case "/mobs":
                        var mobs = SMain.Envir.MonsterInfoList;
                        try
                        {
                            json = JsonConvert.SerializeObject(mobs, Formatting.Indented,
                            new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                            });
                            WriteResponse(response, json);
                        }
                        catch (Exception e)
                        {
                            Console.Write(e);
                        }
                        break;

                    case "/magic":
                        var magic = SMain.Envir.MagicInfoList;
                        try
                        {
                            json = JsonConvert.SerializeObject(magic, Formatting.Indented,
                            new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                            });
                            WriteResponse(response, json);
                        }
                        catch (Exception e)
                        {
                            Console.Write(e);
                        }
                        break;

                    case "/register":

                        /*
                         * eg:
                         * http://localhost:5679/register?id=satch&psd=satch1&email=satch1@satch.com&name=satch1&question=1&answer=1&ip=1.1.1.1
                         */

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

                        /*
                         * Some of the Return codes
                         * 7 = Success
                         * 2 = Incorrect
                         * 6 = too many attempts
                         */

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
                        WriteResponse(response, "No valid API endpoint specified");
                        break;
                }
            }
            catch (Exception error)
            {
                WriteResponse(response, "API Error: " + error);
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
