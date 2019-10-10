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
1. Synchronous Tasks. 
2. Async and await.
3. Configure await.
4. Awaiting multiple Tasks.
5. Asyncifying methods.
6. Exception handling.
7. Long running Tasks.
8. ValueTasks.

. Resources.
";
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
    }
}
