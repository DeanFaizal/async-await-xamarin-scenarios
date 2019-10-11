using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AsyncAwait.ViewModels
{
    public class AwaitingTasksViewModel : BaseViewModel
    {
        private AsyncCommand _awaitNewTaskCommand;
        public AsyncCommand AwaitNewTaskCommand => _awaitNewTaskCommand ?? (_awaitNewTaskCommand = new AsyncCommand(async () =>
        {
            ClearStatus();
            Status += "Command invoked\n";
            var result = await TaskService.GetStringWithNewTaskAsync("New Task");
            Status += "Command returning\n";
        }));
        
        private AsyncCommand _awaitTaskRunCommand;
        public AsyncCommand AwaitTaskRunCommand => _awaitTaskRunCommand ?? (_awaitTaskRunCommand = new AsyncCommand(async () =>
        {
            ClearStatus();
            Status += "Command invoked\n";
            var result = await TaskService.GetStringWithTaskRunAsync("Task.Run");
            Status += "Command returning\n";
        }));
        
        private Command _noAwaitTaskRunCommand;
        public Command NoAwaitTaskRunCommand => _noAwaitTaskRunCommand ?? (_noAwaitTaskRunCommand = new Command(() =>
        {
            ClearStatus();
            Status += "Command invoked\n";
            var result = TaskService.AwaitStringWithTaskRunAsync("No await on Task.Run"); //AwaitStringWithTaskRunAsync awaits internally
            Status += $"Command completed with result: {result}\n";
        }));
        
        private Command _taskRunResultCommand;
        public Command TaskRunResultCommand => _taskRunResultCommand ?? (_taskRunResultCommand = new Command(() =>
        {
            ClearStatus();
            Status += "Command invoked\n";
            var result = TaskService.AwaitStringWithTaskRunAsync("Task.Run.Result").Result;//AwaitStringWithTaskRunAsync awaits internally
            Status += $"Command completed with result: {result}\n";
        }));
    }
}