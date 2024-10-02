using System;
using log4net;

namespace ChatAppWcfService
{
    public class CalculatorService : ICalculator
    {
        // Initialize log4net logger
        private static readonly ILog _log = LogManager.GetLogger(typeof(CalculatorService));

        public double Add(double n1, double n2)
        {
            double result = n1 + n2;

            // Console logging
            Console.WriteLine($"Received Add({n1},{n2})");
            Console.WriteLine($"Return: {result}");

            // log4net logging
            _log.Info($"Received Add({n1},{n2})");
            _log.Info($"Return: {result}");

            return result;
        }

        public double Subtract(double n1, double n2)
        {
            double result = n1 - n2;

            // Console logging
            Console.WriteLine($"Received Subtract({n1},{n2})");
            Console.WriteLine($"Return: {result}");

            // log4net logging
            _log.Info($"Received Subtract({n1},{n2})");
            _log.Info($"Return: {result}");

            return result;
        }

        public double Multiply(double n1, double n2)
        {
            double result = n1 * n2;

            // Console logging
            Console.WriteLine($"Received Multiply({n1},{n2})");
            Console.WriteLine($"Return: {result}");

            // log4net logging
            _log.Info($"Received Multiply({n1},{n2})");
            _log.Info($"Return: {result}");

            return result;
        }

        public double Divide(double n1, double n2)
        {
            double result = n1 / n2;

            // Console logging
            Console.WriteLine($"Received Divide({n1},{n2})");
            Console.WriteLine($"Return: {result}");

            // log4net logging
            _log.Info($"Received Divide({n1},{n2})");
            _log.Info($"Return: {result}");

            return result;
        }
    }
}
