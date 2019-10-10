using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AsyncAwait.ViewModels
{
    public class SynchronousAsyncViewModel : BaseViewModel
    {

        public void Example1()
        {
            //Task dependent method:
            //TaskService.DoWorkAsync();


        }
        public void Examplex()
        {
            //var result = TaskService.DoWorkWithResultAsync(expectedResult: "Result").GetAwaiter().GetResult();
        }

        public void Example2()
        {
            //var result = TaskService.DoWorkWithResultAsync(expectedResult: "Result").GetAwaiter().GetResult();

        }
    }
}
