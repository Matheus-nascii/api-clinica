# ApiClinica 🏥

API REST para gerenciar **pacientes, médicos e consultas** de uma clínica, desenvolvida em **C# / .NET 10** com **Entity Framework Core (ORM)**, **SQLite**, **DTOs** e **mappers manuais**.

Projeto acadêmico do trabalho N2 da disciplina **Programação Server-Side**, do curso de Engenharia de Software da **Católica SC**.

## Tecnologias

- C# e .NET 10 (ASP.NET Core Web API)
- Entity Framework Core (ORM)
- SQLite
- Postman (37 testes automatizados)

## Arquitetura

```
Postman (JSON) → Controller (DTO) → Validações → Mapper → AppDbContext (EF Core) → SQLite
                       ↑                                                              │
                       └──────────────── ReadDTO + código HTTP ←──────────────────────┘
```

| Pasta | Responsabilidade |
| --- | --- |
| `Models/` | Entidades `Paciente`, `Medico` e `Consulta` |
| `Data/` | `AppDbContext`: tabelas, relacionamentos 1:N e CPF único |
| `DTOs/` | `CreateDTO`, `ReadDTO` e `UpdateDTO` de cada entidade |
| `Mappers/` | Conversão manual entre DTO e Model |
| `Validations/` | Email, telefone (regex), data de nascimento e CPF |
| `Controllers/` | Endpoints GET, GET por id, POST, PATCH e DELETE |

## Regras de negócio

**Pacientes**
- Email válido e telefone no formato `(47) 98888-7777` (regex, sem o atributo `[Phone]`)
- Data de nascimento não pode ser no futuro
- CPF validado pelos dígitos verificadores, único no cadastro e não pode ser alterado

**Médicos**
- Email válido e telefone no formato exigido

**Consultas**
- Paciente e médico precisam existir
- Não é possível agendar no passado
- Cada consulta dura 30 minutos: sem horário igual ou sobreposto para o mesmo médico ou paciente
- No PATCH, trocar paciente ou médico revalida todas as regras

**Gerais**
- PATCH: campo `null` (ou não enviado) não é alterado
- Paciente ou médico com consulta futura não pode ser excluído

## Endpoints

| Recurso | Rotas |
| --- | --- |
| Pacientes | `GET /api/pacientes` · `GET /api/pacientes/{id}` · `POST` · `PATCH /{id}` · `DELETE /{id}` |
| Médicos | `GET /api/medicos` · `GET /api/medicos/{id}` · `POST` · `PATCH /{id}` · `DELETE /{id}` |
| Consultas | `GET /api/consultas` · `GET /api/consultas/{id}` · `POST` · `PATCH /{id}` · `DELETE /{id}` |

| Código | Quando |
| --- | --- |
| 200 OK | GET e PATCH com sucesso |
| 201 Created | POST com sucesso |
| 204 No Content | DELETE com sucesso |
| 400 Bad Request | Falha de validação (retorna a lista `erros`) |
| 404 Not Found | Id não encontrado |
| 409 Conflict | CPF duplicado ou exclusão com consulta futura |

## Como rodar

Pré-requisito: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```bash
git clone https://github.com/Matheus-nascii/api-clinica.git
cd api-clinica
dotnet run
```

A API sobe em `http://localhost:5080`. O banco `clinica.db` é criado automaticamente na primeira execução.

## Testes no Postman

1. Importe `Postman/ApiClinica.postman_collection.json`.
2. Com a API rodando, abra a coleção e clique em **Run**.
3. Resultado esperado: **37 testes passando**.

> Os testes esperam o banco vazio. Para rodar de novo, pare a API, apague o `clinica.db` e execute `dotnet run`.

## Equipe

- Matheus Nascimento de Oliveira
- Mateus Knis
- Carlos Alberto
- Victor Garbin
- Brenda Kobroski

Professora: Beatriz M. Reichert · Católica SC · 2026
