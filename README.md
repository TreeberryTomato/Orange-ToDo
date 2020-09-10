# Tomato-ToDo
A software to manage your ToDo tasks 

# Structure of the project

- ## Client
  At present, only has web page client

- ## Server
  A server written in C#

## Structure of the Server
1. __APICore.cs__  
The core program that handle http request and response.

2. __Program.cs__  
  The entry of the Server project with main function.

3. __Scheduler.cs__  
  Store a group of tasks and arrange the tasks based on their StartDateTime

4. __Task.cs__  
  Task represent a task in real world and contains basic information:  
    1. Content
    2. StartDateTime
    3. DeadLine
    4. IsDone
    5. PriorityLevel
    6. TaskID

5. __SQLConnector__  
  A bridge to the SQL Server, at present implement CreateTasksTable and AddTask functions and can be called by the APICore
