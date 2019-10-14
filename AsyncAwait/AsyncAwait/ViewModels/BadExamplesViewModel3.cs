using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAwait.ViewModels
{
    public class BadExamplesViewModel3 : BaseViewModel
    {
        #region Example: Not using longrunning tasks

        private AsyncCommand _badLongRunningTaskCommand;
        public AsyncCommand BadLongRunningTaskCommand => _badLongRunningTaskCommand ?? (_badLongRunningTaskCommand = new AsyncCommand(async () =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var taskCount = 1000;

            Task[] longRunningTasks = new Task[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                //Fire and forget long running task
                Task.Run(() =>
                {
                    PrintDot();
                    Thread.Sleep(millisecondsTimeout: (int)TimeSpan.FromDays(10).TotalMilliseconds);
                });
            }

            Task<string>[] tasks = new Task<string>[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                var task = TaskService.GetStringWithTaskRunAsync($"Task {i}");
                tasks[i] = task;
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            PrintStatus($"Command ending after: {stopwatch.Elapsed.TotalSeconds}s");

            PrintStatus("Command ending");
        }));


        private AsyncCommand _goodLongRunningTaskCommand;
        public AsyncCommand GoodLongRunningTaskCommand => _goodLongRunningTaskCommand ?? (_goodLongRunningTaskCommand = new AsyncCommand(async () =>
        {
            ClearStatus();
            PrintStatus("Command starting");


            ClearStatus();
            PrintStatus("Command starting");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var taskCount = 1000;

            Task[] longRunningTasks = new Task[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                //Fire and forget long running task
                Task.Factory.StartNew(() =>
                {
                    PrintDot();
                    Thread.Sleep(millisecondsTimeout: (int)TimeSpan.FromDays(10).TotalMilliseconds);
                }, creationOptions: TaskCreationOptions.LongRunning);
            }

            Task<string>[] tasks = new Task<string>[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                var task = TaskService.GetStringWithTaskRunAsync($"Task {i}");
                tasks[i] = task;
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            PrintStatus($"Command ending after: {stopwatch.Elapsed.TotalSeconds}s");
        }));

        #endregion
    }
}
