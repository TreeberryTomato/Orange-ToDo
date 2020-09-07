using System;
using System.Collections.Generic;
using System.Text;

namespace OrangeToDo_Server
{
    class Scheduler
    {
        private LinkedList<Task> _tasks;

        private LinkedList<LinkedList<Task>> _schedule;

        public LinkedList<Task> Tasks
        {
            get
            {
                return _tasks;
            }
            private set
            {
                _tasks = value;
            }
        }

        public LinkedList<LinkedList<Task>> Schedule
        {
            get
            {
                return _schedule;
            }
            private set
            {
                _schedule = value;
            }
        }

        public Scheduler()
        {
            Tasks = new LinkedList<Task>();
        }

        public Scheduler(IEnumerable<Task> tasks)
        {
            Add(tasks);
        }

        //Add a task while maintaining the order by StartDateTime
        public LinkedListNode<Task> Add(Task task)
        {
            if( !(Tasks is LinkedList<Task>) )
            {
                Tasks = new LinkedList<Task>();
                return Tasks.AddFirst(task);
            }
            //if there is no task in the LinkedList
            if(Tasks.Count==0)
            {
                return Tasks.AddFirst(task);
            }

            //otherwise add this task to its right position to 
            //maintain LinkedList ordered by StartDateTime
            LinkedListNode<Task> cur = Tasks.First;
            while(cur is LinkedListNode<Task> && 
                cur.Value.StartDateTime <= task.StartDateTime)
            {
                cur = cur.Next;
            }

            if(cur is LinkedListNode<Task>)
            {
                return Tasks.AddBefore(cur, task);
            }

            return Tasks.AddLast(task);

        }

        //Add a group of tasks by using IEnumerator while maintaining the order
        public void Add(IEnumerable<Task> tasks)
        {
            var cur = tasks.GetEnumerator();

            while(cur.MoveNext())
            {
                Add(cur.Current);
            }
        }

        private void Arrange()
        {
            foreach(var task in Tasks)
            {
                var schedule = Schedule.First;
                while(schedule is LinkedListNode<LinkedList<Task>>)
                {
                    //if there is no conflict between the last task in this list
                    //and the current task, add this task to this list.
                    if(schedule.Value.Last.Value.DeadLine<=task.StartDateTime)
                    {
                        schedule.Value.AddLast(task);
                        goto NextTask;
                    }
                }

                //if this task cannot be put into every existing list,
                //create a new list to hold it.
                LinkedList<Task> newList = new LinkedList<Task>();
                newList.AddFirst(task);
                Schedule.AddLast(newList);

            NextTask:
                continue;
            }
        }
    }
}
