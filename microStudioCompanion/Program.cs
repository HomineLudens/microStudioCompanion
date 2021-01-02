﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using microStudioCompanion.Extensions;
using Newtonsoft.Json;
using Websocket.Client;

namespace microStudioCompanion
{
    class Program
    {
        static string projectSlug;
        static Project selectedProject;
        static Dictionary<string, dynamic> projects = new Dictionary<string, dynamic>();
        static Config config;
        static WebsocketClient socket;
        static void Main(string[] args)
        {

            Console.WriteLine(" ----------------------------------------------------------------------");
            Console.WriteLine("| Welcome to microStudio Companion! Let's backup your game! :)         |");
            Console.WriteLine("| If you want to contact me, write me an email: konrad@makegames.today |");
            Console.WriteLine("| You can donate me at https://fenix.itch.io/microstudio-companion     |");
            Console.WriteLine(" ----------------------------------------------------------------------");
            Console.WriteLine();

            var mode = "pull";
            projectSlug = "";
            string filePath = null;
            if (args.Length > 0)
            {
                mode = args[0];
                if (args.Length > 1)
                {
                    projectSlug = args[1];
                }

                if (args.Length > 2)
                {
                    filePath = args[2];
                }
            }

            config = Config.Get();


            using (socket = new WebsocketClient(new Uri("wss://microstudio.dev")))
            {
                using (var webClient = new WebClient())
                {
                    socket.MessageReceived.Subscribe(data => HandleResponse(data));
                    Console.WriteLine(" [i] Starting...");
                    socket.Start().Wait();
                    Console.WriteLine(" [i] Started!");
                    Task.Run(() => StartSendingPing(socket));

                    socket.Send("{\"name\":\"get_language\",\"language\":\"en\",\"request_id\":0}");
                    ////socket.ConnectAsync(new Uri("wss://microstudio.dev"), CancellationToken.None).Wait();
                    TokenHandler.GetToken(config, socket);

                    ////Console.WriteLine("Token is valid.");
                    switch (mode)
                    {
                        case "pull":
                            //if (!Directory.Exists(config.localDirectory))
                            //{
                            //    Console.WriteLine($" <!> Parent directory for your projects ({config.localDirectory}) does not exist.");
                            //    config.AskForDirectory();
                            //    config.Save();
                            //}
                            //PullFiles(projectSlug, host, config, socket, webClient);
                            break;
                        case "push-file":
                            //var projects = GetProjects(socket);
                            //Project selectedProject = SelectProject(ref projectSlug, projects);
                            //PushFile(filePath, selectedProject, config, socket);
                            break;
                        case "watch":
                            WatchProject(projectSlug, config, socket);
                            break;
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine(" ----------------------------------------------------------------------");
            Console.WriteLine("| Done! Have a great day and Make Games Today!                         |");
            Console.WriteLine("| If you want to contact me, write me an email: konrad@makegames.today |");
            Console.WriteLine("| You can donate me at https://fenix.itch.io/microstudio-companion     |");
            Console.WriteLine(" ----------------------------------------------------------------------");
            Console.WriteLine();
            if (args.Length == 0)
            {
                Console.WriteLine("Press enter to close the app.");
                Console.ReadLine();
            }
        }

        private static void HandleResponse(ResponseMessage data)
        {
            Console.WriteLine($" [<-] Incomming message: {data}");
            var response = JsonConvert.DeserializeObject<dynamic>(data.Text);
            string responseTypeText = response.name;
            if (Enum.TryParse(responseTypeText, out ResponseTypes responseType))
            {

                switch (responseType)
                {
                    case ResponseTypes.error:
                        Console.WriteLine($" <!> An error occured: {response.error}");
                        HandleError((string)response.error);
                        break;
                    case ResponseTypes.logged_in:
                        TokenHandler.SaveToken((string)response.token);
                        break;
                    case ResponseTypes.token_valid:
                        Console.WriteLine(" [i] Token valid");
                        break;
                    case ResponseTypes.project_list:
                        projects = new Dictionary<string, dynamic>();
                        foreach (var element in response.list)
                        {
                            projects[(string)element.slug] = element;
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Console.WriteLine($" <!> Unknown response type: {responseTypeText}");
            }
        }

        private static void HandleError(string error)
        {
            if (Enum.TryParse(error.Replace(' ', '_'), out ResponseErrors errorType))
            {
                switch (errorType)
                {
                    case ResponseErrors.unknown_user:
                        break;
                    case ResponseErrors.invalid_token:
                        Console.WriteLine("get new token");
                        TokenHandler.Login(config, socket);
                        break;
                    case ResponseErrors.not_connected:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Console.WriteLine($" <!> Unknown error type: {error}");
            }
        }

        private static async Task StartSendingPing(IWebsocketClient client)
        {
            while (true)
            {
                await Task.Delay(30000);

                if (!client.IsRunning)
                    continue;

                client.Send(new PingRequest().Serialize());
            }
        }
        private static void WatchProject(string projectSlug, Config config, WebsocketClient socket)
        {
            var projects = GetProjects(socket);
            //selectedProject = SelectProject(ref projectSlug, projects);
            //var localProjectPath = Path.Combine(config.localDirectory, selectedProject.title);

            //FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(localProjectPath)
            //{
            //    IncludeSubdirectories = true,
            //    Filters = { "*.ms", "*.png", "*.json", "*.md" },
            //    EnableRaisingEvents = true,
            //    NotifyFilter = NotifyFilters.FileName
            //        | NotifyFilters.DirectoryName
            //        | NotifyFilters.Attributes
            //        | NotifyFilters.Size
            //        | NotifyFilters.LastWrite
            //        | NotifyFilters.LastAccess
            //        | NotifyFilters.CreationTime
            //        | NotifyFilters.Security
            //};
            //fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            //fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;
            //fileSystemWatcher.Created += FileSystemWatcher_Created;
            //fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;


            while (true)
            {

            }
        }

        //private static void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        //{
        //    var subDirectories = new List<string> { "ms", "sprites", "maps", "doc" };
        //    if (!subDirectories.Contains(Path.GetDirectoryName(e.Name)))
        //    {
        //        return;
        //    }
        //    var filePath = e.Name.Replace('\\', '/');
        //    DeleteFile(filePath, selectedProject, config, socket);
        //}

        //private static void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        //{
        //    var subDirectories = new List<string> { "ms", "sprites", "maps", "doc" };
        //    if (!subDirectories.Contains(Path.GetDirectoryName(e.Name)))
        //    {
        //        return;
        //    }
        //    var filePath = e.Name.Replace('\\', '/');

        //    PushFile(filePath, selectedProject, config, socket);
        //}

        //private static void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        //{
        //    var subDirectories = new List<string> { "ms", "sprites", "maps", "doc" };
        //    if (!subDirectories.Contains(Path.GetDirectoryName(e.Name)))
        //    {
        //        return;
        //    }
        //    var filePath = e.Name.Replace('\\', '/');
        //    var oldFilePath = e.OldName.Replace('\\', '/');
        //    DeleteFile(oldFilePath, selectedProject, config, socket);
        //    PushFile(filePath, selectedProject, config, socket);
        //}

        //private static void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        //{
        //    var subDirectories = new List<string> { "ms", "sprites", "maps", "doc" };
        //    if (!subDirectories.Contains(Path.GetDirectoryName(e.Name)))
        //    {
        //        return;
        //    }
        //    var filePath = e.Name.Replace('\\', '/');

        //    PushFile(filePath, selectedProject, config, socket);
        //}

        private static void DeleteFile(string filePath, Project selectedProject, Config config, ClientWebSocket socket)
        {
            var lockProjectFileRequest = new LockProjectFileRequest
            {
                file = filePath,
                project = selectedProject.id
            };
            Console.WriteLine($" [i] Locking file {filePath} in project {selectedProject.slug}");
            socket.SendRequest(lockProjectFileRequest);

            var deleteRequest = new DeleteProjectFileRequest
            {
                project = selectedProject.id,
                file = filePath
            };

            Console.WriteLine($" [i] Deleting file {filePath} in project {selectedProject.slug}");
            var deleteResponse = socket.SendAndReceive<DeleteProjectFileRequest, DeleteProjectFileResponse>(deleteRequest);
            if (deleteResponse.name == "error")
            {
                Console.WriteLine($" <!> An error occured: {deleteResponse.error}");
            }
            else
            {
                Console.WriteLine($" [i] Deleting of file {filePath} completed");
            }
        }

        private static void PushFile(string filePath, Project selectedProject, Config config, ClientWebSocket socket)
        {
            var lockProjectFileRequest = new LockProjectFileRequest
            {
                file = filePath,
                project = selectedProject.id
            };
            Console.WriteLine($" [i] Locking file {filePath} in project {selectedProject.slug}");
            socket.SendRequest(lockProjectFileRequest);
            
            string content;
            try
            {
                content = ReadFileContent(filePath, selectedProject, config);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($" <i> File {filePath} in project {selectedProject.slug} does not exist");
                return;
            }
            catch
            {
                Thread.Sleep(300);
                content = ReadFileContent(filePath, selectedProject, config);
            }

            var writeRequest = new WriteProjectFileRequest
            {
                project = selectedProject.id,
                file = filePath,
                content = content
            };

            Console.WriteLine($" [i] Writing file {filePath} in project {selectedProject.slug}");
            var writeResponse = socket.SendAndReceive<WriteProjectFileRequest, WriteProjectFileResponse>(writeRequest);
            if (writeResponse.name == "error")
            {
                Console.WriteLine($" <!> An error occured: {writeResponse.error}");
            }
            else
            {
                Console.WriteLine($" [i] Writing of file {filePath} completed");
            }
        }

        private static string ReadFileContent(string filePath, Project selectedProject, Config config)
        {
            string content;
            var localFilePath = Path.Combine(config.localDirectory, selectedProject.title, filePath);
            var extension = Path.GetExtension(filePath);
            switch (extension)
            {
                case ".png":
                    content = Convert.ToBase64String(System.IO.File.ReadAllBytes(localFilePath));
                    break;
                default:
                    content = System.IO.File.ReadAllText(localFilePath);
                    break;
            }

            return content;
        }

        private static void PullFiles(string projectSlug, string host, Config config, WebsocketClient socket, WebClient webClient)
        {
            var projects = GetProjects(socket);
            Project selectedProject = SelectProject(ref projectSlug, projects);

            var remoteDirectories = new[] { "ms", "sprites", "maps", "doc" };
            var localDirectoryMapping = new Dictionary<string, string>
                        {
                            { "ms", "ms" },
                            { "sprites", "sprites" },
                            { "maps", "maps" },
                            { "doc", "doc" }
                        };

            foreach (var dir in remoteDirectories)
            {
                var listProjectFilesRequest = new ListProjectFilesRequest
                {
                    folder = dir,
                    project = selectedProject.id
                };

                var nick = selectedProject.owner.nick;
                var slug = selectedProject.slug;
                var code = selectedProject.code;
                var name = selectedProject.title;
                var localDirectoryPath = Path.Combine(config.localDirectory, name, localDirectoryMapping[dir]);
                if (Directory.Exists(localDirectoryPath))
                {
                    Directory.Delete(localDirectoryPath, true);
                }



                var listProjectFilesResponse = new ListProjectFilesResponse();
                    //socket.SendAndReceive<ListProjectFilesRequest, ListProjectFilesResponse>(listProjectFilesRequest);
                if (listProjectFilesResponse.files.Length == 0)
                {
                    continue;
                }

                var directoryCreated = false;
                while (!directoryCreated)
                {
                    try
                    {
                        Directory.CreateDirectory(localDirectoryPath);
                        directoryCreated = true;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Console.WriteLine(e.Message);
                        config.AskForDirectory();
                        config.Save();
                        localDirectoryPath = Path.Combine(config.localDirectory, name, localDirectoryMapping[dir]);
                    }
                }

                int index = 0;
                int amount = listProjectFilesResponse.files.Length;
                foreach (var file in listProjectFilesResponse.files)
                {
                    var onlineFilePath = $"{host}/{nick}/{slug}/{code}/{dir}/{file.file}";
                    var localFilePath = Path.Combine(localDirectoryPath, file.file);
                    Console.WriteLine($" [i] Downloading ({++index}/{amount}) file from \"{dir}\" directory: {file.file}");
                    webClient.DownloadFile(onlineFilePath, localFilePath);
                }
            }
            Console.WriteLine($" [i] Project downloaded to: {Path.Combine(config.localDirectory, selectedProject.title)}");
        }

        private static Project SelectProject(ref string projectSlug, Dictionary<string, Project> projects)
        {
            Project selectedProject;

            while (true)
            {
                if (string.IsNullOrWhiteSpace(projectSlug))
                {
                    Console.Write(" (?) Project slug to backup (leave empty to see available projects): ");
                    projectSlug = Console.ReadLine();
                }

                if (projects.ContainsKey(projectSlug))
                {
                    selectedProject = projects[projectSlug];
                    break;
                }
                else
                {
                    Console.WriteLine($" <!> Pick project slug from the list: (Press any key to continue)");
                    Console.ReadKey(true);
                    foreach (var proj in projects.Values)
                    {
                        Console.WriteLine($"- Slug: {proj.slug}  Title: {proj.title}");
                    }
                    projectSlug = null;
                }
            }

            return selectedProject;
        }

        private static Dictionary<string, Project> GetProjects(WebsocketClient socket)
        {
            var getProjectListRequest = new GetProjectListRequest();
            socket.Send(getProjectListRequest.Serialize());
            //var getProjectListResponse = socket.SendAndReceive<GetProjectListRequest, GetProjectListResponse>(getProjectListRequest);
            
            var projects = new Dictionary<string, Project>();
            //foreach (var element in getProjectListResponse.list)
            //{
            //    projects[element.slug] = element;
            //}

            return projects;
        }
    }
}
