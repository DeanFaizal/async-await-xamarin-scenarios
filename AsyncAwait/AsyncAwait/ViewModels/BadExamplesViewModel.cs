using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAwait.ViewModels
{
    public class BadExamplesViewModel : BaseViewModel
    {
        #region Example: Not awaiting Task

        private Command _badTaskInvokeCommand;
        public Command BadTaskInvokeCommand => _badTaskInvokeCommand ?? (_badTaskInvokeCommand = new Command(() =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            var taskResult = TaskService.GetStringWithTaskRunAsync("Using Task.Result").Result;
            PrintStatus("Task Completed with result: {taskResult}");

            PrintStatus("Command ending");
        }));

        private AsyncCommand _goodTaskInvokeCommand;
        public AsyncCommand GoodTaskInvokeCommand => _goodTaskInvokeCommand ?? (_goodTaskInvokeCommand = new AsyncCommand(async () =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            var taskResult = await TaskService.GetStringWithTaskRunAsync("Using await");

            PrintStatus("Command ending");
        }));

        #endregion



        #region Example: Multiple task execution

        private AsyncCommand _badMultipleTaskExecutionCommand;
        public AsyncCommand BadMultipleTaskExecutionCommand => _badMultipleTaskExecutionCommand ?? (_badMultipleTaskExecutionCommand = new AsyncCommand(async () =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var taskCount = 5;
            for (int i = 0; i < taskCount; i++)
            {
                await TaskService.GetStringWithTaskRunAsync($"Task {i}");
            }

            stopwatch.Stop();

            PrintStatus($"Command ending after: {stopwatch.Elapsed.TotalSeconds}s");
        }));

        private AsyncCommand _goodMultipleTaskExecutionCommand;
        public AsyncCommand GoodMultipleTaskExecutionCommand => _goodMultipleTaskExecutionCommand ?? (_goodMultipleTaskExecutionCommand = new AsyncCommand(async () =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var taskCount = 5;
            var tasks = new Task<string>[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = TaskService.GetStringWithTaskRunAsync($"Task {i}");
            }

            await Task.WhenAll(tasks);

            stopwatch.Stop();

            PrintStatus($"Command ending after: {stopwatch.Elapsed.TotalSeconds}s");
        }));

        #endregion



        #region Example: Not returning Task

        private AsyncCommand _badTaskReturnCommand;
        public AsyncCommand BadTaskReturnCommand => _badTaskReturnCommand ?? (_badTaskReturnCommand = new AsyncCommand(async () =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var taskCount = 1000;
            Task<string>[] tasks = new Task<string>[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                var task = TaskService.AwaitStringWithTaskRunAsync($"Task {i}");
                tasks[i] = task;
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            PrintStatus($"Command ending after: {stopwatch.Elapsed.TotalSeconds}s");
        }));

        private AsyncCommand _goodTaskReturnCommand;
        public AsyncCommand GoodTaskReturnCommand => _goodTaskReturnCommand ?? (_goodTaskReturnCommand = new AsyncCommand(async () =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var taskCount = 1000;
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



        #region Example: Not using configure await false

        private AsyncCommand _badConsecutiveOperations;
        public AsyncCommand BadConsecutiveOperations => _badConsecutiveOperations ?? (_badConsecutiveOperations = new AsyncCommand(async () =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            PrintThreadCheck();
            await TaskService.GetStringWithTaskRunAsync();
            PrintThreadCheck();

            //Simulate CPU intensive operation
            Thread.Sleep((int)TimeSpan.FromSeconds(5).TotalMilliseconds);
          
            PrintStatus("Command ending");
        }));

        private AsyncCommand _goodConsecutiveOperations;
        public AsyncCommand GoodConsecutiveOperations => _goodConsecutiveOperations ?? (_goodConsecutiveOperations = new AsyncCommand(async () =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            PrintThreadCheck();
            await TaskService.GetStringWithTaskRunAsync().ConfigureAwait(false);
            PrintThreadCheck();

            //Simulate CPU intensive operation. Eg: Deserializing a lot of JSON
            Thread.Sleep((int)TimeSpan.FromSeconds(5).TotalMilliseconds);

            PrintStatus("Command ending");
        }));

        #endregion
    }
}
