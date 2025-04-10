using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using MedicamentoAPI.Models;

namespace MedicamentoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicamentoController : ControllerBase
    {
        // Função auxiliar para extrair o preço de string para decimal
        private static decimal? ExtrairPreco(string precoTexto)
        {
            if (string.IsNullOrWhiteSpace(precoTexto)) return null;

            var apenasNumeros = new string(precoTexto
                .Where(c => char.IsDigit(c) || c == ',' || c == '.')
                .ToArray());

            if (apenasNumeros.Contains(',') && !apenasNumeros.Contains('.'))
                apenasNumeros = apenasNumeros.Replace(',', '.');

            if (decimal.TryParse(apenasNumeros, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var precoDecimal))
            {
                return precoDecimal;
            }

            return null;
        }
                
        [HttpGet]
        public async Task<IActionResult> GetMedicamentos([FromQuery] string searchTerm, [FromQuery] decimal? precoMin, [FromQuery] decimal? precoMax)
        {
            if (precoMin > precoMax)
            {
                return BadRequest("Preço mínimo não pode ser maior que o preço máximo.");
            }

            try
            {
                // Codifica o termo de pesquisa para garantir que ele seja seguro para a URL
                var urlEncodedSearchTerm = Uri.EscapeDataString(searchTerm);  // Codifica a string de pesquisa para a URL
                var url = $"https://consultaremedios.com.br/b/{urlEncodedSearchTerm}";

                // Cria uma instância do HttpClient
                var httpClient = new HttpClient();

                // Adiciona o cabeçalho User-Agent para simular uma requisição de um navegador
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.0.0 Safari/537.36");

                // Faz a requisição HTTP para obter o conteúdo da página
                var response = await httpClient.GetStringAsync(url);

                // Verifica se a resposta da requisição está vazia ou nula
                if (string.IsNullOrWhiteSpace(response))
                {
                    return NotFound("Nenhum medicamento encontrado.");
                }

                // Carrega o conteúdo HTML com o HtmlAgilityPack
                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                // Inicializa a lista de medicamentos
                var medicamentos = new List<Medicamento>();

                var medicamentoNodes = doc.DocumentNode.SelectNodes("//li[contains(@class, 'cr-border-divider')]");
                if (medicamentoNodes == null)
                {
                    return NotFound("Nenhum medicamento encontrado.");
                }

                foreach (var item in medicamentoNodes)
                {                    
                    var nomeMed = item.SelectSingleNode(".//a[contains(@class, 'cr-limit-title-product')]")?.InnerText.Trim();
                    if (nomeMed == null) continue;

                    var preco = item.SelectSingleNode(".//a[contains(@class, 'd-flex p-0 flex-wrap')]//p[contains(@class, 'cr-typography-heading-h2')]")?.InnerText.Trim();
                    if (preco == null) continue;

                    var precoDecimal = ExtrairPreco(preco); // Converte o preço extraído para decimal

                    var fornecedorNode = item.SelectSingleNode(".//a[contains(@class, 'd-flex cr-info-details') and contains(@href, '/fabricante')]//p")?.InnerText.Trim();

                    var principioNode = item.SelectSingleNode(".//a[contains(@href, '/pa')]//p");
                    var principio = principioNode != null ? principioNode.InnerText.Trim() : "Princípio não encontrado";

                    var urlProduto = item.SelectSingleNode(".//a[contains(@class, 'cr-limit-title-product')]")?.GetAttributeValue("href", "");

                    medicamentos.Add(new Medicamento
                    {
                        Nome = nomeMed,
                        Preco = preco,
                        Fornecedor = fornecedorNode,
                        PrincipioAtivo = principio,
                        Url = "https://consultaremedios.com.br/" + urlProduto,
                        PrecoDecimal = precoDecimal
                    });
                }

                var resultado = medicamentos.Where(m =>
                    (m.Nome != null && m.Nome.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase)) ||
                    (m.Fornecedor != null && m.Fornecedor.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase)) ||
                    (m.PrincipioAtivo != null && m.PrincipioAtivo.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase))
                ).ToList();

                if (precoMin.HasValue)
                {
                    resultado = resultado.Where(m => m.PrecoDecimal >= precoMin.Value).ToList();
                }

                if (precoMax.HasValue)
                {
                    resultado = resultado.Where(m => m.PrecoDecimal <= precoMax.Value).ToList();
                }

                if (resultado.Count == 0)
                {
                    return NotFound("Nenhum medicamento encontrado.");
                }

                return Ok(resultado);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro na requisição HTTP: {ex.Message}");
                return StatusCode(500, "Erro ao acessar o site de medicamentos.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro desconhecido: {ex.Message}");
                return StatusCode(500, "Erro inesperado.");
            }
        }
    }
}