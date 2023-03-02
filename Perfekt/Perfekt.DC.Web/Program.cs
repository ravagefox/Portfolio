using Perfekt.Core;
using Perfekt.Core.IO.XML;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("----------------------------------------------");
        Console.WriteLine(" WELCOME TO Perfekt HTTP Data Collection Agent");
        Console.WriteLine($" Version: {typeof(Program).Assembly.GetName().Version}");
        Console.WriteLine("----------------------------------------------");

        var configDir = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory, "Configs");
        var taskList = new List<Task>();

        Console.WriteLine("Discovering agents...");

        foreach (var configFile in Directory.EnumerateFiles(configDir, "*.xml", SearchOption.TopDirectoryOnly))
        {
            using (var fr = File.OpenRead(configFile))
            {
                var config = OpenConfig(fr);
                Console.WriteLine($"Discovered agent: {config.Host}");

                var task = CreateTask(config);
                if (task != Task.CompletedTask) { taskList.Add(task); }
            }
        }

        Console.WriteLine($"Collecting data from {taskList.Count} agent(s)");
        Console.WriteLine($"If this number is wrong, please reconfigure the configuration, it may be disabled.");

        taskList.ForEach(t => t.Start());
        Task.WaitAll(taskList.ToArray());
    }

    private static Configuration OpenConfig(Stream stream)
    {
        using (var objParser = new XmlParser(stream))
        {
            var obj = objParser.CreateInstance(objParser.Current);

            var f = BindingFlags.Public | BindingFlags.Instance;
            var properties = obj.GetType().GetProperties(f);

            while (objParser.Read())
            {
                var n = objParser.Current;
                if (n.NodeType == System.Xml.XmlNodeType.Element)
                {
                    var propertyName = n.Name;
                    if (properties.FirstOrDefault(
                        x => x.PropertyType.Name.SequenceEqual(n.Name) ||
                        x.Name.SequenceEqual(n.Name)) is PropertyInfo inf)
                    {
                        object objValue;
                        if (inf.PropertyType.IsValueType && !inf.PropertyType.IsPrimitive)
                        {
                            objValue = objParser.ReadValueType(n);
                        }
                        else
                        {
                            objValue = Convert.ChangeType(n.InnerText, inf.PropertyType);
                        }

                        inf.SetValue(obj, objValue);
                    }
                }
            }

            return (Configuration)obj;
        }
    }

    private static Task CreateTask(Configuration config)
    {
        if (!config.Enabled) { return Task.CompletedTask; }

        var asmPath = config.AssemblyPath;
        if (string.IsNullOrEmpty(Path.GetDirectoryName(asmPath)))
        {
            asmPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory, asmPath);
        }

        if (!File.Exists(asmPath)) { return Task.CompletedTask; }

        var f = BindingFlags.Public | BindingFlags.Static;

        var dll = Assembly.LoadFrom(asmPath);
        var entryClient = dll.GetExportedTypes().SelectMany(t => t.GetMethods(f).Where(f => f.GetCustomAttribute<MainEntryAttribute>() != null));
        if (entryClient.Any() && entryClient.FirstOrDefault() is MethodInfo entryPoint)
        {
            var @delegate = (Action<object?>)Delegate.CreateDelegate(typeof(Action<object?>), entryPoint);
            return new Task(@delegate, config, new CancellationTokenSource().Token, TaskCreationOptions.LongRunning);
        }

        return Task.CompletedTask;
    }
}