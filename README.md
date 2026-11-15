# ApiClinica - N3

API REST acadêmica para cadastro de pacientes, médicos e consultas. A N3 mantém EF Core/SQLite, DTOs e regras da N2, e adiciona Services, injeção de dependência, JWT e autorização por perfil.

## Arquitetura

```
Controllers/  HTTP, status codes e autorização
Services/     casos de uso e regras de negócio
Interfaces/   contratos dos mappers, usados para desacoplar conversao e services
Mappers/      conversão manual entre entidades e DTOs
DTOs/         contratos de entrada e saída da API
Models/       Paciente, Medico, Consulta e Usuario
Data/         AppDbContext, factory de design e Migrations
Validations/  regras reutilizáveis de email, telefone, data e CPF
Postman/      coleção de demonstração N3
```

## Guia rapido para apresentacao

- `ApiClinica.csproj`: manifesto de compilacao. Define que e uma API Web ASP.NET Core, o alvo `net10.0` e as dependencias: EF Core/SQLite para banco e JWT Bearer para autenticacao. Comentarios XML sao aceitos nesse arquivo, pois ele nao e codigo executado.
- `Program.cs`: ponto de entrada. Configura injeção de dependência, SQLite, JWT, autorizacao, migrations e criacao opcional do Admin; depois inicia a API.
- `Controllers/`: camada HTTP. Define rotas, autorizacao e codigos HTTP; nao contem regras de negocio nem acesso direto ao banco.
- `Services/`: camada de casos de uso e regras de negocio. Aqui estao as validacoes de cadastro, CPF imutavel, bloqueio de exclusao, agenda futura e conflito de consultas.
- `Data/AppDbContext.cs`: mapeia as entidades para as tabelas SQLite, os relacionamentos e os indices unicos. `AppDbContextFactory.cs` existe para os comandos `dotnet ef` funcionarem sem iniciar a API.
- `Models/`: entidades persistidas pelo EF Core, isto e, a representacao das tabelas `Pacientes`, `Medicos`, `Consultas` e `Usuarios`.
- `DTOs/`: formatos de entrada e saida da API. `Create` e usado no POST, `Update` no PATCH e `Read` na resposta; isso impede expor diretamente as entidades do banco.
- `Mappers/` e `Interfaces/`: convertem DTOs em entidades e vice-versa. As interfaces permitem que os services dependam do contrato, nao da implementacao.
- `Validations/Validador.cs`: funcoes reutilizaveis para email, telefone, data de nascimento e CPF.
- `Migrations/`: historico gerado pelo EF Core para criar/evoluir o esquema do banco. Nao se edita manualmente.
- `appsettings.json`: configuracao sem segredos (SQLite, emissor/audiencia JWT e logs). A chave JWT e as credenciais Admin devem vir de variaveis de ambiente ou User Secrets.
- `Postman/`: colecao de requisicoes para demonstrar os endpoints e cenarios de autenticacao.

Fluxo: `HTTP -> Controller -> Service -> DbContext/SQLite`. Controllers não acessam mais o banco diretamente. Os services recebem mappers por DI e preservam as regras originais.

`Consulta` possui as FKs `PacienteId` e `MedicoId`: um paciente e um médico podem ter várias consultas. CPF de paciente e email de usuário são únicos. Uma consulta dura 30 minutos e não pode sobrepor outra do mesmo paciente ou médico.

## Requisitos

- .NET SDK 10.0 (o projeto usa `net10.0`)
- SQLite, criado pelo EF Core a partir das migrations

## Configuração segura

Nenhum segredo é versionado. Antes de executar, configure ao menos uma chave JWT de 32 caracteres. No PowerShell:

```powershell
$env:Jwt__Key = "gere-uma-chave-local-com-mais-de-32-caracteres"
```

Configure um administrador de demonstração apenas por variável de ambiente. Ele é criado uma única vez quando não existir; o cadastro público nunca aceita `role` e sempre cria `Usuario`.

```powershell
$env:Admin__Email = "admin@exemplo.local"
$env:Admin__Senha = "uma-senha-local-com-8-ou-mais-caracteres"
```

Alternativamente, use `dotnet user-secrets` em desenvolvimento. Não inclua esses valores em `appsettings.json` ou no Git.

