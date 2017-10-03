using Sentry.data.Core;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sentry.data.DatasetLoader
{
    class ThreadProgram
    {

        

        public static void Main()
        {
            Sentry.data.Infrastructure.Bootstrapper.Init();

            Task<Double>[] taskArray = { Task<Double>.Factory.StartNew(() => DoWork(100.00)),
                                     Task<Double>.Factory.StartNew(() => DoComputation(100.0)),
                                     Task<Double>.Factory.StartNew(() => DoComputation(1000.0)) };

            var results = new Double[taskArray.Length];
            Double sum = 0;

            for (int i = 0; i < taskArray.Length; i++)
            {
                results[i] = taskArray[i].Result;
                Console.Write("{0:N1} {1}", results[i],
                                  i == taskArray.Length - 1 ? "= " : "+ ");
                sum += results[i];
            }
            Console.WriteLine("{0:N1}", sum);
            Console.ReadKey();
        
        }

        private static Double DoComputation(Double start)
        {
            Double sum = 0;
            for (var value = start; value <= start + 10; value += .1)
                sum += value;

            return sum;
        }

        private static Double DoWork(Double input)
        {
            IContainer container;

            using (container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
            {
                var upload = container.GetInstance<IDatasetService>();
                var dscontext = container.GetInstance<IDatasetContext>();
            }
                return input;
        }
    }
}
