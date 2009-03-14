using System;
using System.Collections.Generic;
using Cassia;

namespace SessionInfo
{
    internal class Program
    {
        private static readonly ITerminalServicesManager _manager = new TerminalServicesManager();

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowCurrentSession();
                return;
            }
            switch (args[0].ToLower())
            {
                case "current":
                    ShowCurrentSession();
                    return;
                case "get":
                    GetSessionInfo(args);
                    return;
                case "listservers":
                    ListServers(args);
                    return;
                case "listsessions":
                    ListSessions(args);
                    return;
                case "listsessionprocesses":
                    ListSessionProcesses(args);
                    return;
                case "listprocesses":
                    ListProcesses(args);
                    return;
                case "logoff":
                    LogoffSession(args);
                    return;
                case "disconnect":
                    DisconnectSession(args);
                    return;
                case "message":
                    SendMessage(args);
                    return;
                case "ask":
                    AskQuestion(args);
                    return;
            }
            Console.WriteLine("Unknown command: " + args[0]);
        }

        private static void ListProcesses(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: SessionInfo listprocesses [server]");
            }
            using (ITerminalServer server = GetServerFromName(args[1]))
            {
                server.Open();
                WriteProcesses(server.GetProcesses());
            }
        }

        private static void WriteProcesses(IEnumerable<ITerminalServicesProcess> processes)
        {
            foreach (ITerminalServicesProcess process in processes)
            {
                WriteProcessInfo(process);
            }
        }

        private static void WriteProcessInfo(ITerminalServicesProcess process)
        {
            Console.WriteLine("Session ID: " + process.SessionId);
            Console.WriteLine("Process ID: " + process.ProcessId);
            Console.WriteLine("Process Name: " + process.ProcessName);
            Console.WriteLine("SID: " + process.Sid);
        }

        private static void ListServers(string[] args)
        {
            string domainName = (args.Length > 1 ? args[1] : null);
            foreach (ITerminalServer server in _manager.GetServers(domainName))
            {
                Console.WriteLine(server.ServerName);
            }
        }

        private static void AskQuestion(string[] args)
        {
            if (args.Length < 8)
            {
                Console.WriteLine(
                    "Usage: SessionInfo ask [server] [session id] [icon] [caption] [text] [timeout] [buttons]");
                return;
            }
            int seconds = int.Parse(args[6]);
            int sessionId = int.Parse(args[2]);
            using (ITerminalServer server = GetServerFromName(args[1]))
            {
                server.Open();
                ITerminalServicesSession session = server.GetSession(sessionId);
                RemoteMessageBoxIcon icon =
                    (RemoteMessageBoxIcon) Enum.Parse(typeof(RemoteMessageBoxIcon), args[3], true);
                RemoteMessageBoxButtons buttons =
                    (RemoteMessageBoxButtons) Enum.Parse(typeof(RemoteMessageBoxButtons), args[7], true);
                RemoteMessageBoxResult result =
                    session.MessageBox(args[5], args[4], buttons, icon, default(RemoteMessageBoxDefaultButton),
                                       default(RemoteMessageBoxOptions), TimeSpan.FromSeconds(seconds), true);
                Console.WriteLine("Response: " + result);
            }
        }

        private static void SendMessage(string[] args)
        {
            if (args.Length < 6)
            {
                Console.WriteLine("Usage: SessionInfo message [server] [session id] [icon] [caption] [text]");
                return;
            }
            int sessionId = int.Parse(args[2]);
            using (ITerminalServer server = GetServerFromName(args[1]))
            {
                server.Open();
                ITerminalServicesSession session = server.GetSession(sessionId);
                RemoteMessageBoxIcon icon =
                    (RemoteMessageBoxIcon) Enum.Parse(typeof(RemoteMessageBoxIcon), args[3], true);
                session.MessageBox(args[5], args[4], icon);
            }
        }

        private static void GetSessionInfo(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: SessionInfo get [server] [session id]");
                return;
            }
            int sessionId = int.Parse(args[2]);
            using (ITerminalServer server = GetServerFromName(args[1]))
            {
                server.Open();
                WriteSessionInfo(server.GetSession(sessionId));
            }
        }

        private static void ListSessionProcesses(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: SessionInfo listsessionprocesses [server] [session id]");
                return;
            }
            int sessionId = int.Parse(args[2]);
            using (ITerminalServer server = GetServerFromName(args[1]))
            {
                server.Open();
                ITerminalServicesSession session = server.GetSession(sessionId);
                WriteProcesses(session.GetProcesses());
            }
        }

        private static void ListSessions(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: SessionInfo list [server]");
                return;
            }
            using (ITerminalServer server = GetServerFromName(args[1]))
            {
                server.Open();
                foreach (ITerminalServicesSession session in server.GetSessions())
                {
                    WriteSessionInfo(session);
                }
            }
        }

        private static void ShowCurrentSession()
        {
            WriteSessionInfo(_manager.CurrentSession);
        }

        private static void LogoffSession(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: SessionInfo disconnect [server] [session id]");
                return;
            }
            string serverName = args[1];
            int sessionId = int.Parse(args[2]);
            using (ITerminalServer server = GetServerFromName(serverName))
            {
                server.Open();
                ITerminalServicesSession session = server.GetSession(sessionId);
                session.Logoff();
            }
        }

        private static ITerminalServer GetServerFromName(string serverName)
        {
            return
                (serverName.Equals("local", StringComparison.InvariantCultureIgnoreCase)
                     ? _manager.GetLocalServer()
                     : _manager.GetRemoteServer(serverName));
        }

        private static void DisconnectSession(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: SessionInfo disconnect [server] [session id]");
                return;
            }
            string serverName = args[1];
            int sessionId = int.Parse(args[2]);
            using (ITerminalServer server = GetServerFromName(serverName))
            {
                server.Open();
                ITerminalServicesSession session = server.GetSession(sessionId);
                session.Disconnect();
            }
        }

        private static void WriteSessionInfo(ITerminalServicesSession session)
        {
            Console.WriteLine("Session ID: " + session.SessionId);
            Console.WriteLine("User Name: " + session.UserName);
            Console.WriteLine("State: " + session.ConnectionState);
            Console.WriteLine("Logon Time: " + session.LoginTime);
            Console.WriteLine();
        }
    }
}