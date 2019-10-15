using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncAwait.ViewModels
{
    public class WelcomePageViewModel : BaseViewModel
    {
        private string _text =
            @"Welcome to Async Await Scenarios in Xamarin!
Use the main menu to get started.
Scenarios to explore:
===
Bad examples and how to fix them
===
Bad Examples 1:
- Invoking tasks
- Awaiting multiple tasks
- Returning tasks
- Threading

Bad Examples 2:
- Updating VM properties
- Handling exceptions
- Timing out tasks
- Using TaskCompletionSource

Bad Examples 3:
- Using LongRunningTask

Other samples:
- Executing tasks:
    - The various ways to execute a task without awaiting
- Awaiting tasks:
    - How to await tasks properly
- Configure await:
    - Using configure await
- Awaiting multiple tasks

Resources:
Any Stephen Toub blog post: https://devblogs.microsoft.com/dotnet/author/toub/
Any Stephen Cleary blog post: https://blog.stephencleary.com/
Brandon Minnick's Async/Await talk: https://www.youtube.com/watch?v=J0mcYVxJEl0
";
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
    }
}
