using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Diagnostics;


Console.WriteLine("Escolha a opção:");
Console.WriteLine("1. Backup de PostgreSQL");
Console.WriteLine("2. Backup de SQL Server");

var escolha = Console.ReadLine();

switch (escolha)
{
    case "1":
        FazerBackupPostgreSQL();
        break;

    case "2":
        FazerBackupSQLServer();
        break;

    default:
        Console.WriteLine("Opção inválida. Por favor, escolha 1 ou 2.");
        break;
}



static void FazerBackupPostgreSQL()
{
    try
    {

        //var connectionString = "User ID=postgres;Password=27101997;Host=localhost;Port=5432;Database=dbControlCompany;";
        var backupPath = @"C:\temp\";
        string fullPath = System.IO.Path.Combine(backupPath, $"Backup_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.bak");
        string host = "localhost";
        string port = "5432";
        string database = "dbControlCompany";
        string username = "postgres";
        string password = "27101997";
        var programFile = Uri.EscapeDataString("Arquivos de Programas");
        string pgDumpCommand = $@"C:\{programFile}\PostgreSQL\15\bin\pg_dump.exe --host {host} --port {port} --username {username} --password {password} --format custom --file ""{fullPath}"" {database}";
        string pgDumpPath = @"C:\Program Files\PostgreSQL\15\bin\pg_dump.exe";

        Console.WriteLine("Fazendo backup de PostgreSQL...");

        if (!Directory.Exists(backupPath))
        {
            Directory.CreateDirectory(backupPath);
        }

        Environment.SetEnvironmentVariable("PGPASSWORD", password);

        using (Process process = new Process())
        {
            process.StartInfo.FileName = pgDumpPath;
            process.StartInfo.Arguments = $"-F c -U {username} -h {host} -d {database} -w -f \"{fullPath}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Environment.SetEnvironmentVariable("PGPASSWORD", null);

            if (process.ExitCode == 0)
            {
                Console.WriteLine($"Backup com sucesso: {fullPath}");
            }
            else
            {
                Console.WriteLine($"Error: {error}");
            }
        }

    }
    catch (Exception ex)
    {
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), $@"log-backup-{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt"),
            $@"Data: {DateTime.Now} Erro: {ex.ToString()}");

        Console.WriteLine($"Ocorreu um erro: {ex.Message}");
    }
}

static void FazerBackupSQLServer()
{

    try
    {
        Console.WriteLine("Fazendo backup de SQL Server...");

        string serverName = "SAMUEL\\SQLSERVER2022";
        string databaseName = "Db99";
        string backupPath = @"C:\temp\";
        string backupFileName = $"Backup_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.bak";
        string fullPath = System.IO.Path.Combine(backupPath, $"Backup_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.bak");

        ServerConnection serverConnection = new ServerConnection(serverName);
        Server sqlServer = new Server(serverConnection);

        // Criar um objeto Backup
        Backup backup = new Backup();
        backup.Action = BackupActionType.Database;
        backup.Database = databaseName;
        backup.Devices.AddDevice(fullPath, DeviceType.File);

        // Executar o backup
        backup.SqlBackup(sqlServer);

        Console.WriteLine($"Backup com sucesso: {fullPath}");

    }
    catch (Exception ex)
    {
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), $@"log-backup-{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt"),
            $@"Data: {DateTime.Now} Erro: {ex.ToString()}");

        Console.WriteLine($"Ocorreu um erro: {ex.Message}");
    }
}