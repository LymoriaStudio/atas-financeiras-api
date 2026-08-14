#!/bin/sh
set -e

# O volume persistente do Railway (ou de qualquer orquestrador) só é montado em
# /app/uploads quando o container já está rodando (runtime), não durante o build —
# então o dono dessa pasta pode não bater com o usuário sem privilégios (appuser)
# preparado na imagem. Corrige a posse aqui (como root, só nesse instante) antes de
# trocar pro usuário sem privilégios pra rodar a aplicação de fato.
mkdir -p /app/uploads
chown -R appuser:appgroup /app/uploads

exec su -s /bin/sh appuser -c "dotnet AtasFinanceiras.Api.dll --urls http://+:${PORT:-8080}"
