import { useState } from 'react';

function decodeHtml(html) {
  const txt = document.createElement('textarea');
  txt.innerHTML = html;
  return txt.value;
}

function App() {
  const [searchTerm, setSearchTerm] = useState('');
  const [precoMin, setPrecoMin] = useState('');
  const [precoMax, setPrecoMax] = useState('');
  const [medicamentos, setMedicamentos] = useState([]);
  const [mensagem, setMensagem] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSearch = async (e) => {
    if (e) e.preventDefault(); // Impede reload do form

    if (!searchTerm.trim()) {
      setMensagem("Por favor, preencha o campo de busca.");
      setMedicamentos([]);
      return;
    }

    try {
      setLoading(true);
      setMensagem('');
      const params = new URLSearchParams({
        searchTerm,
        precoMin: precoMin || '',
        precoMax: precoMax || ''
      });

      const response = await fetch(`https://localhost:7253/api/medicamento?${params.toString()}`);

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || "Erro ao buscar medicamentos.");
      }

      const data = await response.json();

      if (data.length === 0) {
        setMensagem("Nenhum medicamento encontrado.");
        setMedicamentos([]);
      } else {
        const decodedData = data.map(med => ({
          ...med,
          nome: decodeHtml(med.nome),
          fornecedor: decodeHtml(med.fornecedor),
          principioAtivo: decodeHtml(med.principioAtivo),
        }));
        setMedicamentos(decodedData);
        setMensagem('');
      }
    } catch (error) {
      setMensagem(error.message || "Erro ao buscar medicamentos.");
      setMedicamentos([]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-white text-gray-900 flex flex-col items-center p-6" style={{ padding: '0 1rem' }}>
      {/* TÍTULO E FORMULÁRIO CENTRALIZADOS */}
      <div className="w-full max-w-xl text-center mt-12">
        <h1 className="text-4xl font-bold mb-8">💊 Busca de Medicamentos</h1>

        <form onSubmit={handleSearch}>
  <div style={{ display: 'flex', gap: '16px', width: '100%', marginBottom: '1rem', flexWrap: 'wrap' }}>
    <input
      type="text"
      placeholder="Nome, princípio ativo ou fornecedor"
      value={searchTerm}
      onChange={(e) => setSearchTerm(e.target.value)}
      style={{
        flex: 2,
        padding: '12px',
        borderRadius: '8px',
        border: '1px solid #ccc',
        outline: 'none',
        minWidth: '200px',
      }}
    />
    <input
      type="number"
      placeholder="Preço Mínimo"
      value={precoMin}
      onChange={(e) => setPrecoMin(e.target.value)}
      style={{
        flex: 1,
        padding: '12px',
        borderRadius: '8px',
        border: '1px solid #ccc',
        outline: 'none',
        minWidth: '100px',
      }}
    />
    <input
      type="number"
      placeholder="Preço Máximo"
      value={precoMax}
      onChange={(e) => setPrecoMax(e.target.value)}
      style={{
        flex: 1,
        padding: '12px',
        borderRadius: '8px',
        border: '1px solid #ccc',
        outline: 'none',
        minWidth: '100px',
      }}
    />
    <button
      type="submit"
      style={{
        padding: '12px 16px',
        backgroundColor: '#2563EB',
        color: '#fff',
        borderRadius: '8px',
        border: 'none',
        cursor: 'pointer',
        fontWeight: 'bold',
        whiteSpace: 'nowrap',
        minWidth: '100px',
      }}
    >
      Buscar
    </button>
    <button
  type="button"
  onClick={() => {
    setSearchTerm('');
    setPrecoMin('');
    setPrecoMax('');
    setMedicamentos([]);
    setMensagem('');
  }}
  style={{
    padding: '12px 16px',
    backgroundColor: '#6B7280', // cinza
    color: '#fff',
    borderRadius: '8px',
    border: 'none',
    cursor: 'pointer',
    fontWeight: 'bold',
    whiteSpace: 'normal',
    minWidth: '100px',
    textAlign: 'center',
    lineHeight: '1.2',
  }}
>
  Limpar<br />resultados
</button>
  </div>
</form>

        {mensagem && (
          <p className="text-red-600 mt-4">{mensagem}</p>
        )}
      </div>

      {/* RESULTADOS */}
      <div className="mt-12 w-full max-w-4xl px-4">
        {loading && <p className="text-center text-gray-600">Carregando...</p>}
        {medicamentos.length > 0 && (
          <div className="grid gap-4 mt-6">
            {medicamentos.map((med, index) => (
              <div
                key={index}
                className="bg-gray-100 p-6 rounded-xl shadow border border-gray-300"
              >
                <h2 className="text-lg font-semibold text-blue-700">{med.nome}</h2>
                <p><strong>Preço:</strong> {med.preco}</p>
                <p><strong>Fornecedor:</strong> {med.fornecedor}</p>
                <p><strong>Princípio Ativo:</strong> {med.principioAtivo}</p>
                <a
                  href={med.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-sm text-blue-600 underline"
                >
                  Ver mais
                </a>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export default App;