namespace NFSe2026.API.Services;

public interface ICriptografiaService
{
    string Criptografar(string texto);
    string Descriptografar(string textoCriptografado);
}



