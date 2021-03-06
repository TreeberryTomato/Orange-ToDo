﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace OrangeToDo_Server
{
	static class APICore
	{
		public static void Run()
		{
			//创建一个线程，监听获取任务请求
			ThreadStart threadStart_GetTasks = new ThreadStart(ListenGetTasks);
			Thread thread_GetTasks = new Thread(threadStart_GetTasks);
			thread_GetTasks.Start();

			//创建一个线程，监听删除任务请求
			ThreadStart threadStart_deleteTask = new ThreadStart(ListenDeleteTask);
			Thread thread_deleteTask = new Thread(threadStart_deleteTask);
			thread_deleteTask.Start();

			//创建一个线程，监听添加任务请求
			ThreadStart threadStart_addTask = new ThreadStart(ListenAddTask);
			Thread thread_addTask = new Thread(threadStart_addTask);
			thread_addTask.Start();
		}

		/// <summary>
		/// this function will continuously listen request 
		/// with prefix "http://localhost:8080/v1/GetTasks/"
		/// Once get the request, it will call ReturnTasks function.
		/// </summary>
		private static void ListenGetTasks()
		{
			string prefix = @"http://localhost:8080/v1/GetTasks/";
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
			SQLServerConnector connector = new SQLServerConnector();
			List<Task> tasks = connector.GetTasks();
			if(tasks.Count==0)
			{
				response.StatusCode = 503;  //status code return to client
			}

			string responseString = System.Text.Json.JsonSerializer.Serialize(tasks);

			byte[] buffer = Encoding.UTF8.GetBytes(responseString);

			// Get a response stream and write the response to it.
			response.ContentLength64 = buffer.Length;
			System.IO.Stream output = response.OutputStream;
			output.Write(buffer, 0, buffer.Length);

			response.Close();

			// Close the output stream.
			output.Close();
		}
		

		private static void ListenDeleteTask()
		{
			string prefix = @"http://localhost:8080/v1/DeleteTask/";
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
					ThreadPool.QueueUserWorkItem(new WaitCallback(DeleteTask), context);
				}
				catch (Exception e)
				{
					Console.WriteLine("DeleteTask service crashed");
					Console.WriteLine(e.Message);
					break;
				}
			}
			listener.Stop();
		}
		
		private static void DeleteTask(object context)
		{
			HttpListenerRequest request = ((HttpListenerContext)context).Request;
			HttpListenerResponse response = ((HttpListenerContext)context).Response;
			response.AppendHeader("Access-Control-Allow-Origin", "*");

			string TaskID = request.RawUrl.Split("?")[1].Split("&")[0].Split("=")[1];

			SQLServerConnector connector = new SQLServerConnector();
			bool isSuccessful = connector.DeleteTask(TaskID);

			byte[] buffer;
			if (isSuccessful)
			{
				buffer = Encoding.UTF8.GetBytes("删除成功");
				response.StatusCode = 200;
			}
			else
			{
				buffer = Encoding.UTF8.GetBytes("删除失败，请稍后重试");
				response.StatusCode = 503;
			}

			// Get a response stream and write the response to it.
			response.ContentLength64 = buffer.Length;
			System.IO.Stream output = response.OutputStream;
			output.Write(buffer, 0, buffer.Length);
			response.Close();
		}

		private static void ListenAddTask()
		{
			string prefix = @"http://localhost:8080/v1/AddTask/";
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
					ThreadPool.QueueUserWorkItem(new WaitCallback(AddTask), context);
				}
				catch (Exception e)
				{
					Console.WriteLine("AddTask service crashed");
					Console.WriteLine(e.Message);
					break;
				}
			}
			listener.Stop();
		}

		private static void AddTask(object context)
		{
			HttpListenerRequest request = ((HttpListenerContext)context).Request;
			HttpListenerResponse response = ((HttpListenerContext)context).Response;
			response.AppendHeader("Access-Control-Allow-Origin", "*");

			System.IO.Stream input = request.InputStream;
			System.IO.StreamReader reader = new System.IO.StreamReader(input, request.ContentEncoding);
			JObject jo = (JObject)JsonConvert.DeserializeObject(reader.ReadToEnd());

			Task task = new Task()
			{
				Content = jo["Content"].ToString(),
				StartDateTime = Convert.ToDateTime(jo["StartDateTime"].ToString()),
				DeadLine = Convert.ToDateTime(jo["DeadLine"].ToString()),
				PriorityLevel = Int16.Parse(jo["PriorityLevel"].ToString()),
				IsDone = jo["IsDone"].ToString().Equals("true"),
			};

			SQLServerConnector connector = new SQLServerConnector();
			bool isSuccessful = connector.AddTask(task);

			byte[] buffer;
			if (isSuccessful)
			{
				buffer = Encoding.UTF8.GetBytes("任务添加成功");
				response.StatusCode = 200;
			}
			else
			{
				buffer = Encoding.UTF8.GetBytes("任务添加失败，请稍后再试");
				response.StatusCode = 503;
			}

			// Get a response stream and write the response to it.
			response.ContentLength64 = buffer.Length;
			System.IO.Stream output = response.OutputStream;
			output.Write(buffer, 0, buffer.Length);
			response.Close();
		}
	}
}
