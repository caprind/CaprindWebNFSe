using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NFSe2026.API.Data;
using NFSe2026.API.DTOs;
using NFSe2026.API.Models;

namespace NFSe2026.API.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConsultaCNPJService _consultaCNPJService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(
        ApplicationDbContext context,
        IConsultaCNPJService consultaCNPJService,
        IMapper mapper,
        ILogger<AuthService> logger,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _context = context;
        _consultaCNPJService = consultaCNPJService;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<LoginResponseDTO> LoginAsync(LoginDTO loginDto)
    {
        // Usa Select para evitar carregar campos que podem não existir ainda no banco (ClientId, ClientSecret)
        var usuario = await _context.Usuarios
            .Where(u => u.Email == loginDto.Email)
            .Select(u => new
            {
                u.Id,
                u.EmpresaId,
                u.Nome,
                u.Email,
                u.SenhaHash,
                u.Telefone,
                u.Ativo,
                u.EmailValidado,
                u.UltimoAcesso,
                Empresa = new
                {
                    u.Empresa.Id,
                    u.Empresa.CNPJ,
                    u.Empresa.RazaoSocial,
                    u.Empresa.NomeFantasia,
                    u.Empresa.InscricaoEstadual,
                    u.Empresa.InscricaoMunicipal,
                    u.Empresa.Endereco,
                    u.Empresa.Numero,
                    u.Empresa.Complemento,
                    u.Empresa.Bairro,
                    u.Empresa.Cidade,
                    u.Empresa.UF,
                    u.Empresa.CodigoMunicipio,
                    u.Empresa.CEP,
                    u.Empresa.Telefone,
                    u.Empresa.Email,
                    u.Empresa.SituacaoCadastral,
                    u.Empresa.Logotipo,
                    u.Empresa.DataVencimentoCertificado,
                    u.Empresa.CertificadoDigital,
                    // ClientId e ClientSecret não são incluídos aqui para evitar erro se as colunas não existirem no banco
                    // Serão verificados separadamente se necessário
                    u.Empresa.RegimeEspecialTributacao,
                    u.Empresa.OptanteSimplesNacional,
                    u.Empresa.IncentivoFiscal,
                    u.Empresa.Ativo
                }
            })
            .FirstOrDefaultAsync();

        if (usuario == null)
        {
            throw new UnauthorizedAccessException("Email ou senha inválidos");
        }

        if (!BCrypt.Net.BCrypt.Verify(loginDto.Senha, usuario.SenhaHash))
        {
            throw new UnauthorizedAccessException("Email ou senha inválidos");
        }

        // Verifica se o email foi validado
        if (!usuario.EmailValidado)
        {
            throw new UnauthorizedAccessException("Email não validado. Verifique sua caixa de entrada e valide seu email antes de fazer login.");
        }

        if (!usuario.Ativo)
        {
            throw new UnauthorizedAccessException("Usuário inativo");
        }

        // Atualiza último acesso - busca o usuário completo para atualizar
        var usuarioCompleto = await _context.Usuarios.FindAsync(usuario.Id);
        if (usuarioCompleto != null)
        {
            usuarioCompleto.UltimoAcesso = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        var token = GenerateJwtToken(usuario.Id, usuario.EmpresaId, usuario.Email);

        // Mapeia manualmente para evitar problemas com campos que podem não existir
        var usuarioDto = new UsuarioDTO
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Telefone = usuario.Telefone,
            Ativo = usuario.Ativo
        };

        var empresaDto = new EmpresaDTO
        {
            Id = usuario.Empresa.Id,
            CNPJ = usuario.Empresa.CNPJ,
            RazaoSocial = usuario.Empresa.RazaoSocial,
            NomeFantasia = usuario.Empresa.NomeFantasia,
            InscricaoEstadual = usuario.Empresa.InscricaoEstadual,
            InscricaoMunicipal = usuario.Empresa.InscricaoMunicipal,
            Endereco = usuario.Empresa.Endereco,
            Numero = usuario.Empresa.Numero,
            Complemento = usuario.Empresa.Complemento,
            Bairro = usuario.Empresa.Bairro,
            Cidade = usuario.Empresa.Cidade,
            UF = usuario.Empresa.UF,
            CodigoMunicipio = usuario.Empresa.CodigoMunicipio,
            CEP = usuario.Empresa.CEP,
            Telefone = usuario.Empresa.Telefone,
            Email = usuario.Empresa.Email,
            SituacaoCadastral = usuario.Empresa.SituacaoCadastral,
            Logotipo = usuario.Empresa.Logotipo,
            DataVencimentoCertificado = usuario.Empresa.DataVencimentoCertificado,
            TemCertificadoDigital = !string.IsNullOrEmpty(usuario.Empresa.CertificadoDigital),
            TemClientIdSecret = false, // Será verificado separadamente se necessário (após migration)
            RegimeEspecialTributacao = usuario.Empresa.RegimeEspecialTributacao,
            OptanteSimplesNacional = usuario.Empresa.OptanteSimplesNacional,
            IncentivoFiscal = usuario.Empresa.IncentivoFiscal,
            Ativo = usuario.Empresa.Ativo
        };

        return new LoginResponseDTO
        {
            Token = token,
            Usuario = usuarioDto,
            Empresa = empresaDto,
            ExpiraEm = DateTime.UtcNow.AddHours(8) // Token válido por 8 horas
        };
    }

    public async Task<CadastroEmpresaResponseDTO> CadastrarEmpresaAsync(CadastroEmpresaDTO cadastroDto)
    {
        // Remove formatação do CNPJ
        var cnpj = cadastroDto.CNPJ.Replace(".", "").Replace("/", "").Replace("-", "").Trim();

        // Verifica se já existe empresa com este CNPJ
        var empresaExistente = await _context.Empresas
            .FirstOrDefaultAsync(e => e.CNPJ == cnpj);

        if (empresaExistente != null)
        {
            throw new InvalidOperationException("Já existe uma empresa cadastrada com este CNPJ");
        }

        // Verifica se já existe usuário com este email
        var usuarioExistente = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == cadastroDto.Email);

        if (usuarioExistente != null)
        {
            throw new InvalidOperationException("Já existe um usuário cadastrado com este email");
        }

        // Consulta dados do CNPJ na API externa
        var dadosCNPJ = await _consultaCNPJService.ConsultarCNPJAsync(cnpj);

        if (dadosCNPJ == null)
        {
            throw new InvalidOperationException("Não foi possível consultar os dados do CNPJ. Verifique se o CNPJ está correto.");
        }

        // Cria a empresa
        var empresa = new Empresa
        {
            CNPJ = cnpj,
            RazaoSocial = dadosCNPJ.RazaoSocial,
            NomeFantasia = dadosCNPJ.NomeFantasia,
            InscricaoEstadual = dadosCNPJ.InscricaoEstadual,
            InscricaoMunicipal = dadosCNPJ.InscricaoMunicipal,
            SituacaoCadastral = dadosCNPJ.SituacaoCadastral,
            Porte = dadosCNPJ.Porte,
            NaturezaJuridica = dadosCNPJ.NaturezaJuridica,
            DataAbertura = dadosCNPJ.DataAbertura,
            Telefone = dadosCNPJ.Telefone ?? cadastroDto.Telefone,
            Email = dadosCNPJ.Email ?? cadastroDto.Email,
            Logotipo = cadastroDto.Logotipo,
            Ativo = true
        };

        // Preenche endereço
        if (dadosCNPJ.Endereco != null)
        {
            empresa.Endereco = dadosCNPJ.Endereco.Logradouro;
            empresa.Numero = dadosCNPJ.Endereco.Numero;
            empresa.Complemento = dadosCNPJ.Endereco.Complemento;
            empresa.Bairro = dadosCNPJ.Endereco.Bairro;
            empresa.Cidade = dadosCNPJ.Endereco.Cidade;
            empresa.UF = dadosCNPJ.Endereco.UF;
            empresa.CEP = dadosCNPJ.Endereco.CEP;
        }

        _context.Empresas.Add(empresa);
        await _context.SaveChangesAsync();

        // Gera código de validação (6 dígitos)
        var codigoValidacao = new Random().Next(100000, 999999).ToString();
        var dataExpiracao = DateTime.UtcNow.AddHours(24);

        // Cria o usuário (primeiro usuário da empresa) - inicialmente inativo até validação
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(cadastroDto.Senha);
        var usuario = new Usuario
        {
            EmpresaId = empresa.Id,
            Nome = cadastroDto.Nome,
            Email = cadastroDto.Email,
            SenhaHash = senhaHash,
            Telefone = cadastroDto.Telefone,
            Ativo = false, // Inativo até validar email
            EmailValidado = false,
            CodigoValidacao = codigoValidacao,
            DataExpiracaoCodigo = dataExpiracao
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        // Envia email de validação
        await _emailService.EnviarEmailValidacaoAsync(cadastroDto.Email, cadastroDto.Nome, codigoValidacao);

        _logger.LogInformation("Empresa {CNPJ} e usuário {Email} cadastrados. Código de validação enviado.", cnpj, cadastroDto.Email);

        // Não retorna token - usuário precisa validar email primeiro
        return new CadastroEmpresaResponseDTO
        {
            Empresa = _mapper.Map<EmpresaDTO>(empresa),
            Usuario = _mapper.Map<UsuarioDTO>(usuario),
            Token = string.Empty, // Token vazio até validação
            ExpiraEm = DateTime.UtcNow,
            EmailEnviado = true
        };
    }

    public string GenerateJwtToken(int usuarioId, int empresaId, string email)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "sua-chave-secreta-super-longa-e-complexa-para-producao-mude-isso";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "NFSe2026";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "NFSe2026";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new Claim("EmpresaId", empresaId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<LoginResponseDTO> ValidarEmailAsync(string email, string codigo)
    {
        // Usa Select para evitar carregar campos que podem não existir ainda no banco (ClientId, ClientSecret)
        var usuario = await _context.Usuarios
            .Where(u => u.Email == email)
            .Select(u => new
            {
                u.Id,
                u.EmpresaId,
                u.Nome,
                u.Email,
                u.Telefone,
                u.Ativo,
                u.EmailValidado,
                u.CodigoValidacao,
                u.DataExpiracaoCodigo,
                Empresa = new
                {
                    u.Empresa.Id,
                    u.Empresa.CNPJ,
                    u.Empresa.RazaoSocial,
                    u.Empresa.NomeFantasia,
                    u.Empresa.InscricaoEstadual,
                    u.Empresa.InscricaoMunicipal,
                    u.Empresa.Endereco,
                    u.Empresa.Numero,
                    u.Empresa.Complemento,
                    u.Empresa.Bairro,
                    u.Empresa.Cidade,
                    u.Empresa.UF,
                    u.Empresa.CodigoMunicipio,
                    u.Empresa.CEP,
                    u.Empresa.Telefone,
                    u.Empresa.Email,
                    u.Empresa.SituacaoCadastral,
                    u.Empresa.Logotipo,
                    u.Empresa.DataVencimentoCertificado,
                    u.Empresa.CertificadoDigital,
                    // ClientId e ClientSecret não são incluídos aqui para evitar erro se as colunas não existirem no banco
                    u.Empresa.RegimeEspecialTributacao,
                    u.Empresa.OptanteSimplesNacional,
                    u.Empresa.IncentivoFiscal,
                    u.Empresa.Ativo
                }
            })
            .FirstOrDefaultAsync();

        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }

        if (usuario.EmailValidado)
        {
            throw new InvalidOperationException("Email já foi validado");
        }

        if (string.IsNullOrEmpty(usuario.CodigoValidacao))
        {
            throw new InvalidOperationException("Código de validação não encontrado. Solicite um novo código.");
        }

        if (usuario.CodigoValidacao != codigo)
        {
            throw new InvalidOperationException("Código de validação inválido");
        }

        if (usuario.DataExpiracaoCodigo.HasValue && usuario.DataExpiracaoCodigo.Value < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Código de validação expirado. Solicite um novo código.");
        }

        // Valida o email e ativa o usuário - busca o usuário completo para atualizar
        var usuarioCompleto = await _context.Usuarios.FindAsync(usuario.Id);
        if (usuarioCompleto == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }

        usuarioCompleto.EmailValidado = true;
        usuarioCompleto.Ativo = true;
        usuarioCompleto.CodigoValidacao = null;
        usuarioCompleto.DataExpiracaoCodigo = null;
        usuarioCompleto.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Email {Email} validado com sucesso", email);

        // Gera token JWT
        var token = GenerateJwtToken(usuario.Id, usuario.EmpresaId, usuario.Email);

        // Mapeia manualmente para evitar problemas com campos que podem não existir
        var usuarioDto = new UsuarioDTO
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Telefone = usuario.Telefone,
            Ativo = usuario.Ativo
        };

        var empresaDto = new EmpresaDTO
        {
            Id = usuario.Empresa.Id,
            CNPJ = usuario.Empresa.CNPJ,
            RazaoSocial = usuario.Empresa.RazaoSocial,
            NomeFantasia = usuario.Empresa.NomeFantasia,
            InscricaoEstadual = usuario.Empresa.InscricaoEstadual,
            InscricaoMunicipal = usuario.Empresa.InscricaoMunicipal,
            Endereco = usuario.Empresa.Endereco,
            Numero = usuario.Empresa.Numero,
            Complemento = usuario.Empresa.Complemento,
            Bairro = usuario.Empresa.Bairro,
            Cidade = usuario.Empresa.Cidade,
            UF = usuario.Empresa.UF,
            CodigoMunicipio = usuario.Empresa.CodigoMunicipio,
            CEP = usuario.Empresa.CEP,
            Telefone = usuario.Empresa.Telefone,
            Email = usuario.Empresa.Email,
            SituacaoCadastral = usuario.Empresa.SituacaoCadastral,
            Logotipo = usuario.Empresa.Logotipo,
            DataVencimentoCertificado = usuario.Empresa.DataVencimentoCertificado,
            TemCertificadoDigital = !string.IsNullOrEmpty(usuario.Empresa.CertificadoDigital),
            TemClientIdSecret = false, // Será verificado separadamente se necessário (após migration)
            RegimeEspecialTributacao = usuario.Empresa.RegimeEspecialTributacao,
            OptanteSimplesNacional = usuario.Empresa.OptanteSimplesNacional,
            IncentivoFiscal = usuario.Empresa.IncentivoFiscal,
            Ativo = usuario.Empresa.Ativo
        };

        return new LoginResponseDTO
        {
            Token = token,
            Usuario = usuarioDto,
            Empresa = empresaDto,
            ExpiraEm = DateTime.UtcNow.AddHours(8)
        };
    }
}

