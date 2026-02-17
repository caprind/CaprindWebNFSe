using AutoMapper;
using NFSe2026.API.DTOs;
using NFSe2026.API.Models;

namespace NFSe2026.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Tomador mappings
        CreateMap<Tomador, TomadorDTO>()
            .ForMember(dest => dest.TipoPessoa, opt => opt.MapFrom(src => (int)src.TipoPessoa));
        CreateMap<TomadorCreateDTO, Tomador>()
            .ForMember(dest => dest.TipoPessoa, opt => opt.MapFrom(src => (TipoPessoa)src.TipoPessoa));
        CreateMap<TomadorUpdateDTO, Tomador>()
            .ForMember(dest => dest.TipoPessoa, opt => opt.MapFrom(src => (TipoPessoa)src.TipoPessoa))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CPFCNPJ, opt => opt.Ignore())
            .ForMember(dest => dest.EmpresaId, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.Empresa, opt => opt.Ignore())
            .ForMember(dest => dest.NotasFiscais, opt => opt.Ignore());

        // NotaFiscal mappings
        CreateMap<NotaFiscal, NotaFiscalDTO>()
            .ForMember(dest => dest.Situacao, opt => opt.MapFrom(src => (int)src.Situacao))
            .ForMember(dest => dest.TomadorNome, opt => opt.MapFrom(src => src.Tomador != null ? src.Tomador.RazaoSocialNome : null));
        CreateMap<NotaFiscalCreateDTO, NotaFiscal>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Empresa, opt => opt.Ignore())
            .ForMember(dest => dest.Tomador, opt => opt.Ignore())
            .ForMember(dest => dest.ItensServico, opt => opt.Ignore());
        CreateMap<NotaFiscalUpdateDTO, NotaFiscal>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Empresa, opt => opt.Ignore())
            .ForMember(dest => dest.Tomador, opt => opt.Ignore())
            .ForMember(dest => dest.ItensServico, opt => opt.Ignore())
            .ForMember(dest => dest.Situacao, opt => opt.Ignore())
            .ForMember(dest => dest.Numero, opt => opt.Ignore())
            .ForMember(dest => dest.CodigoVerificacao, opt => opt.Ignore())
            .ForMember(dest => dest.XML, opt => opt.Ignore())
            .ForMember(dest => dest.JSON, opt => opt.Ignore())
            .ForMember(dest => dest.DataEmissao, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore());

        // ItemServico mappings
        CreateMap<ItemServico, ItemServicoDTO>();
        CreateMap<ItemServicoCreateDTO, ItemServico>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.NotaFiscalId, opt => opt.Ignore())
            .ForMember(dest => dest.NotaFiscal, opt => opt.Ignore());

        // Empresa mappings
        CreateMap<Empresa, EmpresaDTO>()
            .ForMember(dest => dest.TemCertificadoDigital, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.CertificadoDigital)))
            .ForMember(dest => dest.TemClientIdSecret, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.ClientId) && !string.IsNullOrEmpty(src.ClientSecret)))
            .ForMember(dest => dest.ProvedorNFSe, opt => opt.MapFrom(src => (int)src.ProvedorNFSe));
        CreateMap<EmpresaUpdateDTO, Empresa>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CNPJ, opt => opt.Ignore())
            .ForMember(dest => dest.RazaoSocial, opt => opt.Ignore())
            .ForMember(dest => dest.Ambiente, opt => opt.Ignore())
            .ForMember(dest => dest.SituacaoCadastral, opt => opt.Ignore())
            .ForMember(dest => dest.Porte, opt => opt.Ignore())
            .ForMember(dest => dest.NaturezaJuridica, opt => opt.Ignore())
            .ForMember(dest => dest.DataAbertura, opt => opt.Ignore())
            .ForMember(dest => dest.Ativo, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DataAtualizacao, opt => opt.Ignore())
            .ForMember(dest => dest.CertificadoDigital, opt => opt.Ignore())
            .ForMember(dest => dest.SenhaCertificado, opt => opt.Ignore())
            .ForMember(dest => dest.ClientId, opt => opt.Ignore())
            .ForMember(dest => dest.ClientSecret, opt => opt.Ignore())
            .ForMember(dest => dest.Usuarios, opt => opt.Ignore())
            .ForMember(dest => dest.Tomadores, opt => opt.Ignore())
            .ForMember(dest => dest.NotasFiscais, opt => opt.Ignore())
            .ForMember(dest => dest.OptanteSimplesNacional, opt => opt.MapFrom(src => src.OptanteSimplesNacional ?? true))
            .ForMember(dest => dest.IncentivoFiscal, opt => opt.MapFrom(src => src.IncentivoFiscal ?? false))
            .ForMember(dest => dest.ProvedorNFSe, opt => opt.MapFrom(src => src.ProvedorNFSe.HasValue ? (ProvedorNFSe)src.ProvedorNFSe.Value : ProvedorNFSe.Nacional))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Usuario mappings
        CreateMap<Usuario, UsuarioDTO>();
    }
}

