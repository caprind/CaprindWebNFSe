using Microsoft.EntityFrameworkCore;
using NFSe2026.API.Data;

namespace NFSe2026.API;

// Classe para testar conexão com o banco de dados
// Execute: dotnet run --project NFSe2026.API -- test-connection
public class TestConnection
{
    public static async Task TestDatabaseConnection(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            Console.WriteLine("============================================");
            Console.WriteLine("Testando Conexão com Banco de Dados MySQL");
            Console.WriteLine("============================================\n");

            // Teste 1: Verificar se consegue conectar
            Console.WriteLine("1. Testando conexão...");
            var canConnect = await dbContext.Database.CanConnectAsync();
            Console.WriteLine($"   ✓ Conexão: {(canConnect ? "SUCESSO" : "FALHA")}\n");

            if (!canConnect)
            {
                Console.WriteLine("❌ Não foi possível conectar ao banco de dados.");
                Console.WriteLine("   Verifique:");
                Console.WriteLine("   - Se o MySQL está rodando");
                Console.WriteLine("   - Se a connection string está correta");
                Console.WriteLine("   - Se o banco de dados existe");
                return;
            }

            // Teste 2: Verificar se o banco existe e está acessível
            Console.WriteLine("2. Verificando banco de dados...");
            var dbName = dbContext.Database.GetDbConnection().Database;
            Console.WriteLine($"   ✓ Banco de dados: {dbName}\n");

            // Teste 3: Verificar se as tabelas existem
            Console.WriteLine("3. Verificando tabelas...");
            var tables = new[]
            {
                "Empresas", "Usuarios", "Tomadores",
                "NotasFiscais", "ItensServico", "ConfiguracoesAPI"
            };

            foreach (var table in tables)
            {
                // Método seguro: usar apenas DbSet (protege contra SQL injection)
                try
                {
                    var count = table switch
                    {
                        "Empresas" => await dbContext.Empresas.CountAsync(),
                        "Usuarios" => await dbContext.Usuarios.CountAsync(),
                        "Tomadores" => await dbContext.Tomadores.CountAsync(),
                        "NotasFiscais" => await dbContext.NotasFiscais.CountAsync(),
                        "ItensServico" => await dbContext.ItensServico.CountAsync(),
                        "ConfiguracoesAPI" => await dbContext.ConfiguracoesAPI.CountAsync(),
                        _ => -1
                    };
                    
                    if (count >= 0)
                    {
                        Console.WriteLine($"   ✓ Tabela '{table}': Existe ({count} registros)");
                    }
                    else
                    {
                        Console.WriteLine($"   ✗ Tabela '{table}': Não encontrada");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ✗ Tabela '{table}': Erro - {ex.Message}");
                }
            }
            Console.WriteLine();

            // Teste 4: Verificar configurações iniciais
            Console.WriteLine("4. Verificando configurações iniciais...");
            try
            {
                var configCount = await dbContext.ConfiguracoesAPI.CountAsync();
                Console.WriteLine($"   ✓ Configurações API encontradas: {configCount}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠ Configurações não encontradas: {ex.Message}\n");
            }

            Console.WriteLine("============================================");
            Console.WriteLine("✓ Todos os testes concluídos!");
            Console.WriteLine("============================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n❌ Erro ao testar conexão:");
            Console.WriteLine($"   {ex.Message}\n");
            Console.WriteLine("Detalhes:");
            Console.WriteLine($"   Tipo: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Erro interno: {ex.InnerException.Message}");
            }
        }
    }
}

