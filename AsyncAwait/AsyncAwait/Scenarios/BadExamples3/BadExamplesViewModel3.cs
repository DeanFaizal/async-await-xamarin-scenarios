using AsyncAwait.Services;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            var taskCount = 100;

            Task[] longRunningTasks = new Task[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                //Fire and forget long running task using Task.Run 
                //For example purposes only. Real fire and forget tasks a way to be stopped.
                Task.Run(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(millisecondsTimeout: (int)TimeSpan.FromSeconds(1).TotalMilliseconds);
                        PrintDot();
                    }
                });
            }

            //Then run normal Task.Runs
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

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var taskCount = 100;

            Task[] longRunningTasks = new Task[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                //Fire and forget long running task using Task.Factory.StartNew(()=>{}, TaskCreationOptions.LongRunning)
                //For example purposes only. Real fire and forget tasks a way to be stopped.
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(millisecondsTimeout: (int)TimeSpan.FromSeconds(1).TotalMilliseconds);
                        PrintDot();
                    }
                }, creationOptions: TaskCreationOptions.LongRunning);
            }

            //Then run normal Task.Runs
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
