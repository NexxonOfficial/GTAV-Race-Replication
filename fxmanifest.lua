fx_version 'bodacious'
game 'gta5'

ui_page 'UI/core.html'

files {
    'Client/bin/Release/**/publish/*.dll',
    'yep.Shared/**/*.*',
    'UI/**/*.*',
    'UI/*.*',
    '**/*.*',
    '*.*'
}

client_script 'Client/bin/Release/**/publish/*.net.dll'
server_script 'Server/bin/Release/**/publish/*.net.dll'
author 'You'
version '1.0.0'
description 'Example Resource from C# Template'