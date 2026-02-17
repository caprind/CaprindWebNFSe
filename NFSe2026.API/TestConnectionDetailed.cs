using Microsoft.EntityFrameworkCore;
using NFSe2026.API.Data;
using MySqlConnector;

namespace NFSe2026.API;

public class TestConnectionDetailed
{
    public static async Task TestAsync(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine("TESTE DETALHADO DE CONEXÃO MySQL");
        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine();
        
        Console.WriteLine($"Connection String: {connectionString?.Replace("Password=", "Password=***").Replace(";", ";\n")}");
        Console.WriteLine();
        
        // Teste 1: Conexão direta com MySqlConnection
        Console.WriteLine("TESTE 1: Conexão direta com MySqlConnection");
        Console.WriteLine("-".PadRight(60, '-'));
        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            Console.WriteLine("✅ SUCESSO: Conexão direta funcionou!");
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT VERSION(), DATABASE(), USER(), NOW()";
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                Console.WriteLine($"   MySQL Version: {reader[0]}");
                Console.WriteLine($"   Database: {reader[1]}");
                Console.WriteLine($"   User: {reader[2]}");
                Console.WriteLine($"   Server Time: {reader[3]}");
            }
            await connection.CloseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERRO na conexão direta: {ex.Message}");
            Console.WriteLine($"   Tipo: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
        }
        Console.WriteLine();
        
        // Teste 2: Teste com diferentes modos SSL
        var sslModes = new[] { "None", "Preferred", "Required" };
        foreach (var sslMode in sslModes)
        {
            Console.WriteLine($"TESTE 2.{Array.IndexOf(sslModes, sslMode) + 1}: Teste com SslMode={sslMode}");
            Console.WriteLine("-".PadRight(60, '-'));
            
            var testConnectionString = connectionString?.Replace("SslMode=Preferred", $"SslMode={sslMode}");
            if (!testConnectionString!.Contains("SslMode="))
            {
                testConnectionString = testConnectionString.TrimEnd(';') + $";SslMode={sslMode};";
            }
            
            try
            {
                using var connection = new MySqlConnection(testConnectionString);
                await connection.OpenAsync();
                Console.WriteLine($"✅ SUCESSO com SslMode={sslMode}!");
                await connection.CloseAsync();
                break; // Se funcionou, não precisa testar os outros
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO com SslMode={sslMode}: {ex.Message}");
            }
            Console.WriteLine();
        }
        
        // Teste 3: Teste com DbContext
        Console.WriteLine("TESTE 3: Teste com Entity Framework Core");
        Console.WriteLine("-".PadRight(60, '-'));
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var serverVersion = ServerVersion.Parse("8.0.0-mysql");
            optionsBuilder.UseMySql(connectionString, serverVersion);
            
            using var context = new ApplicationDbContext(optionsBuilder.Options);
            var canConnect = await context.Database.CanConnectAsync();
            if (canConnect)
            {
                Console.WriteLine("✅ SUCESSO: DbContext conseguiu conectar!");
                
                // Teste de query simples - verificar banco
                var connection = context.Database.GetDbConnection();
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT DATABASE(), USER()";
                var result = await command.ExecuteScalarAsync();
                Console.WriteLine($"✅ Banco atual: {result}");
                await connection.CloseAsync();
            }
            else
            {
                Console.WriteLine("❌ DbContext não conseguiu conectar (CanConnect retornou false)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERRO no DbContext: {ex.Message}");
            Console.WriteLine($"   Tipo: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
        }
        Console.WriteLine();
        
        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine("FIM DOS TESTES");
        Console.WriteLine("=".PadRight(60, '='));
    }
}

