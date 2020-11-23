using DbUp;
using DbUp.Helpers;
using DbUp.ScriptProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLMigration
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                return ReturnError(
                    "Invalid args. You have to specify connection string");
            }

            var connectionString = args[0];
            var scriptsPath = ".\\Scripts";

            Console.WriteLine("Start executing predeployment scripts..."+ Path.Combine(scriptsPath, "PreDeployment"));
            string preDeploymentScriptsPath = Path.Combine(scriptsPath, "PreDeployment");
            var preDeploymentScriptsExecutor =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsFromFileSystem(preDeploymentScriptsPath, new FileSystemScriptOptions
                    {
                        IncludeSubDirectories = true
                    })
                    .LogToConsole()
                    .JournalTo(new NullJournal())
                    .Build();

            var preDeploymentUpgradeResult = preDeploymentScriptsExecutor.PerformUpgrade();

            if (!preDeploymentUpgradeResult.Successful)
            {
                return ReturnError(preDeploymentUpgradeResult.Error.ToString());
            }

            ShowSuccess();

            Console.WriteLine("Start executing migration scripts..."+ Path.Combine(scriptsPath, "Migrations"));
            var migrationScriptsPath = Path.Combine(scriptsPath, "Migrations");
            var upgrader =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsFromFileSystem(migrationScriptsPath, new FileSystemScriptOptions
                    {
                        IncludeSubDirectories = true
                    })
                    .LogToConsole()
                    .JournalToSqlTable("dbo", "MigrationsJournal")
                    .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                return ReturnError(result.Error.ToString());
            }

            ShowSuccess();

            Console.WriteLine("Start executing postdeployment scripts..."+ Path.Combine(scriptsPath, "PostDeployment"));
            string postdeploymentScriptsPath = Path.Combine(scriptsPath, "PostDeployment");
            var postDeploymentScriptsExecutor =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsFromFileSystem(postdeploymentScriptsPath, new FileSystemScriptOptions
                    {
                        IncludeSubDirectories = true
                    })
                    .LogToConsole()
                    .JournalTo(new NullJournal())
                    .Build();

            var postdeploymentUpgradeResult = postDeploymentScriptsExecutor.PerformUpgrade();

            if (!postdeploymentUpgradeResult.Successful)
            {
                return ReturnError(result.Error.ToString());
            }

            ShowSuccess();

            return 0;
        }

        private static void ShowSuccess()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
        }

        private static int ReturnError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
            return -1;
        }
    }
}
