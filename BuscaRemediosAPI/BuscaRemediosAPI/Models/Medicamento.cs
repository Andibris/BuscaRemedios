namespace MedicamentoAPI.Models
{
    public class Medicamento
    {
        public string? Nome { get; set; }
        public string? Preco { get; set; }
        public string? Fornecedor { get; set; }
        public string? PrincipioAtivo { get; set; }
        public string? Url { get; set; }
        public decimal? PrecoDecimal { get; set; }
    }
}