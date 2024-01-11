Olá, vou deixar algumas configurações para se iniciar o Projeto e Executá-lo
-Rodar a migrations do Entity Framework
-Executar esse comando para incluir os CPF'S:
INSERT INTO public."Cpfs" ("Numero", "Processado") VALUES
('343.228.350-40', false),
('869.230.000-41', false),
('568.946.870-30', false),
('433.510.120-12', false),
('415.022.590-79', false);
- O sistema deverá alimentar a fila do RabbitMq baseado nesses CPF's, sendo feito logo quando se inicializa a api.
- Também precisa-se criar a tabela de Logs manualmente, do Serilog, baseado em testes essa foi a forma com que o serilog se conecta melhor com o postgresql
CREATE TABLE logs (
    "Id" serial PRIMARY KEY,
    "Message" TEXT NULL,
    "MessageTemplate" TEXT NULL,
    "Level" TEXT NULL,
    "TimeStamp" TIMESTAMP NULL,
    "Exception" TEXT NULL,
    "Properties" TEXT NULL,
    "LogEvent" TEXT NULL
);
- Lembrando que para o Postgresql não existe o autosqlcreate.
- Fazendo isso o sistema deverá ser executado sem muitos problemas.
- Lembre-se de ajustar as configurações do rabbitmq, elasticsearch, redis e string de conexão baseado na sua realidade.

Desde já agradeço pela oportunidade de aprender e desenvolver um novo projeto!