## Banco e execução

Em banco novo, execute:

```bash
dotnet restore
dotnet ef database update
dotnet run
```

O `Program.cs` também executa `MigrateAsync` na inicialização. Para um banco N2 existente criado com `EnsureCreated`, faça backup de `clinica.db` antes: ele não possui histórico de migrations e deve ser baselined/migrado conscientemente pela equipe, nunca apagado automaticamente pela API.

## Autenticação e autorização

`POST /api/auth/register` recebe, por exemplo:

```json
{ "nome": "Ana", "email": "ana@exemplo.com", "senha": "SenhaSegura123" }
```

Retorna `200 OK` com `token`, `expiraEm` (UTC) e `role`; hashes e senhas não são retornados. `POST /api/auth/login` recebe email e senha e retorna o mesmo formato.

Envie `Authorization: Bearer <token>` nas rotas de domínio.

| Método | Pacientes, médicos e consultas |
|---|---|
| GET, POST | JWT válido (`Usuario` ou `Admin`) |
| PATCH, DELETE | JWT válido com papel `Admin` |

Sem token, token inválido ou expirado: `401`. Usuário comum tentando PATCH/DELETE: `403`.

Exemplo de criação autenticada:

```http
POST /api/pacientes
Authorization: Bearer <token>
Content-Type: application/json

{ "nome":"Ana Souza", "email":"ana@email.com", "telefone":"(47) 98888-7777", "dataNasc":"2000-05-10", "cpf":"529.982.247-25" }
```

Resposta: `201 Created`, com o paciente e seu CPF formatado. Validações falham com `400`, CPF duplicado com `409`, e ID inexistente com `404`.

## Regras de domínio preservadas

- Paciente: email, telefone, data de nascimento, CPF válido e CPF imutável.
- Médico: email e telefone válidos.
- Consulta: paciente/médico existentes, data futura e sem conflito de 30 minutos.
- Exclusão de paciente ou médico é bloqueada se houver consulta futura.
- PATCH não altera campos omitidos ou `null`.

## Postman — apresentação principal

Importe `Postman/ApiClinica.postman_collection.json` (formato Postman Collection v2.1). A coleção possui **9 testes essenciais em 10 requisições**, numeradas de 01 a 09B. Execute na ordem. `baseUrl` já está configurada para `http://localhost:5080`.

Nas variáveis da coleção, configure `adminEmail` e `adminSenha` conforme o administrador preparado neste computador. Não exporte o ambiente/coleção com a senha ou tokens reais preenchidos. A coleção preenche `userToken`, `adminToken`, `pacienteId`, `medicoId`, `consultaId` e dados de teste automaticamente. Ela não exclui registros.

**Importante:** a implementação inclui também `DELETE` de pacientes, médicos e consultas, com `[Authorize(Roles = "Admin")]`, embora o roteiro enxuto de 9 testes priorize a demonstração de `PATCH` e do controle de acesso. Se a professora solicitar, pode-se testar `DELETE /api/consultas/{id}` com token Admin: sucesso esperado `204 No Content`.

**Ao executar em outro computador:** o ZIP não contém a chave JWT nem as credenciais administrativas (corretamente). É necessário configurar `Jwt:Key` e, para testes de Admin, `Admin:Email` e `Admin:Senha` por User Secrets ou variáveis de ambiente. Para iniciar no Visual Studio Code, abra o terminal na pasta que contém `ApiClinica.csproj` e execute `dotnet run`. O projeto usa a porta 5080 em `Properties/launchSettings.json`; as migrations criam o banco SQLite novo automaticamente. Se for reutilizar um banco da N2, faça backup e verifique compatibilidade antes.


## N3 implementada

- Services e interfaces de services para as três entidades.
- `IPacienteMapper`, `IMedicoMapper`, `IConsultaMapper` e DI Scoped.
- `Usuario`, registro, login, `PasswordHasher` e JWT assinado.
- Claims de identificador e papel, validação de assinatura, emissor, audiência e expiração.
- `[Authorize]` declarativo por método/perfil.
- Migration inicial EF Core para as quatro tabelas.

Não há enunciado nem feedback N2 no repositório; por isso, requisitos N2 além do código disponível não puderam ser confirmados.
