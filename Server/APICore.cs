using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace OrangeToDo_Server
{
	static class APICore
	{
		public static void Run()
		{
			ThreadStart threadStart_GetTasks = new ThreadStart(GetTasks);
			Thread thread_GetTasks = new Thread(threadStart_GetTasks);
			thread_GetTasks.Start();
		}

		/// <summary>
		/// this function will continuously listen request 
		/// with prefix "http://localhost:8080/v1/taskinfo/"
		/// Once get the request, it will call ReturnTasks function.
		/// </summary>
		private static void GetTasks()
		{
			string prefix = @"http://localhost:8080/v1/taskinfo/";
			HttpListener listener = new HttpListener();
			listener.Prefixes.Add(prefix);
			listener.Start();

			while (true)
			{
				try
				{
					//等待请求连接
					//没有请求则GetContext处于阻塞状态
					//听到请求后创建新线程处理请求
					HttpListenerContext context = listener.GetContext();
					Console.WriteLine(context.Request.RawUrl);
					ThreadPool.QueueUserWorkItem(new WaitCallback(ReturnTasks), context);
				}
				catch(Exception e)
				{
					Console.WriteLine("GetTasks service crashed");
					Console.WriteLine(e.Message);
					break;
				}
			}
			listener.Stop();
		}


		/// <summary>
		/// return tasks info in json to client based on param context
		/// </summary>
		/// <param name="context">Used when send response</param>
		private static void ReturnTasks(object context)
		{
			HttpListenerResponse response = ((HttpListenerContext)context).Response;
			response.StatusCode = 200;  //status code return to client
			response.AppendHeader("Access-Control-Allow-Origin", "*");


			//准备数据
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

			byte[] buffer = Encoding.UTF8.GetBytes(responseString);

			// Get a response stream and write the response to it.
			response.ContentLength64 = buffer.Length;
			System.IO.Stream output = response.OutputStream;
			output.Write(buffer, 0, buffer.Length);

			response.Close();

			// Close the output stream.
			output.Close();
		}
	}
}
