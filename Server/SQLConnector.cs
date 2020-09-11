using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace OrangeToDo_Server
{
	class SQLServerConnector
	{
		private string _serverName;
		private string _uid;
		private string _pwd;
		private string _databaseName;

		public string ServerName
		{
			get
			{
				return _serverName;
			}
			set
			{
				sqlConnectionStringBuilder.DataSource = value;
				_serverName = value;
			}
		}

		public string UserID
		{
			get
			{
				return _uid;
			}
			set
			{
				sqlConnectionStringBuilder.UserID = value;
				_uid = value;
			}
		}

		public string Password
		{
			get
			{
				return _pwd;
			}
			set
			{
				sqlConnectionStringBuilder.Password = value;
				_pwd = value;
			}
		}

		public string DatabaseName
		{
			get
			{
				return _databaseName;
			}
			set
			{
				sqlConnectionStringBuilder.InitialCatalog = value;
				_databaseName = value;
			}
		}


		SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();

		public SQLServerConnector()
		{
			ServerName = @"LGGRAM\SQLEXPRESS";
			UserID = @"manager";
			Password = @"123456";
			DatabaseName = @"Orange-ToDo";
		}

		private void CreateTasksTable(SqlConnection connection)
		{
			string createTasksTable = @"
				create table Tasks  --创建表Tasks
				(
					Content text not null,
					StartDateTime datetime not null,
					DeadLine datetime not null,
					IsDone bit not null,
					PriorityLevel int check(PriorityLevel<=10 and PriorityLevel>=1) not null,
					TaskID varchar(37) primary key
				)";
			SqlCommand command = new SqlCommand(createTasksTable, connection);
			command.ExecuteNonQuery();
		}

		public bool AddTask(Task task)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ToString()))
				{
					connection.Open();
					SqlCommand cmd = connection.CreateCommand();
					cmd.CommandText = "if object_id('Tasks', 'u') is not null select 1 else select 0";
					if((int)cmd.ExecuteScalar()==0)
					{
						CreateTasksTable(connection);
					}
					cmd.CommandText = @$"
						INSERT INTO [dbo].[Tasks]
						   ([Content]
						   ,[StartDateTime]
						   ,[DeadLine]
						   ,[IsDone]
						   ,[PriorityLevel]
						   ,[TaskID])
						VALUES
						   ('{task.Content}'
						   ,'{task.StartDateTime.ToUniversalTime().ToString()}'
						   ,'{task.DeadLine.ToUniversalTime().ToString()}'
						   ,'{task.IsDone.ToString()}'
						   ,'{task.PriorityLevel.ToString()}'
						   ,'{task.TaskID}')";
					int affectedLines = cmd.ExecuteNonQuery();
					if (affectedLines > 0)
						return true;
					else
						return false;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}
		}

		public bool DeleteTask(string TaskID)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ToString()))
				{
					connection.Open();
					SqlCommand cmd = connection.CreateCommand();
					cmd.CommandText = "if object_id('Tasks', 'u') is not null select 1 else select 0";
					if ((int)cmd.ExecuteScalar() == 0)
					{
						CreateTasksTable(connection);
						return false;
					}

					cmd.CommandText = @$"DELETE FROM [dbo].[Tasks] WHERE TaskID='{TaskID}'";
					cmd.ExecuteNonQuery();
					return true;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}
		}
	
		public List<Task> GetTasks()
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ToString()))
				{
					List<Task> tasks =  new List<Task>();
					connection.Open();
					SqlCommand cmd = connection.CreateCommand();
					cmd.CommandText = "if object_id('Tasks', 'u') is not null select 1 else select 0";
					if ((int)cmd.ExecuteScalar() == 0)
					{
						CreateTasksTable(connection);
						return tasks;
					}
					SqlDataAdapter dataAdapter = new SqlDataAdapter(@$"SELECT * FROM Tasks", connection);
					DataSet dataSet = new DataSet();      // 创建DataSet
					dataAdapter.Fill(dataSet, "Tasks");   // 将返回的数据集作为“表”填入DataSet中，表名可以与数据库真实的表名不同，并不影响后续的增、删、改等操作
					DataTable tasksTable = dataSet.Tables["Tasks"];
					foreach (DataRow taskRow in tasksTable.Rows)
					{
						Task task = new Task(
							(string)taskRow["Content"],
							Convert.ToDateTime(taskRow["StartDateTime"]),
							Convert.ToDateTime(taskRow["DeadLine"]),
							(bool)taskRow["IsDone"],
							(int)taskRow["PriorityLevel"],
							(string)taskRow["TaskID"]
							);
						tasks.Add(task);
					}
					dataSet.Dispose();        // 释放DataSet对象
					dataAdapter.Dispose();    // 释放SqlDataAdapter对象
					return tasks;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return new List<Task>();
			}
		}
	}
}
