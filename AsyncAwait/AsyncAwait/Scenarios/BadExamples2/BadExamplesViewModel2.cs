using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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

        private bool _isListViewVisible;
        public bool IsListViewVisible
        {
            get => _isListViewVisible;
            set => SetProperty(ref _isListViewVisible, value);
        }

        private ObservableCollection<string> _listItems;
        public ObservableCollection<string> ListItems
        {
            get => _listItems;
            set => SetProperty(ref _listItems, value);
        }

        private AsyncCommand _badViewModelUpdateCommand;
        public AsyncCommand BadViewModelUpdateCommand => _badViewModelUpdateCommand ?? (_badViewModelUpdateCommand = new AsyncCommand(async () =>
        {
            IsListViewVisible = true;
            //From background thread
            ClearStatus();
            PrintStatus("Command starting");

            PrintThreadCheck();
            ListItems = new ObservableCollection<string> { "Item 1", "Item 2" };
            var taskResult = await TaskService.GetStringWithTaskRunAsync().ConfigureAwait(false);
            PrintThreadCheck();
            PrintStatus("Adding item to ListView");
            ListItems.Add(taskResult);
            PrintStatus("Added item to ListView");

            PrintStatus("Command ending");
            Thread.Sleep((int)TimeSpan.FromSeconds(1).TotalMilliseconds);
            IsListViewVisible = false;
        }));

        private AsyncCommand _goodViewModelUpdateCommand;
        public AsyncCommand GoodViewModelUpdateCommand => _goodViewModelUpdateCommand ?? (_goodViewModelUpdateCommand = new AsyncCommand(async () =>
        {
            IsListViewVisible = true;
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
                PrintStatus("Adding item to ListView");
                ListItems.Add(taskResult);
                PrintStatus("Added item to ListView");
            });

            PrintStatus("Command ending");
            Thread.Sleep((int)TimeSpan.FromSeconds(1).TotalMilliseconds);
            IsListViewVisible = false;
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

                TaskService.GetFireAndForgetTask(taskName: "TaskWithException",
                    delaySeconds: 2,
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

            TaskService.GetFireAndForgetTask(taskName: "TaskWithException",
                delaySeconds: 2,
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

            var longRunningTask = TaskService.GetStringWithTaskRunAsync("60 second Task",
                   delaySeconds: 60,
                   taskResult: "TaskResult");
            var tasks = new Task[] { longRunningTask };

            var timeoutSeconds = 3;
            PrintStatus($"Task will timeout in {timeoutSeconds} seconds..");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Task.WaitAll(tasks, timeout: TimeSpan.FromSeconds(timeoutSeconds));

            stopwatch.Stop();
            PrintStatus($"Command ending after {stopwatch.Elapsed.TotalSeconds}s");
        }));


        private AsyncCommand _goodTimeoutCommand;
        public AsyncCommand GoodTimeoutCommand => _goodTimeoutCommand ?? (_goodTimeoutCommand = new AsyncCommand(async () =>
        {
            ClearStatus();
            PrintStatus("Command starting");

            var longRunningTaskCompletionSource = new TaskCompletionSource<string>();
            var timeoutSeconds = 3;
            var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(timeoutSeconds));
            var stopwatch = new Stopwatch();

            try
            {
                PrintStatus($"Task will timeout in {timeoutSeconds} seconds..");

                stopwatch.Start();
                await TaskService.GetStringWithTaskRunAsync("60 second Task",
                    delaySeconds: 60,
                    taskResult: "TaskResult",
                    cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                PrintStatus($"An exception occurred: {ex.Message}");
            }
            finally
            {
                stopwatch.Stop();
                PrintStatus($"Command ending after {stopwatch.Elapsed.TotalSeconds}s");
            }
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

            var taskCount = 10;
            Task<string>[] tasks = new Task<string>[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                var currentException = $"Exception{i}";
                Action exceptionAction = () => throw new Exception(currentException);

                var task = TaskService.GetStringWithTaskCompletionSourceTheWrongWay($"Task {i}",
                    delaySeconds: i,
                    taskResult: $"Task{i}Result",
                    cancellationToken: default,
                    taskAction: exceptionAction);
                tasks[i] = task;
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                PrintStatus("Catch(Exception ex) only exposes the first exception.");
                PrintStatus($"An exception occurred: {ex.Message}");

                PrintStatus("Get all the exceptions from the tasks collection");
                var exceptions = tasks.Where(a => a.Exception != null)
                    .Select(a => a.Exception);
                foreach (var exception in exceptions)
                {
                    PrintStatus($"{exception.Message}");
                }
            }

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

            var taskCount = 10;
            Task<string>[] tasks = new Task<string>[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                var currentException = $"Exception{i}";
                Action exceptionAction = () => throw new Exception(currentException);

                var task = TaskService.GetStringWithTaskCompletionSource($"Task {i}",
                    delaySeconds: i,
                    taskResult: $"Task{i}Result",
                    cancellationToken: default,
                    taskAction: exceptionAction);
                tasks[i] = task;
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                PrintStatus("Catch(Exception ex) only exposes the first exception.");
                PrintStatus($"An exception occurred: {ex.Message}");

                PrintStatus("Get all the exceptions from the tasks collection");
                var exceptions = tasks.Where(a => a.Exception != null)
                    .Select(a => a.Exception);
                foreach (var exception in exceptions)
                {
                    PrintStatus($"{exception.Message}");
                }
            }

            stopwatch.Stop();
            PrintStatus($"Command ending after: {stopwatch.Elapsed.TotalSeconds}s");
        }));

        #endregion
    }
}