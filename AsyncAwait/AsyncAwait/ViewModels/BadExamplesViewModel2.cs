using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Command = MvvmHelpers.Commands.Command;

namespace AsyncAwait.ViewModels
{
    public class BadExamplesViewModel2 : BaseViewModel
    {

        #region Example: Updating VM properties from background thread

        private ObservableCollection<string> _listItems;
        public ObservableCollection<string> ListItems
        {
            get => _listItems;
            set => SetProperty(ref _listItems, value);
        }

        private AsyncCommand _badViewModelUpdateCommand;
        public AsyncCommand BadViewModelUpdateCommand => _badViewModelUpdateCommand ?? (_badViewModelUpdateCommand = new AsyncCommand(async () =>
        {
            //From background thread
            ClearStatus();
            PrintStatus("Command starting");

            PrintThreadCheck();
            ListItems = new ObservableCollection<string> { "Item 1", "Item 2" };
            var taskResult = await TaskService.GetStringWithTaskRunAsync().ConfigureAwait(false);
            PrintThreadCheck();
            ListItems.Add(taskResult);

            PrintStatus("Command ending");
        }));

        private AsyncCommand _goodViewModelUpdateCommand;
        public AsyncCommand GoodViewModelUpdateCommand => _goodViewModelUpdateCommand ?? (_goodViewModelUpdateCommand = new AsyncCommand(async () =>
        {
            //From ui thread
            ClearStatus();
            PrintStatus("Command starting");

            PrintThreadCheck();
            ListItems = new ObservableCollection<string> { "Item 1", "Item 2" };
            var taskResult = await TaskService.GetStringWithTaskRunAsync().ConfigureAwait(false);
            PrintThreadCheck();

            Device.BeginInvokeOnMainThread(() =>
            {
                PrintThreadCheck();
                ListItems.Add(taskResult);
            });

            PrintStatus("Command ending");
        }));

        #endregion



        #region Example: Exception Handling

        private AsyncCommand _badExceptionHandling;
        public AsyncCommand BadExceptionHandling => _badExceptionHandling ?? (_badExceptionHandling = new AsyncCommand(async () =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            try
            {
                Action exceptionAction = () =>
                {
                    throw new Exception("My Exception");
                };

                await TaskService.GetStringWithTaskRunAsync(taskName: "TaskWithException",
                    delaySeconds: 2,
                    taskResult: "TaskResult",
                    cancellationToken: default,
                    taskAction: exceptionAction);
            }
            catch (Exception ex)
            {
                PrintStatus($"Exception occurred: {ex.Message}");
            }

            PrintStatus("Command ending");
        }));

        private AsyncCommand _goodExceptionHandling;
        public AsyncCommand GoodExceptionHandling => _goodExceptionHandling ?? (_goodExceptionHandling = new AsyncCommand(async () =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            Action exceptionAction = () =>
            {
                throw new Exception("My Exception");
            };

            await TaskService.GetStringWithTaskRunAsync(taskName: "TaskWithException",
                delaySeconds: 2,
                taskResult: "TaskResult",
                cancellationToken: default,
                taskAction: exceptionAction)
            .ContinueWith(continuationAction: (task) =>
             {
                 PrintStatus($"Exception occurred: {task.Exception.Message}");
             }, continuationOptions: TaskContinuationOptions.OnlyOnFaulted);

            PrintStatus("Command ending");
        }));

        #endregion



        #region Example: Timing out     

        private Command _badTimeoutCommand;
        public Command BadTimeoutCommand => _badTimeoutCommand ?? (_badTimeoutCommand = new Command(() =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            var longRunningTask = Task.Delay(TimeSpan.FromSeconds(60));
            var tasks = new Task[] { longRunningTask };
            Task.WaitAll(tasks, timeout: TimeSpan.FromSeconds(3));

            PrintStatus("Command ending");
        }));


        private AsyncCommand _goodTimeoutCommand;
        public AsyncCommand GoodTimeoutCommand => _goodTimeoutCommand ?? (_goodTimeoutCommand = new AsyncCommand(async () =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            var longRunningTaskCompletionSource = new TaskCompletionSource<string>();
            var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(3));

            await TaskService.GetStringWithTaskRunAsync("LongRunningTask",
                delaySeconds: 60,
                taskResult: "TaskResult",
                cancellationTokenSource.Token);

            PrintStatus("Command ending");
        }));

        #endregion



        #region Example: Not completing TaskCompletionSource Task

        private AsyncCommand _badTaskCompletionSource;
        public AsyncCommand BadTaskCompletionSource => _badTaskCompletionSource ?? (_badTaskCompletionSource = new AsyncCommand(async () =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Action exceptionAction = () => throw new Exception("My Exception");

            var taskCount = 1000;
            Task<string>[] tasks = new Task<string>[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                var task = TaskService.GetStringWithTaskCompletionSourceTheWrongWay($"Task {i}",
                    delaySeconds: 2,
                    taskResult: $"Task{i}Result",
                    cancellationToken: default,
                    taskAction: exceptionAction);
                tasks[i] = task;
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            PrintStatus($"Command ending after: {stopwatch.Elapsed.TotalSeconds}s");
        }));


        private AsyncCommand _goodTaskCompletionSource;
        public AsyncCommand GoodTaskCompletionSource => _goodTaskCompletionSource ?? (_goodTaskCompletionSource = new AsyncCommand(async () =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Action exceptionAction = () => throw new Exception("My Exception");

            var taskCount = 1000;
            Task<string>[] tasks = new Task<string>[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                var task = TaskService.GetStringWithTaskCompletionSource($"Task {i}",
                    delaySeconds: 2,
                    taskResult: $"Task{i}Result",
                    cancellationToken: default,
                    taskAction: exceptionAction);
                tasks[i] = task;
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            PrintStatus($"Command ending after: {stopwatch.Elapsed.TotalSeconds}s");
        }));

        #endregion



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