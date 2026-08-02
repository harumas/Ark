using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace MathTests
{
    /// <summary>
    /// [Test] を集めて実行するだけの簡易ランナー
    /// </summary>
    internal static class Program
    {
        private static int Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Ark.Mathematics  テスト");

            var fixtures = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.IsClass && type.IsAbstract == false)
                .Where(type => TestMethods(type).Any())
                .OrderBy(type => type.Name)
                .ToArray();

            if (fixtures.Length == 0)
            {
                Console.WriteLine("テストが見つかりません。");
                return 1;
            }

            var passed = 0;
            var failures = new List<string>();

            foreach (var fixture in fixtures)
            {
                Console.WriteLine();
                Console.WriteLine($"── {fixture.Name}");

                foreach (var method in TestMethods(fixture))
                {
                    try
                    {
                        method.Invoke(Activator.CreateInstance(fixture, true), null);

                        passed++;
                        Console.WriteLine($"   OK   {method.Name}");
                    }
                    catch (Exception exception)
                    {
                        var reason = Describe(exception.InnerException ?? exception);

                        failures.Add($"{fixture.Name}.{method.Name}\n       {reason}");
                        Console.WriteLine($"   NG   {method.Name}\n        {reason}");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine(new string('-', 70));

            if (failures.Count == 0)
            {
                Console.WriteLine($"ALL PASSED  ({passed} 件)");
                return 0;
            }

            Console.WriteLine($"FAILED  {failures.Count} 件 / 成功 {passed} 件");
            Console.WriteLine();

            foreach (var failure in failures)
            {
                Console.WriteLine($"  x {failure}");
            }

            return 1;
        }

        private static IEnumerable<MethodInfo> TestMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(method => method.GetCustomAttribute<TestAttribute>() != null)
                .Where(method => method.GetParameters().Length == 0);
        }

        private static string Describe(Exception exception)
        {
            if (exception is NotImplementedException)
            {
                return "未実装 (NotImplementedException)";
            }

            var lines = exception.Message.Replace("\r", "").Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .Take(3);

            return string.Join("  /  ", lines);
        }
    }
}
