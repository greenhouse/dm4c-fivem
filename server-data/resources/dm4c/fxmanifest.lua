fx_version 'bodacious'
game 'gta5'

file 'Client/bin/Release/**/publish/*.dll'

-- client_script 'Client/bin/Release/**/publish/*.net.dll'
-- server_script 'Server/bin/Release/**/publish/*.net.dll'

client_script 'Client/bin/Debug/**/*.net.dll'
server_script 'Server/bin/Debug/**/*.net.dll'

author 'You'
version '1.0.0'
description 'Example Resource from C# Template'

-- client_scripts {
--     'client/*.dll'
-- }

-- server_scripts {
--     'server/*.dll'
-- }

ui_page 'ui/index.html'

files {
    'ui/index.html'
}