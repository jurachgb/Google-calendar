# Google Calendar ICS Sync

Uma aplicação .NET que sincroniza eventos de arquivos ICS (iCalendar) para o Google Calendar de forma automática e inteligente.

## 📋 Funcionalidades

- **Importação de ICS**: Lê eventos de arquivos ICS locais ou download automático via URL
- **Sincronização Inteligente**: Evita duplicação de eventos usando UIDs únicos
- **Suporte a URLs**: Automatiza o download de arquivos ICS usando automação de navegador
- **Eventos de Dia Inteira**: Trata corretamente eventos sem horário específico
- **Timezone**: Configura automaticamente o fuso horário de São Paulo (GMT-3)
- **Autenticação Google**: Usa contas de serviço do Google Cloud para segurança

## 🔧 Pré-requisitos

- .NET 10.0 ou superior
- Google Cloud Project com Google Calendar API ativada
- Credenciais de conta de serviço do Google

## 📦 Dependências

- **Google.Apis.Calendar.v3** (v1.75.0.4206) - Integração com Google Calendar
- **Ical.Net** (v5.2.3) - Parsing de arquivos ICS/iCalendar
- **Microsoft.Playwright** (v1.62.0) - Automação de navegador para download

## ⚙️ Configuração

### 1. Configurar Credenciais Google

1. Acesse [Google Cloud Console](https://console.cloud.google.com/)
2. Crie um novo projeto
3. Ative a API do Google Calendar
4. Crie uma conta de serviço
5. Gere uma chave JSON e salve como `credentials.json` na raiz do projeto

### 2. Configurar ID do Calendário

Substitua `Info.Dados.Id` pelo email do calendário onde os eventos serão sincronizados.

### 3. Compilar o Projeto

```bash
dotnet build
```

## 🚀 Como Usar

### Opção 1: Arquivo Local

```bash
dotnet run -- "link do calendario q ser baixado"
```

Exemplo:
```bash
dotnet run -- "https://..."
```

### Opção 2: URL do Arquivo

```bash
dotnet run -- "https://example.com/calendario.ics"
```

O programa abrirá um navegador, fará o download do arquivo e processará automaticamente.

## 📝 Como Funciona

1. **Entrada**: Aceita um argumento de linha de comando ( URL)

2. **Download**: 
   - Abre um navegador automatizado
   - Aguarda o download do arquivo
   - Salva como `calendario.ics`

3. **Parsing**: 
   - Lê o arquivo ICS
   - Extrai lista de eventos

4. **Autenticação Google**:
   - Carrega credenciais da conta de serviço
   - Conecta-se à API do Google Calendar

5. **Deduplicação**:
   - Busca eventos já sincronizados (usando UID do ICS)
   - Filtra apenas novos eventos

6. **Sincronização**:
   - Para cada novo evento:
     - Copia título, descrição e local
     - Trata eventos de dia inteiro
     - Converte datas/horas com timezone São Paulo
     - Insere no Google Calendar

7. **Feedback**: Exibe cada evento sincronizado com um prefixo `+`

## 📂 Estrutura do Projeto

```
Google-calendar/
├── Program.cs              # Lógica principal de sincronização
├── Google-calendar.csproj  # Arquivo de projeto .NET
├── credentials.json        # Credenciais Google (não versionado)
├── calendario.ics          # Arquivo ICS baixado (gerado)
└── README.md              # Este arquivo
```

## 🔐 Segurança

- **Nunca** committe o arquivo `credentials.json` no repositório
- Use variáveis de ambiente ou arquivo `.gitignore` para proteger credenciais
- A aplicação usa tokens JWT para autenticação com Google

## ⏱️ Comportamento de Sincronização

- **Eventos de Dia Inteira**: Convertidos para formato `Date` (sem hora)
- **Eventos com Horário**: Convertidos para `DateTimeOffset` com timezone São Paulo
- **Eventos Duplicados**: Identificados pelo UID do ICS, não são re-inseridos
- **Duração Padrão**: Eventos sem data de término recebem duração de 1 hora


## 📄 Formato ICS Suportado

Suporta qualquer arquivo iCalendar RFC 5545 padrão com:
- `VEVENT` - Definições de eventos
- `DTSTART` - Data/hora de início (obrigatório)
- `DTEND` - Data/hora de término (opcional)
- `SUMMARY` - Título do evento
- `DESCRIPTION` - Descrição
- `LOCATION` - Local
- `UID` - Identificador único


## 📄 Licença

Este projeto não possui licença especificada. Sinta-se livre para usar e modificar conforme necessário.

## 👤 Autor

Desenvolvido como ferramenta de sincronização de calendários ICS para Google Calendar.

---

**Última atualização**: 2026-09-10