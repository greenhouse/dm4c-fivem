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
        - build locally & transfer dll files to remote server (ref: grok thread)
            $ cd .../MyResources
            $ dotnet build
        - install .net core on remote server (ref: grok thread)
        - install fivem on remote server (ref: https://docs.fivem.net/docs/server-manual/setting-up-a-server-vanilla/)
        - run server via: ./run.sh +exec server.cfg (ref: legacy projects & grok thread)
        - try to join game via fivem, etc. (ref: legacy projects)
    
    DONE - clone vanilla ref: https://github.com/citizenfx/cfx-server-data.git into .../server-data-vanilla
        NOTE: this is lua based (not C#)
    DONE - where does the server.cfg go for c# integration (need initial test server.cfg)
