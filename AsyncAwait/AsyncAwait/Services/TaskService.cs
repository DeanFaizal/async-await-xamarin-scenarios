using AsyncAwait.Helpers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace AsyncAwait.Services
{
    public class TaskService
    {
        public event EventHandler<TaskStatusChangedEventArgs> TaskCreated;
        public event EventHandler<TaskStatusChangedEventArgs> TaskStarting;
        public event EventHandler<TaskStatusChangedEventArgs> TaskCompleted;
        public event EventHandler<TaskStatusChangedEventArgs> TaskFaulted;
        public event EventHandler<TaskStatusChangedEventArgs> TaskCancelled;

        public void RaiseTaskCreated(Task task, [CallerMemberName] string caller = default)
        {
            var status = $"[Created] Task from {caller}";

            TaskCreated?.Invoke(this, new TaskStatusChangedEventArgs(task, status));
        }

        public void RaiseTaskStarting(Task task, [CallerMemberName] string caller = default)
        {
            var status = $"[Started] Task from {caller} ".AppendMainThreadAlert();
            TaskStarting?.Invoke(this, new TaskStatusChangedEventArgs(task, status));
        }

        public void RaiseTaskCompleted(Task task, string caller = default)
        {
            var status = $"[Completed] Task from {caller}. status: {task.Status}".AppendMainThreadAlert();
            TaskCompleted?.Invoke(this, new TaskStatusChangedEventArgs(task, status));
        }

        public void RaiseTaskFaulted(Task task, [CallerMemberName] string caller = default)
        {
            var status = $"[Faulted] Task from {caller}. status: {task.Status}".AppendMainThreadAlert();
            TaskFaulted?.Invoke(this, new TaskStatusChangedEventArgs(task, status));
        }

        public void RaiseTaskCancelled(Task task, [CallerMemberName] string caller = default)
        {
            var status = $"[Cancelled] Task from {caller}. status: {task.Status}".AppendMainThreadAlert();
            TaskCancelled?.Invoke(this, new TaskStatusChangedEventArgs(task, status));
        }

        private void ConsoleWriteTaskFaulted([CallerMemberName] string caller = default) => Console.WriteLine($"Task from {caller} faulted");

        public Task<string> GetStringWithNewTaskAsync([CallerMemberName] string callerId = default, int delaySeconds = 2, string taskResult = "Task Result")
        {
            Task<string> getStringTask = default;
            getStringTask = new Task<string>(() =>
            {
                RaiseTaskStarting(getStringTask, callerId);
                Thread.Sleep((int)TimeSpan.FromSeconds(delaySeconds).TotalMilliseconds); //Use Thread.Sleep to block the current thread. https://stackoverflow.com/questions/20082221/when-to-use-task-delay-when-to-use-thread-sleep
                return taskResult;
            });

            RaiseTaskCreated(getStringTask, callerId);

            getStringTask.ContinueWith(completedGetStringTask =>
            {
                if (completedGetStringTask.IsCanceled)
                {
                    RaiseTaskCancelled(completedGetStringTask, callerId);
                }
                else if (completedGetStringTask.IsFaulted)
                {
                    RaiseTaskFaulted(completedGetStringTask, callerId);
                }
                else
                {
                    RaiseTaskCompleted(completedGetStringTask, callerId);
                }
            });

            return getStringTask;
        }
        public Task<string> GetStringWithTaskRunAsync([CallerMemberName] string callerId = default, int delaySeconds = 2, string taskResult = "Task Result")
        {
            Task<string> getStringTask = default;
            getStringTask = Task.Run(() =>
            {
                RaiseTaskStarting(getStringTask, callerId);
                Thread.Sleep((int)TimeSpan.FromSeconds(delaySeconds).TotalMilliseconds); //Use Thread.Sleep to block the current thread. https://stackoverflow.com/questions/20082221/when-to-use-task-delay-when-to-use-thread-sleep
                return taskResult;
            });

            RaiseTaskCreated(getStringTask, callerId);

            getStringTask.ContinueWith(completedGetStringTask =>
            {
                if (completedGetStringTask.IsCanceled)
                {
                    RaiseTaskCancelled(completedGetStringTask, callerId);
                }
                else if (completedGetStringTask.IsFaulted)
                {
                    RaiseTaskFaulted(completedGetStringTask, callerId);
                }
                else
                {
                    RaiseTaskCompleted(completedGetStringTask, callerId);
                }
            });

            return getStringTask;
        }

        public async Task<string> AwaitStringWithTaskRunAsync([CallerMemberName] string callerId = default, int delaySeconds = 2, string taskResult = "Task Result")
        {
            return await GetStringWithTaskRunAsync(callerId);
        }
    }
}