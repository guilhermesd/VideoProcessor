@echo off
setlocal ENABLEDELAYEDEXPANSION

:: Caminho do arquivo de credenciais
set "AWS_CREDENTIALS=%USERPROFILE%\.aws\credentials"

:: Lê as variáveis do perfil [default]
for /f "tokens=1,2 delims==" %%A in ('findstr /i /r "aws_access_key_id= aws_secret_access_key= aws_session_token=" "%AWS_CREDENTIALS%"') do (
    set "VAR=%%A"
    set "VAL=%%B"
    set "!VAR!=!VAL!"
)

:: Lista de repositórios separada por espaço
set "REPOS=guilhermesd/VideoProcessor"

:: Loop pelos repositórios
for %%R in (%REPOS%) do (
    echo.
    echo Atualizando secrets no repositório %%R ...

    gh secret set AWS_ACCESS_KEY_ID --repo %%R --body "!aws_access_key_id!"
    gh secret set AWS_SECRET_ACCESS_KEY --repo %%R --body "!aws_secret_access_key!"
    gh secret set AWS_SESSION_TOKEN --repo %%R --body "!aws_session_token!"

    echo Secrets atualizados com sucesso para %%R
)

echo.
echo Todos os repositórios foram atualizados!
pause