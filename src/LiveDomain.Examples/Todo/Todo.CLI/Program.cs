using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Todo.Core;
using LiveDomain.Enterprise;

namespace Todo.CLI
{
    class Program
    {
        private static List<string> lists;
        private static string currentList;
        static List<TaskInfo> currentTasks;
        private static LiveDomainClient<TodoModel> client;

        static void Main(string[] args)
        {
            var host = args.Length > 0 ? args[0] : "localhost";
            var port = args.Length > 1 ? Int32.Parse(args[1]) : 9292;
            
            Console.WriteLine("Host: " + host);
            Console.WriteLine("Port: " + port);

            client = new LiveDomainClient<TodoModel>(host, port);

            Console.Write("Connecting to server..");
            client.Open();
            Console.WriteLine("ok!");

            Console.Write("Retrieving list names...");
            lists = client.Execute(new GetListNames()).ToList();
            Console.WriteLine("ok!");
            Console.WriteLine("Type help for commands, exit to quit");

            currentList = lists.FirstOrDefault();
            
            if(currentList != null) GetCurrentTasks();

            while(true)
            {
                Console.Write("[{0}]> ", currentList);
                string line = Console.ReadLine();

                if( line == null || line.ToLower() == "exit") break;
                else if (line.StartsWith("addlist "))
                {
                    string listName = line.Substring(8);
                    if (lists.Contains(listName))
                    {
                        Console.WriteLine("List already exists");
                    }
                    else
                    {
                        lists.Add(listName);
                        client.Execute(new AddListCommand(listName));
                        if(lists.Count == 1)
                        {
                            currentList = listName;
                            currentTasks = new List<TaskInfo>();
                        }
                    }
                }
                else if (line.StartsWith("setlist "))
                {
                    string listName = line.Substring(8);
                    if (!lists.Contains(listName)) Console.WriteLine("No such list");
                    else
                    {
                        currentList = listName;
                        GetCurrentTasks();
                    }
                }
                else if (line.ToLower() == "help") Console.WriteLine("commands: exit, addlist <list>, setlist <list>, add <item>, showlists, showitems");
                else if (line.StartsWith("add "))
                {
                    if(currentList == null)
                    {
                        Console.WriteLine("You must add a list before you can add items!");
                    }
                    string item = line.Substring(4);
                    if(currentTasks.Any(t => t.Title == item))
                    {
                        Console.WriteLine("Item already exists");
                    }
                    else
                    {
                        client.Execute(new AddTaskCommand(new Task(item), currentList));
                        GetCurrentTasks();
                    }
                }
                else if (line.ToLower() == "showlists")
                {
                    lists.ForEach(Console.WriteLine);
                }
                else if (line.ToLower() == "showitems")
                {
                    currentTasks.ForEach(t => Console.WriteLine("{0}", t.Title));
                }
            }
        }

        private static void GetCurrentTasks()
        {
            currentTasks = client.Execute(new GetTasksByListName(currentList)).ToList();
        }

    }
}
