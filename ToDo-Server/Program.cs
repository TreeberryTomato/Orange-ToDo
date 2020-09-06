using System;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;

namespace ToDo_Server
{
    class Program
    {
        static void Main()
        {
            LinkedList<Task> tasks = new LinkedList<Task>();
            var task1 = new Task();
            task1.Content = "Hello";

            System.Threading.Thread.Sleep(2000);

            var task2 = new Task();
            task2.Content = "World";

            tasks.AddFirst(task2);
            tasks.AddLast(task1);

            var scheduler = new Scheduler(tasks);

            Console.WriteLine(scheduler.Tasks.First.Value.Content);

        }
        static void Main1(string[] args)
        {
            string prefix = @"http://localhost:8080/v1/taskinfo/";
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();
            while(true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                context.Response.AppendHeader("Access-Control-Allow-Origin", "*");

                Console.WriteLine(request.RawUrl);

                HttpListenerResponse response = context.Response;
                

                Task task1 = new Task()
                {
                    Content = "完成任务1",
                    StartDateTime = DateTime.Now.ToLocalTime(),
                    DeadLine = DateTime.Now.ToLocalTime().AddDays(3),
                    IsDone = false
                };

                Task task2 = new Task()
                {
                    Content = "完成任务2",
                    StartDateTime = DateTime.Now.ToLocalTime(),
                    DeadLine = DateTime.Now.ToLocalTime().AddDays(3),
                    IsDone = false
                };

                var tasks = new List<Task>(2);

                tasks.Add(task1);
                tasks.Add(task2);
                

                string responseString = JsonSerializer.Serialize(tasks);

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);

                response.Close();

                // You must close the output stream.
                output.Close();
                
            }
            listener.Stop();
        }
    }
}
