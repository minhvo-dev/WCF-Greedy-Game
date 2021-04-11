/** Author:     Vo, Dinh Tue Minh
 *  Date:       March 25, 2021
 *  Purpose:    Implement service host for Greedy Game application
 *              Settings are stored in the App.config file
 */

using System;
using System.ServiceModel;
using GreedyGameLibrary;

namespace GreedyGameService
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost serviceHost = null;
            try
            {
                // create the service host
                serviceHost = new ServiceHost(typeof(GreedyGame));

                // start the service
                serviceHost.Open();
                Console.WriteLine("Service started. Press any key to quit.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ReadKey();
                serviceHost?.Close();
            }
        }
    }
}
