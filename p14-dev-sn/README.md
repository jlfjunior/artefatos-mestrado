# Controle de Fluxo de Caixa  

## Descritivo da Solução  
Esta aplicação foi desenvolvida para atender à necessidade de um comerciante em controlar o seu fluxo de caixa diário. Ela permite realizar lançamentos de débitos e créditos e fornece um relatório com o saldo diário consolidado.  

### Funcionalidades  
- **Cadastro de lançamentos (débitos e créditos)**  
- **Relatório diário consolidado do saldo**  

---

## Requisitos de Negócio  
- Serviço responsável pelo controle dos lançamentos financeiros  
- Serviço que gera o consolidado diário do saldo  

---

## Requisitos Técnicos  
- Implementado em **C#**  
- **Testes automatizados** para garantir a confiabilidade  
- **Boas práticas de desenvolvimento**:  
  - Aplicação de princípios **SOLID**  
  - Utilização de **Design Patterns** e **Padrões de Arquitetura**  

---

## Como Rodar o Projeto Localmente  

### Pré-requisitos  
Antes de começar, certifique-se de que você tenha as ferramentas abaixo instaladas:  
- [.NET SDK](https://dotnet.microsoft.com/download)  
- [Git](https://git-scm.com/)  
- Um editor de código como [Visual Studio](https://visualstudio.microsoft.com/) ou [Visual Studio Code](https://code.visualstudio.com/)  

### Passo a Passo  
1. Clone o repositório:  
   ```bash  
   git clone https://github.com/AugustinhoCelestino/banco-carrefour.git
   cd banco-carrefour
    ```
---
2. Abrir a solução BancoCarrefour.sln no VisualStudio  
   ```bash  
   "BancoCarrefour.sln"
    ```
---
3. Compilar a solução  
   ```bash  
   Crtl + Shift + B
   ou apertar o botão direiro e ir em "Compilar Solução"
    ```
---
4. Colocar o projeto WebAPI como "Projeto de Inicialização"  
   ```bash  
   Apertar o botão direiro e ir em "Configuarar Projeto de Inicialização"
   Selecionar o projeto "WebAPi".
    ```
---
5. Selecionar Http para execução e executar o prejeto
   ```bash  
   Certifique que o projeto está com http selecionado, e clique em executar ou aperte F5.

   O Swagger UI ira abrir em : http://localhost:5150/swagger/index.html
    ```
---



### Estrutura do Projeto
A solução está estruturada da seguinte forma:

   ```scss  
      ├── BancoCarrefour  
      │   ├── 1 - Application      // Endpoints e Controllers
      │   ├── 2 - Domain
      │   │   ├── 2.1 - Domain     // Entidades, Enumeradores, Interfaces, Mappers e Utils        
      │   │   └── 2.2 - Services   // Interfaces e Serviços     
      │   ├── 3 - Infrastructure 
      │   │   ├── 3.1 - Data          // Contextos, Mapping, Migrationns e Repositórios
      │   │   ├── 3.2 - IoC           // Innjeção de dependencias
      └── ...
   ```  
---

### Tecnologias Utilizadas
- C#
- ASP.NET Core para construção de APIs
- xUnit para testes automatizados
- Entity Framework Core
- Swagger para documentação de API 
