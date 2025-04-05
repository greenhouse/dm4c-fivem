# dm4c-fivem
dm4c c# fivem client/server integration


## build (unix base .net core)
    $ dotnet --version
    $ cd .../dm4c-fivem
    $ dotnet new -i CitizenFX.Templates
    $ mkdir MyResource
    $ cd MyResource/
    $ dotnet new cfx-resource
    $ dotnet build

## NEXT
    - build server-data (c# based) and deploy on remote linux server (ubuntu)
        - install .net core on remote server (ref: https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#set-environment-variables-system-wide)
            # NOTE_SUCCESS: appears to work fine on ubuntu 22.0.x but NOT ubuntu 24.0.x (but maybe i messed up and tried 'apt' first like grok suggested below)
                $ sudo su
                $ cd /srv
                $ wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
                $ chmod 744 dotnet-install.sh 
                $ ./dotnet-install.sh --version latest
                $ snap install dotnet-sdk
                $ snap install dotnet-sdk --classic
                $ dotnet --version
                $ dotnet --list-sdks
                $ dotnet --list-runtimes
                $ cd .../MyResources
                $ dotnet build

            - alt install .net core on remote server using 'apt' (ref: grok thread)
                # NOTE_FAILED: does not appear to be successful (tried on ubuntu 24.0.x)
                    $ sudo apt update
                    $ sudo apt install dotnet-runtime-8.0

        - clone git fivem project on remove server
            $ cd /srv/www
            $ git clone https://github.com/housing37/dm4c-fivem.git
        
        - build on remote linux server to generate .dll files (c# based)
            $ cd /srv/www/dm4c-fivem/server-data/resources/MyResources
            $ dotnet build

        - install fivem on remote server (ref: https://docs.fivem.net/docs/server-manual/setting-up-a-server-vanilla/)
        - run server via: ./run.sh +exec server.cfg (ref: legacy projects & grok thread)
        - try to join game via fivem, etc. (ref: legacy projects)
    
    DONE - clone vanilla ref: https://github.com/citizenfx/cfx-server-data.git into .../server-data-vanilla
        NOTE: this is lua based (not C#)
    DONE - where does the server.cfg go for c# integration (need initial test server.cfg)
